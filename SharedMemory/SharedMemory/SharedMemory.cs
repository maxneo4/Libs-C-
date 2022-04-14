using System.IO.MemoryMappedFiles;

namespace SharedMemory
{
    class SharedMemory
    {
        const long BYTES_BY_MB = 1048576;
        const long BYTES_BY_KB = 1024;
        internal static MemoryMappedViewAccessor CreateMapFileAndAccesor(string identifier, int MbCount)
        {
            long BytesCount = GetBytesFromMB(MbCount);
            MemoryMappedFile mmf = MemoryMappedFile.CreateOrOpen(identifier, BytesCount);
            MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor();
            return accessor;
        }

        internal static MemoryMappedViewAccessor CreateMapFileAndAccesor(string identifier, long KBCount)
        {
            long BytesCount = GetBytesFromKB(KBCount);
            MemoryMappedFile mmf = MemoryMappedFile.CreateOrOpen(identifier, BytesCount);
            MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor();
            return accessor;
        }
        private static long GetBytesFromMB(int mb)
        {
            return BYTES_BY_MB * mb;
        }

        private static long GetBytesFromKB(long kb)
        {
            return BYTES_BY_KB * kb;
        }
    }
}
