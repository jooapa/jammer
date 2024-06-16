using SharpHook.Native;
using Spectre.Console;
// #pragma warning disable CS8604
// #pragma warning disable CS8602
// #pragma warning disable CS8600

namespace Jammer {
    public static class TUI {

        static bool cls = false;

        /// <summary>
        /// call DrawPlayer(true, true) to draw the whole player
        /// </summary>
        /// <param name="DrawTime">Draw the time</param>
        /// <param name="drawVisualizer">Draw the visualizer</param>
        /// <param name="drawOnlyVisualizer">Draw only the visualizer</param>
        /// <param name="DrawOnlyTime">Draw only the time</param>
        /// <returns></returns>
        static public void DrawPlayer() {
            try {
                
                var ansiConsoleSettings = new AnsiConsoleSettings();
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

                if (Start.playerView == "default") {
                    UIComponent_Normal(songsTable);
                }
                else if (Start.playerView == "all") {
                    UIComponent_Songs(songsTable);
                }

                if (cls) {
                    if (Start.playerView != "all") {
                        // AnsiConsole.Clear();
                        Debug.dprint("DrawPlayer - clear");
                    }
                    cls = false;
                }
                // if (Start.playerView == "default" || Start.playerView == "fake") {
                //     AnsiConsole.Cursor.SetPosition(0, 0);
                // }
                
                // render maintable with tables in it
                mainTable.AddColumns(Themes.sColor(Funcs.GetSongWithDots(Start.Sanitize(Utils.currentSong), Start.consoleWidth - 8), Themes.CurrentTheme.Playlist.PathColor)).Width(Start.consoleWidth);
                mainTable.AddRow(songsTable.Centered().Width(Start.consoleWidth));

                // add \n to the end of the maintable until the end of the console by height
                int tableRowCount=0;
                int magicIndex;

                if (Start.playerView == "default") {
                    magicIndex = 18;
                    if (Utils.currentPlaylist == "") {
                        magicIndex -= 2;
                    }
                    if (Preferences.isVisualizer) {
                        magicIndex++;
                    }
                }
                else {
                    magicIndex = 22;
                    if (Preferences.isVisualizer) {
                        magicIndex++;
                    }
                    // there is not 5 songs in the playlist
                    if (Utils.songs.Length < 5) {
                        magicIndex += Utils.songs.Length;
                        magicIndex-=5;
                    }
                }

                tableRowCount = Start.consoleHeight - magicIndex;

                if (tableRowCount < 0) {
                    tableRowCount = 0;
                }

                for (int i = 0; i < tableRowCount; i++) {
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


                if (Preferences.isVisualizer) {
                    mainTable.AddEmptyRow();
                }
                
                mainTable.AddRow(UIComponent_Time(timeTable));

                AnsiConsole.Cursor.SetPosition(0, 0);
                AnsiConsole.Write(mainTable);
            }
            catch (Exception e) {
                AnsiConsole.Cursor.SetPosition(0, 0);
                AnsiConsole.MarkupLine($"[red]{Locale.Player.DrawingError}[/]");
                AnsiConsole.MarkupLine($"[red]{Locale.Player.ControlsWillWork}[/]");
                AnsiConsole.MarkupLine("[red]" + e + "[/]");
            }

        }

        static public void DrawVisualizer() {
            AnsiConsole.Cursor.SetPosition(5, Start.consoleHeight - 5);
            if (Start.state == MainStates.playing || Start.state == MainStates.play) {
                AnsiConsole.Write(Visual.GetSongVisual(Start.consoleWidth+35));
            } else {
                AnsiConsole.MarkupLine(Visual.GetSongVisual(Start.consoleWidth+35, false));
            }
        }

        static public void DrawTime() {
            AnsiConsole.Cursor.SetPosition(5, Start.consoleHeight - 3);
            AnsiConsole.MarkupLine(ProgressBar(Utils.MusicTimePlayed, Utils.currentMusicLength));
        }

        static public void ClearScreen() {
            cls = true;
        }
            // "Components" of the Funcs
        static public void UIComponent_Controls(Table table) {
            table.Border = Themes.bStyle(Themes.CurrentTheme.Time.BorderStyle);
            table.BorderColor(Themes.bColor(Themes.CurrentTheme.Time.BorderColor));

            table.Alignment(Justify.Center);
            table.AddColumn(Locale.Player.Looping);
            table.AddColumn(Locale.Player.Shuffle);
            table.AddColumn(Locale.Player.Volume);
            string volume = Preferences.isMuted ? "[grey][strikethrough]" + Math.Round(Preferences.oldVolume * 100) + "%[/][/]" : Math.Round(Preferences.volume * 100) + "%";
            
            // TODO ADD STATE TO LOCALE
            table.AddRow(new Markup(Preferences.isLoop ? $"[green]{Locale.Miscellaneous.On}[/]" : $"[red]{Locale.Miscellaneous.Off}[/]").Centered(), 
                        new Markup(Preferences.isShuffle ? $"[green]{Locale.Miscellaneous.On}[/]" : $"[red]{Locale.Miscellaneous.Off}[/]").Centered());
        }

        static public string GetStateLogo(bool getColor) {
            string state = Start.state.ToString();
            if (Start.state == MainStates.playing || Start.state == MainStates.play) {
                if (getColor) 
                    state = Themes.sColor(Themes.CurrentTheme.Time.PlayingLetterLetter, Themes.CurrentTheme.Time.PlayingLetterColor);
                else 
                    state = Themes.CurrentTheme.Time.PlayingLetterLetter;
            }
            else if (Start.state == MainStates.idle || Start.state == MainStates.pause) {
                if (getColor) 
                    state = Themes.sColor(Themes.CurrentTheme.Time.PausedLetterLetter, Themes.CurrentTheme.Time.PausedLetterColor);
                else
                    state = Themes.CurrentTheme.Time.PausedLetterLetter;
            }
            else if (Start.state == MainStates.stop) {
                if (getColor) 
                    state = Themes.sColor(Themes.CurrentTheme.Time.StoppedLetterLetter, Themes.CurrentTheme.Time.StoppedLetterColor);
                else
                    state = Themes.CurrentTheme.Time.StoppedLetterLetter;
            }
            else if (Start.state == MainStates.next) {
                if (getColor) 
                    state = Themes.sColor(Themes.CurrentTheme.Time.NextLetterLetter, Themes.CurrentTheme.Time.NextLetterColor);
                else
                    state = Themes.CurrentTheme.Time.NextLetterLetter;
            }
            else if (Start.state == MainStates.previous) {
                if (getColor) 
                    state = Themes.sColor(Themes.CurrentTheme.Time.PreviousLetterLetter, Themes.CurrentTheme.Time.PreviousLetterColor);
                else
                    state = Themes.CurrentTheme.Time.PreviousLetterLetter;                    
            }

            state += " ";

            return state;
        }
        
        static public void UIComponent_Songs(Table table) {
            table.Border = Themes.bStyle(Themes.CurrentTheme.WholePlaylist.BorderStyle);
            table.BorderColor(Themes.bColor(Themes.CurrentTheme.WholePlaylist.BorderColor));
            // AnsiConsole.Clear();
            // string[] queueLines = Funcs.GetAllSongsQueue();
            string[] lines = Funcs.GetAllSongs();

            if (Utils.currentPlaylist == "") {
                table.AddColumn("No Specific Playlist Name");
            } else {
                table.AddColumn(Themes.sColor(Locale.Player.Playlist, Themes.CurrentTheme.Playlist.RandomTextColor) + " " 
                    + Themes.sColor(Utils.currentPlaylist, Themes.CurrentTheme.Playlist.PlaylistNameColor));
            }

            // table.AddColumn(Locale.OutsideItems.CurrentQueue);
            for(int i = 0; i < lines.Length; i++){
                // table.AddRow(lines[i], queueLines.Length > i ? queueLines[i] : "");
                table.AddRow(lines[i]);
            }
        }

        static public void UIComponent_Normal(Table table) {
            table.Border = Themes.bStyle(Themes.CurrentTheme.GeneralPlaylist.BorderStyle);
            table.BorderColor(Themes.bColor(Themes.CurrentTheme.GeneralPlaylist.BorderColor));

            if (Utils.currentPlaylist == "") {
                table.AddColumn(Funcs.GetPrevCurrentNextSong());
            } else {
                table.AddColumn(Themes.sColor(Locale.Player.Playlist, Themes.CurrentTheme.Playlist.RandomTextColor) + " " 
                    + Themes.sColor(Utils.currentPlaylist, Themes.CurrentTheme.Playlist.PlaylistNameColor));
                table.AddRow(Funcs.GetPrevCurrentNextSong());
            }
        }

        public static Table UIComponent_Time(Table table) {
            table.Border = Themes.bStyle(Themes.CurrentTheme.Time.BorderStyle);
            table.BorderColor(Themes.bColor(Themes.CurrentTheme.Time.BorderColor));
            table.AddColumn(ProgressBar(Utils.MusicTimePlayed, Utils.currentMusicLength));
            return table;
        }

        public static string ProgressBar(double value, double max) {
            // if (length == null) {
            //     length = 100;
            // }

            int length = Start.consoleWidth -10;

            string volumeMark = Preferences.isMuted ? Themes.sColor(Math.Round(Preferences.oldVolume * 100) + "%", Themes.CurrentTheme.Time.VolumeColorMuted) : Themes.sColor(Math.Round(Preferences.volume * 100) + "%", Themes.CurrentTheme.Time.VolumeColorNotMuted);
            string volumeString = Preferences.isMuted ? Math.Round(Preferences.oldVolume * 100) + "%":Math.Round(Preferences.volume * 100) + "%";
            string shuffleMark = Preferences.isShuffle ? Themes.sColor(Themes.CurrentTheme.Time.ShuffleOnLetter, Themes.CurrentTheme.Time.ShuffleLetterOnColor) : Themes.sColor(Themes.CurrentTheme.Time.ShuffleOffLetter, Themes.CurrentTheme.Time.ShuffleLetterOffColor);
            string shuffleString =
                Preferences.isShuffle ? 
                    Themes.CurrentTheme.Time.ShuffleOnLetter :
                    Themes.CurrentTheme.Time.ShuffleOffLetter;

            string loopMark = Preferences.isLoop ? Themes.sColor(Themes.CurrentTheme.Time.LoopOnLetter, Themes.CurrentTheme.Time.LoopLetterOnColor) : Themes.sColor(Themes.CurrentTheme.Time.LoopOffLetter, Themes.CurrentTheme.Time.LoopLetterOffColor);
            string loopString =
                Preferences.isLoop ? 
                    Themes.CurrentTheme.Time.LoopOnLetter :
                    Themes.CurrentTheme.Time.LoopOffLetter;
            
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
            if (volumeString.Length >= 4) {
                extraVolume = " " + volumeMark;
                extraVolumeString = " " + volumeString;
            } else if (volumeString.Length == 3) {
                extraVolume = "  " + volumeMark;
                extraVolumeString = "  " + volumeString;
            } else {
                extraVolume = "   " + volumeMark;
                extraVolumeString = "   " + volumeString;
            }

            length -= extraVolumeString.Length;

            int progress = (int)(value / max * length);
            // length is modified also by the volume string
            for (int i = 0; i < length; i++) {
                if (i < progress) {
                    progressBar += Themes.CurrentTheme.Time.TimebarLetter;
                } else {
                    progressBar += " ";
                }

            }

            progressBar = Themes.sColor(progressBar, Themes.CurrentTheme.Time.TimebarColor);
            progressBar += Themes.sColor("| ", Themes.CurrentTheme.Time.TimebarColor) + Funcs.CalculateTime(max, true) + extraVolume;

            return progressBar;
        }

        static public void DrawHelp() {
            var table = new Table();
            table.Border = Themes.bStyle(Themes.CurrentTheme.GeneralHelp.BorderStyle);
            table.BorderColor(Themes.bColor(Themes.CurrentTheme.GeneralHelp.BorderColor));
            table.Width = Start.consoleWidth;

            char separator = '+';
            string[] ToMainMenu =              (Keybindings.ToMainMenu).Replace(" ", "").Split(separator);
            string[] AddSongToPlaylist =       (Keybindings.AddSongToPlaylist).Replace(" ", "").Split(separator);
            string[] ShowSongsInPlaylists =    (Keybindings.ShowSongsInPlaylists).Replace(" ", "").Split(separator);
            string[] ListAllPlaylists =        (Keybindings.ListAllPlaylists).Replace(" ", "").Split(separator);
            string[] PlayOtherPlaylist =       (Keybindings.PlayOtherPlaylist).Replace(" ", "").Split(separator);
            string[] SaveCurrentPlaylist =     (Keybindings.SaveCurrentPlaylist).Replace(" ", "").Split(separator);
            string[] SaveAsPlaylist =          (Keybindings.SaveAsPlaylist).Replace(" ", "").Split(separator);
            string[] ShufflePlaylist =         (Keybindings.ShufflePlaylist).Replace(" ", "").Split(separator);
            string[] PlaySong =                (Keybindings.PlaySong).Replace(" ", "").Split(separator);
            string[] RedownloadCurrentSong =   (Keybindings.RedownloadCurrentSong).Replace(" ", "").Split(separator);
            string[] PlayPause =               (Keybindings.PlayPause).Replace(" ", "").Split(separator);
            string[] Quit =                    (Keybindings.Quit).Replace(" ", "").Split(separator);
            string[] Backwards5s =             (Keybindings.Backwards5s).Replace(" ", "").Split(separator);
            string[] Forward5s =               (Keybindings.Forward5s).Replace(" ", "").Split(separator);
            string[] VolumeUp =                (Keybindings.VolumeUp).Replace(" ", "").Split(separator);
            string[] VolumeDown =              (Keybindings.VolumeDown).Replace(" ", "").Split(separator);
            string[] Loop =                    (Keybindings.Loop).Replace(" ", "").Split(separator);
            string[] Mute =                    (Keybindings.Mute).Replace(" ", "").Split(separator);
            string[] Shuffle =                 (Keybindings.Shuffle).Replace(" ", "").Split(separator);
            string[] NextSong =                (Keybindings.NextSong).Replace(" ", "").Split(separator);
            string[] PreviousSong =            (Keybindings.PreviousSong).Replace(" ", "").Split(separator);
            string[] PlayRandomSong =          (Keybindings.PlayRandomSong).Replace(" ", "").Split(separator);
            string[] DeleteCurrentSong =       (Keybindings.DeleteCurrentSong).Replace(" ", "").Split(separator);
            string[] PlaylistOptions =         (Keybindings.PlaylistOptions).Replace(" ", "").Split(separator);
            string[] CommandHelpScreen =       (Keybindings.CommandHelpScreen).Replace(" ", "").Split(separator);
            string[] EditKeybindings =         (Keybindings.EditKeybindings).Replace(" ", "").Split(separator);
            string[] ChangeLanguage =          (Keybindings.ChangeLanguage).Replace(" ", "").Split(separator);
            string[] ChangeTheme =             (Keybindings.ChangeTheme).Replace(" ", "").Split(separator);
            string[] SearchFromYoutube =       (Keybindings.SearchFromYoutube).Replace(" ", "").Split(separator);


            table.AddColumns(Themes.sColor(Locale.Help.Controls, Themes.CurrentTheme.GeneralHelp.HeaderTextColor), Themes.sColor(Locale.Help.Description, Themes.CurrentTheme.GeneralHelp.HeaderTextColor), Themes.sColor(Locale.Help.ModControls, Themes.CurrentTheme.GeneralHelp.HeaderTextColor), Themes.sColor(Locale.Help.Description, Themes.CurrentTheme.GeneralHelp.HeaderTextColor));


            table.AddRow(DrawHelpTextColouring(PlayPause), Themes.sColor(Locale.Help.PlayPause, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),                                               DrawHelpTextColouring(AddSongToPlaylist), Themes.sColor(Locale.Help.AddsongToPlaylist, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor));
            table.AddRow(DrawHelpTextColouring(Quit), Themes.sColor(Locale.Help.Quit, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),                                                         DrawHelpTextColouring(ShowSongsInPlaylists), Themes.sColor(Locale.Help.ListAllSongsInOtherPlaylist, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor));
            table.AddRow(DrawHelpTextColouring(Backwards5s), $"{Themes.sColor(Locale.Help.Rewind +" "+ $"{Preferences.GetRewindSeconds()}", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)} {Themes.sColor(Locale.Help.Seconds, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)}",  
                                                                                                                                                            DrawHelpTextColouring(ListAllPlaylists), Themes.sColor(Locale.Help.ListAllPlaylists, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor));
            table.AddRow(DrawHelpTextColouring(Forward5s), $"{Themes.sColor(Locale.Help.Forward +" "+ $"{Preferences.GetForwardSeconds()}", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)} {Themes.sColor(Locale.Help.Seconds, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor)}",   
                                                                                                                                                            DrawHelpTextColouring(PlayOtherPlaylist), Themes.sColor(Locale.Help.PlayOtherPlaylist, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor));
            table.AddRow(DrawHelpTextColouring(VolumeUp), Themes.sColor(Locale.Help.VolumeUp, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),                                                 DrawHelpTextColouring(SaveCurrentPlaylist), Themes.sColor(Locale.Help.SavePlaylist, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor));
            table.AddRow(DrawHelpTextColouring(VolumeDown), Themes.sColor(Locale.Help.VolumeDown, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),                                             DrawHelpTextColouring(SaveAsPlaylist), Themes.sColor(Locale.Help.SaveAs, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor));
            table.AddRow(DrawHelpTextColouring(Loop), Themes.sColor(Locale.Help.ToggleLooping, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),                                                DrawHelpTextColouring(ShufflePlaylist), Themes.sColor(Locale.Help.ShufflePlaylist, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor));
            table.AddRow(DrawHelpTextColouring(Mute), Themes.sColor(Locale.Help.ToggleMute, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),                                                       DrawHelpTextColouring(PlaySong), Themes.sColor(Locale.Help.PlaySongs, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor));
            table.AddRow(DrawHelpTextColouring(Shuffle), Themes.sColor(Locale.Help.ToggleShuffle, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),                                             DrawHelpTextColouring(RedownloadCurrentSong), Themes.sColor(Locale.Help.RedownloadCurrentSong, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor));

            table.AddRow(Themes.sColor(Locale.Help.Playlist, Themes.CurrentTheme.GeneralHelp.HeaderTextColor), "" ,                                                                                  DrawHelpTextColouring(EditKeybindings), Themes.sColor(Locale.Help.EditKeybinds, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor));
            table.AddRow(DrawHelpTextColouring(NextSong), Themes.sColor(Locale.Help.NextSong, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),                                                 DrawHelpTextColouring(ChangeLanguage), Themes.sColor(Locale.Help.ChangeLanguage, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor));
            table.AddRow(DrawHelpTextColouring(PreviousSong), Themes.sColor(Locale.Help.PreviousSong, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),                                          DrawHelpTextColouring(ChangeTheme), Themes.sColor("Change Theme", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor));
            table.AddRow(DrawHelpTextColouring(PlayRandomSong), Themes.sColor(Locale.Help.PlayRandomSong, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor),                                      DrawHelpTextColouring(SearchFromYoutube), Themes.sColor("Search From Youtube", Themes.CurrentTheme.GeneralHelp.DescriptionTextColor));
            table.AddRow(DrawHelpTextColouring(DeleteCurrentSong), Themes.sColor(Locale.Help.DeleteCurrentSongFromPlaylist, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor));
            table.AddRow(DrawHelpTextColouring(PlaylistOptions), Themes.sColor(Locale.Help.ShowPlaylistOptions, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor));
            table.AddRow(DrawHelpTextColouring(CommandHelpScreen), Themes.sColor(Locale.Help.ShowCmdHelp, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor));
            table.AddRow(DrawHelpTextColouring(ToMainMenu), Themes.sColor(Locale.Help.ToMainMenu, Themes.CurrentTheme.GeneralHelp.DescriptionTextColor));

            AnsiConsole.Cursor.SetPosition(0, 0);
            AnsiConsole.Write(table);
            DrawHelpSettingInfo();
        }
        
        static private string DrawHelpTextColouring(string[] textArray){
            if(textArray.Length == 1){
                return Themes.sColor(textArray[0], Themes.CurrentTheme.GeneralHelp.ControlTextColor);
            }
            else if(textArray.Length == 2){
                // return $"[green1]{textArray[0]}[/] + {textArray[1]}";
                return 
                Themes.sColor(textArray[0], Themes.CurrentTheme.GeneralHelp.ModifierTextColor_1)
                + " + "
                + Themes.sColor(textArray[1], Themes.CurrentTheme.GeneralHelp.ControlTextColor);
            }
            else if(textArray.Length == 3){
                // return $"[green1]{textArray[0]}[/] + [turquoise2]{textArray[1]}[/] + {textArray[2]}";
                return
                Themes.sColor(textArray[0], Themes.CurrentTheme.GeneralHelp.ModifierTextColor_1)
                + " + "
                + Themes.sColor(textArray[1], Themes.CurrentTheme.GeneralHelp.ModifierTextColor_2)
                + " + "
                + Themes.sColor(textArray[2], Themes.CurrentTheme.GeneralHelp.ControlTextColor);
            }
            else if(textArray.Length == 4){
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
            else {
                // return textArray[0];
                return Themes.sColor(textArray[0], Themes.CurrentTheme.GeneralHelp.ControlTextColor);
            }
        } 
        static public void DrawSettings() {
            string ForwardSecondAmount = (Keybindings.ForwardSecondAmount);
            string BackwardSecondAmount = (Keybindings.BackwardSecondAmount);
            string ChangeVolumeAmount = (Keybindings.ChangeVolumeAmount);
            string Autosave = (Keybindings.Autosave);

            var table = new Table();
            table.Border = Themes.bStyle(Themes.CurrentTheme.GeneralSettings.BorderStyle);
            table.BorderColor(Themes.bColor(Themes.CurrentTheme.GeneralSettings.BorderColor));
            table.Width = Start.consoleWidth;

            table.AddColumns(Themes.sColor(Locale.Settings._Settings, Themes.CurrentTheme.GeneralSettings.SettingTextColor), Themes.sColor(Locale.Settings.Value, Themes.CurrentTheme.GeneralSettings.HeaderTextColor), Themes.sColor(Locale.Settings.ChangeValue, Themes.CurrentTheme.GeneralSettings.HeaderTextColor));
            
            table.AddRow(Themes.sColor(Locale.Settings.Forwardseconds, Themes.CurrentTheme.GeneralSettings.SettingTextColor) , Themes.sColor(Preferences.forwardSeconds + " sec", Themes.CurrentTheme.GeneralSettings.SettingValueColor)                                                     , Themes.sColor($"{ForwardSecondAmount} ", Themes.CurrentTheme.GeneralSettings.SettingChangeValueValueColor)            + Themes.sColor($"{Locale.Settings.ToChange}", Themes.CurrentTheme.GeneralSettings.SettingChangeValueColor));
            table.AddRow(Themes.sColor(Locale.Settings.Rewindseconds, Themes.CurrentTheme.GeneralSettings.SettingTextColor)  , Themes.sColor(Preferences.rewindSeconds + " sec", Themes.CurrentTheme.GeneralSettings.SettingValueColor)                                                      , Themes.sColor($"{BackwardSecondAmount} ", Themes.CurrentTheme.GeneralSettings.SettingChangeValueValueColor)           + Themes.sColor($"{Locale.Settings.ToChange}", Themes.CurrentTheme.GeneralSettings.SettingChangeValueColor));
            table.AddRow(Themes.sColor(Locale.Settings.ChangeVolumeBy, Themes.CurrentTheme.GeneralSettings.SettingTextColor) , Themes.sColor(Preferences.changeVolumeBy * 100 + " %", Themes.CurrentTheme.GeneralSettings.SettingValueColor)                                                 , Themes.sColor($"{ChangeVolumeAmount} ", Themes.CurrentTheme.GeneralSettings.SettingChangeValueValueColor)             + Themes.sColor($"{Locale.Settings.ToChange}", Themes.CurrentTheme.GeneralSettings.SettingChangeValueColor));
            table.AddRow(Themes.sColor(Locale.Settings.AutoSave, Themes.CurrentTheme.GeneralSettings.SettingTextColor)       , Themes.sColor(Preferences.isAutoSave ? Locale.Miscellaneous.True : Locale.Miscellaneous.False + "", Themes.CurrentTheme.GeneralSettings.SettingValueColor)    , Themes.sColor($"{Autosave} ", Themes.CurrentTheme.GeneralSettings.SettingChangeValueValueColor)                       + Themes.sColor($"{Locale.Settings.ToToggle}", Themes.CurrentTheme.GeneralSettings.SettingChangeValueColor));
            table.AddRow(Themes.sColor("Load Effects", Themes.CurrentTheme.GeneralSettings.SettingTextColor)                 , ""                                                                                                                                                            , Themes.sColor($"{Keybindings.LoadEffects} ", Themes.CurrentTheme.GeneralSettings.SettingChangeValueValueColor)        + Themes.sColor($"{"To Load Effects settings"}", Themes.CurrentTheme.GeneralSettings.SettingChangeValueColor));
            table.AddRow(Themes.sColor("Toggle Media Buttons", Themes.CurrentTheme.GeneralSettings.SettingTextColor)         , Themes.sColor(Preferences.isMediaButtons ? Locale.Miscellaneous.True : Locale.Miscellaneous.False + "", Themes.CurrentTheme.GeneralSettings.SettingValueColor), Themes.sColor($"{Keybindings.ToggleMediaButtons} ", Themes.CurrentTheme.GeneralSettings.SettingChangeValueValueColor) + Themes.sColor($"{"To Toggle Media Buttons"}", Themes.CurrentTheme.GeneralSettings.SettingChangeValueColor));
            table.AddRow(Themes.sColor("Toggle Visualizer", Themes.CurrentTheme.GeneralSettings.SettingTextColor)            , Themes.sColor(Preferences.isVisualizer ? Locale.Miscellaneous.True : Locale.Miscellaneous.False + "", Themes.CurrentTheme.GeneralSettings.SettingValueColor)  , Themes.sColor($"{Keybindings.ToggleVisualizer} ", Themes.CurrentTheme.GeneralSettings.SettingChangeValueValueColor)   + Themes.sColor($"{"To Toggle Visualizer (change visualizer settings in Visualizer.ini)"}", Themes.CurrentTheme.GeneralSettings.SettingChangeValueColor));
            table.AddRow(Themes.sColor("Load Visualizer", Themes.CurrentTheme.GeneralSettings.SettingTextColor)              , ""                                                                                                                                                            , Themes.sColor($"{Keybindings.LoadVisualizer} ", Themes.CurrentTheme.GeneralSettings.SettingChangeValueValueColor)     + Themes.sColor($"{"To Load Visualizer settings"}", Themes.CurrentTheme.GeneralSettings.SettingChangeValueColor));
            AnsiConsole.Cursor.SetPosition(0, 0);
            AnsiConsole.Write(table);
            DrawHelpSettingInfo();
        }
        
        private static void DrawHelpSettingInfo(){
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
        
        public static void CliHelp() {
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
            table.AddRow($"[grey]jammer[/] [green]-v[/][grey],[/][green] --version[/]", $"{Locale.CliHelp.ShowJammerVersion} [grey]" + Utils.version + "[/]");
            AnsiConsole.Write(table);

            PlaylistHelp();
        }
        static public void PlaylistHelp() {
            var table = new Table();
            table.AddColumn(Locale.CliHelp.PlaylistCommands);
            table.AddColumn(Locale.CliHelp.Description);

            table.AddRow($"[grey]jammer[/] [red]-h[/][grey],[/][red] --help [/] ", "show this help message"); // TODO ADD LOCALE
            table.AddRow($"[grey]jammer[/] [red]-p[/][grey],[/][red] --play  [/] <{Locale.CliHelp.Name}>", Locale.CliHelp.PlayPlaylist);
            table.AddRow($"[grey]jammer[/] [red]-c[/][grey],[/][red] --create[/] <{Locale.CliHelp.Name}>", Locale.CliHelp.CreatePlaylist);
            table.AddRow($"[grey]jammer[/] [red]-d[/][grey],[/][red] --delete[/] <{Locale.CliHelp.Name}>", Locale.CliHelp.DeletePlaylist);
            table.AddRow($"[grey]jammer[/] [red]-a[/][grey],[/][red] --add   [/] <{Locale.CliHelp.Name}> <song> ...", Locale.CliHelp.AddSongsToPlaylist);
            table.AddRow($"[grey]jammer[/] [red]-r[/][grey],[/][red] --remove[/] <{Locale.CliHelp.Name}> <{Locale.CliHelp.Song}> ...", Locale.CliHelp.RemoveSongsFromPlaylist);
            table.AddRow($"[grey]jammer[/] [red]-s[/][grey],[/][red] --show  [/] <{Locale.CliHelp.Name}>", Locale.CliHelp.ShowSongsInPlaylist);
            table.AddRow($"[grey]jammer[/] [red]-l[/][grey],[/][red] --list  [/] ", Locale.CliHelp.ListAllPlaylists);
            table.AddRow($"[grey]jammer[/] [red]-f[/][grey],[/][red] --flush [/] ", "delete all songs from the jammer/songs folder"); // TODO ADD LOCALE
            table.AddRow($"[grey]jammer[/] [red]-sp[/][grey],[/][red] --set-path [/] <path>, <default>", "Set the path to the jammer/songs folder"); // TODO ADD LOCALE
            table.AddRow($"[grey]jammer[/] [red]-gp[/][grey],[/][red] --get-path [/] ", "get the path to the jammer/songs folder"); // TODO ADD LOCALE
            table.AddRow($"[grey]jammer[/] [red]-hm[/][grey],[/][red] --home [/] ", "play all songs from the jammer/songs folder"); // TODO ADD LOCALE
            table.AddRow($"[grey]jammer[/] [red]-so[/][grey],[/][red] --songs [/] ", "open jammer/songs folder"); // TODO ADD LOCALE
            AnsiConsole.Write(table);
        }
        public static void Version() {
            AnsiConsole.MarkupLine($"[green]Jammer {Locale.Miscellaneous.Version} " + Utils.version + "[/]");
        }

        public static void EditKeyBindings(){
            IniFileHandling.Create_KeyDataIni(0);

            var table = new Table();
            table.Border = Themes.bStyle(Themes.CurrentTheme.EditKeybinds.BorderStyle);
            table.BorderColor(Themes.bColor(Themes.CurrentTheme.EditKeybinds.BorderColor));

            table.AddColumn(Locale.Help.Description);
            table.AddColumn(Locale.LocaleKeybind.CurrentControl);
            (string[] _elements, string[] _description) = IniFileHandling.ReadAll_KeyData();

            // Counter to track the index for the description array

            // Loop through the _elements array
            for(int i = 0; i < _elements.Length; i++) {
                // Check if the description at descIndex is not empty
                // Add row to the table
                if(i == 0){
                    table.AddRow(Themes.sColor(_description[i], Themes.CurrentTheme.EditKeybinds.CurrentKeyColor), Themes.sColor(_elements[i], Themes.CurrentTheme.EditKeybinds.CurrentKeyColor));
                } else {
                    table.AddRow(Themes.sColor(_description[i], Themes.CurrentTheme.EditKeybinds.DescriptionColor), Themes.sColor(_elements[i], Themes.CurrentTheme.EditKeybinds.CurrentControlColor));
                }
            }

            AnsiConsole.Cursor.SetPosition(0, 0);
            AnsiConsole.Cursor.Hide();
            AnsiConsole.Write(table);
            AnsiConsole.Cursor.Show();

            if(IniFileHandling.EditingKeybind){
                string final = IniFileHandling.previousClick.ToString();
                if(IniFileHandling.isShiftCtrlAlt){
                    final = "Shift + Ctrl + Alt + " + final;
                }
                else if(IniFileHandling.isShiftCtrl){
                    final = "Shift + Ctrl + " + final;
                }
                else if(IniFileHandling.isShiftAlt){
                    final = "Shift + Alt + " + final;
                }
                else if(IniFileHandling.isCtrlAlt){
                    final = "Ctrl + Alt + " + final;
                }
                else if(IniFileHandling.isShift){
                    final = "Shift + " + final;
                }
                else if(IniFileHandling.isCtrl){
                    final = "Ctrl + " + final;
                }
                else if(IniFileHandling.isAlt){
                    final = "Alt + " + final;
                }
                AnsiConsole.Markup(Themes.sColor($"{Locale.LocaleKeybind.EditKeyBindMessage1} {Keybindings.Choose}\n", Themes.CurrentTheme.Playlist.InfoColor));
                AnsiConsole.Markup(Themes.sColor($"{Locale.LocaleKeybind.EditKeyBindMessage2}\n", Themes.CurrentTheme.Playlist.InfoColor));
                AnsiConsole.Markup(Themes.sColor($"{final}\n", Themes.CurrentTheme.EditKeybinds.EnteredKeyColor));

            } else {
                AnsiConsole.Markup(Themes.sColor($"{Locale.LocaleKeybind.EditKeyBindMessage3}{Keybindings.PlaylistViewScrollup}, {Keybindings.PlaylistViewScrolldown}\n", Themes.CurrentTheme.Playlist.InfoColor)); // Press Enter to edit); // Press Enter to edit
                AnsiConsole.Markup(Themes.sColor($"{Locale.LocaleKeybind.EditKeyBindMessage4}\n", Themes.CurrentTheme.Playlist.InfoColor));
            }
            DrawHelpSettingInfo();
        }
        public static void ChangeLanguage(){
            var table = new Table();
            table.Border = Themes.bStyle(Themes.CurrentTheme.LanguageChange.BorderStyle);
            table.BorderColor(Themes.bColor(Themes.CurrentTheme.LanguageChange.BorderColor));
            table.AddColumn(Locale.LocaleKeybind.Description);
            string[] _elements = IniFileHandling.ReadAll_Locales();
            
            // Loop through the _elements array
            for(int i = 0; i < _elements.Length; i++) {
                if(i==0){
                    table.AddRow(Themes.sColor(_elements[i], Themes.CurrentTheme.LanguageChange.CurrentLanguageColor));
                } else {
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

        public static void RefreshCurrentView() {
            //NOTE(ra) This Clear() caused flickering.
            /* AnsiConsole.Clear(); */
            if (Start.playerView == "default") {
                DrawPlayer();
            }
            else if (Start.playerView == "help") {
                DrawHelp();
            }
            else if (Start.playerView == "settings") {
                DrawSettings();
            }
            else if (Start.playerView == "all") {
                DrawPlayer();
            }
            else if (Start.playerView == "fake") {
                DrawPlayer();
            }
            else if (Start.playerView == "editkeybindings") {
                EditKeyBindings();
            }
            else if (Start.playerView == "changelanguage") {
                ChangeLanguage();
            }
        }
    }
}
