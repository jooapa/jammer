using SharpHook.Native;
using Spectre.Console;
using Jammer;
using Jammer.Components;
#pragma warning disable CS8604
#pragma warning disable CS8602
#pragma warning disable CS8600

namespace Jammer
{
    public static class TUI
    {

        static bool cls = false;

        /// <summary>
        /// call DrawPlayer(true, true) to draw the whole player
        /// </summary>
        /// <param name="DrawTime">Draw the time</param>
        /// <param name="drawVisualizer">Draw the visualizer</param>
        /// <param name="drawOnlyVisualizer">Draw only the visualizer</param>
        /// <param name="DrawOnlyTime">Draw only the time</param>
        /// <returns></returns>
        static public void DrawPlayer()
        {
            var layout = new LayoutConfig(Start.consoleWidth, Start.consoleHeight);
            try
            {
                var ansiConsoleSettings = new AnsiConsoleSettings();
                AnsiConsole.Profile.Encoding = System.Text.Encoding.UTF8;
                var ansiConsole = AnsiConsole.Create(ansiConsoleSettings);
                if (Start.playerView == "help" || Start.playerView == "settings")
                {
                    return;
                }
                var mainTable = new Table
                {
                    Border = Themes.bStyle(Themes.CurrentTheme.Playlist.BorderStyle)
                };
                mainTable.BorderColor(Themes.bColor(Themes.CurrentTheme.Playlist.BorderColor));

                var songsTable = new Table();
                var timeTable = new Table();


                // näyttää aina ne tiedot eikä mitään url hommaa
                // Funcs.UpdateSongListCorrectly();

                if (Start.playerView == "default")
                {
                    songsTable = PlaylistComponent.CreateNormalPlaylistTable(layout);
                }
                else if (Start.playerView == "all")
                {
                    songsTable = PlaylistComponent.CreateAllSongsPlaylistTable(layout);
                }

                if (cls)
                {
                    if (Start.playerView != "all")
                    {
                        // AnsiConsole.Clear();
                        Debug.dprint("DrawPlayer - clear");
                    }
                    cls = false;
                }
                // if (Start.playerView == "default" || Start.playerView == "fake") {
                //     AnsiConsole.Cursor.SetPosition(0, 0);
                // }
                string songPath;
                if (Utils.CurrentMusic == 0 && Utils.CurSongError)
                {
                    if (Utils.CustomTopErrorMessage != "")
                    {
                        songPath = Utils.CustomTopErrorMessage;
                    }
                    else
                    {
                        songPath = "Error: cannot play the song";
                    }
                }
                else if (Utils.CurrentMusic == 0)
                {
                    songPath = "No song is playing";
                }
                else
                {
                    songPath = Utils.CurrentSongPath;
                }

                Utils.CustomTopErrorMessage = "";

                // render maintable with tables in it
                mainTable.AddColumns(Themes.sColor(Funcs.GetSongWithDots(Start.Sanitize(
                    songPath
                ), layout.CalculateMainTableWidth()), Themes.CurrentTheme.Playlist.PathColor)).Width(Start.consoleWidth);
                mainTable.AddRow(songsTable.Centered().Width(Start.consoleWidth));

                // Calculate table row count using LayoutCalculator
                ViewType viewType = LayoutCalculator.GetViewType(Start.playerView);
                bool hasPlaylist = !(Utils.CurrentPlaylist == "" && !Funcs.IsInsideOfARssFeed());
                int tableRowCount = LayoutCalculator.CalculateTableRowCount(
                    layout.ConsoleHeight, 
                    viewType, 
                    Preferences.isVisualizer, 
                    hasPlaylist, 
                    Utils.Songs.Length
                );

                for (int i = 0; i < tableRowCount; i++)
                {
                    mainTable.AddEmptyRow();
                }

                var helpTable = new Table
                {
                    Border = Themes.bStyle(Themes.CurrentTheme.Playlist.MiniHelpBorderStyle),
                };
                helpTable.BorderColor(Themes.bColor(Themes.CurrentTheme.Playlist.MiniHelpBorderColor));
                // helpTable.BorderColor(Themes.bColor(Themes.CurrentTheme.GeneralHelp.BorderColor));
                // helpTable.AddColumn($"[red]{Keybindings.Help}[/] {Locale.Player.ForHelp} | [yellow]{Keybindings.Settings}[/] {Locale.Help.ForSettings} | [green]{Keybindings.ShowHidePlaylist}[/] {Locale.Player.ForPlaylist}");

                helpTable.AddColumn(
                    Themes.sColor($"{Keybindings.Help}", Themes.CurrentTheme.Playlist.HelpLetterColor) + " "
                    + Themes.sColor(Locale.Player.ForHelp, Themes.CurrentTheme.Playlist.ForHelpTextColor) + Themes.sColor(" | ", Themes.CurrentTheme.Playlist.ForSeperatorTextColor)
                    + Themes.sColor($"{Keybindings.Settings}", Themes.CurrentTheme.Playlist.SettingsLetterColor) + " "
                    + Themes.sColor(Locale.Help.ForSettings, Themes.CurrentTheme.Playlist.ForHelpTextColor) + Themes.sColor(" | ", Themes.CurrentTheme.Playlist.ForSeperatorTextColor)
                    + Themes.sColor($"{Keybindings.ShowHidePlaylist}", Themes.CurrentTheme.Playlist.PlaylistLetterColor) + " "
                    + Themes.sColor(Locale.Player.ForPlaylist, Themes.CurrentTheme.Playlist.ForHelpTextColor)
                );

                mainTable.AddRow(helpTable);


                if (Preferences.isVisualizer)
                {
                    mainTable.AddEmptyRow();
                }

                mainTable.AddRow(PlayerTimeComponent.CreateTimeTable(layout));

                AnsiConsole.Cursor.SetPosition(0, 0);
                AnsiConsole.Write(mainTable);
            }
            catch (Exception e)
            {
                AnsiConsole.Cursor.SetPosition(0, 0);
                AnsiConsole.MarkupLine($"[red]{Locale.Player.DrawingError}[/]");
                AnsiConsole.MarkupLine($"[red]{Locale.Player.ControlsWillWork}[/]");
                AnsiConsole.MarkupLine("[red]" + e + "[/]");
                AnsiConsole.WriteLine(Utils.Songs.Length);
            }

        }


        static public void DrawVisualizer()
        {
            var layout = new LayoutConfig(Start.consoleWidth, Start.consoleHeight);
            VisualizerComponent.DrawVisualizerToConsole(layout);
        }

        static public void DrawTime()
        {
            var layout = new LayoutConfig(Start.consoleWidth, Start.consoleHeight);
            PlayerTimeComponent.DrawTimeToConsole(layout);
        }

        static public void ClearScreen()
        {
            cls = true;
        }

        static public string GetStateLogo(bool getColor)
        {
            string state = Start.state.ToString();
            if (Start.state == MainStates.playing || Start.state == MainStates.play)
            {
                if (getColor)
                    state = Themes.sColor(Themes.CurrentTheme.Time.PlayingLetterLetter, Themes.CurrentTheme.Time.PlayingLetterColor);
                else
                    state = Themes.CurrentTheme.Time.PlayingLetterLetter;
            }
            else if (Start.state == MainStates.idle || Start.state == MainStates.pause)
            {
                if (getColor)
                    state = Themes.sColor(Themes.CurrentTheme.Time.PausedLetterLetter, Themes.CurrentTheme.Time.PausedLetterColor);
                else
                    state = Themes.CurrentTheme.Time.PausedLetterLetter;
            }
            else if (Start.state == MainStates.stop)
            {
                if (getColor)
                    state = Themes.sColor(Themes.CurrentTheme.Time.StoppedLetterLetter, Themes.CurrentTheme.Time.StoppedLetterColor);
                else
                    state = Themes.CurrentTheme.Time.StoppedLetterLetter;
            }
            else if (Start.state == MainStates.next)
            {
                if (getColor)
                    state = Themes.sColor(Themes.CurrentTheme.Time.NextLetterLetter, Themes.CurrentTheme.Time.NextLetterColor);
                else
                    state = Themes.CurrentTheme.Time.NextLetterLetter;
            }
            else if (Start.state == MainStates.previous)
            {
                if (getColor)
                    state = Themes.sColor(Themes.CurrentTheme.Time.PreviousLetterLetter, Themes.CurrentTheme.Time.PreviousLetterColor);
                else
                    state = Themes.CurrentTheme.Time.PreviousLetterLetter;
            }

            state += " ";

            return state;
        }

        // Old UIComponent methods have been replaced by component classes

        public static string ProgressBar(double value, double max, LayoutConfig layout)
        {
            // if (length == null) {
            //     length = 100;
            // }

            int length = layout.CalculateProgressBarWidth();

            string volumeMark = Preferences.isMuted ? Themes.sColor(Math.Round(Preferences.oldVolume * 100) + "%", Themes.CurrentTheme.Time.VolumeColorMuted) : Themes.sColor(Math.Round(Preferences.volume * 100) + "%", Themes.CurrentTheme.Time.VolumeColorNotMuted);
            string volumeString = Preferences.isMuted ? Math.Round(Preferences.oldVolume * 100) + "%" : Math.Round(Preferences.volume * 100) + "%";
            string shuffleMark = Preferences.isShuffle ? Themes.sColor(Themes.CurrentTheme.Time.ShuffleOnLetter, Themes.CurrentTheme.Time.ShuffleLetterOnColor) : Themes.sColor(Themes.CurrentTheme.Time.ShuffleOffLetter, Themes.CurrentTheme.Time.ShuffleLetterOffColor);
            string shuffleString =
                Preferences.isShuffle ?
                    Themes.CurrentTheme.Time.ShuffleOnLetter :
                    Themes.CurrentTheme.Time.ShuffleOffLetter;

            string loopMark, loopString;
            switch (Preferences.loopType)
            {
                case LoopType.Always:
                    loopString = Themes.CurrentTheme.Time.LoopOnLetter;
                    loopMark = Themes.sColor(loopString, Themes.CurrentTheme.Time.LoopLetterOnColor);
                    break;
                case LoopType.Once:
                    loopString = Themes.CurrentTheme.Time.LoopOnceLetter;
                    loopMark = Themes.sColor(loopString, Themes.CurrentTheme.Time.LoopLetterOnceColor);
                    break;
                case LoopType.None:
                default:
                    loopString = Themes.CurrentTheme.Time.LoopOffLetter;
                    loopMark = Themes.sColor(loopString, Themes.CurrentTheme.Time.LoopLetterOffColor);
                    break;
            }

            string progressBar = GetStateLogo(true) + shuffleMark + loopMark +
                Funcs.CalculateTime(value, true) + Themes.sColor(" |", Themes.CurrentTheme.Time.TimebarColor);
            // length is modified also by the time string
            length -= GetStateLogo(false).Length
                + shuffleString.Length
                + loopString.Length
                + Funcs.CalculateTime(value, false).Length
                + Funcs.CalculateTime(max, false).Length
                + 2; // 2 is for the " |"

            string extraVolume;
            string extraVolumeString;
            if (volumeString.Length >= 4)
            {
                extraVolume = " " + volumeMark;
                extraVolumeString = " " + volumeString;
            }
            else if (volumeString.Length == 3)
            {
                extraVolume = "  " + volumeMark;
                extraVolumeString = "  " + volumeString;
            }
            else
            {
                extraVolume = "   " + volumeMark;
                extraVolumeString = "   " + volumeString;
            }

            length -= extraVolumeString.Length;

            int progress = (int)(value / max * length);
            // length is modified also by the volume string
            for (int i = 0; i < length; i++)
            {
                if (i < progress)
                {
                    progressBar += Themes.CurrentTheme.Time.TimebarLetter;
                }
                else
                {
                    progressBar += " ";
                }

            }

            progressBar = Themes.sColor(progressBar, Themes.CurrentTheme.Time.TimebarColor);
            progressBar += Themes.sColor("| ", Themes.CurrentTheme.Time.TimebarColor) + Funcs.CalculateTime(max, true) + extraVolume;

            return progressBar;
        }

        public static void PrintToTopOfPlayer(string theText)
        {
            var layout = new LayoutConfig(Start.consoleWidth, Start.consoleHeight);
            var tmpstr = theText;
            var spaces = layout.CalculateTopMessageWidth(theText.Length);
            for (int i = 0; i < spaces; i++)
            {
                tmpstr += " ";
            }

            // AnsiConsole.Cursor.SetPosition(2, 2);
            AnsiConsole.Write($"\x1b[2;3H{tmpstr}");
        }
        static public void DrawHelp()
        {
            var layout = new LayoutConfig(Start.consoleWidth, Start.consoleHeight);
            HelpMenuComponent.DrawHelpToConsole(layout);
        }

        static private string DrawHelpTextColouring(string[] textArray)
        {
            if (textArray.Length == 1)
            {
                return Themes.sColor(textArray[0], Themes.CurrentTheme.GeneralHelp.ControlTextColor);
            }
            else if (textArray.Length == 2)
            {
                // return $"[green1]{textArray[0]}[/] + {textArray[1]}";
                return
                Themes.sColor(textArray[0], Themes.CurrentTheme.GeneralHelp.ModifierTextColor_1)
                + " + "
                + Themes.sColor(textArray[1], Themes.CurrentTheme.GeneralHelp.ControlTextColor);
            }
            else if (textArray.Length == 3)
            {
                // return $"[green1]{textArray[0]}[/] + [turquoise2]{textArray[1]}[/] + {textArray[2]}";
                return
                Themes.sColor(textArray[0], Themes.CurrentTheme.GeneralHelp.ModifierTextColor_1)
                + " + "
                + Themes.sColor(textArray[1], Themes.CurrentTheme.GeneralHelp.ModifierTextColor_2)
                + " + "
                + Themes.sColor(textArray[2], Themes.CurrentTheme.GeneralHelp.ControlTextColor);
            }
            else if (textArray.Length == 4)
            {
                // return $"[green1]{textArray[0]}[/] + [turquoise2]{textArray[1]}[/] + [blue]{textArray[2]}[/] + {textArray[3]}";
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
                // return textArray[0];
                return Themes.sColor(textArray[0], Themes.CurrentTheme.GeneralHelp.ControlTextColor);
            }
        }
        static public void DrawSettings()
        {
            var layout = new LayoutConfig(Start.consoleWidth, Start.consoleHeight);
            SettingsComponent.DrawSettingsToConsole(layout);
        }

        private static void DrawHelpSettingInfo()
        {
            // AnsiConsole.Markup($"{Locale.Help.Press} [red]{Keybindings.Help}[/] {Locale.Help.ToHideHelp}");
            // AnsiConsole.Markup($"\n{Locale.Help.Press} [yellow]{Keybindings.Settings}[/] {Locale.Help.ForSettings}");
            // AnsiConsole.Markup($"\n{Locale.Help.Press} [green]{Keybindings.ShowHidePlaylist}[/] {Locale.Help.ToShowPlaylist}\n");
            var helpTable = new Table
            {
                Border = Themes.bStyle(Themes.CurrentTheme.Playlist.MiniHelpBorderStyle),
            };
            helpTable.BorderColor(Themes.bColor(Themes.CurrentTheme.Playlist.MiniHelpBorderColor));
            // helpTable.BorderColor(Themes.bColor(Themes.CurrentTheme.GeneralHelp.BorderColor));
            // helpTable.AddColumn($"[red]{Keybindings.Help}[/] {Locale.Player.ForHelp} | [yellow]{Keybindings.Settings}[/] {Locale.Help.ForSettings} | [green]{Keybindings.ShowHidePlaylist}[/] {Locale.Player.ForPlaylist}");

            helpTable.AddColumn(
                Themes.sColor($"{Keybindings.Help}", Themes.CurrentTheme.Playlist.HelpLetterColor) + " "
                + Themes.sColor(Locale.Player.ForHelp, Themes.CurrentTheme.Playlist.ForHelpTextColor) + Themes.sColor(" | ", Themes.CurrentTheme.Playlist.ForSeperatorTextColor)
                + Themes.sColor($"{Keybindings.Settings}", Themes.CurrentTheme.Playlist.SettingsLetterColor) + " "
                + Themes.sColor(Locale.Help.ForSettings, Themes.CurrentTheme.Playlist.ForHelpTextColor) + Themes.sColor(" | ", Themes.CurrentTheme.Playlist.ForSeperatorTextColor)
                + Themes.sColor($"{Keybindings.ShowHidePlaylist}", Themes.CurrentTheme.Playlist.PlaylistLetterColor) + " "
                + Themes.sColor(Locale.Player.ForPlaylist, Themes.CurrentTheme.Playlist.ForHelpTextColor)
            );

            AnsiConsole.Write(helpTable);
        }

        public static void CliHelp()
        {
            var table = new Table();
            table.AddColumn(Locale.CliHelp.Commands);
            table.AddColumn(Locale.CliHelp.Description);

            // table.AddRow("[grey]jammer[/] <url> ...", "Play song(s) from url(s)");
            table.AddRow("[grey]jammer[/] <[green]file[/]> ...", Locale.CliHelp.PlaySongFromFile);
            table.AddRow($"[grey]jammer[/] [green]soundcloud.com/{Locale.CliHelp.Username}/{Locale.CliHelp.TrackName} [/] ...", Locale.CliHelp.PlaySongFromSoundcloud);
            table.AddRow($"[grey]jammer[/] [green]soundcloud.com/{Locale.CliHelp.Username}/sets/{Locale.CliHelp.PlaylistName}[/] ...", Locale.CliHelp.PlaySongFromSoundcloudPlaylist);
            table.AddRow($"[grey]jammer[/] [green]youtube.com/watch?v=video-id[/] ...", Locale.CliHelp.PlaySongFromYoutube);
            table.AddRow($"[grey]jammer[/] [green]youtube.com/playlist?list=playlist-id[/] ...", Locale.CliHelp.PlayPlaylistFromYoutube);
            /* table.AddRow("[grey]jammer[/] [green]playlist[/]", Locale.CliHelp.ShowPlaylistCommands); */
            table.AddRow($"[grey]jammer[/] [green]--start[/]", Locale.CliHelp.OpenJammerFolder);
            table.AddRow($"[grey]jammer[/] [green]--update[/]", Locale.CliHelp.AutoUpdateJammer);
            table.AddRow($"[grey]jammer[/] [green]-v[/][grey],[/][green] --version[/]", $"{Locale.CliHelp.ShowJammerVersion} [grey]" + Utils.Version + "[/]");
            AnsiConsole.Write(table);

            PlaylistHelp();
        }
        static public void PlaylistHelp()
        {
            var table = new Table();
            table.AddColumn(Locale.CliHelp.PlaylistCommands);
            table.AddColumn(Locale.CliHelp.Description);
            // TODO LOCALE ehkä joskus
            table.AddRow($"[grey]jammer[/] [red]-h[/][grey],[/][red] --help [/] ", "show this help message");
            table.AddRow($"[grey]jammer[/] [red]-p[/][grey],[/][red] --play  [/] <{Locale.CliHelp.Name}>", Locale.CliHelp.PlayPlaylist);
            table.AddRow($"[grey]jammer[/] [red]-c[/][grey],[/][red] --create[/] <{Locale.CliHelp.Name}>", Locale.CliHelp.CreatePlaylist);
            table.AddRow($"[grey]jammer[/] [red]-d[/][grey],[/][red] --delete[/] <{Locale.CliHelp.Name}>", Locale.CliHelp.DeletePlaylist);
            table.AddRow($"[grey]jammer[/] [red]-a[/][grey],[/][red] --add   [/] <{Locale.CliHelp.Name}> <song> ...", Locale.CliHelp.AddSongsToPlaylist);
            table.AddRow($"[grey]jammer[/] [red]-r[/][grey],[/][red] --remove[/] <{Locale.CliHelp.Name}> <{Locale.CliHelp.Song}> ...", Locale.CliHelp.RemoveSongsFromPlaylist);
            table.AddRow($"[grey]jammer[/] [red]-s[/][grey],[/][red] --show  [/] <{Locale.CliHelp.Name}>", Locale.CliHelp.ShowSongsInPlaylist);
            table.AddRow($"[grey]jammer[/] [red]-l[/][grey],[/][red] --list  [/] ", Locale.CliHelp.ListAllPlaylists);
            table.AddRow($"[grey]jammer[/] [red]-f[/][grey],[/][red] --flush [/] ", "delete all songs from the <jammer/songs> folder");
            table.AddRow($"[grey]jammer[/] [red]-gp[/][grey],[/][red] --get-path [/] ", "get the path to the <jammer/songs> folder");
            table.AddRow($"[grey]jammer[/] [red]-hm[/][grey],[/][red] --home [/] ", "play all songs from the <jammer/songs> folder");
            table.AddRow($"[grey]jammer[/] [red]-so[/][grey],[/][red] --songs [/] ", "open <jammer/songs> folder");
            AnsiConsole.Write(table);
        }
        public static void Version()
        {
            AnsiConsole.MarkupLine($"[green]Jammer {Locale.Miscellaneous.Version} " + Utils.Version + "[/]");
        }

        public static void EditKeyBindings()
        {
            IniFileHandling.Create_KeyDataIni(0);

            var table = new Table();
            table.Border = Themes.bStyle(Themes.CurrentTheme.EditKeybinds.BorderStyle);
            table.BorderColor(Themes.bColor(Themes.CurrentTheme.EditKeybinds.BorderColor));

            // Calculate responsive column widths for consistent display
            var layout = new LayoutConfig(Start.consoleWidth, Start.consoleHeight);
            int totalWidth = layout.ConsoleWidth - 8; // Account for borders and padding
            int descriptionWidth = (int)(totalWidth * 0.6); // 60% for description
            int keybindWidth = (int)(totalWidth * 0.4);     // 40% for keybind
            
            table.AddColumn(new TableColumn(Locale.Help.Description).Width(descriptionWidth));
            table.AddColumn(new TableColumn(Locale.LocaleKeybind.CurrentControl).Width(keybindWidth));
            (string[] _elements, string[] _description) = IniFileHandling.ReadAll_KeyData();

            // Counter to track the index for the description array

            // Loop through the _elements array
            for (int i = 0; i < _elements.Length; i++)
            {
                // Check if the description at descIndex is not empty
                // Add row to the table
                if (i == 0)
                {
                    table.AddRow(Themes.sColor(_description[i], Themes.CurrentTheme.EditKeybinds.CurrentKeyColor), Themes.sColor(_elements[i], Themes.CurrentTheme.EditKeybinds.CurrentKeyColor));
                }
                else
                {
                    table.AddRow(Themes.sColor(_description[i], Themes.CurrentTheme.EditKeybinds.DescriptionColor), Themes.sColor(_elements[i], Themes.CurrentTheme.EditKeybinds.CurrentControlColor));
                }
            }

            AnsiConsole.Cursor.SetPosition(0, 0);
            AnsiConsole.Cursor.Hide();
            AnsiConsole.Write(table);
            AnsiConsole.Cursor.Show();

            if (IniFileHandling.EditingKeybind)
            {
                string final = IniFileHandling.previousClick.ToString();
                if (IniFileHandling.isShiftCtrlAlt)
                {
                    final = "Shift + Ctrl + Alt + " + final;
                }
                else if (IniFileHandling.isShiftCtrl)
                {
                    final = "Shift + Ctrl + " + final;
                }
                else if (IniFileHandling.isShiftAlt)
                {
                    final = "Shift + Alt + " + final;
                }
                else if (IniFileHandling.isCtrlAlt)
                {
                    final = "Ctrl + Alt + " + final;
                }
                else if (IniFileHandling.isShift)
                {
                    final = "Shift + " + final;
                }
                else if (IniFileHandling.isCtrl)
                {
                    final = "Ctrl + " + final;
                }
                else if (IniFileHandling.isAlt)
                {
                    final = "Alt + " + final;
                }
                AnsiConsole.Markup(Themes.sColor($"{Locale.LocaleKeybind.EditKeyBindMessage1} {Keybindings.Choose}\n", Themes.CurrentTheme.Playlist.InfoColor));
                AnsiConsole.Markup(Themes.sColor($"{Locale.LocaleKeybind.EditKeyBindMessage2}\n", Themes.CurrentTheme.Playlist.InfoColor));
                AnsiConsole.Markup(Themes.sColor($"{final}\n", Themes.CurrentTheme.EditKeybinds.EnteredKeyColor));

            }
            else
            {
                AnsiConsole.Markup(Themes.sColor($"{Locale.LocaleKeybind.EditKeyBindMessage3}{Keybindings.PlaylistViewScrollup}, {Keybindings.PlaylistViewScrolldown}\n", Themes.CurrentTheme.Playlist.InfoColor)); // Press Enter to edit); // Press Enter to edit
                AnsiConsole.Markup(Themes.sColor($"{Locale.LocaleKeybind.EditKeyBindMessage4}\n", Themes.CurrentTheme.Playlist.InfoColor));
            }
            DrawHelpSettingInfo();
        }
        public static void ChangeLanguage()
        {
            var table = new Table();
            table.Border = Themes.bStyle(Themes.CurrentTheme.LanguageChange.BorderStyle);
            table.BorderColor(Themes.bColor(Themes.CurrentTheme.LanguageChange.BorderColor));
            table.AddColumn(Locale.LocaleKeybind.Description);
            string[] _elements = IniFileHandling.ReadAll_Locales();

            // Loop through the _elements array
            for (int i = 0; i < _elements.Length; i++)
            {
                if (i == 0)
                {
                    table.AddRow(Themes.sColor(_elements[i], Themes.CurrentTheme.LanguageChange.CurrentLanguageColor));
                }
                else
                {
                    table.AddRow(Themes.sColor(_elements[i], Themes.CurrentTheme.LanguageChange.TextColor));
                }
            }

            AnsiConsole.Cursor.SetPosition(0, 0);
            AnsiConsole.Cursor.Hide();
            AnsiConsole.Write(table);
            AnsiConsole.Cursor.Show();

            AnsiConsole.Markup(Themes.sColor($"{Locale.LocaleKeybind.ChangeLanguageMessage1} {Keybindings.PlaylistViewScrollup}, {Keybindings.PlaylistViewScrolldown}\n", Themes.CurrentTheme.Playlist.InfoColor)); // Press Enter to edit); // Press Enter to edit
            DrawHelpSettingInfo();
        }

        public static void RefreshCurrentView()
        {
            //NOTE(ra) This Clear() caused flickering.
            /* AnsiConsole.Clear(); */
            if (Start.playerView == "default")
            {
                DrawPlayer();
            }
            else if (Start.playerView == "help")
            {
                DrawHelp();
            }
            else if (Start.playerView == "settings")
            {
                DrawSettings();
            }
            else if (Start.playerView == "all")
            {
                DrawPlayer();
            }
            else if (Start.playerView == "fake")
            {
                DrawPlayer();
            }
            else if (Start.playerView == "editkeybindings")
            {
                EditKeyBindings();
            }
            else if (Start.playerView == "changelanguage")
            {
                ChangeLanguage();
            }
        }
    }
}
