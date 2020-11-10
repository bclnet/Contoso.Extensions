using System;
using System.Collections.Generic;

namespace Contoso.Extensions.HAL.BlockDevice
{
    // This class should not support selecting a device or sub device. Each instance must control exactly one device. For example with ATA
    // master/slave, each one needs its own device instance. For ATA this complicates things a bit because they share IO ports, but this
    // is an intentional decision.
    public abstract class BlockDevice : Device
    {
        public static readonly List<BlockDevice> Devices = new List<BlockDevice>();

        public byte[] NewBlockArray(uint blockCount) => new byte[blockCount * BlockSize];

        public ulong BlockCount { get; set; }

        public ulong BlockSize { get; set; }

        // Only allow reading and writing whole blocks because many of the hardware command work that way and we dont want to add complexity at the BlockDevice level.
        public abstract void ReadBlock(ulong blockNo, ulong blockCount, ref byte[] data);

        public abstract void WriteBlock(ulong blockNo, ulong blockCount, ref byte[] data);

        protected void CheckDataSize(byte[] data, ulong blockCount)
        {
            if ((ulong)data.Length != blockCount * BlockSize)
                throw new ArgumentOutOfRangeException(nameof(data), "Invalid size.");
        }

        protected void CheckBlockNo(ulong blockNo, ulong blockCount)
        {
            //if (blockNo + blockCount >= BlockCount)
            //    throw new Exception("Invalid block number.");
        }
    }
}
