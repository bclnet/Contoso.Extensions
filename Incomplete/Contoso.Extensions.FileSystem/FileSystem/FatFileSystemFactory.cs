using Contoso.Extensions.FileSystem.FAT;
using Contoso.Extensions.HAL.BlockDevice;
using System;

namespace Contoso.Extensions.FileSystem
{
    public class FatFileSystemFactory : FileSystemFactory
    {
        public override string Name => "FAT";

        public override bool IsType(Partition device)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            var bpb = device.NewBlockArray(1);
            device.ReadBlock(0UL, 1U, ref bpb);

            var sig = BitConverter.ToUInt16(bpb, 510);
            return sig == 0xAA55;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FatFileSystem"/> class.
        /// </summary>
        /// <param name="device">The partition.</param>
        /// <param name="rootPath">The root path.</param>
        /// <exception cref="Exception">FAT signature not found.</exception>
        public override FileSystem Create(Partition device, string rootPath, long size) => new FatFileSystem(device, rootPath, size);
    }
}
