namespace Jammer.Core.Tests;

public sealed class TerminalInputTests
{
    [Theory]
    [InlineData("[5~", ConsoleKey.PageUp)]
    [InlineData("[6~", ConsoleKey.PageDown)]
    [InlineData("[A", ConsoleKey.UpArrow)]
    [InlineData("[B", ConsoleKey.DownArrow)]
    [InlineData("OA", ConsoleKey.UpArrow)]
    [InlineData("OB", ConsoleKey.DownArrow)]
    [InlineData("[H", ConsoleKey.Home)]
    [InlineData("[F", ConsoleKey.End)]
    public void DecodesCommonTerminalNavigationSequences(string sequence, ConsoleKey expected)
    {
        bool decoded = TerminalInput.TryDecodeAnsiSequence(sequence, out ConsoleKeyInfo keyInfo);

        Assert.True(decoded);
        Assert.Equal(expected, keyInfo.Key);
    }

    [Theory]
    [InlineData("[5;2~", ConsoleKey.PageUp, ConsoleModifiers.Shift)]
    [InlineData("[6;5~", ConsoleKey.PageDown, ConsoleModifiers.Control)]
    [InlineData("[1;3C", ConsoleKey.RightArrow, ConsoleModifiers.Alt)]
    public void PreservesTerminalNavigationModifiers(string sequence, ConsoleKey expectedKey, ConsoleModifiers expectedModifier)
    {
        bool decoded = TerminalInput.TryDecodeAnsiSequence(sequence, out ConsoleKeyInfo keyInfo);

        Assert.True(decoded);
        Assert.Equal(expectedKey, keyInfo.Key);
        Assert.Equal(expectedModifier, keyInfo.Modifiers);
    }

    [Fact]
    public void PreservesAltLetterSequences()
    {
        bool decoded = TerminalInput.TryDecodeAnsiSequence("s", out ConsoleKeyInfo keyInfo);

        Assert.True(decoded);
        Assert.Equal(ConsoleKey.S, keyInfo.Key);
        Assert.Equal(ConsoleModifiers.Alt, keyInfo.Modifiers);
    }

    [Theory]
    [InlineData("")]
    [InlineData("[99~")]
    [InlineData("not-a-sequence")]
    public void RejectsUnknownTerminalSequences(string sequence)
    {
        Assert.False(TerminalInput.TryDecodeAnsiSequence(sequence, out _));
    }
}
