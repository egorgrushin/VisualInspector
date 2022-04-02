using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data;
using MySql.Data.MySqlClient;
using VisualInspector.Models;
using NLog;

#region typedefs

using AssociativeData = System.Collections.Generic.Dictionary<string, object>;

#endregion

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

        public bool ShouldRead { get; set; }
        public bool ShouldWrite { get; set; }

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
            catch (MySqlException e)
            {
                logger.Fatal("An error occured, while connecting to database: {0} Data would not be saved!", e.Message);
                return false;
            }

            return true;
        }

        public void WriteEventToDB(Event eventToSave)
        {
            if (connection.State != System.Data.ConnectionState.Open)
            {
                logger.Warn("Tried to write event {0} into {1} connection", eventToSave, connection.State);
                return;
            }
            if (ShouldWrite)
            {
                try
                {
                    var roomNum = eventToSave.Room;
                    var lockNum = eventToSave.Lock;
                    var accessLevel = eventToSave.AccessLevel;
                    var sensor = eventToSave.AccessLevel;
                    var dateTime = eventToSave.DateTime;
                    var warningLevel = (int)eventToSave.WarningLevel;
                    var data = new AssociativeData()
                    {
                        {"RoomNumber", roomNum},
                        {"Lock", lockNum},
                        {"AccessLevel", accessLevel},
                        {"Sensor", sensor},
                        {"Date", string.Format("{0:yyyy-MM-dd}", dateTime)},
                        {"Time", string.Format("{0:hh:mm:ss}", dateTime)},
                        {"WarningLevel", warningLevel}
                    };
                    Insert("events", data);
                }
                catch (MySqlException e)
                {
                    logger.Error("An error occured, while writing event to database: {0} {1}", e.Message, eventToSave);
                }
            }
        }

        public List<Event> ReadEventsByDate(DateTime date)
        {
			return ReadEventsByDate(date, date);
        }

        public List<Event> ReadEventsByDate(DateTime dateBegin, DateTime dateEnd)
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
					var fieldsList = new List<string>()
					{
						"RoomNumber",
						"Lock",
						"AccessLevel",
						"Sensor",
						"Date",
						"Time",
						"WarningLevel"
					};
					var listOfDates = new List<object>();

					if(dateBegin.Date == dateEnd.Date)
					{
						listOfDates.Add(string.Format("{0:yyyy-MM-dd}", dateBegin));
					}
					else
					{
						var dateSpan = dateEnd.AddDays(1) - dateBegin;

						for(int i = 0; i < dateSpan.Days; i++)
						{
							listOfDates.Add(string.Format("{0:yyyy-MM-dd}", dateBegin.AddDays(i)));
						}
					}
					var where = new AssociativeData()
                    {
                        {"Date", listOfDates}
                    };

					var reader = SelectMany("events", fieldsList, where);
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

        #region SqlCommands

		/// <summary>
		/// Parsing data for where condition. Not very comprehensive, for unusual queries should write where condition manually
		/// </summary>
		private string parseWhereData(AssociativeData data)
		{
			var whereString = "";
			var firstTime = true;
			foreach(var pair in data)
			{
				if(!firstTime)
				{
					whereString += " AND ";
				}
				firstTime = false;
				whereString += string.Format("{0} IN ('", pair.Key);
				var tryConvert = pair.Value as List<object>;
				if(tryConvert == null)
				{
					tryConvert = new List<object>();
					tryConvert.Add(pair.Value.ToString());
				}
				whereString += string.Join("', '", tryConvert);
				whereString += "')";
			}
			return whereString;
		}

        /// <summary>
        /// Insert data in table
        /// </summary>
        private void Insert(string table, AssociativeData data)
        {
            var fieldNames = string.Format("`{0}`", string.Join("`, `", data.Keys));
            var fieldValues = string.Format("'{0}'", string.Join("', '", data.Values));
            var sql = string.Format("INSERT INTO {0} ({1}) VALUES ({2})", table, fieldNames, fieldValues);
            var cmd = new MySqlCommand(sql, connection);
            cmd.ExecuteNonQuery();
        }
        /// <summary>
        /// Select all fields from table
        /// </summary>
        private MySqlDataReader SelectAll(string table)
        {
            var sql = string.Format("SELECT * FROM {0}", table);

            var cmd = new MySqlCommand(sql, connection);
            return cmd.ExecuteReader();
        }

		/// <summary>
		/// Select all fields by where-data
		/// </summary>
		private MySqlDataReader SelectAll(string table, AssociativeData where)
		{
			var whereString = parseWhereData(where);
			var sql = string.Format("SELECT * FROM {0} WHERE {1}", table, whereString);

			var cmd = new MySqlCommand(sql, connection);
			return cmd.ExecuteReader();
		}
        /// <summary>
        /// Select all sorted fields from table 
        /// </summary>
        private MySqlDataReader SelectAll(string table, string orderBy)
        {
            var sql = string.Format("SELECT * FROM {0} ORDER BY {1}", table, orderBy);

            var cmd = new MySqlCommand(sql, connection);
            return cmd.ExecuteReader();
        }
		/// <summary>
		/// Select all sorted fields by where-data 
		/// </summary>
		private MySqlDataReader SelectAll(string table, AssociativeData where, string orderBy)
		{
			var whereString = parseWhereData(where);
			var sql = string.Format("SELECT * FROM {0} WHERE {1} ORDER BY {2}", table, whereString, orderBy);

			var cmd = new MySqlCommand(sql, connection);
			return cmd.ExecuteReader();
		}

        /// <summary>
        /// Select field from table
        /// </summary>
		private List<T> SelectOne<T>(string table, string field)
        {
            var sql = string.Format("SELECT {0} FROM {1}", field, table);
			var cmd = new MySqlCommand(sql, connection);
			var reader = cmd.ExecuteReader();
			var result = new List<T>();
			while(reader.Read())
			{
				result.Add((T)reader[0]);
			}
			return result;
        }
        /// <summary>
        /// Select field by where-data
        /// </summary>
        private List<T> SelectOne<T>(string table, string field, AssociativeData where)
        {
            var whereString = parseWhereData(where);
            var sql = string.Format("SELECT {0} FROM {1} WHERE {2}", field, table, whereString);
            var cmd = new MySqlCommand(sql, connection);
			var reader = cmd.ExecuteReader();
			var result = new List<T>();
			while(reader.Read())
			{
				result.Add((T)reader[0]);
			}
            return result;
        }

		/// <summary>
		/// Select all exact fields from table
		/// </summary>
		private MySqlDataReader SelectMany(string table, List<string> fields)
		{
			var sql = string.Format("SELECT `{0}` FROM {1}", string.Join("`, `", fields), table);

			var cmd = new MySqlCommand(sql, connection);
			return cmd.ExecuteReader();
		}

		/// <summary>
		/// Select all exact fields from table by where-data
		/// </summary>
		private MySqlDataReader SelectMany(string table, List<string> fields, AssociativeData where)
		{
			var whereString = parseWhereData(where);
			var sql = string.Format("SELECT `{0}` FROM {1} WHERE {2}", string.Join("`, `", fields), table, whereString);

			var cmd = new MySqlCommand(sql, connection);
			return cmd.ExecuteReader();
		}

		/// <summary>
		/// Select all exact sorted fields from table by where-data
		/// </summary>
		private MySqlDataReader SelectMany(string table, List<string> fields, AssociativeData where, string orderBy)
		{
			var whereString = parseWhereData(where);
			var sql = string.Format("SELECT `{0}` FROM {1} WHERE {2} ORDER BY {3}", string.Join("`, `", fields), table, whereString, orderBy);

			var cmd = new MySqlCommand(sql, connection);
			return cmd.ExecuteReader();
		}
        #endregion
    }
}
