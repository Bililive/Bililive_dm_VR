using BinarySerialization;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Bililive_dm_VR.Desktop.Model
{
    public class Profile : INotifyPropertyChanged
    {
        /// <summary>
        /// 配置文件名
        /// </summary>
        [FieldOrder(0)]
        public string Name { get => name; set => SetField(ref name, value); }
        private string name = "默认配置";

        /// <summary>
        /// 固定设备
        /// </summary>
        [FieldOrder(1)]
        public MountDevice MountDevice { get => mountDevice; set => SetField(ref mountDevice, value); }
        private MountDevice mountDevice = MountDevice.RightController;

        /// <summary>
        /// 固定位置
        /// </summary>
        [FieldOrder(2)]
        public MountLocation MountLocation { get => mountLocation; set => SetField(ref mountLocation, value); }
        private MountLocation mountLocation = MountLocation.BelowFlipped;

        /// <summary>
        /// 偏移坐标 X
        /// </summary>
        [FieldOrder(3)]
        public float OffsetX { get => offsetX; set => SetField(ref offsetX, value); }
        private float offsetX = 0f;

        /// <summary>
        /// 偏移坐标 Y
        /// </summary>
        [FieldOrder(4)]
        public float OffsetY { get => offsetY; set => SetField(ref offsetY, value); }
        private float offsetY = 0f;

        /// <summary>
        /// 偏移坐标 Z
        /// </summary>
        [FieldOrder(5)]
        public float OffsetZ { get => offsetZ; set => SetField(ref offsetZ, value); }
        private float offsetZ = 0f;

        /// <summary>
        /// 旋转 X
        /// </summary>
        [FieldOrder(6)]
        public float RotationX { get => rotationX; set => SetField(ref rotationX, value); }
        private float rotationX = 0f;

        /// <summary>
        /// 旋转 Y
        /// </summary>
        [FieldOrder(7)]
        public float RotationY { get => rotationY; set => SetField(ref rotationY, value); }
        private float rotationY = 0f;

        /// <summary>
        /// 旋转 Z
        /// </summary>
        [FieldOrder(8)]
        public float RotationZ { get => rotationZ; set => SetField(ref rotationZ, value); }
        private float rotationZ = 0f;

        /// <summary>
        /// 透明度 ( 0 .. 100 )
        /// </summary>
        [FieldOrder(9)]
        public int Alpha { get => alpha; set => SetField(ref alpha, value); }
        private int alpha = 80;

        /// <summary>
        /// 缩放倍数 ( 1% .. 100% )
        /// </summary>
        [FieldOrder(10)]
        public int Scale { get => scale; set => SetField(ref scale, value); }
        private int scale = 15;

        /// <summary>
        /// 颜色 ( AARRGGBB )
        /// </summary>
        [FieldOrder(11)]
        public int Color { get => color; set => SetField(ref color, value); }
        private int color = 275816703;

        /// <summary>
        /// 动作类型
        /// </summary>
        [FieldOrder(12)]
        public AnimationType AnimateOnGaze { get => animateOnGaze; set => SetField(ref animateOnGaze, value); }
        private AnimationType animateOnGaze;

        /// <summary>
        /// 动作后透明度 ( 0% .. 100% )
        /// </summary>
        [FieldOrder(13)]
        public int AnimationAlpha { get => animationAlpha; set => SetField(ref animationAlpha, value); }
        private int animationAlpha;

        /// <summary>
        /// 动作后缩放倍数
        /// </summary>
        [FieldOrder(14)]
        public int AnimationScale { get => animationScale; set => SetField(ref animationScale, value); }
        private int animationScale;

        /// <summary>
        /// 显示的消息类型
        /// </summary>
        [FieldOrder(15)]
        public OverlayMessageType MessageType { get => messageType; set => SetField(ref messageType, value); }
        private OverlayMessageType messageType;


        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}
