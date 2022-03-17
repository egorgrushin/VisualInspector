using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using VisualInspector.ViewModels;
using NLog;

namespace VisualInspector.Infrastructure
{
    public class EventVisualFactory : IVisualFactory<EventViewModel>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public static int VisualSize { get { return 16;	} }

        private readonly IDictionary<string, Pen> pens;
        private readonly IDictionary<string, Brush> brushes;
		private readonly Size itemSize;

        public EventVisualFactory(IDictionary<string, Pen> argPens, IDictionary<string, Brush> argBrushes)
        {
            pens = argPens;
            brushes = argBrushes;
			itemSize = new Size(VisualSize, VisualSize);
        }

        public void Create(EventViewModel viewModel, DrawingVisual canvas)
        {
            using (var context = canvas.RenderOpen())
			{
				var rectBorder = new Rect(new Point(0, 0), itemSize);
                Brush brush = brushes[viewModel.WarningLevel.ToString()];
                context.DrawRectangle(Brushes.Black, null, rectBorder);

                var rectContent = new Rect(new Point(rectBorder.Location.X + 1, rectBorder.Location.Y + 1),
                    new Size(rectBorder.Size.Width - 2, rectBorder.Size.Height - 2));
                context.DrawRectangle(brush, null, rectContent);
            }
        }

		//TODO add method for getting a rect
        public void Toggle(EventViewModel viewModel, DrawingVisual canvas, bool toggleState)
        {
            using (var context = canvas.RenderOpen())
            {
				var rectBorder = canvas.ContentBounds;
				context.DrawRectangle(Brushes.Black, null, canvas.ContentBounds);
                var rectContent = new Rect(new Point(rectBorder.Location.X + 1, rectBorder.Location.Y + 1),
                    new Size(rectBorder.Size.Width - 2, rectBorder.Size.Height - 2));
                var brushOriginal = brushes[viewModel.WarningLevel.ToString()];
                var brushToToggle = new SolidColorBrush(Colors.Black);
                brushToToggle.Opacity = toggleState ? 0.35 : 0.0;
                context.DrawRectangle(brushOriginal, null, rectContent);
				context.DrawRectangle(brushToToggle, null, rectContent);
            }
        }


        public void Mark(EventViewModel eventViewModel, DrawingVisual visual, bool markState)
        {
            using (var context = visual.RenderOpen())
            {
                var rectBorder = visual.ContentBounds;
                context.DrawRectangle(Brushes.Black, null, visual.ContentBounds);
                var rectContent = new Rect(new Point(rectBorder.Location.X + 1, rectBorder.Location.Y + 1),
                    new Size(rectBorder.Size.Width - 2, rectBorder.Size.Height - 2));

                var brushOriginal = brushes[eventViewModel.WarningLevel.ToString()];
                var brushToToggle = new SolidColorBrush(Colors.Green);
                brushToToggle.Opacity = markState ? 0.35 : 0.0;
                context.DrawRectangle(brushOriginal, null, rectContent);
                context.DrawRectangle(brushToToggle, null, rectContent);
            }
        }
    }
}
