using System;
using System.IO;
using Spectre.Console;
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
                if (IsUrl(item))
                {
                    // if url doesnt have http:// or https://
                    if (!item.Contains("http://") && !item.Contains("https://"))
                    {
                        item = "https://" + item;
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

        static bool IsUrl(string input)
        {
            // detect if input is url using regex
            string pattern = @"^(https?:\/\/)?(www\.)?(soundcloud\.com|snd\.sc)\/(.*)$";
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(input);
        }
    }
}
