using Spectre.Console;
using System.IO;

namespace Jammer
{
    public enum JammerPlaylistStream
    {
        Favorites,
    }
    public class Playlists
    {

        static public void Create(string playlist)
        {
            Console.WriteLine($"{Locale.OutsideItems.CreatingPlaylist}: " + playlist + ".jammer");
            string playlistName = playlist;
            (string playlistPath, JammerPlaylistStream? stream) = GetJammerPlaylistPath(playlistName);

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
                    Utils.CurrentPlaylist = playlistName;
                    File.Create(playlistPath);
                }
            }
            else
            {
                Utils.CurrentPlaylist = playlistName;
                File.Create(playlistPath);
            }
        }

        static public void Play(string playlist, bool fromCli)
        {
            if (URL.IsUrl(playlist))
            {
                Utils.Songs = new string[] { playlist };
                if (fromCli)
                {
                    return;
                }
                else
                {
                    Jammer.Play.PlaySong(Utils.Songs, 0);
                    return;
                }
            }

            // AnsiConsole.WriteLine($"{Locale.OutsideItems.StartingUp} " + playlist);
            string playlistName = playlist;
            (string playlistPath, JammerPlaylistStream? stream) = GetJammerPlaylistPath(playlistName);
            AnsiConsole.MarkupLine($"[green]{playlistPath}[/]---");
            if (File.Exists(playlistPath) || URL.IsUrl(playlist))
            {

                Utils.CurrentSongIndex = 0;
                Song song = new Song();
                song.URI = playlistPath;
                song.Stream = stream;

                Utils.Songs = new string[] { song.ToSongString() };

                if (stream == null)
                {
                    Utils.CurrentPlaylist = playlistName;
                }
                else
                {
                    Utils.CurrentPlaylist = "";
                    Utils.BackUpPlaylistName = playlistName;
                }

                if (fromCli)
                {
                    if (Utils.Songs.Length == 0)
                    {
                        Utils.CurrentSongIndex = 0;
                        AnsiConsole.MarkupLine($"[red]{Locale.OutsideItems.PlaylistIsEmpty}[/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"[green]{Locale.OutsideItems.Playing} " + Start.Sanitize(playlistName) + "[/]");
                    }
                }
                else
                {
                    Start.state = MainStates.play;
                    Jammer.Play.PlaySong(Utils.Songs, 0);
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
        /// Returns the path of the playlist and the stream type if any
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static (string, JammerPlaylistStream?) GetJammerPlaylistPath(string input)
        {
            JammerPlaylistStream? stream = null;
            if (!input.Contains(Path.DirectorySeparatorChar))
            {
                string playlistsPath = Preferences.GetPlaylistsPath();

                if (!Path.HasExtension(input))
                {
                    // if the file name has ":" ie "MyPlaylist:Favs" then split it and use the last part as the name
                    if (input.Contains(':') && !input.StartsWith("http") && !input.StartsWith("https") && !input.Contains(":\\"))
                    {
                        string[] parts = input.Split(':');
                        input = parts[0];
                        string streamName = parts[1].ToLower().Trim();
                        if (streamName == "favorites" || streamName == "favs" || streamName == "fav")
                        {
                            stream = JammerPlaylistStream.Favorites;
                        }
                    }

                    string defaultPath = Path.Combine(playlistsPath, input + ".jammer");
                    string alternatePath = Path.Combine(playlistsPath, input + ".playlist");

                    if (File.Exists(alternatePath) && !File.Exists(defaultPath))
                    {
                        return (alternatePath, stream);
                    }

                    return (defaultPath, stream);
                }

                return (Path.Combine(playlistsPath, input), stream);
            }
            return (Path.GetFullPath(input), stream);
        }

        /// <summary>
        /// Returns the name of the playlist if it is in the jammer folder, else returns the path, for visual purposes only
        /// </summary>
        /// <param name="playlist"></param>
        /// <returns>path or name</returns>
        public static string GetJammerPlaylistVisualPath(string playlist)
        {
            // playlist var is a path
            if (playlist.Contains(Preferences.GetPlaylistsPath()))
            {
                return Path.GetFileNameWithoutExtension(playlist);
            }
            return playlist;
        }

        static public void Delete(string playlist)
        {
            Console.WriteLine($"{Locale.OutsideItems.Deleting} " + playlist);
            string playlistName = playlist;
            (string playlistPath, JammerPlaylistStream? stream) = GetJammerPlaylistPath(playlistName);

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
            (string playlistPath, JammerPlaylistStream? stream) = GetJammerPlaylistPath(playlistName);

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
                AnsiConsole.MarkupLine($"[red]{Locale.OutsideItems.Playlist}: " + playlistName + $" {Locale.OutsideItems.DoesntExist}[/]");
            }
            AnsiConsole.MarkupLine($"[green]{Locale.OutsideItems.Done}![/]");
            Environment.Exit(0);
        }

        static public void Remove(string[] args)
        {
            string playlistName = args[0];
            (string playlistPath, JammerPlaylistStream? stream) = GetJammerPlaylistPath(playlistName);

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
                        File.WriteAllLines(playlistPath, songs, System.Text.Encoding.UTF8);
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
            (string playlistPath, JammerPlaylistStream? stream) = GetJammerPlaylistPath(playlist);

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
                Message.Data($"{Locale.OutsideItems.PlaylistDoesntExist}:" + playlist, $"{Locale.OutsideItems.ErrorPlaying}", true);
                playlistList = $"[red]{Locale.OutsideItems.PlaylistDoesntExist}[/]";
            }

            return playlistList;
        }

        /// <summary>
        /// Type can be jammer, m3u, m3u8, auto (Chosen by context)
        /// </summary>
        /// <param name="playlistName"></param>
        /// <param name="force"></param>
        /// <param name="type"></param>
        static public void Save(string playlistName, bool force = false, string type = "auto")
        {
            (string playlistPath, JammerPlaylistStream? stream) = GetJammerPlaylistPath(playlistName);

            // if playlist exists, overwrite it with y/n
            if (File.Exists(playlistPath))
            {
                if (!force)
                {
                    string input = Message.Input(Locale.Miscellaneous.YesNo, Locale.OutsideItems.AlreadyExists + " " + playlistPath + ". " + Locale.OutsideItems.Overwrite, true);
                    // y/n prompt
                    if (input != "y")
                    {
                        return;
                    }
                }
                try
                {

                    string extension = Path.GetExtension(playlistPath);

                    if (extension == ".jammer")
                    {
                        File.WriteAllLines(playlistPath, Utils.Songs, System.Text.Encoding.UTF8);
                        Utils.CurrentPlaylist = playlistName;
                    }
                    else if (extension == ".m3u" || extension == ".m3u8")
                    {
                        string content = File.ReadAllText(playlistPath);
                        string newContent;
                        if (content.StartsWith("#EXTM3U"))
                        {
                            newContent = "#EXTM3U\n";
                        }
                        else
                        {
                            newContent = "";
                        }

                        // if the file has anythng starting with #EXT-X- take that line and add it to the new content
                        string[] lines = content.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string line in lines)
                        {
                            if (line.StartsWith("#EXT-X-"))
                            {
                                newContent += line + Environment.NewLine;
                            }
                        }

                        newContent += Environment.NewLine;

                        foreach (string song in Utils.Songs)
                        {
                            newContent += SongExtensions.ToSong(song).ToSongM3UString() + Environment.NewLine;
                        }

                        File.WriteAllText(playlistPath, newContent, System.Text.Encoding.UTF8);
                        Utils.CurrentPlaylist = playlistName;

                        // Message.Data(newContent, "SAVE M3U");
                    }
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
                    File.WriteAllLines(playlistPath, Utils.Songs, System.Text.Encoding.UTF8);
                    Utils.CurrentPlaylist = playlistName;
                }
                catch (Exception ex)
                {
                    Jammer.Message.Data($"{Locale.OutsideItems.Error}: " + ex.Message, $"ERROR SAVING PLAYLIST", true);
                }
            }
        }

        static public void AutoSave()
        {
            if (!Preferences.isAutoSave)
            {
                return;
            }
            if (Utils.CurrentPlaylist == "")
            {
                return;
            }
            Save(Utils.CurrentPlaylist, true);
        }

        public static void PrintList()
        {
            Console.WriteLine($"{Locale.OutsideItems.Playlists}:");
            Console.WriteLine(GetList());
        }

        public static string GetList()
        {
            string playlistDir = Preferences.GetPlaylistsPath();
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
