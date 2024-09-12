using Spectre.Console;

namespace Jammer
{
    public static class Search
    {
        public static void SearchSong() {
            string platform = Message.Input("Type 'y' for [red]Youtube[/] or 's' for [darkorange]SoundCloud[/]:", "Search for a song on Youtube or SoundCloud", true);
            platform = platform.ToLower();

            if (platform == "youtube" || platform == "y") {
                string type = Message.Input("Type 'v' for Video or 'p' for Playlist:", "[red]Youtube[/] Search for a Video or Playlist?", true);
                type = type.ToLower();

                if (type == "video" || type == "v" || type == "track" || type == "t") {
                    SearchYTSong("video");
                } else if (type == "playlist" || type == "p") {
                    SearchYTSong("playlist");
                }
            } else if (platform == "soundcloud" || platform == "s") {
                string type = Message.Input("Type 't' for Track or 'p' for Playlist:","[darkorange]Soundcloud[/] Search for a Track or Playlist?", true);
                type = type.ToLower();

                if (type == "track" || type == "t") {
                    SearchSCSong("track");
                } else if (type == "playlist" || type == "p") {
                    SearchSCSong("playlist");
                }
            }
        }
        public static void SearchYTSong(string type) {
            // TODO ADD LOCALE(s)
            string search = Message.Input("Search:", "Search a song from Youtube by its name");
            if (search == "") {
                Start.drawWhole = true;
                return;
            }
            List<YTSearchResult> results = new();
            int indexer = 0;
            int max = 10;
            async Task loopedidoo() {
                if (type == "playlist") {
                    await foreach (var result in Download.youtube.Search.GetPlaylistsAsync(search)) {
                        var id = result.Id;
                        var title = Markup.Escape(result.Title);
                        string type = "playlist";
                        results.Add(new YTSearchResult { Id = id, Title = title, Type = type });

                        if (indexer == max - 1) {
                            break;
                        }
                        indexer++;
                    }
                } else if (type == "video") {
                    await foreach (var result in Download.youtube.Search.GetVideosAsync(search)) {
                        var id = result.Id;
                        var title = Markup.Escape(result.Title);
                        string type = "video";
                        results.Add(new YTSearchResult { Id = id, Title = title, Type = type });

                        if (indexer == max - 1) {
                            break;
                        }
                        indexer++;
                    }
                }
            }
            loopedidoo().Wait();

            if (results.Count > 0) {
                string[] resultsString = results.Select(r => Markup.Escape(r.Type + ": " + r.Title)).ToArray();
                // add cancel to the list
                resultsString = new[] { "Cancel" }.Concat(resultsString).ToArray();

                // Display the MultiSelect prompt after the loop completes
                AnsiConsole.Clear();
                string answer = Message.MultiSelect(resultsString, "Search results for '" + search + "' on youtube: " + results.Count + "/" + max);
                // remove the cancel from the array
                if (answer == "Cancel") {
                    Start.drawWhole = true;
                    return;
                }

                // remove first index from the array
                resultsString = resultsString.Skip(1).ToArray();

                // Get the id of the selected song
                string selectedId = "";
                string selectedString = "";
                try{
                    selectedId = results[Array.IndexOf(resultsString, answer)].Id;
                    selectedString = results[Array.IndexOf(resultsString, answer)].Title;
                } catch {
                    // If the user cancels the selection
                    /*
                    Unhandled exception. System.ArgumentOutOfRangeException: Index was out of range. Must be non-negative and less than the size of the collection. (Parameter 'index')
                    at System.Collections.Generic.List`1.get_Item(Int32 index)
                    at Jammer.Start.CheckKeyboardAsync() in C:\Users\%USERNAME%\Documents\GitHub\jammer\Jammer.Core\src\Keyboard.cs:line 495
                    at Jammer.Start.Loop() in C:\Users\%USERNAME%\Documents\GitHub\jammer\Jammer.Core\src\Start.cs:line 373
                    */
                    Start.drawWhole = true;
                    return;
                }
                Song song = new Song() {
                    Title = selectedString
                };

                if (type == "playlist") {
                    song.URI = "https://www.youtube.com/playlist?list=" + selectedId;
                } else {
                    song.URI = "https://www.youtube.com/watch?v=" + selectedId;
                }
                song.ExtractSongDetails();
                string url = song.ToSongString();
                
                // add to the current playlist index +1
                Play.AddSong(url);
            } else {
                Message.Data("No results found", ":(");
            }
            Start.drawWhole = true;
        }
        public static void SearchSCSong(string type) {
            // TODO ADD LOCALE(s)
            string search = Message.Input("Search:", "Search a song from SoundCloud by its name");
            if (search == "") {
                Start.drawWhole = true;
                return;
            }
            List<SCSearchResult> results = new();
            int indexer = 0;
            int max = 10;
            async Task loopedidoo() {

                if (type == "playlist") {
                    await foreach (var result in Download.ReturnSoundCloudClient().Search.GetPlaylistsAsync(search)) {
                        var url = result.Url;
                        if (url == null || url == "" || result.Title == null || result.Title == "") {
                            continue;
                        }
                        var title = Markup.Escape(result.Title);
                        results.Add(new SCSearchResult { Url = url, Title = title});

                        if (indexer == max - 1) {
                            break;
                        }
                        indexer++;
                    }
                } else if (type == "track") {
                    await foreach (var result in Download.ReturnSoundCloudClient().Search.GetTracksAsync(search)) {
                        var url = result.Url;
                        var title = Markup.Escape(result.Title);
                        results.Add(new SCSearchResult { Url = url, Title = title});

                        if (indexer == max - 1) {
                            break;
                        }
                        indexer++;
                    }
                }
            }
            loopedidoo().Wait();

            if (results.Count > 0) {
                string[] resultsString = results.Select(r => Markup.Escape(r.Title)).ToArray();
                resultsString = new[] { "Cancel" }.Concat(resultsString).ToArray();

                // Display the MultiSelect prompt after the loop completes
                AnsiConsole.Clear();
                string answer = Message.MultiSelect(resultsString, "Search results for '" + search + "' on SoundCloud: " + results.Count + "/" + max);

                // remove the cancel from the array
                if (answer == "Cancel") {
                    Start.drawWhole = true;
                    return;
                }

                // remove first index from the array
                resultsString = resultsString.Skip(1).ToArray();

                // Get the url of the selected song
                string selectedUrl = "";
                string selectedString = "";
                try{
                    selectedUrl = results[Array.IndexOf(resultsString, answer)].Url;
                    selectedString = results[Array.IndexOf(resultsString, answer)].Title;
                } catch {
                    // If the user cancels the selection
                    /*
                    Unhandled exception. System.ArgumentOutOfRangeException: Index was out of range. Must be non-negative and less than the size of the collection. (Parameter 'index')
                    at System.Collections.Generic.List`1.get_Item(Int32 index)
                    at Jammer.Start.CheckKeyboardAsync() in C:\Users\%USERNAME%\Documents\GitHub\jammer\Jammer.Core\src\Keyboard.cs:line 495
                    at Jammer.Start.Loop() in C:\Users\%USERNAME%\Documents\GitHub\jammer\Jammer.Core\src\Start.cs:line 373
                    */
                    Start.drawWhole = true;
                    return;
                }
                // Utils.Song song = new Utils.Song();
                // song.Path = selectedUrl;
                // song.Title = selectedString;
                // string url = UtilFuncs.CombineToSongString(song);
                Song song = new Song() {
                    URI = selectedUrl,
                    Title = selectedString
                };
                song.ExtractSongDetails();
                string url = song.ToSongString();
                
                // add to the current playlist index +1
                Play.AddSong(url);
            } else {
                Message.Data("No results found", ":(");
            }
            Start.drawWhole = true;
        }
    }
}