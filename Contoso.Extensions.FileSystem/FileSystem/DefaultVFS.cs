using Contoso.Extensions.FileSystem.VFS;
using Contoso.Extensions.HAL.BlockDevice;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Contoso.Extensions.FileSystem
{
    /// <summary>
    /// default virtual file system.
    /// </summary>
    /// <seealso cref="Contoso.Extensions.FileSystem.VFS.VFSBase" />
    public class DefaultVFS : VFSBase
    {
        List<Partition> Partitions;
        List<FileSystem> FileSystems;
        FileSystem CurrentFileSystem;
        List<FileSystemFactory> RegisteredFileSystems;

        /// <summary>
        /// Initializes the virtual file system.
        /// </summary>
        public override void Initialize()
        {
            Partitions = new List<Partition>();
            FileSystems = new List<FileSystem>();
            RegisteredFileSystems = new List<FileSystemFactory>();

            RegisterFileSystem(new FatFileSystemFactory());

            InitializePartitions();
            if (Partitions.Count > 0)
                InitializeFileSystems();
        }

        public override void RegisterFileSystem(FileSystemFactory fileSystemFactory) => RegisteredFileSystems.Add(fileSystemFactory);

        /// <summary>
        /// Creates a new file.
        /// </summary>
        /// <param name="path">The full path including the file to create.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException">aPath</exception>
        public override DirectoryEntry CreateFile(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            if (File.Exists(path))
                return GetFile(path);

            var fileToCreate = Path.GetFileName(path);
            var parentDirectory = Path.GetDirectoryName(path);
            var parentEntry = GetDirectory(parentDirectory);
            if (parentEntry == null)
                parentEntry = CreateDirectory(parentDirectory);

            return GetFileSystemFromPath(parentDirectory).CreateFile(parentEntry, fileToCreate);
        }

        /// <summary>
        /// Creates a directory.
        /// </summary>
        /// <param name="path">The full path including the directory to create.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException">aPath</exception>
        public override DirectoryEntry CreateDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            if (Directory.Exists(path))
                return GetDirectory(path);

            path = path.TrimEnd(DirectorySeparatorChar, AltDirectorySeparatorChar);

            var directoryToCreate = Path.GetFileName(path);

            var parentDirectory = Path.GetDirectoryName(path);
            var parentEntry = GetDirectory(parentDirectory) ?? CreateDirectory(parentDirectory);

            return GetFileSystemFromPath(parentDirectory).CreateDirectory(parentEntry, directoryToCreate);
        }

        /// <summary>
        /// Deletes a file.
        /// </summary>
        /// <param name="path">The full path.</param>
        /// <returns></returns>
        public override bool DeleteFile(DirectoryEntry path)
        {
            try
            {
                GetFileSystemFromPath(path.FullPath).DeleteFile(path);
                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// Deletes an empty directory.
        /// </summary>
        /// <param name="path">The full path.</param>
        /// <returns></returns>
        public override bool DeleteDirectory(DirectoryEntry path)
        {
            try
            {
                if (GetDirectoryListing(path).Count > 0)
                    throw new Exception("Directory is not empty");

                GetFileSystemFromPath(path.FullPath).DeleteDirectory(path);
                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// Gets the directory listing for a path.
        /// </summary>
        /// <param name="path">The full path.</param>
        /// <returns></returns>
        public override List<DirectoryEntry> GetDirectoryListing(string path)
        {
            var fs = GetFileSystemFromPath(path);
            var directory = DoGetDirectoryEntry(path, fs);
            return fs.GetDirectoryListing(directory);
        }

        /// <summary>
        /// Gets the directory listing for a directory entry.
        /// </summary>
        /// <param name="directory">The directory entry.</param>
        /// <returns></returns>
        public override List<DirectoryEntry> GetDirectoryListing(DirectoryEntry directory)
        {
            if (directory == null || string.IsNullOrEmpty(directory.FullPath))
                throw new ArgumentNullException(nameof(directory));

            return GetDirectoryListing(directory.FullPath);
        }

        /// <summary>
        /// Gets the directory entry for a directory.
        /// </summary>
        /// <param name="path">The full path path.</param>
        /// <returns>A directory entry for the directory.</returns>
        /// <exception cref="Exception"></exception>
        public override DirectoryEntry GetDirectory(string path)
        {
            try
            {
                var fs = GetFileSystemFromPath(path);
                var entry = DoGetDirectoryEntry(path, fs);
                if (entry != null && entry.EntryType == DirectoryEntryType.Directory)
                    return entry;
            }
            catch (Exception)
            {
                Debugger.Log(0, "FileSystem", $"DoGetDirectoryEntry failed, returning null. Path = {path}");
                return null;
            }
            throw new Exception($"{path} was found, but is not a directory.");
        }

        /// <summary>
        /// Gets the directory entry for a file.
        /// </summary>
        /// <param name="path">The full path.</param>
        /// <returns>A directory entry for the file.</returns>
        /// <exception cref="Exception"></exception>
        public override DirectoryEntry GetFile(string path)
        {
            try
            {
                var fs = GetFileSystemFromPath(path);
                var entry = DoGetDirectoryEntry(path, fs);
                if (entry != null && entry.EntryType == DirectoryEntryType.File)
                    return entry;
            }
            catch (Exception)
            {
                Debugger.Log(0, "FileSystem", $"DoGetDirectoryEntry failed, returning null. Path = {path}");
                return null;
            }
            throw new Exception(path + " was found, but is not a file.");
        }

        /// <summary>
        /// Gets the volumes for all registered file systems.
        /// </summary>
        /// <returns>A list of directory entries for all volumes.</returns>
        public override List<DirectoryEntry> GetVolumes()
        {
            var volumes = new List<DirectoryEntry>();
            for (var i = 0; i < FileSystems.Count; i++)
                volumes.Add(GetVolume(FileSystems[i]));
            return volumes;
        }

        /// <summary>
        /// Gets the directory entry for a volume.
        /// </summary>
        /// <param name="aPath">The volume root path.</param>
        /// <returns>A directory entry for the volume.</returns>
        public override DirectoryEntry GetVolume(string aPath)
        {
            if (string.IsNullOrEmpty(aPath))
                return null;

            var fs = GetFileSystemFromPath(aPath);
            return fs != null ? GetVolume(fs) : null;
        }

        /// <summary>
        /// Gets the attributes for a File / Directory.
        /// </summary>
        /// <param name="path">The path of the File / Directory.</param>
        /// <returns>The File / Directory attributes.</returns>
        public override FileAttributes GetFileAttributes(string path)
        {
            // We are limiting ourselves to the simpler attributes File and Directory for now. I think that in the end FAT does not support anything else
            var fs = GetFileSystemFromPath(path);
            var entry = DoGetDirectoryEntry(path, fs);
            if (entry == null)
                throw new Exception($"{path} is neither a file neither a directory");

            switch (entry.EntryType)
            {
                case DirectoryEntryType.File: return FileAttributes.Normal;
                case DirectoryEntryType.Directory: return FileAttributes.Directory;
                case DirectoryEntryType.Unknown:
                default: throw new Exception($"{path} is neither a file neither a directory");
            }
        }

        /// <summary>
        /// Sets the attributes for a File / Directory.
        /// </summary>
        /// <param name="path">The path of the File / Directory.</param>
        /// <param name="fileAttributes">The attributes of the File / Directory.</param>
        public override void SetFileAttributes(string path, FileAttributes fileAttributes) => throw new NotImplementedException();

        /// <summary>
        /// Initializes the partitions for all block devices.
        /// </summary>
        protected virtual void InitializePartitions()
        {
            for (var i = 0; i < BlockDevice.Devices.Count; i++)
                if (BlockDevice.Devices[i] is Partition p)
                    Partitions.Add(p);

            if (Partitions.Count > 0)
                for (var i = 0; i < Partitions.Count; i++)
                    Console.WriteLine(
$@"Partition #: {i + 1}
Block Size: {Partitions[i].BlockSize} bytes
Block Count: {Partitions[i].BlockCount}
Size: {Partitions[i].BlockCount * Partitions[i].BlockSize / 1024 / 1024} MB");
            else
                Console.WriteLine("No partitions found!");
        }

        /// <summary>
        /// Initializes the file system for all partitions.
        /// </summary>
        protected virtual void InitializeFileSystems()
        {
            for (var i = 0; i < Partitions.Count; i++)
            {
                var rootPath = string.Concat(i, VolumeSeparatorChar, DirectorySeparatorChar);
                var size = (long)(Partitions[i].BlockCount * Partitions[i].BlockSize / 1024 / 1024);

                // We 'probe' the partition <i> with all the FileSystem registered until we find a Filesystem that can read / write to it
                foreach (var fs in RegisteredFileSystems)
                    if (fs.IsType(Partitions[i]))
                        FileSystems.Add(fs.Create(Partitions[i], rootPath, size));

                if (FileSystems.Count > 0 && FileSystems[FileSystems.Count - 1].RootPath == rootPath)
                {
                    Console.WriteLine($"Initialized {FileSystems.Count} filesystem(s)...");
                    FileSystems[i].DisplayFileSystemInfo();
                    Directory.SetCurrentDirectory(rootPath);
                }
                else
                    Console.WriteLine("No filesystem found on partition #{i}");
            }
        }

        /// <summary>
        /// Gets the file system from a path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The file system for the path.</returns>
        /// <exception cref="Exception">Unable to determine filesystem for path:  + aPath</exception>
        FileSystem GetFileSystemFromPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            path = Path.GetPathRoot(path);

            if (CurrentFileSystem != null && path == CurrentFileSystem.RootPath)
                return CurrentFileSystem;

            for (var i = 0; i < FileSystems.Count; i++)
                if (FileSystems[i].RootPath == path)
                {
                    CurrentFileSystem = FileSystems[i];
                    return CurrentFileSystem;
                }
            throw new Exception($"Unable to determine filesystem for path: {path}");
        }

        /// <summary>
        /// Attempts to get a directory entry for a path in a file system.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="fs">The file system.</param>
        /// <returns>A directory entry for the path.</returns>
        /// <exception cref="ArgumentNullException">aFS</exception>
        /// <exception cref="Exception">Path part not found</exception>
        DirectoryEntry DoGetDirectoryEntry(string path, FileSystem fs)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            if (fs == null)
                throw new ArgumentNullException(nameof(fs));

            var pathParts = VFSManager.SplitPath(path);
            var baseDirectory = GetVolume(fs);

            if (pathParts.Length == 1)
                return baseDirectory;

            // start at index 1, because 0 is the volume
            for (var i = 1; i < pathParts.Length; i++)
            {
                var pathPart = pathParts[i].ToLowerInvariant();
                var partFound = false;
                var listing = fs.GetDirectoryListing(baseDirectory);
                for (var j = 0; j < listing.Count; j++)
                {
                    var listingItem = listing[j];
                    var listingItemName = listingItem.Name.ToLowerInvariant();
                    pathPart = pathPart.ToLowerInvariant();
                    if (listingItemName == pathPart)
                    {
                        baseDirectory = listingItem;
                        partFound = true;
                        break;
                    }
                }

                if (!partFound)
                    throw new Exception($"Path part '{pathPart}' not found");
            }
            return baseDirectory;
        }

        /// <summary>
        /// Gets the root directory entry for a volume in a file system.
        /// </summary>
        /// <param name="fs">The file system containing the volume.</param>
        /// <returns>A directory entry for the volume.</returns>
        DirectoryEntry GetVolume(FileSystem fs)
        {
            if (fs == null)
                throw new ArgumentNullException(nameof(fs));

            return fs.GetRootDirectory();
        }

        /// <summary>
        /// Verifies if driveId is a valid id for a drive.
        /// </summary>
        /// <param name="driveId">The id of the drive.</param>
        /// <returns>true if the drive id is valid, false otherwise.</returns>
        public override bool IsValidDriveId(string driveId)
        {
            // We need to remove ':\' to get only the numeric value
            driveId = driveId.Remove(driveId.Length - 2);

            // Drive name is really similar to DOS / Windows but a number instead of a letter is used, it is not limited to 1 character but any number is valid
            var ok = int.TryParse(driveId, out var _);
            return ok;
        }

        public override long GetTotalSize(string driveId) => GetFileSystemFromPath(driveId).Size * 1024 * 1024; // return in bytes

        public override long GetAvailableFreeSpace(string driveId) => GetFileSystemFromPath(driveId).AvailableFreeSpace;

        public override long GetTotalFreeSpace(string driveId) => GetFileSystemFromPath(driveId).TotalFreeSpace;

        public override string GetFileSystemType(string driveId) => GetFileSystemFromPath(driveId).Type;

        public override string GetFileSystemLabel(string driveId) => GetFileSystemFromPath(driveId).Label;

        public override void SetFileSystemLabel(string driveId, string label) => GetFileSystemFromPath(driveId).Label = label;

        public override void Format(string driveId, string driveFormat, bool quick) => GetFileSystemFromPath(driveId).Format(driveFormat, quick);
    }
}
