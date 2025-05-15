using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace FinalFactory.IO
{
    [DebuggerDisplay("Extension: {Extension}")]
    public readonly struct FileExtension
    {
        public static FileExtension Asset = new FileExtension(".asset");
        public static FileExtension Json = new FileExtension(".json");
        public static FileExtension Zip = new FileExtension(".zip");

        public string Extension { get; }

        private FileExtension(string extension)
        {
            Extension = extension;
        }

        public string TempExtension => ".temp" + Extension;

        public override string ToString()
        {
            return Extension;
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static FileExtension FromFileName(string filename) => (FileExtension)Path.GetExtension(filename);

        //add explicit conversion to string
        public static explicit operator string(FileExtension extension)
        {
            return extension.Extension;
        }
        
        //add implicit conversion from string
        public static explicit operator FileExtension(string extension)
        {
            return new FileExtension(extension);
        }
        
        //add all helper methods
        public static bool operator ==(FileExtension left, FileExtension right)
        {
            return left.Extension == right.Extension;
        }

        public static bool operator !=(FileExtension left, FileExtension right)
        {
            return left.Extension != right.Extension;
        }
        
        //add Equals, GetHashCode and ToString
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        
        public override int GetHashCode()
        {
            return Extension.GetHashCode();
        }
    }
}