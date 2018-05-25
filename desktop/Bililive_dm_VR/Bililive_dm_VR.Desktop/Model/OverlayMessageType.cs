using System;
using System.ComponentModel;

namespace Bililive_dm_VR.Desktop.Model
{
    [Flags]
    public enum OverlayMessageType : int
    {
        /// <summary>
        /// 无
        /// </summary>
        [Description("无")] None = 0,
        /// <summary>
        /// 弹幕消息
        /// </summary>
        [Description("弹幕消息")] Danmaku = 1 << 0,
        /// <summary>
        /// 赠送礼物
        /// </summary>
        [Description("礼物")] Gift = 1 << 1,
        /// <summary>
        /// 进入直播间
        /// </summary>
        [Description("进入直播间")] Welcome = 1 << 2,
        /// <summary>
        /// 直播间关闭
        /// </summary>
        [Description("直播间关闭")] StreamClose = 1 << 3,

    }
}
