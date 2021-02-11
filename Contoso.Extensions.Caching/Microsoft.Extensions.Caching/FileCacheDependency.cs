using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Directory_ = System.IO.Directory;

namespace Microsoft.Extensions.Caching
{
    /// <summary>
    /// Configuration for FileCacheDependencies
    /// </summary>
    public static class FileCacheDependency
    {
        static string _directory;
        /// <summary>
        /// Gets or sets the directory.
        /// </summary>
        /// <value>
        /// The directory.
        /// </value>
        /// <exception cref="ArgumentNullException">value</exception>
        public static string Directory
        {
            get => _directory;
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(value));
                _directory = value.EndsWith("\\") ? value : value + "\\";
            }
        }

        internal static class CacheFile
        {
            static readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            static string GetFilePathForName(string name) => !string.IsNullOrEmpty(Directory) ? Path.Combine(Directory, $"{name}.txt") : null;
            static void WriteBodyForName(string name, string path) => File.WriteAllText(path, $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}\r\n");

            public static void Touch(string name)
            {
                if (string.IsNullOrEmpty(Directory))
                    return;
                lock (_rwLock)
                {
                    var filePath = GetFilePathForName(name);
                    if (filePath == null)
                        return;
                    try { WriteBodyForName(name, filePath); }
                    catch { };
                }
            }

            public static IChangeToken MakeFileWatchChangeToken(IEnumerable<string> names)
            {
                if (string.IsNullOrEmpty(Directory))
                    return null;
                if (!Directory_.Exists(Directory))
                    Directory_.CreateDirectory(Directory);
                var fileProvider = new PhysicalFileProvider(Directory);
                lock (_rwLock)
                {
                    var changeTokens = new List<IChangeToken>();
                    foreach (var name in names)
                    {
                        var filePath = GetFilePathForName(name);
                        if (filePath == null)
                            continue;
                        try
                        {
                            var filePathAsDirectory = Path.GetDirectoryName(filePath);
                            if (!Directory_.Exists(filePathAsDirectory))
                                Directory_.CreateDirectory(filePathAsDirectory);
                            if (!File.Exists(filePath))
                                WriteBodyForName(name, filePath);
                        }
                        catch { };
                        changeTokens.Add(fileProvider.Watch(filePath));
                    }
                    return changeTokens.Count == 1 ? changeTokens[0] : new CompositeChangeToken(changeTokens);
                }
            }
        }
    }
}
