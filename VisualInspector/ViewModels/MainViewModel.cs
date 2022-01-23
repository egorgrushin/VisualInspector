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
using VisualInspector.Infrastructure.ServerPart;

namespace VisualInspector.ViewModels
{
    public enum Sensors
    {
        Outside, Inside
    }

    public enum AccessLevels
    {
        Without, Guest, Staff, Administator
    }
    public class MainViewModel : ViewModel
    {
        #region Properties & fields
        Random rd;
        private IVisualFactory<EventViewModel> visualFactory;
        private TcpServer server;
        private bool selectionLock;
        public IEnumerable<string> EnumCol { get; set; }

        public ObservableNotifiableCollection<RoomViewModel> Rooms
        {
            get { return Get(() => Rooms); }
            set { Set(() => Rooms, value); }
        }


        public EventViewModel SelectedEvent
        {
            get { return Get(() => SelectedEvent); }
            set { Set(() => SelectedEvent, value); }
        }

        #endregion

        #region Constructors
        public MainViewModel()
        {
            rd = new Random();
            Rooms = new ObservableNotifiableCollection<RoomViewModel>();
            EnumCol = Enum.GetNames(typeof(WarningLevels));

            SelectedEvent = null;
            var pens = new Dictionary<string, Pen>() { { "Black", new Pen(Brushes.Black, 2) } };
            var brushes = new Dictionary<string, Brush>()
            { 
                { "Normal", App.Current.Resources["forNormalBrush"] as SolidColorBrush},
                { "Middle", App.Current.Resources["forMiddleBrush"] as SolidColorBrush},
                { "High",  App.Current.Resources["forHighBrush"] as SolidColorBrush}
            };
            visualFactory = new EventVisualFactory(pens, brushes);
            //var thread = new Thread(InitRoomsFromOtherThread);
            //thread.Start(20);
            InitRooms(50);
            FillRooms();
            //LaunchServer();
        }
        #endregion


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

        #region Server part
        private void LaunchServer()
        {
            server = new TcpServer(SynchronizationContext.Current)
            {
                Port = 3010
            };
            server.MessageRecieved += server_MessageRecieved;
            new Thread(server.Start)
                {
                    IsBackground = true
                }.Start();


        }

        private Event Proceed(string msg)
        {
            var data = msg.Split('/');
            var lockNumber = int.Parse(data[1]);
            var sensorNumber = int.Parse(data[2]);
            var accessLevel = int.Parse(data[3]);
            var roomNumber = int.Parse(data[4]);
            var newEvent = new Event()
            {
                Lock = lockNumber,
                Sensor = sensorNumber,
                AccessLevel = accessLevel,
                Room = roomNumber,
                DateTime = DateTime.Now
            };
            return newEvent;
        }

        void server_MessageRecieved(object sender, ClientMsgEventArgs e)
        {


            //messagebox.show(
            //    string.format("lock: {0}\r\nsensor: {1}\r\naccess: {2}\r\nroom: {3}",
            //    locknumber, sensornumber, accesslevel, roomnumber));

        }
        #endregion


        private void InitRooms(int n)
        {
            for (int i = 0; i < n; i++)
            {
                var roomViewModel = new RoomViewModel();
                roomViewModel.SelectionChanged += roomViewModel_SelectionChanged;
                Rooms.Add(roomViewModel);
            }
        }

        void roomViewModel_SelectionChanged(object sender, EventArgs e)
        {
            if (!selectionLock)
            {
                selectionLock = true;
                var currentRoom = sender as RoomViewModel;
                SelectedEvent = currentRoom.SelectedEvent;
                //MessageBox.Show(SelectedEvent.ToString());
                foreach (var room in Rooms)
                {
                    if (room != currentRoom)
                    {
                        room.SelectedEvent = null;
                    }
                }
                selectionLock = false;
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
            int n = rd.Next(50, 1000);
            for (int j = 0; j < n; j++)
            {
                var newMsg = GenerateRandomMsg();
                Proceed(newMsg);
                var newEvent = Proceed(newMsg);
                newEvent.WarningLevel = ParseWarningLevel(newEvent);
                var roomViewModel = Rooms[newEvent.Room];
                var parameter = new object[] { roomViewModel, new EventViewModel(newEvent, visualFactory) };
                context.Send(AddEventInRoom, new MultiParameter(parameter));
                Thread.Sleep(rdG);
                //Thread.Sleep(50);
            }
        }

        private WarningLevels ParseWarningLevel(Event newEvent)
        {
            WarningLevels result = WarningLevels.Normal;
            var toss = rd.Next(201);

            if (toss == 200)
                result = WarningLevels.High;
            else if (toss >= 186)
                result = WarningLevels.Middle;
            //result = (WarningLevels)Enum.GetValues(typeof(WarningLevels)).GetValue(rd.Next(Enum.GetValues(typeof(WarningLevels)).GetLength(0)));
            return result;
        }

        private string GenerateRandomMsg()
        {
            var lockNumber = rd.Next(Rooms.Count);
            var sensorNumber = (int)Enum.GetValues(typeof(Sensors)).GetValue(rd.Next(Enum.GetValues(typeof(Sensors)).GetLength(0)));
            var accessLevel = 
                sensorNumber != 1 ? (int)Enum.GetValues(typeof(AccessLevels)).GetValue(rd.Next(Enum.GetValues(typeof(AccessLevels)).GetLength(0))) : 0;
            var cardNumber = rd.Next(Rooms.Count);
            return string.Format(@"msg/{0}/{1}/{2}/{3}/end",
                lockNumber, sensorNumber, accessLevel, cardNumber);
        }


        #endregion


    }
}
