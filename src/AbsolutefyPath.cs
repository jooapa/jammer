using System.Text.RegularExpressions;

namespace jammer
{
    public class AbsolutefyPath
    {
        public static string[] Absolutefy(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                string item = args[i];
                if (URL.IsUrl(item))
                {
                    // if url doesnt have http:// or https://
                    if (!item.Contains("http://") && !item.Contains("https://"))
                    {
                        item = "https://" + item;
                    }
                    
                    if (URL.IsSoundCloudUrlValid(item)) {
                        // splice ? and everything after it
                        int index = item.IndexOf("?");
                        if (index > 0)
                        {
                            item = item.Substring(0, index);
                        }
                    } else if (URL.IsYoutubeUrlValid(item)) {
                        // splice & and everything after it
                        int index = item.IndexOf("&");
                        if (index > 0)
                        {
                            item = item.Substring(0, index);
                        }
                    }

                    args[i] = item;
                }
                else if (IsRelativePath(item))
                {
                    args[i] = ConvertToAbsolutePath(item);
                }
            }
            return args;
        }

        static bool IsAbsolutePath(string path)
        {
            return Path.IsPathRooted(path);
        }

        static bool IsRelativePath(string path)
        {
            return !IsAbsolutePath(path);
        }

        static public string ConvertToAbsolutePath(string relativePath)
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            return Path.GetFullPath(Path.Combine(currentDirectory, relativePath));
        }
    }
}
