using Newtonsoft.Json;
using System;
using System.IO.MemoryMappedFiles;
using System.Text;

namespace SharedMemory
{
    public class EventMemory
    {
        private const string EVENT_MAP_MEMORY = "EVENT_MAP_MEMORY";
        private const string EVENT_HEADER_MEMORY = "EVENT_HEADER_MEMORY";
        private MemoryMappedViewAccessor _memoryMappedViewAccessor;
        private MemoryMappedViewAccessor _memoryHeaderViewAccessor;
        private static EventMemory _singleton;
                   
        //***
        //Validate hash of event, if exists increment count.. only
        /*
         * 
         */               
        private int _currentPage;
        private HeaderMemory _headerMemory;
        public EventMemory(int maximiumMb)
        {
            _memoryMappedViewAccessor = SharedMemory.CreateMapFileAndAccesor(EVENT_MAP_MEMORY, maximiumMb);
            //_memoryMappedViewAccessor = SharedMemory.CreateMapFileAndAccesor(EVENT_MAP_MEMORY, 100L);

            _memoryHeaderViewAccessor = SharedMemory.CreateMapFileAndAccesor(EVENT_HEADER_MEMORY, 1L);//1kb size
            _headerMemory = new HeaderMemory();
            _headerMemory.FirstPage = 1;
            _headerMemory.Page1.StartIndex = 0;
            _headerMemory.Page1.EndIndex = 0;
            _headerMemory.Page2.StartIndex = _memoryMappedViewAccessor.Capacity / 2;
            _headerMemory.Page2.EndIndex = _headerMemory.Page2.StartIndex;
            _headerMemory.Page1.Size = _headerMemory.Page2.StartIndex;
            _headerMemory.Page2.Size = _memoryMappedViewAccessor.Capacity - _headerMemory.Page1.Size;
            _headerMemory.Page1.MaxPosition = _headerMemory.Page1.Size - 1;
            _headerMemory.Page2.MaxPosition = _memoryMappedViewAccessor.Capacity - 1;
            _currentPage = 1;
            _headerMemory.Behaviour.WorkingSince = DateTime.UtcNow;
        }

        public static EventMemory GetSingletonInstace(int maximiumMb)
        {
            if (_singleton == null)
                _singleton = new EventMemory(maximiumMb);
            return _singleton;
        }

        private void WriteString(string input)
        {
            byte[] data = Encoding.UTF8.GetBytes(input);
            long dataLenght = data.Length;
            if(data.Length > _headerMemory.Page1.Size && data.Length > _headerMemory.Page2.Size)
            {
                dataLenght = _headerMemory.Page1.Size - 1; //odd possibly, truncated data to write chaging dataLenght to write in buffer
                _headerMemory.Behaviour.LastUnexcpected = $"Data lenght {data.Length} than max page size {_headerMemory.Page1.Size}";
            }
            ValidateEnoughSpaceInCurrentPageAndSwap(dataLenght);
            long startIndex = GetCurrentPosByte();
            _memoryMappedViewAccessor.WriteArray(startIndex, data, 0, data.Length);
            IncrementCurrentPosByte(dataLenght);            
        }

        private void WriteHeader()
        {
            string input = JsonConvert.SerializeObject(_headerMemory) + ":/:";
            byte[] data = Encoding.UTF8.GetBytes(input);
            _memoryHeaderViewAccessor.WriteArray(0, new byte[1024], 0, 1024);//clear data
            _memoryHeaderViewAccessor.WriteArray(0, data, 0, data.Length);
        }

        private long GetCurrentPosByte()
        {
            return _currentPage==1? _headerMemory.Page1.EndIndex : _headerMemory.Page2.EndIndex;
        }

        private void IncrementCurrentPosByte(long newDataLenght)
        {
            if (_currentPage == 1)
                _headerMemory.Page1.EndIndex += newDataLenght;
            else
                _headerMemory.Page2.EndIndex += newDataLenght;
        }

        private void ValidateEnoughSpaceInCurrentPageAndSwap(long length)
        {
            Page currentPage, nextPage;
            if(_currentPage == 1)
            {
                currentPage = _headerMemory.Page1;
                nextPage = _headerMemory.Page2;    
            }
            else
            {
                currentPage = _headerMemory.Page2;
                nextPage = _headerMemory.Page1;    
            }            
            if(currentPage.EndIndex + length > currentPage.MaxPosition)
            {
                _currentPage = nextPage.Number;
                nextPage.EndIndex = nextPage.StartIndex;
                _headerMemory.FirstPage = currentPage.Number;
                _headerMemory.Behaviour.CountSwapPages++;
            }            
        }

        public void WriteEvent(string category, string source, object value)
        {
            DateTime created = DateTime.UtcNow;

            string dateTimeUniversalUC = created.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
            string input = $"{category}:/:{source}:/:{dateTimeUniversalUC}:/:{value}:/:";
            WriteString(input);
            //category::bizagi:/:2022-04-14T01:56:32.044Z:/:Un valor pequeñop:\:
            //@TODO poner campo para veces que se repite... 0001, soportando 10 mil repeticiones...
            _headerMemory.Behaviour.CountRegisteredEvents++;
            WriteHeader();
        }

        public void WriteEvent(string category, object value)
        {
            WriteEvent(category, "defaultSource", value);
        }

        public void WriteEvent(object value)
        {
            WriteEvent("defaultCategory", value?.ToString());
        }
    }
}
