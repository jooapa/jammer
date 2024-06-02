using ManagedBass;
using System.Text;
using System;
using System.IO;
using System.Text.Json;
using Spectre.Console;
using System.Globalization;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Jammer {
    public static class Themes {

        public static string fileContent = @"// Colors https://spectreconsole.net/appendix/colors and https://spectreconsole.net/appendix/styles
// write the color name in lowercase ' ' and the styles ' '
// Example: red bold italic
// Border Styles https://spectreconsole.net/appendix/borders
// write the border name with CamelCase
// Example: Rounded
// Only the BorderColor is in RGB format ie. [0-255,0-255,0-255]

{
    ""Playlist"": {
        ""PathColor"": ""white"",
        ""ErrorColor"": ""red"",
        ""SuccessColor"": ""green"",
        ""InfoColor"": ""green"",
        ""PlaylistNameColor"": ""lightblue"",
        ""MiniHelpBorderStyle"": ""Rounded"",
        ""MiniHelpBorderColor"": [255,255,255],
        ""HelpLetterColor"": ""red"",
        ""ForHelpTextColor"": ""white"",
        ""SettingsLetterColor"": ""yellow"",
        ""ForSettingsTextColor"": ""white"",
        ""PlaylistLetterColor"": ""green"",
        ""ForPlaylistTextColor"": ""white"",
        ""VisualizerColor"": ""white""
    },
    ""GeneralPlaylist"": {
        ""BorderColor"": [255,255,255],
        ""BorderStyle"": ""Rounded"",
        ""CurrentSongColor"": ""green"",
        ""PreviousSongColor"": ""grey"",
        ""NextSongColor"": ""grey""
    },
    ""WholePlaylist"": {
        ""BorderColor"": [255,255,255],
        ""BorderStyle"": ""Rounded"",
        ""ChoosingColor"": ""yellow""
    },
    ""Time"": {
        ""BorderColor"": [255,255,255],
        ""BorderStyle"": ""Rounded"",
        ""PlayingLetterColor"": ""white"",
        ""PlayingLetterLetter"": ""❚❚"",
        ""PausedLetterColor"": ""white"",
        ""PausedLetterLetter"": ""▶"",
        ""ShuffleLetterOffColor"": ""red"",
        ""ShuffleOffLetter"": ""⇌"",
        ""ShuffleLetterOnColor"": ""green"",
        ""ShuffleOnLetter"": ""⇌"",
        ""LoopLetterOffColor"": ""red"",
        ""LoopOffLetter"": ""↻"",
        ""LoopLetterOnColor"": ""green"",
        ""LoopOnLetter"": ""⟳"",
        ""CurrentTimeColor"": ""white"",
        ""TotalTimeColor"": ""white"",
        ""VolumeColorNotMuted"": ""white"",
        ""VolumeColorMuted"": ""grey strikethrough"",
        ""TimebarColor"": ""white"",
        ""TimebarLetter"": ""█""
    },
    ""GeneralHelp"": {
        ""BorderColor"": [255,255,255],
        ""BorderStyle"": ""Rounded"",
        ""ControlTextColor"": ""white"",
        ""DescriptionTextColor"": ""white"",
        ""ModifierTextColor_1"": ""green"",
        ""ModifierTextColor_2"": ""yellow"", // TODO
        ""ModifierTextColor_3"": ""red"" // TODO
    },
    ""GeneralSettings"": {
        ""BorderColor"": [255,255,255],
        ""BorderStyle"": ""Rounded"",
        ""SettingTextColor"": ""white"",
        ""SettingValueColor"": ""green"",
        ""SettingChangeValueColor"": ""white"",
        ""SettingChangeValueValueColor"": ""green""
    },
    ""EditKeybinds"": {
        ""BorderColor"": [255,255,255],
        ""BorderStyle"": ""Rounded"",
        ""DescriptionColor"": ""white"",
        ""CurrentControlColor"": ""white"",
        ""CurrentKeyColor"": ""red"",
        ""EnteredKeyColor"": ""lightblue""
    },
    ""LanguageChange"": {
        ""BorderColor"": [255,255,255],
        ""BorderStyle"": ""Rounded"",
        ""TextColor"": ""white"",
        ""CurrentLanguageColor"": ""red""
    }
}";

        public static Theme CurrentTheme { get; private set; }
        static string themePath = Path.Combine(Utils.JammerPath, "themes");
        public static void Init() {
            CreateTheme("Default");            

            if (!SetThemeUsingPreferences()) {
                SetTheme("Default");
            }
        }
        public static void CreateTheme(string themeName) {
            if (!Directory.Exists(themePath)) {
                Directory.CreateDirectory(themePath);
            }
            if (!File.Exists(Path.Combine(themePath, themeName + ".json"))) {
                File.WriteAllText(Path.Combine(themePath, themeName + ".json"), fileContent);
            }
        }

        public static bool SetTheme(string themeName = "Default") {
            string path = Path.Combine(themePath, themeName + ".json");
            if (!File.Exists(path)) {
                AnsiConsole.MarkupLine("[red]Error:[/] Theme [yellow]{0}[/] does not exist", themeName);
                return false;
            }
            string json = File.ReadAllText(path);
            string jsonWithoutComments = Regex.Replace(json, @"//.*$", "", RegexOptions.Multiline);
            var theme = System.Text.Json.JsonSerializer.Deserialize<Theme>(jsonWithoutComments);

            if (theme == null) {
                AnsiConsole.MarkupLine("[red]Error:[/] Theme [yellow]{0}[/] is not valid", themeName);
                return false;
            }
            Preferences.theme = themeName;
            CurrentTheme = theme;
            return true;
        }

        public static bool SetThemeUsingPreferences() {
            string themeName = Preferences.theme;

            if (themeName == null) {
                return false;
            }

            if (SetTheme(themeName)) {
                return true;
            }

            Preferences.theme = "";
            return false;
        }

        public static string sColor(string str, string color) {
            return $"[{color}]{str}[/]";
        }

        public static Color bColor(int[] color) {
            return new Color((byte)color[0], (byte)color[1], (byte)color[2]);
        }

        public static TableBorder bStyle(string style) {
            // to lowercase
            style = style.ToLower();
            
            if (style == "ascii")
                return TableBorder.Ascii;
            if (style == "ascii2")
                return TableBorder.Ascii2;
            if (style == "asciidoublehead")
                return TableBorder.AsciiDoubleHead;
            if (style == "horizontal")
                return TableBorder.Horizontal;
            if (style == "simple")
                return TableBorder.Simple;
            if (style == "simpleheavy")
                return TableBorder.SimpleHeavy;
            if (style == "minimal")
                return TableBorder.Minimal;
            if (style == "minimalheavyhead")
                return TableBorder.MinimalHeavyHead;
            if (style == "minimaldoublehead")
                return TableBorder.MinimalDoubleHead;
            if (style == "square")
                return TableBorder.Square;
            if (style == "rounded")
                return TableBorder.Rounded;
            if (style == "heavy")
                return TableBorder.Heavy;
            if (style == "heavyedge")
                return TableBorder.HeavyEdge;
            if (style == "heavyhead")
                return TableBorder.HeavyHead;
            if (style == "double")
                return TableBorder.Double;
            if (style == "doubleedge")
                return TableBorder.DoubleEdge;
            if (style == "markdown")
                return TableBorder.Markdown;

            throw new Exception("Invalid Border Style");
        }

        public class Theme
        {
            public PlaylistTheme? Playlist { get; set; } 
            public GeneralPlaylistTheme? GeneralPlaylist { get; set; }
            public WholePlaylistTheme? WholePlaylist { get; set; }
            public TimeTheme? Time { get; set; }
            public GeneralHelpTheme? GeneralHelp { get; set; }
            public GeneralSettingsTheme? GeneralSettings { get; set; }
            public EditKeybindsTheme? EditKeybinds { get; set; }
            public LanguageChangeTheme? LanguageChange { get; set; }
        }

        public class PlaylistTheme
        {
            public string? PathColor { get; set; }
            public string? ErrorColor { get; set; }
            public string? SuccessColor { get; set; }
            public string? InfoColor { get; set; }
            public string? PlaylistNameColor { get; set; }
            public string? MiniHelpBorderStyle { get; set; }
            public int[]? MiniHelpBorderColor { get; set; }
            public string? HelpLetterColor { get; set; }
            public string? ForHelpTextColor { get; set; }
            public string? SettingsLetterColor { get; set; }
            public string? ForSettingsTextColor { get; set; }
            public string? PlaylistLetterColor { get; set; }
            public string? ForPlaylistTextColor { get; set; }
            public string? VisualizerColor { get; set; }
        }

        public class GeneralPlaylistTheme
        {
            public int[]? BorderColor { get; set; }
            public string? BorderStyle { get; set; }
            public string? CurrentSongColor { get; set; }
            public string? PreviousSongColor { get; set; }
            public string? NextSongColor { get; set; }
        }

        public class WholePlaylistTheme
        {
            public int[]? BorderColor { get; set; }
            public string? BorderStyle { get; set; }
            public string? ChoosingColor { get; set; }
        }

        public class TimeTheme
        {
            public int[]? BorderColor { get; set; }
            public string? BorderStyle { get; set; }
            public string? PlayingLetterColor { get; set; }
            public string? PlayingLetterLetter { get; set; }
            public string? PausedLetterColor { get; set; }
            public string? PausedLetterLetter { get; set; }
            public string? ShuffleLetterOffColor { get; set; }
            public string? ShuffleOffLetter { get; set; }
            public string? ShuffleLetterOnColor { get; set; }
            public string? ShuffleOnLetter { get; set; }
            public string? LoopLetterOffColor { get; set; }
            public string? LoopOffLetter { get; set; }
            public string? LoopLetterOnColor { get; set; }
            public string? LoopOnLetter { get; set; }
            public string? CurrentTimeColor { get; set; }
            public string? TotalTimeColor { get; set; }
            public string? VolumeColorNotMuted { get; set; }
            public string? VolumeColorMuted { get; set; }
            public string? TimebarColor { get; set; }
            public string? TimebarLetter { get; set; }
        }

        public class GeneralHelpTheme
        {
            public int[]? BorderColor { get; set; }
            public string? BorderStyle { get; set; }
            public string? ControlTextColor { get; set; }
            public string? DescriptionTextColor { get; set; }
            public string? ModifierTextColor_1 { get; set; }
            public string? ModifierTextColor_2 { get; set; }
            public string? ModifierTextColor_3 { get; set; }
        }

        public class GeneralSettingsTheme
        {
            public int[]? BorderColor { get; set; }
            public string? BorderStyle { get; set; }
            public string? SettingTextColor { get; set; }
            public string? SettingValueColor { get; set; }
            public string? SettingChangeValueColor { get; set; }
            public string? SettingChangeValueValueColor { get; set; }
        }

        public class EditKeybindsTheme
        {
            public int[]? BorderColor { get; set; }
            public string? BorderStyle { get; set; }
            public string? DescriptionColor { get; set; }
            public string? CurrentControlColor { get; set; }
            public string? CurrentKeyColor { get; set; }
            public string? EnteredKeyColor { get; set; }
        }

        public class LanguageChangeTheme
        {
            public int[]? BorderColor { get; set; }
            public string? BorderStyle { get; set; }
            public string? TextColor { get; set; }
            public string? CurrentLanguageColor { get; set; }
        }
    }
}
