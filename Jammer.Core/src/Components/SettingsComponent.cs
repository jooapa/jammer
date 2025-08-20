using Spectre.Console;

namespace Jammer.Components
{
    /// <summary>
    /// Component responsible for rendering the settings menu
    /// Extracted from TUI.DrawSettings method
    /// </summary>
    public class SettingsComponent : IUIComponent
    {
        public Table Render(LayoutConfig layout)
        {
            var table = new Table();
            table.Border = Themes.bStyle(Themes.CurrentTheme?.GeneralSettings?.BorderStyle ?? "rounded");
            table.BorderColor(Themes.bColor(Themes.CurrentTheme?.GeneralSettings?.BorderColor ?? new int[] { 255, 255, 255 }));
            table.Width = layout.ConsoleWidth;

            BuildSettingsContent(table);
            return table;
        }

        private void BuildSettingsContent(Table table)
        {
            // Get keybinding strings for settings
            string ForwardSecondAmount = Keybindings.SettingsKeys.ForwardSecondAmount.ToString();
            string BackwardSecondAmount = Keybindings.SettingsKeys.BackwardSecondAmount.ToString();
            string ChangeVolumeAmount = Keybindings.SettingsKeys.ChangeVolumeAmount.ToString();
            string Autosave = Keybindings.SettingsKeys.Autosave.ToString();

            // Add table headers
            table.AddColumns(
                Themes.sColor(Locale.Settings._Settings, Themes.CurrentTheme?.GeneralSettings?.SettingTextColor ?? "white"),
                Themes.sColor(Locale.Settings.Value, Themes.CurrentTheme?.GeneralSettings?.HeaderTextColor ?? "white"),
                Themes.sColor(Locale.Settings.ChangeValue, Themes.CurrentTheme?.GeneralSettings?.HeaderTextColor ?? "white")
            );

            // Add settings rows
            AddSettingRow(table, Locale.Settings.Forwardseconds, $"{Preferences.forwardSeconds} sec", ForwardSecondAmount, Locale.Settings.ToChange);
            AddSettingRow(table, Locale.Settings.Rewindseconds, $"{Preferences.rewindSeconds} sec", BackwardSecondAmount, Locale.Settings.ToChange);
            AddSettingRow(table, Locale.Settings.ChangeVolumeBy, $"{Preferences.changeVolumeBy * 100} %", ChangeVolumeAmount, Locale.Settings.ToChange);
            AddSettingRow(table, Locale.Settings.AutoSave, Preferences.isAutoSave ? Locale.Miscellaneous.True : Locale.Miscellaneous.False, Autosave, Locale.Settings.ToToggle);

            // Add additional settings
            AddSettingRow(table, "Load Effects", "", Keybindings.SettingsKeys.LoadEffects.ToString(), "To Load Effects settings");
            AddSettingRow(table, "Toggle Media Buttons", Preferences.isMediaButtons ? Locale.Miscellaneous.True : Locale.Miscellaneous.False, Keybindings.SettingsKeys.ToggleMediaButtons.ToString(), "To Toggle Media Buttons");
            AddSettingRow(table, "Toggle Visualizer", Preferences.isVisualizer ? Locale.Miscellaneous.True : Locale.Miscellaneous.False, Keybindings.SettingsKeys.ToggleVisualizer.ToString(), "To Toggle Visualizer");
            AddSettingRow(table, "Load Visualizer", "", Keybindings.SettingsKeys.LoadVisualizer.ToString(), "To Load Visualizer settings");
            AddSettingRow(table, "Set Soundcloud Client ID", "", Keybindings.SettingsKeys.SoundCloudClientID.ToString(), "To Set Soundcloud Client ID");
            AddSettingRow(table, "Fetch Client ID", "", Keybindings.SettingsKeys.FetchClientID.ToString(), "To Fetch and set Soundcloud Client ID");
            AddSettingRow(table, "Toggle Key Mofifier Helpers", Preferences.isModifierKeyHelper ? Locale.Miscellaneous.True : Locale.Miscellaneous.False, Keybindings.SettingsKeys.KeyModifierHelper.ToString(), "To Toggle Key Helpers (restart required)");
            AddSettingRow(table, "Toggle Skip Errors", Preferences.isSkipErrors ? Locale.Miscellaneous.True : Locale.Miscellaneous.False, Keybindings.SettingsKeys.SkipErrors.ToString(), "To Toggle Skip Errors");
            AddSettingRow(table, "Toggle Playlist Position", Preferences.showPlaylistPosition ? Locale.Miscellaneous.True : Locale.Miscellaneous.False, Keybindings.SettingsKeys.TogglePlaylistPosition.ToString(), "To Toggle Playlist Position");
        }

        private void AddSettingRow(Table table, string settingName, string currentValue, string changeKey, string changeDescription)
        {
            table.AddRow(
                Themes.sColor(settingName, Themes.CurrentTheme?.GeneralSettings?.SettingTextColor ?? "white"),
                Themes.sColor(currentValue, Themes.CurrentTheme?.GeneralSettings?.SettingValueColor ?? "white"),
                Themes.sColor($"{changeKey} ", Themes.CurrentTheme?.GeneralSettings?.SettingChangeValueValueColor ?? "white") +
                Themes.sColor(changeDescription, Themes.CurrentTheme?.GeneralSettings?.SettingChangeValueColor ?? "white")
            );
        }

        public Table CreateNavigationTable()
        {
            var table = new Table();
            table.Border = Themes.bStyle(Themes.CurrentTheme?.GeneralSettings?.BorderStyle ?? "rounded");
            table.BorderColor(Themes.bColor(Themes.CurrentTheme?.GeneralSettings?.BorderColor ?? new int[] { 255, 255, 255 }));

            table.AddColumn(
                Locale.Help.ToMainMenu + ": " +
                Themes.sColor(Keybindings.ToMainMenu, Themes.CurrentTheme?.GeneralSettings?.SettingChangeValueValueColor ?? "white")
            );

            return table;
        }

        /// <summary>
        /// Renders the complete settings screen to console
        /// </summary>
        /// <param name="layout">Layout configuration</param>
        public static void DrawSettingsToConsole(LayoutConfig layout)
        {
            var component = new SettingsComponent();
            
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