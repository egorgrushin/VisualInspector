using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Foundation;
using System.Windows.Media;
using VisualInspector.Models;
using System.Windows;

namespace VisualInspector.Infrastructure
{
    public class EventViewModel : ViewModel
    {

        public Guid Id { get; set; }

        private readonly Event eventModel;
        private readonly IVisualFactory<EventViewModel> visualFactory;

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
			return String.Format("{0}", GetWarningLevel());
		}
    }
}
