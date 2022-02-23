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
using System.Windows.Media.Imaging;
using VisualInspector.Views;
using NLog;

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
		private static Logger logger = LogManager.GetCurrentClassLogger();

        #region Properties & fields
        Random rd;
        private IVisualFactory<EventViewModel> visualFactory;
        private TcpServer server;
        private bool selectionLock;
        private RelayCommand showVideoCommand;
        public IEnumerable<string> EnumCol { get; set; }

        public ObservableNotifiableCollection<RoomViewModel> Rooms
        {
            get { return Get(() => Rooms); }
            set { Set(() => Rooms, value); }
        }
        
        public EventViewModel SelectedEvent
        {
            get { return Get(() => SelectedEvent); }
            set { 
					Set(() => SelectedEvent, value);
                    if (SelectedEvent == null)
                        SelectedFrameList = null;
                    else
                    {
                        InitFrameList();
                    }
				}
        }

        private void InitFrameList()
        {
            var framesList = new List<BitmapImage>();
            new Thread(delegate(object param)
                {
                    var context = param as SynchronizationContext;
                    SelectedEvent.InitFramesList(framesList);
                    context.Send(delegate
                    {
                        SelectedFrameList = framesList;
                    }, null);
                }).Start(SynchronizationContext.Current);
        }

		public List<BitmapImage> SelectedFrameList
		{
            get { return Get(() => SelectedFrameList); }
            set { Set(() => SelectedFrameList, value); }
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

			InitRooms(2);

			var thread = new Thread(FillRooms);
			thread.IsBackground = true;
			var context = SynchronizationContext.Current;
			thread.Start(context);
        }
        #endregion

        public ICommand ShowVideoCommand
        {
            get
            {
                if (showVideoCommand == null)
                    showVideoCommand = new RelayCommand(
                        (param) => ShowVideo(),
                        (param) => SelectedEvent != null);
                return showVideoCommand;
            }
        }

        private void ShowVideo()
        {
            var videoForm = new VideoPlayerWindow();
            videoForm.DataContext = SelectedEvent;
            videoForm.ShowDialog();
        }

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
			var fileName = "test.mp4";
            var newEvent = new Event()
            {
                Lock = lockNumber,
                Sensor = sensorNumber,
                AccessLevel = accessLevel,
                Room = roomNumber,
                DateTime = DateTime.Now,
                VideoFileName = fileName
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
                var roomViewModel = new RoomViewModel(i + 1);
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

        #region Life emulation

        private void FillRooms(object state)
		{
			var context = state as SynchronizationContext;
			while(true)
			{
				var randomSleep = rd.Next(1, 30);
				var newMsg = GenerateRandomMsg();


				var newEvent = Proceed(newMsg);

				newEvent.WarningLevel = ParseWarningLevel(newEvent);
				context.Send(delegate 
					{
                        Rooms[newEvent.Room].Events.Add(new EventViewModel(newEvent, visualFactory));
					}, null);

				Thread.Sleep(randomSleep);
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
