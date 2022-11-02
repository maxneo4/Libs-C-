using System.IO.MemoryMappedFiles;

namespace SharedMemory
{
    class SharedMemory
    {
        const int KBS_BY_MB = 1024;
        const int BYTES_BY_KB = 1024;
        internal static MemoryMappedViewAccessor CreateOrOpenMapFileAndAccesorByMB(string identifier, int MbCount)
        {
            int kbsCount = GetKbFromMB(MbCount);
            return CreateOrOpenMapFileAndAccesorByKb(identifier, kbsCount);
        }

        internal static MemoryMappedViewAccessor CreateOrOpenMapFileAndAccesorByKb(string identifier, int KBCount)
        {
            long BytesCount = GetBytesFromKB(KBCount);
            MemoryMappedFile mmf = MemoryMappedFile.CreateOrOpen(identifier, BytesCount);
            MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor();
            return accessor;
        }

        internal static MemoryMappedViewAccessor OpenMapFileAndAccesor(string identifier)
        {            
            MemoryMappedFile mmf = MemoryMappedFile.OpenExisting(identifier);
            MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor();
            return accessor;
        }
        private static int GetKbFromMB(int mb)
        {
            return KBS_BY_MB * mb;
        }

        private static long GetBytesFromKB(long kb)
        {
            return BYTES_BY_KB * kb;
        }
    }
}
