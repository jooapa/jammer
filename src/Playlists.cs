using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Spectre.Console;
using jammer;
namespace jammer
{
    public class Playlists
    {
        string playlistPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jammer/playlists/";

        static public void Create(string playlist)
        {
            Console.WriteLine("Creating playlist: " + playlist + ".jammer");
            string playlistName = playlist;
            string playlistPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jammer/playlists/" + playlistName + ".jammer";
            if (System.IO.File.Exists(playlistPath))
            {
                Console.WriteLine("Playlist already exists in " + playlistPath + ". Overwrite? (y/n)");
                // y/n prompt
                if (Console.ReadLine() == "y")
                {
                    System.IO.File.Create(playlistPath);
                }
            }
        }

        static public void Play(string playlist)
        {
            Console.WriteLine("Starting up " + playlist + ".jammer");
            string playlistName = playlist;
            string playlistPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jammer/playlists/" + playlistName + ".jammer";
            if (System.IO.File.Exists(playlistPath))
            {
                string[] songs = System.IO.File.ReadAllLines(playlistPath);
                foreach (string song in songs)
                {
                    // remove all \n from song
                    string songPath = song.Replace("\n", "");
                }
                Program.textRenderedType = "normal";
                UI.Update();
                Program.Main(songs);
            }
            else
            {
                Console.WriteLine("Playlist does not exist");
            }
        }

        static public void Delete(string playlist)
        {
            Console.WriteLine("Deleting " + playlist + ".jammer");
            string playlistName = playlist;
            string playlistPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jammer/playlists/" + playlistName + ".jammer";

            if (System.IO.File.Exists(playlistPath))
            {
                System.IO.File.Delete(playlistPath);
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
                args = AbsolutefyPath.Absolutefy(args);

                // get all songs in playlist
                string[] songs = System.IO.File.ReadAllLines(playlistPath);

                // add songs to playlist
                for (int i = 3; i < args.Length; i++)
                {
                    string song = args[i];

                    // check if song is valid
                    if (System.IO.File.Exists(song) || song.StartsWith("https://") || song.StartsWith("http://"))
                    {
                        // check if song is already in playlist
                        if (songs.Contains(song))
                        {
                            Console.WriteLine("Song already in playlist");
                        }
                        else
                        {
                            System.IO.File.AppendAllText(playlistPath, song + "\n");
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
                args = AbsolutefyPath.Absolutefy(args);

                // get all songs in playlist
                string[] songs = System.IO.File.ReadAllLines(playlistPath);

                // remove songs from playlist
                for (int i = 3; i < args.Length; i++)
                {
                    string song = args[i];

                    // check if song is valid
                    if (System.IO.File.Exists(song) || song.StartsWith("https://") || song.StartsWith("http://"))
                    {
                        // check if song is already in playlist
                        if (songs.Contains(song))
                        {
                            // remove song from playlist
                            List<string> songsList = songs.ToList();
                            songsList.Remove(song);
                            songs = songsList.ToArray();
                            System.IO.File.WriteAllLines(playlistPath, songs);
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

            if (System.IO.File.Exists(playlistPath))
            {
                string[] songs = System.IO.File.ReadAllLines(playlistPath);
                foreach (string song in songs)
                {
                    AnsiConsole.MarkupLine("[green]" + song + "[/]");
                }
            }
            else
            {
                AnsiConsole.WriteLine("Playlist does not exist");
            }
        }

        static public void Save(string playlistName, string[] songs) {
            string playlistPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jammer/playlists/" + playlistName + ".jammer";
            // if playlist exists, overwrite it with y/n
            if (System.IO.File.Exists(playlistPath))
            {
                Console.WriteLine("Playlist already exists in " + playlistPath + ". Overwrite? (y/n)");
                // y/n prompt
                if (Console.ReadLine() == "y")
                {
                    System.IO.File.Create(playlistPath);
                }
            }
            System.IO.File.WriteAllLines(playlistPath, songs);
        }

        static public void List()
        {
            Console.WriteLine("Listing playlists");
            string[] playlists = System.IO.Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jammer/playlists/");
            foreach (string playlist in playlists)
            {
                AnsiConsole.WriteLine(playlist);
            }
        }
    }
}