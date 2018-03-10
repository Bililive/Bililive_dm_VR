using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class TwitchIRC : MonoBehaviour
{
    public const string DefaultChatNameColor = "#FFFFFFFF";
    public string Oauth;
    public string NickName;
    public string ChannelName;
    private const string Server = "irc.twitch.tv";
    private const int Port = 6667;

    //event(buffer).
    public class MsgEvent : UnityEngine.Events.UnityEvent<TwitchMessage> { }
    public MsgEvent MessageRecievedEvent = new MsgEvent();

    private string _buffer = string.Empty;
    private bool _stopThreads;
    private readonly Queue<string> _commandQueue = new Queue<string>();
    private readonly List<TwitchMessage> _recievedMsgs = new List<TwitchMessage>();
    private System.Threading.Thread _inProc, _outProc;

    private static readonly Dictionary<string, string> UserColors = new Dictionary<string, string>();
    private static System.Random Random = new System.Random();

    private bool _connected;
    private bool _loggedin;

    public void StartIRC()
    {
        _stopThreads = false;
        _commandQueue.Clear();
        _recievedMsgs.Clear();
        var sock = new System.Net.Sockets.TcpClient();
        sock.Connect(Server, Port);
        if (!sock.Connected)
        {
            ToNotice("System", "Failed to connect!", NoticeColor.Red);
            return;
        }
        var networkStream = sock.GetStream();
        var input = new System.IO.StreamReader(networkStream);
        var output = new System.IO.StreamWriter(networkStream);
        
        _loggedin = false;
        _connected = false;
        //Send PASS & NICK.
        output.WriteLine("PASS " + Oauth);
        output.WriteLine("NICK " + NickName.ToLower());
        output.Flush();

        //output proc
        _outProc = new System.Threading.Thread(() => IRCOutputProcedure(output));
        _outProc.Start();
        //input proc
        _inProc = new System.Threading.Thread(() => IRCInputProcedure(input, networkStream));
        _inProc.Start();

        CancelInvoke("CheckConnection");
        Invoke("CheckConnection", 20f);
    }

    private void CheckConnection()
    {
        if (_stopThreads) return;
        lock (_recievedMsgs)
        {
            if (!_loggedin)
            {
                _recievedMsgs.Add(new TwitchMessage(ToNotice("System", "Should be logged in by now.. are the username and oauth correct?", NoticeColor.Yellow)));
            }
            else if (!_connected)
            {
                _recievedMsgs.Add(new TwitchMessage(ToNotice("System", "Should be connected by now.. is the channel name correct?", NoticeColor.Yellow)));
            }
        }
    }

    private void IRCInputProcedure(System.IO.TextReader input, System.Net.Sockets.NetworkStream networkStream)
    {
        while (!_stopThreads)
        {
            try
            {
                if (!networkStream.DataAvailable)
                {
                    Thread.Sleep(20);
                    continue;
                }

                _buffer = input.ReadLine();
                if (_buffer == null) continue;
                //Debug.Log(_buffer);

                string[] tokens;
                string message;
                List<EmoteKey> emoteKeys = null;
                if (_buffer.StartsWith("@"))
                {
                    var split = _buffer.IndexOf(' ');
                    var userstate = _buffer.Substring(0, split);
                    message = _buffer.Substring(split + 1);
                    tokens = message.Split(' ');

                    var username = tokens[0].Split('!')[0].Substring(1);
                    var keys = userstate.Split(';');

                    emoteKeys = new List<EmoteKey>();

                    foreach (var k in keys)
                    {
                        var key = k;
                        if (key.StartsWith("@")) key = key.Substring(1);
                        if (key.StartsWith("color="))
                        {
                            if (GetUserColor(username) != DefaultChatNameColor) continue;
                            var color = (key != "color=") ? key.Substring(7) : null;
                            if (string.IsNullOrEmpty(color))
                            {
                                var r = Mathf.Max(0.25f, Random.Next(0, 100)/100f);
                                var g = Mathf.Max(0.25f, Random.Next(0, 100)/100f);
                                var b = Mathf.Max(0.25f, Random.Next(0, 100)/100f);
                                color = ColorToHex(new Color(r, g, b));
                            }
                            lock (UserColors)
                            {
                                UserColors.Add(username, color);
                            }
                        }
                        else if (key.StartsWith("emotes="))
                        {
                            var emotes = key.Substring(7).Split('/');
                            emoteKeys.AddRange(from emote in emotes let emoteSplit = emote.IndexOf(":") where emoteSplit != -1 let emoteId = int.Parse(emote.Substring(0, emoteSplit)) let emotePos = emote.Substring(emoteSplit + 1) let emotePoses = emotePos.Split(',') from emoteP in emotePoses let emoteTokens = emoteP.Split('-') let emoteStart = int.Parse(emoteTokens[0]) let emoteLen = (int.Parse(emoteTokens[1]) - emoteStart) + 1 select new EmoteKey(emoteId, emoteStart, emoteLen)); // LINQ master :P
                        }
                    }

                    emoteKeys.Sort((a, b) => a.EmoteStart.CompareTo(b.EmoteStart));

                    if (emoteKeys.Count > 0)
                    {
                        var messageSplit = message.Substring(1).IndexOf(':');
                        var messageHeader = message.Substring(0, messageSplit);
                        var messageBody = message.Substring(messageSplit + 2);
                        
                        for (var i = 0; i < emoteKeys.Count; i++)
                        {
                            messageBody = messageBody.Remove(emoteKeys[i].EmoteStart, emoteKeys[i].EmoteLength);
                            for (var j = i + 1; j < emoteKeys.Count; j++)
                            {
                                emoteKeys[j].EmoteStart -= emoteKeys[i].EmoteLength;
                                //Debug.Log("Adjusting Emote Length");
                            }
                        }

                        message = messageHeader + " :" + messageBody;
                    }
                }
                else
                {
                    message = _buffer;
                    tokens = _buffer.Split(' ');
                }

                switch (tokens[1])
                {
                    case "PRIVMSG":
                    case "NOTICE":
                        lock (_recievedMsgs)
                        {
                            _recievedMsgs.Add(new TwitchMessage(message, emoteKeys));
                        }
                        break;
                    case "JOIN":
                        lock (_recievedMsgs)
                        {
                            _recievedMsgs.Add(new TwitchMessage(ToTwitchNotice(string.Format("Connected to {0}!", tokens[2]))));
                            _connected = true;
                        }
                        break;
                    case "001":
                        lock (_recievedMsgs)
                        {
                            _recievedMsgs.Add(new TwitchMessage(ToTwitchNotice("Logged in! Connecting to chat..")));
                            _loggedin = true;
                        }
                        SendCommand("CAP REQ :twitch.tv/tags");
                        SendCommand("CAP REQ :twitch.tv/commands");
                        SendCommand("JOIN #" + ChannelName);
                        break;
                    case "CAP":
                        lock (_recievedMsgs)
                        {
                            _recievedMsgs.Add(new TwitchMessage(ToTwitchNotice("Acknowledging Client Capabilities!")));
                        }
                        break;
                    case "USERSTATE":
                        break;
                    default:
                        if (_buffer.StartsWith("PING "))
                        {
                            SendCommand(_buffer.Replace("PING", "PONG"));
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                lock (_recievedMsgs)
                {
                    _recievedMsgs.Add(new TwitchMessage(ToNotice("EXCEPTION", e.ToString(), NoticeColor.Red)));
                }
            }
        }
    }

    private void IRCOutputProcedure(System.IO.TextWriter output)
    {
        System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
        stopWatch.Start();
        while (!_stopThreads)
        {
            lock (_commandQueue)
            {
                if (_commandQueue.Count <= 0)
                {
                    Thread.Sleep(20);
                    continue;
                }
                // https://github.com/justintv/Twitch-API/blob/master/IRC.md#command--message-limit 
                //has enough time passed since we last sent a message/command?
                if (stopWatch.ElapsedMilliseconds <= 1750)
                {
                    Thread.Sleep(20);
                    continue;
                }
                //send msg.
                output.WriteLine(_commandQueue.Peek());
                output.Flush();
                //remove msg from queue.
                _commandQueue.Dequeue();
                //restart stopwatch.
                stopWatch.Reset();
                stopWatch.Start();
            }
        }
    }

    public void SendCommand(string cmd)
    {
        lock (_commandQueue)
        {
            _commandQueue.Enqueue(cmd);
        }
    }
    public void SendMsg(string msg)
    {
        lock (_commandQueue)
        {
            _commandQueue.Enqueue("PRIVMSG #" + ChannelName + " :" + msg);
        }
    }
    public void OnEnable()
    {
        _stopThreads = false;
    }
    public void OnDisable()
    {
        _stopThreads = true;
        CancelInvoke("CheckConnection");
    }
    public void OnDestroy()
    {
        _stopThreads = true;
        CancelInvoke("CheckConnection");
    }
    public void Update()
    {
        lock (_recievedMsgs)
        {
            if (_recievedMsgs.Count <= 0) return;
            foreach (var t in _recievedMsgs)
            {
                MessageRecievedEvent.Invoke(t);
            }
            _recievedMsgs.Clear();
        }
    }

    public static string ToTwitchNotice(string msgIn, NoticeColor colorEnum = NoticeColor.Green)
    {
        return ToNotice("Twitch", msgIn, colorEnum);
    }

    public static string ToNotice(string nickname, string msgIn, NoticeColor colorEnum = NoticeColor.Green)
    {
        return string.Format(":{0} NOTICE {1} :{2}", nickname, NoticeColorToString(colorEnum), msgIn);
    }

    public static string GetUserColor(string username)
    {
        lock (UserColors)
        {
            string hex;
            return UserColors.TryGetValue(username, out hex) ? hex : DefaultChatNameColor;
        }
    }

    public static string ColorToHex(Color32 color)
    {
        return color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
    }

    public static Color HexToColor(string hex)
    {
        hex = hex.Replace("0x", "");//in case the string is formatted 0xFFFFFF
        hex = hex.Replace("#", "");//in case the string is formatted #FFFFFF
        byte a = 255;//assume fully visible unless specified in hex
        var r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        var g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        var b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        //Only use alpha if the string has enough characters
        if (hex.Length == 8)
            a = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        return new Color32(r, g, b, a);
    }


    public static string NoticeColorToString(NoticeColor colorEnum)
    {
        switch (colorEnum)
        {
            case NoticeColor.Green:
                return "*System-Green";
            case NoticeColor.Red:
                return "*System-Red";
            case NoticeColor.Blue:
                return "*System-Blue";
            case NoticeColor.Yellow:
                return "*System-Yellow";
            case NoticeColor.Purple:
                return "*System-Purple";
            case NoticeColor.White:
                return "*System-White";
            default:
                throw new ArgumentOutOfRangeException("colorEnum", colorEnum, null);
        }
    }

    public enum NoticeColor
    {
        Green,
        Red,
        Blue,
        Yellow,
        Purple,
        White
    }

    public struct TwitchMessage
    {
        public readonly string Message;
        public readonly List<EmoteKey> Emotes;

        public TwitchMessage(string message)
        {
            Message = message;
            Emotes = null;
        }

        public TwitchMessage(string message, List<EmoteKey> emotes)
        {
            Message = message;
            Emotes = emotes;
        }
    }

    public class EmoteKey
    {
        public readonly int EmoteId;
        public int EmoteStart;
        public int EmoteLength;

        public EmoteKey(int id, int start, int length)
        {
            EmoteId = id;
            EmoteStart = start;
            EmoteLength = length;
        }
    }
}
