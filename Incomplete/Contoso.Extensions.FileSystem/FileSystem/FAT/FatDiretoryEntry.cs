using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Contoso.Extensions.FileSystem.FAT
{
    internal class FatDirectoryEntry : DirectoryEntry
    {
        readonly uint EntryHeaderDataOffset;
        readonly uint FirstClusterNum;

        // Size is UInt32 because FAT doesn't support bigger. Don't change to UInt64
        public FatDirectoryEntry(FatFileSystem fileSystem, FatDirectoryEntry parent, string fullPath, string name, long size, uint firstCluster, uint entryHeaderDataOffset, DirectoryEntryType entryType)
            : base(fileSystem, parent, fullPath, name, size, entryType)
        {
            if (firstCluster < fileSystem.RootCluster)
                throw new ArgumentOutOfRangeException(nameof(firstCluster));

            FirstClusterNum = firstCluster;
            EntryHeaderDataOffset = entryHeaderDataOffset;
        }
        public FatDirectoryEntry(FatFileSystem fileSystem, FatDirectoryEntry parent, string fullPath, long size, string name, uint firstCluster)
            : base(fileSystem, parent, fullPath, name, size, DirectoryEntryType.Directory)
        {
            if (firstCluster < fileSystem.RootCluster)
                throw new ArgumentOutOfRangeException(nameof(firstCluster));

            FirstClusterNum = firstCluster;
            EntryHeaderDataOffset = 0;
        }

        public uint[] GetFatTable()
        {
            var fat = ((FatFileSystem)FileSystem).GetFat(0);
            return fat?.GetFatChain(FirstClusterNum, Size);
        }

        public FatFileSystem GetFileSystem() => ((FatFileSystem)FileSystem);

        public override Stream GetFileStream() => EntryType == DirectoryEntryType.File ? new FatStream(this) : null;

        public override void SetName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            SetDirectoryEntryMetadataValue(FatDirectoryEntryMetadata.ShortName, name);
            Name = name;
        }

        public override void SetSize(long size)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(nameof(size));

            SetDirectoryEntryMetadataValue(FatDirectoryEntryMetadata.Size, size);
            Size = size;
        }

        void AllocateDirectoryEntry(string aShortName)
        {
            var nameString = GetShortName(aShortName);

            SetDirectoryEntryMetadataValue(FatDirectoryEntryMetadata.ShortName, nameString);

            if (EntryType == DirectoryEntryType.Directory)
                SetDirectoryEntryMetadataValue(FatDirectoryEntryMetadata.Attributes, FatDirectoryEntryAttributes.Directory);

            // TODO: Add a define so we can skip blocks when running outside.
            // Date and Time
            //var date = ((((uint)RTC.Century * 100 + (uint)RTC.Year) - 1980) << 9) | (uint)RTC.Month << 5 | (uint)RTC.DayOfTheMonth;
            //var time = (uint)RTC.Hour << 11 | (uint)RTC.Minute << 5 | ((uint)RTC.Second / 2);

            //SetDirectoryEntryMetadataValue(FatDirectoryEntryMetadata.CreationDate, date);
            //SetDirectoryEntryMetadataValue(FatDirectoryEntryMetadata.ModifiedDate, date);
            //SetDirectoryEntryMetadataValue(FatDirectoryEntryMetadata.CreationTime, time);
            //SetDirectoryEntryMetadataValue(FatDirectoryEntryMetadata.ModifiedTime, time);

            // First cluster
            SetDirectoryEntryMetadataValue(FatDirectoryEntryMetadata.FirstClusterHigh, (ushort)(FirstClusterNum >> 16));
            SetDirectoryEntryMetadataValue(FatDirectoryEntryMetadata.FirstClusterLow, (ushort)(FirstClusterNum & 0xFFFF));

            // GetFatTable calls GetFatChain, which "refreshes" the FAT table and clusters
            GetFatTable();
        }

        public FatDirectoryEntry AddDirectoryEntry(string name, DirectoryEntryType entryType)
        {
            if (entryType == DirectoryEntryType.Directory || entryType == DirectoryEntryType.File)
            {
                var shortName = name;
                uint[] directoryEntriesToAllocate = null;

                var x1 = entryType == DirectoryEntryType.File;
                var x2 = name.Contains(".");
                var x3 = x2 ? name.Substring(0, name.LastIndexOf('.')).Contains(".") : false;
                var x4 = x2 ? name.Substring(0, name.IndexOf('.')).Length > 8 : false;
                var x5 = x2 ? name.Substring(name.IndexOf('.') + 1).Length > 3 : false;
                var x6 = entryType == DirectoryEntryType.Directory;
                var x7 = name.Length > 11;

                var x8 = x3 || (x4 || x5);
                var x9 = x2 && x8;

                var x10 = (x1 && x9) || (x6 && x7);

                if (x10)
                {
                    var longName = name;

                    var lastPeriodPosition = name.LastIndexOf('.');

                    var ext = string.Empty;

                    // Only take the name until the first dot
                    if (lastPeriodPosition + 1 > 0 && lastPeriodPosition + 1 < name.Length)
                        ext = shortName.Substring(lastPeriodPosition + 1);

                    // Remove all whitespaces and dots (except final)
                    for (var i = shortName.Length - 1; i > 0; i--)
                    {
                        var c = shortName[i];
                        if (char.IsWhiteSpace(c) || (c == '.' && i != lastPeriodPosition))
                            shortName.Remove(i, 1);
                    }

                    var invalidShortNameChars = new[] { '"', '*', '+', ',', '.', '/', ':', ';', '<', '=', '>', '?', '[', '\\', ']', '|' };

                    // Remove all invalid characters
                    foreach (var invalidChar in invalidShortNameChars)
                        shortName.Replace(invalidChar, '_');

                    var n = 1;
                    var directoryEntries = ReadDirectoryContents(true);
                    var shortFilenames = new string[directoryEntries.Count];
                    for (var i = 0; i < directoryEntries.Count; i++)
                        shortFilenames[i] = directoryEntries[i].Name;

                    var nameTry = string.Empty;
                    var test = false;
                    do
                    {
                        nameTry = (shortName.Substring(0, 7 - n.ToString().Length) + "~" + n).ToUpperInvariant();
                        if (!string.IsNullOrEmpty(ext))
                            nameTry += '.' + ext.ToUpperInvariant();
                        n++;
                        test = false;
                        foreach (var name2 in shortFilenames)
                            if (name2 == nameTry)
                            {
                                test = true;
                                break;
                            }
                    }
                    //while (Array.IndexOf((Array)xShortFilenames, xNameTry) != -1); //TODO: use the generic version of IndexOf, just remove the cast to Array
                    while (test);

                    shortName = nameTry;
                    var checksum = CalculateChecksum(GetShortName(shortName));
                    var numEntries = (int)Math.Ceiling(longName.Length / 13d);
                    var longNameWithPad = new char[numEntries * 13];
                    longNameWithPad[longNameWithPad.Length - 1] = (char)0xFFFF;
                    Array.Copy(longName.ToCharArray(), longNameWithPad, longName.Length);

                    directoryEntriesToAllocate = GetNextUnallocatedDirectoryEntries(numEntries + 1);

                    for (var i = numEntries - 1; i >= 0; i--)
                    {
                        var entry = directoryEntriesToAllocate[numEntries - i - 1];

                        SetLongFilenameEntryMetadataValue(entry, FatDirectoryEntryMetadata.LongFilenameEntryMetadata.SequenceNumberAndAllocationStatus, (i + 1) | (i == numEntries - 1 ? (1 << 6) : 0));
                        SetLongFilenameEntryMetadataValue(entry, FatDirectoryEntryMetadata.LongFilenameEntryMetadata.Attributes, FatDirectoryEntryAttributes.LongName);
                        SetLongFilenameEntryMetadataValue(entry, FatDirectoryEntryMetadata.LongFilenameEntryMetadata.Checksum, checksum);

                        var a1 = new string(longNameWithPad, i * 13, 5);
                        var a2 = new string(longNameWithPad, i * 13 + 5, 6);
                        var a3 = new string(longNameWithPad, i * 13 + 11, 2);

                        SetLongFilenameEntryMetadataValue(entry, FatDirectoryEntryMetadata.LongFilenameEntryMetadata.LongName1, a1);
                        SetLongFilenameEntryMetadataValue(entry, FatDirectoryEntryMetadata.LongFilenameEntryMetadata.LongName2, a2);
                        SetLongFilenameEntryMetadataValue(entry, FatDirectoryEntryMetadata.LongFilenameEntryMetadata.LongName3, a3);
                    }
                }

                var fullPath = Path.Combine(FullPath, name);
                var firstCluster = ((FatFileSystem)FileSystem).GetFat(0).GetNextUnallocatedFatEntry();
                var entryHeaderDataOffset = directoryEntriesToAllocate == null ? GetNextUnallocatedDirectoryEntry() : directoryEntriesToAllocate[directoryEntriesToAllocate.Length - 1];

                var newEntry = new FatDirectoryEntry((FatFileSystem)FileSystem, this, fullPath, name, 0, firstCluster, entryHeaderDataOffset, entryType);

                newEntry.AllocateDirectoryEntry(shortName);

                return newEntry;
            }

            throw new ArgumentOutOfRangeException(nameof(entryType), "Unknown directory entry type.");
        }

        bool IsRootDirectory() => Parent == null;

        public void DeleteDirectoryEntry()
        {
            if (EntryType == DirectoryEntryType.Unknown)
                throw new NotImplementedException();

            if (IsRootDirectory())
                throw new Exception("Root directory can not be deleted");

            var data = ((FatDirectoryEntry)Parent).GetDirectoryEntryData();
            var entryOffset = EntryHeaderDataOffset - 32;

            while (data[entryOffset + 11] == FatDirectoryEntryAttributes.LongName)
            {
                data[entryOffset] = FatDirectoryEntryAttributes.UnusedOrDeletedEntry;
                entryOffset -= 32;
            }

            ((FatDirectoryEntry)Parent).SetDirectoryEntryData(data);

            SetDirectoryEntryMetadataValue(FatDirectoryEntryMetadata.FirstByte, FatDirectoryEntryAttributes.UnusedOrDeletedEntry);

            // GetFatTable calls GetFatChain, which "refreshes" the FAT table and clusters
            GetFatTable();
        }

        /// <summary>
        /// Retrieves a <see cref="List{T}"/> of <see cref="FatDirectoryEntry"/> objects that represent the Directory Entries inside this Directory
        /// </summary>
        /// <returns>Returns a <see cref="List{T}"/> of the Directory Entries inside this Directory</returns>
        public List<FatDirectoryEntry> ReadDirectoryContents(bool returnShortFilenames = false)
        {
            var data = GetDirectoryEntryData();
            var result = new List<FatDirectoryEntry>();
            FatDirectoryEntry parent = this;

            //TODO: Change longName to StringBuilder
            var longName = string.Empty;
            var name = string.Empty;

            for (var i = 0; i < data.Length; i += 32)
            {
                var attrib = data[i + 11];
                var status = data[i];

                if (attrib == FatDirectoryEntryAttributes.LongName)
                {
                    var type = data[i + 12];

                    if (returnShortFilenames)
                        continue;

                    if (status == FatDirectoryEntryAttributes.UnusedOrDeletedEntry)
                    {
                        Debugger.Log(0, "FileSystem", $"<DELETED> : Attrib = {attrib}, Status = {status}");
                        continue;
                    }

                    if (type == 0)
                    {
                        if ((status & 0x40) > 0)
                            longName = string.Empty;

                        // TODO: Check LDIR_Ord for ordering and throw exception if entries are found out of order.
                        // Also save buffer and only copy name if a end Ord marker is found.
                        var longPart = Encoding.Unicode.GetString(data, i + 1, 10);

                        // We have to check the length because 0xFFFF is a valid Unicode codepoint. So we only want to stop if the 0xFFFF is AFTER a 0x0000. We can determin
                        // this by also looking at the length. Since we short circuit the or, the length is rarely evaluated.
                        if (BitConverter.ToUInt16(data, i + 14) != 0xFFFF || longPart.Length == 5)
                        {
                            longPart += Encoding.Unicode.GetString(data, i + 14, 12);
                            if (BitConverter.ToUInt16(data, i + 28) != 0xFFFF || longPart.Length == 11)
                                longPart += Encoding.Unicode.GetString(data, i + 28, 4);
                        }

                        longName = longPart + longName;
                        longPart = null;
                        //TODO: LDIR_Chksum
                    }
                }
                else
                {
                    name = longName;

                    if (status == 0x00)
                    {
                        Debugger.Log(0, "FileSystem", $"<EOF> : Attrib = {attrib}, Status = {status}");
                        break;
                    }

                    switch (status)
                    {
                        case 0x05: break; // Japanese characters - We dont handle these
                        case 0x2E: continue; // Dot entry
                        case FatDirectoryEntryAttributes.UnusedOrDeletedEntry: continue; // Empty slot, skip it
                        default:
                            var test = attrib & (FatDirectoryEntryAttributes.Directory | FatDirectoryEntryAttributes.VolumeID);

                            if (status >= 0x20)
                                if (longName.Length > 0)
                                {
                                    // Leading and trailing spaces are to be ignored according to spec.
                                    // Many programs (including Windows) pad trailing spaces although it is not required for long names.
                                    // As per spec, ignore trailing periods
                                    name = longName.Trim(new[] { '\0', '\uffff' }).Trim();

                                    // If there are trailing periods
                                    var nameIndex = name.Length - 1;
                                    if (name[nameIndex] == '.')
                                    {
                                        // Search backwards till we find the first non-period character
                                        for (; nameIndex > 0; nameIndex--)
                                            if (name[nameIndex] != '.')
                                                break;

                                        // Substring to remove the periods
                                        name = name.Substring(0, nameIndex + 1);
                                    }
                                    longName = string.Empty;
                                }
                                else if (test == 0)
                                {
                                    var entry = Encoding.ASCII.GetString(data, i, 11);
                                    name = entry.Substring(0, 8).TrimEnd();
                                    var ext = entry.Substring(8, 3).TrimEnd();
                                    if (ext.Length > 0)
                                        name = name + "." + ext;
                                }
                                else
                                    name = Encoding.ASCII.GetString(data, i, 11).TrimEnd();

                            var firstCluster = (uint)(BitConverter.ToUInt16(data, i + 20) << 16 | BitConverter.ToUInt16(data, i + 26));
                            if (test == 0)
                            {
                                var size = BitConverter.ToUInt32(data, i + 28);
                                if (size == 0 && name.Length == 0)
                                    continue;

                                var fullPath = Path.Combine(FullPath, name);
                                var entry = new FatDirectoryEntry(((FatFileSystem)FileSystem), parent, fullPath, name, size, firstCluster, (uint)i, DirectoryEntryType.File);
                                result.Add(entry);
                            }
                            else if (test == FatDirectoryEntryAttributes.Directory)
                            {
                                var fullPath = Path.Combine(FullPath, name);
                                var size = BitConverter.ToUInt32(data, (int)i + 28);
                                var entry = new FatDirectoryEntry(((FatFileSystem)FileSystem), parent, fullPath, name, size, firstCluster, (uint)i, DirectoryEntryType.Directory);
                                result.Add(entry);
                            }
                            else if (test == FatDirectoryEntryAttributes.VolumeID)
                                Debugger.Log(0, "FileSystem", $"<VOLUME ID> : Attrib = {attrib}, Status = {status}");
                            else
                                Debugger.Log(0, "FileSystem", $"<INVALID ENTRY> : Attrib = {attrib}, Status = {status}");
                            break;
                    }
                }
            }
            return result;
        }

        public FatDirectoryEntry FindVolumeId()
        {
            if (!IsRootDirectory())
                throw new Exception("VolumeId can be found only in Root Directory");

            var data = GetDirectoryEntryData();
            var parent = this;

            FatDirectoryEntry result = null;
            for (var i = 0; i < data.Length; i += 32)
            {
                var attrib = data[i + 11];

                if (attrib != FatDirectoryEntryAttributes.VolumeID)
                    continue;

                // The Label in FAT could be only a shortName (limited to 11 characters) so it is more easy
                var name = Encoding.ASCII.GetString(data, i, 11);
                name = name.TrimEnd();

                var fullPath = Path.Combine(FullPath, name);
                // Probably can be OK to hardcode 0 here
                var size = BitConverter.ToUInt32(data, i + 28);
                //var firstCluster = (uint)(data.ToUInt16(i + 20) << 16 | data.ToUInt16(i + 26));
                var firstCluster = parent.FirstClusterNum;

                result = new FatDirectoryEntry(((FatFileSystem)FileSystem), parent, fullPath, name, size, firstCluster, (uint)i, DirectoryEntryType.File);
                break;
            }

            if (result == null)
                Debugger.Log(0, "FileSystem", $"VolumeID not found, returning null");
            return result;
        }

        public FatDirectoryEntry CreateVolumeId(string name)
        {
            if (!IsRootDirectory())
                throw new Exception("VolumeId can be created only in Root Directory");

            // VolumeId is really a special type of File with attribute 'VolumeID' set
            var volumeId = AddDirectoryEntry(name, DirectoryEntryType.File);
            volumeId.SetDirectoryEntryMetadataValue(FatDirectoryEntryMetadata.Attributes, FatDirectoryEntryAttributes.VolumeID);
            return volumeId;
        }

        /// <summary>
        /// Tries to find an empty space for a directory entry and returns the offset to that space if successful, otherwise throws an exception.
        /// </summary>
        /// <returns>Returns the offset to the next unallocated directory entry.</returns>
        uint GetNextUnallocatedDirectoryEntry()
        {
            var data = GetDirectoryEntryData();
            for (var i = 0; i < data.Length; i += 32)
            {
                var x1 = BitConverter.ToUInt32(data, i);
                var x2 = BitConverter.ToUInt32(data, i + 8);
                var x3 = BitConverter.ToUInt32(data, i + 16);
                var x4 = BitConverter.ToUInt32(data, i + 24);
                if (x1 == 0 && x2 == 0 && x3 == 0 && x4 == 0)
                    return (uint)i;
            }

            // TODO: What should we return if no available entry is found. - Update Method description above.
            throw new Exception("Failed to find an unallocated directory entry.");
        }

        /// <summary>
        /// Tries to find an empty space for the specified number of directory entries and returns an array of offsets to those spaces if successful, otherwise throws an exception.
        /// </summary>
        /// <param name="entryCount">The number of entried to allocate.</param>
        /// <returns>Returns an array of offsets to the next unallocated directory entries.</returns>
        uint[] GetNextUnallocatedDirectoryEntries(int entryCount)
        {
            var data = GetDirectoryEntryData();
            var count = 0;
            var entries = new uint[entryCount];

            for (var i = 0; i < data.Length; i += 32)
            {
                var x1 = BitConverter.ToUInt32(data, i);
                var x2 = BitConverter.ToUInt32(data, i + 8);
                var x3 = BitConverter.ToUInt32(data, i + 16);
                var x4 = BitConverter.ToUInt32(data, i + 24);
                if (x1 == 0 && x2 == 0 && x3 == 0 && x4 == 0)
                {
                    entries[count] = (uint)i;
                    count++;

                    if (entryCount == count)
                        return entries;
                }
                else
                    count = 0;
            }

            // TODO: What should we return if no available entry is found. - Update Method description above.
            throw new Exception($"Failed to find {entryCount} unallocated directory entries.");
        }

        byte[] GetDirectoryEntryData()
        {
            if (EntryType != DirectoryEntryType.Unknown)
            {
                ((FatFileSystem)FileSystem).Read(FirstClusterNum, out var data);
                return data;
            }
            throw new Exception("Invalid directory entry type");
        }

        void SetDirectoryEntryData(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.Length == 0)
                throw new ArgumentException("aData does not contain any data.", nameof(data));

            if (EntryType != DirectoryEntryType.Unknown)
                ((FatFileSystem)FileSystem).Write(FirstClusterNum, data);
            else
                throw new Exception("Invalid directory entry type");
        }

        internal void SetDirectoryEntryMetadataValue(FatDirectoryEntryMetadata entryMetadata, byte value)
        {
            if (IsRootDirectory())
                throw new Exception("Root directory metadata can not be changed using the file stream.");

            var data = ((FatDirectoryEntry)Parent).GetDirectoryEntryData();
            if (data.Length > 0)
            {
                var offset = EntryHeaderDataOffset + entryMetadata.DataOffset;
                data[offset] = value;
                ((FatDirectoryEntry)Parent).SetDirectoryEntryData(data);
            }
        }

        internal void SetDirectoryEntryMetadataValue(FatDirectoryEntryMetadata entryMetadata, ushort value)
        {
            if (IsRootDirectory())
                throw new Exception("Root directory metadata can not be changed using the file stream.");

            var data = ((FatDirectoryEntry)Parent).GetDirectoryEntryData();
            if (data.Length > 0)
            {
                var value2 = new byte[entryMetadata.DataLength];
                value2.SetUInt16(0, value);
                var offset = EntryHeaderDataOffset + entryMetadata.DataOffset;
                Array.Copy(value2, 0, data, offset, entryMetadata.DataLength);
                ((FatDirectoryEntry)Parent).SetDirectoryEntryData(data);
            }
        }

        internal void SetDirectoryEntryMetadataValue(FatDirectoryEntryMetadata entryMetadata, uint value)
        {
            if (IsRootDirectory())
                throw new Exception("Root directory metadata can not be changed using the file stream.");

            var data = ((FatDirectoryEntry)Parent).GetDirectoryEntryData();
            if (data.Length > 0)
            {
                var value2 = new byte[entryMetadata.DataLength];
                value2.SetUInt32(0, value);
                uint offset = EntryHeaderDataOffset + entryMetadata.DataOffset;
                Array.Copy(value2, 0, data, offset, entryMetadata.DataLength);
                ((FatDirectoryEntry)Parent).SetDirectoryEntryData(data);
            }
        }

        internal void SetDirectoryEntryMetadataValue(FatDirectoryEntryMetadata entryMetadata, long value)
        {
            if (IsRootDirectory())
                throw new Exception("Root directory metadata can not be changed using the file stream.");

            var data = ((FatDirectoryEntry)Parent).GetDirectoryEntryData();
            if (data.Length > 0)
            {
                var value2 = new byte[entryMetadata.DataLength];
                value2.SetUInt32(0, (uint)value);
                var offset = EntryHeaderDataOffset + entryMetadata.DataOffset;
                Array.Copy(value2, 0, data, offset, entryMetadata.DataLength);
                ((FatDirectoryEntry)Parent).SetDirectoryEntryData(data);
            }
        }

        internal void SetDirectoryEntryMetadataValue(FatDirectoryEntryMetadata entryMetadata, string value)
        {
            if (IsRootDirectory())
                throw new Exception("Root directory metadata can not be changed using the file stream.");

            var data = ((FatDirectoryEntry)Parent).GetDirectoryEntryData();
            if (data.Length > 0)
            {
                var value2 = new byte[entryMetadata.DataLength];
                var value3 = Encoding.UTF8.GetBytes(value);
                for (var i = 0; i < value2.Length; i++)
                {
                    if (i < value3.Length) value2[i] = value3[i];
                    else value2[i] = 32;
                }
                var offset = EntryHeaderDataOffset + entryMetadata.DataOffset;
                Array.Copy(value2, 0, data, offset, entryMetadata.DataLength);
                ((FatDirectoryEntry)Parent).SetDirectoryEntryData(data);
            }
        }

        internal void SetLongFilenameEntryMetadataValue(uint entryHeaderDataOffset, FatDirectoryEntryMetadata entryMetadata, uint value)
        {
            var data = GetDirectoryEntryData();
            if (data.Length > 0)
            {
                var value2 = new byte[entryMetadata.DataLength];
                value2.SetUInt32(0, value);
                var offset = entryHeaderDataOffset + entryMetadata.DataOffset;
                Array.Copy(value2, 0, data, (int)offset, (int)entryMetadata.DataLength);
                SetDirectoryEntryData(data);
            }
        }

        internal void SetLongFilenameEntryMetadataValue(uint entryHeaderDataOffset, FatDirectoryEntryMetadata entryMetadata, long value)
        {
            var data = GetDirectoryEntryData();
            if (data.Length > 0)
            {
                var value2 = new byte[entryMetadata.DataLength];
                value2.SetUInt32(0, (uint)value);
                var offset = entryHeaderDataOffset + entryMetadata.DataOffset;
                Array.Copy(value2, 0, data, (int)offset, (int)entryMetadata.DataLength);
                SetDirectoryEntryData(data);
            }
        }

        internal void SetLongFilenameEntryMetadataValue(uint entryHeaderDataOffset, FatDirectoryEntryMetadata entryMetadata, string value)
        {
            var data = GetDirectoryEntryData();
            if (data.Length > 0)
            {
                var value2 = Encoding.Unicode.GetBytes(value);
                var offset = entryHeaderDataOffset + entryMetadata.DataOffset;
                Array.Copy(value2, 0, data, (int)offset, (int)entryMetadata.DataLength);
                SetDirectoryEntryData(data);
            }
        }

        /// <summary>
        /// Gets the short filename to be written to the FAT directory entry.
        /// </summary>
        /// <param name="shortName">The short filename.</param>
        /// <returns>Returns the short filename to be written to the FAT directory entry.</returns>
        internal static string GetShortName(string shortName)
        {
            var name = new char[11];
            for (var i = 0; i < name.Length; i++)
                name[i] = (char)0x20;

            var j = 0;
            for (var i = 0; i < shortName.Length; i++)
            {
                if (shortName[i] == '.') { i++; j = 8; }
                if (i > name.Length) break;
                name[j] = shortName[i];
                j++;
            }
            return new string(name);
        }

        /// <summary>
        /// Calculates the checksum for a given short filename.
        /// </summary>
        /// <param name="shortName">The short filename without the extension period.</param>
        /// <returns>Returns the checksum for the given short filename.</returns>
        internal static uint CalculateChecksum(string shortName)
        {
            var checksum = 0U;
            for (var i = 0; i < 11; i++)
                checksum = (((checksum & 1) << 7) | ((checksum & 0xFE) >> 1)) + shortName[i];
            return checksum;
        }

        long GetDirectoryEntrySize(byte[] directoryEntryData)
        {
            var result = 0L;
            for (var i = 0; i < directoryEntryData.Length; i += 32)
            {
                var attrib = directoryEntryData[i + 11];
                var status = directoryEntryData[i];

                if (attrib == FatDirectoryEntryAttributes.LongName)
                    continue;

                if (status == 0x00)
                    break;

                switch (status)
                {
                    case 0x05: continue; // Japanese characters - We dont handle these
                    case 0x2E: continue; // Dot entry
                    case FatDirectoryEntryAttributes.UnusedOrDeletedEntry: continue; // Empty slot, skip it
                    default: break;
                }

                var test = attrib & (FatDirectoryEntryAttributes.Directory | FatDirectoryEntryAttributes.VolumeID);

                switch (test)
                {
                    case 0: // Normal file
                        var size = BitConverter.ToUInt32(directoryEntryData, i + 28);
                        result += size;
                        break;
                    case FatDirectoryEntryAttributes.Directory:
                        var firstCluster = (uint)(BitConverter.ToUInt16(directoryEntryData, i + 20) << 16 | BitConverter.ToUInt16(directoryEntryData, i + 26));
                        ((FatFileSystem)FileSystem).Read(firstCluster, out var dirData);
                        result += GetDirectoryEntrySize(dirData);
                        break;
                    case FatDirectoryEntryAttributes.VolumeID: continue;
                    default: continue;
                }
            }
            return result;
        }

        /*
         * Please note that this could become slower and slower as the partion becomes greater this could be optimized in two ways:
         * 1. Compute the value using this function on FS inizialization and write the difference between TotalSpace and the computed
         *    value to the specif field of 'FS Information Sector' of FAT32
         * 2. Compute the value using this function on FS inizialization and write the difference between TotalSpace and the computed
         *    value in a sort of memory cache in VFS itself
         *
         *    In any case if one of this two methods will be used in the future when a file is removed or new data are written on it,
         *    the value on the field should be always updated.
         */
        public override long GetUsedSpace()
        {
            var result = 0L;
            var data = GetDirectoryEntryData();
            result += GetDirectoryEntrySize(data);
            return result;
        }
    }
}
