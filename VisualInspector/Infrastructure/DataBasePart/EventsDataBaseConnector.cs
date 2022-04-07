using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VisualInspector.Models;

#region typedefs

using AssociativeData = System.Collections.Generic.Dictionary<string, object>;
using MySql.Data.MySqlClient;

#endregion
namespace VisualInspector.Infrastructure.DataBasePart
{
    public class EventsDataBaseConnector : DataBaseConnector
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        
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
                    var lockNum = eventToSave.LockID;
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

                    var dateSpan = dateEnd.AddDays(1) - dateBegin;

                    for (int i = 0; i < dateSpan.Days; i++)
                    {
                        listOfDates.Add(string.Format("{0:yyyy-MM-dd}", dateBegin.AddDays(i)));
                    }

                    var where = new AssociativeData()
                    {
                        {"Date", listOfDates}
                    };

                    var reader = SelectMany("events", fieldsList, where);
                    while (reader.Read())
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
                            LockID = (string)reader[1],
                            AccessLevel = (int)reader[2],
                            Sensor = (int)reader[3],
                            DateTime = dateTime,
                            WarningLevel = (WarningLevels)reader[6]
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
    }
}
