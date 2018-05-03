using BinarySerialization;

namespace Bililive_dm_VR.Desktop.Model
{
    public class ProfileCommand : Command
    {
        [FieldOrder(0)]
        public Profile Profile { get; set; }
    }
}
