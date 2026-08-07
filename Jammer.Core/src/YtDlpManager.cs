using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;

namespace Jammer
{
    public enum YtDlpPlatform
    {
        WindowsX64,
        LinuxX64,
        MacOSUniversal
    }

    public sealed record YtDlpResolution(string? Path, string Source, string? Version)
    {
        public bool IsAvailable => Path != null;
    }

    public interface IYtDlpProcessRunner
    {
        Task<string?> GetVersionAsync(string executable, CancellationToken cancellationToken);
    }

    public sealed class YtDlpProcessRunner : IYtDlpProcessRunner
    {
        public async Task<string?> GetVersionAsync(string executable, CancellationToken cancellationToken)
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(10));
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = executable,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.StartInfo.ArgumentList.Add("--version");
            try
            {
                if (!process.Start())
                {
                    return null;
                }

                string output = await process.StandardOutput.ReadToEndAsync(timeout.Token);
                await process.WaitForExitAsync(timeout.Token);
                return process.ExitCode == 0 ? output.Trim() : null;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return null;
            }
            catch (Exception ex) when (ex is IOException or InvalidOperationException or System.ComponentModel.Win32Exception)
            {
                return null;
            }
            finally
            {
                try
                {
                    if (process.StartTime != default && !process.HasExited) process.Kill(true);
                }
                catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception)
                {
                    // Process never started or exited between checks.
                }
            }
        }
    }

    /// <summary>Resolves and atomically installs official yt-dlp release binaries.</summary>
    public sealed class YtDlpManager
    {
        private static readonly Uri ReleaseBaseUri = new("https://github.com/yt-dlp/yt-dlp/releases/latest/download/");
        private readonly string _jammerPath;
        private readonly YtDlpPlatform _platform;
        private readonly HttpClient _httpClient;
        private readonly IYtDlpProcessRunner _runner;
        private readonly Func<string?> _overridePath;

        public YtDlpManager(
            string? jammerPath = null,
            YtDlpPlatform? platform = null,
            HttpClient? httpClient = null,
            IYtDlpProcessRunner? runner = null,
            Func<string?>? overridePath = null)
        {
            _jammerPath = jammerPath ?? Utils.JammerPath;
            _platform = platform ?? DetectPlatform();
            _httpClient = httpClient ?? CreateHttpClient();
            _runner = runner ?? new YtDlpProcessRunner();
            _overridePath = overridePath ?? (() => Environment.GetEnvironmentVariable("JAMMER_YTDLP_BIN"));
        }

        public string ManagedBinaryPath => Path.Combine(
            _jammerPath,
            "tools",
            _platform == YtDlpPlatform.WindowsX64 ? "yt-dlp.exe" : "yt-dlp");

        public string ReleaseAssetName => _platform switch
        {
            YtDlpPlatform.WindowsX64 => "yt-dlp.exe",
            YtDlpPlatform.LinuxX64 => "yt-dlp_linux",
            YtDlpPlatform.MacOSUniversal => "yt-dlp_macos",
            _ => throw new PlatformNotSupportedException()
        };

        public async Task<YtDlpResolution> ResolveAsync(bool installIfMissing, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            string? configured = _overridePath();
            if (!string.IsNullOrWhiteSpace(configured))
            {
                string? version = File.Exists(configured)
                    ? await _runner.GetVersionAsync(configured, cancellationToken)
                    : null;
                if (version != null)
                {
                    return new YtDlpResolution(configured, "JAMMER_YTDLP_BIN", version);
                }
            }

            if (File.Exists(ManagedBinaryPath))
            {
                string? version = await _runner.GetVersionAsync(ManagedBinaryPath, cancellationToken);
                if (version != null)
                {
                    return new YtDlpResolution(ManagedBinaryPath, "managed", version);
                }
            }

            string? pathVersion = await _runner.GetVersionAsync("yt-dlp", cancellationToken);
            if (pathVersion != null)
            {
                return new YtDlpResolution("yt-dlp", "PATH", pathVersion);
            }

            if (!installIfMissing)
            {
                return new YtDlpResolution(null, "missing", null);
            }

            return await InstallAsync(true, progress, cancellationToken);
        }

        public async Task<YtDlpResolution> InstallAsync(bool force, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            if (!force)
            {
                YtDlpResolution current = await ResolveAsync(false, progress, cancellationToken);
                if (current.IsAvailable)
                {
                    return current;
                }
            }

            string toolsDirectory = Path.GetDirectoryName(ManagedBinaryPath)!;
            Directory.CreateDirectory(toolsDirectory);
            string temporaryPath = Path.Combine(toolsDirectory, $".{Path.GetFileName(ManagedBinaryPath)}.{Guid.NewGuid():N}.download");

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(ReleaseBaseUri, ReleaseAssetName));
                using HttpResponseMessage response = await _httpClient.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);
                response.EnsureSuccessStatusCode();

                long? length = response.Content.Headers.ContentLength;
                await using Stream source = await response.Content.ReadAsStreamAsync(cancellationToken);
                await using (var target = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true))
                {
                    var buffer = new byte[81920];
                    long received = 0;
                    int count;
                    while ((count = await source.ReadAsync(buffer, cancellationToken)) != 0)
                    {
                        await target.WriteAsync(buffer.AsMemory(0, count), cancellationToken);
                        received += count;
                        if (length > 0)
                        {
                            progress?.Report((double)received / length.Value);
                        }
                    }
                    await target.FlushAsync(cancellationToken);
                }

                if (!OperatingSystem.IsWindows())
                {
                    File.SetUnixFileMode(temporaryPath,
                        UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                        UnixFileMode.GroupRead | UnixFileMode.GroupExecute |
                        UnixFileMode.OtherRead | UnixFileMode.OtherExecute);
                }

                string? version = await _runner.GetVersionAsync(temporaryPath, cancellationToken);
                if (version == null)
                {
                    throw new InvalidDataException("The downloaded yt-dlp executable failed its --version check.");
                }

                File.Move(temporaryPath, ManagedBinaryPath, true);
                progress?.Report(1);
                return new YtDlpResolution(ManagedBinaryPath, "managed", version);
            }
            finally
            {
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }
            }
        }

        public static YtDlpPlatform DetectPlatform()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && RuntimeInformation.ProcessArchitecture == Architecture.X64)
                return YtDlpPlatform.WindowsX64;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && RuntimeInformation.ProcessArchitecture == Architecture.X64)
                return YtDlpPlatform.LinuxX64;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) &&
                RuntimeInformation.ProcessArchitecture is Architecture.X64 or Architecture.Arm64)
                return YtDlpPlatform.MacOSUniversal;
            throw new PlatformNotSupportedException("Managed yt-dlp installation supports win-x64, linux-x64, osx-x64, and osx-arm64.");
        }

        private static HttpClient CreateHttpClient()
        {
            var handler = new SocketsHttpHandler
            {
                AllowAutoRedirect = true,
                AutomaticDecompression = DecompressionMethods.All,
                ConnectTimeout = TimeSpan.FromSeconds(15)
            };
            return new HttpClient(handler) { Timeout = TimeSpan.FromMinutes(5) };
        }
    }
}
