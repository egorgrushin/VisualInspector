using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data;
using MySql.Data.MySqlClient;
using VisualInspector.Models;
using NLog;

namespace VisualInspector.Infrastructure.DataBasePart
{
	public class DataBaseConnector
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private MySqlConnection connection;
		
		private string host;
		private string user;
		private string password;
		private string dataBaseName;

		public bool ShouldRead{ get; set; }
		public bool ShouldWrite{ get; set; }

		public DataBaseConnector()
		{
			host = "127.0.0.1";
			user = "VisualInspector";
			password = "EventsApp";
			dataBaseName = "visual_inspector_db";

			connection = new MySqlConnection();
		}

		public bool TryConnect()
		{
			try
			{
				var connectionString = "server=" + host + "; " + 
										"uid=" + user + "; " +
										"pwd=" + password + "; " +
										"database=" + dataBaseName + ";";
				connection.ConnectionString = connectionString;
				connection.Open();
				logger.Info("Connected to database {0} as {1}", dataBaseName, user);
			}
			catch(MySqlException e)
			{
				logger.Fatal("An error occured, while connecting to database: {0} Data would not be saved!", e.Message);
				return false;
			}

			return true;
		}

		public void WriteEventToDB(Event eventToSave)
		{
			if(connection.State != System.Data.ConnectionState.Open)
			{
				logger.Warn("Tried to write event {0} into {1} connection", eventToSave, connection.State);
				return;
			}
			if(ShouldWrite)
			{
				try
				{
					var roomNum = eventToSave.Room;
					var lockNum = eventToSave.Lock;
					var accesLevel = eventToSave.AccessLevel;
					var sensor = eventToSave.AccessLevel;
					var dateTime = eventToSave.DateTime;
					var warningLevel = (int)eventToSave.WarningLevel;

					var sql = "INSERT INTO `events`" + 
								"(`RoomNumber`, `Lock`, `AccessLevel`, `Sensor`, `Date`, `Time`, `WarningLevel`)" +
								" VALUES ('" + roomNum.ToString() + "', '" + 
								lockNum.ToString() + "', '" +
								accesLevel.ToString() + "', '" +
								sensor.ToString() + "', '" +
								string.Format("{0:yyyy-MM-dd}", dateTime) + "', '" +
								string.Format("{0:hh:mm:ss}", dateTime) + "', '" +
								warningLevel.ToString() + "')";

					var cmd = new MySqlCommand(sql, connection);
					cmd.ExecuteNonQuery();
				}
				catch (MySqlException e)
				{
					logger.Error("An error occured, while writing event to database: {0} {1}", e.Message, eventToSave);
				}
			}
		}

		public List<Event> ReadEventsByDate(DateTime date)
		{
			if(connection.State != System.Data.ConnectionState.Open)
			{
				logger.Warn("Tried to read events from {0} connection", connection.State);
				return null;
			}
			if(ShouldRead)
			{
				var result = new List<Event>();

				try
				{
					var sql = "SELECT `RoomNumber`, `Lock`, `AccessLevel`, `Sensor`, `Date`, `Time`, `WarningLevel` FROM `events`" +
						"WHERE Date = '" + string.Format("{0:yyyy-MM-dd}", date) + "'";

					var cmd = new MySqlCommand(sql, connection);
					var reader = cmd.ExecuteReader();
					while(reader.Read())
					{
						var dateParser = (DateTime)reader[4];
						var timeParser = (TimeSpan)reader[5];
						var dateTime = new DateTime(
							dateParser.Year,
							dateParser.Month,
							dateParser.Day,
							timeParser.Hours,
							timeParser.Minutes,
							timeParser.Seconds
						);

						var newEvent = new Event()
						{
							Room = (int)reader[0],
							Lock = (int)reader[1],
							AccessLevel = (int)reader[2],
							Sensor = (int)reader[3],
							DateTime = dateTime,
							WarningLevel = (WarningLevels)reader[6]
						};
						result.Add(newEvent);
					}
					reader.Close();
				}
				catch(MySqlException e)
				{
					logger.Error("An error occured, while reading events from database: {0}", e.Message);
					return null;
				}
				return result;
			}
			else
			{
				return null;
			}
		}
	}
}
