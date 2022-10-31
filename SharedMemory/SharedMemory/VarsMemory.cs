using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Text;

namespace SharedMemory
{
    public class VarsMemory
    {
        private const string VARS_MAP_MEMORY = "VARS_MAP_MEMORY";
        private const string VARS_LENGTH = "VarsLength";
        private MemoryMappedViewAccessor _memoryVarsViewAccesor;
        private int _kbSize=1;

        private Dictionary<string, object> Vars { get; set; }

        public VarsMemory()
        {
            _memoryVarsViewAccesor = SharedMemory.CreateMapFileAndAccesorByKb(VARS_MAP_MEMORY, _kbSize);
            Vars = new Dictionary<string, object>() { { VARS_LENGTH, 0 } };
            WriteVars();
        }

        public void SetVar(string varName, object value)
        {
            Vars = GetVars();
            Vars[varName] = value;
            Vars[VARS_LENGTH] = Vars.Keys.Count-1;
            WriteVars();
        }

        public object GetVar(string varName)
        {
            Vars = GetVars();
            object result;
            Vars.TryGetValue(varName, out result);
            return result;
        }

        public Dictionary<string, object> GetVars()
        {
            int bytesSize = _kbSize * 1024;
            byte[] buffer = new byte[bytesSize];
            _memoryVarsViewAccesor.ReadArray(0, buffer, 0, bytesSize);
            string json = Encoding.UTF8.GetString(buffer);
            Vars = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            return Vars;
        }

        private void WriteVars()
        {
            int bytesSize = _kbSize * 1024;
            string input = JsonConvert.SerializeObject(Vars);
            byte[] data = Encoding.UTF8.GetBytes(input);            
            _memoryVarsViewAccesor.WriteArray(0, new byte[bytesSize], 0, bytesSize);//clear data
            if (data.Length <= (_kbSize * 1024))
            {
                _memoryVarsViewAccesor.WriteArray(0, data, 0, data.Length);
            }
            else
            {
                Vars = new Dictionary<string, object>() { { "error", $"Data too long, maximium allowed {(_kbSize * 1024)} bytes but was {data.Length}"   } };
                WriteVars();
            }
        }
    }
}
