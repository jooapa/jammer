namespace Jammer.Core.Tests;

public sealed class PlaylistsTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "jammer-playlists-tests-" + Guid.NewGuid().ToString("N"));

    [Fact]
    public void AvailablePlaylistsAreFilteredAndSorted()
    {
        Directory.CreateDirectory(_root);
        File.WriteAllText(Path.Combine(_root, "Zulu.m3u"), string.Empty);
        File.WriteAllText(Path.Combine(_root, "alpha.jammer"), string.Empty);
        File.WriteAllText(Path.Combine(_root, "Beta.m3u8"), string.Empty);
        File.WriteAllText(Path.Combine(_root, "notes.txt"), string.Empty);

        string[] result = Playlists.GetAvailablePlaylistPaths(_root);

        Assert.Equal(new[] { "alpha.jammer", "Beta.m3u8", "Zulu.m3u" }, result.Select(Path.GetFileName));
    }

    [Fact]
    public void MissingPlaylistDirectoryReturnsAnEmptyList()
    {
        Assert.Empty(Playlists.GetAvailablePlaylistPaths(_root));
    }

    public void Dispose()
    {
        if (Directory.Exists(_root)) Directory.Delete(_root, true);
    }
}
