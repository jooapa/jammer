using Spectre.Console;
using System.Globalization;
using System.Linq;
using System.IO;
using System;
using System.Collections.Generic;
using System.Text.Json;
using AngleSharp.Text;
using System.Text.RegularExpressions;

namespace Jammer
{

    public static class Funcs
    {

        static public void UpdateSongListCorrectly()
        {
            for (int i = Utils.Songs.Length - 1; i > 0; i--)
            {

                Song song = new()
                {
                    URI = Utils.Songs[i]
                };
                song.ExtractSongDetails();

                string fullPath = Path.GetFullPath(song.URI);

                TagLib.File? tagFile;
                string title = "", author = "", album = "", year = "", genre = "";
                try
                {
                    tagFile = TagLib.File.Create(fullPath);
                    title = tagFile.Tag.Title;
                    author = tagFile.Tag.FirstPerformer;
                    album = tagFile.Tag.Album;
                    year = tagFile.Tag.Year.ToString();
                    genre = tagFile.Tag.FirstGenre;
                }
                catch (Exception)
                {
                    tagFile = null;
                    // Log.Error("Error getting title of the song");
                }


                if (song.Title == null || song.Title == "")
                {
                    song.Title = title;
                }
                if (song.Author == null || song.Author == "")
                {
                    song.Author = author;
                }
                if (song.Album == null || song.Album == "")
                {
                    song.Album = album;
                }
                if (song.Year == null || song.Year == "" || song.Year == "0")
                {
                    song.Year = year;
                }
                if (song.Genre == null || song.Genre == "")
                {
                    song.Genre = genre;
                }

                // Utils.songs[Utils.currentSongIndex] = song.ToSongString();
                Utils.Songs[i] = SongExtensions.ToSongString(song);
            }

        }

        static public string[] GetAllSongs()
        {
            if (Utils.Songs.Length == 0)
            {
                string[] returnstring = { Themes.sColor("No songs in playlist", Themes.CurrentTheme.Playlist.InfoColor) }; // "No songs in playlist"
                return returnstring;
            }

            List<string> results = new()
            {
                Themes.sColor($"{Locale.OutsideItems.CurrPlaylistView} {Keybindings.PlaylistViewScrollup}, {Keybindings.PlaylistViewScrolldown}", Themes.CurrentTheme.Playlist.InfoColor),
                Themes.sColor($"{Locale.OutsideItems.PlaySongWith} {Keybindings.Choose}. {Locale.OutsideItems.DeleteSongWith} {Keybindings.DeleteCurrentSong}.", Themes.CurrentTheme.Playlist.InfoColor),
            };

            int maximum = 7;

            int songLength = Start.consoleWidth - 17; // 26

            for (int i = 0; i < Utils.Songs.Length; i++)
            {
                string keyValue = Utils.Songs[i].ToString();

                if (i >= IniFileHandling.ScrollIndexLanguage && results.Count != maximum)
                {
                    if (i == Utils.CurrentSongIndex)
                    {
                        // results.Add($"[green]{i + 1}. {Start.Sanitize(Play.Title(keyValue, "get"))}[/]");
                        results.Add(Themes.sColor($"{i + 1}. {GetSongWithDots(Start.Sanitize(SongExtensions.Title(keyValue)), songLength)}", Themes.CurrentTheme.WholePlaylist.CurrentSongColor));
                    }
                    else if (i == Utils.CurrentPlaylistSongIndex)
                    {
                        // results.Add($"[yellow]{i + 1}. {Start.Sanitize(Play.Title(keyValue, "get"))}[/]");
                        results.Add(Themes.sColor($"{i + 1}. {GetSongWithDots(Start.Sanitize(SongExtensions.Title(keyValue)), songLength)}", Themes.CurrentTheme.WholePlaylist.ChoosingColor));
                    }
                    else if (Utils.CurrentPlaylistSongIndex <= 3)
                    {
                        // results.Add($"{i + 1}. {Start.Sanitize(Play.Title(keyValue, "get"))}");
                        results.Add(Themes.sColor($"{i + 1}. {GetSongWithDots(Start.Sanitize(SongExtensions.Title(keyValue)), songLength)}", Themes.CurrentTheme.WholePlaylist.NormalSongColor));
                    }
                    else if (i >= Utils.CurrentPlaylistSongIndex - 2 && i < Utils.CurrentPlaylistSongIndex + 3)
                    {
                        // results.Add($"{i + 1}. {Start.Sanitize(Play.Title(keyValue, "get"))}");
                        results.Add(Themes.sColor($"{i + 1}. {GetSongWithDots(Start.Sanitize(SongExtensions.Title(keyValue)), songLength)}", Themes.CurrentTheme.WholePlaylist.NormalSongColor));
                    }
                    else if (i >= Utils.Songs.Length - (maximum - results.Count))
                    {
                        // results.Add($"{i + 1}. {Start.Sanitize(Play.Title(keyValue, "get"))}");
                        results.Add(Themes.sColor($"{i + 1}. {GetSongWithDots(Start.Sanitize(SongExtensions.Title(keyValue)), songLength)}", Themes.CurrentTheme.WholePlaylist.NormalSongColor));
                    }
                }
            }

            return results.ToArray();
        }

        public static bool DontShowErrorWhenSongNotFound()
        {
            if (Utils.PlaylistCheckedForAllTheSongsAndNoneOfThemWereFound)
            {
                // Message.Data("a","a");
                return false;
            }

            if (Preferences.isSkipErrors)
            {
                // Message.Data("b", "b");
                return true;
            }

            // Message.Data("c", "c");

            return false;
        }

        public static string[] GetAllSongsQueue()
        {
            int maximum = 10;
            List<string> results = new();
            for (int i = 0; i < Utils.QueueSongs.Count; i++)
            {
                if (results.Count != maximum)
                {
                    results.Add(Utils.QueueSongs[i]);
                }
            }

            return Start.Sanitize(results.ToArray());
        }

        public static int GetTerminalWidth(string text)
        {
            int width = 0;
            foreach (var c in text.Normalize(System.Text.NormalizationForm.FormKC))
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(c);
                // CJK Unified Ideographs, Hangul, Hiragana, Katakana, Fullwidth forms
                if (IsFullWidth(c))
                    width += 2;
                else
                    width += 1;
            }
            return width;
        }

        // Helper to check if a char is fullwidth
        public static bool IsFullWidth(char c)
        {
            int code = (int)c;
            // CJK Unified Ideographs
            if ((code >= 0x4E00 && code <= 0x9FFF) ||
                (code >= 0x3400 && code <= 0x4DBF) ||
                (code >= 0xF900 && code <= 0xFAFF) ||
                (code >= 0xFF01 && code <= 0xFF60) || // Fullwidth ASCII variants
                (code >= 0xFFE0 && code <= 0xFFE6) || // Fullwidth symbol variants
                (code >= 0x1100 && code <= 0x11FF) || // Hangul Jamo
                (code >= 0x3040 && code <= 0x309F) || // Hiragana
                (code >= 0x30A0 && code <= 0x30FF))   // Katakana
                return true;
            return false;
        }

        public static string PadAuthorToRight(string author, string title, int consoleWidth, int strpadding)
        {
            if (string.IsNullOrEmpty(author)) return string.Empty;
            string realAuthor = ArtistsToArtist(author);
            int titleWidth = GetTerminalWidth(title);
            int authorWidth = GetTerminalWidth(realAuthor);

            int remainingSpace = consoleWidth - (strpadding + titleWidth + 12);

            if (remainingSpace <= 0) return string.Empty;
            if (authorWidth > remainingSpace) return string.Empty;

            return new string(' ', remainingSpace - authorWidth) + realAuthor;
        }

        public static string GetSongWithDots(string song, int length = 80)
        {
            var textElementEnumerator = StringInfo.GetTextElementEnumerator(song);
            var textElements = new List<string>();
            while (textElementEnumerator.MoveNext())
            {
                textElements.Add(textElementEnumerator.GetTextElement());
            }

            if (textElements.Count > length)
            {
                song = "..." + string.Concat(textElements.Skip(textElements.Count - length));
            }
            return song;
        }
        
        public static void ResetRssExitVariables() {
            Utils.BackUpSongs = null;
            Utils.BackUpPlaylistName = null;
            Utils.RssFeedSong = new Song();
            Utils.RssFeedSavedName = null;
        }

        public static bool IsInsideOfARssFeed()
        {
            if (Utils.RssFeedSong.URI != null)
            {
                return true;
            }
            return false;
        }

        public static bool IsCurrentSongARssFeed()
        {
            if (Utils.Songs.Length == 0 || Utils.CurrentSongIndex >= Utils.Songs.Length)
            {
                return false;
            }

            var currentSong = SongExtensions.ToSong(Utils.Songs[Utils.CurrentSongIndex]);
            if (currentSong.URI != null && URL.IsValidRssFeed(currentSong.URI))
            {
                return true;
            }
            return false;
        }
        public static string GetPrevCurrentNextSong(
            string? currentLabel = null,
            string? prevLabel = null,
            string? nextLabel = null)
        {
            int songLength = Start.consoleWidth - 26;

            // Always pad the labels to the maximum length
            int maxLabelLength = Math.Max(
            Math.Max(Locale.Player.Current.Length, Locale.Player.Previos.Length),
            Locale.Player.Next.Length
            );

            currentLabel = (currentLabel ?? Locale.Player.Current).PadRight(maxLabelLength);
            prevLabel = (prevLabel ?? Locale.Player.Previos).PadRight(maxLabelLength);
            nextLabel = (nextLabel ?? Locale.Player.Next).PadRight(maxLabelLength);

            // Check if we have songs and valid indices
            if (Utils.Songs == null || Utils.Songs.Length == 0 || Utils.CurrentSongIndex < 0 || Utils.CurrentSongIndex >= Utils.Songs.Length)
            {
                string emptyCurrent = $"{currentLabel} : -";
                string emptyPrev = $"{prevLabel} : -";
                string emptyNext = $"{nextLabel} : -";

                emptyCurrent = Start.Sanitize(emptyCurrent);
                emptyPrev = Start.Sanitize(emptyPrev);
                emptyNext = Start.Sanitize(emptyNext);

                // Apply colors for empty state
                emptyCurrent = Themes.sColor(emptyCurrent, Themes.CurrentTheme.GeneralPlaylist.NoneSongColor);
                emptyPrev = Themes.sColor(emptyPrev, Themes.CurrentTheme.GeneralPlaylist.NoneSongColor);
                emptyNext = Themes.sColor(emptyNext, Themes.CurrentTheme.GeneralPlaylist.NoneSongColor);

                emptyCurrent = RemoveControlChars(emptyCurrent);
                emptyPrev = RemoveControlChars(emptyPrev);
                emptyNext = RemoveControlChars(emptyNext);

                var emptyText = $"{emptyPrev}\n{emptyCurrent}\n{emptyNext}";
                return emptyText.Normalize(System.Text.NormalizationForm.FormC);
            }

            // Get song strings with consistent formatting
            string customAfterText_current = "";
            string customAfterText_prev = "";
            string customAfterText_next = "";

            string curStringSong = Utils.Songs[Utils.CurrentSongIndex] ?? "";
            string prevStringSong = Utils.CurrentSongIndex > 0 ? Utils.Songs[Utils.CurrentSongIndex - 1] ?? "" : "";
            string nextStringSong = Utils.CurrentSongIndex < Utils.Songs.Length - 1 ? Utils.Songs[Utils.CurrentSongIndex + 1] ?? "" : "";

            // assign "star" to the song if isFavorite
            if (SongExtensions.IsFavorite(curStringSong)){
                customAfterText_current += " ★";
                
            }
            // if the current song is a rss feed then add a open text to the end of the title
            if (IsCurrentSongARssFeed())
                customAfterText_current += " (Open with " + Keybindings.Choose + ")";

            if (SongExtensions.IsFavorite(prevStringSong))
                customAfterText_prev += " ★";

            if (SongExtensions.IsFavorite(nextStringSong))
                customAfterText_next += " ★";

            string currentSong = $"{currentLabel} : {GetSongWithDots(SongExtensions.Title(curStringSong) + customAfterText_current, songLength)}"
                + PadAuthorToRight(SongExtensions.Author(curStringSong),
                            SongExtensions.Title(curStringSong) + customAfterText_current,
                            Start.consoleWidth, currentLabel.Length);

            string prevSong = Utils.CurrentSongIndex > 0
                ? $"{prevLabel} : {GetSongWithDots(SongExtensions.Title(Utils.Songs[Utils.CurrentSongIndex - 1]) + customAfterText_prev, songLength)}"
                + PadAuthorToRight(SongExtensions.Author(Utils.Songs[Utils.CurrentSongIndex - 1]),
                            SongExtensions.Title(Utils.Songs[Utils.CurrentSongIndex - 1]) + customAfterText_prev,
                            Start.consoleWidth, prevLabel.Length)
                : $"{prevLabel} : -";

            string nextSong = Utils.CurrentSongIndex < Utils.Songs.Length - 1
                ? $"{nextLabel} : {GetSongWithDots(SongExtensions.Title(Utils.Songs[Utils.CurrentSongIndex + 1]) + customAfterText_next, songLength)}"
            + PadAuthorToRight(SongExtensions.Author(Utils.Songs[Utils.CurrentSongIndex + 1]),
                        SongExtensions.Title(Utils.Songs[Utils.CurrentSongIndex + 1]) + customAfterText_next,
                        Start.consoleWidth, nextLabel.Length)
            : $"{nextLabel} : -";

            prevSong = Start.Sanitize(prevSong);
            currentSong = Start.Sanitize(currentSong);
            nextSong = Start.Sanitize(nextSong);

            // Apply colors
            currentSong = Themes.sColor(currentSong, Utils.Songs.Length == 0
                ? Themes.CurrentTheme.GeneralPlaylist.NoneSongColor
                : Themes.CurrentTheme.GeneralPlaylist.CurrentSongColor);
            prevSong = Themes.sColor(prevSong, Utils.CurrentSongIndex > 0
                ? Themes.CurrentTheme.GeneralPlaylist.PreviousSongColor
                : Themes.CurrentTheme.GeneralPlaylist.NoneSongColor);
            nextSong = Themes.sColor(nextSong, Utils.CurrentSongIndex < Utils.Songs.Length - 1
                ? Themes.CurrentTheme.GeneralPlaylist.NextSongColor
                : Themes.CurrentTheme.GeneralPlaylist.NoneSongColor);

            prevSong = RemoveControlChars(prevSong);
            currentSong = RemoveControlChars(currentSong);
            nextSong = RemoveControlChars(nextSong);

            var text = $"{prevSong}\n{currentSong}\n{nextSong}";
            string normalized = text.Normalize(System.Text.NormalizationForm.FormC);
            return normalized;
        }

        public static string ArtistsToArtist(string artists)
        {
            if (artists == null || artists == "")
            {
                return "";
            }
            string[] splitters = { ", ", " & ", " and ", " + ", " / ", " | ", " x ", " X " };
            string[] parts = artists.Split(splitters, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return artists;
            }
            // get the first partq
            string firstPart = parts[0].Trim();
            if (firstPart == "")
            {
                return artists;
            }
            return firstPart;
        }

        public static string RemoveControlChars(string input)
        {
            return new string(input.Where(c => !char.IsControl(c)).ToArray());
        }

        public static string GetPlaylistPositionText(string processedPlaylistName = "")
        {
            if (Utils.Songs.Length == 0 || !Preferences.showPlaylistPosition)
            {
                return "";
            }
            
            int currentPosition = Utils.CurrentSongIndex + 1; // Convert to 1-based indexing
            int totalSongs = Utils.Songs.Length;
            string positionText = $"{currentPosition}/{totalSongs}";
            
            // Use the same calculation as PadAuthorToRight
            int strpadding = 9; // Length of "playlist " prefix
            int titleWidth = GetTerminalWidth(processedPlaylistName);
            
            // Use exact same formula as PadAuthorToRight: consoleWidth - (strpadding + titleWidth + 12)
            int remainingSpace = Start.consoleWidth - (strpadding + titleWidth + 12);
            
            if (remainingSpace <= 0) return ""; // No space available
            
            // Truncate position text with ... if it doesn't fit, similar to GetSongWithDots
            int positionWidth = GetTerminalWidth(positionText);
            
            if (positionWidth > remainingSpace)
            {
                // Try to fit "...X/Y" format where we truncate numbers if needed
                string minPositionText = "...1/1"; // Minimum expected format
                if (GetTerminalWidth(minPositionText) > remainingSpace)
                {
                    return ""; // Not enough space even for minimal format
                }
                
                // Truncate the position text to fit
                string truncatedPosition = GetPositionWithDots(positionText, remainingSpace);
                return new string(' ', remainingSpace - GetTerminalWidth(truncatedPosition)) + truncatedPosition;
            }
            
            return new string(' ', remainingSpace - positionWidth) + positionText;
        }
        
        private static string GetPositionWithDots(string positionText, int maxLength)
        {
            // If original fits, return it
            if (GetTerminalWidth(positionText) <= maxLength)
            {
                return positionText;
            }
            
            // Try "...X/Y" format by progressively truncating
            for (int len = maxLength; len >= 4; len--) // Minimum "...1" is 4 chars
            {
                if (len >= positionText.Length) continue;
                
                string truncated = "..." + positionText.Substring(positionText.Length - (len - 3));
                if (GetTerminalWidth(truncated) <= maxLength)
                {
                    return truncated;
                }
            }
            
            // Last resort - just return dots
            return "...";
        }

        public static async Task ContinueToRss() {
            // when opening the new view its actually gonna save the playlist aand come back to it to the same position it left.
            Utils.lastPositionInPreviousPlaylist = Utils.CurrentSongIndex;
            Utils.BackUpSongs = Utils.Songs;
            Utils.RssFeedSong = SongExtensions.ToSong(Utils.Songs[Utils.CurrentPlaylistSongIndex]);
            Utils.BackUpPlaylistName = Utils.CurrentPlaylist;
            Utils.CurrentPlaylist = "";
            // Message.Data(Utils.BackUpPlaylistName, "a");


            if (Utils.CurrentPlaylist != "")
            {
                Funcs.SaveCurrentPlaylist();
            }


            // convert all the rssfeeds to songs
            RootRssData rssFeed = await Rss.GetRssData(Utils.RssFeedSong.URI);
            // state = MainStates.next;
            // break;
            Utils.Songs = Array.Empty<string>();
            Utils.CurrentPlaylistSongIndex = 0;
            Utils.CurrentSongIndex = 0;

            foreach (var i in rssFeed.Content)
            {
                // Convert each RSS feed item to a song
                Song song = new Song
                {
                    Title = i.Title,
                    Author = i.Author,
                    URI = i.Link,
                    Description = i.Description,
                    PubDate = i.PubDate
                };

                song.ExtractSongDetails();
                Utils.Songs = Utils.Songs.Concat(new[] { song.ToSongString() }).ToArray();
            }
        }

        public static string CalculateTime(double time, bool getColor)
        {
            int minutes = (int)time / 60;
            int seconds = (int)time % 60;
            string timeString = $"{minutes}:{seconds:D2}";

            if (getColor)
            {
                return Themes.sColor(timeString, Themes.CurrentTheme.Time.TimeColor);
            }
            return timeString;
        }

        public static void PlaylistInput()
        {
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
            switch (playlistInput)
            {
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
            if (songToAdd == "" || songToAdd == null)
            {
                Jammer.Message.Data(Locale.Player.AddSongToPlaylistError1, Locale.Player.AddSongToPlaylistError2, true);
                return;
            }
            // remove quotes from songToAdd
            songToAdd = songToAdd.Replace("\"", "");
            if (!IsValidSong(songToAdd))
            {
                Jammer.Message.Data(Locale.Player.AddSongToPlaylistError3 + " " + songToAdd, Locale.Player.AddSongToPlaylistError4, true);
                return;
            }
            string[] songsToAdd = Absolute.Correctify(new string[] { songToAdd });
            // Play.AddSong(songToAdd);
            foreach (string song in songsToAdd)
            {
                Play.AddSong(song);
            }
        }

        public static void ToggleCurrentSongToFavorites()
        {
            if (Utils.Songs == null || Utils.Songs.Length == 0)
            {
                Message.Data(Locale.Player.AddCurrentSongToFavoritesNoSong, Locale.Player.AddCurrentSongToFavoritesTitle, true);
                return;
            }

            if (Utils.CurrentSongIndex < 0 || Utils.CurrentSongIndex >= Utils.Songs.Length)
            {
                Message.Data(Locale.Player.AddCurrentSongToFavoritesError, Locale.Player.AddCurrentSongToFavoritesTitle, true);
                return;
            }

            string currentSong = Utils.Songs[Utils.CurrentSongIndex];
            if (string.IsNullOrWhiteSpace(currentSong))
            {
                Message.Data(Locale.Player.AddCurrentSongToFavoritesError, Locale.Player.AddCurrentSongToFavoritesTitle, true);
                return;
            }

            Song song = SongExtensions.ToSong(currentSong);
            if (SongExtensions.IsFavorite(song.ToSongString()))
            {
                song.IsFavorite = null;
                Utils.Songs[Utils.CurrentSongIndex] = song.ToSongString();
            }
            else
            {
                song.IsFavorite = "true";
                Utils.Songs[Utils.CurrentSongIndex] = SongExtensions.ToSongString(song);
                if (Preferences.favoriteExplainer)
                {
                    Message.Data("You have marked this song as favorite. \nYou can play all your favorite songs by playing the current playlist name with ':fav' appended. i.e. [bold]example:fav[/]\nor you can add play the playlist by path and adding -fav to the extension i.e. [bold]example.jammer-fav[/]\nYou can remove this message from the settings. ([bold]" + Keybindings.SettingsKeys.FavoriteExplainer.ToString() + "[/])", "Favorite song added");
                }
            }

        }

        // Delete current song from playlist
        public static void DeleteCurrentSongFromPlaylist()
        {
            Play.DeleteSong(Utils.CurrentSongIndex, false);
        }

        // Show songs in playlist
        public static void ShowSongsInPlaylist()
        {
            string? playlistNameToShow = Jammer.Message.Input(Locale.Player.ShowSongsInPlaylistMessage1, Locale.Player.ShowSongsInPlaylistMessage2);
            if (playlistNameToShow == "" || playlistNameToShow == null)
            {
                Jammer.Message.Data(Locale.Player.ShowSongsInPlaylistError1, Locale.Player.ShowSongsInPlaylistError2, true);
                return;
            }
            AnsiConsole.Clear();
            // show songs in playlist
            Jammer.Message.Data(Playlists.GetShow(playlistNameToShow), Locale.Player.SongsInPlaylist + " " + playlistNameToShow);
        }

        // List all playlists
        public static void ListAllPlaylists()
        {
            Jammer.Message.Data(Playlists.GetList(), Locale.Player.AllPlaylists);
        }

        // Play other playlist
        public static void PlayOtherPlaylist()
        {
            string? playlistNameToPlay = Message.Input(Locale.Player.PlayOtherPlaylistMessage1, Locale.Player.PlayOtherPlaylistMessage2);
            if (playlistNameToPlay == "" || playlistNameToPlay == null)
            {
                Jammer.Message.Data(Locale.Player.PlayOtherPlaylistError1, Locale.Player.PlayOtherPlaylistError2, true);
                return;
            }

            // when playing another playlist inside the player need to reset this, without it will not check all the songs again and will not work right in the DontShowErrorWhenSongNotFound()
            Utils.PlaylistCheckedForAllTheSongsAndNoneOfThemWereFound = false;
            
            // play other playlist
            Playlists.Play(playlistNameToPlay, false);
        }

        // Save/replace playlist
        public static void SaveReplacePlaylist()
        {
            string playlistNameToSave = Jammer.Message.Input(Locale.Player.SaveReplacePlaylistMessage1, Locale.Player.SaveReplacePlaylistMessage2);
            if (playlistNameToSave == "" || playlistNameToSave == null)
            {
                Jammer.Message.Data(Locale.Player.SaveReplacePlaylistError1, Locale.Player.SaveReplacePlaylistError2, true);
                return;
            }
            // save playlist
            Playlists.Save(playlistNameToSave);
        }

        public static void SaveCurrentPlaylist()
        {
            if (Utils.CurrentPlaylist == "")
            {
                SaveReplacePlaylist();
            }
            // save playlist
            Playlists.Save(Utils.CurrentPlaylist, true);
        }

        public static void SaveAsPlaylist()
        {
            string playlistNameToSave = Jammer.Message.Input(Locale.Player.SaveAsPlaylistMessage1, Locale.Player.SaveAsPlaylistMessage2);
            if (playlistNameToSave == "" || playlistNameToSave == null)
            {
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
            if (songToGoto == "" || songToGoto == null)
            {
                Jammer.Message.Data(Locale.Player.GotoSongInPlaylistError1, Locale.Player.GotoSongInPlaylistError2, true);
                return;
            }
            // songToGoto = GotoSong(songToGoto);
        }

        // Shuffle playlist (randomize)
        public static void ShufflePlaylist()
        {
            // get the name of the current song
            string currentSong = Utils.Songs[Utils.CurrentSongIndex];
            // shuffle playlist
            Play.Shuffle();

            Utils.CurrentSongIndex = Array.IndexOf(Utils.Songs, currentSong);
            // set new song from shuffle to the current song
            Utils.CurrentSongPath = Utils.Songs[Utils.CurrentSongIndex];
        }

        // Play single song
        public static void PlaySingleSong()
        {
            string[]? songsToPlay = Jammer.Message.Input(Locale.Player.PlaySingleSongMessage1, Locale.Player.PlaySingleSongMessage2).Split(" ");

            if (songsToPlay == null || songsToPlay.Length == 0)
            {
                Jammer.Message.Data(Locale.Player.PlaySingleSongError1, Locale.Player.PlaySingleSongError2, true);
                return;
            }

            // if blank "  " remove
            songsToPlay = songsToPlay.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

            songsToPlay = Absolute.Correctify(songsToPlay);

            // if no songs left, return
            if (songsToPlay.Length == 0) { return; }

            Utils.Songs = songsToPlay;
            Utils.CurrentSongIndex = 0;
            Utils.CurrentPlaylist = "";
            Play.StopSong();
            Play.PlaySong(Utils.Songs, Utils.CurrentSongIndex);
        }

        public static bool IsValidSong(string song)
        {
            if (File.Exists(song) || URL.IsUrl(song) || Directory.Exists(song))
            {
                AnsiConsole.Markup($"\n[green]{Locale.Player.ValidSong}[/]");
                return true;
            }
            AnsiConsole.Markup($"\n[red]{Locale.Player.InvalidSong}[/]");
            return false;
        }

        public static bool IsDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                return true;
            }
            return false;
        }

        public static bool IsFile(string path)
        {
            if (File.Exists(path))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Really fkn smart rename tool that will take in consideration all the possibilities
        /// things this does
        ///  - if author has " - Topic", it means that song title is correct, but just remove the " - Topic"
        ///  - check for the "-" and split (author - title)
        ///  - for each spit will take the element and split remove from the result if "[", "]", "(", ")", "{", "}", "ft.", "feat." in lower case
        ///  - if none of the above works, just return the original song
        /// </summary>
        /// <param name="song"></param>
        /// <returns></returns>
        public static Song SmartRename(Song song)
        {
            if (song == null || string.IsNullOrEmpty(song.URI))
            {
                return song ?? new Song();
            }
            string author = song.Author ?? "";
            string title = song.Title ?? "";

            if (author?.EndsWith(" - Topic") == true)
            {
                author = author.Replace(" - Topic", "").Trim();
                song.Author = author;

                return song;
            }

            if (!string.IsNullOrEmpty(title) && title.Contains("-"))
            {
                var parts = title.Split(new[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    author = parts[0].Trim();
                    title = parts[1].Trim();
                }
            }

            string[] patternsToRemove = new[]
            {
                @"\[(?>[^\[\]]+|\[(?<depth>)|\](?<-depth>))*(?(depth)(?!))\]",
                @"\((?>[^\(\)]+|\((?<depth>)|\)(?<-depth>))*(?(depth)(?!))\)",
                @"\{(?>[^\{\}]+|\{(?<depth>)|\}(?<-depth>))*(?(depth)(?!))\}",
                @"(?i)\s*ft\..*$",
                @"(?i)\s*feat\..*$",
                @"(?i)\s*\|.*$",
            };

            foreach (var pattern in patternsToRemove)
            {
                title = Regex.Replace(title, pattern, "").Trim();
                author = Regex.Replace(author, pattern, "").Trim();
            }

            // Remove multiple spaces
            title = Regex.Replace(title, @"\s+", " ");
            author = Regex.Replace(author, @"\s+", " ");

            song.Title = title;
            song.Author = author;

            return song;
        }
    }

    public class YTSearchResult
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public TimeSpan? Duration { get; set; }
        public string Type { get; set; }
        public string Author { get; set; }
    }

    public class SCSearchResult
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
    }
}
