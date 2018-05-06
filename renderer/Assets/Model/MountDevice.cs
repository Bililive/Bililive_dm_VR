using System.ComponentModel;

/// <summary>
/// Used to determine where an Overlay should be attached.
/// </summary>
public enum MountDevice : int
{
    /// <summary>
    /// Attempts to attach the Overlay to the World
    /// </summary>
    [Description("世界")] World = 0,
    /// <summary>
    /// Attempts to attach the Overlay to the Screen / HMD
    /// </summary>
    [Description("屏幕")] Screen = 1,
    /// <summary>
    /// Attempts to attach the Overlay to the Left Controller
    /// </summary>
    [Description("左手柄")] LeftController = 2,
    /// <summary>
    /// Attempts to attach the Overlay to the Right Controller
    /// </summary>
    [Description("右手柄")] RightController = 3,

}
