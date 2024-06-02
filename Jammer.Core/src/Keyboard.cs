using ManagedBass;
using Spectre.Console;
using System.IO;
using SharpHook;

namespace Jammer
{
    public partial class Start
    {
        public static string Action = "";
        public static string playerView = "default"; // default, all, help, settings, fake, editkeybindings, changelanguage
        public static void CheckKeyboard()
        {
            if (Console.KeyAvailable || Action != "")
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
                
                Action = IniFileHandling.FindMatch_KeyData(
                    pressed_key,
                    isAlt,
                    isCtrl,
                    isShift,
                    isShiftAlt,
                    isShiftCtrl,
                    isCtrlAlt,
                    isShiftCtrlAlt
                    );

                if(playerView.Equals("editkeybindings") || IniFileHandling.EditingKeybind){
                    Console.Clear();
                    if(key.Key == ConsoleKey.Delete && isShiftAlt && !IniFileHandling.EditingKeybind){
                        IniFileHandling.Create_KeyDataIni(1);
                        #if CLI_UI
                        Jammer.Message.Data(Locale.LocaleKeybind.KeybindResettedMessage1, Locale.LocaleKeybind.KeybindResettedMessage2);
                        #endif
                        #if AVALONIA_UI
                        // TODO AVALONIA_UI
                        #endif
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
                    if (Action == "Choose" && !IniFileHandling.EditingKeybind){
                        Action = "";
                        IniFileHandling.EditingKeybind = true;
                    }
                    else if (Action == "Choose"&& IniFileHandling.EditingKeybind){
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
                else if(playerView.Equals("changelanguage")){
                    Console.Clear();
                    // Jammer.Message.Data("A", $"{IniFileHandling.ScrollIndexLanguage}");
                    if(Action == "PlaylistViewScrolldown"){
                        Action = "";
                        if(IniFileHandling.ScrollIndexLanguage + 1 >= IniFileHandling.LocaleAmount){
                            IniFileHandling.ScrollIndexLanguage = 0;
                        } else {
                            IniFileHandling.ScrollIndexLanguage += 1;
                        }
                    }
                    if(Action == "PlaylistViewScrollup"){
                        Action = "";
                        if(IniFileHandling.ScrollIndexLanguage - 1 < 0 ){
                            IniFileHandling.ScrollIndexLanguage = IniFileHandling.LocaleAmount - 1;
                        } else {
                            IniFileHandling.ScrollIndexLanguage -= 1;
                        }
                    } 
                    if(Action == "Choose"){
                        IniFileHandling.Ini_LoadNewLocale();
                    }
                }

                else if(playerView.Equals("all")){
                    Console.Clear();
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
                    if(Action == "Choose"){
                        // EDIT MENU
                        Action = "";
                        Utils.currentSongIndex = Utils.currentPlaylistSongIndex;
                        Play.PlaySong(Utils.songs, Utils.currentSongIndex);
                    } else if(Action == "DeleteCurrentSong"){
                        Action = "";
                        int new_value = Utils.currentPlaylistSongIndex;
                        if(Utils.currentPlaylistSongIndex <= Utils.songs.Length 
                        && Utils.currentPlaylistSongIndex != 0){
                            new_value--;
                        }
                        Play.DeleteSong(Utils.currentPlaylistSongIndex, true);
                        Utils.currentPlaylistSongIndex = new_value;
                    } else if(Action == "AddSongToQueue"){
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
                            Start.state = MainStates.pause;
                            Environment.Exit(0);
                            break;
                        case "NextSong":
                            state = MainStates.next; // next song
                            break;
                        case "PreviousSong":
                            state = MainStates.previous; // previous song
                            break;
                        case "PlaySongs":
                                Funcs.PlaySingleSong();
                                break;
                        case "Forward5s": // move forward 5 seconds
                            Play.SeekSong(Preferences.forwardSeconds, true);
                            drawTime = true;
                            break;
                        case "Backwards5s": // move backward 5 seconds
                            Play.SeekSong(-Preferences.rewindSeconds, true);
                            drawTime = true;
                            break;
                        case "VolumeUp": // volume up
                            if (Preferences.isMuted)
                            {
                                Play.ToggleMute();
                            }
                            Play.ModifyVolume(Preferences.GetChangeVolumeBy());
                            Preferences.SaveSettings();
                            drawTime = true;
                            break;
                        case "VolumeDown": // volume down
                            if (Preferences.isMuted)
                            {
                                Play.ToggleMute();
                            }
                            Play.ModifyVolume(-Preferences.GetChangeVolumeBy());
                            Preferences.SaveSettings();
                            drawTime = true;
                            break;
                        case "Shuffle": // suffle or save
                            Preferences.isShuffle = !Preferences.isShuffle;
                            Preferences.SaveSettings();
                            drawTime = true;
                            break;
                        case "SaveAsPlaylist":
                            #if CLI_UI
                            Funcs.SaveAsPlaylist();
                            #endif
                            #if AVALONIA_UI
                            // TODO AVALONIA_UI
                            #endif
                            break;
                        case "SaveCurrentPlaylist":
                            #if CLI_UI
                            Funcs.SaveCurrentPlaylist();
                            #endif
                            #if AVALONIA_UI
                            // TODO AVALONIA_UI
                            #endif
                            break;
                        case "ShufflePlaylist":
                            #if CLI_UI
                            Funcs.ShufflePlaylist();
                            #endif
                            #if AVALONIA_UI
                            // TODO AVALONIA_UI
                            #endif
                            break;
                        case "Loop": // loop
                            Preferences.isLoop = !Preferences.isLoop;
                            Preferences.SaveSettings();
                            drawTime = true;
                            break;
                        case "Mute": // mute
                            Play.ToggleMute();
                            Preferences.SaveSettings();
                            drawTime = true;
                            break;
                        case "ShowHidePlaylist": // show all view
                            if (playerView == "default")
                            {   
                                playerView = "all";
                                #if AVALONIA_UI
                                // TODO AVALONIA_UI
                                #endif
                            }
                            else
                            {
                                playerView = "default";
                            }
                            break;
                        case "ListAllPlaylists":
                            #if CLI_UI
                            Funcs.ListAllPlaylists();
                            #endif
                            #if AVALONIA_UI
                            // TODO AVALONIA_UI
                            #endif
                            break;
                        case "Help": // show help
                            AnsiConsole.Clear();
                            if (playerView == "help")
                            {
                                playerView = "default";
                                break;
                            }
                            playerView = "help";
                            break;
                        case "Settings": // show settings
                            AnsiConsole.Clear();
                            if (playerView == "settings")
                            {
                                playerView = "default";
                                break;
                            }
                            playerView = "settings";
                            break;

                        case "Autosave": // autosave or not
                            Preferences.isAutoSave = !Preferences.isAutoSave;
                            Preferences.SaveSettings();
                            break;
                        case "LoadEffects": // reset effects
                            Effects.ReadEffects();
                            Play.SetEffectsToChannel();
                            break;
                        case "ToggleMediaButtons": // toggle media buttons
                            Preferences.isMediaButtons = !Preferences.isMediaButtons;
                            Preferences.SaveSettings();
                            break;
                        case "ToggleVisualizer": // toggle visualizer
                            Preferences.isVisualizer = !Preferences.isVisualizer;
                            Preferences.SaveSettings();
                            drawWhole = true;
                            break;
                        case "LoadVisualizer":
                            Visual.Read();
                            break;
                        case "ToSongStart": // goto song start
                            Play.SeekSong(0, false);
                            break;
                        case "ToSongEnd": // goto song end
                            Play.MaybeNextSong();
                            break;
                        case "PlaylistOptions": // playlist options
                            #if CLI_UI
                            Funcs.PlaylistInput();
                            #endif
                            #if AVALONIA_UI
                            // TODO AVALONIA_UI
                            #endif
                            break;
                        case "ForwardSecondAmount": // set forward seek to 1 second

                            string forwardSecondsString = Jammer.Message.Input(Locale.OutsideItems.EnterForwardSeconds, "");
                            if (int.TryParse(forwardSecondsString, out int forwardSeconds))
                            {
                                Preferences.forwardSeconds = forwardSeconds;
                                Preferences.SaveSettings();
                            }
                            else
                            {
                                #if CLI_UI
                                Jammer.Message.Data($"[red]{Locale.OutsideItems.InvalidInput}.[/] {Locale.OutsideItems.PressToContinue}.", Locale.OutsideItems.InvalidInput);
                                #endif
                                #if AVALONIA_UI
                                // TODO AVALONIA_UI
                                #endif
                            }
                            break;
                        case "BackwardSecondAmount": // set rewind seek to 2 seconds

                            string rewindSecondsString = Jammer.Message.Input(Locale.OutsideItems.EnterBackwardSeconds, "");
                            if (int.TryParse(rewindSecondsString, out int rewindSeconds))
                            {
                                Preferences.rewindSeconds = rewindSeconds;
                                Preferences.SaveSettings();
                            }
                            else
                            {
                                #if CLI_UI
                                Jammer.Message.Data($"[red]{Locale.OutsideItems.InvalidInput}.[/] {Locale.OutsideItems.PressToContinue}.", Locale.OutsideItems.InvalidInput);
                                #endif
                                #if AVALONIA_UI
                                // TODO AVALONIA_UI
                                #endif
                            }
                            break;
                        case "ChangeVolumeAmount": // set volume change to 3
                            string volumeChangeString = Jammer.Message.Input(Locale.OutsideItems.EnterVolumeChange, "");
                            if (int.TryParse(volumeChangeString, out int volumeChange))
                            {
                                float changeVolumeByFloat = float.Parse(volumeChange.ToString()) / 100;
                                Preferences.changeVolumeBy = changeVolumeByFloat;
                                Preferences.SaveSettings();
                            }
                            else
                            {
                                #if CLI_UI
                                Jammer.Message.Data($"[red]{Locale.OutsideItems.InvalidInput}.[/] {Locale.OutsideItems.PressToContinue}.", Locale.OutsideItems.InvalidInput);
                                #endif
                                #if AVALONIA_UI
                                // TODO AVALONIA_UI
                                #endif
                            }
                            break;
                        case "CommandHelpScreen":
                            #if CLI_UI
                            TUI.CliHelp();

                            AnsiConsole.MarkupLine($"\n{Locale.OutsideItems.PressToContinue}.");
                            #endif
                            #if AVALONIA_UI
                            // TODO AVALONIA_UI
                            #endif
                            Console.ReadKey(true);
                            break;
                        case "DeleteCurrentSong":
                            Play.DeleteSong(Utils.currentSongIndex, false);
                            break;
                        
                        // Case For A
                        case "AddSongToPlaylist":
                            #if CLI_UI
                            Funcs.AddSongToPlaylist();
                            #endif
                            #if AVALONIA_UI
                            // TODO AVALONIA_UI
                            #endif
                            break;
                        // Case For ?
                        case "ShowSongsInPlaylists":
                            #if CLI_UI
                            Funcs.ShowSongsInPlaylist();
                            #endif
                            #if AVALONIA_UI
                            // TODO AVALONIA_UI
                            #endif
                            break;
                        case "PlayOtherPlaylist":
                            #if CLI_UI
                            Funcs.PlayOtherPlaylist();
                            #endif
                            #if AVALONIA_UI
                            // TODO AVALONIA_UI
                            #endif
                            break;
                        case "RedownloadCurrentSong":
                            Play.ReDownloadSong();
                            break;
                        case "EditKeybindings":
                            #if CLI_UI
                            AnsiConsole.Clear();
                            IniFileHandling.ScrollIndexKeybind = 0;
                            #endif
                            #if AVALONIA_UI
                            // TODO AVALONIA_UI
                            #endif
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
                            #if CLI_UI
                            AnsiConsole.Clear();
                            IniFileHandling.ScrollIndexLanguage = 0;
                            #endif
                            #if AVALONIA_UI
                            // TODO AVALONIA_UI
                            #endif
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
                            Play.RandomSong();
                            break;
                        // case ConsoleKey.J:
                        //     Jammer.Message.Input();
                        //     break;
                        // case ConsoleKey.K:
                        //     Jammer.Message.Data(Playlists.GetList());
                        //     break;
                    }
            
                #if CLI_UI
                #endif
                #if AVALONIA_UI
                // TODO AVALONIA_UI
                #endif
                Action = "";
            }

        }

        public static void PauseSong(bool onlyPause = false)
        {
            if(onlyPause){
                Bass.ChannelPause(Utils.currentMusic);
                state = MainStates.pause;
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

        public static void OnKeyReleased(object sender, KeyboardHookEventArgs e) {
            if (Preferences.isMediaButtons) {
                switch(e.Data.KeyCode) {
                    case SharpHook.Native.KeyCode.VcMediaNext:
                        state = MainStates.next; // next song
                        break;
                    case SharpHook.Native.KeyCode.VcMediaPrevious:
                        state = MainStates.previous; // previous song
                        break;
                    case SharpHook.Native.KeyCode.VcMediaPlay:
                        PauseSong();
                        Play.PlayDrawReset();
                        break;
                    case SharpHook.Native.KeyCode.VcMediaStop:
                        PauseSong(true);
                        Play.PlayDrawReset();
                        break;
                }
            }
        }
        public static async void InitializeSharpHook() {
                var hook = new TaskPoolGlobalHook();
                hook.KeyReleased += OnKeyReleased;     // EventHandler<KeyboardHookEventArgs>
                await hook.RunAsync();
        }   

    }
}
