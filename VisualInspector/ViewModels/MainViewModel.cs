using Foundation;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VisualInspector.Models;
using System.Windows.Input;
using VisualInspector.Infrastructure;
using System.Windows.Media;
using System.Threading;

namespace VisualInspector.ViewModels
{
    public class MainViewModel : ViewModel
    {
        Random rd;
        public IEnumerable<string> EnumCol { get; set; }

        public CrossthreadObservableCollection<RoomViewModel> Rooms
        {
            get { return Get(() => Rooms); }
            set { Set(() => Rooms, value); }
        }

        public Event CurrentEvent
        {
            get { return Get(() => CurrentEvent); }
            set { Set(() => CurrentEvent, value); }
        }

        public MainViewModel()
        {
            rd = new Random();
            Rooms = new CrossthreadObservableCollection<RoomViewModel>();
            EnumCol = Enum.GetNames(typeof(WarningLevels));
            CurrentEvent = new Event() { WarningLevel = WarningLevels.Normal };
            var pens = new Dictionary<string, Pen>() { { "Black", new Pen(Brushes.Black, 2) } };
            var brushes = new Dictionary<string, Brush>()
            { 
                { "Normal", Brushes.LimeGreen},
                { "Middle", Brushes.Yellow },
                { "High", Brushes.Red }
            };
            visualFactory = new EventVisualFactory(pens, brushes);
            //TestInit();
            new Thread(TestInit)
                {
                    IsBackground = true
                }.Start();
        }
        private void TestInit()
        {
            for (int i = 0; i < 200; i++)
            {
                var roomViewModel = new RoomViewModel();
                int n = rd.Next(1, 200);
                for (int j = 0; j < 200; j++)
                {
                    var newEvent = new Event()
                    {
                        WarningLevel =
                        (WarningLevels)Enum.GetValues(typeof(WarningLevels)).GetValue(rd.Next(Enum.GetValues(typeof(WarningLevels)).Length))
                    };
                    roomViewModel.Events.Add(new EventViewModel(newEvent, visualFactory));
                }
                Rooms.Add(roomViewModel);
                //Thread.Sleep(TimeSpan.FromSeconds(0.1));
            }
        }
        private ICommand addNewEventCommand;
        private IVisualFactory<EventViewModel> visualFactory;

        public ICommand AddNewEventCommand
        {
            get
            {
                if (addNewEventCommand == null)
                    addNewEventCommand = new RelayCommand(AddNewEvent);
                return addNewEventCommand;
            }
        }

        private void AddNewEvent(object obj)
        {
            //Events.Add(new EventViewModel(CurrentEvent, visualFactory));
            CurrentEvent = new Event() { WarningLevel = WarningLevels.Normal };
        }
    }
}
