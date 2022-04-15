using System;

namespace SharedMemory
{
    internal class HeaderMemory
    {
        public int FirstPage { get; set; }
        public Page Page1 { get; set; } = new Page(1);
        public Page Page2 { get; set; } = new Page(2);
        public Behaviour Behaviour { get; set; } = new Behaviour();
    }

    internal class Page
    {
        public int Number { get; private set; }
        public long Size { get; set; }
        public long StartIndex { get; set; }
        public long EndIndex { get; set; }
        public long MaxPosition { get; set; }
        public Page(int number)
        {
            Number = number;
        }
    }

    internal class Behaviour
    {
        public int CountSwapPages { get; set; }
        public int CountRegisteredEvents { get; set; }
        public string LastUnexcpected { get; set; }
        public DateTime WorkingSince { get; set; }
    }
}
