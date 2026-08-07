using Spectre.Console;
using FuzzySharp;
using JRead;

namespace Jammer
{
    public static class Search
    {
        public static void SearchSongOnMediaPlatform()
        {
            string platform = Message.Input(Locale.UiMessages.SearchPlatformPrompt, Locale.Miscellaneous.SearchForSongOnYoutubeorSoundcloud, true);
            platform = platform.ToLower();

            if (platform == "youtube" || platform == "y")
            {
                string type = Message.Input("", Locale.UiMessages.YoutubeSearchTypePrompt, true);
                type = type.ToLower();

                if (type == "video" || type == "v" || type == "track" || type == "t")
                {
                    SearchYTSong("video");
                }
                else if (type == "playlist" || type == "p")
                {
                    SearchYTSong("playlist");
                }
            }
            else if (platform == "soundcloud" || platform == "s")
            {
                string type = Message.Input(Locale.UiMessages.SoundCloudSearchTypePrompt, Locale.Miscellaneous.SearchASongFromSoundcloudByName, true);
                type = type.ToLower();

                if (type == "track" || type == "t")
                {
                    SearchSCSong("track");
                }
                else if (type == "playlist" || type == "p")
                {
                    SearchSCSong("playlist");
                }
            }
        }
        public static void SearchYTSong(string type)
        {
            string search = Message.Input(Locale.EditKeysTexts.Search + ":", Locale.Miscellaneous.SearchASongFromYoutubeByName);
            if (search == "")
            {
                Start.drawWhole = true;
                return;
            }
            List<YTSearchResult> results = new();
            int indexer = 0;
            int max = 10;
            async Task loopedidoo_YT()
            {
                try
                {
                    if (type == "playlist")
                    {
                        await foreach (var result in Download.youtube.Search.GetPlaylistsAsync(search))
                        {
                            var id = result.Id;
                            var title = Markup.Escape(result.Title);
                            var author = Markup.Escape(result.Author.ToString());
                            string type = "playlist";
                            results.Add(new YTSearchResult { Id = id, Title = title, Type = type, Author = author });

                            if (indexer == max - 1)
                            {
                                break;
                            }
                            indexer++;
                        }
                    }
                    else if (type == "video")
                    {
                        await foreach (var result in Download.youtube.Search.GetVideosAsync(search))
                        {
                            var id = result.Id;
                            var title = Markup.Escape(result.Title);
                            var author = Markup.Escape(result.Author.ToString());
                            string type = "video";
                            results.Add(new YTSearchResult { Id = id, Title = title, Type = type, Author = author });

                            if (indexer == max - 1)
                            {
                                break;
                            }
                            indexer++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Message.Data(Locale.OutsideItems.Error + ": " + ex.Message, ":(");
                    return;
                }
            }
            loopedidoo_YT().Wait();

            if (results.Count() > 0)
            {
                var inputs = new CustomSelectInput[]
                {
                    
                };

                inputs = inputs.Concat(results.Select(r => new CustomSelectInput
                {
                    DataURI = r.Id,
                    Title = $"{r.Type}: {r.Title}",
                    Author = r.Author
                })).ToArray();

                AnsiConsole.Clear();
                string? answer = Message.CustomMenuSelect(inputs, string.Format(Locale.UiMessages.SearchResultsYoutube, search, results.Count, max), new CustomSelectInputSettings { StartIndex = 0 });
                
                if (answer == null || answer == "Cancel")
                {
                    Start.drawWhole = true;
                    return;
                }

                // Get the selected result
                var selectedResult = results.FirstOrDefault(r => r.Id == answer);
                if (selectedResult == null)
                {
                    Start.drawWhole = true;
                    return;
                }
                Song song = new Song()
                {
                    Title = selectedResult.Title
                };

                if (type == "playlist")
                {
                    song.URI = "https://www.youtube.com/playlist?list=" + selectedResult.Id;
                }
                else
                {
                    song.URI = "https://www.youtube.com/watch?v=" + selectedResult.Id;
                }
                song.ExtractSongDetails();
                string url = song.ToSongString();

                // add to the current playlist index +1
                Play.AddSong(url);
            }
            else
            {
                Message.Data(Locale.UiMessages.NoResultsFound, ":(");
            }
            Start.drawWhole = true;
        }
        public static void SearchSCSong(string type)
        {
            string search = Message.Input(Locale.EditKeysTexts.Search + ":", Locale.Miscellaneous.SearchASongFromSoundcloudByName);
            if (search == "")
            {
                Start.drawWhole = true;
                return;
            }
            List<SCSearchResult> results = new();
            int indexer = 0;
            int max = 10;
            async Task loopedidoo_SC()
            {

                try
                {
                    if (type == "playlist")
                    {
                        await foreach (var result in Download.ReturnSoundCloudClient().Search.GetPlaylistsAsync(search))
                        {
                            var url = result.Url;
                            if (url == null || url == "" || result.Title == null || result.Title == "")
                            {
                                continue;
                            }
                            var title = Markup.Escape(result.Title);
                            results.Add(new SCSearchResult { Url = url, Title = title });

                            if (indexer == max - 1)
                            {
                                break;
                            }
                            indexer++;
                        }
                    }
                    else if (type == "track")
                    {
                        await foreach (var result in Download.ReturnSoundCloudClient().Search.GetTracksAsync(search))
                        {
                            var url = result.Url;
                            var title = Markup.Escape(result.Title);
                            var author = Markup.Escape(result.User.ToString());
                            results.Add(new SCSearchResult { Url = url, Title = title, Author = author });

                            if (indexer == max - 1)
                            {
                                break;
                            }
                            indexer++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Message.Data(Locale.OutsideItems.Error + ": " + ex.Message, Locale.UiMessages.SoundCloudClientIdMayBeInvalid);
                    return;
                }
            }
            loopedidoo_SC().Wait();

            if (results.Count > 0)
            {
                var inputs = new CustomSelectInput[]
                {
                    
                };

                inputs = inputs.Concat(results.Select(r => new CustomSelectInput
                {
                    DataURI = r.Url,
                    Title = r.Title,
                    Author = r.Author
                })).ToArray();

                AnsiConsole.Clear();
                string? answer = Message.CustomMenuSelect(inputs, string.Format(Locale.UiMessages.SearchResultsSoundCloud, search, results.Count, max), new CustomSelectInputSettings { StartIndex = 0 });

                if (answer == null || answer == "Cancel")
                {
                    Start.drawWhole = true;
                    return;
                }

                // Get the selected result
                var selectedResult = results.FirstOrDefault(r => r.Url == answer);
                if (selectedResult == null)
                {
                    Start.drawWhole = true;
                    return;
                }
                // Utils.Song song = new Utils.Song();
                // song.Path = selectedUrl;
                // song.Title = selectedString;
                // string url = UtilFuncs.CombineToSongString(song);
                Song song = new Song()
                {
                    URI = selectedResult.Url,
                    Title = selectedResult.Title
                };
                song.ExtractSongDetails();
                string url = song.ToSongString();

                // add to the current playlist index +1
                Play.AddSong(url);
            }
            else
            {
                Message.Data(Locale.UiMessages.NoResultsFound, ":(");
            }
            Start.drawWhole = true;
        }
        public static void SearchForSongInPlaylistAsync()
        {
            List<string> songs = Utils.Songs.ToList();
            List<string> songTitles = Utils.Songs.Select(SongExtensions.Title).ToList();
            List<string> songAuthors = Utils.Songs.Select(SongExtensions.Author).ToList();

            var options = new JReadOptions
            {
                EnableAutoComplete = true,
                AutoCompleteItems = songTitles
            };

            string search = Message.Input("", Locale.UiMessages.SearchCurrentPlaylist, options: options);

            if (string.IsNullOrEmpty(search))
            {
                return;
            }

            // Check for exact match if QuickSearch is enabled
            if (Preferences.isQuickSearch)
            {
                var exactMatch = songTitles.FirstOrDefault(title => 
                    string.Equals(title, search.Trim(), StringComparison.OrdinalIgnoreCase));
                
                if (exactMatch != null)
                {
                    // Found exact match, play it directly
                    int exactIndex = songTitles.IndexOf(exactMatch);
                    Play.PlaySong(Utils.Songs, exactIndex);
                    return;
                }
            }

            // Perform fuzzy search using FuzzySharp
            var results = Process.ExtractTop(search, songTitles, limit: songTitles.Count)
                                .Where(result => result.Score > 50)
                                .Select(result => result.Value)
                                .ToList();

            if (results.Count > 0)
            {
                string[] resultsString = results.ToArray();
                resultsString = new[] { "Cancel" }.Concat(resultsString).ToArray();

                // Display the MultiSelect prompt after the loop completes
                AnsiConsole.Clear();
                // string answer = Message.MultiSelect(resultsString, $"Search results for '{search}' in the current playlist: {results.Count}");
                var inputs = new CustomSelectInput[]
                {
                    
                };

                inputs = inputs.Concat(results.Select(r => 
                {
                    int titleIndex = songTitles.IndexOf(r);
                    string author = titleIndex >= 0 && titleIndex < songAuthors.Count ? songAuthors[titleIndex] : "";
                    return new CustomSelectInput 
                    { 
                        DataURI = r, 
                        Title = r,
                        Author = author
                    };
                })).ToArray();

                if (Preferences.isQuickPlayFromSearch)
                {
                    // Automatically play the first result
                    string firstResult = results[0];
                    int index = songTitles.IndexOf(firstResult);
                    Play.PlaySong(Utils.Songs, index);
                    return;
                }

                string? answer = Message.CustomMenuSelect(inputs, string.Format(Locale.UiMessages.SearchResultsCurrentPlaylist, search, results.Count), new CustomSelectInputSettings { StartIndex = 0 });
                if (answer != null && answer != "Cancel")
                {
                    // Find the index of the selected song by the title in the songTitles list
                    int index = songTitles.IndexOf(answer);
                    Play.PlaySong(Utils.Songs, index);
                }
            }
            else
            {
                Message.Data(Locale.UiMessages.NoResultsFound, ":(");
            }
        }

        public static void SearchByAuthorAsync()
        {
            List<string> songs = Utils.Songs.ToList();
            List<string> songTitles = Utils.Songs.Select(SongExtensions.Title).ToList();
            List<string> songAuthors = Utils.Songs.Select(SongExtensions.Author).ToList();

            var uniqueAuthors = songAuthors.Where(a => !string.IsNullOrEmpty(a)).Distinct().ToList();

            var options = new JReadOptions
            {
                EnableAutoComplete = true,
                AutoCompleteItems = uniqueAuthors
            };

            string search = Message.Input("", Locale.UiMessages.SearchByAuthorCurrentPlaylist, options: options);

            if (string.IsNullOrEmpty(search))
            {
                return;
            }

            var results = Process.ExtractTop(search, uniqueAuthors, limit: uniqueAuthors.Count)
                                .Where(result => result.Score > 50)
                                .SelectMany(result =>
                                {
                                    // Find all songs by this author
                                    var authorName = result.Value;
                                    return songTitles
                                        .Select((title, index) => new { title, index, author = songAuthors[index] })
                                        .Where(item => string.Equals(item.author, authorName, StringComparison.OrdinalIgnoreCase))
                                        .Select(item => item.title);
                                })
                                .ToList();

            if (results.Count > 0)
            {
                var inputs = new CustomSelectInput[]
                {
            
                };

                inputs = inputs.Concat(results.Select(r =>
                {
                    int titleIndex = songTitles.IndexOf(r);
                    string author = titleIndex >= 0 && titleIndex < songAuthors.Count ? songAuthors[titleIndex] : "";
                    return new CustomSelectInput
                    {
                        DataURI = r,
                        Title = r,
                        Author = author
                    };
                })).ToArray();

                string? answer = Message.CustomMenuSelect(inputs, string.Format(Locale.UiMessages.SearchResultsByAuthor, search, results.Count));

                if (answer != null && answer != "Cancel")
                {
                    int songIndex = songTitles.IndexOf(answer);
                    if (songIndex >= 0)
                    {
                        Play.PlaySong(Utils.Songs, songIndex);
                    }
                }
            }
            else
            {
                Message.Data(Locale.UiMessages.NoResultsFound, ":(");
            }
        }
    }
}
