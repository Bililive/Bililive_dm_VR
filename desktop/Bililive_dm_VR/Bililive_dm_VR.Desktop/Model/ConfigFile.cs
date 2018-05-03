using BinarySerialization;
using System.Collections.Generic;

namespace Bililive_dm_VR.Desktop.Model
{
    public class ConfigFile
    {
        /// <summary>
        /// 配置文件版本号
        /// </summary>
        [FieldOrder(0)]
        public uint Version { get; set; } = 1;

        /// <summary>
        /// 上次连接的直播房间号
        /// </summary>
        [FieldOrder(1)]
        [SerializeWhen(nameof(Version), 1)]
        public int RoomId { get; set; } = 0;

        /// <summary>
        /// 配置数量
        /// </summary>
        [FieldOrder(2)]
        [SerializeWhen(nameof(Version), 1)]
        public int ProfileCount { get; set; } = 0;

        /// <summary>
        /// 配置列表
        /// </summary>
        [FieldOrder(3)]
        [SerializeWhen(nameof(Version), 1)]
        [ItemLength(nameof(ProfileCount))]
        // [FieldChecksum(nameof(Checksum), Mode = ChecksumMode.Xor)]
        public List<Profile> Profiles { get; set; } = null;

        /// <summary>
        /// 校验位 
        /// </summary>
        // [FieldOrder(4)]
        // [SerializeWhen(nameof(Version), 1)]
        // public byte Checksum { get; set; }
    }
}
