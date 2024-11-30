using System.Text.Json;

namespace Jammer
{
    public class Absolute
    {
        public static string[] Correctify(string[] args)
        {
            string details;
            for (int i = 0; i < args.Length; i++)
            {
                details = "";

                // split title by ###@@@###
                string[] detailSplit = args[i].Split(Utils.jammerFileDelimeter);
    
                if (detailSplit.Length > 1)
                {
                    details = detailSplit[1];
                    args[i] = detailSplit[0];
                }

                string item = args[i];
                // AnsiConsole.MarkupLine($"[green]{Locale.OutsideItems.Checking} {item}[/]");
                Log.Info($"{Locale.OutsideItems.Checking} {item}");

                if (URL.IsUrl(item))
                {
                    // if url doesnt have http:// or https://
                    if (!item.Contains("http://") && !item.Contains("https://"))
                    {
                        item = "https://" + item;
                    }

                    if (URL.IsValidSoundcloudSong(item))
                    {
                        // splice ? and everything after it
                        int index = item.IndexOf("?");
                        if (index > 0)
                        {
                            item = item.Substring(0, index);
                        }
                    }
                    else if (URL.IsValidYoutubeSong(item))
                    {
                        // splice & and everything after it
                        int index = item.IndexOf("&");
                        if (index > 0)
                        {
                            item = item.Substring(0, index);
                        }
                    }
                    else if (URL.IsUrl(item))
                    {
                        // AnsiConsole.MarkupLine($"[green]URL {item} {Locale.OutsideItems.IsValid}[/]");
                        Log.Info($"URL {item} {Locale.OutsideItems.IsValid}");
                    }
                    else {
                        // AnsiConsole.MarkupLine($"[red]URL {item} {Locale.OutsideItems.IsntValid}[/]");
                        Log.Error($"URL {item} {Locale.OutsideItems.IsntValid}");
                        // delete item from args
                        args = args.Take(i).Concat(args.Skip(i + 1)).ToArray();
                        i--;
                    }

                    args[i] = item;
                }
                // if folder exists, convert to absolute path
                else if (Directory.Exists(item))
                {
                    if (IsRelativePath(item)) {
                        args[i] = ConvertToAbsolutePath(item);
                    }
                    // Message.Data(Start.Sanitize(JsonSerializer.Serialize(args)), "12");

                    // get every file in the folder
                    string[] files = Directory.GetFiles(item);

                    // dekete the folder
                    args = args.Take(i).Concat(args.Skip(i + 1)).ToArray();
                    i--;

                    foreach (string file in files)
                    {
                        args = args.Take(i + 1).Concat(new string[] { IsRelativePath(file) ? ConvertToAbsolutePath(file): file}).Concat(args.Skip(i + 1)).ToArray();
                        i++;
                    }

                    // Message.Data(Start.Sanitize(JsonSerializer.Serialize(args)), "12");
                }
                // if exits, convert to absolute path
                else if (File.Exists(item))
                {
                    if (IsRelativePath(item)) {
                        args[i] = ConvertToAbsolutePath(item);
                    }
                }
                // else if (!File.Exists(item))
                // {
                //     AnsiConsole.MarkupLine($"[red]{Locale.OutsideItems.File} '{item}'{Locale.OutsideItems.DoesntExist}[/]");
                //     // delete item from args
                //     args = args.Take(i).Concat(args.Skip(i + 1)).ToArray();
                //     i--;
                // }

                if (details != "")
                {
                    args[i] = args[i] + Utils.jammerFileDelimeter + details;
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
