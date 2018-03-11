using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[System.Serializable]
public class ProgramSettings
{
    public const uint CurrentSaveVersion = 1;

    public string LastProfile;
}

[System.Serializable]
public class TwitchSettings
{
    public const uint CurrentSaveVersion = 4;

    public uint SaveFileVersion;

    public string Username;
    public string Channel;
    public float X, Y, Z;
    public float RX, RY, RZ;
    // public string ChatSound;
    // public float Volume, Pitch;
    // public string FollowerSound;
    // public float FollowerVolume, FollowerPitch;
    public HOTK_Overlay.AttachmentDevice Device;
    public HOTK_Overlay.AttachmentPoint Point;
    public HOTK_Overlay.AnimationType Animation;

    public float BackgroundR, BackgroundG, BackgroundB, BackgroundA;

    public float AlphaStart, AlphaEnd, AlphaSpeed;
    public float ScaleStart, ScaleEnd, ScaleSpeed;
}

public static class TwitchSettingsSaver
{
    public static string ProgramSettingsFileName = Application.persistentDataPath + "/programSettings.gd";
    public static string ProfilesFileName = Application.persistentDataPath + "/savedProfiles.gd";
    public static string OriginalProfilesFileName = Application.persistentDataPath + "/savedSettings.gd"; // Legacy Savefile compatibility

    public static string Current;
    public static ProgramSettings CurrentProgramSettings;
    public static Dictionary<string, TwitchSettings> SavedProfiles = new Dictionary<string, TwitchSettings>();

    public static void SaveProfiles(int mode = -1)
    {
        var bf = new BinaryFormatter();
        var file = File.Create(ProfilesFileName);
        bf.Serialize(file, SavedProfiles);
        file.Close();
        switch (mode)
        {
            case 1: // Legacy Savefile compatibility
                Logger4UIScripts.Log.Invoke("已从旧版本配置文件更新。", Logger4UIScripts.LogColor.Blue);
                // Logger4UIScripts.Log.Invoke("Upgrading Legacy Profile Save Data.", Logger4UIScripts.LogColor.Blue);
                break;
            case 2:
                Logger4UIScripts.Log.Invoke("配置已删除。", Logger4UIScripts.LogColor.Blue);
                // Logger4UIScripts.Log.Invoke("Profile deleted.", Logger4UIScripts.LogColor.Blue);
                break;
            default:
                Logger4UIScripts.Log.Invoke("保存了 " + SavedProfiles.Count + " 个配置。", Logger4UIScripts.LogColor.Blue);
                // Logger4UIScripts.Log.Invoke("Saved " + SavedProfiles.Count + " profile(s).", Logger4UIScripts.LogColor.Blue);
                break;
        }
    }

    public static void LoadProgramSettings()
    {
        LoadProfiles();
        LoadSettings();
    }
    private static void LoadSettings()
    {
        if (!File.Exists(ProgramSettingsFileName)) return;
        var bf = new BinaryFormatter();
        var file = File.Open(ProgramSettingsFileName, FileMode.Open);
        CurrentProgramSettings = (ProgramSettings)bf.Deserialize(file);
        file.Close();

        if (SavedProfiles != null && SavedProfiles.Count > 0 && SavedProfiles.ContainsKey(CurrentProgramSettings.LastProfile))
        {
            Current = CurrentProgramSettings.LastProfile;
        }
        Logger4UIScripts.Log.Invoke("已加载程序配置。", Logger4UIScripts.LogColor.Blue);
        // Logger4UIScripts.Log.Invoke("Loaded program settings.", Logger4UIScripts.LogColor.Blue);
    }

    public static void SaveProgramSettings()
    {
        if (CurrentProgramSettings == null)
        {
            CurrentProgramSettings = new ProgramSettings();
        }
        if (SavedProfiles != null && SavedProfiles.Count > 0 && SavedProfiles.ContainsKey(Current))
        {
            CurrentProgramSettings.LastProfile = Current;
        }

        var bf = new BinaryFormatter();
        var file = File.Create(ProgramSettingsFileName);
        bf.Serialize(file, CurrentProgramSettings);
        file.Close();
        Logger4UIScripts.Log.Invoke("已保存程序配置。", Logger4UIScripts.LogColor.Blue);
        // Logger4UIScripts.Log.Invoke("Saved program settings.", Logger4UIScripts.LogColor.Blue);
    }

    public static void LoadProfiles()
    {
        bool legacy = false;
        var filename = ProfilesFileName;
        if (!File.Exists(filename)) // Legacy Savefile compatibility
        {
            filename = OriginalProfilesFileName;
            if (!File.Exists(filename)) return;
            legacy = true;
            Logger4UIScripts.Log.Invoke("找到了旧版配置文件: " + filename, Logger4UIScripts.LogColor.Blue);
            // Logger4UIScripts.Log.Invoke("Found Legacy Profile Save Data: " + filename, Logger4UIScripts.LogColor.Blue);
        }
        var bf = new BinaryFormatter();
        var file = File.Open(filename, FileMode.Open);
        SavedProfiles = (Dictionary<string, TwitchSettings>)bf.Deserialize(file);
        file.Close();
        Logger4UIScripts.Log.Invoke("已加载 " + SavedProfiles.Count + " 个配置。", Logger4UIScripts.LogColor.Blue);
        // Logger4UIScripts.Log.Invoke("Loaded " + SavedProfiles.Count + " profile(s).", Logger4UIScripts.LogColor.Blue);

        if (!legacy) return; // Legacy Savefile compatibility
        File.Move(OriginalProfilesFileName, OriginalProfilesFileName + ".bak");
        SaveProfiles(1);
    }

    public static void DeleteProfile(string profileName = null)
    {
        if (profileName == null)
            profileName = Current;

        if (!SavedProfiles.ContainsKey(profileName)) return;

        if (!SavedProfiles.Remove(profileName)) return;
        if (profileName == Current)
            Current = null;
        SaveProfiles(2);
    }
}