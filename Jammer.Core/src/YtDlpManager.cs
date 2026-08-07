using System.Diagnostics;
using System.IO.Compression;
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
        Task<string?> GetVersionAsync(
            string executable,
            CancellationToken cancellationToken,
            TimeSpan? validationTimeout = null);
    }

    public sealed class YtDlpProcessRunner : IYtDlpProcessRunner
    {
        public async Task<string?> GetVersionAsync(
            string executable,
            CancellationToken cancellationToken,
            TimeSpan? validationTimeout = null)
        {
            TimeSpan effectiveTimeout = validationTimeout ?? TimeSpan.FromSeconds(10);
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(effectiveTimeout);
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
            bool started = false;
            try
            {
                if (!process.Start())
                {
                    throw new InvalidDataException($"Could not start '{executable}' for its --version check.");
                }
                started = true;

                Task<string> outputTask = process.StandardOutput.ReadToEndAsync(timeout.Token);
                Task<string> errorTask = process.StandardError.ReadToEndAsync(timeout.Token);
                await process.WaitForExitAsync(timeout.Token);
                string output = (await outputTask).Trim();
                string error = (await errorTask).Trim();
                if (process.ExitCode != 0)
                {
                    string detail = string.IsNullOrWhiteSpace(error) ? output : error;
                    throw new InvalidDataException(
                        $"'{executable} --version' exited with code {process.ExitCode}" +
                        (string.IsNullOrWhiteSpace(detail) ? "." : $": {detail}"));
                }

                return string.IsNullOrWhiteSpace(output) ? null : output;
            }
            catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException(
                    $"'{executable} --version' did not finish within {effectiveTimeout.TotalSeconds:0} seconds.",
                    ex);
            }
            catch (Exception ex) when (ex is IOException or InvalidOperationException or System.ComponentModel.Win32Exception)
            {
                throw new InvalidDataException($"Could not run '{executable} --version': {ex.Message}", ex);
            }
            finally
            {
                try
                {
                    if (started && !process.HasExited) process.Kill(true);
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
            _jammerPath = Path.GetFullPath(jammerPath ?? Utils.JammerPath);
            _platform = platform ?? DetectPlatform();
            _httpClient = httpClient ?? CreateHttpClient();
            _runner = runner ?? new YtDlpProcessRunner();
            _overridePath = overridePath ?? (() => Environment.GetEnvironmentVariable("JAMMER_YTDLP_BIN"));
        }

        public string ManagedBinaryPath => _platform switch
        {
            YtDlpPlatform.WindowsX64 => Path.Combine(Utils.GetToolsPath(_jammerPath), "yt-dlp.exe"),
            YtDlpPlatform.LinuxX64 => Path.Combine(Utils.GetToolsPath(_jammerPath), "yt-dlp"),
            YtDlpPlatform.MacOSUniversal => Path.Combine(Utils.GetToolsPath(_jammerPath), "yt-dlp-macos", "yt-dlp_macos"),
            _ => throw new PlatformNotSupportedException()
        };

        public string ReleaseAssetName => _platform switch
        {
            YtDlpPlatform.WindowsX64 => "yt-dlp.exe",
            YtDlpPlatform.LinuxX64 => "yt-dlp_linux",
            YtDlpPlatform.MacOSUniversal => "yt-dlp_macos.zip",
            _ => throw new PlatformNotSupportedException()
        };

        public async Task<YtDlpResolution> ResolveAsync(bool installIfMissing, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            string? configured = _overridePath();
            if (!string.IsNullOrWhiteSpace(configured))
            {
                string? version = File.Exists(configured)
                    ? await TryGetVersionAsync(configured, cancellationToken)
                    : null;
                if (version != null)
                {
                    return new YtDlpResolution(configured, "JAMMER_YTDLP_BIN", version);
                }
            }

            if (File.Exists(ManagedBinaryPath))
            {
                string? version = await TryGetVersionAsync(ManagedBinaryPath, cancellationToken);
                if (version != null)
                {
                    return new YtDlpResolution(ManagedBinaryPath, "managed", version);
                }
            }

            string? pathVersion = await TryGetVersionAsync("yt-dlp", cancellationToken);
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

            string toolsDirectory = Utils.GetToolsPath(_jammerPath);
            Directory.CreateDirectory(toolsDirectory);
            string temporaryPath = Path.Combine(toolsDirectory, $".{ReleaseAssetName}.{Guid.NewGuid():N}.download");
            string? stagingDirectory = null;

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

                string executableToValidate = temporaryPath;
                if (_platform == YtDlpPlatform.MacOSUniversal)
                {
                    stagingDirectory = Path.Combine(toolsDirectory, $".yt-dlp-macos.{Guid.NewGuid():N}.staging");
                    await ExtractArchiveAsync(temporaryPath, stagingDirectory, cancellationToken);
                    executableToValidate = Path.Combine(stagingDirectory, "yt-dlp_macos");
                    if (!File.Exists(executableToValidate))
                    {
                        throw new InvalidDataException("The yt-dlp macOS archive does not contain yt-dlp_macos.");
                    }
                }

                if (!OperatingSystem.IsWindows())
                {
                    File.SetUnixFileMode(executableToValidate,
                        UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                        UnixFileMode.GroupRead | UnixFileMode.GroupExecute |
                        UnixFileMode.OtherRead | UnixFileMode.OtherExecute);
                }

                TimeSpan? validationTimeout = _platform == YtDlpPlatform.MacOSUniversal
                    ? TimeSpan.FromMinutes(1)
                    : null;
                string? version = await _runner.GetVersionAsync(
                    executableToValidate,
                    cancellationToken,
                    validationTimeout);
                if (version == null)
                {
                    throw new InvalidDataException("The downloaded yt-dlp executable failed its --version check.");
                }

                if (_platform == YtDlpPlatform.MacOSUniversal)
                {
                    ReplaceManagedMacBundle(stagingDirectory!);
                    stagingDirectory = null;
                }
                else
                {
                    File.Move(temporaryPath, ManagedBinaryPath, true);
                }
                progress?.Report(1);
                return new YtDlpResolution(ManagedBinaryPath, "managed", version);
            }
            finally
            {
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }
                if (stagingDirectory != null && Directory.Exists(stagingDirectory))
                {
                    Directory.Delete(stagingDirectory, true);
                }
            }
        }

        private async Task<string?> TryGetVersionAsync(string executable, CancellationToken cancellationToken)
        {
            try
            {
                return await _runner.GetVersionAsync(executable, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex) when (ex is InvalidDataException or TimeoutException)
            {
                return null;
            }
        }

        private static async Task ExtractArchiveAsync(string archivePath, string destinationDirectory, CancellationToken cancellationToken)
        {
            Directory.CreateDirectory(destinationDirectory);
            string destinationRoot = Path.GetFullPath(destinationDirectory) + Path.DirectorySeparatorChar;
            await using FileStream archiveStream = File.OpenRead(archivePath);
            using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Read);

            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string destinationPath = Path.GetFullPath(Path.Combine(destinationDirectory, entry.FullName));
                if (!destinationPath.StartsWith(destinationRoot, StringComparison.Ordinal))
                {
                    throw new InvalidDataException($"Unsafe path in yt-dlp archive: {entry.FullName}");
                }

                int unixFileType = (entry.ExternalAttributes >> 16) & 0xF000;
                if (unixFileType == 0xA000)
                {
                    throw new InvalidDataException($"Symbolic links are not allowed in the yt-dlp archive: {entry.FullName}");
                }

                if (string.IsNullOrEmpty(entry.Name))
                {
                    Directory.CreateDirectory(destinationPath);
                    continue;
                }

                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
                await using Stream source = entry.Open();
                await using var target = new FileStream(destinationPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true);
                await source.CopyToAsync(target, cancellationToken);
            }
        }

        private void ReplaceManagedMacBundle(string stagingDirectory)
        {
            string targetDirectory = Path.GetDirectoryName(ManagedBinaryPath)!;
            string backupDirectory = Path.Combine(
                Path.GetDirectoryName(targetDirectory)!,
                $".{Path.GetFileName(targetDirectory)}.{Guid.NewGuid():N}.backup");
            bool backedUp = false;

            try
            {
                if (Directory.Exists(targetDirectory))
                {
                    Directory.Move(targetDirectory, backupDirectory);
                    backedUp = true;
                }

                Directory.Move(stagingDirectory, targetDirectory);
                if (backedUp)
                {
                    Directory.Delete(backupDirectory, true);
                }
            }
            catch
            {
                if (!Directory.Exists(targetDirectory) && backedUp && Directory.Exists(backupDirectory))
                {
                    Directory.Move(backupDirectory, targetDirectory);
                }
                throw;
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
