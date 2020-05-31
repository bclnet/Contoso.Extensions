using System;
using System.IO;

namespace Contoso.Extensions.FileSystem
{
    /// <summary>
    /// Enumeration for the directory entry type.
    /// </summary>
    public enum DirectoryEntryType
    {
        /// <summary>
        /// Directory
        /// </summary>
        Directory,
        /// <summary>
        /// File
        /// </summary>
        File,
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown
    }

    /// <summary>
    /// A generic file system directory entry.
    /// </summary>
    public abstract class DirectoryEntry
    {
        public long Size;
        public string FullPath;
        public string Name;
        protected readonly FileSystem FileSystem;
        public readonly DirectoryEntry Parent;
        public readonly DirectoryEntryType EntryType;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryEntry"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system that contains the directory entry.</param>
        /// <param name="parent">The parent directory entry or null if the current entry is the root.</param>
        /// <param name="fullPath">The full path to the entry.</param>
        /// <param name="name">The entry name.</param>
        /// <param name="size">The size of the entry.</param>
        /// <param name="entryType">The ype of the entry.</param>
        /// <exception cref="ArgumentNullException"></exception>
        protected DirectoryEntry(FileSystem fileSystem, DirectoryEntry parent, string fullPath, string name, long size, DirectoryEntryType entryType)
        {
            if (fileSystem == null)
                throw new ArgumentNullException(nameof(fileSystem));
            if (string.IsNullOrEmpty(fullPath))
                throw new ArgumentNullException(nameof(fullPath));
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            FileSystem = fileSystem;
            Parent = parent;
            EntryType = entryType;
            Name = name;
            Size = size;
            FullPath = fullPath;
        }

        public abstract void SetName(string name);

        public abstract void SetSize(long size);

        public abstract Stream GetFileStream();

        public abstract long GetUsedSpace();
    }
}
