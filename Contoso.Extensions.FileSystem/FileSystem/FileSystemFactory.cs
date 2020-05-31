using Contoso.Extensions.HAL.BlockDevice;

namespace Contoso.Extensions.FileSystem
{
    public abstract class FileSystemFactory
    {
        /// <summary>
        /// The name of the file system.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Checks if the file system can handle the partition.
        /// </summary>
        /// <param name="device">The partition.</param>
        /// <returns>Returns true if the file system can handle the partition, false otherwise.</returns>
        public abstract bool IsType(Partition device);

        /// <summary>
        /// Creates a new <see cref="FileSystem"/> object for the given partition, root path, and size.
        /// </summary>
        /// <param name="device">The partition.</param>
        /// <param name="rootPath">The root path.</param>
        /// <param name="size">The size, in MB.</param>
        /// <returns></returns>
        public abstract FileSystem Create(Partition device, string rootPath, long size);
    }
}
