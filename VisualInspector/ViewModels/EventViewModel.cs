using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Foundation;
using System.Windows.Media;
using VisualInspector.Models;
using System.Windows;
using VisualInspector.ViewModels;
using System.Windows.Media.Imaging;
using VisualInspector.Infrastructure;
using System.Threading;
using NLog;
using System.ComponentModel;

namespace VisualInspector.ViewModels
{
    public class EventViewModel : ViewModel
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

        private readonly Event eventModel;
        private readonly IVisualFactory<EventViewModel> visualFactory;
        public WarningLevels WarningLevel { get { return eventModel.WarningLevel; } }

		public EventViewModel(Event eventModel, IVisualFactory<EventViewModel> visualFactory)
		{
			this.eventModel = eventModel;
			this.visualFactory = visualFactory;
		}

        public string VideoFileName
        {
            get { return eventModel.VideoFileName; }
        }
		public List<BitmapImage> InitFramesList(object sender, DoWorkEventArgs e)
        {
            return eventModel.InitFramesList(sender, e);
        }

		#region Working vith visuals
        public void DrawVisual(DrawingVisual canvas)
        {
            visualFactory.Create(this, canvas);
        }
        public void ToggleVisual(DrawingVisual oldVisual, bool toggleState)
        {
            visualFactory.Toggle(this, oldVisual, toggleState);
        }
        internal void MarkSelected(DrawingVisual visual, bool p)
        {
            visualFactory.Mark(this, visual, p);
        }
		#endregion

        public override string ToString()
        {
            return eventModel.ToString();
        }

    }
}
