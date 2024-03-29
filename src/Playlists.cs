using Spectre.Console;

namespace jammer
{
    public class Playlists
    {
        static public void Create(string playlist)
        {
            Console.WriteLine("Creating playlist: " + playlist + ".jammer");
            string playlistName = playlist;
            string playlistPath = Path.Combine(
                Utils.jammerPath,
                "playlists",
                playlistName + ".jammer"
            );

            if (File.Exists(playlistPath))
            {
                Console.WriteLine("Playlist already exists in " + playlistPath + ". Overwrite? (y/n)");
                // y/n prompt
                if (Console.ReadLine() == "y")
                {
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

        static public void Play(string playlist)
        {
            AnsiConsole.WriteLine("Starting up " + playlist + ".jammer");
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
                // Start.state = MainStates.playing;

                foreach (string song in songs)
                {
                    AnsiConsole.MarkupLine("[green]Playing " + song + "[/]");
                }
            }
            else
            {
                Message.Data("Playlist doesn't exist:" + playlist + ".jammer", "Error Playing Playlist", true);
                Environment.Exit(0);
            }
        }

        static public void Delete(string playlist)
        {
            Console.WriteLine("Deleting " + playlist + ".jammer");
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
                AnsiConsole.MarkupLine("[red]Playlist doesn't exist[/]");
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

            AnsiConsole.MarkupLine("[green]Adding songs to " + playlistPath + "[/]");
            if (File.Exists(playlistPath))
            {
                // take args and remove first 3 elements
                args = args.Skip(3).ToArray();

                // absoulutify arg if its a relative path and add https:// if url
                args = Absolute.Correctify(args);
                // get all songs in playlist
                string[] songs = File.ReadAllLines(playlistPath);

                // add songs to playlist
                for (int i = 0; i < args.Length; i++)
                {
                    string song = args[i];
                    AnsiConsole.MarkupLine("[green]Adding " + song + "[/]");

                    // check if song is already in playlist
                    if (songs.Contains(song))
                    {
                        AnsiConsole.MarkupLine("[red]" + song + " is already in playlist[/]"); 
                    }
                    else
                    {
                        File.AppendAllText(playlistPath, song + "\n");
                    }

                }
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Playlist: "+ playlistName + " doesn't exist[/]");
            }
            AnsiConsole.MarkupLine("[green]Done![/]");
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

            AnsiConsole.MarkupLine("[green]Removing songs from " + playlistPath + "[/]");
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
                    AnsiConsole.MarkupLine("[green]Removing " + song + "[/]");

                    // check if song is already in playlist
                    if (songs.Contains(song))
                    {
                        // delete song from playlist
                        songs = songs.Where(val => val != song).ToArray();
                        File.WriteAllLines(playlistPath, songs);
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[red]" + song + " is not in playlist[/]");
                    }

                }
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Playlist: " + playlistName + " doesn't exist[/]");
            }
            AnsiConsole.MarkupLine("[green]Done![/]");
            Environment.Exit(0);
        }

        static public void ShowCli(string playlist)
        {
            AnsiConsole.MarkupLine(GetShow(playlist));
        }

        static public string GetShow(string playlist)
        {
            AnsiConsole.MarkupLine("Showing playlist [red]" + playlist + "[/]");
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
                    playlistList = "[red]Playlist is empty[/]";
                }
                foreach (string song in songs)
                {
                    playlistList += "[green]" + song + "[/]" + "\n";
                }
            }
            else
            {
                playlistList = "[red]Playlist doesn't exist[/]";
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
                    string input = Message.Input("(y/n)", "Playlist already exists in " + playlistPath + ". Overwrite?");
                    // y/n prompt
                    if (input != "y")
                    {
                        return;
                    }
                }
                File.Delete(playlistPath);
                File.WriteAllLines(playlistPath, Utils.songs);
                Utils.currentPlaylist = playlistName;
            }
            else
            {
                File.WriteAllLines(playlistPath, Utils.songs);
                Utils.currentPlaylist = playlistName;
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

        static public void PrintList()
        {
            Console.WriteLine("Playlists:");
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
