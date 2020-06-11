using System;
using System.IO;
using IOStream = System.IO.Stream;

namespace Contoso.Extensions.Caching.Stream
{
    [Serializable]
    public class StreamWithHeader : IOStream
    {
        public StreamWithHeader(IOStream @base, byte[] header = null)
        {
            if (@base == null)
                throw new ArgumentNullException(nameof(@base));
            //if (header == null)
            //    throw new ArgumentNullException(nameof(header));

            Base = @base;
            Header = header;
        }

        public static implicit operator byte[](StreamWithHeader s) => s.Header;

        public IOStream Base { get; }

        public byte[] Header { get; }

        public override bool CanRead => Base.CanRead;

        public override bool CanSeek => Base.CanSeek;

        public override bool CanWrite => Base.CanWrite;

        public override long Length => Base.Length;

        public long TotalLength => Base.Length + Header.Length;

        public override long Position
        {
            get => Base.Position;
            set => Base.Position = value;
        }

        public override void Flush() => Base.Flush();

        public override int Read(byte[] buffer, int offset, int count) => Base.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin) => Base.Seek(offset, origin);

        public override void SetLength(long value) => Base.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count) => Base.Write(buffer, offset, count);
    }
}