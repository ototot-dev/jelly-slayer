using System.IO;

namespace FinalFactory.Utilities
{
    public static class FileUtilities
    {
        public static string ReplaceInvalidFilePathChars(this string filename)
        {
            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));    
        }
    }
}