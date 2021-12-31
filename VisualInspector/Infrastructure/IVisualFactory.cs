using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace VisualInspector.Infrastructure
{
    public interface IVisualFactory<T> where T : ViewModel
    {
        DrawingVisual Create(T viewModel, DrawingVisual canvas, Rect rect);
        DrawingVisual Change(T viewModel, DrawingVisual canvas);
    }
}
