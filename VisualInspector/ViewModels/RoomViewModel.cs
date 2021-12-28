using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VisualInspector.Infrastructure;

namespace VisualInspector.ViewModels
{
    public class RoomViewModel : ViewModel
    {
        public CrossthreadObservableCollection<EventViewModel> Events
        {
            get { return Get(() => Events); }
            set { Set(() => Events, value); }
        }
        public RoomViewModel()
        {
            Events = new CrossthreadObservableCollection<EventViewModel>();
        }
    }
}
