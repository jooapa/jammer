using System.Diagnostics;
using System.Runtime.InteropServices;
using Spectre.Console;

namespace Jammer
{
    public partial class Start
    {
        public static void CheckArgs(string[] args) {
            // NOTE(ra) If debug switch is defined remove it from the args list
            for (int i = 0; i < args.Length; i++) {
                string arg = args[i];
                switch(arg) {
                    case "-D":
                        Utils.isDebug = true;
                        Debug.dprint("\n--- Debug Started ---\n");
                        Debug.dprint($"HOME Path Environment Variable: {Environment.GetEnvironmentVariable("APPDIR")}");
                        var APPDIRlen = Environment.GetEnvironmentVariable("APPDIR");
                        if (APPDIRlen == null) { 
                            Debug.dprint("APPDIR == null");
                        } else {
                            Debug.dprint(APPDIRlen.Length.ToString());
                        }

                        try {
                            long size = Preferences.DirSize(new System.IO.DirectoryInfo(Utils.JammerPath));
                            Debug.dprint($"JammerDirSize: {size}");
                            Debug.dprint($"JammerDirSize: {Preferences.ToKilobytes(size)}");
                            Debug.dprint($"JammerDirSize: {Preferences.ToMegabytes(size)}");
                            Debug.dprint($"JammerDirSize: {Preferences.ToGigabytes(size)}");
                        }
                        catch (DirectoryNotFoundException e) {
                            Debug.dprint("Error in DirSize, folder doesnt exist yet.");
                            Debug.dprint(e.Message);
                        }
                        catch (System.Exception) {
                            Debug.dprint("Error in DirSize");
                        }
                        List<string> argumentsList = new List<string>(args);
                        argumentsList.RemoveAt(i);
                        args = argumentsList.ToArray();

                        //NOTES(ra) So nasty it breaks my hearth,
                        Utils.songs = args;
                        break;
                }
            }
            
            for (int i = 0; i < args.Length; i++) {
                    string arg = args[i];
                    switch (arg) {
                        case "-h":
                        case "--help":
                                TUI.CliHelp();
                                Environment.Exit(0);
                            return;
                        case "--play":
                        case "-p":
                            if (args.Length > i+1) {
                                Playlists.Play(args[i+1], true);
                            } else {
                                AnsiConsole.WriteLine(Locale.OutsideItems.NoPlaylistName);
                                Environment.Exit(1);
                            }
                            break;
                        case "--create":
                        case "-c":
                            if (args.Length > i+1) {
                                Playlists.Create(args[i+1]);
                            } else {
                                AnsiConsole.WriteLine(Locale.OutsideItems.NoPlaylistName);

                            }
                            Environment.Exit(0);
                            break;
                        case "--delete":
                        case "-d":
                            if (args.Length > i+1) {
                                Playlists.Delete(args[i+1]);
                            } else {
                                AnsiConsole.WriteLine(Locale.OutsideItems.NoPlaylistNameSong);
                            }
                            Environment.Exit(0);
                            break;
                        case "--add":
                        case "-a":
                            if (args.Length > i+1) {
                                var splitIndex = i+1;
                                string[] firstHalf = args.Take(splitIndex).ToArray();
                                string[] secondHalf = args.Skip(splitIndex).ToArray();
                                Console.WriteLine(secondHalf[0]);

                                Playlists.Add(secondHalf);
                            } else {
                                AnsiConsole.WriteLine(Locale.OutsideItems.NoPlaylistNameSong);
                            }
                            Environment.Exit(0);
                            break;
                        case "--remove":
                        case "-r":
                            if (args.Length > i+1) {
                                var splitIndex = i+1;
                                string[] firstHalf = args.Take(splitIndex).ToArray();
                                string[] secondHalf = args.Skip(splitIndex).ToArray();
                                Console.WriteLine(secondHalf[1]);

                                Playlists.Remove(secondHalf);
                            } else {
                                AnsiConsole.WriteLine(Locale.OutsideItems.NoPlaylistNameSong);
                            }
                            Environment.Exit(0);
                            break;
                        case "--show":
                        case "-s":
                            if (args.Length > i+1) {
                                Playlists.ShowCli(args[i+1]);
                            } else {
                                AnsiConsole.WriteLine(Locale.OutsideItems.NoPlaylistNameSong);
                            }
                            Environment.Exit(0);
                            return;
                        case "--list":
                        case "-l":
                            Playlists.PrintList();
                            Environment.Exit(0);
                            return;
                        case "--version":
                        case "-v":
                            AnsiConsole.MarkupLine($"[green]Jammer {Locale.Miscellaneous.Version}: " + Utils.version + "[/]");
                            Environment.Exit(0);
                            return;
                        case "--flush":
                        case "-f":
                            Songs.Flush();
                            Environment.Exit(0);
                            return;
                        case "--set-path":
                        case "-sp": // TODO ADD LOCALE :)) https://www.youtube.com/watch?v=thPv_v7890g
                            if (args.Length > i+1) {
                                if (Directory.Exists(args[i+1])) {
                                    Preferences.songsPath = Path.GetFullPath(Path.Combine(args[i+1], "songs"));
                                    AnsiConsole.MarkupLine("[green]Songs path set to: " + Preferences.songsPath + "[/]");

                                }
                                else if (args[i+1] == "") {
                                    AnsiConsole.MarkupLine("No path given.");

                                    return;
                                }
                                else if (args[i+1] == "default") {
                                    Preferences.songsPath = Path.Combine(Utils.JammerPath, "songs");
                                    AnsiConsole.MarkupLine("[green]Songs path set to default.[/]"); // TODO ADD LOCALE

                                } else {
                                    AnsiConsole.MarkupLine($"[red]Path [grey]'[/][white]{args[i+1]}[/][grey]'[/] does not exist.[/]"); // TODO ADD LOCALE

                                }

                                Preferences.SaveSettings();
                            } else {
                                AnsiConsole.MarkupLine("[red]No songs path given.[/]"); // TODO ADD LOCALE
                                

                            }
                            Environment.Exit(0);
                            return;
                        case "--get-path":
                        case "-gp":
                            AnsiConsole.MarkupLine("[green]Songs path: " + Preferences.songsPath + "[/]"); // TODO ADD LOCALE
                            Environment.Exit(0);
                            return;
                        case "--songs":
                        case "-so":
                            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                                System.Diagnostics.Process.Start("explorer.exe", Preferences.songsPath);
                            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                                System.Diagnostics.Process.Start("xdg-open", Preferences.songsPath);
                            }
                            Environment.Exit(0);
                            return;
                        case "--home":
                        case "-hm":
                                // if(Utils.songs.Length != 1 && args.Length != 1) {
                                //     AnsiConsole.MarkupLine("[red]When using --songs or -so, do not provide any other arguments.[/]"); // TODO ADD LOCALE
                                //     System.Environment.Exit(1);
                                // } 
                                Utils.songs[0] = Preferences.songsPath;
                                break;
                        case "--start":
                            // open explorer in Jammer folder
                            AnsiConsole.MarkupLine($"[green]{Locale.OutsideItems.OpeningFolder}[/]");
                            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                                System.Diagnostics.Process.Start("explorer.exe", Utils.JammerPath);
                            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                                System.Diagnostics.Process.Start("xdg-open", Utils.JammerPath);
                            }
                            Environment.Exit(0);
                            return;
                        case "--update":
                            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                                AnsiConsole.MarkupLine($"[red]{Locale.OutsideItems.RunUpdate}[/]");
                                return;
                            }
                            AnsiConsole.MarkupLine($"[green]{Locale.OutsideItems.CheckingUpdates}[/]");
                            string latestVersion = Update.CheckForUpdate(Utils.version);
                            if (latestVersion != "") {
                                
                                string downloadPath = Update.UpdateJammer(latestVersion);
                                AnsiConsole.MarkupLine($"[green]{Locale.OutsideItems.UpdateFound}[/]" + "\n" + $"{Locale.Miscellaneous.Version}: [green]" + latestVersion + "[/]");
                                AnsiConsole.MarkupLine($"[green]{Locale.OutsideItems.Downloading}[/]");
            
                                AnsiConsole.MarkupLine($"[green]{Locale.OutsideItems.DownloadedTo}: " + downloadPath + "[/]");
                                AnsiConsole.MarkupLine($"[cyan]{Locale.OutsideItems.Installing}[/]");
                                // Run run_command.bat with argument as the path to the downloaded file
                                // Define the batch file and arguments
                                string batchFile = "run_command.bat";
                                string arguments = "update " + downloadPath;

                                // Start the process
                                while (!File.Exists(downloadPath)) {
                                    // Wait for the file to be downloaded
                                }
                                Process.Start(batchFile, arguments);
                            } else {
                                AnsiConsole.MarkupLine($"[green]{Locale.OutsideItems.UpToDate}[/]");
                            }
                            Environment.Exit(0);
                            return;
                    }
                }
        }
    }
}
