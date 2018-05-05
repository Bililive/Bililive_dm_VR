using Bililive_dm_VR.Desktop.Model;
using BinarySerialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class RpcClient : MonoBehaviour
{

    private NamedPipeClientStream clientStream;
    private static readonly BinarySerializer binarySerializer = new BinarySerializer() { Encoding = Encoding.UTF8, Endianness = Endianness.Big };

    private void OnGUI()
    {
        int w = Screen.width, h = Screen.height;
        GUIStyle style = new GUIStyle();
        Rect rect = new Rect(0, h * 3 / 100, w, h * 3 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 3 / 100;
        style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);
        GUI.Label(rect, Environment.CommandLine, style);
    }

    private void Awake()
    {
        Debug.Log(Environment.CommandLine);

        // string server = Environment.GetCommandLineArgs().FirstOrDefault(str => str.StartsWith("bililivevrdm"));
        return;
        string server = "bililivevrdm1472026309";

        if (string.IsNullOrWhiteSpace(server))
        {
            Debug.Log("退出，未提供服务器");
            Shutdown();
            return;
        }

        clientStream = new NamedPipeClientStream(".", server, PipeDirection.In);
        clientStream.Connect(1000);
        if (!clientStream.IsConnected)
        {
            Debug.Log("退出，连接服务器失败");
            Shutdown();
            return;
        }

        new Thread(ReadLoop) { Name = "RpcReadLoop", IsBackground = true }.Start();
    }

    private void ReadLoop(object obj)
    {
        try
        {
            do
            {
                var command = binarySerializer.Deserialize<RpcCommand>(clientStream);
                Debug.Log("收到 Command");
                switch (command.Command)
                {
                    case ConnectionCommand connectionCommand:
                        Debug.Log("连接断开直播间" + connectionCommand.Connect + connectionCommand.RoomId);
                        break;
                    case ProfileCommand profileCommand:
                        Debug.Log("收到 profile command");
                        break;
                    default:
                        Debug.Log("收到了一个奇怪的 Command " + command.CommandType);
                        break;
                }
            } while (true);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.Log("Rpc 错误 退出");
            Shutdown();
        }
    }

    private void Shutdown()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
