using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace FinalFactory.IO
{
    [DebuggerDisplay("FilePath: {Path}")]
    public readonly struct FilePath : IEquatable<FilePath>
    {
        public static readonly FilePath Empty = new FilePath(string.Empty);
        
        public readonly string Path;

        public FilePath(string path)
        {
            Path = path;
        }
        
        /// <summary>
        /// Generates a new FilePath from path pieces.
        /// </summary>
        /// <param name="paths"></param>
        public FilePath(params string[] paths)
        {
            Path = System.IO.Path.Combine(paths);
        }

        public FileExtension Extension => FileExtension.FromFileName(Path);
        
        public bool IsAbsolute => System.IO.Path.IsPathRooted(Path);

        public bool Exists => File.Exists(Path);
        
        public string FullName => System.IO.Path.GetFileName(Path);
        
        public string Name => System.IO.Path.GetFileNameWithoutExtension(Path);

        public string DirectoryPath => System.IO.Path.GetDirectoryName(Path);
        
        public string DirectoryName => System.IO.Path.GetFileName(DirectoryPath);
        
        public string GetWithExtension(FileExtension extension) => System.IO.Path.ChangeExtension(Path, extension.ToString());

        public FileStream OpenStream(FileMode mode = FileMode.OpenOrCreate, FileAccess access = FileAccess.ReadWrite, FileShare share = FileShare.None)
        {
            return File.Open(Path, mode, access, share);
        }
        public void Rename(string newFileName)
        {
            File.Move(Path, newFileName);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public void CreateDirectory() => Directory.CreateDirectory(Path);

        public void Delete()
        {
            File.Delete(Path);
        }
        
        public static explicit operator FilePath(string path)
        {
            return new FilePath(path);
        }

        public static explicit operator string(FilePath path)
        {
            return path.Path;
        }

        public static bool operator ==(FilePath path1, FilePath path2)
        {
            return path1.Path == path2.Path;
        }

        public static bool operator !=(FilePath path1, FilePath path2)
        {
            return path1.Path != path2.Path;
        }
        
        public static FilePath operator +(FilePath path1, FilePath path2)
        {
            if (path2.IsAbsolute)
            {
                throw new InvalidOperationException("Cannot add an absolute path to a relative path.");
            }
            return new FilePath(System.IO.Path.Combine(path1.Path, path2.Path));
        }
        
        public static FilePath operator +(FilePath path1, string path2)
        {
            return new FilePath(System.IO.Path.Combine(path1.Path, path2));
        }

        public bool Equals(FilePath other)
        {
            return Path == other.Path;
        }

        public override bool Equals(object obj)
        {
            return obj is FilePath other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Path != null ? Path.GetHashCode() : 0);
        }
    }
}