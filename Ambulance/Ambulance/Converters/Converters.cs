using System;
using Xamarin.Forms;
using Ambulance.ObjectModel;

namespace Ambulance.Converters
{
    public class PriorityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            DateTime? ToDate = value as DateTime?;
            if (ToDate == null) return Color.Green;

            var ExpiredTime = TimeSpan.FromTicks(ToDate.Value.Ticks - DateTime.Now.Ticks).TotalHours;

            if (ExpiredTime < 0)
                return Color.Black;
            if (ExpiredTime < 2)
                return Color.Red;
            if (ExpiredTime < 4)
                return Color.Orange;

            return Color.Lime;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class TimeFromToConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is Order Order)) return null;

            return Order.ArrivalDate.ToString("dd.MM.yy HH:mm");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class PatientInfoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is Order Order)) return null;

            return Order.SickName + Environment.NewLine + Order.SickPhone;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
