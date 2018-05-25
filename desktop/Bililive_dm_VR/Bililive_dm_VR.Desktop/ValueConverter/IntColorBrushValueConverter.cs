using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Bililive_dm_VR.Desktop
{
    public class IntColorBrushValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int color32)
            {
                return new SolidColorBrush(new Color()
                {
                    R = (byte)((color32 >> 24) & 0xff),
                    G = (byte)((color32 >> 16) & 0xff),
                    B = (byte)((color32 >> 8) & 0xff),
                    A = 0xff,
                });
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
