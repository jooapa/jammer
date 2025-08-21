using Spectre.Console;

namespace Jammer.Components
{
    /// <summary>
    /// Represents a single help item
    /// </summary>
    public class HelpItem
    {
        public string ControlKeys { get; set; } = "";
        public string Description { get; set; } = "";
        public string ModControlKeys { get; set; } = "";
        public string ModDescription { get; set; } = "";
    }

    /// <summary>
    /// Component responsible for rendering the help menu
    /// Extracted from TUI.DrawHelp method
    /// </summary>
    public class HelpMenuComponent : IUIComponent
    {
        private static int _currentPage = 1;
        private static int _totalPages = 1;
        private static readonly int _helpItemsPerPage = 10; // Configurable page size
        private static List<HelpItem> _helpItems = new List<HelpItem>();

        static HelpMenuComponent()
        {
            InitializeHelpItems();
            CalculateTotalPages();
        }

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
            // Add table headers with fixed column widths
            table.AddColumn(new TableColumn(Themes.sColor(Locale.Help.Controls, Themes.CurrentTheme.GeneralHelp.HeaderTextColor)).Width(20));
            table.AddColumn(new TableColumn(Themes.sColor(Locale.Help.Description, Themes.CurrentTheme.GeneralHelp.HeaderTextColor)).Width(25));
            table.AddColumn(new TableColumn(Themes.sColor(Locale.Help.ModControls, Themes.CurrentTheme.GeneralHelp.HeaderTextColor)).Width(20));
            table.AddColumn(new TableColumn(Themes.sColor($"{Locale.Help.Description} ({_currentPage}/{_totalPages})", Themes.CurrentTheme.GeneralHelp.HeaderTextColor)).Width(30));

            // Calculate which help items to show on current page
            var startIndex = (_currentPage - 1) * _helpItemsPerPage;
            var endIndex = Math.Min(startIndex + _helpItemsPerPage, _helpItems.Count);

            // Add help items for current page
            for (int i = startIndex; i < endIndex; i++)
            {
                var helpItem = _helpItems[i];
                table.AddRow(
                    helpItem.ControlKeys,
                    helpItem.Description,
                    helpItem.ModControlKeys,
                    helpItem.ModDescription
                );
            }

            // Add navigation hints
            AddNavigationHints(table);
        }

        private static void InitializeHelpItems()
        {
            _helpItems.Clear();
            
            // Parse keybindings for display
            char separator = '+';
            var keybindings = ParseKeybindings(separator);

            // Basic Controls
            _helpItems.Add(new HelpItem
            {
                ControlKeys = DrawHelpTextColouring(keybindings["PlayPause"]),
                Description = Themes.sColor(Locale.Help.PlayPause, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                ModControlKeys = DrawHelpTextColouring(keybindings["NextSong"]),
                ModDescription = Themes.sColor(Locale.Help.NextSong, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            });

            _helpItems.Add(new HelpItem
            {
                ControlKeys = DrawHelpTextColouring(keybindings["PreviousSong"]),
                Description = Themes.sColor(Locale.Help.PreviousSong, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                ModControlKeys = DrawHelpTextColouring(keybindings["Quit"]),
                ModDescription = Themes.sColor(Locale.Help.Quit, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            });

            // Navigation
            _helpItems.Add(new HelpItem
            {
                ControlKeys = DrawHelpTextColouring(keybindings["ToMainMenu"]),
                Description = Themes.sColor(Locale.Help.ToMainMenu, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                ModControlKeys = DrawHelpTextColouring(keybindings["Help"]),
                ModDescription = Themes.sColor("Show Help", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            });

            // Volume Controls
            _helpItems.Add(new HelpItem
            {
                ControlKeys = DrawHelpTextColouring(keybindings["VolumeUp"]),
                Description = Themes.sColor(Locale.Help.VolumeUp, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                ModControlKeys = DrawHelpTextColouring(keybindings["VolumeDown"]),
                ModDescription = Themes.sColor(Locale.Help.VolumeDown, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            });

            _helpItems.Add(new HelpItem
            {
                ControlKeys = DrawHelpTextColouring(keybindings["VolumeUpByOne"]),
                Description = Themes.sColor("Volume +1", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                ModControlKeys = DrawHelpTextColouring(keybindings["VolumeDownByOne"]),
                ModDescription = Themes.sColor("Volume -1", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            });

            _helpItems.Add(new HelpItem
            {
                ControlKeys = DrawHelpTextColouring(keybindings["Mute"]),
                Description = Themes.sColor(Locale.Help.ToggleMute, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                ModControlKeys = DrawHelpTextColouring(keybindings["Forward5s"]),
                ModDescription = Themes.sColor(Locale.Help.Forward + " 5 " + Locale.Help.Seconds, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            });

            _helpItems.Add(new HelpItem
            {
                ControlKeys = DrawHelpTextColouring(keybindings["Backwards5s"]),
                Description = Themes.sColor(Locale.Help.Rewind + " 5 " + Locale.Help.Seconds, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                ModControlKeys = DrawHelpTextColouring(keybindings["ToSongStart"]),
                ModDescription = Themes.sColor("Go to Start", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            });

            _helpItems.Add(new HelpItem
            {
                ControlKeys = DrawHelpTextColouring(keybindings["ToSongEnd"]),
                Description = Themes.sColor("Go to End", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                ModControlKeys = DrawHelpTextColouring(keybindings["Loop"]),
                ModDescription = Themes.sColor(Locale.Help.ToggleLooping, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            });

            // Basic Playlist Controls
            _helpItems.Add(new HelpItem
            {
                ControlKeys = DrawHelpTextColouring(keybindings["Shuffle"]),
                Description = Themes.sColor(Locale.Help.ToggleShuffle, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                ModControlKeys = DrawHelpTextColouring(keybindings["ShowHidePlaylist"]),
                ModDescription = Themes.sColor(Locale.Help.ToShowPlaylist, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            });

            // Scrolling
            _helpItems.Add(new HelpItem
            {
                ControlKeys = DrawHelpTextColouring(keybindings["PlaylistViewScrollup"]),
                Description = Themes.sColor("Scroll Up", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                ModControlKeys = DrawHelpTextColouring(keybindings["PlaylistViewScrolldown"]),
                ModDescription = Themes.sColor("Scroll Down", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            });

            // Playlist Management
            _helpItems.Add(new HelpItem
            {
                ControlKeys = DrawHelpTextColouring(keybindings["ShufflePlaylist"]),
                Description = Themes.sColor(Locale.Help.ShufflePlaylist, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                ModControlKeys = DrawHelpTextColouring(keybindings["AddSongToPlaylist"]),
                ModDescription = Themes.sColor(Locale.Help.AddsongToPlaylist, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            });

            _helpItems.Add(new HelpItem
            {
                ControlKeys = DrawHelpTextColouring(keybindings["AddSongToQueue"]),
                Description = Themes.sColor("Add to Queue", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                ModControlKeys = DrawHelpTextColouring(keybindings["ShowSongsInPlaylists"]),
                ModDescription = Themes.sColor(Locale.Help.ListAllSongsInOtherPlaylist, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            });

            _helpItems.Add(new HelpItem
            {
                ControlKeys = DrawHelpTextColouring(keybindings["ListAllPlaylists"]),
                Description = Themes.sColor(Locale.Help.ListAllPlaylists, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                ModControlKeys = DrawHelpTextColouring(keybindings["PlayOtherPlaylist"]),
                ModDescription = Themes.sColor(Locale.Help.PlayOtherPlaylist, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            });

            _helpItems.Add(new HelpItem
            {
                ControlKeys = DrawHelpTextColouring(keybindings["SaveCurrentPlaylist"]),
                Description = Themes.sColor(Locale.Help.SavePlaylist, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                ModControlKeys = DrawHelpTextColouring(keybindings["SaveAsPlaylist"]),
                ModDescription = Themes.sColor(Locale.Help.SaveAs, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            });

            // Song Management
            _helpItems.Add(new HelpItem
            {
                ControlKeys = DrawHelpTextColouring(keybindings["PlaySong"]),
                Description = Themes.sColor(Locale.Help.PlaySongs, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                ModControlKeys = DrawHelpTextColouring(keybindings["PlayRandomSong"]),
                ModDescription = Themes.sColor(Locale.Help.PlayRandomSong, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            });

            _helpItems.Add(new HelpItem
            {
                ControlKeys = DrawHelpTextColouring(keybindings["DeleteCurrentSong"]),
                Description = Themes.sColor(Locale.Help.DeleteCurrentSongFromPlaylist, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                ModControlKeys = DrawHelpTextColouring(keybindings["HardDeleteCurrentSong"]),
                ModDescription = Themes.sColor("Delete from PC", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            });

            _helpItems.Add(new HelpItem
            {
                ControlKeys = DrawHelpTextColouring(keybindings["RenameSong"]),
                Description = Themes.sColor("Rename", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                ModControlKeys = DrawHelpTextColouring(keybindings["RedownloadCurrentSong"]),
                ModDescription = Themes.sColor(Locale.Help.RedownloadCurrentSong, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            });

            // Search and Navigation
            _helpItems.Add(new HelpItem
            {
                ControlKeys = DrawHelpTextColouring(keybindings["Search"]),
                Description = Themes.sColor("Search", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                ModControlKeys = DrawHelpTextColouring(keybindings["SearchInPlaylist"]),
                ModDescription = Themes.sColor("Search Playlist", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            });

            _helpItems.Add(new HelpItem
            {
                ControlKeys = DrawHelpTextColouring(keybindings["ChooseSong"]),
                Description = Themes.sColor("Select Song", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                ModControlKeys = DrawHelpTextColouring(keybindings["ToggleInfo"]),
                ModDescription = Themes.sColor("Show/Hide Info", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            });

            // Settings and Configuration
            _helpItems.Add(new HelpItem
            {
                ControlKeys = DrawHelpTextColouring(keybindings["Settings"]),
                Description = Themes.sColor("Settings", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                ModControlKeys = DrawHelpTextColouring(keybindings["EditKeybindings"]),
                ModDescription = Themes.sColor(Locale.Help.EditKeybinds, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            });

            _helpItems.Add(new HelpItem
            {
                ControlKeys = DrawHelpTextColouring(keybindings["ChangeLanguage"]),
                Description = Themes.sColor(Locale.Help.ChangeLanguage, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                ModControlKeys = DrawHelpTextColouring(keybindings["ChangeTheme"]),
                ModDescription = Themes.sColor("Change Theme", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            });

            _helpItems.Add(new HelpItem
            {
                ControlKeys = DrawHelpTextColouring(keybindings["ChangeSoundFont"]),
                Description = Themes.sColor("Change Font", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                ModControlKeys = DrawHelpTextColouring(keybindings["ShowLog"]),
                ModDescription = Themes.sColor("Show Log", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            });

            // Special
            _helpItems.Add(new HelpItem
            {
                ControlKeys = DrawHelpTextColouring(keybindings["CommandHelpScreen"]),
                Description = Themes.sColor(Locale.Help.ShowCmdHelp, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                ModControlKeys = DrawHelpTextColouring(keybindings["ExitRssFeed"]),
                ModDescription = Themes.sColor("Exit RSS", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
            });
        }

        private static void CalculateTotalPages()
        {
            _totalPages = Math.Max(1, (int)Math.Ceiling((double)_helpItems.Count / _helpItemsPerPage));
        }

        private void AddNavigationHints(Table table)
        {
            // Add empty row for spacing
            table.AddRow("", "", "", "");

            if (_totalPages > 1)
            {
                if (_currentPage < _totalPages)
                {
                    table.AddRow(
                        "",
                        "",
                        Themes.sColor("PgDn/→", Themes.CurrentTheme.GeneralHelp.HeaderTextColor),
                        Themes.sColor("Next page", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)
                    );
                }

                if (_currentPage > 1)
                {
                    table.AddRow(
                        Themes.sColor("PgUp/←", Themes.CurrentTheme.GeneralHelp.HeaderTextColor),
                        Themes.sColor("Previous page", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),
                        "",
                        ""
                    );
                }
            }
        }

        /// <summary>
        /// Adds a new help item to the help list. Call RefreshPagination() after adding items.
        /// </summary>
        public static void AddHelpItem(string controlKeys, string description, string modControlKeys = "", string modDescription = "")
        {
            _helpItems.Add(new HelpItem
            {
                ControlKeys = controlKeys,
                Description = description,
                ModControlKeys = modControlKeys,
                ModDescription = modDescription
            });
        }

        /// <summary>
        /// Refreshes pagination after adding or removing help items
        /// </summary>
        public static void RefreshPagination()
        {
            CalculateTotalPages();
            // Ensure current page is still valid
            if (_currentPage > _totalPages)
            {
                _currentPage = _totalPages;
            }
        }

        private static Dictionary<string, string[]> ParseKeybindings(char separator)
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

        private static string DrawHelpTextColouring(string[] textArray)
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
        /// Gets the current help items per page configuration
        /// </summary>
        public static int GetHelpItemsPerPage()
        {
            return _helpItemsPerPage;
        }

        /// <summary>
        /// Updates help items and refreshes pagination
        /// </summary>
        public static void RefreshHelpItems()
        {
            InitializeHelpItems();
            CalculateTotalPages();
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
