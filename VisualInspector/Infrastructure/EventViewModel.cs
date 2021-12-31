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
        public DrawingVisual ChangeVisual(DrawingVisual oldVisual)
        {
            return visualFactory.Change(this, oldVisual);
        }
    }
}
