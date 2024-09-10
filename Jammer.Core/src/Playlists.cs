using Spectre.Console;
using System.IO;

namespace Jammer
{
    public class Playlists
    {
        static public void Create(string playlist)
        {
            Console.WriteLine($"{Locale.OutsideItems.CreatingPlaylist}: " + playlist + ".jammer");
            string playlistName = playlist;
            string playlistPath = GetJammerPlaylistPath(playlistName);

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
            if (URL.IsUrl(playlist))
            {
                Utils.songs = new string[] { playlist };
                if (fromCli) {
                    return;
                } else {
                    Jammer.Play.PlaySong(Utils.songs, 0);
                    return;
                }
            }
            
            // AnsiConsole.WriteLine($"{Locale.OutsideItems.StartingUp} " + playlist);
            string playlistName = playlist;
            string playlistPath = GetJammerPlaylistPath(playlistName);
            AnsiConsole.MarkupLine($"[green]{playlistPath}[/]");
            if (File.Exists(playlistPath) || URL.IsUrl(playlist))
            {

                Utils.currentSongIndex = 0;
                Utils.songs = new string[] { GetJammerPlaylistPath(playlist) };

                Utils.currentPlaylist = playlistName;
                
                if (fromCli)
                {
                    if (Utils.songs.Length == 0)
                    {
                        Utils.currentSongIndex = 0;
                        AnsiConsole.MarkupLine($"[red]{Locale.OutsideItems.PlaylistIsEmpty}[/]");
                    }
                    else {
                        AnsiConsole.MarkupLine($"[green]{Locale.OutsideItems.Playing} " + Start.Sanitize(playlistName) + "[/]");
                    }
                }
                else
                {
                    Start.state = MainStates.play;
                    Jammer.Play.PlaySong(Utils.songs, 0);
                }
            }
            else
            {
                if (!fromCli)
                {
                    Message.Data($"{Locale.OutsideItems.PlaylistDoesntExist}:" + playlist, $"{Locale.OutsideItems.ErrorPlaying}", true);
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]{Locale.OutsideItems.Playlist}: " + playlist + $" {Locale.OutsideItems.DoesntExist}[/]");
                    Environment.Exit(0);
                }
            }
        }
        
        /// <summary>
        /// Returns the path of the playlist
        /// </summary>
        /// <param name="playlist"></param>
        /// <returns></returns>
        static public string GetJammerPlaylistPath(string playlist)
        {
            // if playlist is not a path, return the path
            if (!playlist.Contains(Path.DirectorySeparatorChar))
            {
                return Path.Combine(
                    Utils.JammerPath,
                    "playlists",
                    playlist + ".jammer"
                );
            }
            return Path.GetFullPath(playlist);
        }

        /// <summary>
        /// Returns the name of the playlist if it is in the jammer folder, else returns the path, for visual purposes only
        /// </summary>
        /// <param name="playlist"></param>
        /// <returns>path or name</returns>
        public static string GetJammerPlaylistVisualPath(string playlist)
        {
            // playlist var is a path
            // return the name of the file, if its in the jammer folder
            if (playlist.Contains(Utils.JammerPath))
            {
                return Path.GetFileNameWithoutExtension(playlist);
            }
            return playlist;
        }

        static public void Delete(string playlist)
        {
            Console.WriteLine($"{Locale.OutsideItems.Deleting} " + playlist);
            string playlistName = playlist;
            string playlistPath = GetJammerPlaylistPath(playlistName);

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
            string playlistPath = GetJammerPlaylistPath(playlistName);

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
            string playlistPath = GetJammerPlaylistPath(playlistName);

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
            string playlistPath = GetJammerPlaylistPath(playlist);

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
            string playlistPath = GetJammerPlaylistPath(playlistName);
            
            // if playlist exists, overwrite it with y/n
            if (File.Exists(playlistPath))
            {
                if (!force) {
                    string input = Jammer.Message.Input(Locale.Miscellaneous.YesNo,Locale.OutsideItems.AlreadyExists + " " + playlistPath + ". " + Locale.OutsideItems.Overwrite);
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
                    Message.Data($"{Locale.OutsideItems.Error}: " + ex.Message, $"ERROR SAVING PLAYLIST", true);
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
                    Jammer.Message.Data($"{Locale.OutsideItems.Error}: " + ex.Message, $"ERROR SAVING PLAYLIST", true);
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
            string playlistDir = Path.Combine(Utils.JammerPath, "playlists");
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
