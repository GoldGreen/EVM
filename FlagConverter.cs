using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace EVM
{
    public class FlagConverter : MarkupExtension, IValueConverter
    {
        private static FlagConverter single = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = (bool)value;
            
            if (parameter.Equals("int"))
            {
                return flag ? 1 : 0;
            }
            else if (parameter.Equals("color"))
            {
                return flag ? Brushes.Red : Brushes.Green;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return single;
        }
    }
}
