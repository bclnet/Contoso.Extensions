namespace Contoso.Extensions.HAL.BlockDevice
{
    public class Partition : BlockDevice
    {
        readonly BlockDevice Host;
        readonly ulong StartingSector;

        public Partition(BlockDevice host, ulong startingSector, ulong sectorCount)
        {
            Host = host;
            StartingSector = startingSector;
            BlockCount = sectorCount;
            BlockSize = host.BlockSize;
        }
        
        public override string ToString() => "Partition";

        public override void ReadBlock(ulong blockNo, ulong blockCount, ref byte[] data)
        {
            CheckDataSize(data, blockCount);
            var hostBlockNo = StartingSector + blockNo;
            CheckBlockNo(hostBlockNo, blockCount);
            Host.ReadBlock(hostBlockNo, blockCount, ref data);
        }

        public override void WriteBlock(ulong blockNo, ulong blockCount, ref byte[] data)
        {
            CheckDataSize(data, blockCount);
            var hostBlockNo = StartingSector + blockNo;
            CheckBlockNo(hostBlockNo, blockCount);
            Host.WriteBlock(hostBlockNo, blockCount, ref data);
        }
    }
}
