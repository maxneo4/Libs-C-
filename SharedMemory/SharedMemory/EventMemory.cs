using Newtonsoft.Json;
using System;
using System.IO.MemoryMappedFiles;
using System.Text;

namespace SharedMemory
{
    class EventMemory
    {
        private const string EVENT_MAP_MEMORY = "EVENT_MAP_MEMORY";
        private const string EVENT_HEADER_MEMORY = "EVENT_HEADER_MEMORY";
        private MemoryMappedViewAccessor _memoryMappedViewAccessor;
        private MemoryMappedViewAccessor _memoryHeaderViewAccessor;

        //calculate pages, positions
        //define headers info of events
        //include reserved space for headers
        //define current page
        //make swap method
        //*** Validate hash of event, if exists increment count.. only
        /*
         * Add events in other space with strange events as size of value greater than size of one page... etc
         */

        private long _maxPositionPage1, _maxPositionPage2;
        private int _currentPage;
        private HeaderMemory _headerMemory;
        public EventMemory(int maximiumMb)
        {
            //_memoryMappedViewAccessor = SharedMemory.CreateMapFileAndAccesor(EVENT_MAP_MEMORY, maximiumMb);
            _memoryMappedViewAccessor = SharedMemory.CreateMapFileAndAccesor(EVENT_MAP_MEMORY, 10L);

            _memoryHeaderViewAccessor = SharedMemory.CreateMapFileAndAccesor(EVENT_HEADER_MEMORY, 1L);//1kb size
            _headerMemory = new HeaderMemory();
            _headerMemory.FirstPage = 1;
            _headerMemory.Page1StartIndex = 0;
            _headerMemory.Page1EndIndex = 0;
            _headerMemory.Page2StartIndex = _memoryMappedViewAccessor.Capacity / 2;
            _headerMemory.Page2EndIndex = _headerMemory.Page2StartIndex;
            _headerMemory.Page1Size = _headerMemory.Page2StartIndex;
            _headerMemory.Page2Size = _memoryMappedViewAccessor.Capacity - _headerMemory.Page1Size;
            _maxPositionPage1 = _headerMemory.Page1Size - 1;
            _maxPositionPage2 = _memoryMappedViewAccessor.Capacity - 1;
            _currentPage = 1;
        }

        public void WriteString(string input)
        {
            byte[] data = Encoding.UTF8.GetBytes(input);
            long dataLenght = data.Length;
            if(data.Length > _headerMemory.Page1Size && data.Length > _headerMemory.Page2Size)
            {
                dataLenght = _headerMemory.Page1Size - 1; //odd possibly, truncated data to write
                //Write in own events memory space of lib
            }
            ValidateEnoughSpaceInCurrentPage(dataLenght);
            long startIndex = GetCurrentPosByte();
            _memoryMappedViewAccessor.WriteArray(startIndex, data, 0, data.Length);
            IncrementCurrentPosByte(dataLenght);
            WriteHeader();
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
            return _currentPage==1? _headerMemory.Page1EndIndex : _headerMemory.Page2EndIndex;
        }

        private void IncrementCurrentPosByte(long newDataLenght)
        {
            if (_currentPage == 1)
                _headerMemory.Page1EndIndex += newDataLenght;
            else
                _headerMemory.Page2EndIndex += newDataLenght;
        }

        private void ValidateEnoughSpaceInCurrentPage(long length)
        {
            if (_currentPage == 1)
            {
                if (_headerMemory.Page1EndIndex + length > _maxPositionPage1)
                {
                    _currentPage = 2;
                    _headerMemory.Page2EndIndex = _headerMemory.Page2StartIndex;
                    _headerMemory.FirstPage = 1;
                }                
            }
            else
            {
                if (_headerMemory.Page2EndIndex + length > _maxPositionPage2)
                {
                    _currentPage = 1;
                    _headerMemory.Page1EndIndex = _headerMemory.Page1StartIndex;
                    _headerMemory.FirstPage = 2;
                }                
            }
        }

        public void WriteEvent(string category, string source, string value)
        {
            DateTime created = DateTime.UtcNow;

            string dateTimeUniversalUC = created.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
            string input = $"{category}:/:{source}:/:{dateTimeUniversalUC}:/:{value}:/:";
            WriteString(input);
            //category::bizagi:/:2022-04-14T01:56:32.044Z:/:Un valor pequeñop:\:
            //@TODO poner campo para veces que se repite... 0001, soportando 10 mil repeticiones...
        }

        public void WriteEvent(string category, string value)
        {
            WriteEvent(category, "defaultSource", value);
        }

        public void WriteEvent(string value)
        {
            WriteEvent("defaultCategory", value);
        }
    }
}
