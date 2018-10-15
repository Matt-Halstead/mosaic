using System.IO;

namespace Mosaic.Util
{
    public static class FileUtils
    {
        public static void EnsureFolderExists(string path)
        {
            var folders = path.Split('\\');
            var current = string.Empty;
            foreach (var folder in folders)
            {
                current = Path.Combine(current, folder);
                if (current.EndsWith(":"))
                {
                    current += "\\";
                }

                if (!Directory.Exists(current))
                {
                    Directory.CreateDirectory(current);
                }
            }
        }
    }
}
