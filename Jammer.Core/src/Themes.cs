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
        public static readonly string comments = @"// Colors https://spectreconsole.net/appendix/colors and https://spectreconsole.net/appendix/styles
// write the color name in lowercase ' ' and the styles ' '
// ""PlaylistNameColor"": ""red bold italic""
// but you can have just ""bold""
// if you type """" it will be the default color
// Border Styles https://spectreconsole.net/appendix/borders
// write the border name with CamelCase
// Example: Rounded
// Only the BorderColor is in RGB format: [0-255,0-255,0-255]
";

        public static readonly string fileContent = @"
{
    ""Playlist"": {
        ""BorderStyle"": ""Rounded"",
        ""BorderColor"": [255,255,255],
        ""PathColor"": """",
        ""ErrorColor"": ""red"",
        ""SuccessColor"": ""green"",
        ""InfoColor"": ""green"",
        ""PlaylistNameColor"": ""cyan"",
        ""MiniHelpBorderStyle"": ""Rounded"",
        ""MiniHelpBorderColor"": [255,255,255],
        ""HelpLetterColor"": ""red"",
        ""ForHelpTextColor"": """",
        ""SettingsLetterColor"": ""yellow"",
        ""ForSettingsTextColor"": """",
        ""ForSeperatorTextColor"": """",
        ""PlaylistLetterColor"": ""green"",
        ""ForPlaylistTextColor"": """",
        ""VisualizerColor"": """",
        ""RandomTextColor"": """"
    },
    ""GeneralPlaylist"": {
        ""BorderColor"": [255,255,255],
        ""BorderStyle"": ""Rounded"",
        ""CurrentSongColor"": ""green"",
        ""PreviousSongColor"": ""grey"",
        ""NextSongColor"": ""grey"",
        ""NoneSongColor"": ""grey""
    },
    ""WholePlaylist"": {
        ""BorderColor"": [255,255,255],
        ""BorderStyle"": ""Rounded"",
        ""NormalSongColor"": """",
        ""ChoosingColor"": ""yellow"",
        ""CurrentSongColor"": ""green""
    },
    ""Time"": {
        ""BorderColor"": [255,255,255],
        ""BorderStyle"": ""Rounded"",
        ""PlayingLetterColor"": """",
        ""PlayingLetterLetter"": ""❚❚"",
        ""PausedLetterColor"": """",
        ""PausedLetterLetter"": ""▶"",
        ""StoppedLetterColor"": """",
        ""StoppedLetterLetter"": ""■"",
        ""NextLetterColor"": """",
        ""NextLetterLetter"": ""▶▶"",
        ""PreviousLetterColor"": """",
        ""PreviousLetterLetter"": ""◀◀"",
        ""ShuffleLetterOffColor"": ""red"",
        ""ShuffleOffLetter"": ""⇌ "",
        ""ShuffleLetterOnColor"": ""green"",
        ""ShuffleOnLetter"": ""⇌ "",
        ""LoopLetterOffColor"": ""red"",
        ""LoopOffLetter"": "" ↻  "",
        ""LoopLetterOnColor"": ""green"",
        ""LoopOnLetter"": "" ⟳  "",
        ""TimeColor"": """",
        ""VolumeColorNotMuted"": """",
        ""VolumeColorMuted"": ""grey strikethrough"",
        ""TimebarColor"": """",
        ""TimebarLetter"": ""█""
    },
    ""GeneralHelp"": {
        ""BorderColor"": [255,255,255],
        ""BorderStyle"": ""Rounded"",
        ""HeaderTextColor"": """",
        ""ControlTextColor"": """",
        ""DescriptionTextColor"": """",
        ""ModifierTextColor_1"": ""green"",
        ""ModifierTextColor_2"": ""yellow"",
        ""ModifierTextColor_3"": ""red""
    },
    ""GeneralSettings"": {
        ""BorderColor"": [255,255,255],
        ""BorderStyle"": ""Rounded"",
        ""HeaderTextColor"": """",
        ""SettingTextColor"": """",
        ""SettingValueColor"": ""green"",
        ""SettingChangeValueColor"": """",
        ""SettingChangeValueValueColor"": ""green""
    },
    ""EditKeybinds"": {
        ""BorderColor"": [255,255,255],
        ""BorderStyle"": ""Rounded"",
        ""HeaderTextColor"": """",
        ""DescriptionColor"": """",
        ""CurrentControlColor"": """",
        ""CurrentKeyColor"": ""red"",
        ""EnteredKeyColor"": ""cyan""
    },
    ""LanguageChange"": {
        ""BorderColor"": [255,255,255],
        ""BorderStyle"": ""Rounded"",
        ""TextColor"": """",
        ""CurrentLanguageColor"": ""red""
    },
    ""InputBox"": {
        ""BorderColor"": [255,255,255],
        ""BorderStyle"": ""Rounded"",
        ""TitleColor"": """",
        ""InputTextColor"": """",
        ""InputBorderStyle"": ""Rounded"",
        ""InputBorderColor"": [255,255,255],
        ""TitleColorIfError"": ""red bold"",
        ""InputTextColorIfError"": ""red"",
        ""MultiSelectMoreChoicesTextColor"": ""grey""
    }
}";

        public static Theme? CurrentTheme { get; private set; }
        static string themePath = Path.Combine(Utils.JammerPath, "themes");
        public static void Init() {
            int returne = 0;
            if (Preferences.GetTheme() == null || Preferences.GetTheme() == "" || Preferences.GetTheme() == "Jammer Default") {
                SetDefaultTheme();
                returne = 1;
            }            

            if (!SetThemeUsingPreferences() && returne == 0) {
                AnsiConsole.MarkupLine("[red]Error:[/] Theme [yellow]"+Preferences.GetTheme()+"[/] does not exist");
                AnsiConsole.MarkupLine("[red]Error:[/] Setting theme to [yellow]Jammer Default[/]");
                SetDefaultTheme();
            }
            if (Preferences.theme != "Jammer Default") {
                AddAllMissingPropertiesInJsonFileIfMissing(Path.Combine(themePath, Preferences.GetTheme() + ".json"));
            }
        }
        public static void CreateTheme(string themeName) {
            if (!Directory.Exists(themePath)) {
                Directory.CreateDirectory(themePath);
            }
            if (!File.Exists(Path.Combine(themePath, themeName + ".json"))) {
                File.WriteAllText(Path.Combine(themePath, themeName + ".json"), comments + fileContent);
            }
        }

        public static string[] GetAllThemes() {
            if (!Directory.Exists(themePath)) {
                Directory.CreateDirectory(themePath);
            }

            string[] themes = Directory.GetFiles(themePath, "*.json");

            for (int i = 0; i < themes.Length; i++) {
                themes[i] = Path.GetFileNameWithoutExtension(themes[i]);
            }


            return themes;
        }

        public static void SetDefaultTheme() {
            Preferences.theme = "Jammer Default";
            // read fileContent
            string jsonWithoutComments = Regex.Replace(fileContent, @"//.*$", "", RegexOptions.Multiline);
            var theme = System.Text.Json.JsonSerializer.Deserialize<Theme>(jsonWithoutComments);
            CurrentTheme = theme;
        }

        public static void AddAllMissingPropertiesInJsonFileIfMissing(string pathToTheme) {
            // loop through all properties in default theme and add them to the theme if they are missing in the theme
            // if missing, add the property with the value from the default theme
            string json = File.ReadAllText(pathToTheme);
            string jsonWithoutComments = Regex.Replace(json, @"//.*$", "", RegexOptions.Multiline);
            var theme = System.Text.Json.JsonSerializer.Deserialize<Theme>(jsonWithoutComments);
            var defaultThemeWithoutComments = Regex.Replace(fileContent, @"//.*$", "", RegexOptions.Multiline);
            var defaultThemeJson = System.Text.Json.JsonSerializer.Deserialize<Theme>(defaultThemeWithoutComments);
            
            // loop through all properties in default theme
            foreach (var property in defaultThemeJson.GetType().GetProperties()) {
                // if the property is null in the theme, add it
                if (property.GetValue(theme) == null) {
                    property.SetValue(theme, property.GetValue(defaultThemeJson));
                }
                // if the property is a class, loop through all properties in the class
                else if (property.GetValue(theme) != null) {
                    foreach (var subProperty in property.GetValue(defaultThemeJson).GetType().GetProperties()) {
                        // if the subProperty is null in the theme, add it
                        if (subProperty.GetValue(property.GetValue(theme)) == null) {
                            subProperty.SetValue(property.GetValue(theme), subProperty.GetValue(property.GetValue(defaultThemeJson)));
                        }
                    }
                }
            }

            // write the theme back to the file
            string newJson = JsonConvert.SerializeObject(theme, Formatting.Indented);
            File.WriteAllText(pathToTheme, comments + newJson);

            // set the theme
            CurrentTheme = theme;

        }
        
        public static bool SetTheme(string themeName) {

            if (themeName == "Jammer Default") {
                SetDefaultTheme();
            }

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
            string themeName = Preferences.GetTheme();

            if (themeName == "Jammer Default") {
                SetDefaultTheme();
                return true;
            }
            
            if (themeName == null) {
                return false;
            }

            if (SetTheme(themeName)) {
                return true;
            }

            Preferences.theme = "Jammer Default";
            return false;
        }

        public static string sColor(string str, string color) {
            if (Play.EmptySpaces(str) || color == "") {
                return str;
            }
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
            if (style == "none")
                return TableBorder.None;
                
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
            public InputBoxTheme? InputBox { get; set; }
        }

        public class PlaylistTheme
        {
            public string? BorderStyle { get; set; }
            public int[]? BorderColor { get; set; }
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
            public string? ForSeperatorTextColor { get; set; }
            public string? VisualizerColor { get; set; }
            public string? RandomTextColor { get; set; }
        }

        public class GeneralPlaylistTheme
        {
            public int[]? BorderColor { get; set; }
            public string? BorderStyle { get; set; }
            public string? CurrentSongColor { get; set; }
            public string? PreviousSongColor { get; set; }
            public string? NextSongColor { get; set; }
            public string? NoneSongColor { get; set; }
        }

        public class WholePlaylistTheme
        {
            public int[]? BorderColor { get; set; }
            public string? BorderStyle { get; set; }
            public string? ChoosingColor { get; set; }
            public string? NormalSongColor { get; set; }
            public string? CurrentSongColor { get; set; }
        }

        public class TimeTheme
        {
            public int[]? BorderColor { get; set; }
            public string? BorderStyle { get; set; }
            public string? PlayingLetterColor { get; set; }
            public string? PlayingLetterLetter { get; set; }
            public string? PausedLetterColor { get; set; }
            public string? PausedLetterLetter { get; set; }
            public string? StoppedLetterColor { get; set; }
            public string? StoppedLetterLetter { get; set; }
            public string? NextLetterColor { get; set; }
            public string? NextLetterLetter { get; set; }
            public string? PreviousLetterColor { get; set; }
            public string? PreviousLetterLetter { get; set; }
            public string? ShuffleLetterOffColor { get; set; }
            public string? ShuffleOffLetter { get; set; }
            public string? ShuffleLetterOnColor { get; set; }
            public string? ShuffleOnLetter { get; set; }
            public string? LoopLetterOffColor { get; set; }
            public string? LoopOffLetter { get; set; }
            public string? LoopLetterOnColor { get; set; }
            public string? LoopOnLetter { get; set; }
            public string? TimeColor { get; set; }
            public string? VolumeColorNotMuted { get; set; }
            public string? VolumeColorMuted { get; set; }
            public string? TimebarColor { get; set; }
            public string? TimebarLetter { get; set; }
        }

        public class GeneralHelpTheme
        {
            public int[]? BorderColor { get; set; }
            public string? BorderStyle { get; set; }
            public string? HeaderTextColor { get; set; }
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
            public string? HeaderTextColor { get; set; }
            public string? SettingTextColor { get; set; }
            public string? SettingValueColor { get; set; }
            public string? SettingChangeValueColor { get; set; }
            public string? SettingChangeValueValueColor { get; set; }
        }

        public class EditKeybindsTheme
        {
            public int[]? BorderColor { get; set; }
            public string? BorderStyle { get; set; }
            public string? HeaderTextColor { get; set; }
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

        public class InputBoxTheme
        {
            public int[]? BorderColor { get; set; }
            public string? BorderStyle { get; set; }
            public string? InputTextColor { get; set; }
            public string? TitleColor { get; set; }
            public string? InputBorderStyle { get; set; }
            public int[]? InputBorderColor { get; set; }
            public string? TitleColorIfError { get; set; }
            public string? InputTextColorIfError { get; set; }
            public string? MultiSelectMoreChoicesTextColor { get; set; }
        }
    }
}
