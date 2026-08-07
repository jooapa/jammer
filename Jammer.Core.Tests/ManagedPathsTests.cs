namespace Jammer.Core.Tests;

public sealed class ManagedPathsTests
{
    [Fact]
    public void DefaultRuntimePathsAreCentralizedUnderJammerRoot()
    {
        string root = Path.GetFullPath(Utils.JammerPath) + Path.DirectorySeparatorChar;
        string[] managedPaths =
        {
            Utils.SongsPath,
            Utils.PlaylistsPath,
            Utils.ToolsPath,
            Utils.DownloadsPath,
            Utils.CachePath,
            Utils.YtDlpCachePath,
            Utils.LocalesPath,
            Utils.SoundFontsPath,
            Utils.ThemesPath,
            Utils.PlaylistBackupsPath
        };

        Assert.All(managedPaths, path =>
            Assert.StartsWith(root, Path.GetFullPath(path) + Path.DirectorySeparatorChar));

        string[] managedFiles =
        {
            Utils.SettingsFilePath,
            Utils.KeyDataFilePath,
            Utils.EffectsFilePath,
            Utils.VisualizerFilePath
        };
        Assert.All(managedFiles, path =>
            Assert.Equal(Path.GetFullPath(Utils.JammerPath), Path.GetDirectoryName(Path.GetFullPath(path))));
    }
}
