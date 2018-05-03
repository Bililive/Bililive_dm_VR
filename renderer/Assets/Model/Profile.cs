using BinarySerialization;

namespace Bililive_dm_VR.Desktop.Model
{
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
        /// 偏移坐标X
        /// </summary>
        [FieldOrder(3)]
        public int OffsetX { get; set; }

        /// <summary>
        /// 偏移坐标Y
        /// </summary>
        [FieldOrder(4)]
        public int OffsetY { get; set; }

        /// <summary>
        /// 偏移坐标Z
        /// </summary>
        [FieldOrder(5)]
        public int OffsetZ { get; set; }

        /// <summary>
        /// 透明度 ( 0 - 100 )
        /// </summary>
        [FieldOrder(6)]
        public int Alpha { get; set; }

        /// <summary>
        /// 缩放倍数 ( 100% )
        /// </summary>
        [FieldOrder(7)]
        public int Scale { get; set; }

        /// <summary>
        /// 颜色 ( AARRGGBB )
        /// </summary>
        [FieldOrder(8)]
        public int Color { get; set; }

        /// <summary>
        /// 动作类型
        /// </summary>
        [FieldOrder(9)]
        public AnimationType AnimationType { get; set; }

        /// <summary>
        /// 动作后透明度
        /// </summary>
        [FieldOrder(10)]
        public int AnimationAlpha { get; set; }

        /// <summary>
        /// 动作后缩放倍数
        /// </summary>
        [FieldOrder(11)]
        public int AnimationScale { get; set; }

        /// <summary>
        /// 显示的消息类型
        /// </summary>
        [FieldOrder(12)]
        public MessageType MessageType { get; set; }


    }
}
