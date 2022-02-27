using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace CheckEnvironment
{   
    //@TODO
    //Desactivar / activarlo [OK]
    //Agreagar metodo de limpieza de valores [OK]
    //Verificar cuantos megas ocupa en memoria la tabla de eventos [OK]
    //Agregar source como dato opcional [OK]
    //no repetir valores (tabla valor, tabla instancia de valor...)
    //Metodo para recibir consultas
    //Formatear/formalizar mejor valores de fecha al ser visibles para analisis
    //Conocer cuantos bytes se han guardado y poder controlar el limite maximo de bytes en memoria cuando se prende la feature
    //Poder poner en modo persistencia (validar si en webapp funciona por defecto) Util para onpremise...
    //Proteger contra hilos algunas propiedades sensibles
    public class Checker
    {
        private static SQLiteConnection _connection;
        private static bool _enabled = false;

        public static bool Enabled { get { return _enabled; } set { _enabled = value; } }
        public static EnabledState UpdateEnabled(bool newValue)
        {
            EnabledState result = new EnabledState() { OldState = Enabled };
            Enabled = newValue;
            result.NewState = newValue;
            return result;
        }
        public static SQLiteConnection GetConnection()
        {
            if (_connection == null)
                StartConnection();
            return _connection;
        }

        private static void StartConnection()
        {
            _connection = new SQLiteConnection("Data Source=:memory:");
            //_connection = new SqliteConnection("Data Source=InMemorySample;Mode=Memory;Cache=Shared");
            _connection.Open();
            var cmd = new SQLiteCommand("create table event(created datetime, category varchar(100), source varchar(200), value text)", _connection);
            cmd.ExecuteNonQuery();
        }
               

        public static void RegisterEvent(string category, object value, string source = null)
        {
            if (!Enabled)
                return;
            try
            {
                value = GetTextValue(value);
                var cmd = new SQLiteCommand("insert into event values(CURRENT_TIMESTAMP, $category, $source, $value)", GetConnection());
                cmd.Parameters.AddWithValue("category", category);
                cmd.Parameters.AddWithValue("source", source);
                cmd.Parameters.AddWithValue("value", value);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
            }
            catch { }
        }

        public static int CleanAllEvents()
        {            
            try
            {               
                var cmd = new SQLiteCommand("delete from event", GetConnection());               
                return cmd.ExecuteNonQuery();
            }
            catch { return -1; }
        }


        public static List<Event> GetEventsByCategory(string category)
        {            
            string stm = "SELECT created, category, value, source from event where category = $category";
            try
            {
                var cmd = new SQLiteCommand(stm, GetConnection());
                cmd.Parameters.AddWithValue("category", category);
                cmd.Prepare();
                SQLiteDataReader reader = cmd.ExecuteReader();
                List<Event> events = new List<Event>();
                while (reader.Read())
                {
                    Event @event = new Event();
                    @event.Created = IsDbNull(reader, 0)? DateTime.MinValue : reader.GetDateTime(0);
                    @event.Category = IsDbNull(reader, 1)? null: reader.GetString(1);
                    @event.Value = IsDbNull(reader, 2) ? null : reader.GetString(2);
                    @event.Source = IsDbNull(reader, 3) ? null : reader.GetString(3);
                    events.Add(@event);
                }
                return events;
            }catch (Exception ex)
            {
                return new List<Event>() { new Event() { Category= "error GetEventsByCategory", Value = GetTextValue(ex), Created = DateTime.UtcNow } };
            }
        }

        private static bool IsDbNull(SQLiteDataReader reader, int ordinal)
        {
            return reader[ordinal] == DBNull.Value;
        }

        public static List<Event> GetEventsByQueryCondition(string condition)
        {
            string stm = $"SELECT created, category, value, source from event where {condition}";
            try
            {
                var cmd = new SQLiteCommand(stm, GetConnection());                
                SQLiteDataReader reader = cmd.ExecuteReader();
                List<Event> events = new List<Event>();
                while (reader.Read())
                {
                    Event @event = new Event();
                    @event.Created = reader.GetDateTime(0);
                    @event.Category = reader.GetString(1);
                    @event.Value = reader.GetString(2);
                    @event.Source = reader.GetString(3);
                    events.Add(@event);
                }
                return events;
            }
            catch (Exception ex)
            {
                return new List<Event>() { new Event() {
                    Category = "error GetEventsByQueryCondition",
                    Value = GetTextValue(ex),
                    Created = DateTime.UtcNow,
                    Source = "Checker"
                } };
            }
        }

        public static double GetMBSizeInMemory()
        {            
            string stm = "SELECT SUM(\"pgsize\") FROM \"dbstat\" WHERE name='event'";
            try
            {
                var cmd = new SQLiteCommand(stm, GetConnection());                
                SQLiteDataReader reader = cmd.ExecuteReader();                
                double result = -1;
                while (reader.Read())
                {
                    result = reader.GetDouble(0);
                }
                return result/(1024*1204);
            }catch 
            {
                return -1;
            }
        }

        private static string GetTextValue(object obj)
        {
            if (obj == null)
                return "value was null";
            else if (obj is Exception)
                return FormatException(obj);
            else if (!(obj is string))
                return JsonConvert.SerializeObject(obj);
            else
                return obj.ToString();
        }

        private static string FormatException(object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.None, new JsonSerializerSettings
            {
                ContractResolver = new JsonIgnoreResolver("TargetSite")//because sqliteException has big and cycle information with this property...
            });
        }
    }

    public class Event
    {
        public string Category { get; set; }
        public string Value { get; set; }
        public string Source { get; set; }
        public DateTime Created { get; set; }
    }

    public class EnabledState
    {
        public bool OldState { get; set; }
        public bool NewState { get; set; }
    }
}