using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

// ProStripe timecode striper
// Data model: access file system information

// Copyright 2013, Bill Price

namespace ProStripe.Model
{
    /// <summary>
    /// Access file system information
    /// </summary>
    public class FileSystem
    {
        /// <summary>
        /// List the roots
        /// </summary>
        /// <returns>List of root folders</returns>
        private static IEnumerable<FileSystemInfo> GetRoots()
        {
            DriveInfo[] d = DriveInfo.GetDrives();
            IEnumerable<FileSystemInfo> result = 
                from x in DriveInfo.GetDrives() select (FileSystemInfo)x.RootDirectory;
            return result;
        }

        /// <summary>
        /// List children of a folder
        /// </summary>
        /// <param name="path">parent folder name</param>
        /// <returns>List of subfolders and files</returns>
        public static IEnumerable<FileSystemInfo> GetChildren(string path)
        {
            if (path == "")
                return GetRoots();
            DirectoryInfo dir = new DirectoryInfo(path);
            IEnumerable<FileSystemInfo> result;
            try
            {
                result =
                    from x in dir.EnumerateFileSystemInfos() select x;
            }
            catch {
                result = Enumerable.Empty<FileSystemInfo>();
            };
            return result;
        }

        public static FileSystemInfo GetItem(string fullPath)
        {
            return new FileInfo(fullPath);
        }

        static IEnumerable<FileSystemInfo> drives;

        public static bool DrivesChanged()
        {
            IEnumerable<FileSystemInfo> newDrives = GetRoots();
            IEnumerableComparer<FileSystemInfo> comparer = new IEnumerableComparer<FileSystemInfo>();
            if (!comparer.Equals(drives, newDrives))
            {
                drives = newDrives;
                return true;
            }
            return false;
        }

        public class IEnumerableComparer<T> : IEqualityComparer<IEnumerable<T>>
        {
            public bool Equals(IEnumerable<T> x, IEnumerable<T> y)
            {
                return Object.ReferenceEquals(x, y) || (x != null && y != null && x.SequenceEqual(y));
            }

            public int GetHashCode(IEnumerable<T> obj)
            {
                if (obj == null)
                    return 0;

                return unchecked(obj.Select(e => e.GetHashCode()).Aggregate(0, (a, b) => a + b));  // BAD 
            }
        }
    }
}
