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
}
