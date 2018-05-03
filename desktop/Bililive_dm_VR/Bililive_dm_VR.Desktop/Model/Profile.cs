using BinarySerialization;

namespace Bililive_dm_VR.Desktop.Model
{
    public class Profile
    {
        /// <summary>
        /// 配置文件名
        /// </summary>
        [FieldLength(0)]
        public string Name { get; set; }

        /// <summary>
        /// 固定设备
        /// </summary>
        [FieldLength(1)]
        public MountDevice MountDevice { get; set; }

        /// <summary>
        /// 固定位置
        /// </summary>
        [FieldLength(2)]
        public MountLocation MountLocation { get; set; }

        /// <summary>
        /// 偏移坐标X
        /// </summary>
        [FieldLength(3)]
        public int OffsetX { get; set; }

        /// <summary>
        /// 偏移坐标Y
        /// </summary>
        [FieldLength(4)]
        public int OffsetY { get; set; }

        /// <summary>
        /// 偏移坐标Z
        /// </summary>
        [FieldLength(5)]
        public int OffsetZ { get; set; }

        /// <summary>
        /// 透明度 ( 0 - 100 )
        /// </summary>
        [FieldLength(6)]
        public int Alpha { get; set; }

        /// <summary>
        /// 缩放倍数 ( 100% )
        /// </summary>
        [FieldLength(7)]
        public int Scale { get; set; }

        /// <summary>
        /// 颜色 ( AARRGGBB )
        /// </summary>
        [FieldLength(8)]
        public int Color { get; set; }

        /// <summary>
        /// 动作类型
        /// </summary>
        [FieldLength(9)]
        public AnimationType AnimationType { get; set; }

        /// <summary>
        /// 动作后透明度
        /// </summary>
        [FieldLength(10)]
        public int AnimationAlpha { get; set; }

        /// <summary>
        /// 动作后缩放倍数
        /// </summary>
        [FieldLength(11)]
        public int AnimationScale { get; set; }

        /// <summary>
        /// 显示的消息类型
        /// </summary>
        [FieldLength(12)]
        public MessageType MessageType { get; set; }



    }
}
