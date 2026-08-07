using SoundCloudExplode;
using YoutubeExplode;
using YoutubeExplode.Common;
using Spectre.Console;
using System.IO;
using TagLib;
using System.Net;
using System.Text.RegularExpressions;
using YoutubeExplode.Videos.Streams;
using System.Runtime.InteropServices;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;

namespace Jammer
{
    public class Download
    {
        public static string songPath = "";
        public static Song? constructedSong = null;
        static string[] playlistSongs = { "" };
        public static readonly YoutubeClient youtube = new();
        private static readonly YtDlpManager YtDlp = new();

        public static (string, Song?) DownloadSong(string url)
        {
            songPath = "";
            constructedSong = null;

            Debug.dprint($"{Locale.OutsideItems.Downloading}: " + url.ToString());
            if (URL.IsValidSoundcloudSong(url))
            {
                DownloadSoundCloudTrackAsync(url).Wait();
            }
            else if (URL.IsValidYoutubeSong(url))
            {
                DownloadYoutubeTrackAsync(url).Wait();
            }
            else if (URL.IsValidRssFeed(url))
            {
                DownloadRssFeed(url).Wait();
            }
            else if (URL.IsUrl(url))
            {
                if (url.EndsWith(".jammer"))
                {
                    DownloadJammerFile(url).Wait();
                }
                else
                {
                    GeneralDownload(url).Wait();
                }
            }
            else
            {
                Console.WriteLine(Locale.OutsideItems.InvalidUrl);
                Debug.dprint("Invalid url");
            }

            Start.drawWhole = true;
            return (songPath, constructedSong);
        }

        private static async Task DownloadRssFeed(string url)
        {
            RootRssData rssData = await Rss.GetRssData(url);

            Log.Info($"RSS Title: {rssData.Title}");
            Log.Info($"RSS Author: {rssData.Author}");

            songPath = url;
            constructedSong = new Song
            {
                URI = url,
                Title = rssData.Title,
                Author = rssData.Author
            };
            return;
        }

        private static async Task GeneralDownload(string url)
        {
            string formattedUrl = FormatUrlForFilename(url, true);
            songPath = Path.Combine(
                Preferences.songsPath,
                formattedUrl
            );

            if (System.IO.File.Exists(songPath))
            {
                constructedSong = new Song
                {
                    URI = url,
                };
                return;
            }
            // AnsiConsole.MarkupLine($"{Locale.OutsideItems.Downloading}: {url} to {songPath}");

            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");
                    httpClient.Timeout = TimeSpan.FromMinutes(10);

                    var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(songPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        var buffer = new byte[8192];
                        int bytesRead;
                        long totalBytesRead = 0;
                        long totalBytes = response.Content.Headers.ContentLength ?? -1;

                        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            totalBytesRead += bytesRead;

                            if (totalBytes > 0)
                            {
                                double progressPercentage = (double)totalBytesRead / totalBytes * 100;
                                // Console.WriteLine($"{Locale.OutsideItems.Downloaded} {totalBytesRead} {Locale.OutsideItems.Of} {totalBytes} {Locale.OutsideItems.Bytes} ({progressPercentage:P}).");

                                TUI.PrintToTopOfPlayer($"{Locale.OutsideItems.Downloaded} {totalBytesRead} {Locale.OutsideItems.Of} {totalBytes} {Locale.OutsideItems.Bytes} ({progressPercentage:P}).");
                            }
                        }

                        constructedSong = new Song
                        {
                            URI = url,
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"{Locale.OutsideItems.ErrorDownload}" + ex.Message);
            }
        }

        private static string GetDownloadedJammerFileName(string url)
        {
            string downloadedPath = Path.Combine(Preferences.GetPlaylistsPath(), url);
            return downloadedPath.LastIndexOf("/") > 0 ? downloadedPath.Substring(downloadedPath.LastIndexOf("/") + 1) : downloadedPath;
        }

        private static async Task DownloadJammerFile(string url)
        {
            string downloadedPath = Path.Combine(Preferences.GetPlaylistsPath(), GetDownloadedJammerFileName(url));

            if (System.IO.File.Exists(downloadedPath))
            {
                string input = Message.Input(
                    Locale.UiMessages.Warning,
                    Locale.OutsideItems.AlreadyExists + " " + downloadedPath + ". " + Locale.OutsideItems.Overwrite + " " + Locale.Miscellaneous.YesNo,
                    true);
                if (!input.Equals(Locale.Miscellaneous.YesAnswer, StringComparison.OrdinalIgnoreCase))
                {
                    songPath = downloadedPath;
                    constructedSong = new Song
                    {
                        URI = url,
                        Title = Path.GetFileNameWithoutExtension(downloadedPath),
                    };
                    return;
                }
            }

            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");
                httpClient.Timeout = TimeSpan.FromMinutes(10);

                var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(downloadedPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                {
                    var buffer = new byte[8192];
                    int bytesRead;
                    long totalBytesRead = 0;
                    long totalBytes = response.Content.Headers.ContentLength ?? -1;

                    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead);
                        totalBytesRead += bytesRead;

                        if (totalBytes > 0)
                        {
                            double progressPercentage = (double)totalBytesRead / totalBytes * 100;
                            // Console.WriteLine($"{Locale.OutsideItems.Downloaded} {totalBytesRead} {Locale.OutsideItems.Of} {totalBytes} {Locale.OutsideItems.Bytes} ({progressPercentage:P}).");
                            TUI.PrintToTopOfPlayer($"{Locale.OutsideItems.Downloaded} {totalBytesRead} {Locale.OutsideItems.Of} {totalBytes} {Locale.OutsideItems.Bytes} ({progressPercentage:P}).");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"{Locale.OutsideItems.ErrorDownload} " + ex.Message, "Error id:1");
            }


            songPath = Path.Combine(Preferences.GetPlaylistsPath(), GetDownloadedJammerFileName(url));
            constructedSong = new Song
            {
                URI = url,
                Title = Path.GetFileNameWithoutExtension(songPath),
            };
        }

        private static async Task DownloadYoutubeTrackAsync(string url)
        {
            // Route to appropriate backend based on preference
            if (Preferences.backEndType == BackEndTypeYT.YoutubeDL)
            {
                await DownloadYoutubeTrackWithYtDlpAsync(url);
            }
            else
            {
                await DownloadYoutubeTrackWithYoutubeExplodeAsync(url);
            }
        }

        private static async Task DownloadYoutubeTrackWithYoutubeExplodeAsync(string url)
        {
            string formattedUrl = FormatUrlForFilename(url);

            songPath = Path.Combine(
                Preferences.songsPath,
                formattedUrl
            );

            if (System.IO.File.Exists(songPath))
            {
                constructedSong = new Song
                {
                    URI = url,
                };
                return;
            }

            // AnsiConsole.Cursor.SetPosition(3, 2);
            var theText = Locale.UiMessages.GettingTrack;

            TUI.PrintToTopOfPlayer(theText);

            try
            {
                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(url);
                var streamInfo = streamManifest.GetAudioStreams().TryGetWithHighestBitrate();
                var video = await youtube.Videos.GetAsync(url);

                if (streamInfo != null)
                {
                    string ffmpegPath = ResolveFFmpegPath();

                    // detect if ffmpeg is installed on the system in the path
                    if (!IsFFmpegInstalled() && !System.IO.File.Exists(ffmpegPath))
                    {
                        Message.Data(Locale.UiMessages.FfmpegMissing, Locale.OutsideItems.Error, true);
                        return;
                    }

                    // int lastPercentage = -1;  // Track last printed percentage
                    var progress = new Progress<double>(data =>
                    {
                        TUI.PrintToTopOfPlayer(theText + $" {data:P}");
                    });

                    await youtube.Videos.Streams.DownloadAsync(streamInfo, songPath, progress);

                    // if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    // If using Linux
                    //Message.Data(songPath, "Debug");
                    TUI.PrintToTopOfPlayer(Locale.UiMessages.ConvertingToOgg);

                    await FFMPEGConvert(songPath, new Song
                    {
                        Title = video.Title,
                        Author = video.Author.ChannelTitle
                    });

                    TUI.PrintToTopOfPlayer(Locale.UiMessages.TaggingSong);
                    // TagLib
                    // try
                    // {
                    //     var file = TagLib.File.Create(songPath);
                    //     file.Tag.Title = Start.Sanitize(video.Title);
                    //     file.Tag.Performers = new string[] { video.Author.ChannelTitle };
                    //     file.Tag.Album = video.Author.ChannelTitle;
                    //     file.Save();
                    // }
                    // catch (Exception ex)
                    // {
                    //     // Message.Data($"{Locale.OutsideItems.Error}: " + ex.Message, "Error id:234234");
                    //     Log.Error(ex.Message);
                    //     Log.Error("doing the dumb ://: title convert bullshit");
                    //     songPath += "://:" + video.Title + "://:" + video.Author.ChannelTitle;
                    // }
                    constructedSong = new Song
                    {
                        URI = url,
                        Title = video.Title,
                        Author = video.Author.ChannelTitle
                    };
                }
                else
                {
                    Message.Data(Locale.OutsideItems.NoAudioStream, Locale.OutsideItems.Error);
                }
            }
            catch (Exception ex)
            {
                if (Funcs.DontShowErrorWhenSongNotFound())
                {
                    Log.Error("Skipping song due to error: " + ex.Message);
                    return;
                }

                // Jammer.Message.Data($"{Locale.OutsideItems.Error}: " + ex.Message, "Error id:4");
                Utils.CustomTopErrorMessage = Locale.UiMessages.SongPrivateOrInvalidUrl;
                Log.Error(ex.Message);
                songPath = "";
                constructedSong = null;
            }
        }

        private static async Task DownloadYoutubeTrackWithYtDlpAsync(string url)
        {
            string formattedUrl = FormatUrlForFilename(url);

            songPath = Path.Combine(
                Preferences.songsPath,
                formattedUrl
            );

            if (System.IO.File.Exists(songPath))
            {
                constructedSong = new Song
                {
                    URI = url,
                };
                return;
            }

            var theText = "Getting track with yt-dlp. please wait...";
            TUI.PrintToTopOfPlayer(theText);

            try
            {
                var ytdl = new YoutubeDL();
                var installProgress = new Progress<double>(value =>
                    TUI.PrintToTopOfPlayer($"{Locale.Settings.InstallingYtDlp} {value:P0}"));
                YtDlpResolution resolution = await YtDlp.ResolveAsync(true, installProgress);
                if (!resolution.IsAvailable)
                {
                    throw new InvalidOperationException(Locale.Settings.NotInstalled);
                }
                ytdl.YoutubeDLPath = resolution.Path!;

                // Prefer the packaged ffmpeg and fall back to PATH.
                string ffmpegPath = ResolveFFmpegPath();
                if (System.IO.File.Exists(ffmpegPath) || IsFFmpegInstalled())
                {
                    ytdl.FFmpegPath = ffmpegPath;
                }
                else
                {
                    Message.Data(Locale.UiMessages.FfmpegMissing, Locale.OutsideItems.Error, true);
                    return;
                }

                Debug.dprint("Using yt-dlp at: " + ytdl.YoutubeDLPath);

                ytdl.OutputFolder = Preferences.songsPath;
                ytdl.OutputFileTemplate = "www.%(webpage_url_domain)s watch v=%(id)s";
                Directory.CreateDirectory(Utils.YtDlpCachePath);
                var ytDlpOptions = new OptionSet
                {
                    CacheDir = Utils.YtDlpCachePath
                };
                TUI.PrintToTopOfPlayer(Locale.UiMessages.DownloadingWithYtDlp);
                
                // Use simple approach to download best audio
                var result = await ytdl.RunAudioDownload(
                    url,
                    AudioConversionFormat.Vorbis,
                    overrideOptions: ytDlpOptions);

                if (result.Success && result.Data != null)
                {
                    // Move the downloaded file to our expected path
                    // var downloadedFile = result.Data;
                    // if (System.IO.File.Exists(downloadedFile))
                    // {
                    //     // Convert to OGG if needed
                    //     if (!downloadedFile.EndsWith(".ogg"))
                    //     {
                    //         // TUI.PrintToTopOfPlayer("Converting to OGG with ffmpeg...");
                    //         // await FFMPEGConvert(downloadedFile);
                            
                    //         // Replace original with converted
                    //         if (System.IO.File.Exists(songPath))
                    //         {
                    //             System.IO.File.Delete(downloadedFile);
                    //         }
                    //         else
                    //         {
                    //             System.IO.File.Move(downloadedFile, songPath);
                    //         }
                    //     }
                    //     else
                    //     {
                    //         System.IO.File.Move(downloadedFile, songPath);
                    //     }
                    // }

                    TUI.PrintToTopOfPlayer(Locale.UiMessages.GettingVideoInfo);
                    
                    // Get video info for metadata
                    var infoResult = await ytdl.RunVideoDataFetch(url, overrideOptions: ytDlpOptions);
                    if (infoResult.Success && infoResult.Data != null)
                    {
                        var videoData = infoResult.Data;
                        constructedSong = new Song
                        {
                            URI = url,
                            Title = videoData.Title ?? "",
                            Author = videoData.Uploader ?? ""
                        };
                    }
                    else
                    {
                        constructedSong = new Song
                        {
                            URI = url,
                        };
                    }
                }
                else
                {
                    throw new Exception("yt-dlp download failed: " + string.Join("; ", result.ErrorOutput ?? new string[0]) + " :: "+ytdl.YoutubeDLPath);
                }
            }
            catch (Exception ex)
            {
                if (Funcs.DontShowErrorWhenSongNotFound())
                {
                    Log.Error("Skipping song due to error: " + ex.Message);
                    return;
                }

                Utils.CustomTopErrorMessage = "Error: yt-dlp download failed. (check log)";
                Log.Error(ex.Message + " :: Could be that the youtube has changed once again something in their backend so this version of yt-dlp doesnt work?. or could also be that url is invalid or private or live.");
                songPath = "";
                constructedSong = null;
            }
        }

        private static string ResolveFFmpegPath()
        {
            string fileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffmpeg.exe" : "ffmpeg";
            string packaged = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            return System.IO.File.Exists(packaged) ? packaged : fileName;
        }

        private static bool IsFFmpegInstalled()
        {
            try
            {
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = "-version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using System.Diagnostics.Process? process = System.Diagnostics.Process.Start(startInfo);
                if (process == null)
                {
                    return false;
                }

                process.WaitForExit();
                return process.ExitCode == 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static Task FFMPEGConvert(string songPath, Song? metadata = null)
        {
            return Task.Run(() =>
            {

                string tempSongPath = songPath + ".ogg";

                string ffmpegPath = ResolveFFmpegPath();

                // detect if ffmpeg is installed on the system in the path
                if (!IsFFmpegInstalled() && !System.IO.File.Exists(ffmpegPath))
                {
                    Message.Data(Locale.UiMessages.FfmpegMissing, Locale.OutsideItems.Error, true);
                    return;
                }

                // Message.Data($"Converting {songPath} to {tempSongPath}", "Debug");

                string arguments;
                if (metadata != null)
                {
                    arguments = $"-y -i \"{songPath}\" -vn -acodec libvorbis -q:a 4 -metadata title=\"{metadata.Title}\" -metadata artist=\"{metadata.Author}\" -metadata album=\"{metadata.Album}\" -c:a libvorbis \"{tempSongPath}\"";
                    Log.Info("Converting to OGG with metadata" + arguments);
                }
                else
                {
                    Log.Info("Converting to OGG");
                    arguments = $"-y -i \"{songPath}\" -vn -acodec libvorbis -q:a 4 \"{tempSongPath}\"";
                }
                // Message.Data(arguments, "Debug");
                //Message.Data($"Converting {songPath} to {tempSongPath}", "Debug");
                System.Diagnostics.ProcessStartInfo startInfo = new()
                {
                    FileName = ffmpegPath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                System.Diagnostics.Process process = new System.Diagnostics.Process
                {
                    StartInfo = startInfo
                };

                try
                {
                    process.Start();
                    process.WaitForExit();
                    process.Close();
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                    Log.Error("FFMPEG failed to convert the file");

                    if (Funcs.DontShowErrorWhenSongNotFound())
                    {
                        Log.Error("Skipping song due to error: " + ex.Message);
                        return;
                    }

                    Message.Data(ex.ToString(), "Error id:323");
                }

                //Message.Data($"Conversion complete: {tempSongPath} to {songPath}", "Debug");

                // Replace the original file with the temporary file
                System.IO.File.Delete(songPath);
                System.IO.File.Move(tempSongPath, songPath);
                //Message.Data($"Conversion complete: {songPath}", "Debug");
            });
        }

        public static SoundCloudClient ReturnSoundCloudClient()
        {
            if (Preferences.clientID == "")
            {
                return new SoundCloudClient();
            }
            return new SoundCloudClient(Preferences.clientID);
        }

        public static async Task DownloadSoundCloudTrackAsync(string url)
        {
            // if already downloaded, don't download again
            string formattedUrl = FormatUrlForFilename(url);

            songPath = Path.Combine(
                Preferences.songsPath,
                formattedUrl
            );

            if (System.IO.File.Exists(songPath))
            {
                constructedSong = new Song
                {
                    URI = url
                };
                return;
            }

            var theText = Locale.UiMessages.GettingTrack;
            TUI.PrintToTopOfPlayer(theText);

            try
            {
                await DownloadSoundCloudTrackWithRetryAsync(url, theText);
                Utils.SCClientIdAlreadyLookedAndItsIncorrect = false;
            }
            catch (Exception ex)
            {
                bool clientIdIncorrect =
                    ex is HttpRequestException { StatusCode: HttpStatusCode.Unauthorized } ||
                    ex.Message.Contains("401", StringComparison.Ordinal);
                Utils.SCClientIdAlreadyLookedAndItsIncorrect = clientIdIncorrect;
                if (clientIdIncorrect)
                {
                    Utils.CustomTopErrorMessage = Locale.UiMessages.ClientIdIncorrect;
                }
                else
                {
                    Utils.CustomTopErrorMessage = Locale.UiMessages.SongNotFoundPrivateOrInvalid;
                }

                DeleteFailedSoundCloudDownload();
                songPath = "";
                constructedSong = null;
                if (Funcs.DontShowErrorWhenSongNotFound())
                {
                    Log.Error("Skipping song due to error: " + ex.Message);
                    return;
                }
                // Message.Data($"{Locale.OutsideItems.Error}: " + ex.Message + ": " + url
                // , Locale.OutsideItems.DownloadErrorSoundcloud + "\nMaybe your Client ID is invalid or the song is private");
                // Utils.CustomTopErrorMessage = "Error: Maybe your Client ID is invalid or the song is private. (check log)";
                Log.Error(ex.Message);
            }
        }

        private static async Task DownloadSoundCloudTrackWithRetryAsync(string url, string statusText)
        {
            try
            {
                await DownloadSoundCloudTrackOnceAsync(ReturnSoundCloudClient(), url, statusText);
            }
            catch (Exception firstFailure)
            {
                Log.Error("Initial SoundCloud download failed: " + firstFailure.Message);
                DeleteFailedSoundCloudDownload();
                TUI.PrintToTopOfPlayer(Locale.Settings.FetchingSoundCloudId);

                try
                {
                    string clientId = await SCClientIdFetcher.GetClientId();
                    Preferences.clientID = clientId;
                    Utils.SCClientIdAlreadyLookedAndItsIncorrect = false;
                    Preferences.SaveSettings();
                }
                catch (Exception refreshFailure)
                {
                    // Still make the promised second attempt in case the original
                    // download failure was transient rather than client-ID related.
                    Log.Error("Failed to refresh the SoundCloud client ID: " + refreshFailure.Message);
                }

                TUI.PrintToTopOfPlayer(statusText);
                await DownloadSoundCloudTrackOnceAsync(ReturnSoundCloudClient(), url, statusText);
            }
        }

        private static void DeleteFailedSoundCloudDownload()
        {
            try
            {
                if (!string.IsNullOrEmpty(songPath) && System.IO.File.Exists(songPath))
                {
                    System.IO.File.Delete(songPath);
                }
            }
            catch (Exception cleanupFailure)
            {
                Log.Error("Failed to remove an incomplete SoundCloud download: " + cleanupFailure.Message);
            }
        }

        private static async Task DownloadSoundCloudTrackOnceAsync(
            SoundCloudClient soundcloud,
            string url,
            string statusText)
        {
            var track = await soundcloud.Tracks.GetAsync(url);
            if (track?.Title == null)
            {
                throw new InvalidOperationException("SoundCloud did not return a valid track.");
            }

            var progress = new Progress<double>(data =>
            {
                TUI.PrintToTopOfPlayer(statusText + $" {data:P}");
            });

            await soundcloud.DownloadAsync(track, songPath, progress);
            TUI.PrintToTopOfPlayer(Locale.UiMessages.TaggingSong);

            try
            {
                using (var file = TagLib.File.Create(songPath))
                {
                    file.Tag.Title = Start.Sanitize(track.Title);
                    file.Tag.Description = track.Description;
                    if (track.User?.Username != null)
                    {
                        file.Tag.Performers = new[] { track.User.Username };
                    }
                    file.Save();
                }
                if (track.ArtworkUrl != null)
                {
                    await DownloadThumbnailAsync(track.ArtworkUrl, songPath);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                Log.Error("doing the dumb ://: title convert bullshit");
            }

            constructedSong = new Song
            {
                URI = url,
                Title = track.Title,
                Author = track.User?.Username
            };
        }

        static async Task DownloadThumbnailAsync(Uri imageUrl, string songPath)
        {
            using var file = TagLib.File.Create(songPath);
            using WebClient webClient = new();
            byte[] imageBytes = webClient.DownloadData(imageUrl);
            Picture picture = new(imageBytes);
            file.Tag.Pictures = Array.Empty<IPicture>();
            file.Tag.Pictures = new[] { picture };
            file.Save();
        }

        public static async Task GetPlaylist(string plurl)
        {

            var soundcloud = ReturnSoundCloudClient();

            // Get all playlist tracks
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine(Locale.UiMessages.GettingPlaylistTracks);
            try
            {
                var playlist = await soundcloud.Playlists.GetAsync(plurl, true);

                if (playlist == null)
                {
                    Console.WriteLine(Locale.OutsideItems.NoTrackPlaylist);
                    Message.Data(Locale.UiMessages.ClientIdInvalidOrPlaylistPrivate, Locale.OutsideItems.Error);
                    return;
                }

                if (playlist.Tracks.Count() == 0 || playlist.Tracks == null)
                {
                    Console.WriteLine(Locale.OutsideItems.NoTrackPlaylist);
                    Console.ReadLine();
                    return;
                }

                // add all tracks permalinkUrl to songs array
                playlistSongs = new string[playlist.Tracks.Count()];
                int i = 0;
                foreach (var track in playlist.Tracks)
                {
                    playlistSongs[i] = track.PermalinkUrl?.ToString() ?? string.Empty;
                    i++;
                }

                // debug
                foreach (var song in playlistSongs)
                {
                    Console.WriteLine(song);
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"{Locale.OutsideItems.Error}: ", "Error id:3");
                Console.WriteLine(ex.Message);
                Environment.Exit(1);
            }
        }
        public static async Task GetPlaylistYoutube(string plurl)
        {
            // Get all playlist tracks
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine(Locale.UiMessages.GettingPlaylistTracks);
            var playlist = await youtube.Playlists.GetVideosAsync(plurl);
            Console.WriteLine(playlist[0]);
            if (playlist.Count() == 0 || playlist == null)
            {
                Console.WriteLine(Locale.OutsideItems.NoTrackPlaylist);
                Console.ReadLine();
                return;
            }

            // add all tracks permalinkUrl to songs array
            playlistSongs = new string[playlist.Count()];
            int i = 0;
            foreach (var track in playlist)
            {
                var _url = track.Url?.ToString() ?? string.Empty;
                var index = _url.IndexOf('&');
                if (index != -1)
                {
                    _url = _url.Substring(0, index);
                }
                playlistSongs[i] = _url;
                i++;
            }
        }
        public static string GetSongsFromPlaylist(string plurl, string service)
        {
            if (service == "soundcloud")
            {
                GetPlaylist(plurl).Wait();
            }
            else if (service == "youtube")
            {
                GetPlaylistYoutube(plurl).Wait();

            }

            // print all the songs in the playlist
            // foreach (var song in Utils.songs) {
            //     Console.WriteLine(song);
            // }
            // Console.ReadLine();
            // remove the plurl from the songs array if it exists any where
            Utils.Songs = Utils.Songs.Where(val => !val.Contains(plurl)).ToArray();


            // Console.WriteLine("----Utils.songs----");
            // foreach (var song in Utils.songs) {
            //     Console.WriteLine(song);
            // }
            // Console.WriteLine("--------");
            // Console.WriteLine("----playlistSongs----");
            // foreach (var song in playlistSongs) {
            //     Console.WriteLine(song);
            // }
            // Console.WriteLine("--------");
            // Console.ReadLine();
            // Message.Data("current song: " + Utils.songs[Utils.currentSongIndex], "Debug");
            // Message.Data("playlist first song: " + playlistSongs[0], "Debug");
            // Message.Data("last song: " + Utils.songs[Utils.songs.Length - 1], "Debug");

            // add all songs from playlist to Utils.songs but start adding at the currentSongIndex
            Utils.Songs = Utils.Songs.Take(Utils.CurrentSongIndex).Concat(playlistSongs).Concat(Utils.Songs.Skip(Utils.CurrentSongIndex)).ToArray();
            // Message.Data(Utils.songs[0], "d1");
            // Console.WriteLine("----Utils.songs----");
            // foreach (var song in Utils.songs) {
            //     Console.WriteLine(song);
            // }
            // Console.WriteLine("--------");
            // Console.WriteLine("----playlistSongs)----");
            // foreach (var song in playlistSongs) {
            //     Console.WriteLine(song);
            // }
            // Console.WriteLine("--------");
            // Console.ReadLine();


            return DownloadSong(Utils.Songs[Utils.CurrentSongIndex]).Item1;
        }
        public static string FormatUrlForFilename(string url, bool isCheck = false)
        {
            if (URL.isValidSoundCloudPlaylist(url))
            {
                return "Soundcloud Playlist";
            }
            else if (URL.IsValidSoundcloudSong(url))
            {
                // remove ? and everything after
                int index = url.IndexOf("?");
                if (index > 0)
                {
                    url = url.Substring(0, index);
                }

                string formattedSCUrl = url.Replace("https://", "")
                                     .Replace("/", " ")
                                     .Replace("?", " ");
                if (isCheck)
                {
                    return formattedSCUrl;
                }
                else
                {
                    return formattedSCUrl + ".mp3";
                }
            }
            else if (URL.IsValidYoutubeSong(url))
            {
                int index = url.IndexOf("&");
                if (index > 0)
                {
                    url = url.Substring(0, index);
                }
            }
            else
            {
                // remove every thing that cant be in filename
                url = url.Replace("https://", "")
                            .Replace("/", " ")
                            .Replace("?", " ");
                return url;
            }
            string formattedYTUrl = url.Replace("https://", "")
                                     .Replace("/", " ")
                                     .Replace("?", " ");



            if (isCheck)
            {
                return formattedYTUrl;
            }
            else
            {
                return formattedYTUrl + ".ogg";
            }
        }
    }
}
