using System.ComponentModel;

public enum AnimationType : int
{
    /// <summary>
    /// Don't animate this Overlay.
    /// </summary>
    [Description("无")] None = 0,
    /// <summary>
    /// Animate this Overlay by changing its Alpha.
    /// </summary>
    [Description("透明度")] Alpha = 1,
    /// <summary>
    /// Animate this Overlay by scaling it.
    /// </summary>
    [Description("缩放")] Scale = 2,
    /// <summary>
    /// Animate this Overlay by changing its Alpha and scaling it.
    /// </summary>
    [Description("透明度+缩放")] AlphaAndScale = 3,
}
