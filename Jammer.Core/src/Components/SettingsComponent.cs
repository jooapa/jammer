using Spectre.Console;
using System.Text.RegularExpressions;

namespace Jammer.Components
{
    public sealed record SettingDescriptor(
        Func<string> Name,
        Func<string> Value,
        Func<Task> Activate,
        Func<string> Description);

    public sealed record SettingsCategoryDescriptor(
        Func<string> Name,
        Func<string> Description,
        Func<IReadOnlyList<SettingDescriptor>> Settings);

    /// <summary>Category-based, descriptor-driven settings UI and input handler.</summary>
    public sealed class SettingsComponent : IUIComponent
    {
        private static readonly Regex ClientIdRegex = new("^[A-Za-z0-9]{32}$", RegexOptions.Compiled);
        private static readonly YtDlpManager YtDlp = new();
        private static readonly SpotifyIntegrationService Spotify = new();
        private static SettingsCategoryDescriptor? _currentCategory;
        private static int _currentPage;
        private static int _selectedIndex;
        private static int _selectedCategoryIndex;
        private static int _pageSize = 6;
        private static YtDlpResolution _ytDlpStatus = new(null, "missing", null);
        private static bool _ytDlpStatusChecked;

        public static void Open()
        {
            _currentCategory = null;
            _currentPage = 0;
            _selectedIndex = 0;
            _selectedCategoryIndex = 0;
            Redraw();
        }

        public static void Redraw()
        {
            if (Start.playerView != "settings") return;
            AnsiConsole.Clear();
            TUI.RefreshCurrentView();
        }

        public Table Render(LayoutConfig layout)
        {
            _pageSize = Math.Max(2, Math.Min(8, layout.ConsoleHeight - 8));
            IReadOnlyList<SettingsCategoryDescriptor> categories = BuildCategories();
            var table = new Table
            {
                Border = Themes.bStyle(Themes.CurrentTheme.GeneralSettings.BorderStyle),
                Width = layout.ConsoleWidth
            };
            table.BorderColor(Themes.bColor(Themes.CurrentTheme.GeneralSettings.BorderColor));

            if (_currentCategory == null)
            {
                table.AddColumns(
                    Themes.sColor(Locale.Settings.Category, Themes.CurrentTheme.GeneralSettings.SettingTextColor),
                    Themes.sColor(Locale.Settings.Description, Themes.CurrentTheme.GeneralSettings.HeaderTextColor),
                    Themes.sColor(Locale.Settings.Open, Themes.CurrentTheme.GeneralSettings.HeaderTextColor));
                AddRows(table, categories.Count, index => (
                    categories[index].Name(),
                    categories[index].Description(),
                    Locale.Settings.ToOpen));
            }
            else
            {
                IReadOnlyList<SettingDescriptor> settings = _currentCategory.Settings();
                table.AddColumns(
                    Themes.sColor(_currentCategory.Name(), Themes.CurrentTheme.GeneralSettings.SettingTextColor),
                    Themes.sColor(Locale.Settings.Value, Themes.CurrentTheme.GeneralSettings.HeaderTextColor),
                    Themes.sColor(Locale.Settings.ChangeValue, Themes.CurrentTheme.GeneralSettings.HeaderTextColor));
                AddRows(table, settings.Count, index => (
                    settings[index].Name(),
                    settings[index].Value(),
                    settings[index].Description()));
            }

            return table;
        }

        public static async Task<bool> HandleKeyAsync(ConsoleKeyInfo key, bool backRequested)
        {
            if (backRequested || key.Key == ConsoleKey.Escape)
            {
                if (_currentCategory != null)
                {
                    _currentCategory = null;
                    _selectedIndex = _selectedCategoryIndex;
                    _currentPage = _selectedIndex / _pageSize;
                    return false;
                }
                return true;
            }

            int count = _currentCategory == null
                ? BuildCategories().Count
                : _currentCategory.Settings().Count;
            int totalPages = Math.Max(1, (int)Math.Ceiling((double)count / _pageSize));
            if (key.Key == ConsoleKey.DownArrow)
            {
                MoveSelection(1, count);
                return false;
            }
            if (key.Key == ConsoleKey.UpArrow)
            {
                MoveSelection(-1, count);
                return false;
            }
            if (key.Key is ConsoleKey.PageDown or ConsoleKey.RightArrow)
            {
                MovePage(1, count, totalPages);
                return false;
            }
            if (key.Key is ConsoleKey.PageUp or ConsoleKey.LeftArrow)
            {
                MovePage(-1, count, totalPages);
                return false;
            }
            if (key.Key is not (ConsoleKey.Enter or ConsoleKey.Spacebar))
            {
                return false;
            }

            _selectedIndex = Math.Clamp(_selectedIndex, 0, Math.Max(0, count - 1));

            if (_currentCategory == null)
            {
                IReadOnlyList<SettingsCategoryDescriptor> categories = BuildCategories();
                _selectedCategoryIndex = _selectedIndex;
                _currentCategory = categories[_selectedIndex];
                _currentPage = 0;
                _selectedIndex = 0;
                if (_currentCategory.Name() == Locale.Settings.Integrations)
                {
                    await RefreshYtDlpStatusAsync();
                }
            }
            else
            {
                SettingDescriptor setting = _currentCategory.Settings()[_selectedIndex];
                try
                {
                    await setting.Activate();
                }
                catch (OperationCanceledException)
                {
                    Log.Info(Locale.Settings.OperationCancelled);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                    Message.Data(Locale.Settings.OperationFailed, Locale.Settings.IntegrationError, true, false);
                }
            }

            return false;
        }

        private static IReadOnlyList<SettingsCategoryDescriptor> BuildCategories()
        {
            return new SettingsCategoryDescriptor[]
            {
                new(() => Locale.Settings.Playback, () => Locale.Settings.PlaybackDescription, BuildPlaybackSettings),
                new(() => Locale.Settings.Interface, () => Locale.Settings.InterfaceDescription, BuildInterfaceSettings),
                new(() => Locale.Settings.LibraryFeeds, () => Locale.Settings.LibraryFeedsDescription, BuildLibrarySettings),
                new(() => Locale.Settings.Integrations, () => Locale.Settings.IntegrationsDescription, BuildIntegrationSettings),
                new(() => Locale.Settings.Advanced, () => Locale.Settings.AdvancedDescription, BuildAdvancedSettings)
            };
        }

        private static void MoveSelection(int direction, int count)
        {
            if (count <= 0) return;
            _selectedIndex = (_selectedIndex + direction + count) % count;
            _currentPage = _selectedIndex / _pageSize;
        }

        private static void MovePage(int direction, int count, int totalPages)
        {
            if (count <= 0) return;
            int offsetWithinPage = _selectedIndex % _pageSize;
            _currentPage = (_currentPage + direction + totalPages) % totalPages;
            _selectedIndex = Math.Min((_currentPage * _pageSize) + offsetWithinPage, count - 1);
        }

        private static IReadOnlyList<SettingDescriptor> BuildPlaybackSettings() =>
            new SettingDescriptor[]
            {
                IntegerSetting(() => Locale.Settings.Forwardseconds, () => Preferences.forwardSeconds, value => Preferences.forwardSeconds = value, () => Locale.OutsideItems.EnterForwardSeconds, () => " " + Locale.Help.Seconds),
                IntegerSetting(() => Locale.Settings.Rewindseconds, () => Preferences.rewindSeconds, value => Preferences.rewindSeconds = value, () => Locale.OutsideItems.EnterBackwardSeconds, () => " " + Locale.Help.Seconds),
                new(() => Locale.Settings.ChangeVolumeBy, () => $"{Preferences.changeVolumeBy * 100:0} %", async () =>
                {
                    string input = Message.Input("", Locale.OutsideItems.EnterVolumeChange);
                    if (int.TryParse(input, out int value) && value > 0)
                    {
                        Preferences.changeVolumeBy = value / 100f;
                        Preferences.SaveSettings();
                    }
                    else ShowInvalidInput();
                    await Task.CompletedTask;
                }, () => Locale.Settings.ToChange),
                ToggleSetting(() => Locale.Settings.AutoSave, () => Preferences.isAutoSave, value => Preferences.isAutoSave = value)
            };

        private static IReadOnlyList<SettingDescriptor> BuildInterfaceSettings()
        {
            var settings = new List<SettingDescriptor>
            {
                ToggleSetting(() => Locale.Settings.Visualizer, () => Preferences.isVisualizer, value => Preferences.isVisualizer = value),
                ActionSetting(() => Locale.Settings.ReloadVisualizer, () => Visual.Read()),
                ToggleSetting(() => Locale.Settings.PlaylistPosition, () => Preferences.showPlaylistPosition, value => Preferences.showPlaylistPosition = value),
                ToggleSetting(() => Locale.Settings.FavoriteExplainer, () => Preferences.favoriteExplainer, value => Preferences.favoriteExplainer = value)
            };
            if (!OperatingSystem.IsMacOS())
            {
                settings.Insert(0, ToggleSetting(
                    () => Locale.Settings.MediaButtons,
                    () => Preferences.isMediaButtons,
                    value => Preferences.isMediaButtons = value));
            }
            return settings;
        }

        private static IReadOnlyList<SettingDescriptor> BuildLibrarySettings() =>
            new SettingDescriptor[]
            {
                ToggleSetting(() => Locale.Settings.RssSkip, () => Preferences.rssSkipAfterTime, value => Preferences.rssSkipAfterTime = value),
                IntegerSetting(() => Locale.Settings.RssSkipSeconds, () => Preferences.rssSkipAfterTimeValue, value => Preferences.rssSkipAfterTimeValue = value, () => Locale.Settings.EnterRssSkipSeconds, () => " " + Locale.Help.Seconds),
                ToggleSetting(() => Locale.Settings.QuickSearch, () => Preferences.isQuickSearch, value => Preferences.isQuickSearch = value),
                ToggleSetting(() => Locale.Settings.QuickPlaySearch, () => Preferences.isQuickPlayFromSearch, value => Preferences.isQuickPlayFromSearch = value)
            };

        private static IReadOnlyList<SettingDescriptor> BuildAdvancedSettings() =>
            new SettingDescriptor[]
            {
                new(() => Locale.Settings.ReloadEffects, () => "", async () =>
                {
                    Effects.ReadEffects();
                    if (Utils.Songs.Length > 0) Play.SetEffectsToChannel();
                    await Task.CompletedTask;
                }, () => Locale.Settings.ToRun),
                ToggleSetting(() => Locale.Settings.ModifierHelpers, () => Preferences.isModifierKeyHelper, value => Preferences.isModifierKeyHelper = value),
                ToggleSetting(() => Locale.Settings.SkipErrors, () => Preferences.isSkipErrors, value => Preferences.isSkipErrors = value)
            };

        private static IReadOnlyList<SettingDescriptor> BuildIntegrationSettings() =>
            new SettingDescriptor[]
            {
                new(() => Locale.Settings.YouTubeBackend, BackendValue, SelectBackendAsync, () => Locale.Settings.ToSelect),
                new(() => Locale.Settings.YtDlpStatus, YtDlpStatusValue, RefreshYtDlpStatusAsync, () => Locale.Settings.ToRefresh),
                new(() => Locale.Settings.InstallRepairYtDlp, () => "", () => InstallYtDlpAsync(false), () => Locale.Settings.ToRun),
                new(() => Locale.Settings.UpdateYtDlp, () => "", () => InstallYtDlpAsync(true), () => Locale.Settings.ToRun),
                new(() => Locale.Settings.SoundCloudStatus, SoundCloudStatusValue, () => Task.CompletedTask, () => Locale.Settings.StatusOnly),
                new(() => Locale.Settings.ManualSoundCloudId, () => "", SetSoundCloudIdAsync, () => Locale.Settings.ToChange),
                new(() => Locale.Settings.FetchSoundCloudId, () => "", FetchSoundCloudIdAsync, () => Locale.Settings.ToRun),
                new(() => Locale.Settings.ResetSoundCloudId, () => "", ResetSoundCloudIdAsync, () => Locale.Settings.ToRun),
                new(() => Locale.Settings.SpotifyStatus, SpotifyStatusValue, () => Task.CompletedTask, () => Locale.Settings.StatusOnly),
                new(() => Locale.Settings.SpotifyClientId, SpotifyClientIdValue, SetSpotifyClientIdAsync, () => Locale.Settings.ToChange),
                new(() => Locale.Settings.SpotifyAuthorize, () => "", AuthorizeSpotifyAsync, () => Locale.Settings.ToRun),
                new(() => Locale.Settings.SpotifyPlaylists, () => SpotifyIntegrationService.LoadRegistry().Playlists.Count.ToString(), ManageSpotifyPlaylistsAsync, () => Locale.Settings.ToOpen),
                new(() => Locale.Settings.SpotifyResolver, () => Preferences.spotifyResolutionProvider.ToString(), SelectSpotifyResolverAsync, () => Locale.Settings.ToSelect),
                new(() => Locale.Settings.SpotifyDisconnect, () => "", DisconnectSpotifyAsync, () => Locale.Settings.ToRun)
            };

        private static SettingDescriptor ToggleSetting(Func<string> name, Func<bool> get, Action<bool> set) =>
            new(name,
                () => get() ? Locale.Miscellaneous.True : Locale.Miscellaneous.False,
                () =>
                {
                    set(!get());
                    Preferences.SaveSettings();
                    return Task.CompletedTask;
                },
                () => Locale.Settings.ToToggle);

        private static SettingDescriptor IntegerSetting(Func<string> name, Func<int> get, Action<int> set, Func<string> prompt, Func<string> suffix) =>
            new(name, () => get() + suffix(), async () =>
            {
                string input = Message.Input("", prompt());
                if (int.TryParse(input, out int value) && value >= 0)
                {
                    set(value);
                    Preferences.SaveSettings();
                }
                else ShowInvalidInput();
                await Task.CompletedTask;
            }, () => Locale.Settings.ToChange);

        private static SettingDescriptor ActionSetting(Func<string> name, Action action) =>
            new(name, () => "", () => { action(); return Task.CompletedTask; }, () => Locale.Settings.ToRun);

        private static async Task SelectBackendAsync()
        {
            var options = new[]
            {
                new CustomSelectInput { DataURI = "YoutubeExplode", Title = "YoutubeExplode", Author = "Tyrrrz", Description = Locale.Settings.YoutubeExplodeDescription },
                new CustomSelectInput { DataURI = "yt-dlp", Title = "yt-dlp", Author = "yt-dlp", Description = Locale.Settings.YtDlpDescription }
            };
            string choice = Message.CustomMenuSelect(options, Locale.Settings.SelectBackendPrompt);
            if (choice == "YoutubeExplode")
            {
                Preferences.backEndType = BackEndTypeYT.YoutubeExplode;
                Preferences.SaveSettings();
                return;
            }
            if (choice != "yt-dlp") return;

            await RefreshYtDlpStatusAsync();
            if (!_ytDlpStatus.IsAvailable)
            {
                string answer = Message.Input(
                    string.Format(Locale.Settings.YtDlpMissingPrompt, Locale.Miscellaneous.YesAnswer),
                    Locale.Settings.YtDlpMissingTitle).Trim();
                if (!answer.Equals(Locale.Miscellaneous.YesAnswer, StringComparison.OrdinalIgnoreCase)) return;
                await InstallYtDlpAsync(false);
                if (!_ytDlpStatus.IsAvailable) return;
            }
            Preferences.backEndType = BackEndTypeYT.YoutubeDL;
            Preferences.SaveSettings();
        }

        private static async Task RefreshYtDlpStatusAsync()
        {
            _ytDlpStatus = await YtDlp.ResolveAsync(false);
            _ytDlpStatusChecked = true;
        }

        private static async Task InstallYtDlpAsync(bool update)
        {
            string operation = update ? Locale.Settings.UpdatingYtDlp : Locale.Settings.InstallingYtDlp;
            var progress = new InlineProgress<double>(value => TUI.PrintToTopOfPlayer($"{operation} {value:P0}"));
            _ytDlpStatus = await YtDlp.InstallAsync(true, progress);
            _ytDlpStatusChecked = true;
            Message.Data(
                string.Format(Locale.Settings.YtDlpReadyMessage, _ytDlpStatus.Version),
                Locale.Settings.YtDlpReadyTitle,
                false,
                false);
        }

        private static async Task SetSoundCloudIdAsync()
        {
            string input = Message.Input(Locale.Settings.SoundCloudIdPrompt, Locale.Settings.SoundCloudIdTitle).Trim();
            if (!ClientIdRegex.IsMatch(input))
            {
                Message.Data(Locale.Settings.InvalidSoundCloudId, Locale.OutsideItems.InvalidInput, true, false);
                return;
            }
            Preferences.clientID = input;
            Utils.SCClientIdAlreadyLookedAndItsIncorrect = false;
            Preferences.SaveSettings();
            await Task.CompletedTask;
        }

        private static async Task FetchSoundCloudIdAsync()
        {
            TUI.PrintToTopOfPlayer(Locale.Settings.FetchingSoundCloudId);
            string clientId = await SCClientIdFetcher.GetClientId();
            Preferences.clientID = clientId;
            Utils.SCClientIdAlreadyLookedAndItsIncorrect = false;
            Preferences.SaveSettings();
        }

        private static Task ResetSoundCloudIdAsync()
        {
            Preferences.clientID = "";
            Utils.SCClientIdAlreadyLookedAndItsIncorrect = false;
            Preferences.SaveSettings();
            return Task.CompletedTask;
        }

        private static Task SetSpotifyClientIdAsync()
        {
            string input = Message.Input("", Locale.Settings.SpotifyClientIdPrompt).Trim();
            if (string.IsNullOrWhiteSpace(input)) return Task.CompletedTask;
            Preferences.spotifyClientID = input;
            Preferences.SaveSettings();
            return Task.CompletedTask;
        }

        private static async Task AuthorizeSpotifyAsync()
        {
            if (string.IsNullOrWhiteSpace(Preferences.spotifyClientID))
            {
                await SetSpotifyClientIdAsync();
                if (string.IsNullOrWhiteSpace(Preferences.spotifyClientID)) return;
            }
            TUI.PrintToTopOfPlayer(string.Format(Locale.Settings.SpotifyRedirectHelp, SpotifyIntegrationService.RedirectUri));
            var progress = new InlineProgress<string>(TUI.PrintToTopOfPlayer);
            string user = await Spotify.AuthorizeAsync(progress);
            Message.Data(
                string.Format(Locale.Settings.SpotifyAuthorizedMessage, user),
                Locale.Settings.SpotifyAuthorized,
                false,
                false);
        }

        private static async Task ManageSpotifyPlaylistsAsync()
        {
            if (!Spotify.IsAuthorized) throw new InvalidOperationException(Locale.Settings.SpotifyNotAuthorized);
            IReadOnlyList<SpotifyPlaylistSummary> playlists = await Spotify.GetPlaylistsAsync();
            var options = new List<CustomSelectInput>
            {
                new()
                {
                    DataURI = "__UPDATE_ALL__",
                    Title = Locale.Settings.SpotifyUpdateAll,
                    Author = SpotifyIntegrationService.LoadRegistry().Playlists.Count.ToString()
                }
            };
            options.AddRange(playlists.Select(playlist => new CustomSelectInput
            {
                DataURI = playlist.Id,
                Title = playlist.Name,
                Author = playlist.Owner,
                Description = playlist.IsImported ? Locale.Settings.SpotifyImported : Locale.Settings.SpotifyNotImported
            }));

            if (playlists.Count == 0)
            {
                Message.Data(Locale.Settings.SpotifyNoPlaylists, Locale.Settings.SpotifyPlaylists, false, false);
                return;
            }

            string? choice = Message.CustomMenuSelect(options.ToArray(), Locale.Settings.SpotifyPlaylistMenu);
            if (string.IsNullOrEmpty(choice)) return;
            var progress = new InlineProgress<string>(TUI.PrintToTopOfPlayer);
            if (choice == "__UPDATE_ALL__")
            {
                int count = await Spotify.UpdateAllAsync(progress);
                Message.Data(string.Format(Locale.Settings.SpotifyUpdateComplete, count), Locale.Settings.SpotifyPlaylists, false, false);
                return;
            }

            SpotifyPlaylistSummary? selected = playlists.FirstOrDefault(x => x.Id == choice);
            if (selected == null) return;
            SpotifyPlaylistImport imported = await Spotify.ImportOrUpdateAsync(selected, progress);
            Message.Data(
                string.Format(Locale.Settings.SpotifyImportComplete, imported.PlaylistPath),
                Locale.Settings.SpotifyPlaylists,
                false,
                false);
        }

        private static Task SelectSpotifyResolverAsync()
        {
            var options = new[]
            {
                new CustomSelectInput { DataURI = nameof(SpotifyResolutionProvider.YouTube), Title = "YouTube", Description = Locale.Settings.YoutubeExplodeDescription },
                new CustomSelectInput { DataURI = nameof(SpotifyResolutionProvider.SoundCloud), Title = "SoundCloud" }
            };
            string? choice = Message.CustomMenuSelect(options, Locale.Settings.SpotifyChooseResolver);
            if (Enum.TryParse(choice, out SpotifyResolutionProvider provider))
            {
                Preferences.spotifyResolutionProvider = provider;
                Preferences.SaveSettings();
            }
            return Task.CompletedTask;
        }

        private static Task DisconnectSpotifyAsync()
        {
            string answer = Message.Input(
                string.Format(Locale.Settings.SpotifyDisconnectPrompt, Locale.Miscellaneous.YesAnswer),
                Locale.Settings.SpotifyDisconnect).Trim();
            if (answer.Equals(Locale.Miscellaneous.YesAnswer, StringComparison.OrdinalIgnoreCase)) Spotify.Disconnect();
            return Task.CompletedTask;
        }

        private static string BackendValue() => Preferences.backEndType == BackEndTypeYT.YoutubeDL ? "yt-dlp" : "YoutubeExplode";

        private static string SpotifyStatusValue() => Spotify.IsAuthorized
            ? Locale.Settings.SpotifyAuthorized
            : Locale.Settings.SpotifyNotAuthorized;

        private static string SpotifyClientIdValue() => string.IsNullOrWhiteSpace(Preferences.spotifyClientID)
            ? Locale.Settings.NotInstalled
            : MaskClientId(Preferences.spotifyClientID);

        private static string YtDlpStatusValue()
        {
            if (!_ytDlpStatusChecked) return Locale.Settings.CheckingStatus;
            string source = _ytDlpStatus.Source switch
            {
                "managed" => Locale.Settings.ManagedToolSource,
                "PATH" => Locale.Settings.PathToolSource,
                "JAMMER_YTDLP_BIN" => Locale.Settings.OverrideToolSource,
                _ => _ytDlpStatus.Source
            };
            return _ytDlpStatus.IsAvailable
                ? string.Format(Locale.Settings.AvailableStatus, _ytDlpStatus.Version, source)
                : Locale.Settings.NotInstalled;
        }

        private static string SoundCloudStatusValue()
        {
            if (string.IsNullOrEmpty(Preferences.clientID))
            {
                string libraryId = new SoundCloudExplode.SoundCloudClient().ClientId;
                return string.Format(Locale.Settings.LibraryDefaultStatus, MaskClientId(libraryId));
            }
            return string.Format(Locale.Settings.CustomStatus, MaskClientId(Preferences.clientID));
        }

        private static string MaskClientId(string value) => value.Length < 8 ? value : value[..4] + "..." + value[^4..];

        private static void AddRows(Table table, int count, Func<int, (string Name, string Value, string Action)> rowFactory)
        {
            int totalPages = Math.Max(1, (int)Math.Ceiling((double)count / _pageSize));
            _selectedIndex = Math.Clamp(_selectedIndex, 0, Math.Max(0, count - 1));
            _currentPage = _selectedIndex / _pageSize;
            int start = _currentPage * _pageSize;
            int end = Math.Min(start + _pageSize, count);
            for (int index = start; index < end; index++)
            {
                var row = rowFactory(index);
                string escapedName = Markup.Escape(row.Name);
                string selectableName = index == _selectedIndex
                    ? $"> [b]{escapedName}[/] <"
                    : $"  {escapedName}";
                table.AddRow(
                    Themes.sColor(selectableName, Themes.CurrentTheme.GeneralSettings.SettingTextColor),
                    Themes.sColor(Markup.Escape(row.Value), Themes.CurrentTheme.GeneralSettings.SettingValueColor),
                    Themes.sColor(Markup.Escape(row.Action), Themes.CurrentTheme.GeneralSettings.SettingChangeValueColor));
            }
            table.AddEmptyRow();
            table.AddRow(
                Themes.sColor(Locale.Settings.BackHint, Themes.CurrentTheme.GeneralSettings.HeaderTextColor),
                totalPages > 1 ? string.Format(Locale.Settings.PageStatus, _currentPage + 1, totalPages) : "",
                Locale.UiMessages.SelectionInstructions);
        }

        private static void ShowInvalidInput() => Message.Data(
            $"[red]{Locale.OutsideItems.InvalidInput}.[/] {Locale.OutsideItems.PressToContinue}.",
            Locale.OutsideItems.InvalidInput);

        private sealed class InlineProgress<T> : IProgress<T>
        {
            private readonly Action<T> _report;

            public InlineProgress(Action<T> report) => _report = report;

            public void Report(T value) => _report(value);
        }

        public static void DrawSettingsToConsole(LayoutConfig layout)
        {
            var component = new SettingsComponent();
            AnsiConsole.Cursor.SetPosition(0, 0);
            AnsiConsole.Write(component.Render(layout));
        }
    }
}
