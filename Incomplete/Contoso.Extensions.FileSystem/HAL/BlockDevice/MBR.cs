using System;
using System.Collections.Generic;

namespace Contoso.Extensions.HAL.BlockDevice
{
    public class MBR
    {
        public readonly List<PartInfo> Partitions = new List<PartInfo>();

        public uint EBRLocation = 0;

        public class PartInfo
        {
            public readonly byte SystemId;
            public readonly uint StartSector;
            public readonly uint SectorCount;

            public PartInfo(byte systemId, uint startSector, uint sectorCount)
            {
                SystemId = systemId;
                StartSector = startSector;
                SectorCount = sectorCount;
            }
        }

        public MBR(byte[] mbr)
        {
            ParsePartition(mbr, 446);
            ParsePartition(mbr, 462);
            ParsePartition(mbr, 478);
            ParsePartition(mbr, 494);
        }

        protected void ParsePartition(byte[] mbr, uint loc)
        {
            var systemId = mbr[loc + 4]; // SystemID = 0 means no partition
            if (systemId == 0x5 || systemId == 0xF || systemId == 0x85)
            {
                // Extended Partition Detected
                // DOS only knows about 05, Windows 95 introduced 0F, Linux introduced 85
                // Search for logical volumes - http://thestarman.pcministry.com/asm/mbr/PartTables2.htm
                EBRLocation = BitConverter.ToUInt32(mbr, (int)loc + 8);
            }
            else if (systemId != 0)
            {
                var startSector = BitConverter.ToUInt32(mbr, (int)loc + 8);
                var sectorCount = BitConverter.ToUInt32(mbr, (int)loc + 12);
                Partitions.Add(new PartInfo(systemId, startSector, sectorCount));
            }
        }
    }
}
