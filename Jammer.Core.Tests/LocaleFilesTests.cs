namespace Jammer.Core.Tests;

public sealed class LocaleFilesTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "jammer-locales-tests-" + Guid.NewGuid().ToString("N"));

    [Fact]
    public void CopiesBundledLocalesWithoutOverwritingExistingTranslations()
    {
        string source = Path.Combine(_root, "bundled");
        string destination = Path.Combine(_root, "user");
        Directory.CreateDirectory(source);
        Directory.CreateDirectory(destination);
        File.WriteAllText(Path.Combine(source, "en.ini"), "bundled English");
        File.WriteAllText(Path.Combine(source, "fi.ini"), "bundled Finnish");
        File.WriteAllText(Path.Combine(destination, "en.ini"), "custom English");

        int copied = IniFileHandling.CopyMissingLocaleFiles(source, destination);

        Assert.Equal(1, copied);
        Assert.Equal("custom English", File.ReadAllText(Path.Combine(destination, "en.ini")));
        Assert.Equal("bundled Finnish", File.ReadAllText(Path.Combine(destination, "fi.ini")));
    }

    [Fact]
    public void MissingBundledLocaleDirectoryIsSafe()
    {
        int copied = IniFileHandling.CopyMissingLocaleFiles(
            Path.Combine(_root, "missing"),
            Path.Combine(_root, "user"));

        Assert.Equal(0, copied);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root)) Directory.Delete(_root, true);
    }
}
