using System.Net;
using System.IO.Compression;

namespace Jammer.Core.Tests;

public sealed class YtDlpManagerTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "jammer-tests-" + Guid.NewGuid().ToString("N"));

    [Theory]
    [InlineData(YtDlpPlatform.WindowsX64, "yt-dlp.exe")]
    [InlineData(YtDlpPlatform.LinuxX64, "yt-dlp_linux")]
    [InlineData(YtDlpPlatform.MacOSUniversal, "yt-dlp_macos.zip")]
    public void UsesOfficialReleaseAssetName(YtDlpPlatform platform, string expected)
    {
        Assert.Equal(expected, Create(platform).ReleaseAssetName);
    }

    [Fact]
    public async Task OverridePrecedesManagedAndPath()
    {
        Directory.CreateDirectory(_root);
        string configured = Path.Combine(_root, "configured");
        string managed = Path.Combine(_root, "tools", "yt-dlp");
        File.WriteAllText(configured, "configured");
        Directory.CreateDirectory(Path.GetDirectoryName(managed)!);
        File.WriteAllText(managed, "managed");
        var runner = new FakeRunner(new Dictionary<string, string?>
        {
            [configured] = "1",
            [managed] = "2",
            ["yt-dlp"] = "3"
        });
        var manager = Create(YtDlpPlatform.LinuxX64, runner, () => configured);

        YtDlpResolution result = await manager.ResolveAsync(false);

        Assert.Equal(configured, result.Path);
        Assert.Equal("JAMMER_YTDLP_BIN", result.Source);
    }

    [Fact]
    public async Task FallsBackFromInvalidOverrideToManagedThenPath()
    {
        string managed = Path.Combine(_root, "tools", "yt-dlp");
        Directory.CreateDirectory(Path.GetDirectoryName(managed)!);
        File.WriteAllText(managed, "managed");
        var runner = new FakeRunner(new Dictionary<string, string?> { [managed] = "2", ["yt-dlp"] = "3" });
        var manager = Create(YtDlpPlatform.LinuxX64, runner, () => Path.Combine(_root, "missing"));

        YtDlpResolution result = await manager.ResolveAsync(false);

        Assert.Equal(managed, result.Path);
        Assert.Equal("managed", result.Source);
    }

    [Fact]
    public async Task InstallsToToolsOnlyAfterValidation()
    {
        byte[] payload = { 1, 2, 3, 4 };
        using var client = new HttpClient(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(payload)
        }));
        var manager = new YtDlpManager(_root, YtDlpPlatform.WindowsX64, client, new AlwaysValidRunner(), () => null);

        YtDlpResolution result = await manager.InstallAsync(true);

        Assert.Equal(Path.Combine(_root, "tools", "yt-dlp.exe"), result.Path);
        Assert.Equal(payload, File.ReadAllBytes(result.Path!));
        Assert.Empty(Directory.GetFiles(Path.Combine(_root, "tools"), "*.download"));
    }

    [Fact]
    public async Task InvalidDownloadNeverReplacesManagedBinary()
    {
        using var client = new HttpClient(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(new byte[] { 1 })
        }));
        var manager = new YtDlpManager(_root, YtDlpPlatform.WindowsX64, client, new FakeRunner(), () => null);

        await Assert.ThrowsAsync<InvalidDataException>(() => manager.InstallAsync(true));

        Assert.False(File.Exists(manager.ManagedBinaryPath));
        Assert.Empty(Directory.GetFiles(Path.Combine(_root, "tools"), "*.download"));
    }

    [Fact]
    public async Task InstallsCompleteMacBundleUnderTools()
    {
        byte[] payload = CreateMacArchive();
        using var client = new HttpClient(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(payload)
        }));
        var runner = new AlwaysValidRunner();
        var manager = new YtDlpManager(_root, YtDlpPlatform.MacOSUniversal, client, runner, () => null);

        YtDlpResolution result = await manager.InstallAsync(true);

        Assert.Equal(Path.Combine(_root, "tools", "yt-dlp-macos", "yt-dlp_macos"), result.Path);
        Assert.Equal("executable", File.ReadAllText(result.Path!));
        Assert.Equal("dependency", File.ReadAllText(Path.Combine(_root, "tools", "yt-dlp-macos", "_internal", "dependency")));
        Assert.Equal(TimeSpan.FromMinutes(1), runner.LastValidationTimeout);
        Assert.DoesNotContain(Directory.EnumerateFileSystemEntries(Path.Combine(_root, "tools")),
            path => Path.GetFileName(path).StartsWith(".", StringComparison.Ordinal));
    }

    private static byte[] CreateMacArchive()
    {
        using var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
        {
            using (var writer = new StreamWriter(archive.CreateEntry("yt-dlp_macos").Open()))
            {
                writer.Write("executable");
            }
            using (var writer = new StreamWriter(archive.CreateEntry("_internal/dependency").Open()))
            {
                writer.Write("dependency");
            }
        }
        return stream.ToArray();
    }

    private YtDlpManager Create(YtDlpPlatform platform, IYtDlpProcessRunner? runner = null, Func<string?>? configured = null) =>
        new(_root, platform, new HttpClient(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound))), runner ?? new FakeRunner(), configured ?? (() => null));

    public void Dispose()
    {
        if (Directory.Exists(_root)) Directory.Delete(_root, true);
    }

    private sealed class FakeRunner : IYtDlpProcessRunner
    {
        private readonly IReadOnlyDictionary<string, string?> _versions;
        public FakeRunner(IReadOnlyDictionary<string, string?>? versions = null) => _versions = versions ?? new Dictionary<string, string?>();
        public Task<string?> GetVersionAsync(
            string executable,
            CancellationToken cancellationToken,
            TimeSpan? validationTimeout = null) =>
            Task.FromResult(_versions.TryGetValue(executable, out string? version) ? version : null);
    }

    private sealed class AlwaysValidRunner : IYtDlpProcessRunner
    {
        public TimeSpan? LastValidationTimeout { get; private set; }

        public Task<string?> GetVersionAsync(
            string executable,
            CancellationToken cancellationToken,
            TimeSpan? validationTimeout = null)
        {
            LastValidationTimeout = validationTimeout;
            return Task.FromResult<string?>("test-version");
        }
    }
}
