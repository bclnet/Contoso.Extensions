using System.Collections.Generic;
using System.IO;

namespace Contoso.Extensions.FileSystem.VFS
{
    public abstract class VFSBase
    {
        public abstract void Initialize();

        public abstract void RegisterFileSystem(FileSystemFactory fileSystemFactory);

        public abstract DirectoryEntry CreateFile(string path);

        public abstract DirectoryEntry CreateDirectory(string path);

        public abstract bool DeleteFile(DirectoryEntry path);

        public abstract bool DeleteDirectory(DirectoryEntry path);

        public abstract DirectoryEntry GetDirectory(string path);

        public abstract DirectoryEntry GetFile(string path);

        public abstract List<DirectoryEntry> GetDirectoryListing(string path);

        public abstract List<DirectoryEntry> GetDirectoryListing(DirectoryEntry entry);

        public abstract DirectoryEntry GetVolume(string volume);

        public abstract List<DirectoryEntry> GetVolumes();

        public abstract FileAttributes GetFileAttributes(string path);

        public abstract void SetFileAttributes(string path, FileAttributes fileAttributes);

        public static char DirectorySeparatorChar => '\\';

        public static char AltDirectorySeparatorChar => '/';

        public static char VolumeSeparatorChar => ':';

        public abstract bool IsValidDriveId(string driveId);

        public abstract long GetTotalSize(string driveId);

        public abstract long GetAvailableFreeSpace(string driveId);

        public abstract long GetTotalFreeSpace(string driveId);

        public abstract string GetFileSystemType(string driveId);

        public abstract string GetFileSystemLabel(string driveId);

        public abstract void SetFileSystemLabel(string driveId, string label);

        public abstract void Format(string driveId, string driveFormat, bool quick);
    }
}
