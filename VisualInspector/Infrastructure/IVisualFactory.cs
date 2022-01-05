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


		/// <summary>
		/// Toggle visual for selection(selected or not)
		/// </summary>
		/// <param name="viewModel">model of the visual, for future params</param>
		/// <param name="canvas">visual itself</param>
		/// <param name="toggleState">true - selected, false - unselected</param>
		/// <returns></returns>
        DrawingVisual Toggle(T viewModel, DrawingVisual canvas, bool toggleState);
    }
}
