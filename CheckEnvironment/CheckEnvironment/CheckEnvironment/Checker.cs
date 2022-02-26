using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CheckEnvironment
{
    //@TODO revisar control de errores, incluyendo la razon que falle traer valores
    public class Checker
    {
        private static SqliteConnection _connection;

        public static SqliteConnection GetConnection()
        {
            if (_connection == null)
                StartConnection();
            return _connection;
        }

        private static void StartConnection()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            //_connection = new SqliteConnection("Data Source=InMemorySample;Mode=Memory;Cache=Shared");
            _connection.Open();
            var cmd = new SqliteCommand("create table event(created datetime, category text, value text)", _connection);
            cmd.ExecuteNonQuery();
        }

        public static void RegisterEvent(string category, object value, string source = null)
        {
            if (value == null)
                value = "value was null";
            else if (!(value is string))
                value = JsonConvert.SerializeObject(value);
            using var cmd = new SqliteCommand("insert into event values(CURRENT_TIMESTAMP, $category, $value)", GetConnection());
            cmd.Parameters.AddWithValue("category", category);
            cmd.Parameters.AddWithValue("value", value); 
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        public static List<Event> GetEventsByCategory(string category)
        {            
            string stm = "SELECT created, value from event where category = $category";
            try
            {
                using var cmd = new SqliteCommand(stm, GetConnection());
                cmd.Parameters.AddWithValue("category", category);
                cmd.Prepare();
                SqliteDataReader reader = cmd.ExecuteReader();
                List<Event> events = new List<Event>();
                while (reader.Read())
                {
                    Event @event = new Event();
                    @event.Category = category;
                    @event.Value = reader.GetString(1);
                    @event.Created = reader.GetDateTime(0);
                    events.Add(@event);
                }
                return events;
            }catch (Exception ex)
            {
                return new List<Event>() { new Event() { Category= "error GetEventsByCategory", Value = ex.Message, Created = DateTime.UtcNow } };
            }
        }
    }

    public class Event
    {
        public string Category { get; set; }
        public string Value { get; set; }
        public DateTime Created { get; set; }
    }
}
