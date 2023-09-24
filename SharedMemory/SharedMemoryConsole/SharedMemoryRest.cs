using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SharedMemoryConsole
{
    internal class SharedMemoryRest
    {
        private static string BaseUrl { get; } = "http://localhost:8040/sharedmemory";

        public static Dictionary<string, object> GetVars()
        {
            string json = GetWebServiceContent(BaseUrl + "/vars");
            var result = json==null? 
                new Dictionary<string, object>()
                : JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            return result;
        }

        public static void SetVars(Dictionary<string, object> vars)
        {
            string json = JsonConvert.SerializeObject(vars);
            PostWebServiceContent(BaseUrl + "/vars", json);
        }

        public static void ClearVars()
        {
            var vars = new Dictionary<string, object>();
            SetVars(vars);
        }

        public static void SetVar(string varName, object value)
        {
            var vars = GetVars();
            vars[varName] = value;
            SetVars(vars);           
        }

        public static object GetVar(string varName)
        {
            var vars = GetVars();
            object result;
            vars.TryGetValue(varName, out result);
            return result;
        }

        public static void WriteEvent(object value)
        {
            WriteEvent(new Event(null, null, DateTime.UtcNow, value));
        }
        
        public static void WriteEvent(string category, object value)
        {
            WriteEvent(new Event(category, null, DateTime.UtcNow, value));
        }
        public static void WriteEvent(string category, string source, object value)
        {
            WriteEvent(new Event(category, source, DateTime.UtcNow, value));
        }

        private static void WriteEvent(Event ev)
        {
            PostWebServiceContentAsync(BaseUrl +"/events", JsonConvert.SerializeObject(ev));            
        }

        private static string GetWebServiceContent(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            var response = (HttpWebResponse)request.GetResponse();
            var stream = response.GetResponseStream();
            var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();
            stream.Close();
            reader.Close();
            return content;
        }

        private static void PostWebServiceContent(string url, string bodyContent)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(bodyContent);
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = buffer.Length;
            var reqStream = request.GetRequestStream();
            reqStream.Write(buffer, 0, buffer.Length);
            reqStream.Close();
        }

        private static async Task<HttpResponseMessage> PostWebServiceContentAsync(string url, string bodyContent)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            var stringContent = new StringContent(bodyContent, Encoding.UTF8, "application/json");
            request.Content = stringContent;
            var response = await client.SendAsync(request);
            client.Dispose();
            return response;
        }

        internal class Event
        {
            public string Category { get; set; }
            public string Source { get; set; }
            public DateTime Created { get; set; }
            public object Value { get; set; }

            public Event(string category, string source, DateTime created, object value)
            {
                Category = category;
                Source = source;
                Created = created;
                Value = value;
            }
        }
    }
}
