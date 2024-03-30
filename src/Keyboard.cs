using ManagedBass;
using Spectre.Console;
using System.IO;

namespace jammer
{
    public partial class Start
    {
        public static string playerView = "default"; // default, all, help, settings, fake, editkeybindings, changelanguage
        public static void CheckKeyboard()
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                bool isAlt = IfHoldingDownALT(key);
                bool isCtrl = IfHoldingDownCTRL(key);
                bool isShift = IfHoldingDownSHIFT(key);
                bool isShiftAlt = IfHoldingDownSHIFTandALT(key);
                bool isShiftCtrl = IfHoldingDownSHIFTandCTRL(key);
                bool isCtrlAlt = IfHoldingDownCtrlandALT(key);
                bool isShiftCtrlAlt = IfHoldingDownSHIFTandCTRLandAlt(key);

                var pressed_key = key.Key;
                
                string Action = IniFileHandling.FindMatch_KeyData(
                    pressed_key,
                    isAlt,
                    isCtrl,
                    isShift,
                    isShiftAlt,
                    isShiftCtrl,
                    isCtrlAlt,
                    isShiftCtrlAlt
                    );
                // Media key presses
                /*
                switch(pressed_key){
                    case ConsoleKey.MediaPlay:
                        PauseSong();
                        Play.PlayDrawReset();
                        break;
                    case ConsoleKey.MediaStop:
                        PauseSong(true);
                        Play.PlayDrawReset();
                        break;
                    case ConsoleKey.MediaNext:
                        state = MainStates.next; // next song
                        break;
                    case ConsoleKey.MediaPrevious:
                        if (IfHoldingDownSHIFT(key))
                        {
                            TUI.PlaySingleSong();
                            break;
                        }
                        state = MainStates.previous; // previous song
                        break;
                }
                */
                if(playerView.Equals("editkeybindings") || IniFileHandling.EditingKeybind){
                    if(key.Key == ConsoleKey.Delete && isShiftAlt && !IniFileHandling.EditingKeybind){
                        IniFileHandling.Create_KeyDataIni(1);
                        Message.Data("Keybinds resetted","Keybinds have been resetted");
                    }

                    if(key.Key == ConsoleKey.DownArrow && !IniFileHandling.EditingKeybind){
                        // nullifu action
                        Action = "";
                        if(IniFileHandling.ScrollIndexKeybind + 1 > IniFileHandling.KeybindAmount){
                            IniFileHandling.ScrollIndexKeybind = -1;
                        } else {
                            IniFileHandling.ScrollIndexKeybind += 1;
                        }
                    } 
                    else if(key.Key == ConsoleKey.UpArrow && !IniFileHandling.EditingKeybind){
                        Action = "";
                        if(IniFileHandling.ScrollIndexKeybind - 1 < 0 ){
                            IniFileHandling.ScrollIndexKeybind = IniFileHandling.KeybindAmount - 1;
                        } else {
                            IniFileHandling.ScrollIndexKeybind -= 1;
                        }
                    } 
                    if (key.Key == ConsoleKey.Enter && !IniFileHandling.EditingKeybind){
                        Action = "";
                        IniFileHandling.EditingKeybind = true;
                    }
                    else if (key.Key == ConsoleKey.Enter && IniFileHandling.EditingKeybind){
                        Action = "";
                        IniFileHandling.EditingKeybind = false;
                        IniFileHandling.WriteIni_KeyData();
                        IniFileHandling.isAlt = false;
                        IniFileHandling.isCtrl = false;
                        IniFileHandling.isShift = false;
                        IniFileHandling.isShiftAlt = false;
                        IniFileHandling.isShiftCtrl = false;
                        IniFileHandling.isCtrlAlt = false;
                        IniFileHandling.isShiftCtrlAlt = false;
                    }
                    if (key.Key == ConsoleKey.Escape && isShift && IniFileHandling.EditingKeybind){
                        Action = "";
                        IniFileHandling.EditingKeybind = false;
                    }
                    
                    IniFileHandling.isAlt = IfHoldingDownALT(key);
                    IniFileHandling.isCtrl = IfHoldingDownCTRL(key);
                    IniFileHandling.isShift = IfHoldingDownSHIFT(key);
                    IniFileHandling.isShiftAlt = IfHoldingDownSHIFTandALT(key);
                    IniFileHandling.isShiftCtrl = IfHoldingDownSHIFTandCTRL(key);
                    IniFileHandling.isCtrlAlt = IfHoldingDownCtrlandALT(key);
                    IniFileHandling.isShiftCtrlAlt = IfHoldingDownSHIFTandCTRLandAlt(key);
                    IniFileHandling.previousClick = key.Key;

                    if(IniFileHandling.EditingKeybind){
                        Action = "";
                    }
                }
                if(playerView.Equals("changelanguage")){
                    // Message.Data("A", $"{IniFileHandling.ScrollIndexLanguage}");
                    if(key.Key == ConsoleKey.DownArrow){
                        Action = "";
                        if(IniFileHandling.ScrollIndexLanguage + 1 >= IniFileHandling.LocaleAmount){
                            IniFileHandling.ScrollIndexLanguage = 0;
                        } else {
                            IniFileHandling.ScrollIndexLanguage += 1;
                        }
                    }
                    if(key.Key == ConsoleKey.UpArrow){
                        Action = "";
                        if(IniFileHandling.ScrollIndexLanguage - 1 < 0 ){
                            IniFileHandling.ScrollIndexLanguage = IniFileHandling.LocaleAmount - 1;
                        } else {
                            IniFileHandling.ScrollIndexLanguage -= 1;
                        }
                    } 
                    if(key.Key == ConsoleKey.Enter){
                        IniFileHandling.Ini_LoadNewLocale();
                    }
                }

                if(playerView.Equals("all")){
                    if(Action == "PlaylistViewScrolldown"){
                        Action = "";
                        if(Utils.currentPlaylistSongIndex + 1 >= Utils.songs.Length){
                            Utils.currentPlaylistSongIndex = Utils.songs.Length -1;
                        } else {
                            Utils.currentPlaylistSongIndex += 1;
                        }
                    }
                    if(Action == "PlaylistViewScrollup"){
                        Action = "";
                        if(Utils.currentPlaylistSongIndex - 1 < 0 ){
                            Utils.currentPlaylistSongIndex = 0;
                        } else {
                            Utils.currentPlaylistSongIndex -= 1;
                        }
                    } 
                    if(key.Key == ConsoleKey.Enter){
                        // EDIT MENU
                        Action = "";
                        Utils.currentSongIndex = Utils.currentPlaylistSongIndex;
                        Play.PlaySong(Utils.songs, Utils.currentSongIndex);
                    } else if(Action == "DeleteCurrentSong"){
                        Action = "";
                        Play.DeleteSong(Utils.currentPlaylistSongIndex);
                    } else if(key.Key == ConsoleKey.F1){
                        Utils.queueSongs.Add(Utils.songs[Utils.currentPlaylistSongIndex]);
                    }
                }

                switch (Action)
                    {
                        case "ToMainMenu":
                            playerView = "default";
                            break;
                        case "PlayPause":
                            PauseSong();
                            Play.PlayDrawReset();
                            break;
                        case "CurrentState":
                            Console.WriteLine("CurrentState: " + state);
                            break;
                        case "Quit":
                            Console.WriteLine("Quit");
                            AnsiConsole.Clear();
                            Environment.Exit(0);
                            break;
                        case "NextSong":
                            state = MainStates.next; // next song
                            break;
                        case "PreviousSong":
                            state = MainStates.previous; // previous song
                            break;
                        case "PlaySongs":
                                TUI.PlaySingleSong();
                                break;
                        case "Forward5s": // move forward 5 seconds
                            Play.SeekSong(Preferences.forwardSeconds, true);
                            break;
                        case "Backwards5s": // move backward 5 seconds
                            Play.SeekSong(-Preferences.rewindSeconds, true);
                            break;
                        case "VolumeUp": // volume up
                            if (Preferences.isMuted)
                            {
                                Play.ToggleMute();
                            }
                            Play.ModifyVolume(Preferences.GetChangeVolumeBy());
                            Preferences.SaveSettings();
                            break;
                        case "VolumeDown": // volume down
                            if (Preferences.isMuted)
                            {
                                Play.ToggleMute();
                            }
                            Play.ModifyVolume(-Preferences.GetChangeVolumeBy());
                            Preferences.SaveSettings();
                            break;
                        case "Shuffle": // suffle or save
                            Preferences.isShuffle = !Preferences.isShuffle;
                            Preferences.SaveSettings();
                            break;
                        case "SaveAsPlaylist":
                            TUI.SaveAsPlaylist();
                            break;
                        case "SaveCurrentPlaylist":
                            TUI.SaveCurrentPlaylist();
                            break;
                        case "ShufflePlaylist":
                            TUI.ShufflePlaylist();
                            break;
                        case "Loop": // loop
                            Preferences.isLoop = !Preferences.isLoop;
                            Preferences.SaveSettings();
                            break;
                        case "Mute": // mute
                            Play.ToggleMute();
                            Preferences.SaveSettings();
                            break;
                        case "ShowHidePlaylist": // show all view
                            AnsiConsole.Clear();
                            if (playerView == "default")
                            {   
                                playerView = "all";
                                var table = new Table();
                                AnsiConsole.Write(table);
                                AnsiConsole.Markup($"{Locale.Help.Press} [red]{Keybindings.Help}[/] {Locale.Help.ToHideHelp}");
                                AnsiConsole.Markup($"\n{Locale.Help.Press} [yellow]{Keybindings.Settings}[/] {Locale.Help.ForSettings}");
                                AnsiConsole.Markup($"\n{Locale.Help.Press} [green]{Keybindings.ShowHidePlaylist}[/] {Locale.Help.ToShowPlaylist}");
                            }
                            else
                            {
                                playerView = "default";
                            }
                            break;
                        case "ListAllPlaylists":
                            TUI.ListAllPlaylists();
                            break;
                        case "Help": // show help
                            AnsiConsole.Clear();
                            if (playerView == "help")
                            {
                                playerView = "default";
                                TUI.DrawPlayer();
                                break;
                            }
                            playerView = "help";
                            TUI.DrawHelp();
                            break;
                        case "Settings": // show settings
                            AnsiConsole.Clear();
                            if (playerView == "settings")
                            {
                                playerView = "default";
                                TUI.DrawPlayer();
                                break;
                            }
                            playerView = "settings";
                            TUI.DrawSettings();
                            break;

                        case "Autosave": // autosave or not
                            Preferences.isAutoSave = !Preferences.isAutoSave;
                            Preferences.SaveSettings();
                            break;
                        case "ToSongStart": // goto song start
                            Play.SeekSong(0, false);
                            break;
                        case "ToSongEnd": // goto song end
                            Play.MaybeNextSong();
                            break;
                        case "PlaylistOptions": // playlist options
                            TUI.PlaylistInput();
                            break;
                        case "ForwardSecondAmount": // set forward seek to 1 second

                            string forwardSecondsString = Message.Input(Locale.OutsideItems.EnterForwardSeconds, "");
                            if (int.TryParse(forwardSecondsString, out int forwardSeconds))
                            {
                                Preferences.forwardSeconds = forwardSeconds;
                                Preferences.SaveSettings();
                            }
                            else
                            {
                                Message.Data($"[red]{Locale.OutsideItems.InvalidInput}.[/] {Locale.OutsideItems.PressToContinue}.", Locale.OutsideItems.InvalidInput);
                            }
                            break;
                        case "BackwardSecondAmount": // set rewind seek to 2 seconds

                            string rewindSecondsString = Message.Input(Locale.OutsideItems.EnterBackwardSeconds, "");
                            if (int.TryParse(rewindSecondsString, out int rewindSeconds))
                            {
                                Preferences.rewindSeconds = rewindSeconds;
                                Preferences.SaveSettings();
                            }
                            else
                            {
                                Message.Data($"[red]{Locale.OutsideItems.InvalidInput}.[/] {Locale.OutsideItems.PressToContinue}.", Locale.OutsideItems.InvalidInput);
                            }
                            break;
                        case "ChangeVolumeAmount": // set volume change to 3
                            string volumeChangeString = Message.Input(Locale.OutsideItems.EnterVolumeChange, "");
                            if (int.TryParse(volumeChangeString, out int volumeChange))
                            {
                                float changeVolumeByFloat = float.Parse(volumeChange.ToString()) / 100;
                                Preferences.changeVolumeBy = changeVolumeByFloat;
                                Preferences.SaveSettings();
                            }
                            else
                            {
                                Message.Data($"[red]{Locale.OutsideItems.InvalidInput}.[/] {Locale.OutsideItems.PressToContinue}.", Locale.OutsideItems.InvalidInput);
                            }
                            break;
                        case "CommandHelpScreen":
                            TUI.CliHelp();

                            AnsiConsole.MarkupLine($"\n{Locale.OutsideItems.PressToContinue}.");
                            Console.ReadKey(true);
                            break;
                        case "DeleteCurrentSong":
                            Play.DeleteSong(Utils.currentSongIndex);
                            break;
                        
                        // Case For A
                        case "AddSongToPlaylist":
                            TUI.AddSongToPlaylist();
                            break;
                        // Case For ?
                        case "ShowSongsInPlaylists":
                            TUI.ShowSongsInPlaylist();
                            break;
                        case "PlayOtherPlaylist":
                            TUI.PlayOtherPlaylist();
                            break;
                        case "RedownloadCurrentSong":
                            Play.ReDownloadSong();
                            break;
                        case "EditKeybindings":
                            AnsiConsole.Clear();
                            IniFileHandling.ScrollIndexKeybind = 0;
                            if (playerView == "default")
                            {
                                playerView = "editkeybindings";
                            }
                            else
                            {
                                playerView = "default";
                            }
                            break;
                        case "ChangeLanguage":
                            AnsiConsole.Clear();
                            IniFileHandling.ScrollIndexLanguage = 0;
                            if (playerView == "default")
                            {
                                playerView = "changelanguage";
                            }
                            else
                            {
                                playerView = "default";
                            }
                            break;
                        case "PlayRandomSong":
                            // TODO: Play random song
                            break;
                        // case ConsoleKey.J:
                        //     Message.Input();
                        //     break;
                        // case ConsoleKey.K:
                        //     Message.Data(Playlists.GetList());
                        //     break;
                    }
            
                
                TUI.RefreshCurrentView();
            }
        }

        public static void PauseSong(bool onlyPause = false)
        {
            if(onlyPause){
                Bass.ChannelPause(Utils.currentMusic);
                return;
            }
            if (Bass.ChannelIsActive(Utils.currentMusic) == PlaybackState.Playing)
            {
                state = MainStates.pause;
            }
            else
            {
                state = MainStates.play;
            }
        }


        public static bool IfHoldingDownCTRL(ConsoleKeyInfo key)
        {
            if (key.Modifiers != ConsoleModifiers.Control)
            {
                return false;
            }
            return true;
        }

        public static bool IfHoldingDownSHIFTandCTRL(ConsoleKeyInfo key)
        {
            // if key.Modifiers has the value of both shift and control
            if (key.Modifiers != (ConsoleModifiers.Shift | ConsoleModifiers.Control))
            {
                return false;
            }
            return true;
        }

        public static bool IfHoldingDownSHIFTandALT(ConsoleKeyInfo key)
        {
            if (key.Modifiers != (ConsoleModifiers.Shift | ConsoleModifiers.Alt))
            {
                return false;
            }
            return true;
        }
        public static bool IfHoldingDownCtrlandALT(ConsoleKeyInfo key)
        {
            if (key.Modifiers != (ConsoleModifiers.Control | ConsoleModifiers.Alt))
            {
                return false;
            }
            return true;
        }
        public static bool IfHoldingDownSHIFTandCTRLandAlt(ConsoleKeyInfo key)
        {
            if (key.Modifiers != (ConsoleModifiers.Shift | ConsoleModifiers.Alt | ConsoleModifiers.Control))
            {
                return false;
            }
            return true;
        }

        public static bool IfHoldingDownSHIFT(ConsoleKeyInfo key)
        {
            if (key.Modifiers != ConsoleModifiers.Shift)
            {
                return false;
            }
            return true;
        }

        public static bool IfHoldingDownALT(ConsoleKeyInfo key)
        {
            if (key.Modifiers != ConsoleModifiers.Alt)
            {
                return false;
            }
            return true;
        }
    }

}
