using Contoso.Extensions.FileSystem.VFS;
using System;

namespace Contoso.Extensions.FileSystem
{
    public class DiskManager
    {
        public string Name { get; }

        public DiskManager(string driveName)
        {
            if (driveName == null)
                throw new ArgumentNullException(nameof(driveName));
            if (!VFSManager.IsValidDriveId(driveName))
                throw new ArgumentException("Argument must be drive identifier or root dir");

            Name = driveName;
        }

        public void Format(string driveFormat, bool quick = true)
        {
            // For now we do the more easy thing: quick format of a drive with the same filesystem
            if (VFSManager.GetFileSystemType(Name) != driveFormat)
                throw new NotSupportedException($"Formatting in {driveFormat} drive {Name} with Filesystem {VFSManager.GetFileSystemType(Name)} not yet supported");
            if (!quick)
                throw new NotSupportedException("Slow format not implemented yet");

            VFSManager.Format(Name, driveFormat, quick);
        }

        public void ChangeDriveLetter(string newName)
        {
            if (newName == null)
                throw new ArgumentNullException(nameof(newName));
            if (!VFSManager.IsValidDriveId(newName))
                throw new ArgumentException("Argument must be drive identifier or root dir");

            throw new NotSupportedException("ChangeDriveLetter");
        }

        public void CreatePartion(long start, long end)
        {
            if (start < 0 || end < 0)
                throw new ArgumentOutOfRangeException(nameof(start));
            if (end <= start)
                throw new ArgumentOutOfRangeException(nameof(start), "end is <= start");

            throw new NotSupportedException("CreatePartion");
        }
    }
}
