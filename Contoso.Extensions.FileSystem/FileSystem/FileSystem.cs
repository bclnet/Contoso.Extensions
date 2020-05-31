using Contoso.Extensions.HAL.BlockDevice;
using System.Collections.Generic;

// https://github.com/CosmosOS
namespace Contoso.Extensions.FileSystem
{
    public abstract class FileSystem
    {
        protected FileSystem(Partition device, string rootPath, long size)
        {
            Device = device;
            RootPath = rootPath;
            Size = size;
        }

        public abstract void DisplayFileSystemInfo();

        public abstract List<DirectoryEntry> GetDirectoryListing(DirectoryEntry baseDirectory);

        public abstract DirectoryEntry GetRootDirectory();

        public abstract DirectoryEntry CreateDirectory(DirectoryEntry parentDirectory, string newDirectory);

        public abstract DirectoryEntry CreateFile(DirectoryEntry parentDirectory, string newFile);

        public abstract void DeleteDirectory(DirectoryEntry path);

        public abstract void DeleteFile(DirectoryEntry path);

        protected Partition Device { get; }

        public string RootPath { get; }

        public long Size { get; }

        public abstract long AvailableFreeSpace { get; }

        public abstract long TotalFreeSpace { get; }

        public abstract string Type { get; }

        public abstract string Label { get; set; }

        public abstract void Format(string driveFormat, bool quick);
    }
}
