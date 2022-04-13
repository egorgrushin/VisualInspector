using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace VisualInspector.Infrastructure
{
    public interface IVisualFactory<T> where T : ViewModelBase
    {
		/// <summary>
		/// Drawing on visual from event info
		/// </summary>
		/// <param name="viewModel">Event</param>
		/// <param name="canvas">Visual to draw on</param>
        void Create(T viewModel, DrawingVisual canvas);

		/// <summary>
		/// Toggle visual for selection(selected or not)
		/// </summary>
		/// <param name="viewModel">Event</param>
		/// <param name="canvas">Visual to draw on</param>
		/// <param name="toggleState">true - selected, false - unselected</param>
        void Toggle(T viewModel, DrawingVisual canvas, bool toggleState);

		/// <summary>
		/// Mark a visual for mouse hover event
		/// </summary>
		/// <param name="viewModel">Event</param>
		/// <param name="canvas">Visual to draw on</param>
		/// <param name="markState">true - hovered, false - unhovered</param>
        void Mark(ViewModels.EventViewModel eventViewModel, DrawingVisual visual, bool markState);
    }
}
