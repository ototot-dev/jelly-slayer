namespace FinalFactory.IO
{
    public static class FilePathExtension
    {
        public static FilePath ToFilePath(this string path)
        {
            return new FilePath(path);
        }
    }
}