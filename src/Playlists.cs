using Spectre.Console;

namespace jammer
{
    public class Playlists
    {
        static public void Create(string playlist)
        {
            Console.WriteLine("Creating playlist: " + playlist + ".jammer");
            string playlistName = playlist;
            string playlistPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jammer/playlists/" + playlistName + ".jammer";
            if (File.Exists(playlistPath))
            {
                Console.WriteLine("Playlist already exists in " + playlistPath + ". Overwrite? (y/n)");
                // y/n prompt
                if (Console.ReadLine() == "y")
                {
                    File.Create(playlistPath);
                }
            }
            else
            {
                File.Create(playlistPath);
            }
        }

        static public void Play(string playlist)
        {
            AnsiConsole.WriteLine("Starting up " + playlist + ".jammer");
            string playlistName = playlist;
            string playlistPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jammer/playlists/" + playlistName + ".jammer";
            if (File.Exists(playlistPath))
            {
                string[] songs = File.ReadAllLines(playlistPath);

                Utils.currentSongIndex = 0;
                Utils.songs = songs;

                jammer.Play.ResetMusic();
                Start.state = MainStates.playing;
                jammer.Play.PlaySong(Utils.songs, Utils.currentSongIndex);
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Playlist doesn't exist[/]");
                Console.ReadLine();
            }
        }

        static public void Delete(string playlist)
        {
            Console.WriteLine("Deleting " + playlist + ".jammer");
            string playlistName = playlist;
            string playlistPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jammer/playlists/" + playlistName + ".jammer";

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
            string playlistName = args[2];
            string playlistPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "jammer",
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
            string playlistName = args[2];
            string playlistPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "jammer",
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

        static public void Show(string playlist)
        {
            AnsiConsole.MarkupLine("Showing [red]" + playlist + ".jammer[/]");
            playlist = playlist + ".jammer";
            string playlistPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jammer/playlists/" + playlist;

            if (File.Exists(playlistPath))
            {
                string[] songs = File.ReadAllLines(playlistPath);
                if (songs.Length == 0)
                {
                    AnsiConsole.MarkupLine("[red]Playlist is empty[/]");
                }
                foreach (string song in songs)
                {
                    AnsiConsole.MarkupLine("[green]" + song + "[/]");
                }
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Playlist doesn't exist[/]");
                Console.ReadLine();
            }
        }

        static public void Save(string playlistName)
        {
            string playlistPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jammer/playlists/" + playlistName + ".jammer";
            // if playlist exists, overwrite it with y/n
            if (File.Exists(playlistPath))
            {
                Console.WriteLine("Playlist already exists in " + playlistPath + ". Overwrite? (y/n)");
                // y/n prompt
                if (Console.ReadLine() == "y")
                {
                    File.Delete(playlistPath);
                    File.WriteAllLines(playlistPath, Utils.songs);
                }
            }
            else
            {
                File.WriteAllLines(playlistPath, Utils.songs);
            }
        }

        static public void List()
        {
            ListOnly();
            Console.ReadLine();
        }

        static public void ListOnly() {
            Console.WriteLine("Listing playlists: ");
            string[] playlists = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jammer/playlists/");
            foreach (string playlist in playlists)
            {
                AnsiConsole.WriteLine(Path.GetFileName(playlist));
            }
        }
    }
}