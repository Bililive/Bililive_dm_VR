using BinarySerialization;

public class Profile
{
    /// <summary>
    /// 配置文件名
    /// </summary>
    [FieldOrder(0)]
    public string Name { get; set; }

    /// <summary>
    /// 固定设备
    /// </summary>
    [FieldOrder(1)]
    public MountDevice MountDevice { get; set; }

    /// <summary>
    /// 固定位置
    /// </summary>
    [FieldOrder(2)]
    public MountLocation MountLocation { get; set; }

    /// <summary>
    /// 偏移坐标 X
    /// </summary>
    [FieldOrder(3)]
    public float OffsetX { get; set; }

    /// <summary>
    /// 偏移坐标 Y
    /// </summary>
    [FieldOrder(4)]
    public float OffsetY { get; set; }

    /// <summary>
    /// 偏移坐标 Z
    /// </summary>
    [FieldOrder(5)]
    public float OffsetZ { get; set; }

    /// <summary>
    /// 旋转 X
    /// </summary>
    [FieldOrder(6)]
    public float RotationX { get; set; }

    /// <summary>
    /// 旋转 Y
    /// </summary>
    [FieldOrder(7)]
    public float RotationY { get; set; }

    /// <summary>
    /// 旋转 Z
    /// </summary>
    [FieldOrder(8)]
    public float RotationZ { get; set; }

    /// <summary>
    /// 透明度 ( 0 .. 100 )
    /// </summary>
    [FieldOrder(9)]
    public int Alpha { get; set; }

    /// <summary>
    /// 缩放倍数 ( 1% .. 100% )
    /// </summary>
    [FieldOrder(10)]
    public int Scale { get; set; }

    /// <summary>
    /// 颜色 ( AARRGGBB )
    /// </summary>
    [FieldOrder(11)]
    public int Color { get; set; }

    /// <summary>
    /// 动作类型
    /// </summary>
    [FieldOrder(12)]
    public AnimationType AnimateOnGaze { get; set; }

    /// <summary>
    /// 动作后透明度 ( 0% .. 100% )
    /// </summary>
    [FieldOrder(13)]
    public int AnimationAlpha { get; set; }

    /// <summary>
    /// 动作后缩放倍数
    /// </summary>
    [FieldOrder(14)]
    public int AnimationScale { get; set; }

    /// <summary>
    /// 显示的消息类型
    /// </summary>
    [FieldOrder(15)]
    public OverlayMessageType MessageType { get; set; }


}