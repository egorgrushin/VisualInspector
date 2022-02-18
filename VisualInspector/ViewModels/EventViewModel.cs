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

namespace VisualInspector.ViewModels
{
    public class EventViewModel : ViewModel
    {

        public Guid Id { get; set; }

        private readonly Event eventModel;
        private readonly IVisualFactory<EventViewModel> visualFactory;
        

        public string VideoFileName
        {
            get { return eventModel.VideoFileName; }
        }
		public List<BitmapImage> FramesList
		{
			get
			{
				return eventModel.FramesList;
			}
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

        public override string ToString()
        {
            return string.Format("Warning: {0}\r\nLock: {1}\r\nSensor: {2}\r\nAccess: {3}\r\nRoom: {4}\r\nDateTime: {5}",
                eventModel.WarningLevel,
                eventModel.Lock,
                Enum.GetName(typeof(Sensors), eventModel.Sensor),
                Enum.GetName(typeof(AccessLevels), eventModel.AccessLevel),
                eventModel.Room, 
                string.Format("{0:dd.MM.yyyy hh:mm:ss}", eventModel.DateTime));
        }
    }
}
