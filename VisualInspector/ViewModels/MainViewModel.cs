using Foundation;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using System.ComponentModel;
using VisualInspector.Infrastructure.DataBasePart;
using System.Windows.Controls;

namespace VisualInspector.ViewModels
{
	public enum Sensors
	{
		Outside,
		Inside
	}
	public enum AccessLevels
	{
		Without,
		Guest,
		Staff,
		Administator
	}

	public class MainViewModel : ViewModel
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private static readonly int RoomsNumber = 30;

		#region Properties & fields
		Random rd;
		private IVisualFactory<EventViewModel> visualFactory;
		private TcpServer server;
		private DataBaseConnector dataBaseConnection;
		private bool selectionLock;
		private RelayCommand showVideoCommand;
		//Worker that was started by previous selection. Should be canceled to avoid massive SelectedFrameList assignment calls
		private BackgroundWorker currentWorker;

		public ObservableNotifiableCollection<RoomViewModel> Rooms
		{
			get	{ return Get(() => Rooms); }
			set	{ Set(() => Rooms, value); }
		}

		public EventViewModel SelectedEvent
		{
			get	{ return Get(() => SelectedEvent); }
			set
			{
				if(SelectedEvent != value)
				{
					SelectedFrameList = null;
				}
				Set(() => SelectedEvent, value);
				if(SelectedEvent == null)
				{
					SelectedFrameList = null;
				}
				else
				{
					if(currentWorker != null)
					{
						currentWorker.CancelAsync();
					}
					var worker = new BackgroundWorker();
					worker.DoWork += delegate(object sender, DoWorkEventArgs e)
					{
						e.Result = SelectedEvent.InitFramesList(worker, e);
					};
					worker.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs e)
					{
						if(!e.Cancelled && e.Error == null && e.Result != null)
						{
							SelectedFrameList = e.Result as List<BitmapImage>;
						}
					};
					worker.WorkerSupportsCancellation = true;
					worker.RunWorkerAsync();
					currentWorker = worker;
				}
			}
		}
		public List<BitmapImage> SelectedFrameList
		{
			get	{ return Get(() => SelectedFrameList); }
			set	{ Set(() => SelectedFrameList, value); }
		}
		
		public DateTime SelectedDateBegin
		{
			get	{ return Get(() => SelectedDateBegin); }
			set	
			{ 
				Set(() => SelectedDateBegin, value); 
				ReloadEventsByDate();
			}
		}
		public DateTime SelectedDateEnd
		{
			get	{ return Get(() => SelectedDateEnd); }
			set	
			{
				Set(() => SelectedDateEnd, value);
				ReloadEventsByDate(); 
			}
		}

		#endregion

		#region Constructors
		public MainViewModel()
		{
			rd = new Random();
			var pens = new Dictionary<string, Pen>() { { "Black", new Pen(Brushes.Black, 2) } };
			var brushes = new Dictionary<string, Brush>()
            { 
                { "Normal", App.Current.Resources["forNormalBrush"] as SolidColorBrush},
                { "Middle", App.Current.Resources["forMiddleBrush"] as SolidColorBrush},
                { "High",  App.Current.Resources["forHighBrush"] as SolidColorBrush}
            };
			visualFactory = new EventVisualFactory(pens, brushes);

			dataBaseConnection = new DataBaseConnector()
			{
				ShouldRead = true,
				ShouldWrite = false
			};
			dataBaseConnection.TryConnect();

			InitRooms();
			SelectedEvent = null;
			SelectedDateBegin = DateTime.Now;
			SelectedDateEnd = DateTime.Now;

			//var thread = new Thread(FillRooms);
			//thread.IsBackground = true;
			//var context = SynchronizationContext.Current;
			//thread.Start(context);
		}
		#endregion

		#region Actions with events
		/// <summary>
		/// Adding event only as a visual, don't save it. Should be used for reloading events from database
		/// </summary>
		private void AddEventToVisualHost(Event eventToAdd)
		{
			Rooms[eventToAdd.Room].Events.Add(new EventViewModel(eventToAdd, visualFactory));
		}
		/// <summary>
		/// Adding event to the application, saving all data to database. Should be used for new events from clients
		/// </summary>
		private void AddEventToApp(Event eventToAdd)
		{
			AddEventToVisualHost(eventToAdd);
			dataBaseConnection.WriteEventToDB(eventToAdd);
		}

		public void ReloadEventsByDate()
		{
			InitRooms();
			var eventsList = dataBaseConnection.ReadEventsByDate(SelectedDateBegin, SelectedDateEnd);
			if(eventsList != null)
			{
				foreach(var ev in eventsList)
				{
					AddEventToVisualHost(ev);
				}
			}
		}
		#endregion

		#region Commands 
		public ICommand ShowVideoCommand
		{
			get
			{
				if(showVideoCommand == null)
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
			Rooms = new ObservableNotifiableCollection<RoomViewModel>();
			for(int i = 0; i < n; i++)
			{
				var roomViewModel = new RoomViewModel(i + 1);
				roomViewModel.SelectionChanged += roomViewModel_SelectionChanged;
				Rooms.Add(roomViewModel);
			}
		}

		private void InitRooms()
		{
			InitRooms(RoomsNumber);
		}

		void roomViewModel_SelectionChanged(object sender, EventArgs e)
		{
			if(!selectionLock)
			{
				selectionLock = true;
				var currentRoom = sender as RoomViewModel;
				SelectedEvent = currentRoom.SelectedEvent;
				foreach(var room in Rooms)
				{
					if(room != currentRoom)
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
				newEvent.ParseWarningLevel(rd.Next(201));
				context.Send(delegate
					{
						AddEventToApp(newEvent);
					}, null);

				Thread.Sleep(randomSleep);
			}
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
