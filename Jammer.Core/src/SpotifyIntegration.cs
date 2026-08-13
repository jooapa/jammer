using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;
using SpotifyAPI.Web;
using YoutubeExplode.Search;
using static SpotifyAPI.Web.Scopes;

namespace Jammer;

public sealed record SpotifyPlaylistSummary(
    string Id,
    string Name,
    string Owner,
    string? SnapshotId,
    string? SpotifyUrl,
    bool IsImported);

public sealed record SpotifyTrackMetadata(
    string Id,
    string Title,
    string Artist,
    string? Album,
    string? ReleaseYear,
    string? SpotifyUrl);

public sealed class SpotifyPlaylistImport
{
    public string SpotifyPlaylistId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Owner { get; set; } = "";
    public string? SpotifyUrl { get; set; }
    public string PlaylistPath { get; set; } = "";
    public string? SnapshotId { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class SpotifyImportRegistry
{
    public List<SpotifyPlaylistImport> Playlists { get; set; } = new();
}

public sealed class SpotifyAuthState
{
    public string ClientId { get; set; } = "";
    public PKCETokenResponse Token { get; set; } = new();
}

/// <summary>
/// Spotify metadata import using Authorization Code with PKCE. Spotify audio is never requested.
/// Imported metadata is independently resolved to a public media URL by the selected Jammer provider.
/// </summary>
public sealed class SpotifyIntegrationService
{
    public static readonly Uri RedirectUri = new("http://127.0.0.1:5543/callback/");
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> PlaylistLocks = new(StringComparer.OrdinalIgnoreCase);
    private readonly SemaphoreSlim _authLock = new(1, 1);

    public bool IsAuthorized
    {
        get
        {
            try
            {
                SpotifyAuthState? state = ReadAuthState();
                return state != null
                    && !string.IsNullOrWhiteSpace(state.Token.RefreshToken)
                    && string.Equals(state.ClientId, Preferences.spotifyClientID.Trim(), StringComparison.Ordinal);
            }
            catch
            {
                return false;
            }
        }
    }

    public async Task<string> AuthorizeAsync(IProgress<string>? progress = null, CancellationToken cancellationToken = default)
    {
        string clientId = RequireClientId();
        await _authLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            using var listener = new HttpListener();
            listener.Prefixes.Add(RedirectUri.ToString());
            listener.Start();

            var (verifier, challenge) = PKCEUtil.GenerateCodes();
            string state = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            var login = new LoginRequest(RedirectUri, clientId, LoginRequest.ResponseType.Code)
            {
                CodeChallenge = challenge,
                CodeChallengeMethod = "S256",
                State = state,
                Scope = new List<string> { PlaylistReadPrivate, PlaylistReadCollaborative }
            };

            Uri authorizationUri = login.ToUri();
            progress?.Report($"Open this Spotify authorization URL if the browser does not open: {authorizationUri}");
            OpenBrowser(authorizationUri);

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromMinutes(5));
            HttpListenerContext context = await listener.GetContextAsync().WaitAsync(timeout.Token).ConfigureAwait(false);
            string? code = context.Request.QueryString["code"];
            string? returnedState = context.Request.QueryString["state"];
            string? error = context.Request.QueryString["error"];

            bool valid = string.IsNullOrEmpty(error)
                && !string.IsNullOrWhiteSpace(code)
                && CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(state),
                    Encoding.UTF8.GetBytes(returnedState ?? ""));

            await RespondAsync(context.Response, valid
                ? "Spotify authorization complete. You can return to Jammer."
                : "Spotify authorization failed. You can return to Jammer.").ConfigureAwait(false);

            if (!string.IsNullOrEmpty(error)) throw new InvalidOperationException($"Spotify denied authorization: {error}");
            if (!valid) throw new InvalidOperationException("Spotify authorization returned an invalid state or no authorization code.");

            PKCETokenResponse token = await new OAuthClient().RequestToken(
                new PKCETokenRequest(clientId, code!, RedirectUri, verifier), cancellationToken).ConfigureAwait(false);
            SaveToken(clientId, token);

            SpotifyClient client = CreateClient(clientId, token);
            PrivateUser profile = await client.UserProfile.Current(cancellationToken).ConfigureAwait(false);
            return string.IsNullOrWhiteSpace(profile.DisplayName) ? profile.Id : profile.DisplayName;
        }
        finally
        {
            _authLock.Release();
        }
    }

    public void Disconnect()
    {
        if (File.Exists(Utils.SpotifyAuthFilePath)) File.Delete(Utils.SpotifyAuthFilePath);
    }

    public async Task<string> GetAuthorizedUserAsync(CancellationToken cancellationToken = default)
    {
        PrivateUser profile = await (await GetClientAsync()).UserProfile.Current(cancellationToken).ConfigureAwait(false);
        return string.IsNullOrWhiteSpace(profile.DisplayName) ? profile.Id : profile.DisplayName;
    }

    public async Task<IReadOnlyList<SpotifyPlaylistSummary>> GetPlaylistsAsync(CancellationToken cancellationToken = default)
    {
        SpotifyClient client = await GetClientAsync().ConfigureAwait(false);
        PrivateUser me = await client.UserProfile.Current(cancellationToken).ConfigureAwait(false);
        IList<FullPlaylist> playlists = await client.PaginateAll(
            await client.Playlists.CurrentUsers(cancellationToken).ConfigureAwait(false),
            cancellationToken: cancellationToken).ConfigureAwait(false);
        HashSet<string> imported = LoadRegistry().Playlists.Select(x => x.SpotifyPlaylistId).ToHashSet();

        return playlists
            .Where(x => !string.IsNullOrWhiteSpace(x.Id)
                && (string.Equals(x.Owner?.Id, me.Id, StringComparison.Ordinal) || x.Collaborative == true))
            .Select(x => new SpotifyPlaylistSummary(
                x.Id!,
                x.Name ?? "Untitled Spotify playlist",
                x.Owner?.DisplayName ?? x.Owner?.Id ?? "Spotify",
                x.SnapshotId,
                SpotifyUrl(x.ExternalUrls),
                imported.Contains(x.Id!)))
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public async Task<SpotifyPlaylistImport> ImportOrUpdateAsync(
        SpotifyPlaylistSummary playlist,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        SpotifyClient client = await GetClientAsync().ConfigureAwait(false);
        progress?.Report($"Reading {playlist.Name} from Spotify...");
        IList<PlaylistTrack<IPlayableItem>> items = await client.PaginateAll(
            await client.Playlists.GetPlaylistItems(playlist.Id, cancellationToken).ConfigureAwait(false),
            cancellationToken: cancellationToken).ConfigureAwait(false);

        List<SpotifyTrackMetadata> tracks = items
            .Select(x => x.Item ?? x.Track)
            .OfType<FullTrack>()
            .Where(x => !string.IsNullOrWhiteSpace(x.Id) && !string.IsNullOrWhiteSpace(x.Name))
            .Select(x => new SpotifyTrackMetadata(
                x.Id,
                x.Name,
                string.Join(", ", x.Artists?.Select(a => a.Name).Where(n => !string.IsNullOrWhiteSpace(n)) ?? Array.Empty<string>()),
                x.Album?.Name,
                x.Album?.ReleaseDate?.Length >= 4 ? x.Album.ReleaseDate[..4] : null,
                SpotifyUrl(x.ExternalUrls)))
            .ToList();

        SpotifyImportRegistry registry = LoadRegistry();
        SpotifyPlaylistImport? existingImport = registry.Playlists.FirstOrDefault(x => x.SpotifyPlaylistId == playlist.Id);
        string path = existingImport?.PlaylistPath ?? BuildPlaylistPath(playlist.Name, playlist.Id);
        SemaphoreSlim fileLock = PlaylistLocks.GetOrAdd(Path.GetFullPath(path), _ => new SemaphoreSlim(1, 1));
        await fileLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            IReadOnlyList<Song> existingSongs = ReadSongs(path);
            IReadOnlyList<Song> merged = MergeTracks(tracks, existingSongs, playlist.Id, Preferences.spotifyResolutionProvider);
            WriteSongs(path, merged);

            SpotifyPlaylistImport import = existingImport ?? new SpotifyPlaylistImport { SpotifyPlaylistId = playlist.Id };
            import.Name = playlist.Name;
            import.Owner = playlist.Owner;
            import.SpotifyUrl = playlist.SpotifyUrl;
            import.PlaylistPath = path;
            import.SnapshotId = playlist.SnapshotId;
            import.UpdatedAt = DateTimeOffset.UtcNow;
            if (existingImport == null) registry.Playlists.Add(import);
            SaveRegistry(registry);
            progress?.Report($"Imported {tracks.Count} tracks from {playlist.Name}.");
            QueueResolution(path, null);
            return import;
        }
        finally
        {
            fileLock.Release();
        }
    }

    public async Task<int> UpdateAllAsync(IProgress<string>? progress = null, CancellationToken cancellationToken = default)
    {
        SpotifyImportRegistry registry = LoadRegistry();
        if (registry.Playlists.Count == 0) return 0;
        IReadOnlyDictionary<string, SpotifyPlaylistSummary> available = (await GetPlaylistsAsync(cancellationToken))
            .ToDictionary(x => x.Id);
        int updated = 0;
        foreach (SpotifyPlaylistImport import in registry.Playlists.ToArray())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!available.TryGetValue(import.SpotifyPlaylistId, out SpotifyPlaylistSummary? playlist)) continue;
            await ImportOrUpdateAsync(playlist, progress, cancellationToken).ConfigureAwait(false);
            updated++;
        }
        return updated;
    }

    public static IReadOnlyList<Song> MergeTracks(
        IEnumerable<SpotifyTrackMetadata> tracks,
        IEnumerable<Song> existingSongs,
        string spotifyPlaylistId,
        SpotifyResolutionProvider provider)
    {
        Dictionary<string, Song> existing = existingSongs
            .Where(x => !string.IsNullOrWhiteSpace(x.SpotifyTrackId))
            .GroupBy(x => x.SpotifyTrackId!)
            .ToDictionary(x => x.Key, x => x.First());

        return tracks.Select(track =>
        {
            existing.TryGetValue(track.Id, out Song? previous);
            bool previouslyResolved = previous != null
                && !string.IsNullOrWhiteSpace(previous.URI)
                && !previous.URI.StartsWith("spotify-import://", StringComparison.OrdinalIgnoreCase);
            return new Song
            {
                URI = previouslyResolved ? previous!.URI : $"spotify-import://track/{track.Id}",
                Title = track.Title,
                Author = track.Artist,
                Album = track.Album,
                Year = track.ReleaseYear,
                ImportSource = "Spotify",
                SpotifyTrackId = track.Id,
                SpotifyPlaylistId = spotifyPlaylistId,
                SpotifyUrl = track.SpotifyUrl,
                IsFavorite = previous?.IsFavorite,
                Resolver = previouslyResolved && !string.IsNullOrWhiteSpace(previous!.Resolver)
                    ? previous.Resolver
                    : provider.ToString(),
                ResolutionStatus = previouslyResolved ? "Resolved" : "Pending"
            };
        }).ToArray();
    }

    public static SpotifyImportRegistry LoadRegistry()
    {
        string path = Utils.GetSpotifyImportsFilePath(Preferences.GetPlaylistsPath());
        if (!File.Exists(path)) return new SpotifyImportRegistry();
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<SpotifyImportRegistry>(File.ReadAllText(path), JsonOptions)
                ?? new SpotifyImportRegistry();
        }
        catch (System.Text.Json.JsonException ex)
        {
            Log.Error($"Could not read Spotify import registry: {ex}");
            return new SpotifyImportRegistry();
        }
    }

    public static void ResumePendingResolutions()
    {
        foreach (SpotifyPlaylistImport import in LoadRegistry().Playlists)
        {
            if (File.Exists(import.PlaylistPath)) QueueResolution(import.PlaylistPath, null);
        }
    }

    private Task<SpotifyClient> GetClientAsync()
    {
        string clientId = RequireClientId();
        if (!File.Exists(Utils.SpotifyAuthFilePath)) throw new InvalidOperationException("Spotify is not authorized.");
        SpotifyAuthState? state = ReadAuthState();
        if (state == null || !string.Equals(state.ClientId, clientId, StringComparison.Ordinal)
            || string.IsNullOrWhiteSpace(state.Token.RefreshToken))
            throw new InvalidDataException("The saved Spotify authorization is invalid. Authorize Spotify again.");
        return Task.FromResult(CreateClient(clientId, state.Token));
    }

    private SpotifyClient CreateClient(string clientId, PKCETokenResponse token)
    {
        string refreshToken = token.RefreshToken;
        var authenticator = new PKCEAuthenticator(clientId, token);
        authenticator.TokenRefreshed += (_, refreshed) =>
        {
            if (string.IsNullOrWhiteSpace(refreshed.RefreshToken)) refreshed.RefreshToken = refreshToken;
            else refreshToken = refreshed.RefreshToken;
            SaveToken(clientId, refreshed);
        };
        return new SpotifyClient(SpotifyClientConfig.CreateDefault().WithAuthenticator(authenticator));
    }

    private static string RequireClientId()
    {
        string clientId = Preferences.spotifyClientID.Trim();
        if (string.IsNullOrWhiteSpace(clientId))
            throw new InvalidOperationException("Set a Spotify application client ID before authorizing.");
        return clientId;
    }

    private static SpotifyAuthState? ReadAuthState()
    {
        if (!File.Exists(Utils.SpotifyAuthFilePath)) return null;
        return JsonConvert.DeserializeObject<SpotifyAuthState>(File.ReadAllText(Utils.SpotifyAuthFilePath));
    }

    private static void SaveToken(string clientId, PKCETokenResponse token)
    {
        Directory.CreateDirectory(Utils.JammerPath);
        var state = new SpotifyAuthState { ClientId = clientId, Token = token };
        WriteAtomic(Utils.SpotifyAuthFilePath, JsonConvert.SerializeObject(state, Formatting.Indented), privateFile: true);
    }

    private static void SaveRegistry(SpotifyImportRegistry registry)
    {
        string path = Utils.GetSpotifyImportsFilePath(Preferences.GetPlaylistsPath());
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        WriteAtomic(path, System.Text.Json.JsonSerializer.Serialize(registry, JsonOptions));
    }

    private static IReadOnlyList<Song> ReadSongs(string path)
    {
        if (!File.Exists(path)) return Array.Empty<Song>();
        return File.ReadAllLines(path)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => line.ToSong())
            .ToArray();
    }

    private static void WriteSongs(string path, IEnumerable<Song> songs)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        string content = string.Join(Environment.NewLine, songs.Select(x => x.ToSongString()));
        if (content.Length > 0) content += Environment.NewLine;
        WriteAtomic(path, content);
    }

    private static void WriteAtomic(string path, string content, bool privateFile = false)
    {
        string temp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        File.WriteAllText(temp, content, Encoding.UTF8);
        if (privateFile && !OperatingSystem.IsWindows())
            File.SetUnixFileMode(temp, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        File.Move(temp, path, true);
    }

    private static string BuildPlaylistPath(string name, string id)
    {
        string safeName = string.Concat(name.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c)).Trim();
        if (string.IsNullOrWhiteSpace(safeName)) safeName = "Spotify playlist";
        string directory = Preferences.GetPlaylistsPath();
        string path = Path.Combine(directory, safeName + ".jammer");
        if (!File.Exists(path)) return path;
        return Path.Combine(directory, $"{safeName} (Spotify {id[..Math.Min(8, id.Length)]}).jammer");
    }

    private static string? SpotifyUrl(Dictionary<string, string>? urls) =>
        urls != null && urls.TryGetValue("spotify", out string? value) ? value : null;

    private static void QueueResolution(string path, IProgress<string>? progress)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await ResolvePendingAsync(path, progress).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error($"Spotify background resolution failed for {path}: {ex}");
            }
        });
    }

    private static async Task ResolvePendingAsync(string path, IProgress<string>? progress)
    {
        SemaphoreSlim fileLock = PlaylistLocks.GetOrAdd(Path.GetFullPath(path), _ => new SemaphoreSlim(1, 1));
        await fileLock.WaitAsync().ConfigureAwait(false);
        try
        {
            List<Song> songs = ReadSongs(path).ToList();
            for (int i = 0; i < songs.Count; i++)
            {
                Song song = songs[i];
                if (!string.Equals(song.ImportSource, "Spotify", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(song.ResolutionStatus, "Resolved", StringComparison.OrdinalIgnoreCase)) continue;

                SpotifyResolutionProvider provider = Enum.TryParse(song.Resolver, true, out SpotifyResolutionProvider parsed)
                    ? parsed
                    : Preferences.spotifyResolutionProvider;
                progress?.Report($"Resolving {song.Title} — {song.Author} on {provider}...");
                try
                {
                    string? url = await ResolveFirstAsync(song, provider).ConfigureAwait(false);
                    song.URI = url ?? song.URI;
                    song.ResolutionStatus = url == null ? "NotFound" : "Resolved";
                }
                catch (Exception ex)
                {
                    song.ResolutionStatus = "Failed";
                    Log.Error($"Could not resolve Spotify track {song.SpotifyTrackId}: {ex.Message}");
                }
                WriteSongs(path, songs);
            }
        }
        finally
        {
            fileLock.Release();
        }
    }

    public static string? ResolveSpotifyImportSynchronously(Song song)
    {
        SpotifyResolutionProvider provider = Enum.TryParse(song.Resolver, true, out SpotifyResolutionProvider parsed)
            ? parsed
            : Preferences.spotifyResolutionProvider;
        try
        {
            return ResolveFirstAsync(song, provider).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Log.Error($"Could not resolve Spotify track {song.SpotifyTrackId} synchronously: {ex.Message}");
            return null;
        }
    }

    private static async Task<string?> ResolveFirstAsync(Song song, SpotifyResolutionProvider provider)
    {
        string query = $"{song.Title} {song.Author}".Trim();
        if (string.IsNullOrWhiteSpace(query)) return null;

        if (provider == SpotifyResolutionProvider.SoundCloud)
        {
            await foreach (var result in Download.ReturnSoundCloudClient().Search.GetTracksAsync(query))
            {
                if (!string.IsNullOrWhiteSpace(result.Url)) return result.Url;
            }
            return null;
        }

        await foreach (VideoSearchResult result in Download.youtube.Search.GetVideosAsync(query))
        {
            return $"https://www.youtube.com/watch?v={result.Id}";
        }
        return null;
    }

    private static void OpenBrowser(Uri uri)
    {
        try
        {
            Process.Start(new ProcessStartInfo(uri.ToString()) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Log.Error($"Could not open a browser for Spotify authorization: {ex}");
        }
    }

    private static async Task RespondAsync(HttpListenerResponse response, string message)
    {
        byte[] body = Encoding.UTF8.GetBytes($"<!doctype html><meta charset=\"utf-8\"><title>Jammer Spotify</title><p>{WebUtility.HtmlEncode(message)}</p>");
        response.StatusCode = 200;
        response.ContentType = "text/html; charset=utf-8";
        response.ContentLength64 = body.Length;
        await response.OutputStream.WriteAsync(body).ConfigureAwait(false);
        response.Close();
    }
}
