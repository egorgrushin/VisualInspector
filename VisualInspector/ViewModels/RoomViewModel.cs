using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VisualInspector.Infrastructure;
using System.Windows.Media.Imaging;
using NLog;

namespace VisualInspector.ViewModels
{
    public class RoomViewModel : ViewModelBase
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public event EventHandler SelectionChanged;
		public RoomViewModel(int number)
		{
			Number = number;
			Events = new ObservableNotifiableCollection<EventViewModel>();
		}

        public ObservableNotifiableCollection<EventViewModel> Events
        {
            get { return Get(() => Events); }
            set { Set(() => Events, value); }
        }
        public int Number
        {
            get { return Get(() => Number); }
            set { Set(() => Number, value); }
        }
        
		public EventViewModel SelectedEvent
		{
			get	{ return Get(() => SelectedEvent); }
			set	{
					Set(() => SelectedEvent, value);
					OnSelectionChanged();
				}
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
