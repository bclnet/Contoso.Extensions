using System;
using System.IO;

namespace Contoso.Extensions.FileSystem.FAT
{
    internal class FatStream : Stream
    {
        protected byte[] ReadBuffer;
        protected long? ReadBufferPosition;
        readonly FatDirectoryEntry DirectoryEntry;
        readonly FatFileSystem FS;
        //TODO: In future we might read this in as needed rather than all at once. This structure will also consume 2% of file size in RAM
        // (for default cluster size of 2kb, ie 4 bytes per cluster) so we might consider a way to flush it and only keep parts.
        // Example, a 100 MB file will require 2MB for this structure. That is probably acceptable for the mid term future.
        uint[] FatTable;
        long Size;

        public FatStream(FatDirectoryEntry entry)
        {
            DirectoryEntry = entry ?? throw new ArgumentNullException(nameof(entry));
            FS = entry.GetFileSystem();
            FatTable = entry.GetFatTable();
            Size = entry.Size;
            if (FatTable == null)
                throw new Exception("The fat chain returned for the directory entry was null.");
        }

        public override bool CanSeek => true;

        public override bool CanRead => true;

        public override bool CanWrite => true;

        public sealed override long Length => Size;

        protected long _position;
        public override long Position
        {
            get => _position;
            set
            {
                if (value < 0L)
                    throw new ArgumentOutOfRangeException(nameof(value));
                _position = value;
            }
        }

        public override void Flush() => throw new NotImplementedException();

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin: return Position = offset;
                case SeekOrigin.Current: return Position += offset;
                case SeekOrigin.End: return Position = Length + offset;
                default: throw new ArgumentOutOfRangeException(nameof(origin), origin.ToString());
            }
        }

        public override void SetLength(long value)
        {
            DirectoryEntry.SetSize(value);
            Size = value;
            FatTable = DirectoryEntry.GetFatTable();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count + offset > buffer?.Length)
                throw new ArgumentOutOfRangeException(nameof(offset), "Invalid offset length.");

            if (FatTable.Length == 0 || FatTable[0] == 0)
                return 0;
            if (Position >= DirectoryEntry.Size)
                return 0;

            var maxReadableBytes = DirectoryEntry.Size - Position;
            var count2 = (long)count;
            var offset2 = (long)offset;
            if (count2 > maxReadableBytes)
                count2 = maxReadableBytes;
            var clusterSize = FS.BytesPerCluster;

            while (count2 > 0)
            {
                var clusterIdx = Position / clusterSize;
                var posInCluster = Position % clusterSize;
                FS.Read(FatTable[(int)clusterIdx], out byte[] cluster);
                var readSize = posInCluster + count2 > clusterSize ? clusterSize - posInCluster : count2;
                Array.Copy(cluster, posInCluster, buffer, offset2, readSize);
                offset2 += readSize;
                count2 -= readSize;
                Position += readSize;
            }
            return (int)offset2;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset), "Invalid offset length.");

            var clusterSize = FS.BytesPerCluster;
            var count2 = (long)count;
            var offset2 = (long)offset;
            var totalLength = Position + count2;
            if (totalLength > Length)
                SetLength(totalLength);

            while (count2 > 0)
            {
                var clusterIdx = Position / clusterSize;
                var posInCluster = Position % clusterSize;
                var writeSize = posInCluster + count2 > clusterSize ? clusterSize - posInCluster : count2;

                FS.Read(FatTable[clusterIdx], out byte[] cluster);
                Array.Copy(buffer, offset, cluster, (int)posInCluster, (int)writeSize);
                FS.Write(FatTable[clusterIdx], cluster);

                offset2 += writeSize;
                count2 -= writeSize;
                offset += (int)writeSize;
                Position += writeSize;
            }
        }
    }
}
