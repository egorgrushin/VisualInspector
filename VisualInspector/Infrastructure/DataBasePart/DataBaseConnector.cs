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

        protected MySqlConnection connection;

        protected string host;
        protected string user;
        protected string password;
        protected string dataBaseName;

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
                var connectionString = string.Format("server={0}; uid={1}; pwd={2}; database={3};", 
                                                        host, user, password, dataBaseName);
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

       

        #region SqlCommands

		/// <summary>
		/// Parsing data for where condition. Not very comprehensive, for unusual queries should write where condition manually
		/// </summary>
		protected string ParseWhereData(AssociativeData data)
		{
            var whereArray = new List<string>();
			foreach(var pair in data)
            {
                var tryConvert = pair.Value as List<object>;
                if (tryConvert == null)
                {
                    tryConvert = new List<object>();
                    tryConvert.Add(pair.Value);
                }
                var whereString = string.Format("{0} IN ('{1}')", 
                                                    pair.Key, 
                                                    string.Join("', '", tryConvert));
                whereArray.Add(whereString);
			}
            return string.Join(" AND ", whereArray);
		}

        /// <summary>
        /// Insert data in table
        /// </summary>
        protected void Insert(string table, AssociativeData data)
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
        protected MySqlDataReader SelectAll(string table)
        {
            var sql = string.Format("SELECT * FROM {0}", table);

            var cmd = new MySqlCommand(sql, connection);
            return cmd.ExecuteReader();
        }

		/// <summary>
		/// Select all fields by where-data
		/// </summary>
		protected MySqlDataReader SelectAll(string table, AssociativeData where)
		{
			var whereString = ParseWhereData(where);
			var sql = string.Format("SELECT * FROM {0} WHERE {1}", table, whereString);

			var cmd = new MySqlCommand(sql, connection);
			return cmd.ExecuteReader();
		}
        /// <summary>
        /// Select all sorted fields from table 
        /// </summary>
        protected MySqlDataReader SelectAll(string table, string orderBy)
        {
            var sql = string.Format("SELECT * FROM {0} ORDER BY {1}", table, orderBy);
            var cmd = new MySqlCommand(sql, connection);
            return cmd.ExecuteReader();
        }
		/// <summary>
		/// Select all sorted fields by where-data 
		/// </summary>
		protected MySqlDataReader SelectAll(string table, AssociativeData where, string orderBy)
		{
			var whereString = ParseWhereData(where);
			var sql = string.Format("SELECT * FROM {0} WHERE {1} ORDER BY {2}", table, whereString, orderBy);
			var cmd = new MySqlCommand(sql, connection);
			return cmd.ExecuteReader();
		}

        /// <summary>
        /// Select field from table
        /// </summary>
		protected List<T> SelectOne<T>(string table, string field)
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
        protected List<T> SelectOne<T>(string table, string field, AssociativeData where)
        {
            var whereString = ParseWhereData(where);
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
		protected MySqlDataReader SelectMany(string table, List<string> fields)
		{
			var sql = string.Format("SELECT `{0}` FROM {1}", string.Join("`, `", fields), table);

			var cmd = new MySqlCommand(sql, connection);
			return cmd.ExecuteReader();
		}

		/// <summary>
		/// Select all exact fields from table by where-data
		/// </summary>
		protected MySqlDataReader SelectMany(string table, List<string> fields, AssociativeData where)
		{
			var whereString = ParseWhereData(where);
			var sql = string.Format("SELECT `{0}` FROM {1} WHERE {2}", string.Join("`, `", fields), table, whereString);

			var cmd = new MySqlCommand(sql, connection);
			return cmd.ExecuteReader();
		}

		/// <summary>
		/// Select all exact sorted fields from table by where-data
		/// </summary>
		protected MySqlDataReader SelectMany(string table, List<string> fields, AssociativeData where, string orderBy)
		{
			var whereString = ParseWhereData(where);
			var sql = string.Format("SELECT `{0}` FROM {1} WHERE {2} ORDER BY {3}", string.Join("`, `", fields), table, whereString, orderBy);

			var cmd = new MySqlCommand(sql, connection);
			return cmd.ExecuteReader();
		}

        #endregion
    }
}
