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
using System.Windows.Controls;

namespace VisualInspector.Infrastructure.Converters
{
    public class AdaptingWidthListBoxItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var listBox = value as ListBox;
            var finalWidth = listBox.ActualWidth / listBox.Items.Count;
            return finalWidth;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
