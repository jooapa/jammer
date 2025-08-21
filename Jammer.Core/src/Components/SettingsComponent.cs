using Spectre.Console;

namespace Jammer.Components
{
    /// <summary>
    /// Represents a single setting item
    /// </summary>
    public class SettingItem
    {
        public string Name { get; set; } = "";
        public string CurrentValue { get; set; } = "";
        public string ChangeKey { get; set; } = "";
        public string ChangeDescription { get; set; } = "";
    }

    /// <summary>
    /// Component responsible for rendering the settings menu
    /// Extracted from TUI.DrawSettings method
    /// </summary>
    public class SettingsComponent : IUIComponent
    {
        private static int _currentPage = 1;
        private static int _totalPages = 1;
        private static readonly int _settingsPerPage = 6; // Configurable page size
        private static List<SettingItem> _settings = new List<SettingItem>();

        static SettingsComponent()
        {
            InitializeSettings();
            CalculateTotalPages();
        }

        public Table Render(LayoutConfig layout)
        {
            var table = new Table();
            table.Border = Themes.bStyle(Themes.CurrentTheme.GeneralSettings.BorderStyle);
            table.BorderColor(Themes.bColor(Themes.CurrentTheme.GeneralSettings.BorderColor));
            table.Width = layout.ConsoleWidth;

            BuildSettingsContent(table);
            return table;
        }

        private void BuildSettingsContent(Table table)
        {
            // Add table headers
            table.AddColumns(
                Themes.sColor(Locale.Settings._Settings, Themes.CurrentTheme.GeneralSettings.SettingTextColor),
                Themes.sColor(Locale.Settings.Value, Themes.CurrentTheme.GeneralSettings.HeaderTextColor),
                Themes.sColor(Locale.Settings.ChangeValue + $" ({_currentPage}/{_totalPages})", Themes.CurrentTheme.GeneralSettings.HeaderTextColor)
            );

            // Calculate which settings to show on current page
            var startIndex = (_currentPage - 1) * _settingsPerPage;
            var endIndex = Math.Min(startIndex + _settingsPerPage, _settings.Count);

            // Add settings for current page
            for (int i = startIndex; i < endIndex; i++)
            {
                var setting = _settings[i];
                AddSettingRow(table, setting.Name, setting.CurrentValue, setting.ChangeKey, setting.ChangeDescription);
            }

            // Add navigation hints
            AddNavigationHints(table);
        }

        private static void InitializeSettings()
        {
            _settings.Clear();

            // Core playback settings
            _settings.Add(new SettingItem
            {
                Name = Locale.Settings.Forwardseconds,
                CurrentValue = $"{Preferences.forwardSeconds} sec",
                ChangeKey = Keybindings.SettingsKeys.ForwardSecondAmount.ToString(),
                ChangeDescription = Locale.Settings.ToChange
            });

            _settings.Add(new SettingItem
            {
                Name = Locale.Settings.Rewindseconds,
                CurrentValue = $"{Preferences.rewindSeconds} sec",
                ChangeKey = Keybindings.SettingsKeys.BackwardSecondAmount.ToString(),
                ChangeDescription = Locale.Settings.ToChange
            });

            _settings.Add(new SettingItem
            {
                Name = Locale.Settings.ChangeVolumeBy,
                CurrentValue = $"{Preferences.changeVolumeBy * 100} %",
                ChangeKey = Keybindings.SettingsKeys.ChangeVolumeAmount.ToString(),
                ChangeDescription = Locale.Settings.ToChange
            });

            _settings.Add(new SettingItem
            {
                Name = Locale.Settings.AutoSave,
                CurrentValue = Preferences.isAutoSave ? Locale.Miscellaneous.True : Locale.Miscellaneous.False,
                ChangeKey = Keybindings.SettingsKeys.Autosave.ToString(),
                ChangeDescription = Locale.Settings.ToToggle
            });

            // Advanced settings
            _settings.Add(new SettingItem
            {
                Name = "Load Effects",
                CurrentValue = "",
                ChangeKey = Keybindings.SettingsKeys.LoadEffects.ToString(),
                ChangeDescription = "To Load Effects settings"
            });

            _settings.Add(new SettingItem
            {
                Name = "Toggle Media Buttons",
                CurrentValue = Preferences.isMediaButtons ? Locale.Miscellaneous.True : Locale.Miscellaneous.False,
                ChangeKey = Keybindings.SettingsKeys.ToggleMediaButtons.ToString(),
                ChangeDescription = "To Toggle Media Buttons"
            });

            _settings.Add(new SettingItem
            {
                Name = "Toggle Visualizer",
                CurrentValue = Preferences.isVisualizer ? Locale.Miscellaneous.True : Locale.Miscellaneous.False,
                ChangeKey = Keybindings.SettingsKeys.ToggleVisualizer.ToString(),
                ChangeDescription = "To Toggle Visualizer"
            });

            _settings.Add(new SettingItem
            {
                Name = "Load Visualizer",
                CurrentValue = "",
                ChangeKey = Keybindings.SettingsKeys.LoadVisualizer.ToString(),
                ChangeDescription = "To Load Visualizer settings"
            });

            _settings.Add(new SettingItem
            {
                Name = "Set Soundcloud Client ID",
                CurrentValue = "",
                ChangeKey = Keybindings.SettingsKeys.SoundCloudClientID.ToString(),
                ChangeDescription = "To Set Soundcloud Client ID"
            });

            _settings.Add(new SettingItem
            {
                Name = "Fetch Client ID",
                CurrentValue = "",
                ChangeKey = Keybindings.SettingsKeys.FetchClientID.ToString(),
                ChangeDescription = "To Fetch and set Soundcloud Client ID"
            });

            _settings.Add(new SettingItem
            {
                Name = "Toggle Key Modifier Helpers",
                CurrentValue = Preferences.isModifierKeyHelper ? Locale.Miscellaneous.True : Locale.Miscellaneous.False,
                ChangeKey = Keybindings.SettingsKeys.KeyModifierHelper.ToString(),
                ChangeDescription = "To Toggle Key Helpers (restart required)"
            });

            _settings.Add(new SettingItem
            {
                Name = "Toggle Skip Errors",
                CurrentValue = Preferences.isSkipErrors ? Locale.Miscellaneous.True : Locale.Miscellaneous.False,
                ChangeKey = Keybindings.SettingsKeys.SkipErrors.ToString(),
                ChangeDescription = "To Toggle Skip Errors"
            });

            _settings.Add(new SettingItem
            {
                Name = "Toggle Playlist Position",
                CurrentValue = Preferences.showPlaylistPosition ? Locale.Miscellaneous.True : Locale.Miscellaneous.False,
                ChangeKey = Keybindings.SettingsKeys.TogglePlaylistPosition.ToString(),
                ChangeDescription = "To Toggle Playlist Position"
            });

            _settings.Add(new SettingItem
            {
                Name = "Skip Rss after some time",
                CurrentValue = Preferences.rssSkipAfterTime.ToString(),
                ChangeKey = Keybindings.SettingsKeys.rssSkipAfterTime.ToString(),
                ChangeDescription = "To Toggle Skip Rss after some time"
            });

            _settings.Add(new SettingItem
            {
                Name = "Amount of time to skip Rss",
                CurrentValue = Preferences.rssSkipAfterTimeValue.ToString(),
                ChangeKey = Keybindings.SettingsKeys.rssSkipAfterTimeValue.ToString(),
                ChangeDescription = "To Set Amount of time to skip Rss"
            });
        }

        private static void CalculateTotalPages()
        {
            _totalPages = Math.Max(1, (int)Math.Ceiling((double)_settings.Count / _settingsPerPage));
        }

        private void AddNavigationHints(Table table)
        {
            // Add empty row for spacing
            table.AddRow("", "", "");

            if (_totalPages > 1)
            {
                if (_currentPage < _totalPages)
                {
                    table.AddRow(
                        "",
                        "",
                        Themes.sColor("PgDn/→", Themes.CurrentTheme.GeneralSettings.HeaderTextColor) + " " +
                        Themes.sColor("Next page", Themes.CurrentTheme.GeneralSettings.SettingChangeValueColor)
                    );
                }

                if (_currentPage > 1)
                {
                    table.AddRow(
                        Themes.sColor("PgUp/←", Themes.CurrentTheme.GeneralSettings.HeaderTextColor) + " " +
                        Themes.sColor("Previous page", Themes.CurrentTheme.GeneralSettings.SettingChangeValueColor),
                        "",
                        ""
                    );
                }
            }
        }

        /// <summary>
        /// Adds a new setting to the settings list. Call RefreshPagination() after adding settings.
        /// </summary>
        public static void AddSetting(string name, string currentValue, string changeKey, string changeDescription)
        {
            _settings.Add(new SettingItem
            {
                Name = name,
                CurrentValue = currentValue,
                ChangeKey = changeKey,
                ChangeDescription = changeDescription
            });
        }

        /// <summary>
        /// Refreshes pagination after adding or removing settings
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

        private void AddSettingRow(Table table, string settingName, string currentValue, string changeKey, string changeDescription)
        {
            table.AddRow(
                Themes.sColor(settingName, Themes.CurrentTheme.GeneralSettings.SettingTextColor),
                Themes.sColor(currentValue, Themes.CurrentTheme.GeneralSettings.SettingValueColor),
                Themes.sColor($"{changeKey} ", Themes.CurrentTheme.GeneralSettings.SettingChangeValueValueColor) +
                Themes.sColor(changeDescription, Themes.CurrentTheme.GeneralSettings.SettingChangeValueColor)
            );
        }

        public Table CreateNavigationTable()
        {
            var table = new Table();
            table.Border = Themes.bStyle(Themes.CurrentTheme.GeneralSettings.BorderStyle);
            table.BorderColor(Themes.bColor(Themes.CurrentTheme.GeneralSettings.BorderColor));

            table.AddColumn(
                Locale.Help.ToMainMenu + ": " +
                Themes.sColor(Keybindings.ToMainMenu, Themes.CurrentTheme.GeneralSettings.SettingChangeValueValueColor)
            );

            return table;
        }

        public static void NextSettingsPage()
        {
            AnsiConsole.Clear();
            _currentPage = (_currentPage % _totalPages) + 1;
        }

        public static void PreviousSettingsPage()
        {
            AnsiConsole.Clear();
            _currentPage = _currentPage > 1 ? _currentPage - 1 : _totalPages;
        }

        public static void SetSettingsPage(int page)
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
        /// Gets the current settings per page configuration
        /// </summary>
        public static int GetSettingsPerPage()
        {
            return _settingsPerPage;
        }

        /// <summary>
        /// Updates settings values and refreshes pagination
        /// </summary>
        public static void RefreshSettings()
        {
            InitializeSettings();
            CalculateTotalPages();
        }

        /// <summary>
        /// Renders the complete settings screen to console
        /// </summary>
        /// <param name="layout">Layout configuration</param>
        public static void DrawSettingsToConsole(LayoutConfig layout)
        {
            var component = new SettingsComponent();
            InitializeSettings(); // reload correct settings. might have been changed

            // Render main settings table
            var settingsTable = component.Render(layout);
            AnsiConsole.Cursor.SetPosition(0, 0);
            AnsiConsole.Write(settingsTable);

            // Render navigation table
            var navigationTable = component.CreateNavigationTable();
            AnsiConsole.Write(navigationTable);
        }
    }
}