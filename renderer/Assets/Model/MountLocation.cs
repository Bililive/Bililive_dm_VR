using System.ComponentModel;

/// <summary>
/// Used when attaching Overlays to Controllers, to determine the base attachment offset.
/// </summary>
public enum MountLocation
{
    /// <summary>
    /// Directly in the center at (0, 0, 0), facing upwards through the Trackpad.
    /// </summary>
    [Description("中心")] Center = 0,
    /// <summary>
    /// At the end of the controller, like a staff ornament, facing towards the center.
    /// </summary>
    [Description("顶端")] FlatAbove = 1,
    /// <summary>
    /// At the bottom of the controller, facing away from the center.
    /// </summary>
    [Description("底端")] FlatBelow = 2,
    /// <summary>
    /// At the bottom of the controller, facing towards the center.
    /// </summary>
    [Description("底端反手")] FlatBelowFlipped = 3,
    /// <summary>
    /// Just above the Trackpad, facing away from the center.
    /// </summary>
    [Description("上侧")] Above = 4,
    /// <summary>
    /// Just above thr Trackpad, facing the center.
    /// </summary>
    [Description("上侧反手")] AboveFlipped = 5,
    /// <summary>
    /// Just below the Trigger, facing the center.
    /// </summary>
    [Description("下侧")] Below = 6,
    /// <summary>
    /// Just below the Trigger, facing away from the center.
    /// </summary>
    [Description("下侧反手")] BelowFlipped = 7,
    /// <summary>
    /// When holding the controller out vertically, Like "Center", but "Up", above the controller.
    /// </summary>
    [Description("上")] Up = 8,
    /// <summary>
    /// When holding the controller out vertically, Like "Center", but "Down", below the controller.
    /// </summary>
    [Description("下")] Down = 9,
    /// <summary>
    /// When holding the controller out vertically, Like "Center", but "Left", to the side of the controller.
    /// </summary>
    [Description("左")] Left = 10,
    /// <summary>
    /// When holding the controller out vertically, Like "Center", but "Right", to the side of the controller.
    /// </summary>
    [Description("右")] Right = 11,
}
