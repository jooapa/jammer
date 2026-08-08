using Spectre.Console;

namespace Jammer.Components
{
    /// <summary>
    /// Popup editor for per-song playback metadata: speed, reverse, trim, and inline effects.
    /// Opened with the EditSongMetadata keybind (default: Shift + M).
    /// </summary>
    public sealed class SongMetadataComponent : IUIComponent
    {
        private static bool _editingEffects;
        private static int _selectedIndex;
        private static int _pageSize = 8;
        private static int _currentPage;

        private static SongPlaybackMetadata _workingCopy = new();
        private static string _songUri = "";

        public static void Open()
        {
            _editingEffects = false;
            _selectedIndex = 0;
            _currentPage = 0;
            string songEntry = Utils.Songs[Utils.CurrentSongIndex];
            _songUri = songEntry;
            if (songEntry.Contains(Utils.JammerFileDelimeter))
            {
                _songUri = songEntry.Split(Utils.JammerFileDelimeter)[0];
            }
            _workingCopy = SongMetadataStore.Get(_songUri) ?? new SongPlaybackMetadata();
            Redraw();
        }

        public static void Redraw()
        {
            if (Start.playerView != "songmetadata") return;
            AnsiConsole.Clear();
            TUI.RefreshCurrentView();
        }

        public static void DrawSongMetadataToConsole(LayoutConfig layout)
        {
            var component = new SongMetadataComponent();
            AnsiConsole.Cursor.SetPosition(0, 0);
            AnsiConsole.Write(component.Render(layout));
        }

        public Table Render(LayoutConfig layout)
        {
            _pageSize = Math.Max(4, Math.Min(12, layout.ConsoleHeight - 6));
            var table = new Table
            {
                Border = Themes.bStyle(Themes.CurrentTheme.GeneralSettings.BorderStyle),
                Width = layout.ConsoleWidth
            };
            table.BorderColor(Themes.bColor(Themes.CurrentTheme.GeneralSettings.BorderColor));

            string title = _editingEffects
                ? Locale.SongMetadata.EffectsTitle
                : Locale.SongMetadata.Title;

            table.AddColumns(
                Themes.sColor(title, Themes.CurrentTheme.GeneralSettings.SettingTextColor),
                Themes.sColor(Locale.Settings.Value, Themes.CurrentTheme.GeneralSettings.HeaderTextColor),
                Themes.sColor(Locale.Settings.ChangeValue, Themes.CurrentTheme.GeneralSettings.HeaderTextColor));

            IReadOnlyList<SettingDescriptor> settings = _editingEffects
                ? BuildEffectSettings()
                : BuildMainSettings();

            AddRows(table, settings.Count, index => (
                settings[index].Name(),
                settings[index].Value(),
                settings[index].Description()));

            return table;
        }

        public static async Task<bool> HandleKeyAsync(ConsoleKeyInfo key, bool backRequested)
        {
            if (backRequested || key.Key == ConsoleKey.Escape)
            {
                if (_editingEffects)
                {
                    _editingEffects = false;
                    _selectedIndex = 0;
                    _currentPage = 0;
                    return false;
                }

                SaveAndApply();
                return true;
            }

            IReadOnlyList<SettingDescriptor> settings = _editingEffects
                ? BuildEffectSettings()
                : BuildMainSettings();

            int count = settings.Count;
            if (count == 0)
            {
                return false;
            }

            _selectedIndex = Math.Clamp(_selectedIndex, 0, count - 1);
            _currentPage = _selectedIndex / _pageSize;

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
            SettingDescriptor setting = settings[_selectedIndex];
            try
            {
                await setting.Activate();
            }
            catch (OperationCanceledException)
            {
                // User cancelled input; do nothing.
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                Message.Data(Locale.Settings.OperationFailed, Locale.OutsideItems.Error, true, false);
            }

            return false;
        }

        private static IReadOnlyList<SettingDescriptor> BuildMainSettings()
        {
            var settings = new List<SettingDescriptor>
            {
                new(
                    () => Locale.SongMetadata.Speed,
                    () => $"{_workingCopy.Speed:0.00}x",
                    EditSpeedAsync,
                    () => Locale.Settings.ToChange),
                new(
                    () => Locale.SongMetadata.Pitch,
                    () => $"{_workingCopy.Pitch:0.0}",
                    EditPitchAsync,
                    () => Locale.Settings.ToChange),
                new(
                    () => Locale.SongMetadata.Reversed,
                    () => _workingCopy.Reversed ? Locale.Miscellaneous.True : Locale.Miscellaneous.False,
                    () => { _workingCopy.Reversed = !_workingCopy.Reversed; return Task.CompletedTask; },
                    () => Locale.Settings.ToToggle),
                new(
                    () => Locale.SongMetadata.TrimStart,
                    () => string.IsNullOrWhiteSpace(_workingCopy.TrimStart) ? Locale.Miscellaneous.None : _workingCopy.TrimStart,
                    EditTrimStartAsync,
                    () => Locale.Settings.ToChange),
                new(
                    () => Locale.SongMetadata.TrimEnd,
                    () => string.IsNullOrWhiteSpace(_workingCopy.TrimEnd) ? Locale.Miscellaneous.None : _workingCopy.TrimEnd,
                    EditTrimEndAsync,
                    () => Locale.Settings.ToChange),
                new(
                    () => Locale.SongMetadata.CustomEffects,
                    () => _workingCopy.UseCustomEffects ? Locale.Miscellaneous.True : Locale.Miscellaneous.False,
                    () => { _workingCopy.UseCustomEffects = !_workingCopy.UseCustomEffects; return Task.CompletedTask; },
                    () => Locale.Settings.ToToggle)
            };

            if (_workingCopy.UseCustomEffects)
            {
                settings.Add(new(
                    () => Locale.SongMetadata.EditEffects,
                    () => "",
                    () => { _editingEffects = true; _selectedIndex = 0; _currentPage = 0; return Task.CompletedTask; },
                    () => Locale.Settings.ToOpen));
            }

            return settings;
        }

        private static IReadOnlyList<SettingDescriptor> BuildEffectSettings()
        {
            var fx = _workingCopy.Effects;
            return new SettingDescriptor[]
            {
                ToggleSetting(() => Locale.Effects.Chorus, () => fx.IsChorus, value => fx.IsChorus = value),
                FloatSetting(() => Locale.Effects.ChorusFrequency, () => fx.ChorusFrequency, value => fx.ChorusFrequency = value),
                FloatSetting(() => Locale.Effects.ChorusWetDryMix, () => fx.ChorusWetDryMix, value => fx.ChorusWetDryMix = value),
                FloatSetting(() => Locale.Effects.ChorusDepth, () => fx.ChorusDepth, value => fx.ChorusDepth = value),
                FloatSetting(() => Locale.Effects.ChorusFeedback, () => fx.ChorusFeedback, value => fx.ChorusFeedback = value),
                FloatSetting(() => Locale.Effects.ChorusDelay, () => fx.ChorusDelay, value => fx.ChorusDelay = value),

                ToggleSetting(() => Locale.Effects.Compressor, () => fx.IsCompressor, value => fx.IsCompressor = value),
                FloatSetting(() => Locale.Effects.CompressorGain, () => fx.CompressorGain, value => fx.CompressorGain = value),
                FloatSetting(() => Locale.Effects.CompressorAttack, () => fx.CompressorAttack, value => fx.CompressorAttack = value),
                FloatSetting(() => Locale.Effects.CompressorRelease, () => fx.CompressorRelease, value => fx.CompressorRelease = value),
                FloatSetting(() => Locale.Effects.CompressorThreshold, () => fx.CompressorThreshold, value => fx.CompressorThreshold = value),
                FloatSetting(() => Locale.Effects.CompressorRatio, () => fx.CompressorRatio, value => fx.CompressorRatio = value),
                FloatSetting(() => Locale.Effects.CompressorPredelay, () => fx.CompressorPredelay, value => fx.CompressorPredelay = value),

                ToggleSetting(() => Locale.Effects.Distortion, () => fx.IsDistortion, value => fx.IsDistortion = value),
                FloatSetting(() => Locale.Effects.DistortionGain, () => fx.DistortionGain, value => fx.DistortionGain = value),
                FloatSetting(() => Locale.Effects.DistortionEdge, () => fx.DistortionEdge, value => fx.DistortionEdge = value),
                FloatSetting(() => Locale.Effects.DistortionPostEQ, () => fx.DistortionPostEQCenterFrequency, value => fx.DistortionPostEQCenterFrequency = value),

                ToggleSetting(() => Locale.Effects.Echo, () => fx.IsEcho, value => fx.IsEcho = value),
                FloatSetting(() => Locale.Effects.EchoWetDryMix, () => fx.EchoWetDryMix, value => fx.EchoWetDryMix = value),
                FloatSetting(() => Locale.Effects.EchoFeedback, () => fx.EchoFeedback, value => fx.EchoFeedback = value),
                FloatSetting(() => Locale.Effects.EchoLeftDelay, () => fx.EchoLeftDelay, value => fx.EchoLeftDelay = value),
                FloatSetting(() => Locale.Effects.EchoRightDelay, () => fx.EchoRightDelay, value => fx.EchoRightDelay = value),
                ToggleSetting(() => Locale.Effects.EchoPanDelay, () => fx.EchoPanDelay, value => fx.EchoPanDelay = value),

                ToggleSetting(() => Locale.Effects.Flanger, () => fx.IsFlanger, value => fx.IsFlanger = value),
                FloatSetting(() => Locale.Effects.FlangerWetDryMix, () => fx.FlangerWetDryMix, value => fx.FlangerWetDryMix = value),
                FloatSetting(() => Locale.Effects.FlangerDepth, () => fx.FlangerDepth, value => fx.FlangerDepth = value),
                FloatSetting(() => Locale.Effects.FlangerFeedback, () => fx.FlangerFeedback, value => fx.FlangerFeedback = value),
                FloatSetting(() => Locale.Effects.FlangerFrequency, () => fx.FlangerFrequency, value => fx.FlangerFrequency = value),
                FloatSetting(() => Locale.Effects.FlangerDelay, () => fx.FlangerDelay, value => fx.FlangerDelay = value),

                ToggleSetting(() => Locale.Effects.Gargle, () => fx.IsGargle, value => fx.IsGargle = value),
                IntegerSetting(() => Locale.Effects.GargleRate, () => fx.GargleRate, value => fx.GargleRate = value),
                FloatSetting(() => Locale.Effects.GargleWaveShape, () => fx.GargleWaveShape, value => fx.GargleWaveShape = value),

                ToggleSetting(() => Locale.Effects.ParamEQ, () => fx.IsParamEQ, value => fx.IsParamEQ = value),
                FloatSetting(() => Locale.Effects.ParamEQCenter, () => fx.ParamEQCenter, value => fx.ParamEQCenter = value),
                FloatSetting(() => Locale.Effects.ParamEQBandwidth, () => fx.ParamEQBandwidth, value => fx.ParamEQBandwidth = value),
                FloatSetting(() => Locale.Effects.ParamEQGain, () => fx.ParamEQGain, value => fx.ParamEQGain = value),

                ToggleSetting(() => Locale.Effects.Reverb, () => fx.IsReverb, value => fx.IsReverb = value),
                FloatSetting(() => Locale.Effects.ReverbInGain, () => fx.ReverbInGain, value => fx.ReverbInGain = value),
                FloatSetting(() => Locale.Effects.ReverbMix, () => fx.ReverbReverbMix, value => fx.ReverbReverbMix = value),
                FloatSetting(() => Locale.Effects.ReverbTime, () => fx.ReverbReverbTime, value => fx.ReverbReverbTime = value),
                FloatSetting(() => Locale.Effects.ReverbHighFreqRT, () => fx.ReverbHighFreqRTRatio, value => fx.ReverbHighFreqRTRatio = value),
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

        private static void AddRows(Table table, int count, Func<int, (string, string, string)> rowSelector)
        {
            int startIndex = _currentPage * _pageSize;
            int endIndex = Math.Min(startIndex + _pageSize, count);

            for (int i = startIndex; i < endIndex; i++)
            {
                var (name, value, hint) = rowSelector(i);
                if (i == _selectedIndex)
                {
                    table.AddRow(
                        Themes.sColor($"> {Markup.Escape(name)}", Themes.CurrentTheme.GeneralSettings.SettingTextColor),
                        Themes.sColor(Markup.Escape(value), Themes.CurrentTheme.GeneralSettings.HeaderTextColor),
                        Themes.sColor(Markup.Escape(hint), Themes.CurrentTheme.GeneralSettings.HeaderTextColor));
                }
                else
                {
                    table.AddRow(
                        Markup.Escape(name),
                        Markup.Escape(value),
                        Markup.Escape(hint));
                }
            }
        }

        private static Task EditSpeedAsync()
        {
            string input = Message.InputFloat(Locale.SongMetadata.EnterSpeed, _workingCopy.Speed, "0.00");
            if (float.TryParse(input, out float value) && value >= 0.25f && value <= 4.0f)
            {
                _workingCopy.Speed = value;
            }
            else
            {
                Message.Data(Locale.SongMetadata.InvalidSpeed, Locale.OutsideItems.InvalidInput, true, false);
            }
            return Task.CompletedTask;
        }

        private static Task EditPitchAsync()
        {
            string input = Message.InputFloat(Locale.SongMetadata.EnterPitch, _workingCopy.Pitch, "0.0");
            if (float.TryParse(input, out float value) && value >= -24.0f && value <= 24.0f)
            {
                _workingCopy.Pitch = value;
            }
            else
            {
                Message.Data(Locale.SongMetadata.InvalidPitch, Locale.OutsideItems.InvalidInput, true, false);
            }
            return Task.CompletedTask;
        }

        private static Task EditTrimStartAsync()
        {
            string input = Message.Input("", Locale.SongMetadata.EnterTrimStart, _workingCopy.TrimStart ?? "");
            if (string.IsNullOrWhiteSpace(input))
            {
                _workingCopy.TrimStart = null;
            }
            else if (SongPlaybackMetadata.TimeStringToSeconds(input) is double seconds)
            {
                _workingCopy.TrimStart = SongPlaybackMetadata.SecondsToTimeString(seconds);
            }
            else
            {
                Message.Data(Locale.SongMetadata.InvalidTime, Locale.OutsideItems.InvalidInput, true, false);
            }
            return Task.CompletedTask;
        }

        private static Task EditTrimEndAsync()
        {
            string input = Message.Input("", Locale.SongMetadata.EnterTrimEnd, _workingCopy.TrimEnd ?? "");
            if (string.IsNullOrWhiteSpace(input))
            {
                _workingCopy.TrimEnd = null;
            }
            else if (SongPlaybackMetadata.TimeStringToSeconds(input) is double seconds)
            {
                _workingCopy.TrimEnd = SongPlaybackMetadata.SecondsToTimeString(seconds);
            }
            else
            {
                Message.Data(Locale.SongMetadata.InvalidTime, Locale.OutsideItems.InvalidInput, true, false);
            }
            return Task.CompletedTask;
        }

        private static void SaveAndApply()
        {
            SongMetadataStore.Set(_songUri, _workingCopy);

            // If the song we edited is currently playing, restart it so metadata takes effect.
            string currentEntry = Utils.Songs[Utils.CurrentSongIndex];
            string currentUri = currentEntry;
            if (currentEntry.Contains(Utils.JammerFileDelimeter))
            {
                currentUri = currentEntry.Split(Utils.JammerFileDelimeter)[0];
            }
            if (SongMetadataStore.GetKey(currentUri) == SongMetadataStore.GetKey(_songUri))
            {
                Play.SetEffectsToChannel();
            }
        }

        private static SettingDescriptor ToggleSetting(Func<string> name, Func<bool> get, Action<bool> set) =>
            new(name,
                () => get() ? Locale.Miscellaneous.True : Locale.Miscellaneous.False,
                () => { set(!get()); return Task.CompletedTask; },
                () => Locale.Settings.ToToggle);

        private static SettingDescriptor IntegerSetting(Func<string> name, Func<int> get, Action<int> set) =>
            new(name, () => get().ToString(), async () =>
            {
                string input = Message.InputInt(name(), get());
                if (int.TryParse(input, out int value))
                {
                    set(value);
                }
                else
                {
                    Message.Data(Locale.OutsideItems.InvalidInput, Locale.OutsideItems.Error, true, false);
                }
                await Task.CompletedTask;
            }, () => Locale.Settings.ToChange);

        private static SettingDescriptor FloatSetting(Func<string> name, Func<float> get, Action<float> set) =>
            new(name, () => get().ToString(), async () =>
            {
                string input = Message.InputFloat(name(), get(), "0.00");
                if (float.TryParse(input, out float value))
                {
                    set(value);
                }
                else
                {
                    Message.Data(Locale.OutsideItems.InvalidInput, Locale.OutsideItems.Error, true, false);
                }
                await Task.CompletedTask;
            }, () => Locale.Settings.ToChange);
    }
}
