namespace Jammer.Core.Tests;

public sealed class KeybindingTests : IDisposable
{
    private readonly string _originalJammerPath = Utils.JammerPath;
    private readonly string _tempRoot;

    public KeybindingTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "jammer-keybind-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
        Utils.JammerPath = _tempRoot;
    }

    [Fact]
    public void ShiftM_Matches_EditSongMetadata()
    {
        File.WriteAllText(Path.Combine(_tempRoot, "KeyData.ini"), @"
[Keybinds]
EditSongMetadata = Shift + M
NextSong = N
");

        IniFileHandling.ReadNewKeybinds();

        string result = IniFileHandling.FindMatch_KeyData(
            ConsoleKey.M,
            isAlt: false,
            isCtrl: false,
            isShift: true,
            isShiftAlt: false,
            isShiftCtrl: false,
            isCtrlAlt: false,
            isShiftCtrlAlt: false);

        Assert.Equal("EditSongMetadata", result);
    }

    [Fact]
    public void LowercaseShiftM_Matches_EditSongMetadata()
    {
        File.WriteAllText(Path.Combine(_tempRoot, "KeyData.ini"), @"
[Keybinds]
EditSongMetadata = shift + m
NextSong = N
");

        IniFileHandling.ReadNewKeybinds();

        string result = IniFileHandling.FindMatch_KeyData(
            ConsoleKey.M,
            isAlt: false,
            isCtrl: false,
            isShift: true,
            isShiftAlt: false,
            isShiftCtrl: false,
            isCtrlAlt: false,
            isShiftCtrlAlt: false);

        Assert.Equal("EditSongMetadata", result);
    }

    public void Dispose()
    {
        Utils.JammerPath = _originalJammerPath;
        if (Directory.Exists(_tempRoot)) Directory.Delete(_tempRoot, true);
    }
}
