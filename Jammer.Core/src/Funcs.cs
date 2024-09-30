using Spectre.Console;
using System.Globalization;
using System.Linq;
using System.IO;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Jammer {

    public static class Funcs {
        
        static public string[] GetAllSongs() {
            if (Utils.songs.Length == 0) {
                string[] returnstring = {Themes.sColor("No songs in playlist", Themes.CurrentTheme.Playlist.InfoColor)}; // "No songs in playlist"
                return returnstring;
            }

            List<string> results = new()
            {
                Themes.sColor($"{Locale.OutsideItems.CurrPlaylistView} {Keybindings.PlaylistViewScrollup}, {Keybindings.PlaylistViewScrolldown}", Themes.CurrentTheme.Playlist.InfoColor),
                Themes.sColor($"{Locale.OutsideItems.PlaySongWith} {Keybindings.Choose}. {Locale.OutsideItems.DeleteSongWith} {Keybindings.DeleteCurrentSong}.", Themes.CurrentTheme.Playlist.InfoColor),
            };

            int maximum = 7;
            
            int songLength = Start.consoleWidth - 17; // 26

            for (int i = 0; i < Utils.songs.Length; i++) {
                string keyValue = Utils.songs[i].ToString();
                            
                if (i >= IniFileHandling.ScrollIndexLanguage && results.Count != maximum) {
                    if (i == Utils.currentSongIndex) {
                        // results.Add($"[green]{i + 1}. {Start.Sanitize(Play.Title(keyValue, "get"))}[/]");
                        results.Add(Themes.sColor($"{i + 1}. {GetSongWithDots(Start.Sanitize(SongExtensions.Title(keyValue)), songLength)}", Themes.CurrentTheme.WholePlaylist.CurrentSongColor));
                    }
                    else if (i == Utils.currentPlaylistSongIndex) {
                        // results.Add($"[yellow]{i + 1}. {Start.Sanitize(Play.Title(keyValue, "get"))}[/]");
                        results.Add(Themes.sColor($"{i + 1}. {GetSongWithDots(Start.Sanitize(SongExtensions.Title(keyValue)), songLength)}", Themes.CurrentTheme.WholePlaylist.ChoosingColor));
                    }
                    else if (Utils.currentPlaylistSongIndex <= 3) {
                        // results.Add($"{i + 1}. {Start.Sanitize(Play.Title(keyValue, "get"))}");
                        results.Add(Themes.sColor($"{i + 1}. {GetSongWithDots(Start.Sanitize(SongExtensions.Title(keyValue)), songLength)}", Themes.CurrentTheme.WholePlaylist.NormalSongColor));
                    }
                    else if (i >= Utils.currentPlaylistSongIndex - 2 && i < Utils.currentPlaylistSongIndex + 3) {
                        // results.Add($"{i + 1}. {Start.Sanitize(Play.Title(keyValue, "get"))}");
                        results.Add(Themes.sColor($"{i + 1}. {GetSongWithDots(Start.Sanitize(SongExtensions.Title(keyValue)), songLength)}", Themes.CurrentTheme.WholePlaylist.NormalSongColor));
                    }
                    else if (i >= Utils.songs.Length - (maximum - results.Count)) {
                        // results.Add($"{i + 1}. {Start.Sanitize(Play.Title(keyValue, "get"))}");
                        results.Add(Themes.sColor($"{i + 1}. {GetSongWithDots(Start.Sanitize(SongExtensions.Title(keyValue)), songLength)}", Themes.CurrentTheme.WholePlaylist.NormalSongColor));
                    }
                }
            }

            return results.ToArray();
        }

        public static string[] GetAllSongsQueue() {
            int maximum = 10;
            List<string> results = new();
            for (int i = 0; i < Utils.queueSongs.Count; i++) {
                if (results.Count != maximum) {
                    results.Add(Utils.queueSongs[i]);
                }
            }

            return Start.Sanitize(results.ToArray());
        }

        public static string GetSongWithDots(string song, int length = 80) {
            var textElementEnumerator = StringInfo.GetTextElementEnumerator(song);
            var textElements = new List<string>();
            while (textElementEnumerator.MoveNext()) {
                textElements.Add(textElementEnumerator.GetTextElement());
            }

            if (textElements.Count > length) {
                song = "..." + string.Concat(textElements.Skip(textElements.Count - length));
            }
            return song;
        }
        public static string GetPrevCurrentNextSong() {
            // return previous, current and next song in playlist
            string prevSong;
            string nextSong;
            string currentSong;
            int songLength = Start.consoleWidth - 26;
            if (Utils.songs.Length == 0)
            {
                currentSong = $"[grey]{Locale.Player.Current}  : -[/]";
            }
            else
            {
                currentSong = Locale.Player.Current + "  : " + GetSongWithDots(Start.Sanitize(SongExtensions.Title(Utils.songs[Utils.currentSongIndex])), songLength);
                currentSong = Themes.sColor(currentSong, Themes.CurrentTheme.GeneralPlaylist.CurrentSongColor);
            }

            if (Utils.currentSongIndex > 0)
            {
                prevSong = Locale.Player.Previos + " : " + GetSongWithDots(Start.Sanitize(SongExtensions.Title(Utils.songs[Utils.currentSongIndex - 1])), songLength);
                prevSong = Themes.sColor(prevSong, Themes.CurrentTheme.GeneralPlaylist.PreviousSongColor);
            }
            else
            {
                prevSong = $"{Locale.Player.Previos} : -";
                prevSong = Themes.sColor(prevSong, Themes.CurrentTheme.GeneralPlaylist.NoneSongColor);
            }


            if (Utils.currentSongIndex < Utils.songs.Length - 1)
            {
                nextSong = $"{Locale.Player.Next}     : " + GetSongWithDots(Start.Sanitize(SongExtensions.Title(Utils.songs[Utils.currentSongIndex + 1])), songLength);
                nextSong = Themes.sColor(nextSong, Themes.CurrentTheme.GeneralPlaylist.NextSongColor);
            }
            else
            {
                nextSong = $"{Locale.Player.Next}     : -";
                nextSong = Themes.sColor(nextSong, Themes.CurrentTheme.GeneralPlaylist.NoneSongColor);
            }      
            
            return prevSong + $"\n[green]" + currentSong + "[/]\n" + nextSong;
        }

        public static string CalculateTime(double time, bool getColor) {
            int minutes = (int)time / 60;
            int seconds = (int)time % 60;
            string timeString = $"{minutes}:{seconds:D2}";
            
            if (getColor) {
                return Themes.sColor(timeString, Themes.CurrentTheme.Time.TimeColor);
            }
            return timeString;
        }

        public static void PlaylistInput() {
            AnsiConsole.Markup($"\n{Locale.PlaylistOptions.EnterPlayListCmd} \n");
            AnsiConsole.MarkupLine($"[grey]1. {Locale.PlaylistOptions.AddSongToPlaylist}[/]");
            AnsiConsole.MarkupLine($"[grey]2. {Locale.PlaylistOptions.Deletesong}[/]");
            AnsiConsole.MarkupLine($"[grey]3. {Locale.PlaylistOptions.ShowSongs}[/]");
            AnsiConsole.MarkupLine($"[grey]4. {Locale.PlaylistOptions.ListAll}[/]");
            AnsiConsole.MarkupLine($"[grey]5. {Locale.PlaylistOptions.PlayOther}[/]");
            AnsiConsole.MarkupLine($"[grey]6. {Locale.PlaylistOptions.SaveReplace}[/]");
            // AnsiConsole.MarkupLine($"[grey]7. {Locale.PlaylistOptions.GoToSong}[/]");
            AnsiConsole.MarkupLine($"[grey]8. {Locale.PlaylistOptions.Shuffle}[/]");
            AnsiConsole.MarkupLine($"[grey]9. {Locale.PlaylistOptions.PlaySong}[/]");
            AnsiConsole.MarkupLine($"[grey]0. {Locale.PlaylistOptions.Exit}[/]");

            var playlistInput = Console.ReadKey(true).Key;
            // if (playlistInput == "" || playlistInput == null) { return; }
            switch (playlistInput) {
                // Add song to playlist
                case ConsoleKey.D1:
                    AddSongToPlaylist();
                    break;
                // Delete current song from playlist
                case ConsoleKey.D2:
                    DeleteCurrentSongFromPlaylist();
                    break;
                // Show songs in playlist
                case ConsoleKey.D3:
                    ShowSongsInPlaylist();
                    break;
                // List all playlists
                case ConsoleKey.D4:
                    ListAllPlaylists();
                    break;
                // Play other playlist
                case ConsoleKey.D5:
                    PlayOtherPlaylist();
                    break;
                // Save/replace playlist
                case ConsoleKey.D6:
                    SaveReplacePlaylist();
                    break;
                // Goto song in playlist
                case ConsoleKey.D7:
                    GotoSongInPlaylist();
                    break;
                    
                // Shuffle playlist (randomize)
                case ConsoleKey.D8:
                    ShufflePlaylist();
                    break;
                // Play single song
                case ConsoleKey.D9:
                    PlaySingleSong();
                    break;
                // Exit
                case ConsoleKey.D0:
                    return;
            }
            AnsiConsole.Clear();
        }

        public static void AddSongToPlaylist()
        {
            string songToAdd = Jammer.Message.Input(Locale.Player.AddSongToPlaylistMessage1, Locale.Player.AddSongToPlaylistMessage2);
            if (songToAdd == "" || songToAdd == null) {
                Jammer.Message.Data(Locale.Player.AddSongToPlaylistError1, Locale.Player.AddSongToPlaylistError2, true);
                return;
            }
            // remove quotes from songToAdd
            songToAdd = songToAdd.Replace("\"", "");
            if (!IsValidSong(songToAdd)) {
                Jammer.Message.Data( Locale.Player.AddSongToPlaylistError3+ " " + songToAdd, Locale.Player.AddSongToPlaylistError4, true);
                return;
            }
            songToAdd = Absolute.Correctify(new string[] { songToAdd })[0];
            Play.AddSong(songToAdd);
        }

        // Delete current song from playlist
        public static void DeleteCurrentSongFromPlaylist()
        {
            Play.DeleteSong(Utils.currentSongIndex, false);
        }

        // Show songs in playlist
        public static void ShowSongsInPlaylist()
        {
            string? playlistNameToShow = Jammer.Message.Input(Locale.Player.ShowSongsInPlaylistMessage1, Locale.Player.ShowSongsInPlaylistMessage2);
            if (playlistNameToShow == "" || playlistNameToShow == null) { 
                Jammer.Message.Data(Locale.Player.ShowSongsInPlaylistError1, Locale.Player.ShowSongsInPlaylistError2, true);
                return;
            }
            AnsiConsole.Clear();
            // show songs in playlist
            Jammer.Message.Data(Playlists.GetShow(playlistNameToShow), Locale.Player.SongsInPlaylist +" "+ playlistNameToShow);
        }

        // List all playlists
        public static void ListAllPlaylists()
        {
            Jammer.Message.Data(Playlists.GetList(), Locale.Player.AllPlaylists);
        }

        // Play other playlist
        public static void PlayOtherPlaylist()
        {
            string? playlistNameToPlay = Message.Input(Locale.Player.PlayOtherPlaylistMessage1,Locale.Player.PlayOtherPlaylistMessage2);
            if (playlistNameToPlay == "" || playlistNameToPlay == null) { 
                Jammer.Message.Data(Locale.Player.PlayOtherPlaylistError1, Locale.Player.PlayOtherPlaylistError2, true);
                return;
            }

            // play other playlist
            Playlists.Play(playlistNameToPlay, false);
        }

        // Save/replace playlist
        public static void SaveReplacePlaylist()
        {
            string playlistNameToSave = Jammer.Message.Input(Locale.Player.SaveReplacePlaylistMessage1, Locale.Player.SaveReplacePlaylistMessage2);
            if (playlistNameToSave == "" || playlistNameToSave == null) {
                Jammer.Message.Data(Locale.Player.SaveReplacePlaylistError1, Locale.Player.SaveReplacePlaylistError2, true);
                return;
            }
            // save playlist
            Playlists.Save(playlistNameToSave);
        }

        public static void SaveCurrentPlaylist()
        {
            if (Utils.currentPlaylist == "") {
                SaveReplacePlaylist();
            }
            // save playlist
            Playlists.Save(Utils.currentPlaylist, true);
        }

        public static void SaveAsPlaylist()
        {
            string playlistNameToSave = Jammer.Message.Input(Locale.Player.SaveAsPlaylistMessage1, Locale.Player.SaveAsPlaylistMessage2);
            if (playlistNameToSave == "" || playlistNameToSave == null) {
                Jammer.Message.Data(Locale.Player.SaveAsPlaylistError1, Locale.Player.SaveAsPlaylistError2, true);
                return;
            }
            // save playlist
            Playlists.Save(playlistNameToSave);
        }

        // Goto song in playlist
        public static void GotoSongInPlaylist()
        {
            string songToGoto = Jammer.Message.Input(Locale.Player.GotoSongInPlaylistMessage1, Locale.Player.GotoSongInPlaylistMessage2);
            if (songToGoto == "" || songToGoto == null) {
                Jammer.Message.Data(Locale.Player.GotoSongInPlaylistError1, Locale.Player.GotoSongInPlaylistError2, true);
                return;
            }
            // songToGoto = GotoSong(songToGoto);
        }

        // Shuffle playlist (randomize)
        public static void ShufflePlaylist()
        {
            // get the name of the current song
            string currentSong = Utils.songs[Utils.currentSongIndex];
            // shuffle playlist
            Play.Shuffle();

            Utils.currentSongIndex = Array.IndexOf(Utils.songs, currentSong);
            // set new song from shuffle to the current song
            Utils.currentSong = Utils.songs[Utils.currentSongIndex];
        }

        // Play single song
        public static void PlaySingleSong()
        {
            string[]? songsToPlay = Jammer.Message.Input(Locale.Player.PlaySingleSongMessage1, Locale.Player.PlaySingleSongMessage2).Split(" ");
            
            if (songsToPlay == null || songsToPlay.Length == 0) {
                Jammer.Message.Data(Locale.Player.PlaySingleSongError1, Locale.Player.PlaySingleSongError2, true);
                return;
            }

            // if blank "  " remove
            songsToPlay = songsToPlay.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

            songsToPlay = Absolute.Correctify(songsToPlay);
            
            // if no songs left, return
            if (songsToPlay.Length == 0) { return; }

            Utils.songs = songsToPlay;
            Utils.currentSongIndex = 0;
            Utils.currentPlaylist = "";
            Play.StopSong();
            Play.PlaySong(Utils.songs, Utils.currentSongIndex);
        }

        public static bool IsValidSong(string song) {
            if (File.Exists(song) || URL.IsUrl(song) || Directory.Exists(song)) {
                AnsiConsole.Markup($"\n[green]{Locale.Player.ValidSong}[/]");
                return true;
            }
            AnsiConsole.Markup($"\n[red]{Locale.Player.InvalidSong}[/]");
            return false;
        }

        public static bool IsDirectory(string path) {
            if (Directory.Exists(path)) {
                return true;
            }
            return false;
        }

        public static bool IsFile(string path) {
            if (File.Exists(path)) {
                return true;
            }
            return false;
        }

    }

    public class YTSearchResult {
        public string Id { get; set; }
        public string Title { get; set; }
        public TimeSpan? Duration { get; set; }
        public string Type { get; set; }
    }

    public class SCSearchResult {
        public string Url { get; set; }
        public string Title { get; set; }
    }
}
