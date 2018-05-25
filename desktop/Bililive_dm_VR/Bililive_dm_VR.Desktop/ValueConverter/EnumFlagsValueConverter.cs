using System;
using System.Globalization;
using System.Windows.Data;

namespace Bililive_dm_VR.Desktop
{
    public class EnumFlagsValuveConverter : IValueConverter
    {
        private int target;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int mask = (int)parameter;
            target = (int)value;
            return ((mask & target) != 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            target ^= (int)parameter;
            return Enum.ToObject(targetType, target);
        }
    }
}
