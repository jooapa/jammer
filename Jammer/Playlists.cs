using Spectre.Console;
using System.IO;

namespace jammer
{
    public class Playlists
    {
        static public void Create(string playlist)
        {
            Console.WriteLine($"{Locale.OutsideItems.CreatingPlaylist}: " + playlist + ".jammer");
            string playlistName = playlist;
            string playlistPath = Path.Combine(
                Utils.jammerPath,
                "playlists",
                playlistName + ".jammer"
            );

            if (File.Exists(playlistPath))
            {
                Console.WriteLine($"{Locale.OutsideItems.AlreadyExists} " + playlistPath + $". {Locale.OutsideItems.Overwrite} {Locale.Miscellaneous.YesNo}");
                // y/n prompt
                if (Console.ReadLine() == "y")
                {
                    if (File.Exists(playlistPath))
                    {
                        File.Delete(playlistPath);
                    }
                    Utils.currentPlaylist = playlistName;
                    File.Create(playlistPath);
                }
            }
            else
            {
                Utils.currentPlaylist = playlistName;
                File.Create(playlistPath);
            }
        }

        static public void Play(string playlist, bool fromCli)
        {
            AnsiConsole.WriteLine($"{Locale.OutsideItems.StartingUp} " + playlist + ".jammer");
            string playlistName = playlist;
            string playlistPath = Path.Combine(
                Utils.jammerPath,
                "playlists",
                playlistName + ".jammer"
            );

            if (File.Exists(playlistPath))
            {
                string[] songs = File.ReadAllLines(playlistPath);

                Utils.currentSongIndex = 0;
                Utils.songs = songs;

                Utils.currentPlaylist = playlistName;
                
                if (fromCli)
                {
                    if (Utils.songs.Length == 0)
                    {
                        Utils.currentSongIndex = 0;
                        AnsiConsole.MarkupLine($"[red]{Locale.OutsideItems.PlaylistIsEmpty}[/]");
                    }
                    else {
                        AnsiConsole.MarkupLine($"[green]{Locale.OutsideItems.Playing} " + songs[0] + "[/]");
                    }
                }
                else
                {
                    AnsiConsole.MarkupLine($"[green]{Locale.OutsideItems.Playing} " + songs[0] + "[/]");
                    Start.state = MainStates.play;
                    jammer.Play.PlaySong(songs, 0);
                }
            }
            else
            {
                if (!fromCli)
                {
                    jammer.Message.Data($"{Locale.OutsideItems.PlaylistDoesntExist}:" + playlist + ".jammer", $"{Locale.OutsideItems.ErrorPlaying}", true);
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]{Locale.OutsideItems.Playlist}: " + playlist + $" {Locale.OutsideItems.DoesntExist}[/]");
                    Environment.Exit(0);
                    return;
                }
            }
        }

        static public void Delete(string playlist)
        {
            Console.WriteLine($"{Locale.OutsideItems.Deleting} " + playlist + ".jammer");
            string playlistName = playlist;
            string playlistPath = Path.Combine(
                Utils.jammerPath,
                "playlists",
                playlistName + ".jammer"
            );

            if (File.Exists(playlistPath))
            {
                File.Delete(playlistPath);
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]{Locale.OutsideItems.Playlist} {Locale.OutsideItems.DoesntExist}[/]");
            }
        }

        static public void Add(string[] args)
        {
            string playlistName = args[0];
            string playlistPath = Path.Combine(
                Utils.jammerPath,
                "playlists",
                playlistName + ".jammer"
            );

            AnsiConsole.MarkupLine($"[green]{Locale.OutsideItems.AddingSongsTo} " + playlistPath + "[/]");
            if (File.Exists(playlistPath))
            {
                // take args and remove playlist name
                args = args.Skip(1).ToArray();

                // absoulutify arg if its a relative path and add https:// if url
                args = Absolute.Correctify(args);
                // get all songs in playlist
                string[] songs = File.ReadAllLines(playlistPath);

                // add songs to playlist
                for (int i = 0; i < args.Length; i++)
                {
                    string song = args[i];
                    AnsiConsole.MarkupLine($"[green]{Locale.OutsideItems.Adding} " + song + "[/]");

                    // check if song is already in playlist
                    if (songs.Contains(song))
                    {
                        AnsiConsole.MarkupLine("[red]" + song + $" {Locale.OutsideItems.IsALreadyInPlaylist}[/]"); 
                    }
                    else
                    {
                        File.AppendAllText(playlistPath, song + "\n");
                    }

                }
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]{Locale.OutsideItems.Playlist}: "+ playlistName + $" {Locale.OutsideItems.DoesntExist}[/]");
            }
            AnsiConsole.MarkupLine($"[green]{Locale.OutsideItems.Done}![/]");
            Environment.Exit(0);
        }

        static public void Remove(string[] args)
        {
            string playlistName = args[0];
            string playlistPath = Path.Combine(
                Utils.jammerPath,
                "playlists",
                playlistName + ".jammer"
            );

            AnsiConsole.MarkupLine($"[green]{Locale.OutsideItems.RemovingFrom} " + playlistPath + "[/]");
            if (File.Exists(playlistPath))
            {
                // take args and remove first 3 elements
                args = args.Skip(3).ToArray();

                // absoulutify arg if its a relative path and add https:// if url
                args = Absolute.Correctify(args);
                // get all songs in playlist
                string[] songs = File.ReadAllLines(playlistPath);

                // remove songs from playlist
                for (int i = 0; i < args.Length; i++)
                {
                    string song = args[i];
                    AnsiConsole.MarkupLine($"[green]{Locale.OutsideItems.Removing} " + song + "[/]");

                    // check if song is already in playlist
                    if (songs.Contains(song))
                    {
                        // delete song from playlist
                        songs = songs.Where(val => val != song).ToArray();
                        File.WriteAllLines(playlistPath, songs);
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[red]" + song + $" {Locale.OutsideItems.NotInPlaylist}[/]");
                    }

                }
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]{Locale.OutsideItems.Playlist}: " + playlistName + $" {Locale.OutsideItems.DoesntExist}[/]");
            }
            AnsiConsole.MarkupLine($"[green]{Locale.OutsideItems.Done}![/]");
            Environment.Exit(0);
        }

        static public void ShowCli(string playlist)
        {
            AnsiConsole.MarkupLine(GetShow(playlist));
        }

        static public string GetShow(string playlist)
        {
            AnsiConsole.MarkupLine($"{Locale.OutsideItems.ShowingPlaylist} [red]" + playlist + "[/]");
            playlist = playlist + ".jammer";
            string playlistPath = Path.Combine(
                Utils.jammerPath,
                "playlists",
                playlist
            );

            string playlistList = "";
            if (File.Exists(playlistPath))
            {
                string[] songs = File.ReadAllLines(playlistPath);
                if (songs.Length == 0)
                {
                    playlistList = $"[red]{Locale.OutsideItems.PlaylistIsEmpty}[/]";
                }
                foreach (string song in songs)
                {
                    playlistList += "[green]" + song + "[/]" + "\n";
                }
            }
            else
            {
                playlistList = $"[red]{Locale.OutsideItems.PlaylistDoesntExist}[/]";
            }

            return playlistList;
        }

        static public void Save(string playlistName, bool force = false)
        {
            string playlistPath = Path.Combine(
                Utils.jammerPath,
                "playlists",
                playlistName + ".jammer"
            );
            // if playlist exists, overwrite it with y/n
            if (File.Exists(playlistPath))
            {
                if (!force) {
                    string input = jammer.Message.Input(Locale.Miscellaneous.YesNo,Locale.OutsideItems.AlreadyExists + " " + playlistPath + ". " + Locale.OutsideItems.Overwrite);
                    // y/n prompt
                    if (input != "y")
                    {
                        return;
                    }
                }
                try
                {
                    if (File.Exists(playlistPath))
                    {
                        File.Delete(playlistPath);
                    }

                    File.WriteAllLines(playlistPath, Utils.songs);
                    Utils.currentPlaylist = playlistName;
                }
                catch (Exception ex)
                {
                    jammer.Message.Data($"{Locale.OutsideItems.Error}: " + ex.Message, $"ERROR SAVING PLAYLIST", true);
                }
            }
            else
            {
                try
                {
                    File.WriteAllLines(playlistPath, Utils.songs);
                    Utils.currentPlaylist = playlistName;
                }
                catch (Exception ex)
                {
                    jammer.Message.Data($"{Locale.OutsideItems.Error}: " + ex.Message, $"ERROR SAVING PLAYLIST", true);
                }
            }
        }

        static public void AutoSave() {
            if (!Preferences.isAutoSave) {
                return;
            }
            if (Utils.currentPlaylist == "") {
                return;
            }
            Save(Utils.currentPlaylist, true);
        }

        public static void PrintList()
        {
            Console.WriteLine($"{Locale.OutsideItems.Playlists}:");
            Console.WriteLine(GetList());
        }

        public static string GetList() {
            string playlistDir = Path.Combine(Utils.jammerPath, "playlists");
            string[] playlists = Directory.GetFiles(playlistDir);
            string playlistList = "";
            foreach (string playlist in playlists)
            {
                playlistList += Path.GetFileNameWithoutExtension(playlist) + "\n";
            }
            playlistList = playlistList.Remove(playlistList.Length - 1);
            return playlistList;
        }
    }
}
