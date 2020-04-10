using UnityEngine;
using System.Collections;
using System;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Xml;
using System.Threading;
using System.Linq;
using System.Text;

public class DanmakuReceiver : MonoBehaviour
{
    private const string defaulthosts = "broadcastlv.chat.bilibili.com";
    private string CIDInfoUrl = "http://live.bilibili.com/api/player?id=cid:";
    private string ChatHost = defaulthosts;
    private int ChatPort = 2243;

    private TcpClient Client;
    private NetworkStream NetStream;

    private Thread ReceiveMessageLoopThread;
    private Thread HeartbeatLoopThread;

    public bool Connected { get; private set; }
    public Exception Error { get; private set; }
    public uint ViewerCount { get; private set; }

    public class ReceivedDanmakuEvent : UnityEngine.Events.UnityEvent<DanmakuModel> { }
    public ReceivedDanmakuEvent ReceivedDanmaku = new ReceivedDanmakuEvent();

    public class ReceivedRoomCountEvent : UnityEngine.Events.UnityEvent<uint> { }
    public ReceivedRoomCountEvent ReceivedRoomCount = new ReceivedRoomCountEvent();

    public class DisconnectEvent : UnityEngine.Events.UnityEvent<Exception> { }
    public DisconnectEvent Disconnected = new DisconnectEvent();

    public class LogMessageEvent : UnityEngine.Events.UnityEvent<string> { }
    public LogMessageEvent LogMessage = new LogMessageEvent();

    public bool Connect(int roomId)
    {
        try
        {
            if(this.Connected)
                throw new InvalidOperationException();
            int channelId = roomId;

            // try
            // {
            //     var www = new WWW(CIDInfoUrl + channelId);
            //     var xml = "<root>" + www.text + "</root>";
            //     XmlDocument doc = new XmlDocument();
            //     doc.LoadXml(xml);
            //     ChatHost = doc["root"]["dm_server"].InnerText;
            //     ChatPort = int.Parse(doc["root"]["dm_port"].InnerText);
            //     // var request2 = WebRequest.Create(CIDInfoUrl + channelId);
            //     // request2.Timeout = 2000;
            //     // var response2 = request2.GetResponse();
            //     // using (var stream = response2.GetResponseStream())
            //     // {
            //     //     using (var sr = new StreamReader(stream))
            //     //     {
            //     //         var text = sr.ReadToEnd();
            //     //         var xml = "<root>" + text + "</root>";
            //     //         XmlDocument doc = new XmlDocument();
            //     //         doc.LoadXml(xml);
            //     //         ChatHost = doc["root"]["dm_server"].InnerText;
            //     //         ChatPort = int.Parse(doc["root"]["dm_port"].InnerText);
            //     //     }
            //     // }
            // }
            // catch(WebException ex)
            // {
            //     HttpWebResponse errorResponse = ex.Response as HttpWebResponse;
            //     if(errorResponse.StatusCode == HttpStatusCode.NotFound)
            //     { // 直播间不存在（HTTP 404）
            //         LogMessage.Invoke("该直播间疑似不存在，弹幕姬只支持使用原房间号连接");
            //     }
            //     else
            //     { // B站服务器响应错误
            //         LogMessage.Invoke("B站服务器响应弹幕服务器地址出错，尝试使用常见地址连接");
            //     }
            // }
            // catch(Exception ex)
            // { // 其他错误（XML解析错误？）
            //     LogMessage.Invoke("获取弹幕服务器地址时出现未知错误，尝试使用常见地址连接");
            //     Debug.LogError(ex.ToString());
            // }

            Client = new TcpClient();
            Client.Connect(ChatHost, ChatPort);
            if(!Client.Connected)
            {
                return false;
            }
            NetStream = Client.GetStream();
            SendSocketData(7, "{\"roomid\":" + channelId + ",\"uid\":0}");
            Connected = true;
            ReceiveMessageLoopThread = new Thread(this.ReceiveMessageLoop);
            ReceiveMessageLoopThread.IsBackground = true;
            ReceiveMessageLoopThread.Start();
            HeartbeatLoopThread = new Thread(this.HeartbeatLoop);
            HeartbeatLoopThread.IsBackground = true;
            HeartbeatLoopThread.Start();
            return true;
        }
        catch(Exception ex)
        {
            this.Error = ex;
            return false;
        }
    }

    public void Disconnect()
    {
        Connected = false;
        try
        {
            Client.Close();
        }
        catch(Exception)
        { }

        NetStream = null;
    }

    private void ReceiveMessageLoop()
    {
        Debug.Log("ReceiveMessageLoop Started!");
        try
        {
            var stableBuffer = new byte[16];
            var buffer = new byte[4096];
            while(this.Connected)
            {
                NetStream.ReadB(stableBuffer, 0, 16);
                DanmakuProtocol protocol = Parse2Protocol(stableBuffer);

                if (protocol.PacketLength < 16)
                {
                    throw new NotSupportedException("协议失败: (L:" + protocol.PacketLength + ")");
                }
                var payloadlength = protocol.PacketLength - 16;
                if (payloadlength == 0)
                {
                    continue;//没有内容了
                }
                if (buffer.Length < payloadlength) // 不够长再申请
                {
                    buffer = new byte[payloadlength];
                }

                NetStream.ReadB(buffer, 0, payloadlength);

                if (protocol.Version == 2 && protocol.Action == 5) // 处理deflate消息
                {
                    // Skip 0x78 0xDA
                    using (DeflateStream deflate = new DeflateStream(new MemoryStream(buffer, 2, payloadlength - 2), CompressionMode.Decompress))
                    {
                        while (deflate.Read(stableBuffer, 0, 16) > 0)
                        {
                            protocol = Parse2Protocol(stableBuffer);
                            payloadlength = protocol.PacketLength - 16;
                            if (payloadlength == 0)
                            {
                                continue; // 没有内容了
                            }
                            if (buffer.Length < payloadlength) // 不够长再申请
                            {
                                buffer = new byte[payloadlength];
                            }
                            deflate.Read(buffer, 0, payloadlength);
                            ProcessDanmaku(protocol.Action, buffer, payloadlength);
                        }
                    }
                }
                else
                {
                    ProcessDanmaku(protocol.Action, buffer, payloadlength);
                }
            }
        }
        catch(Exception ex)
        {
            this.Error = ex;
            _disconnect();
        }
    }

    private void ProcessDanmaku(int action, byte[] local_buffer, int length)
    {
        switch (action)
        {
            case 3:
                var viewer = BitConverter.ToUInt32(local_buffer.Take(4).Reverse().ToArray(), 0);
                ViewerCount = viewer;
                ReceivedRoomCount.Invoke(viewer);
                break;
            case 5:
                var json = Encoding.UTF8.GetString(local_buffer, 0, length);
                try
                {
                    ReceivedDanmaku.Invoke(new DanmakuModel(json));
                }
                catch (Exception)
                { }
                break;
            default:
                break;
        }
    }

    private void HeartbeatLoop()
    {
        Debug.Log("HeartbeatLoop Started!");
        try
        {
            while(this.Connected)
            {
                this.SendHeartbeat();
                for(int i = 0; i < 30; i++)
                {
                    Thread.Sleep(1000);//1s
                    if(!Connected)
                    {
                        Debug.Log("HeartbeatLoop Break");
                        break;
                    }
                }
            }
        }
        catch(Exception ex)
        {
            this.Error = ex;
            _disconnect();
        }
    }

    private void _disconnect()
    {
        if(Connected)
        {
            Debug.Log("Disconnected");
            Connected = false;
            Client.Close();
            NetStream = null;
            Disconnected.Invoke(Error);
        }
    }

    private void SendHeartbeat()
    {
        SendSocketData(2);
        Debug.Log("Message Sent: Heartbeat");
    }

    private void SendSocketData(int action, string body = "")
    {
        SendSocketData(0, 16, /*protocolversion*/2, action, 1, body);
    }

    private void SendSocketData(int packetlength, short magic, short ver, int action, int param = 1, string body = "")
    {
        var playload = Encoding.UTF8.GetBytes(body);
        if(packetlength == 0)
        {
            packetlength = playload.Length + 16;
        }
        var buffer = new byte[packetlength];
        using(var ms = new MemoryStream(buffer))
        {
            var b = BitConverter.GetBytes(buffer.Length).ToBE();
            ms.Write(b, 0, 4);
            b = BitConverter.GetBytes(magic).ToBE();
            ms.Write(b, 0, 2);
            b = BitConverter.GetBytes(ver).ToBE();
            ms.Write(b, 0, 2);
            b = BitConverter.GetBytes(action).ToBE();
            ms.Write(b, 0, 4);
            b = BitConverter.GetBytes(param).ToBE();
            ms.Write(b, 0, 4);
            if(playload.Length > 0)
            {
                ms.Write(playload, 0, playload.Length);
            }
            NetStream.Write(buffer, 0, buffer.Length);
            NetStream.Flush();
        }
    }


    // Use this for initialization
    void Awake()
    {
        Connected = false;
        ViewerCount = 0;
    }

    private static unsafe DanmakuProtocol Parse2Protocol(byte[] buffer)
    {
        DanmakuProtocol protocol;
        fixed (byte* ptr = buffer)
        {
            protocol = *(DanmakuProtocol*)ptr;
        }
        protocol.ChangeEndian();
        return protocol;
    }

    private struct DanmakuProtocol
    {
        /// <summary>
        /// 消息总长度 (协议头 + 数据长度)
        /// </summary>
        public int PacketLength;
        /// <summary>
        /// 消息头长度 (固定为16[sizeof(DanmakuProtocol)])
        /// </summary>
        public short HeaderLength;
        /// <summary>
        /// 消息版本号
        /// </summary>
        public short Version;
        /// <summary>
        /// 消息类型
        /// </summary>
        public int Action;
        /// <summary>
        /// 参数, 固定为1
        /// </summary>
        public int Parameter;
        /// <summary>
        /// 转为本机字节序
        /// </summary>
        public void ChangeEndian()
        {
            PacketLength = IPAddress.HostToNetworkOrder(PacketLength);
            HeaderLength = IPAddress.HostToNetworkOrder(HeaderLength);
            Version = IPAddress.HostToNetworkOrder(Version);
            Action = IPAddress.HostToNetworkOrder(Action);
            Parameter = IPAddress.HostToNetworkOrder(Parameter);
        }
    }
}
