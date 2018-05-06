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
    public HOTK_Overlay Overlay;
    public Material Background;


    private Profile new_profile = null;
    private NamedPipeClientStream clientStream;
    private static readonly BinarySerializer binarySerializer = new BinarySerializer() { Encoding = Encoding.UTF8, Endianness = Endianness.Big };

    private void Awake()
    {
        Debug.Log(Environment.CommandLine);

        string server = Environment.GetCommandLineArgs().FirstOrDefault(str => str.StartsWith("bililivevrdm"));

#if UNITY_EDITOR
        server = "bililivevrdmdebug";
#endif

        if (string.IsNullOrWhiteSpace(server))
        {
            Debug.Log("退出，未提供服务器");
            Shutdown();
            return;
        }

        clientStream = new NamedPipeClientStream(".", server);
        clientStream.Connect(1000);
        if (!clientStream.IsConnected)
        {
            Debug.Log("退出，连接服务器失败");
            Shutdown();
            return;
        }

        new Thread(ReadLoop) { Name = "RpcReadLoop", IsBackground = true }.Start();
    }

    private void OnApplicationQuit()
    {
        if (clientStream != null)
        {
            try
            {
                clientStream.Close();
                clientStream.Dispose();
            }
            catch (Exception) { }
        }
    }

    private void Update()
    {
        if (new_profile != null)
        {
            SetProfile(new_profile);
            new_profile = null;
        }
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
                        new_profile = profileCommand.Profile;
                        break;
                    default:
                        Debug.Log("收到了一个奇怪的 Command " + command.CommandType);
                        break;
                }
            } while ((clientStream?.IsConnected ?? false) && (clientStream?.CanRead ?? false));
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.Log("Rpc 错误 退出");
            Shutdown();
        }
    }

    public void SetProfile(Profile profile)
    {
        Overlay.AnchorDevice = profile.MountDevice;
        Overlay.AnchorPoint = profile.MountLocation;
        Overlay.AnchorOffset = new Vector3(profile.OffsetX, profile.OffsetY, profile.OffsetZ);
        Overlay.AnchorRotation = new Vector3(profile.RotationX, profile.RotationY, profile.RotationZ);

        Overlay.Alpha = profile.Alpha / 100f;
        Overlay.Scale = profile.Scale / 100f;

        Overlay.AnimateOnGaze = profile.AnimateOnGaze;
        Overlay.AnimationAlpha = profile.AnimationAlpha / 100f;
        Overlay.AnimationScale = profile.AnimationScale / 100f;

        SetBackgroundColor(profile.Color);
    }

    public void SetBackgroundColor(int argb)
    {
        // byte a = (byte)((argb >> 6) & 0xff);
        byte r = (byte)((argb >> 4) & 0xff);
        byte g = (byte)((argb >> 2) & 0xff);
        byte b = (byte)((argb >> 0) & 0xff);

        if (Background == null) return;
        var tex = (Background.mainTexture ?? (Background.mainTexture = GenerateBaseTexture())) as Texture2D;

        var a = tex.GetPixel(0, 0).a;
        Color color = new Color32(r, g, b, 0xff);
        color.a = a;

        tex.SetPixel(0, 0, color);
        tex.Apply();
    }

    public void SetBackgroundAlpha(int alpha)
    {
        if (alpha < 0 || alpha > 100) return;
        if (Background == null) return;

        float a = alpha / 100f;
        var tex = (Background.mainTexture ?? (Background.mainTexture = GenerateBaseTexture())) as Texture2D;

        Color color = tex.GetPixel(0, 0);
        color.a = a;
        tex.SetPixel(0, 0, color);
        tex.Apply();
    }

    private static Texture2D GenerateBaseTexture()
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, new Color(0.45f, 0.2f, 0.75f));
        tex.Apply();
        return tex;
    }

    private void Shutdown()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }

}
