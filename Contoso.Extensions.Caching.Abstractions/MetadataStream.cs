using System.IO;
using IOStream = System.IO.Stream;

namespace Contoso.Extensions.Caching.Stream
{
    public class MetadataStream<T> : IOStream
    {
        public MetadataStream(IOStream @base, T metadata)
        {
            Base = @base;
            Metadata = metadata;
        }

        public IOStream Base { get; }

        public T Metadata { get; }

        public override bool CanRead => Base.CanRead;

        public override bool CanSeek => Base.CanSeek;

        public override bool CanWrite => Base.CanWrite;

        public override long Length => Base.Length;

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