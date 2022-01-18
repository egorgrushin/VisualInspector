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
		public event EventHandler SelectionChanged;

        public ObservableNotifiableCollection<EventViewModel> Events
        {
            get { return Get(() => Events); }
            set { Set(() => Events, value); }
        }
		public EventViewModel SelectedEvent
		{
			get	{ return Get(() => SelectedEvent); }
			set	{
					Set(() => SelectedEvent, value);
					OnSelectionChanged();
				}
		}

        public RoomViewModel()
        {
            Events = new ObservableNotifiableCollection<EventViewModel>();
        }

		private void OnSelectionChanged()
		{
			var handler = SelectionChanged;
			if(handler != null)
			{
				handler(this, EventArgs.Empty);
			}
		}
    }
}
