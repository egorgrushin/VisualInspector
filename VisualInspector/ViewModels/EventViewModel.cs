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

        public Guid Id { get; set; }

        private readonly Event eventModel;
        private readonly IVisualFactory<EventViewModel> visualFactory;
        

        public string VideoFileName
        {
            get { return eventModel.VideoFileName; }
        }

		public List<BitmapImage> InitFramesList(object sender, DoWorkEventArgs e)
        {
            return eventModel.InitFramesList(sender, e);
        }
        public EventViewModel(Event eventModel, IVisualFactory<EventViewModel> visualFactory)
        {
            this.eventModel = eventModel;
            this.visualFactory = visualFactory;
            Id = Guid.NewGuid();
        }
        public DrawingVisual GetVisual(DrawingVisual canvas, Rect rect)
        {
            return visualFactory.Create(this, canvas, rect);
        }

        public WarningLevels GetWarningLevel()
        {
            return eventModel.WarningLevel;
        }

        /// <summary>
        /// Toggle visual for selection(selected or not)
        /// </summary>
        /// <param name="oldVisual">Visual to toggle</param>
        /// <param name="toggleState">true - selected, false - unselected</param>
        /// <returns>Toggled visual, but it toggling anyway</returns>
        public DrawingVisual ToggleVisual(DrawingVisual oldVisual, bool toggleState)
        {
            return visualFactory.Toggle(this, oldVisual, toggleState);
        }

        internal void MarkSelected(DrawingVisual visual, bool p)
        {
            visualFactory.Mark(this, visual, p);
        }
        public override string ToString()
        {
            return eventModel.ToString();
        }

    }
}
