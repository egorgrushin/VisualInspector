using Foundation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using VisualInspector.Models;
using System.Windows;

namespace VisualInspector.Infrastructure.Converters
{
    public class LevelBrushConverter : ConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var level = (WarningLevels)value;
            var brush = new SolidColorBrush();
            switch (level)
            {
                case WarningLevels.Normal:
                    brush.Color = Colors.LightGreen;
                    break;
                case WarningLevels.Middle:
                    brush.Color = Colors.Yellow;
                    break;
                case WarningLevels.High:
                    brush.Color = Colors.Red;
                    break;
            }
            return brush;
        }
    }

    public class IsNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value == null);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("IsNullConverter can only be used OneWay.");
        }
    }
	public class IsVisibleConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var boolValue = (bool)value;

			return boolValue ? Visibility.Visible : Visibility.Hidden;
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new InvalidOperationException("IsVisibleConverter can only be used OneWay.");
		}
	}
}
