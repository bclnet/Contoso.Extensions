using Contoso.Extensions.Caching.Stream;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using IOFileStream = System.IO.FileStream;

namespace Contoso.Extensions.Caching.FileStream
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ChunkHeader
    {
        public ushort magic;        // magic number says we're valid
        public ushort count;        // number of chunks in sequence
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ChunkRef
    {
        public long chunk; // chunk reference
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DeviceHeap
    {
        public int chunkSize;
        public int chunksLength;
        public int chunkRefsLength; // Size of circular buffer (set up by host)
        public long chunkRefs;      // Start of circular buffer
        public long freeChunkPtr;   // Current atomically-incremented non-wrapped offset
        public long retnChunkPtr;   // Current atomically-incremented non-wrapped offset
        public long chunks;
    }

    public class Falloc
    {
        const ushort FALLOC_MAGIC = 0x3412; // All our headers are prefixed with a magic number so we know they're ours
        readonly byte[] _heapBytes;
        DeviceHeap _heap;
        GenericPool<IOFileStream> _pool;
        int _blockSize;

        public unsafe Falloc(string filePath, int chunkSize = 4096, int blockSize = 1048576)
        {
            FilePath = filePath;
            _pool = new GenericPool<IOFileStream>(() => File.Open(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite));
            _heapBytes = new byte[sizeof(DeviceHeap)];
            _pool.Action(s => ReadHeapOrCreate(s, chunkSize, blockSize));
        }

        public string FilePath { get; }

        public DeviceHeap Heap => _heap;

        unsafe void ReadHeap(IOFileStream source)
        {
            source.Position = 0;
            source.Read(_heapBytes, 0, sizeof(DeviceHeap));
            fixed (byte* src = _heapBytes)
                _heap = Marshal.PtrToStructure<DeviceHeap>(new IntPtr(src));
        }

        unsafe void WriteHeap(IOFileStream source)
        {
            fixed (byte* src = _heapBytes)
                Marshal.StructureToPtr(_heap, new IntPtr(src), false);
            source.Position = 0;
            source.Write(_heapBytes, 0, sizeof(DeviceHeap));
        }

        unsafe void ReadHeapOrCreate(IOFileStream source, int chunkSize, int blockSize)
        {
            if (source.Length != 0)
            {
                ReadHeap(source);
                return;
            }

            // fix up chunkSize to include fallocChunkHeader
            chunkSize = (chunkSize + sizeof(ChunkHeader) + 15) & ~15;

            // fix up blockSize to be a multiple of chunkSize
            if (blockSize == 0 || (blockSize % chunkSize) != 0)
                blockSize += chunkSize - (blockSize % chunkSize);

            var chunksLength = blockSize;
            var chunks = blockSize / chunkSize;
            if (chunks == 0)
                throw new InvalidOperationException();

            // fix up blockSize to include DeviceHeap + freechunks
            var chunkRefsLength = chunks * sizeof(ChunkRef);
            _blockSize = (blockSize + chunkRefsLength + sizeof(DeviceHeap) + 15) & ~15;

            // transfer to heap
            var offset = sizeof(DeviceHeap);
            _heap = new DeviceHeap
            {
                chunkSize = chunkSize,
                chunksLength = chunksLength,
                chunkRefsLength = chunkRefsLength,
                chunkRefs = offset,
                freeChunkPtr = offset,
                retnChunkPtr = offset,
                chunks = offset + chunkRefsLength,
            };
            WriteHeap(source);
        }



        //static void writeChunkHeader(ChunkHeader hdr, ushort count)
        //{
        //    ChunkHeader header;
        //    header.magic = FALLOC_MAGIC;
        //    header.count = count;
        //}

        unsafe void GetChunk()
        {
            // advance circular buffer
            var chunkRefs = _heap.chunkRefs;
            var offset = _heap.freeChunkPtr += sizeof(ChunkRef);
            offset %= _heap.chunkRefsLength;
            var chunkRef = chunkRefs + offset;
            //var chunk = chunkRef.chunk;
            //WriteChunkHeader(chunk, 1);
            //chunkRef->chunk = nullptr;
            //return (void*)((char*)chunk + sizeof(fallocChunkHeader));
        }

        // void fallocFreeChunk(void* obj, cuFallocDeviceHeap* heap)
        //{
        //    if (!heap) heap = _defaultDeviceHeap;
        //    fallocChunkHeader* chunk = (fallocChunkHeader*)((char*)obj - sizeof(fallocChunkHeader));
        //    if (chunk->magic != FALLOC_MAGIC || chunk->count > 1) panic("bad magic"); // bad magic or not a singular chunk
        //                                                                              // advance circular buffer
        //    fallocChunkRef* chunkRefs = heap->chunkRefs;
        //    size_t offset = atomicAdd((unsigned int *) & heap->retnChunkPtr, sizeof(fallocChunkRef)) -(size_t)chunkRefs;
        //    offset %= heap->chunkRefsLength;
        //    writeChunkRef((fallocChunkRef*)((char*)chunkRefs + offset), chunk);
        //    chunk->magic = 0;
        //}
    }
}