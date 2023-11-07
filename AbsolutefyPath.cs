using System;
using System.IO;

namespace jammer
{
    public class AbsolutefyPath
    {
        public static string[] Absolutefy(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                string item = args[i];
                if (IsRelativePath(item))
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

        static string ConvertToAbsolutePath(string relativePath)
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            return Path.GetFullPath(Path.Combine(currentDirectory, relativePath));
        }
    }
}
