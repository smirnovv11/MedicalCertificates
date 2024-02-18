using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;

namespace MedicalCertificates.Services
{
    public class DateBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int period = Int32.Parse(JsonServices.ReadByProperty("warningPeriod"));

            if (value is DateTime date)
            {
                if (date <= DateTime.Now)
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#b00c00"));
                }
                else if (date <= DateTime.Now.AddMonths(period))
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ebb434"));
                }
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
