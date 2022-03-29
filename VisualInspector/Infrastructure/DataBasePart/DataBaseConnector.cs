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
            if (connection.State != System.Data.ConnectionState.Open)
            {
                logger.Warn("Tried to read events from {0} connection", connection.State);
                return null;
            }
            if (ShouldRead)
            {
                var result = new List<Event>();

                try
                {
                    //var sql = "SELECT `RoomNumber`, `Lock`, `AccessLevel`, `Sensor`, `Date`, `Time`, `WarningLevel` FROM `events`" +
                    //    "WHERE Date = '" + string.Format("{0:yyyy-MM-dd}", date) + "'";

                    var where = new AssociativeData()
                    {
                        {"Date", string.Format("{0:yyyy-MM-dd}", date) }
                    };

                    var reader = Select("events", where);
                    while (reader.Read())
                    {
                        var dateParser = (DateTime)reader[5];
                        var timeParser = (TimeSpan)reader[6];
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
                            Room = (int)reader[1],
                            Lock = (int)reader[2],
                            AccessLevel = (int)reader[3],
                            Sensor = (int)reader[4],
                            DateTime = dateTime,
                            WarningLevel = (WarningLevels)reader[7]
                        };
                        result.Add(newEvent);
                    }
                    reader.Close();
                }
                catch (MySqlException e)
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

        public List<Event> ReadEventsByDate(DateTime dateBegin, DateTime dateEnd)
        {
            if (dateBegin.Date == dateEnd.Date)
            {
                return ReadEventsByDate(dateBegin);
            }

            var dateSpan = dateEnd.AddDays(1) - dateBegin;
            var result = new List<Event>();

            for (int i = 0; i < dateSpan.Days; i++)
            {
                var oneDayEvents = ReadEventsByDate(dateBegin.AddDays(i));
                if (oneDayEvents != null)
                {
                    result.AddRange(oneDayEvents);
                }
            }

            return result;
        }



        #region SqlCommands

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
        private MySqlDataReader Select(string table)
        {
            var sql = string.Format("SELECT * FROM {0}", table);
            var cmd = new MySqlCommand(sql, connection);
            return cmd.ExecuteReader();
        }
        /// <summary>
        /// Select all sorted fields from table 
        /// </summary>
        private MySqlDataReader SelectBy(string table, string orderBy)
        {
            var sql = string.Format("SELECT * FROM {0} ORDER BY {1}", table, orderBy);
            var cmd = new MySqlCommand(sql, connection);
            return cmd.ExecuteReader();
        }
        /// <summary>
        /// Select field from table
        /// </summary>
        private MySqlDataReader Select(string table, string field)
        {
            var sql = string.Format("SELECT {0} FROM {1}", field, table);
            var cmd = new MySqlCommand(sql, connection);
            return cmd.ExecuteReader();
        }

        /// <summary>
        /// Select all fields by where-data
        /// </summary>
        private MySqlDataReader Select(string table, AssociativeData where)
        {
            var whereString = "";
            foreach (var pair in where)
            {
                whereString += string.Format("{0} = '{1}'", pair.Key, pair.Value);
            }
            var sql = string.Format("SELECT * FROM {0} WHERE {1}", table, whereString);
            var cmd = new MySqlCommand(sql, connection);
            return cmd.ExecuteReader();
        }
        /// <summary>
        /// Select all sorted fields by where-data 
        /// </summary>
        private MySqlDataReader SelectBy(string table, AssociativeData where, string orderBy)
        {
            var whereString = "";
            foreach (var pair in where)
            {
                whereString += string.Format("{0} = '{1}'", pair.Key, pair.Value);
            }
            var sql = string.Format("SELECT * FROM {0} WHERE {1} ORDER BY {2}", table, whereString, orderBy);
            var cmd = new MySqlCommand(sql, connection);
            return cmd.ExecuteReader();
        }
        /// <summary>
        /// Select field by where-data
        /// </summary>
        private MySqlDataReader Select(string table, string field, AssociativeData where)
        {
            var whereString = "";
            foreach (var pair in where)
            {
                whereString += string.Format("{0} = '{1}'", pair.Key, pair.Value);
            }
            var sql = string.Format("SELECT {0} FROM {1} WHERE {2}", field, table, whereString);
            var cmd = new MySqlCommand(sql, connection);
            return cmd.ExecuteReader();
        }
        #endregion



    }
}
