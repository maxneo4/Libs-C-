using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SharedMemoryConsole
{
    internal class SharedMemoryRest
    {
        private static string BaseUrl { get; } = "https://sharedmemory.azurewebsites.net/sharedmemory";

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
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);
            var content = client.GetAsync("").Result;            
            return content.Content.ReadAsStringAsync().Result;
        }

        private static void PostWebServiceContent(string url, string bodyContent)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);
            StringContent content = new StringContent(bodyContent, Encoding.UTF8, "application/json");
            var r = client.PostAsync("", content).Result;    
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
