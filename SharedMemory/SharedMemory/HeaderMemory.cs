namespace SharedMemory
{
    internal class HeaderMemory
    {
        public int FirstPage { get; set; }
        public long Page1Size { get; set; }
        public long Page2Size { get; set; }
        public long Page1StartIndex { get; set; }
        public long Page1EndIndex { get; set; }
        public long Page2StartIndex { get; set; }
        public long Page2EndIndex { get; set; }
    }
}
