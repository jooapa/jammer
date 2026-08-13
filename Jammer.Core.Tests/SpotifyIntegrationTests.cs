namespace Jammer.Core.Tests;

public sealed class SpotifyIntegrationTests
{
    [Fact]
    public void ImportedTrackUsesPlaceholderAndSpotifyAttributionMetadata()
    {
        var track = new SpotifyTrackMetadata(
            "track-1", "A Song", "An Artist", "An Album", "2026", "https://open.spotify.com/track/track-1");

        Song song = Assert.Single(SpotifyIntegrationService.MergeTracks(
            new[] { track }, Array.Empty<Song>(), "playlist-1", SpotifyResolutionProvider.YouTube));
        Song roundTrip = song.ToSongString().ToSong();

        Assert.Equal("spotify-import://track/track-1", roundTrip.URI);
        Assert.Equal("A Song", roundTrip.Title);
        Assert.Equal("An Artist", roundTrip.Author);
        Assert.Equal("Spotify", roundTrip.ImportSource);
        Assert.Equal("track-1", roundTrip.SpotifyTrackId);
        Assert.Equal("playlist-1", roundTrip.SpotifyPlaylistId);
        Assert.Equal("https://open.spotify.com/track/track-1", roundTrip.SpotifyUrl);
        Assert.Equal("YouTube", roundTrip.Resolver);
        Assert.Equal("Pending", roundTrip.ResolutionStatus);
    }

    [Fact]
    public void UpdatePreservesResolvedUrlButRefreshesSpotifyMetadata()
    {
        var existing = new Song
        {
            URI = "https://www.youtube.com/watch?v=resolved",
            Title = "Old title",
            ImportSource = "Spotify",
            SpotifyTrackId = "track-1",
            Resolver = "YouTube",
            IsFavorite = "true",
            ResolutionStatus = "Resolved"
        };
        var updatedTrack = new SpotifyTrackMetadata(
            "track-1", "New title", "New artist", null, null, "https://open.spotify.com/track/track-1");

        Song result = Assert.Single(SpotifyIntegrationService.MergeTracks(
            new[] { updatedTrack }, new[] { existing }, "playlist-1", SpotifyResolutionProvider.SoundCloud));

        Assert.Equal(existing.URI, result.URI);
        Assert.Equal("New title", result.Title);
        Assert.Equal("New artist", result.Author);
        Assert.Equal("YouTube", result.Resolver);
        Assert.Equal("true", result.IsFavorite);
        Assert.Equal("Resolved", result.ResolutionStatus);
    }

    [Fact]
    public void UpdateDropsTracksRemovedFromSpotifyPlaylist()
    {
        var existing = new[]
        {
            new Song { URI = "https://example.test/one", SpotifyTrackId = "one" },
            new Song { URI = "https://example.test/two", SpotifyTrackId = "two" }
        };
        var current = new[]
        {
            new SpotifyTrackMetadata("two", "Two", "Artist", null, null, null)
        };

        IReadOnlyList<Song> result = SpotifyIntegrationService.MergeTracks(
            current, existing, "playlist-1", SpotifyResolutionProvider.YouTube);

        Assert.Collection(result, song => Assert.Equal("two", song.SpotifyTrackId));
    }

    [Fact]
    public void ResolveSpotifyImportSynchronouslyReturnsNullWhenMetadataEmpty()
    {
        var song = new Song
        {
            URI = "spotify-import://track/empty",
            Title = "",
            Author = ""
        };

        string? resolved = SpotifyIntegrationService.ResolveSpotifyImportSynchronously(song);
        Assert.Null(resolved);
    }
}
