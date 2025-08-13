using Spectre.Console;

namespace Jammer.Components
{
    /// <summary>
    /// Component responsible for rendering the help menu
    /// Extracted from TUI.DrawHelp method
    /// </summary>
    public class HelpMenuComponent : IUIComponent
    {
        private static int _currentPage = 1;
        private static int _totalPages = 2;

        public Table Render(LayoutConfig layout)
        {
            var table = new Table();
            table.Border = Themes.bStyle(Themes.CurrentTheme.GeneralHelp.BorderStyle);
            table.BorderColor(Themes.bColor(Themes.CurrentTheme.GeneralHelp.BorderColor));
            table.Width = layout.ConsoleWidth;

            // Build the help content using the original logic
            BuildHelpContent(table);

            return table;
        }

        private void BuildHelpContent(Table table)
        {
            // Parse keybindings for display
            char separator = '+';
            var keybindings = ParseKeybindings(separator);

            // Add table headers with fixed column widths
            table.AddColumn(new TableColumn(Themes.sColor(Locale.Help.Controls, Themes.CurrentTheme.GeneralHelp.HeaderTextColor)).Width(20));
            table.AddColumn(new TableColumn(Themes.sColor(Locale.Help.Description, Themes.CurrentTheme.GeneralHelp.HeaderTextColor)).Width(25));
            table.AddColumn(new TableColumn(Themes.sColor(Locale.Help.ModControls, Themes.CurrentTheme.GeneralHelp.HeaderTextColor)).Width(20));
            table.AddColumn(new TableColumn(Themes.sColor($"{Locale.Help.Description} ({_currentPage}/{_totalPages})", Themes.CurrentTheme.GeneralHelp.HeaderTextColor)).Width(30));

            // Add help rows based on current page
            if (_currentPage == 1)
            {
                AddHelpRowsPage1(table, keybindings);
            }
            else if (_currentPage == 2)
            {
                AddHelpRowsPage2(table, keybindings);
            }
        }

        private Dictionary<string, string[]> ParseKeybindings(char separator)
        {
            return new Dictionary<string, string[]>
            {
                ["ToMainMenu"] = Keybindings.ToMainMenu.Replace(" ", "").Split(separator),
                ["AddSongToPlaylist"] = Keybindings.AddSongToPlaylist.Replace(" ", "").Split(separator),
                ["AddSongToQueue"] = Keybindings.AddSongToQueue.Replace(" ", "").Split(separator),
                ["ShowSongsInPlaylists"] = Keybindings.ShowSongsInPlaylists.Replace(" ", "").Split(separator),
                ["ListAllPlaylists"] = Keybindings.ListAllPlaylists.Replace(" ", "").Split(separator),
                ["PlayOtherPlaylist"] = Keybindings.PlayOtherPlaylist.Replace(" ", "").Split(separator),
                ["SaveCurrentPlaylist"] = Keybindings.SaveCurrentPlaylist.Replace(" ", "").Split(separator),
                ["SaveAsPlaylist"] = Keybindings.SaveAsPlaylist.Replace(" ", "").Split(separator),
                ["ShufflePlaylist"] = Keybindings.ShufflePlaylist.Replace(" ", "").Split(separator),
                ["PlaySong"] = Keybindings.PlaySong.Replace(" ", "").Split(separator),
                ["RedownloadCurrentSong"] = Keybindings.RedownloadCurrentSong.Replace(" ", "").Split(separator),
                ["PlayPause"] = Keybindings.PlayPause.Replace(" ", "").Split(separator),
                ["Quit"] = Keybindings.Quit.Replace(" ", "").Split(separator),
                ["Backwards5s"] = Keybindings.Backwards5s.Replace(" ", "").Split(separator),
                ["Forward5s"] = Keybindings.Forward5s.Replace(" ", "").Split(separator),
                ["VolumeUp"] = Keybindings.VolumeUp.Replace(" ", "").Split(separator),
                ["VolumeDown"] = Keybindings.VolumeDown.Replace(" ", "").Split(separator),
                ["VolumeUpByOne"] = Keybindings.VolumeUpByOne.Replace(" ", "").Split(separator),
                ["VolumeDownByOne"] = Keybindings.VolumeDownByOne.Replace(" ", "").Split(separator),
                ["Loop"] = Keybindings.Loop.Replace(" ", "").Split(separator),
                ["Mute"] = Keybindings.Mute.Replace(" ", "").Split(separator),
                ["Shuffle"] = Keybindings.Shuffle.Replace(" ", "").Split(separator),
                ["NextSong"] = Keybindings.NextSong.Replace(" ", "").Split(separator),
                ["PreviousSong"] = Keybindings.PreviousSong.Replace(" ", "").Split(separator),
                ["PlayRandomSong"] = Keybindings.PlayRandomSong.Replace(" ", "").Split(separator),
                ["DeleteCurrentSong"] = Keybindings.DeleteCurrentSong.Replace(" ", "").Split(separator),
                ["HardDeleteCurrentSong"] = Keybindings.HardDeleteCurrentSong.Replace(" ", "").Split(separator),
                ["SearchInPlaylist"] = Keybindings.SearchInPlaylist.Replace(" ", "").Split(separator),
                ["Search"] = Keybindings.Search.Replace(" ", "").Split(separator),
                ["RenameSong"] = Keybindings.RenameSong.Replace(" ", "").Split(separator),
                ["CommandHelpScreen"] = Keybindings.CommandHelpScreen.Replace(" ", "").Split(separator),
                ["Help"] = Keybindings.Help.Replace(" ", "").Split(separator),
                ["EditKeybindings"] = Keybindings.EditKeybindings.Replace(" ", "").Split(separator),
                ["ChangeLanguage"] = Keybindings.ChangeLanguage.Replace(" ", "").Split(separator),
                ["ChangeTheme"] = Keybindings.ChangeTheme.Replace(" ", "").Split(separator),
                ["ChangeSoundFont"] = Keybindings.ChangeSoundFont.Replace(" ", "").Split(separator),
                ["ToSongStart"] = Keybindings.ToSongStart.Replace(" ", "").Split(separator),
                ["ToSongEnd"] = Keybindings.ToSongEnd.Replace(" ", "").Split(separator),
                ["ShowLog"] = Keybindings.ShowLog.Replace(" ", "").Split(separator),
                ["ChooseSong"] = Keybindings.Choose.Replace(" ", "").Split(separator),
                ["ExitRssFeed"] = Keybindings.ExitRssFeed.Replace(" ", "").Split(separator),
                ["ShowHidePlaylist"] = Keybindings.ShowHidePlaylist.Replace(" ", "").Split(separator),
                ["ToggleInfo"] = Keybindings.ToggleInfo.Replace(" ", "").Split(separator),
                ["Settings"] = Keybindings.Settings.Replace(" ", "").Split(separator),
                ["PlaylistViewScrollup"] = Keybindings.PlaylistViewScrollup.Replace(" ", "").Split(separator),
                ["PlaylistViewScrolldown"] = Keybindings.PlaylistViewScrolldown.Replace(" ", "").Split(separator)
            };
        }

        private void AddHelpRowsPage1(Table table, Dictionary<string, string[]> keybindings)
        {
            // Basic Controls
            table.AddRow(
                DrawHelpTextColouring(keybindings["PlayPause"]),
                Themes.sColor(Locale.Help.PlayPause, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                DrawHelpTextColouring(keybindings["NextSong"]),
                Themes.sColor(Locale.Help.NextSong, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            );

            table.AddRow(
                DrawHelpTextColouring(keybindings["PreviousSong"]),
                Themes.sColor(Locale.Help.PreviousSong, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                DrawHelpTextColouring(keybindings["Quit"]),
                Themes.sColor(Locale.Help.Quit, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            );

            // Navigation
            table.AddRow(
                DrawHelpTextColouring(keybindings["ToMainMenu"]),
                Themes.sColor(Locale.Help.ToMainMenu, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                DrawHelpTextColouring(keybindings["Help"]),
                Themes.sColor("Show Help", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            );

            // Volume Controls
            table.AddRow(
                DrawHelpTextColouring(keybindings["VolumeUp"]),
                Themes.sColor(Locale.Help.VolumeUp, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                DrawHelpTextColouring(keybindings["VolumeDown"]),
                Themes.sColor(Locale.Help.VolumeDown, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            );

            table.AddRow(
                DrawHelpTextColouring(keybindings["VolumeUpByOne"]),
                Themes.sColor("Volume +1", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                DrawHelpTextColouring(keybindings["VolumeDownByOne"]),
                Themes.sColor("Volume -1", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            );

            table.AddRow(
                DrawHelpTextColouring(keybindings["Mute"]),
                Themes.sColor(Locale.Help.ToggleMute, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                DrawHelpTextColouring(keybindings["Forward5s"]),
                Themes.sColor(Locale.Help.Forward + " 5 " + Locale.Help.Seconds, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            );

            table.AddRow(
                DrawHelpTextColouring(keybindings["Backwards5s"]),
                Themes.sColor(Locale.Help.Rewind + " 5 " + Locale.Help.Seconds, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                DrawHelpTextColouring(keybindings["ToSongStart"]),
                Themes.sColor("Go to Start", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            );

            table.AddRow(
                DrawHelpTextColouring(keybindings["ToSongEnd"]),
                Themes.sColor("Go to End", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                DrawHelpTextColouring(keybindings["Loop"]),
                Themes.sColor(Locale.Help.ToggleLooping, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            );

            // Basic Playlist Controls
            table.AddRow(
                DrawHelpTextColouring(keybindings["Shuffle"]),
                Themes.sColor(Locale.Help.ToggleShuffle, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                DrawHelpTextColouring(keybindings["ShowHidePlaylist"]),
                Themes.sColor(Locale.Help.ToShowPlaylist, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            );

            // Scrolling
            table.AddRow(
                DrawHelpTextColouring(keybindings["PlaylistViewScrollup"]),
                Themes.sColor("Scroll Up", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                DrawHelpTextColouring(keybindings["PlaylistViewScrolldown"]),
                Themes.sColor("Scroll Down", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            );

            // Page navigation hint
            table.AddRow(
                "",
                ""
            );
            table.AddRow(
                "",
                "",
                Themes.sColor("PgDn/→", Themes.CurrentTheme.GeneralHelp.HeaderTextColor),
                Themes.sColor("Next page", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            );
        }

        private void AddHelpRowsPage2(Table table, Dictionary<string, string[]> keybindings)
        {
            // Playlist Management
            table.AddRow(
                DrawHelpTextColouring(keybindings["ShufflePlaylist"]),
                Themes.sColor(Locale.Help.ShufflePlaylist, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                DrawHelpTextColouring(keybindings["AddSongToPlaylist"]),
                Themes.sColor(Locale.Help.AddsongToPlaylist, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            );

            table.AddRow(
                DrawHelpTextColouring(keybindings["AddSongToQueue"]),
                Themes.sColor("Add to Queue", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                DrawHelpTextColouring(keybindings["ShowSongsInPlaylists"]),
                Themes.sColor(Locale.Help.ListAllSongsInOtherPlaylist, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            );

            table.AddRow(
                DrawHelpTextColouring(keybindings["ListAllPlaylists"]),
                Themes.sColor(Locale.Help.ListAllPlaylists, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                DrawHelpTextColouring(keybindings["PlayOtherPlaylist"]),
                Themes.sColor(Locale.Help.PlayOtherPlaylist, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            );

            table.AddRow(
                DrawHelpTextColouring(keybindings["SaveCurrentPlaylist"]),
                Themes.sColor(Locale.Help.SavePlaylist, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                DrawHelpTextColouring(keybindings["SaveAsPlaylist"]),
                Themes.sColor(Locale.Help.SaveAs, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            );

            // Song Management
            table.AddRow(
                DrawHelpTextColouring(keybindings["PlaySong"]),
                Themes.sColor(Locale.Help.PlaySongs, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                DrawHelpTextColouring(keybindings["PlayRandomSong"]),
                Themes.sColor(Locale.Help.PlayRandomSong, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            );

            table.AddRow(
                DrawHelpTextColouring(keybindings["DeleteCurrentSong"]),
                Themes.sColor(Locale.Help.DeleteCurrentSongFromPlaylist, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                DrawHelpTextColouring(keybindings["HardDeleteCurrentSong"]),
                Themes.sColor("Delete from PC", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            );

            table.AddRow(
                DrawHelpTextColouring(keybindings["RenameSong"]),
                Themes.sColor("Rename", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                DrawHelpTextColouring(keybindings["RedownloadCurrentSong"]),
                Themes.sColor(Locale.Help.RedownloadCurrentSong, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            );

            // Search and Navigation
            table.AddRow(
                DrawHelpTextColouring(keybindings["Search"]),
                Themes.sColor("Search", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                DrawHelpTextColouring(keybindings["SearchInPlaylist"]),
                Themes.sColor("Search Playlist", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            );

            table.AddRow(
                DrawHelpTextColouring(keybindings["ChooseSong"]),
                Themes.sColor("Select Song", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                DrawHelpTextColouring(keybindings["ToggleInfo"]),
                Themes.sColor("Show/Hide Info", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            );

            // Settings and Configuration
            table.AddRow(
                DrawHelpTextColouring(keybindings["Settings"]),
                Themes.sColor("Settings", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                DrawHelpTextColouring(keybindings["EditKeybindings"]),
                Themes.sColor(Locale.Help.EditKeybinds, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            );

            table.AddRow(
                DrawHelpTextColouring(keybindings["ChangeLanguage"]),
                Themes.sColor(Locale.Help.ChangeLanguage, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                DrawHelpTextColouring(keybindings["ChangeTheme"]),
                Themes.sColor("Change Theme", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            );

            table.AddRow(
                DrawHelpTextColouring(keybindings["ChangeSoundFont"]),
                Themes.sColor("Change Font", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                DrawHelpTextColouring(keybindings["ShowLog"]),
                Themes.sColor("Show Log", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            );

            // Special
            table.AddRow(
                DrawHelpTextColouring(keybindings["CommandHelpScreen"]),
                Themes.sColor(Locale.Help.ShowCmdHelp, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                DrawHelpTextColouring(keybindings["ExitRssFeed"]),
                Themes.sColor("Exit RSS", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            );

            // Page navigation hint
            table.AddRow(
                "",
                ""
            );
            table.AddRow(
                Themes.sColor("PgUp/←", Themes.CurrentTheme.GeneralHelp.HeaderTextColor),
                Themes.sColor("Previous page", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                "",
                ""
            );
        }

        private string DrawHelpTextColouring(string[] textArray)
        {
            if (textArray.Length == 1)
            {
                return Themes.sColor(textArray[0], Themes.CurrentTheme.GeneralHelp.ControlTextColor);
            }
            else if (textArray.Length == 2)
            {
                return
                Themes.sColor(textArray[0], Themes.CurrentTheme.GeneralHelp.ModifierTextColor_1)
                + " + "
                + Themes.sColor(textArray[1], Themes.CurrentTheme.GeneralHelp.ControlTextColor);
            }
            else if (textArray.Length == 3)
            {
                return
                Themes.sColor(textArray[0], Themes.CurrentTheme.GeneralHelp.ModifierTextColor_1)
                + " + "
                + Themes.sColor(textArray[1], Themes.CurrentTheme.GeneralHelp.ModifierTextColor_2)
                + " + "
                + Themes.sColor(textArray[2], Themes.CurrentTheme.GeneralHelp.ControlTextColor);
            }
            else if (textArray.Length == 4)
            {
                return
                Themes.sColor(textArray[0], Themes.CurrentTheme.GeneralHelp.ModifierTextColor_1)
                + " + "
                + Themes.sColor(textArray[1], Themes.CurrentTheme.GeneralHelp.ModifierTextColor_2)
                + " + "
                + Themes.sColor(textArray[2], Themes.CurrentTheme.GeneralHelp.ModifierTextColor_3)
                + " + "
                + Themes.sColor(textArray[3], Themes.CurrentTheme.GeneralHelp.ControlTextColor);
            }
            else
            {
                return Themes.sColor(textArray[0], Themes.CurrentTheme.GeneralHelp.ControlTextColor);
            }
        }

        public static void NextHelpPage()
        {
            AnsiConsole.Clear();
            _currentPage = (_currentPage % _totalPages) + 1;
        }

        public static void PreviousHelpPage()
        {
            AnsiConsole.Clear();
            _currentPage = _currentPage > 1 ? _currentPage - 1 : _totalPages;
        }

        public static void SetHelpPage(int page)
        {
            if (page >= 1 && page <= _totalPages)
            {
                _currentPage = page;
            }
        }

        public static int GetCurrentPage()
        {
            return _currentPage;
        }

        public static int GetTotalPages()
        {
            return _totalPages;
        }

        /// <summary>
        /// Renders the complete help screen to console
        /// </summary>
        /// <param name="layout">Layout configuration</param>
        public static void DrawHelpToConsole(LayoutConfig layout)
        {
            var component = new HelpMenuComponent();
            var table = component.Render(layout);

            AnsiConsole.Cursor.SetPosition(0, 0);
            AnsiConsole.Write(table);
            
            // Note: DrawHelpSettingInfo() was private in TUI, so we skip it for now
            // This can be implemented separately if needed
        }
    }
}
