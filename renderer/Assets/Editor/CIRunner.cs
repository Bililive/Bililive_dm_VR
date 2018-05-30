using System.IO;
using System.Linq;
using UnityEditor;

public class CIRunner
{
    [MenuItem("Build/Windows Build")]
    public static void Build()
    {
        string path = "../build";
        string[] scenes = new string[] { "Assets/Main.unity" };

        FileUtil.DeleteFileOrDirectory(path);

        BuildPipeline.BuildPlayer(scenes, path + "/Bililive_dm_VR.Renderer.exe", BuildTarget.StandaloneWindows64, BuildOptions.CompressWithLz4HC);

        FileUtil.DeleteFileOrDirectory(path + "/UnityCrashHandler64.exe");
        new DirectoryInfo(path + "/Bililive_dm_VR.Renderer_Data/Managed").EnumerateFiles("*.xml").ToList().ForEach(x => x.Delete());
    }
}
