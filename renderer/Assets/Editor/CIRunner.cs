using System.IO;
using System.Linq;
using UnityEditor;

class CIRunner
{
    [MenuItem("Build/Windows Build")]
    static void Build()
    {
        string path = "../build";
        string[] scenes = new string[] { "Assets/Main.unity" };

        FileUtil.DeleteFileOrDirectory(path);

        BuildPipeline.BuildPlayer(scenes, path + "/Bililive_dm_VR.Renderer.bin", BuildTarget.StandaloneWindows64, BuildOptions.CompressWithLz4HC);

        FileUtil.DeleteFileOrDirectory(path + "/UnityCrashHandler64.exe");
        FileUtil.MoveFileOrDirectory(path + "/Bililive_dm_VR.Renderer_Data", path + "/Data");

        new DirectoryInfo(path + "/Data/Managed").EnumerateFiles("*.xml").ToList().ForEach(x => x.Delete());
    }
}
