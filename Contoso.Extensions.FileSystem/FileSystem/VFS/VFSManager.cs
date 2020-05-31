using System;
using System.Collections.Generic;
using System.IO;

namespace Contoso.Extensions.FileSystem.VFS
{
    public static class VFSManager
    {
        private static VFSBase VFS;

        public static void RegisterVFS(VFSBase vfs)
        {
            if (VFS != null)
                throw new Exception("Virtual File System Manager already initialized!");

            vfs.Initialize();
            VFS = vfs;
        }

        public static DirectoryEntry CreateFile(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            return VFS.CreateFile(path);
        }

        public static void DeleteFile(string path)
        {
            if (VFS == null)
                throw new Exception("VFSManager isn't ready.");

            var file = VFS.GetFile(path);

            if (file.EntryType != DirectoryEntryType.File)
                throw new UnauthorizedAccessException("The specified path isn't a file");

            VFS.DeleteFile(file);
        }

        public static DirectoryEntry GetFile(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            var fileName = Path.GetFileName(path);
            var directory = path.Remove(path.Length - fileName.Length);

            var lastChar = directory[directory.Length - 1];
            if (lastChar != Path.DirectorySeparatorChar)
                directory += Path.DirectorySeparatorChar;

            var list = GetDirectoryListing(directory);
            for (var i = 0; i < list.Count; i++)
            {
                var entry = list[i];
                if (entry != null && entry.EntryType == DirectoryEntryType.File && string.Equals(entry.Name, fileName, StringComparison.OrdinalIgnoreCase))
                    return entry;
            }
            return null;
        }

        public static DirectoryEntry CreateDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            return VFS.CreateDirectory(path);
        }

        public static void DeleteDirectory(string path, bool recursive)
        {
            if (VFS == null)
                throw new Exception("VFSManager isn't ready.");

            var directory = VFS.GetDirectory(path);
            var directoryListing = VFS.GetDirectoryListing(directory);

            if (directory.EntryType != DirectoryEntryType.Directory)
                throw new IOException("The specified path isn't a directory");

            if (directoryListing.Count > 0 && !recursive)
                throw new IOException("Directory is not empty");

            if (recursive)
                foreach (var entry in directoryListing)
                    if (entry.EntryType == DirectoryEntryType.Directory) DeleteDirectory(entry.FullPath, true);
                    else if (entry.EntryType == DirectoryEntryType.File) DeleteFile(entry.FullPath);
                    else throw new IOException("The directory contains a corrupted file");

            VFS.DeleteDirectory(directory);
        }

        public static DirectoryEntry GetDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            return VFS.GetDirectory(path);
        }

        public static List<DirectoryEntry> GetDirectoryListing(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            return VFS.GetDirectoryListing(path);
        }

        public static DirectoryEntry GetVolume(string volume)
        {
            if (string.IsNullOrEmpty(volume))
                throw new ArgumentNullException(nameof(volume));

            return VFS.GetVolume(volume);
        }

        public static List<DirectoryEntry> GetVolumes() => VFS.GetVolumes();

        public static void RegisterFileSystem(FileSystemFactory fileSystemFactory) => VFS.RegisterFileSystem(fileSystemFactory);

        public static List<string> GetLogicalDrives()
        {
            var drives = new List<string>();
            foreach (var entry in GetVolumes())
                drives.Add($"{entry.Name}{Path.VolumeSeparatorChar}{Path.DirectorySeparatorChar}");
            return drives;
        }

        //public static List<string> InternalGetFileDirectoryNames(string path, string userPathOriginal, string searchPattern, bool includeFiles, bool includeDirs, SearchOption searchOption)
        //{
        //    return null;

        //    /*
        //    //TODO: Add SearchOption functionality
        //    //TODO: What is userPathOriginal?
        //    //TODO: Add SearchPattern functionality
        //    List<string> xFileAndDirectoryNames = new List<string>();
        //    //Validate input arguments
        //    if ((searchOption != SearchOption.TopDirectoryOnly) && (searchOption != SearchOption.AllDirectories))
        //        throw new ArgumentOutOfRangeException("searchOption");
        //    searchPattern = searchPattern.TrimEnd(new char[0]);
        //    if (searchPattern.Length == 0)
        //        return new string[0];
        //    //Perform search in filesystem
        //    FilesystemEntry[] xEntries = GetDirectoryListing(path);
        //    foreach (FilesystemEntry xEntry in xEntries)
        //    {
        //        if (xEntry.IsDirectory && includeDirs)
        //            xFileAndDirectoryNames.Add(xEntry.Name);
        //        else if (!xEntry.IsDirectory && includeFiles)
        //            xFileAndDirectoryNames.Add(xEntry.Name);
        //    }
        //    return xFileAndDirectoryNames.ToArray();
        //     */
        //}

        public static bool FileExists(string path)
        {
            if (path == null)
                return false;

            try
            {
                path = Path.GetFullPath(path);
                return GetFile(path) != null;
            }
            catch (Exception e)
            {
                Console.Write($"Exception occurred: {e.Message}");
                return false;
            }
        }

        public static bool FileExists(DirectoryEntry entry)
        {
            if (entry == null)
                return false;

            try
            {
                var path = GetFullPath(entry);
                return GetFile(path) != null;
            }
            catch { return false; }
        }

        public static bool DirectoryExists(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            try
            {
                path = Path.GetFullPath(path);
                return GetDirectory(path) != null;
            }
            catch { return false; }
        }

        public static bool DirectoryExists(DirectoryEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            try
            {
                var path = GetFullPath(entry);
                return GetDirectory(path) != null;
            }
            catch (Exception e)
            {
                Console.Write($"Exception occurred: {e.Message}");
                return false;
            }
        }

        public static string GetFullPath(DirectoryEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            var parent = entry.Parent;
            var path = entry.Name;

            while (parent != null)
            {
                path = parent.Name + path;
                parent = parent.Parent;
            }
            return path;
        }

        public static Stream GetFileStream(string pathname)
        {
            if (string.IsNullOrEmpty(pathname))
                return null;

            var fileInfo = GetFile(pathname);
            if (fileInfo == null)
                throw new Exception($"File not found: {pathname}");

            return fileInfo.GetFileStream();
        }

        public static FileAttributes GetFileAttributes(string path) => VFS.GetFileAttributes(path);

        public static void SetFileAttributes(string path, FileAttributes fileAttributes) => VFS.SetFileAttributes(path, fileAttributes);

        public static bool IsValidDriveId(string path) => VFS.IsValidDriveId(path);

        public static long GetTotalSize(string driveId) => VFS.GetTotalSize(driveId);

        public static long GetAvailableFreeSpace(string driveId) => VFS.GetAvailableFreeSpace(driveId);

        public static long GetTotalFreeSpace(string driveId) => VFS.GetTotalFreeSpace(driveId);

        public static string GetFileSystemType(string driveId) => VFS.GetFileSystemType(driveId);

        public static string GetFileSystemLabel(string driveId) => VFS.GetFileSystemLabel(driveId);

        public static void SetFileSystemLabel(string driveId, string label) => VFS.SetFileSystemLabel(driveId, label);

        public static void Format(string driveId, string driveFormat, bool quick) => VFS.Format(driveId, driveFormat, quick);

        /// <summary>
        /// Gets the parent directory entry from the path.
        /// </summary>
        /// <param name="path">The full path to the current directory entry.</param>
        /// <returns>The parent directory entry.</returns>
        /// <exception cref="ArgumentException">Argument is null or empty</exception>
        /// <exception cref="NotImplementedException"></exception>
        public static DirectoryEntry GetParent(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            if (path == Path.GetPathRoot(path))
                return null;

            var fileOrDirectory = Path.HasExtension(path) ? Path.GetFileName(path) : Path.GetDirectoryName(path);
            if (fileOrDirectory != null)
            {
                path = path.Remove(path.Length - fileOrDirectory.Length);
                return GetDirectory(path);
            }
            return null;
        }

        #region Helpers

        public static char AltDirectorySeparatorChar = '/';

        public static char DirectorySeparatorChar = '\\';

        public static char[] InvalidFileNameChars = { '"', '<', '>', '|', '\0', '\a', '\b', '\t', '\n', '\v', '\f', '\r', ':', '*', '?', '\\', '/' };

        public static char[] InvalidPathCharsWithAdditionalChecks = { '"', '<', '>', '|', '\0', '\a', '\b', '\t', '\n', '\v', '\f', '\r', '*', '?' };

        public static char PathSeparator = ';';

        public static char[] RealInvalidPathChars = { '"', '<', '>', '|' };

        public static char[] TrimEndChars = { (char)0x9, (char)0xA, (char)0xB, (char)0xC, (char)0xD, (char)0x20, (char)0x85, (char)0xA0 };

        public static char VolumeSeparatorChar = ':';

        public static int MaxPath = 260;

        public static string[] SplitPath(string path) => path.Split(DirectorySeparators, StringSplitOptions.RemoveEmptyEntries);

        static char[] DirectorySeparators => new[] { DirectorySeparatorChar, AltDirectorySeparatorChar };

        #endregion
    }
}
