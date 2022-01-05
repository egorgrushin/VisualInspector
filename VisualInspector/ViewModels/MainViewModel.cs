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
using System.Diagnostics;
using System.Windows;

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
            Rooms = new CrossthreadObservableCollection<RoomViewModel>(SynchronizationContext.Current, Thread.CurrentThread);
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
            var thread = new Thread(InitRoomsFromOtherThread);
            //thread.Start(20);
            InitRooms(50);
            FillRooms();
        }


        #region TestCrossthreadCollection
        private void InitRoomsFromOtherThread(object obj)
        {
            var count = (int)obj;
            for (int i = 0; i < count; i++)
            {
                var roomViewModel = new RoomViewModel();
                for (int j = 0; j < 10; j++)
                {
                    var newEvent = new Event()
                    {
                        WarningLevel =
                        (WarningLevels)Enum.GetValues(typeof(WarningLevels)).GetValue(rd.Next(Enum.GetValues(typeof(WarningLevels)).Length))
                    };
                    roomViewModel.Events.Add(new EventViewModel(newEvent, visualFactory));
                }
                Rooms.Add(roomViewModel);
                Thread.Sleep(200);
            }
        }
        #endregion
        private void InitRooms(int n)
        {
            for (int i = 0; i < n; i++)
            {
                var roomViewModel = new RoomViewModel();
                Rooms.Add(roomViewModel);
            }
        }
        #region TestMultiThreadInitialization
        private void AddEventInRoom(object state)
        {
            Trace.WriteLine("AddEventInRoom in: " + Thread.CurrentThread.ManagedThreadId);
            var parameters = state as MultiParameter;
            var room = parameters.Parameters[0] as RoomViewModel;
            var ev = parameters.Parameters[1] as EventViewModel;
            room.Events.Add(ev);
            
        }
        private void FillRooms()
        {
            Trace.WriteLine("FillRooms in: " + Thread.CurrentThread.ManagedThreadId);
            var context = SynchronizationContext.Current;
            foreach (var item in Rooms)
            {
                var parameters = new MultiParameter(new object[] { item, context, rd.Next(100, 3000) });
                var thread = new Thread(InitEvents);
                thread.IsBackground = true;
                thread.Start(parameters);
            }
        }
        private void InitEvents(object state)
        {
            Trace.WriteLine("InitEvents in: " + Thread.CurrentThread.ManagedThreadId);
            var parameters = state as MultiParameter;
            var room = parameters.Parameters[0] as RoomViewModel;
            var context = parameters.Parameters[1] as SynchronizationContext;
            var rdG = (int)parameters.Parameters[2];
            int n = rd.Next(50, 100);
            for (int j = 0; j < n; j++)
            {
                var newEvent = new Event()
                {
                    WarningLevel =
                    (WarningLevels)Enum.GetValues(typeof(WarningLevels)).GetValue(rd.Next(Enum.GetValues(typeof(WarningLevels)).Length))
                };
                var parameter = new object[] { room, new EventViewModel(newEvent, visualFactory) };
                context.Send(AddEventInRoom, new MultiParameter(parameter));
                Thread.Sleep(rdG);
            }
        }


        #endregion

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
