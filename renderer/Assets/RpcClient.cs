using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class RpcClient : MonoBehaviour
{

    private NamedPipeClientStream clientStream;

    private void Awake()
    {
        Debug.Log(Environment.CommandLine);

        string server = Environment.GetCommandLineArgs().FirstOrDefault(str => str.StartsWith("bililivevrdm"));

        if (string.IsNullOrWhiteSpace(server))
        {
            Debug.Log("退出，未提供服务器");
            Shutdown();
            return;
        }

        clientStream = new NamedPipeClientStream(server);
        clientStream.Connect(1000);
        if (!clientStream.IsConnected)
        {
            Debug.Log("退出，连接服务器失败");
            Shutdown();
            return;
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
