using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

[RequireComponent(typeof(DanmakuReceiver), typeof(TextMesh))]
public class DanmakuDisplayer : MonoBehaviour
{
    public static DanmakuDisplayer Instance
    {
        get { return _instance ?? (_instance = FindObjectOfType<DanmakuDisplayer>()); }
    }
    private static DanmakuDisplayer _instance;

    public int ChatLineCount = 27; // Max line count for our display

    public InputField RoomIDBox;
    public Button ConnectButton;
    public Text ConnectButtonText;

    private TextMesh TextSize_Mesh // Used to check each message as it's being built to WordWrap each message into lines
    { get { return _TextSize_Mesh ?? (_TextSize_Mesh = GetComponent<TextMesh>()); } }
    private Renderer TextSize_Renderer
    { get { return _TextSize_renderer ?? (_TextSize_renderer = TextSize_Mesh.GetComponent<Renderer>()); } }
    private TextMesh _TextSize_Mesh;
    private Renderer _TextSize_renderer;

    public GameObject TextMeshBase; // Cloned into ChatTextMeshes

    private bool _hasGeneratedTextMeshes = false;
    public TextMesh[] ChatTextMeshes; // Each one of these is a line on our Chat Display
    public Renderer[] ChatTextRenderers; // Each one of these is a line on our Chat Display

    // These are used to display ViewerCount and ChannelName when connected
    public TextMesh ViewerCountTextMesh;
    public TextMesh ChannelNameTextMesh;

    public DanmakuReceiver Receiver
    {
        get { return _receiver ?? (_receiver = GetComponent<DanmakuReceiver>()); }
    }
    private DanmakuReceiver _receiver;

    private readonly List<DanmakuMessage> _messages = new List<DanmakuMessage>();


    public bool Connected
    {
        get { return Receiver.Connected; }
    }

    public void Awake()
    {
        _instance = this;
        GenChatTexts();

        Receiver.ReceivedDanmaku.AddListener(HandlerReceivedDanmaku);
        // Receiver.ReceivedRoomCount.AddListener(HandlerReceivedRoomCount);
        Receiver.Disconnected.AddListener(HandlerDisconnected);
        Receiver.LogMessage.AddListener(HandlerLogMessage);

        StartCoroutine(UpdateViewerCount());

        Logger4UIScripts.Log.AddListener((msg, color) =>
        { AddMsg("系统", msg, color.LogColor2Hex()); });
    }

    public void HandlerReceivedDanmaku(DanmakuModel model)
    {
        if (model.MsgType == MsgTypeEnum.Comment)
        {
            AddMsg(model.UserName, model.CommentText, "FFFFFF");
        }
        else
        {
            Debug.Log("Danmaku Received: " + model.MsgType.ToString());
        }
    }

    public void HandlerReceivedRoomCount(uint viewer)
    {
        if (ViewerCountTextMesh != null)
            ViewerCountTextMesh.text = string.Format("人气: {0}", viewer);
    }

    private IEnumerator UpdateViewerCount()
    {
        while (true)
        {
            yield return new WaitForSeconds(5);
            if (ViewerCountTextMesh != null)
                ViewerCountTextMesh.text = string.Format("人气: {0}", Receiver.ViewerCount);
        }
    }

    public void HandlerDisconnected(Exception error)
    {
        AddMsg("系统", "被断开连接：" + (error != null ? error.Message : "神秘错误"), "FF0000");
        ClearViewerCountAndChannelName("被断开连接");
    }

    public void HandlerLogMessage(string log)
    {
        AddMsg("系统", log, "CC0000");
    }

    public void ToggleConnect()
    {
        Debug.Log("ToggleConnect Clicked! with Connected:" + Connected);
        if (!Connected)
        {// do connect
            int roomid = 0;
            if (int.TryParse(RoomIDBox.text, out roomid) && roomid > 0)
            {
                if (Receiver.Connect(roomid))
                {
                    AddMsg("系统", "连接成功！", "00FF00");
                    ClearViewerCountAndChannelName("已连接到 " + roomid);
                }
                else
                {
                    AddMsg("系统", "连接失败：" + (Receiver.Error == null ? "神秘错误" : Receiver.Error.Message), "FF0000");
                    Debug.LogError("连接失败" + Receiver.Error.ToString());
                }
            }
            else
            {
                AddMsg("系统", "房间号是正整数");
                RoomIDBox.text = string.Empty;
            }
        }
        else
        {// disconnect
            Receiver.Disconnect();
            AddMsg("系统", "断开了连接");
            ClearViewerCountAndChannelName("断开了连接");
        }
        ConnectButtonText.text = Connected ? "断开连接" : "连接";
    }

    public void Start()
    {
        ClearViewerCountAndChannelName("未连接");
        StartCoroutine("SyncWithSteamVR");
    }

    private static readonly System.Object GenTextMeshLocker = new System.Object();
    // Generate our Text Meshes and ensure they are only generated once
    private void GenChatTexts()
    {
        lock (GenTextMeshLocker)
        {
            if (_hasGeneratedTextMeshes) return;
            ChatTextMeshes = new TextMesh[ChatLineCount];
            ChatTextRenderers = new Renderer[ChatLineCount];
            for (var i = 0; i < ChatLineCount; i++)
            {
                var obj = GameObject.Instantiate(TextMeshBase);
                ChatTextRenderers[i] = obj.GetComponent<Renderer>();
                ChatTextMeshes[i] = obj.GetComponent<TextMesh>();
                ChatTextMeshes[i].text = "";
                obj.transform.parent = gameObject.transform.parent;
                obj.transform.localScale = new Vector3(0.005f, 0.005f, 1f);
                obj.transform.localPosition = new Vector3(-0.5f, 0.465f - (0.037f * i), -1f);
                obj.SetActive(true);
            }
            _hasGeneratedTextMeshes = true;
        }
    }

    // Reset the channel and viewer text
    private void ClearViewerCountAndChannelName(string channelText = null)
    {
        if (ChannelNameTextMesh != null) ChannelNameTextMesh.text = (channelText ?? "");
        // if (ViewerCountTextMesh != null) ViewerCountTextMesh.text = "";
    }

    public void AddMsg(string author, string message, string color = "0066FF")
    {
        _messages.Add(new DanmakuMessage(author, color, message));
        while (_messages.Count > ChatLineCount)
            _messages.RemoveAt(0);

        // GenChatTexts();

        if (!_needRefresh)
        {
            lock (_needRefreshlock)
            {
                _needRefresh = true;
            }
        }

    }
    // TODO: 优化，使 AddMsg 线程安全 maybe use UnityEvent
    private bool _needRefresh = false;
    private object _needRefreshlock = new object();
    private void Update()
    {
        lock (_needRefreshlock)
        {
            if (_needRefresh)
                RefreshTexts();
        }
    }
    private static readonly object RefreshLocker = new object();
    private void RefreshTexts()
    {
        const float rowLimit = 0.975f;

        try
        {
            lock (RefreshLocker)
            {
                var lines = new List<string>();

                foreach (var msg in _messages)
                {
                    // var chars = msg.Message.ToCharArray();
                    // TODO

                    lines.Add(string.Format("<color=#{0}FF>{1}</color>: ", msg.Color, msg.Name) + msg.Message);
                }

                for (int i = 0; i < ChatLineCount; i++)
                {
                    if (i >= lines.Count) continue;
                    ChatTextMeshes[i].text = lines[i];
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private System.Collections.IEnumerator SyncWithSteamVR()
    {
        while (Application.isPlaying)
        {
            var compositor = OpenVR.Compositor;
            if (compositor != null)
            {
                var trackingSpace = compositor.GetTrackingSpace();
                SteamVR_Render.instance.trackingSpace = trackingSpace;
            }
            yield return new WaitForSeconds(10f);
        }
    }
}

public struct DanmakuMessage
{
    public readonly string Name;
    public readonly string Color;
    public readonly string Message;

    public DanmakuMessage(string name, string color, string message)
    {
        Name = name;
        Color = color;
        Message = message;
    }
}

