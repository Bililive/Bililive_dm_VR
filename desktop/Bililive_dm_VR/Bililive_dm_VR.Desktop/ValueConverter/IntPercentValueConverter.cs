using System;
using System.Globalization;
using System.Windows.Data;

namespace Bililive_dm_VR.Desktop
{
    public class IntPercentValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intvalue && targetType == typeof(double?))
            {
                return intvalue / 100d;
            }
            else
                throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doublevalue && targetType == typeof(int))
            {
                return (int)Math.Round(doublevalue * 100);
            }
            else
                throw new NotImplementedException();
        }
    }
}
