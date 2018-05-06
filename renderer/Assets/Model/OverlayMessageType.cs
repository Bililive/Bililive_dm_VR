using System;

[Flags]
public enum OverlayMessageType : int
{
    /// <summary>
    /// 无
    /// </summary>
    None = 0,
    /// <summary>
    /// 弹幕消息
    /// </summary>
    Danmaku = 1 << 0,
    /// <summary>
    /// 赠送礼物
    /// </summary>
    Gift = 1 << 1,
    /// <summary>
    /// 进入直播间
    /// </summary>
    Welcome = 1 << 2,
    /// <summary>
    /// 直播间关闭
    /// </summary>
    StreamClose = 1 << 3,

}
