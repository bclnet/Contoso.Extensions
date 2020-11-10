using System;
using System.Collections.Generic;

namespace Contoso.Extensions.HAL.BlockDevice
{
    public class EBR
    {
        public readonly List<PartInfo> Partitions = new List<PartInfo>();

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

        public EBR(byte[] ebr)
        {
            ParsePartition(ebr, 446);
            ParsePartition(ebr, 462);
        }

        protected void ParsePartition(byte[] ebr, uint loc)
        {
            var systemId = ebr[loc + 4]; // SystemID = 0 means no partition
            if (systemId == 0x5 || systemId == 0xF || systemId == 0x85)
            {
                // Extended Partition Detected
                // TODO: Extended Partition Table
            }
            else if (systemId != 0)
            {
                var startSector = BitConverter.ToUInt32(ebr, (int)loc + 8);
                var sectorCount = BitConverter.ToUInt32(ebr, (int)loc + 12);
                Partitions.Add(new PartInfo(systemId, startSector, sectorCount));
            }
        }
    }
}
