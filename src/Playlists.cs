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
            Console.WriteLine("Starting up " + playlist + ".jammer");
            string playlistName = playlist;
            string playlistPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jammer/playlists/" + playlistName + ".jammer";
            if (File.Exists(playlistPath))
            {
                string[] songs = File.ReadAllLines(playlistPath);

                Utils.currentSongIndex = 0;
                Utils.songs = songs;

                jammer.Play.ResetMusic();
                jammer.Play.PlaySong(Utils.songs, Utils.currentSongIndex);
                Start.state = MainStates.play;
            }
            else
            {
                AnsiConsole.WriteLine("[red]Playlist does not exist[/]");
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
                AnsiConsole.MarkupLine("[red]Playlist does not exist[/]");
            }
        }

        static public void Add(string[] args)
        {
            Console.WriteLine("Adding songs to playlist");

            string playlistName = args[2];
            string playlistPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jammer/playlists/" + playlistName + ".jammer";
            if (System.IO.File.Exists(playlistPath))
            {
                // absoulutify arg if its a relative path and add https:// if url
                args = Absolute.Correctify(args);

                // get all songs in playlist
                string[] songs = System.IO.File.ReadAllLines(playlistPath);

                // add songs to playlist
                for (int i = 3; i < args.Length; i++)
                {
                    string song = args[i];

                    // check if song is valid
                    if (File.Exists(song) || song.StartsWith("https://") || song.StartsWith("http://"))
                    {
                        // check if song is already in playlist
                        if (songs.Contains(song))
                        {
                            Console.WriteLine("Song already in playlist");
                        }
                        else
                        {
                            File.AppendAllText(playlistPath, song + "\n");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Song does not exist: " + song);
                    }
                }
            }
            else
            {
                Console.WriteLine("Playlist does not exist");
            }

        }

        static public void Remove(string[] args)
        {
            Console.WriteLine("Removing songs from playlist");

            string playlistName = args[2];
            string playlistPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jammer/playlists/" + playlistName + ".jammer";
            if (System.IO.File.Exists(playlistPath))
            {
                // absoulutify arg if its a relative path and add https:// if url
                args = Absolute.Correctify(args);

                // get all songs in playlist
                string[] songs = File.ReadAllLines(playlistPath);

                // remove songs from playlist
                for (int i = 3; i < args.Length; i++)
                {
                    string song = args[i];

                    // check if song is valid
                    if (File.Exists(song) || song.StartsWith("https://") || song.StartsWith("http://"))
                    {
                        // check if song is already in playlist
                        if (songs.Contains(song))
                        {
                            // remove song from playlist
                            List<string> songsList = songs.ToList();
                            songsList.Remove(song);
                            songs = songsList.ToArray();
                            File.WriteAllLines(playlistPath, songs);
                        }
                        else
                        {
                            Console.WriteLine("Song not in playlist");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Song does not exist: " + song);
                    }
                }
            }
            else
            {
                Console.WriteLine("Playlist does not exist");
            }
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
                AnsiConsole.WriteLine("Playlist does not exist");
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
            Console.WriteLine("Listing playlists: ");
            string[] playlists = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jammer/playlists/");
            foreach (string playlist in playlists)
            {
                AnsiConsole.WriteLine(playlist);
            }
            Console.ReadLine();
        }
    }
}