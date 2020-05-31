using Contoso.Extensions.HAL.BlockDevice;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Contoso.Extensions.FileSystem.FAT
{
    internal class FatFileSystem : FileSystem
    {
        internal enum FatTypeEnum
        {
            Unknown,
            Fat12,
            Fat16,
            Fat32
        }

        internal class Fat
        {
            readonly FatFileSystem FileSystem;
            readonly ulong FatSector;

            /// <summary>
            /// Initializes a new instance of the <see cref="Fat"/> class.
            /// </summary>
            /// <param name="fileSystem">The file system.</param>
            /// <param name="fatSector">The first sector of the FAT table.</param>
            public Fat(FatFileSystem fileSystem, ulong fatSector)
            {
                if (fileSystem == null)
                    throw new ArgumentNullException(nameof(fileSystem));

                FileSystem = fileSystem;
                FatSector = fatSector;
            }

            /// <summary>
            /// Gets the size of a FAT entry in bytes.
            /// </summary>
            /// <returns>The size of a FAT entry in bytes.</returns>
            /// <exception cref="NotSupportedException">Can not get the FAT entry size for an unknown FAT type.</exception>
            uint GetFatEntrySizeInBytes()
            {
                switch (FileSystem.FatType)
                {
                    case FatTypeEnum.Fat32: return 4;
                    case FatTypeEnum.Fat16: return 2;
                    case FatTypeEnum.Fat12:
                    default: throw new ArgumentOutOfRangeException(nameof(FileSystem.FatType), "Can not get the FAT entry size for an unknown FAT type.");
                }
            }

            /// <summary>
            /// Gets the FAT chain.
            /// </summary>
            /// <param name="firstEntry">The first entry.</param>
            /// <param name="dataSize">Size of a data to be stored in bytes.</param>
            /// <returns>An array of cluster numbers for the FAT chain.</returns>
            /// <exception cref="ArgumentOutOfRangeException"></exception>
            public uint[] GetFatChain(uint firstEntry, long dataSize = 0)
            {
                var rtn = new uint[0];
                var currentEntry = firstEntry;

                var entriesRequired = dataSize / FileSystem.BytesPerCluster;
                if (dataSize % FileSystem.BytesPerCluster != 0)
                    entriesRequired++;

                GetFatEntry(currentEntry, out var value);
                Array.Resize(ref rtn, rtn.Length + 1);
                rtn[rtn.Length - 1] = currentEntry;

                if (entriesRequired > 0)
                {
                    while (!FatEntryIsEof(value))
                    {
                        currentEntry = value;
                        GetFatEntry(currentEntry, out value);
                        Array.Resize(ref rtn, rtn.Length + 1);
                        rtn[rtn.Length - 1] = currentEntry;
                    }

                    if (entriesRequired > rtn.Length)
                    {
                        var newClusters = entriesRequired - rtn.Length;
                        for (var i = 0; i < newClusters; i++)
                        {
                            currentEntry = GetNextUnallocatedFatEntry();
                            var lastFatEntry = rtn[rtn.Length - 1];
                            SetFatEntry(lastFatEntry, currentEntry);
                            SetFatEntry(currentEntry, FatEntryEofValue());
                            Array.Resize(ref rtn, rtn.Length + 1);
                            rtn[rtn.Length - 1] = currentEntry;
                        }
                    }
                }

                var chain = string.Empty;
                for (var i = 0; i < rtn.Length; i++)
                {
                    chain += rtn[i];
                    if (i > 0 || i < rtn.Length - 1)
                        chain += "->";
                }

                SetFatEntry(currentEntry, FatEntryEofValue());
                return rtn;
            }

            /// <summary>
            /// Gets the next unallocated FAT entry.
            /// </summary>
            /// <returns>The index of the next unallocated FAT entry.</returns>
            /// <exception cref="Exception">Failed to find an unallocated FAT entry.</exception>
            public uint GetNextUnallocatedFatEntry()
            {
                var totalEntries = FileSystem.FatSectorCount * FileSystem.BytesPerSector / GetFatEntrySizeInBytes();
                for (var i = FileSystem.RootCluster; i < totalEntries; i++)
                {
                    GetFatEntry(i, out var entryValue);
                    if (FatEntryIsFree(entryValue))
                        return i;
                }
                throw new Exception("Failed to find an unallocated FAT entry.");
            }

            /// <summary>
            /// Clears a FAT entry.
            /// </summary>
            /// <param name="entryNumber">The entry number.</param>
            public void ClearFatEntry(ulong entryNumber) => SetFatEntry(entryNumber, 0);

            void SetFatEntry(byte[] data, ulong entryNumber, ulong value)
            {
                var entrySize = GetFatEntrySizeInBytes();
                var entryOffset = entryNumber * entrySize;

                switch (FileSystem.FatType)
                {
                    case FatTypeEnum.Fat12: data.SetUInt16(entryOffset, (ushort)value); break;
                    case FatTypeEnum.Fat16: data.SetUInt16(entryOffset, (ushort)value); break;
                    case FatTypeEnum.Fat32: data.SetUInt32(entryOffset, (uint)value); break;
                    default: throw new ArgumentOutOfRangeException(nameof(FileSystem.FatType), "Unknown FAT type.");
                }
            }

            void GetFatEntry(byte[] data, uint entryNumber, out uint value)
            {
                var entrySize = GetFatEntrySizeInBytes();
                var entryOffset = (ulong)(entryNumber * entrySize);

                switch (FileSystem.FatType)
                {
                    case FatTypeEnum.Fat12:
                        var result = BitConverter.ToUInt16(data, (int)entryOffset);
                        value = (uint)((entryNumber & 0x01) == 0 ? result & 0x0FFF : result >> 4);
                        break;
                    case FatTypeEnum.Fat16: value = BitConverter.ToUInt16(data, (int)entryOffset); break;
                    case FatTypeEnum.Fat32: value = BitConverter.ToUInt32(data, (int)entryOffset) & 0x0FFFFFFF; break;
                    default: throw new ArgumentOutOfRangeException(nameof(FileSystem.FatType), "Unknown FAT type.");
                }
            }

            public void ClearAllFat()
            {
                var fatTable = FileSystem.NewBlockArray();
                ReadFatSector(0, out var fatTableFistSector);

                SetFatEntry(fatTableFistSector, 2, FatEntryEofValue()); // Change 3rd entry (RootDirectory) to be EOC
                Array.Copy(fatTableFistSector, fatTable, 12); // Copy first three elements on fatTable
                WriteFatSector(0, fatTable); // The rest of 'fatTable' should be all 0s as new does this internally

                // Array.Clear() not work: stack overflow!
                for (var i = 0; i < 11; i++)
                    fatTable[i] = 0;

                for (var sector = 1UL; sector < FileSystem.FatSectorCount; sector++)
                {
                    if (sector % 100 == 0)
                        Debugger.Log(0, "FileSystem", $"Clearing sector {sector}");
                    WriteFatSector(sector, fatTable);
                }
            }

            void ReadFatSector(ulong sector, out byte[] data)
            {
                data = FileSystem.NewBlockArray();
                FileSystem.Device.ReadBlock(FatSector + sector, FileSystem.SectorsPerCluster, ref data);
            }

            void WriteFatSector(ulong sector, byte[] data)
            {
                if (data == null)
                    throw new ArgumentNullException(nameof(data));

                FileSystem.Device.WriteBlock(FatSector + sector, FileSystem.SectorsPerCluster, ref data);
            }

            /// <summary>
            /// Gets a FAT entry.
            /// </summary>
            /// <param name="entryNumber">The entry number.</param>
            /// <param name="value">The entry value.</param>
            internal void GetFatEntry(uint entryNumber, out uint value)
            {
                var entrySize = GetFatEntrySizeInBytes();
                var entryOffset = (ulong)(entryNumber * entrySize);
                var sector = entryOffset / FileSystem.BytesPerSector;

                ReadFatSector(sector, out var data);

                switch (FileSystem.FatType)
                {
                    case FatTypeEnum.Fat12:
                        var result = BitConverter.ToUInt16(data, (int)entryOffset);
                        value = (uint)((entryNumber & 0x01) == 0 ? result & 0x0FFF : result >> 4);
                        break;
                    case FatTypeEnum.Fat16: value = BitConverter.ToUInt16(data, (int)entryOffset); break;
                    case FatTypeEnum.Fat32: value = BitConverter.ToUInt32(data, (int)entryOffset) & 0x0FFFFFFF; break;
                    default: throw new ArgumentOutOfRangeException(nameof(FileSystem.FatType), "Unknown FAT type.");
                }
            }

            /// <summary>
            /// Sets a FAT entry.
            /// </summary>
            /// <param name="entryNumber">The entry number.</param>
            /// <param name="value">The value.</param>
            internal void SetFatEntry(ulong entryNumber, ulong value)
            {
                var entrySize = GetFatEntrySizeInBytes();
                var entryOffset = entryNumber * entrySize;
                var sector = entryOffset / FileSystem.BytesPerSector;
                var sectorOffset = (sector * FileSystem.BytesPerSector) - entryOffset;

                ReadFatSector(sector, out var data);

                switch (FileSystem.FatType)
                {
                    case FatTypeEnum.Fat12: data.SetUInt16(entryOffset, (ushort)value); break;
                    case FatTypeEnum.Fat16: data.SetUInt16(entryOffset, (ushort)value); break;
                    case FatTypeEnum.Fat32: data.SetUInt32(entryOffset, (uint)value); break;
                    default: throw new ArgumentOutOfRangeException(nameof(FileSystem.FatType), "Unknown FAT type.");
                }

                WriteFatSector(sector, data);
            }

            internal bool FatEntryIsFree(uint value) => value == 0;

            internal bool FatEntryIsEof(uint value)
            {
                switch (FileSystem.FatType)
                {
                    case FatTypeEnum.Fat12: return (value & 0x0FFF) >= 0x0FF8;
                    case FatTypeEnum.Fat16: return (value & 0xFFFF) >= 0xFFF8;
                    case FatTypeEnum.Fat32: return (value & 0x0FFFFFF8) >= 0x0FFFFFF8;
                    default: throw new ArgumentOutOfRangeException(nameof(FileSystem.FatType), "Unknown FAT type");
                }
            }

            internal bool FatEntryIsBad(uint value)
            {
                switch (FileSystem.FatType)
                {
                    case FatTypeEnum.Fat12: return (value & 0x0FFF) == 0x0FF7;
                    case FatTypeEnum.Fat16: return (value & 0xFFFF) == 0xFFF7;
                    case FatTypeEnum.Fat32: return (value & 0x0FFFFFF8) == 0x0FFFFFF7;
                    default: throw new ArgumentOutOfRangeException(nameof(FileSystem.FatType), "Unknown FAT type");
                }
            }

            /// <summary>
            /// The the EOF value for a specific FAT type.
            /// </summary>
            /// <returns>The EOF value.</returns>
            /// <exception cref="Exception">Unknown file system type.</exception>
            internal ulong FatEntryEofValue()
            {
                switch (FileSystem.FatType)
                {
                    case FatTypeEnum.Fat12: return 0x0FFF;
                    case FatTypeEnum.Fat16: return 0xFFFF;
                    case FatTypeEnum.Fat32: return 0x0FFFFFFF;
                    default: throw new ArgumentOutOfRangeException(nameof(FileSystem.FatType), "Unknown FAT type");
                }
            }
        }

        public readonly uint BytesPerCluster;

        public readonly uint BytesPerSector;

        public readonly uint ClusterCount;

        public readonly uint DataSector; // First Data Sector

        public readonly uint DataSectorCount;

        public readonly uint FatSectorCount;

        readonly FatTypeEnum FatType;

        public readonly uint NumberOfFATs;

        public readonly uint ReservedSectorCount;

        public readonly uint RootCluster; // FAT32

        public readonly uint RootEntryCount;

        public readonly uint RootSector; // FAT12/16

        public readonly uint RootSectorCount; // FAT12/16, FAT32 remains 0

        public readonly uint SectorsPerCluster;

        public readonly uint TotalSectorCount;

        readonly Fat[] Fats;

        public override string Type
        {
            get
            {
                switch (FatType)
                {
                    case FatTypeEnum.Fat12: return "FAT12";
                    case FatTypeEnum.Fat16: return "FAT16";
                    case FatTypeEnum.Fat32: return "FAT32";
                    default: throw new ArgumentOutOfRangeException(nameof(FatType), "Unknown FAT type");
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FatFileSystem"/> class.
        /// </summary>
        /// <param name="device">The partition.</param>
        /// <param name="rootPath">The root path.</param>
        /// <exception cref="Exception">FAT signature not found.</exception>
        public FatFileSystem(Partition device, string rootPath, long size)
            : base(device, rootPath, size)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));
            if (string.IsNullOrEmpty(rootPath))
                throw new ArgumentException(nameof(rootPath));

            var bpb = Device.NewBlockArray(1);
            Device.ReadBlock(0UL, 1U, ref bpb);

            var sig = BitConverter.ToUInt16(bpb, 510);
            if (sig != 0xAA55)
                throw new Exception("FAT signature not found.");

            BytesPerSector = BitConverter.ToUInt16(bpb, 11);
            SectorsPerCluster = bpb[13];
            BytesPerCluster = BytesPerSector * SectorsPerCluster;
            ReservedSectorCount = BitConverter.ToUInt16(bpb, 14);
            NumberOfFATs = bpb[16];
            RootEntryCount = BitConverter.ToUInt16(bpb, 17);

            TotalSectorCount = BitConverter.ToUInt16(bpb, 19);
            if (TotalSectorCount == 0)
                TotalSectorCount = BitConverter.ToUInt32(bpb, 32);

            // FATSz
            FatSectorCount = BitConverter.ToUInt16(bpb, 22);
            if (FatSectorCount == 0)
                FatSectorCount = BitConverter.ToUInt32(bpb, 36);

            DataSectorCount = TotalSectorCount - (ReservedSectorCount + NumberOfFATs * FatSectorCount + ReservedSectorCount);

            // Computation rounds down.
            ClusterCount = DataSectorCount / SectorsPerCluster;

            // Determine the FAT type. Do not use another method - this IS the official and proper way to determine FAT type.
            // Comparisons are purposefully < and not <= FAT16 starts at 4085, FAT32 starts at 65525
            if (ClusterCount < 4085) FatType = FatTypeEnum.Fat12;
            else if (ClusterCount < 65525) FatType = FatTypeEnum.Fat16;
            else FatType = FatTypeEnum.Fat32;

            if (FatType == FatTypeEnum.Fat32)
                RootCluster = BitConverter.ToUInt32(bpb, 44);
            else
            {
                RootSector = ReservedSectorCount + NumberOfFATs * FatSectorCount;
                RootSectorCount = (RootEntryCount * 32 + (BytesPerSector - 1)) / BytesPerSector;
            }
            DataSector = ReservedSectorCount + NumberOfFATs * FatSectorCount + RootSectorCount;

            Fats = new Fat[NumberOfFATs];
            for (var i = 0UL; i < NumberOfFATs; i++)
                Fats[i] = new Fat(this, (ReservedSectorCount + i * FatSectorCount));
        }

        internal Fat GetFat(int tableNumber)
        {
            if (Fats.Length > tableNumber)
                return Fats[tableNumber];
            throw new Exception("The fat table number doesn't exist.");
        }

        internal byte[] NewBlockArray() => new byte[BytesPerCluster];

        internal void Read(long cluster, out byte[] data, long size = 0, long offset = 0)
        {
            if (size == 0)
                size = BytesPerCluster;

            if (FatType == FatTypeEnum.Fat32)
            {
                data = NewBlockArray();
                var sector = DataSector + (cluster - RootCluster) * SectorsPerCluster;
                Device.ReadBlock((ulong)sector, SectorsPerCluster, ref data);
            }
            else
            {
                data = Device.NewBlockArray(1);
                Device.ReadBlock((ulong)cluster, RootSectorCount, ref data);
            }
        }

        internal void Write(long cluster, byte[] data, long size = 0, long offset = 0)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (size == 0)
                size = BytesPerCluster;

            Read(cluster, out var data2);
            Array.Copy(data, 0, data2, offset, data.Length);

            if (FatType == FatTypeEnum.Fat32)
            {
                long xSector = DataSector + (cluster - RootCluster) * SectorsPerCluster;
                Device.WriteBlock((ulong)xSector, SectorsPerCluster, ref data2);
            }
            else
            {
                Device.WriteBlock((ulong)cluster, RootSectorCount, ref data2);
            }
        }

        public override void DisplayFileSystemInfo()
        {
            Console.WriteLine(
$@"-------File System--------
Bytes per Cluster     = {BytesPerCluster}
Bytes per Sector      = {BytesPerSector}
Cluster Count         = {ClusterCount}
Data Sector           = {DataSector}
Data Sector Count     = {DataSectorCount}
FAT Sector Count      = {FatSectorCount}
FAT Type              = {(uint)FatType}
Number of FATS        = {NumberOfFATs}
Reserved Sector Count = {ReservedSectorCount}
Root Cluster          = {RootCluster}
Root Entry Count      = {RootEntryCount}
Root Sector           = {RootSector}
Root Sector Count     = {RootSectorCount}
Sectors per Cluster   = {SectorsPerCluster}
Total Sector Count    = {TotalSectorCount}");
        }

        public override List<DirectoryEntry> GetDirectoryListing(DirectoryEntry baseDirectory)
        {
            if (baseDirectory == null)
                throw new ArgumentNullException(nameof(baseDirectory));

            var result = new List<DirectoryEntry>();
            var entry = (FatDirectoryEntry)baseDirectory;
            var fatListing = entry.ReadDirectoryContents();

            for (var i = 0; i < fatListing.Count; i++)
                result.Add(fatListing[i]);
            return result;
        }

        public override DirectoryEntry GetRootDirectory() => new FatDirectoryEntry(this, null, RootPath, Size, RootPath, RootCluster);

        public override DirectoryEntry CreateDirectory(DirectoryEntry parentDirectory, string newDirectory)
        {
            if (parentDirectory == null)
                throw new ArgumentNullException(nameof(parentDirectory));
            if (string.IsNullOrEmpty(newDirectory))
                throw new ArgumentNullException(nameof(newDirectory));

            var parentDirectory2 = (FatDirectoryEntry)parentDirectory;
            var directoryEntryToAdd = parentDirectory2.AddDirectoryEntry(newDirectory, DirectoryEntryType.Directory);
            return directoryEntryToAdd;
        }

        public override DirectoryEntry CreateFile(DirectoryEntry parentDirectory, string newFile)
        {
            if (parentDirectory == null)
                throw new ArgumentNullException(nameof(parentDirectory));
            if (string.IsNullOrEmpty(newFile))
                throw new ArgumentNullException(nameof(newFile));

            var parentDirectory2 = (FatDirectoryEntry)parentDirectory;
            var directoryEntryToAdd = parentDirectory2.AddDirectoryEntry(newFile, DirectoryEntryType.File);
            return directoryEntryToAdd;
        }

        public override void DeleteDirectory(DirectoryEntry directoryEntry)
        {
            if (directoryEntry == null)
                throw new ArgumentNullException(nameof(directoryEntry));

            var directoryEntry2 = (FatDirectoryEntry)directoryEntry;
            directoryEntry2.DeleteDirectoryEntry();
        }

        public override void DeleteFile(DirectoryEntry directoryEntry)
        {
            if (directoryEntry == null)
                throw new ArgumentNullException(nameof(directoryEntry));

            var directoryEntry2 = (FatDirectoryEntry)directoryEntry;

            var entries = directoryEntry2.GetFatTable();

            foreach (var entry in entries)
                GetFat(0).ClearFatEntry(entry);

            directoryEntry2.DeleteDirectoryEntry();
        }

        public override string Label
        {
            // In the FAT filesystem the name field of RootDirectory is - in reality - the Volume Label
            get
            {
                var rootDirectory = (FatDirectoryEntry)GetRootDirectory();

                var volumeId = rootDirectory.FindVolumeId();
                return volumeId == null ? rootDirectory.Name : volumeId.Name.TrimEnd();
            }
            set
            {
                var rootDirectory = (FatDirectoryEntry)GetRootDirectory();

                var volumeId = rootDirectory.FindVolumeId();
                if (volumeId != null)
                {
                    volumeId.SetName(value);
                    return;
                }
                rootDirectory.CreateVolumeId(value);
            }
        }

        public override long AvailableFreeSpace
        {
            get
            {
                var rootDirectory = (FatDirectoryEntry)GetRootDirectory();
                // this is effectively the same as TotalFreeSpace ("user quotas" not supported)

                // Size is expressed in MegaByte
                var TotalSizeInBytes = Size * 1024 * 1024;
                var UsedSpace = rootDirectory.GetUsedSpace();

                return TotalSizeInBytes - UsedSpace;
            }
        }

        public override long TotalFreeSpace
        {
            get
            {
                var rootDirectory = (FatDirectoryEntry)GetRootDirectory();

                // Size is expressed in MegaByte
                var TotalSizeInBytes = Size * 1024 * 1024;
                var UsedSpace = rootDirectory.GetUsedSpace();

                return TotalSizeInBytes - UsedSpace;
            }
        }

        public override void Format(string aDriveFormat, bool aQuick)
        {
            var rootDirectory = (FatDirectoryEntry)GetRootDirectory();
            var fat = GetFat(0);

            var x = rootDirectory.ReadDirectoryContents();
            foreach (var el in x)
                el.DeleteDirectoryEntry();

            fat.ClearAllFat();
        }
    }
}
