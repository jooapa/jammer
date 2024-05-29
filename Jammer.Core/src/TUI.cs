using Spectre.Console;

namespace Jammer {
    public static class TUI {

        static bool cls = false;
        
        static public void DrawPlayer() {
            try {
                var ansiConsoleSettings = new AnsiConsoleSettings();
                var ansiConsole = AnsiConsole.Create(ansiConsoleSettings);
                if (Start.playerView == "help" || Start.playerView == "settings")
                {
                    return;
                }
                var mainTable = new Table();
                var songsTable = new Table();
                var controlsTable = new Table();
                var timeTable = new Table();

                if (Start.playerView == "default") {
                    UIComponent_Normal(songsTable);
                }
                else if (Start.playerView == "all") {
                    UIComponent_Songs(songsTable);
                }

                UIComponent_Controls(controlsTable);

                if (cls) {
                    if (Start.playerView != "all") {
                        AnsiConsole.Clear();
                        Debug.dprint("DrawPlayer - clear");
                    }
                    cls = false;
                }
                if (Start.playerView == "default" || Start.playerView == "fake") {
                    AnsiConsole.Cursor.SetPosition(0, 0);
                }

                // render maintable with tables in it
                mainTable.AddColumns(Funcs.GetSongWithdots(Start.Sanitize(Utils.currentSong), Start.consoleWidth - 8)).Width(Start.consoleWidth);
                mainTable.AddRow(songsTable.Centered().Width(Start.consoleWidth));
                songsTable.Border = TableBorder.Rounded;
                
                mainTable.Border = TableBorder.Rounded;

                // add \n to the end of the maintable until the end of the console by height
                // int tableRowCount;
                // if (Start.playerView != "all") {
                //     if (Utils.currentPlaylist == "")
                //         tableRowCount = Start.consoleHeight - 17;
                //     else
                //         tableRowCount = Start.consoleHeight - 18;
                // }
                // else {
                //     if (Utils.currentPlaylist == "")
                //         tableRowCount = Start.consoleHeight - 21;
                //     else
                //         tableRowCount = Start.consoleHeight - 22;
                // }

                // if (tableRowCount < 0) {
                //     tableRowCount = 0;
                // }

                // for (int i = 0; i < tableRowCount; i++) {
                //     mainTable.AddEmptyRow();
                // }

                var helpTable = new Table();
                helpTable.Border = TableBorder.Rounded;
                helpTable.AddColumn($"[red]{Keybindings.Help}[/] {Locale.Player.ForHelp} | [yellow]{Keybindings.Settings}[/] {Locale.Help.ForSettings} | [green]{Keybindings.ShowHidePlaylist}[/] {Locale.Player.ForPlaylist}");
                mainTable.AddRow(helpTable);

                var displayTable = new Table();
                displayTable.Border = TableBorder.None;
                displayTable.AddColumn(Visual.GetSongVisual()).Centered();
                
                mainTable.AddRow(displayTable);
                mainTable.AddRow(UIComponent_Time(timeTable, Start.consoleWidth - 20).Centered());

                // render the main table
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

        static public void ClearScreen() {
            cls = true;
        }
            // "Components" of the Funcs
        static public void UIComponent_Controls(Table table) {
            table.Border = TableBorder.Rounded;
            table.Border = TableBorder.Simple;
            table.Alignment(Justify.Left);
            table.AddColumn(Locale.Player.Looping);
            table.AddColumn(Locale.Player.Shuffle);
            table.AddColumn(Locale.Player.Volume);
            string volume = Preferences.isMuted ? "[grey][strikethrough]" + Math.Round(Preferences.oldVolume * 100) + "%[/][/]" : Math.Round(Preferences.volume * 100) + "%";
            
            // TODO ADD STATE TO LOCALE
            table.AddRow(new Markup(Preferences.isLoop ? $"[green]{Locale.Miscellaneous.On}[/]" : $"[red]{Locale.Miscellaneous.Off}[/]").Centered(), 
                        new Markup(Preferences.isShuffle ? $"[green]{Locale.Miscellaneous.On}[/]" : $"[red]{Locale.Miscellaneous.Off}[/]").Centered(), 
                        new Markup(volume).Centered());
        }

        static public string GetStateLogo() {
            string state = Start.state.ToString();
            if (Start.state == MainStates.playing || Start.state == MainStates.play) {
                state = "❚❚";
            }
            else if (Start.state == MainStates.idle || Start.state == MainStates.pause) {
                state = "▶ ";
            }
            else if (Start.state == MainStates.stop) {
                state = "■";
            }
            else if (Start.state == MainStates.next) {
                state = "▶▶";
            }
            else if (Start.state == MainStates.previous) {
                state = "◀◀";
            }

            return state;
        }
        
        static public void UIComponent_Songs(Table table) {
            // AnsiConsole.Clear();
            string[] queueLines = Funcs.GetAllSongsQueue();
            string[] lines = Funcs.GetAllSongs();

            if (Utils.currentPlaylist == "") {
                table.AddColumn(Locale.OutsideItems.CurrentPlaylist);
            } else {
                table.AddColumn(Locale.OutsideItems.CurrentPlaylist + " [cyan]" + Utils.currentPlaylist + "[/]");
            }

            table.AddColumn(Locale.OutsideItems.CurrentQueue);
            for(int i = 0; i < lines.Length; i++){
                table.AddRow(lines[i], queueLines.Length > i ? queueLines[i] : "");
            }
        }

        static public void UIComponent_Normal(Table table) {
            if (Utils.currentPlaylist == "") {
                table.AddColumn(Funcs.GetPrevCurrentNextSong());
            } else {
                table.AddColumn($"{Locale.Player.Playlist} [cyan]" + Utils.currentPlaylist + "[/]");
                table.AddRow(Funcs.GetPrevCurrentNextSong());
            }
        }

        public static Table UIComponent_Time(Table table, int? length = 100) {
            table.Border = TableBorder.Rounded;
            table.AddColumn(ProgressBar(Utils.MusicTimePlayed, Utils.currentMusicLength, length));
            return table;
        }

        public static string ProgressBar(double value, double max, int? length = 100) {
            if (length == null) {
                length = 100;
            }

            string volumeMark = Preferences.isMuted ? "[grey][strikethrough]" + Math.Round(Preferences.oldVolume * 100) + "%[/][/]" : Math.Round(Preferences.volume * 100) + "%";
            string volumeString = Preferences.isMuted ? Math.Round(Preferences.oldVolume * 100) + "%" : Math.Round(Preferences.volume * 100) + "%";

            string shuffleMark = Preferences.isShuffle ? "[green]" + "⇌" + "[/]" : "[red]" + "⇌" + "[/]";
            string loopMark = Preferences.isLoop ? "[green]" + "⟳" + "[/]" : "[red]" + "↻" + "[/]";

            int progress = (int)(value / max * length);
            string progressBar = GetStateLogo() + " " + shuffleMark + "  " + loopMark + "   " +
                Funcs.CalculateTime(value) + " |";
            // length is modified also by the time string
            length -= Funcs.CalculateTime(value).Length;
            length -= GetStateLogo().Length;
            length -= 4;

            string extraVolume;
            if (volumeString.Length >= 4) {
                extraVolume = " " + volumeMark;
            } else if (volumeString.Length == 3) {
                extraVolume = "  " + volumeMark;
            } else {
                extraVolume = "   " + volumeMark;
            }

            length -= extraVolume.Length;

            for (int i = 0; i < length; i++) {
                if (i < progress) {
                    progressBar += "█";
                }
                else {
                    progressBar += " ";
                }
            }


            progressBar += "| " + Funcs.CalculateTime(max) + extraVolume;

            return progressBar;
        }

        static public void DrawHelp() {
            var table = new Table();
            char separator = '+';
            string[] ToMainMenu = (Keybindings.ToMainMenu).Replace(" ", "").Split(separator);
            string[] AddSongToPlaylist = (Keybindings.AddSongToPlaylist).Replace(" ", "").Split(separator);
            string[] ShowSongsInPlaylists = (Keybindings.ShowSongsInPlaylists).Replace(" ", "").Split(separator);
            string[] ListAllPlaylists = (Keybindings.ListAllPlaylists).Replace(" ", "").Split(separator);
            string[] PlayOtherPlaylist = (Keybindings.PlayOtherPlaylist).Replace(" ", "").Split(separator);
            string[] SaveCurrentPlaylist = (Keybindings.SaveCurrentPlaylist).Replace(" ", "").Split(separator);
            string[] SaveAsPlaylist = (Keybindings.SaveAsPlaylist).Replace(" ", "").Split(separator);
            string[] ShufflePlaylist = (Keybindings.ShufflePlaylist).Replace(" ", "").Split(separator);
            string[] PlaySong = (Keybindings.PlaySong).Replace(" ", "").Split(separator);
            string[] RedownloadCurrentSong = (Keybindings.RedownloadCurrentSong).Replace(" ", "").Split(separator);
            string[] PlayPause = (Keybindings.PlayPause).Replace(" ", "").Split(separator);
            string[] Quit = (Keybindings.Quit).Replace(" ", "").Split(separator);
            string[] Backwards5s = (Keybindings.Backwards5s).Replace(" ", "").Split(separator);
            string[] Forward5s = (Keybindings.Forward5s).Replace(" ", "").Split(separator);
            string[] VolumeUp = (Keybindings.VolumeUp).Replace(" ", "").Split(separator);
            string[] VolumeDown = (Keybindings.VolumeDown).Replace(" ", "").Split(separator);
            string[] Loop = (Keybindings.Loop).Replace(" ", "").Split(separator);
            string[] Mute = (Keybindings.Mute).Replace(" ", "").Split(separator);
            string[] Shuffle = (Keybindings.Shuffle).Replace(" ", "").Split(separator);
            string[] NextSong = (Keybindings.NextSong).Replace(" ", "").Split(separator);
            string[] PreviousSong = (Keybindings.PreviousSong).Replace(" ", "").Split(separator);
            string[] PlayRandomSong = (Keybindings.PlayRandomSong).Replace(" ", "").Split(separator);
            string[] DeleteCurrentSong = (Keybindings.DeleteCurrentSong).Replace(" ", "").Split(separator);
            string[] PlaylistOptions = (Keybindings.PlaylistOptions).Replace(" ", "").Split(separator);
            string[] CommandHelpScreen = (Keybindings.CommandHelpScreen).Replace(" ", "").Split(separator);
            string[] EditKeybindings = (Keybindings.EditKeybindings).Replace(" ", "").Split(separator);
            string[] ChangeLanguage = (Keybindings.ChangeLanguage).Replace(" ", "").Split(separator);


            table.AddColumns(Locale.Help.Controls, Locale.Help.Description,Locale.Help.ModControls,Locale.Help.Description);


            table.AddRow(DrawHelpTextColouring(PlayPause), Locale.Help.PlayPause,                                               DrawHelpTextColouring(AddSongToPlaylist), Locale.Help.AddsongToPlaylist);
            table.AddRow(DrawHelpTextColouring(Quit), Locale.Help.Quit,                                                         DrawHelpTextColouring(ShowSongsInPlaylists), Locale.Help.ListAllSongsInOtherPlaylist);
            table.AddRow(DrawHelpTextColouring(Backwards5s), $"{Locale.Help.Rewind} {Preferences.changeVolumeBy * 100} {Locale.Help.Seconds}",  
                                                                                                                                                            DrawHelpTextColouring(ListAllPlaylists), Locale.Help.ListAllPlaylists);
            table.AddRow(DrawHelpTextColouring(Forward5s), $"{Locale.Help.Forward} {Preferences.changeVolumeBy * 100} {Locale.Help.Seconds}",   
                                                                                                                                                            DrawHelpTextColouring(PlayOtherPlaylist), Locale.Help.PlayOtherPlaylist);
            table.AddRow(DrawHelpTextColouring(VolumeUp), Locale.Help.VolumeUp,                                                 DrawHelpTextColouring(SaveCurrentPlaylist), Locale.Help.SavePlaylist);
            table.AddRow(DrawHelpTextColouring(VolumeDown), Locale.Help.VolumeDown,                                             DrawHelpTextColouring(SaveAsPlaylist), Locale.Help.SaveAs);
            table.AddRow(DrawHelpTextColouring(Loop), Locale.Help.ToggleLooping,                                                DrawHelpTextColouring(ShufflePlaylist), Locale.Help.ShufflePlaylist);
            table.AddRow(DrawHelpTextColouring(Mute), Locale.Help.ToggleMute,                                                       DrawHelpTextColouring(PlaySong), Locale.Help.PlaySongs);
            table.AddRow(DrawHelpTextColouring(Shuffle), Locale.Help.ToggleShuffle,                                             DrawHelpTextColouring(RedownloadCurrentSong), Locale.Help.RedownloadCurrentSong);

            table.AddRow(Locale.Help.Playlist, "" ,DrawHelpTextColouring(EditKeybindings), Locale.Help.EditKeybinds);
            table.AddRow(DrawHelpTextColouring(NextSong), Locale.Help.NextSong, DrawHelpTextColouring(ChangeLanguage), Locale.Help.ChangeLanguage);
            table.AddRow(DrawHelpTextColouring(PreviousSong), Locale.Help.PreviousSong);
            table.AddRow(DrawHelpTextColouring(PlayRandomSong), Locale.Help.PlayRandomSong);
            table.AddRow(DrawHelpTextColouring(DeleteCurrentSong), Locale.Help.DeleteCurrentSongFromPlaylist);
            table.AddRow(DrawHelpTextColouring(PlaylistOptions), Locale.Help.ShowPlaylistOptions);
            table.AddRow(DrawHelpTextColouring(CommandHelpScreen), Locale.Help.ShowCmdHelp);
            table.AddRow(DrawHelpTextColouring(ToMainMenu), Locale.Help.ToMainMenu);

            AnsiConsole.Cursor.SetPosition(0, 0);
            AnsiConsole.Write(table);
            DrawHelpSettingInfo();
        }
        
        static private string DrawHelpTextColouring(string[] textArray){
            if(textArray.Length == 1){
                return textArray[0];
            }
            else if(textArray.Length == 2){
                return $"[green1]{textArray[0]}[/] + {textArray[1]}";
            }
            else if(textArray.Length == 3){
                return $"[green1]{textArray[0]}[/] + [turquoise2]{textArray[1]}[/] + {textArray[2]}";
            }
            else if(textArray.Length == 4){
                return $"[green1]{textArray[0]}[/] + [turquoise2]{textArray[1]}[/] + [blue]{textArray[2]}[/] + {textArray[3]}";
            } 
            else {
                return textArray[0];
            }
        } 
        static public void DrawSettings() {
            string ForwardSecondAmount = (Keybindings.ForwardSecondAmount);
            string BackwardSecondAmount = (Keybindings.BackwardSecondAmount);
            string ChangeVolumeAmount = (Keybindings.ChangeVolumeAmount);
            string Autosave = (Keybindings.Autosave);

            var table = new Table();
            table.AddColumns(Locale.Settings._Settings, Locale.Settings.Value, Locale.Settings.ChangeValue);
            table.AddRow(Locale.Settings.Forwardseconds, Preferences.forwardSeconds + " sec", $"[green]{ForwardSecondAmount}[/] {Locale.Settings.ToChange}");
            table.AddRow(Locale.Settings.Rewindseconds, Preferences.rewindSeconds + " sec", $"[green]{BackwardSecondAmount}[/] {Locale.Settings.ToChange}");
            table.AddRow(Locale.Settings.ChangeVolumeBy, Preferences.changeVolumeBy * 100 + " %", $"[green]{ChangeVolumeAmount}[/] {Locale.Settings.ToChange}");
            table.AddRow(Locale.Settings.AutoSave, Preferences.isAutoSave ? Locale.Miscellaneous.True : Locale.Miscellaneous.False + "", $"[green]{Autosave}[/] {Locale.Settings.ToToggle}");
            table.AddRow("Load Effects", "", $"[green]{Keybindings.LoadEffects}[/] {"To Load Effects (again)"}");
            table.AddRow("Toggle Media Buttons", Preferences.isMediaButtons ? Locale.Miscellaneous.True : Locale.Miscellaneous.False + "", $"[green]{Keybindings.ToggleMediaButtons}[/] {"To Toggle Media Buttons"}");
            AnsiConsole.Cursor.SetPosition(0, 0);
            AnsiConsole.Write(table);
            DrawHelpSettingInfo();
        }
        
        private static void DrawHelpSettingInfo(){
            AnsiConsole.Markup($"{Locale.Help.Press} [red]{Keybindings.Help}[/] {Locale.Help.ToHideHelp}");
            AnsiConsole.Markup($"\n{Locale.Help.Press} [yellow]{Keybindings.Settings}[/] {Locale.Help.ForSettings}");
            AnsiConsole.Markup($"\n{Locale.Help.Press} [green]{Keybindings.ShowHidePlaylist}[/] {Locale.Help.ToShowPlaylist}\n");
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

            table.AddRow($"[grey]jammer[/] [red]-p[/][grey],[/][red] --play  [/] <{Locale.CliHelp.Name}>", Locale.CliHelp.PlayPlaylist);
            table.AddRow($"[grey]jammer[/] [red]-c[/][grey],[/][red] --create[/] <{Locale.CliHelp.Name}>", Locale.CliHelp.CreatePlaylist);
            table.AddRow($"[grey]jammer[/] [red]-d[/][grey],[/][red] --delete[/] <{Locale.CliHelp.Name}>", Locale.CliHelp.DeletePlaylist);
            table.AddRow($"[grey]jammer[/] [red]-a[/][grey],[/][red] --add   [/] <{Locale.CliHelp.Name}> <song> ...", Locale.CliHelp.AddSongsToPlaylist);
            table.AddRow($"[grey]jammer[/] [red]-r[/][grey],[/][red] --remove[/] <{Locale.CliHelp.Name}> <{Locale.CliHelp.Song}> ...", Locale.CliHelp.RemoveSongsFromPlaylist);
            table.AddRow($"[grey]jammer[/] [red]-s[/][grey],[/][red] --show  [/] <{Locale.CliHelp.Name}>", Locale.CliHelp.ShowSongsInPlaylist);
            table.AddRow($"[grey]jammer[/] [red]-l[/][grey],[/][red] --list  [/] ", Locale.CliHelp.ListAllPlaylists);
            table.AddRow($"[grey]jammer[/] [red]-f[/][grey],[/][red] --flush [/] ", "Flush all songs from the jammer/songs folder"); // TODO ADD LOCALE
            table.AddRow($"[grey]jammer[/] [red]-sp[/][grey],[/][red] --set-path [/] <path>, <default>", "Set the path to the jammer/songs folder"); // TODO ADD LOCALE
            AnsiConsole.Write(table);
        }
        public static void Version() {
            AnsiConsole.MarkupLine($"[green]Jammer {Locale.Miscellaneous.Version} " + Utils.version + "[/]");
        }

        public static void EditKeyBindings(){
            IniFileHandling.Create_KeyDataIni(0);

            var table = new Table();
            table.AddColumn(Locale.Help.Description);
            table.AddColumn(Locale.LocaleKeybind.CurrentControl);
            (string[] _elements, string[] _description) = IniFileHandling.ReadAll_KeyData();

            // Counter to track the index for the description array

            // Loop through the _elements array
            for(int i = 0; i < _elements.Length; i++) {
                // Check if the description at descIndex is not empty
                // Add row to the table
                if(i == 0){
                    table.AddRow("[red]"+_description[i]+"[/]", "[red]"+_elements[i]+"[/]");
                } else {
                    table.AddRow(_description[i], _elements[i]);
                }
            }
            AnsiConsole.Cursor.SetPosition(0, 0);
            AnsiConsole.Write(table);
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
                AnsiConsole.Markup($"[green]{Locale.LocaleKeybind.EditKeyBindMessage1} {Keybindings.Choose}[/]\n");
                AnsiConsole.Markup($"{Locale.LocaleKeybind.EditKeyBindMessage2}\n");
                AnsiConsole.Markup($"[cyan]{final}[/]\n\n");

            } else {
                AnsiConsole.Markup($"[green]{Locale.LocaleKeybind.EditKeyBindMessage3}{Keybindings.PlaylistViewScrollup}, {Keybindings.PlaylistViewScrolldown}[/]\n"); // Press Enter to edit
                AnsiConsole.Markup($"[green]{Locale.LocaleKeybind.EditKeyBindMessage4}[/]\n");
            }
            DrawHelpSettingInfo();
        }
        public static void ChangeLanguage(){
            var table = new Table();
            table.AddColumn(Locale.LocaleKeybind.Description);
            string[] _elements = IniFileHandling.ReadAll_Locales();
            
            // Loop through the _elements array
            for(int i = 0; i < _elements.Length; i++) {
                if(i==0){
                    table.AddRow("[red]"+_elements[i]+"[/]");
                } else {
                    table.AddRow(_elements[i]);
                }
            }
            AnsiConsole.Cursor.SetPosition(0, 0);
            AnsiConsole.Write(table);
            AnsiConsole.Markup($"[green]{Locale.LocaleKeybind.ChangeLanguageMessage1} {Keybindings.PlaylistViewScrollup}, {Keybindings.PlaylistViewScrolldown}[/]\n");
            DrawHelpSettingInfo();
        }

        public static void RefreshCurrentView() {
            //NOTE(ra) This Clear() caused flickering.
            /* AnsiConsole.Clear(); */
            AnsiConsole.Cursor.Hide();
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
            AnsiConsole.Cursor.Show();
        }
    }
}
