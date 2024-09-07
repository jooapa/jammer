using ManagedBass;
using Spectre.Console;
using System.IO;
using SharpHook;
using System.Diagnostics.CodeAnalysis;

namespace Jammer
{
    public partial class Start
    {
        public static string Action = "";
        public static string playerView = "default"; // default, all, help, settings, fake, editkeybindings, changelanguage
        public static void CheckKeyboardAsync()
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
                        Message.Data(Locale.LocaleKeybind.KeybindResettedMessage1, Locale.LocaleKeybind.KeybindResettedMessage2);
                    }

                    if(Action == "PlaylistViewScrolldown" && !IniFileHandling.EditingKeybind){
                        // nullifu action
                        Action = "";
                        if(IniFileHandling.ScrollIndexKeybind + 1 > IniFileHandling.KeybindAmount){
                            IniFileHandling.ScrollIndexKeybind = -1;
                        } else {
                            IniFileHandling.ScrollIndexKeybind += 1;
                        }
                    } 
                    else if(Action == "PlaylistViewScrollup" && !IniFileHandling.EditingKeybind){
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

                    drawWhole = true;
                }
                else if(playerView.Equals("changelanguage")){
                    AnsiConsole.Clear();
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
                        AnsiConsole.Clear();
                        Action = "";
                    }

                    drawWhole = true;
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
                        // Utils.queueSongs.Add(Utils.songs[Utils.currentPlaylistSongIndex]);
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
                            Funcs.SaveAsPlaylist();
                            drawWhole = true;
                            break;
                        case "SaveCurrentPlaylist":
                            Funcs.SaveCurrentPlaylist();
                            drawWhole = true;
                            break;
                        case "ShufflePlaylist":
                            Funcs.ShufflePlaylist();
                            drawWhole = true;
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
                            }
                            else
                            {
                                playerView = "default";
                            }
                            break;
                        case "ListAllPlaylists":
                            Funcs.ListAllPlaylists();
                            drawWhole = true;
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
                            if(Utils.songs.Length > 0){
                                Play.SetEffectsToChannel();
                            }
                            break;
                        case "ToggleMediaButtons": // toggle media buttons
                            Preferences.isMediaButtons = !Preferences.isMediaButtons;
                            Preferences.SaveSettings();
                            drawWhole = true;   
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
                        case "ToggleInfo": // toggle info
                            Message.Data("Info", "Info is toggled");
                            drawWhole = true;
                            break;
                        case "PlaylistOptions": // playlist options
                            Funcs.PlaylistInput();
                            drawWhole = true;
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

                                Jammer.Message.Data($"[red]{Locale.OutsideItems.InvalidInput}.[/] {Locale.OutsideItems.PressToContinue}.", Locale.OutsideItems.InvalidInput);
                            }

                            drawWhole = true;
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
                                Jammer.Message.Data($"[red]{Locale.OutsideItems.InvalidInput}.[/] {Locale.OutsideItems.PressToContinue}.", Locale.OutsideItems.InvalidInput);
                            }
                            drawWhole = true;
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
                                Jammer.Message.Data($"[red]{Locale.OutsideItems.InvalidInput}.[/] {Locale.OutsideItems.PressToContinue}.", Locale.OutsideItems.InvalidInput);
                            }
                            drawWhole = true;
                            break;
                        case "SetSoundcloudClientID":
                            if (Preferences.clientID == "")
                            {
                                
                            }
                            SoundCloudExplode.SoundCloudClient client = new SoundCloudExplode.SoundCloudClient();

                            string soundcloudClientID = Jammer.Message.Input(
                                "Enter your Soundcloud Client ID:", 
                                "Current Soundcloud Client ID: " + (string.IsNullOrEmpty(Preferences.clientID) ? client.ClientId : Preferences.clientID) + "\n" + 
                                "type 'cancel' to cancel" + "\n" + 
                                "type 'reset' to reset to default"
                            );

                            if (soundcloudClientID == "cancel")
                            {
                                drawWhole = true;
                                break;
                            }

                            if (soundcloudClientID == "reset")
                            {
                                Preferences.clientID = "";
                                Preferences.SaveSettings();
                                drawWhole = true;                                
                                break;
                            }

                            Preferences.clientID = soundcloudClientID;

                            Preferences.SaveSettings();

                            drawWhole = true;
                            break;
                        case "CommandHelpScreen":
                            TUI.CliHelp();

                            AnsiConsole.MarkupLine($"\n{Locale.OutsideItems.PressToContinue}.");
                            Console.ReadKey(true);
                            drawWhole = true;
                            break;
                        case "DeleteCurrentSong":
                            Play.DeleteSong(Utils.currentSongIndex, false);
                            drawWhole = true;
                            break;
                        
                        // Case For A
                        case "AddSongToPlaylist":
                            Funcs.AddSongToPlaylist();
                            drawWhole = true;
                            break;
                        // Case For ?
                        case "ShowSongsInPlaylists":
                            Funcs.ShowSongsInPlaylist();
                            drawWhole = true;
                            break;
                        case "PlayOtherPlaylist":
                            Funcs.PlayOtherPlaylist();
                            drawWhole = true;
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
                        case "ChangeTheme":
                            AnsiConsole.Clear();
                            string[] themes = Themes.GetAllThemes();
                            // move that first element is "Create a new theme" and "Jammer Default"
                            string[] newThemes = new string[themes.Length + 2];
                            newThemes[0] = "Create a new theme";                        // TODO ADD LOCALE
                            newThemes[1] = "Jammer Default";
                            for (int i = 0; i < themes.Length; i++)
                            {
                                newThemes[i + 2] = themes[i];
                            }
                            themes = newThemes;
                            string chosen = Message.MultiSelect(themes, "Choose a theme:"); // TODO ADD LOCALE
                            

                            if (chosen == "Jammer Default")
                            {
                                Preferences.theme = chosen;
                                Preferences.SaveSettings();
                                Themes.SetTheme(Preferences.theme);
                                drawWhole = true;
                                break;
                            }

                            // if the user wants to create a new theme
                            if (chosen == "Create a new theme")
                            {
                                AnsiConsole.Clear();
                                string themeName = Message.Input("Enter a theme name:", "Name of your AMAZING theme"); // TODO ADD LOCALE
                                if (Play.EmptySpaces(themeName) || themeName == "Create a new theme" || themeName == "Jammer Default")
                                {
                                    drawWhole = true;
                                    break;
                                }
                                Themes.CreateTheme(themeName);
                                Message.Input("Go edit the theme file", "Theme file created in the jammer/themes folder"); // TODO ADD LOCALE
                                // If windows, open with notepad
#if WINDOWS
                                    System.Diagnostics.Process.Start("explorer.exe", Path.Combine(Utils.JammerPath, "themes"));
#elif LINUX
                                    System.Diagnostics.Process.Start("xdg-open", Path.Combine(Utils.JammerPath, "themes"));
#elif MAC
                                    System.Diagnostics.Process.Start("open", Path.Combine(Utils.JammerPath, "themes"));
#endif
                                Preferences.theme = themeName;
                            }
                            else {
                                Preferences.theme = chosen;
                            }

                            Preferences.SaveSettings();
                            Themes.SetTheme(Preferences.theme);
                            drawWhole = true;
                            break;
                        case "Search":
                            Search.SearchSong();
                            drawWhole = true;
                            break;
                        case "PlayRandomSong":
                            Play.RandomSong();
                            break;
                        case "ShowLog":
                            AnsiConsole.Clear();
                            Message.Data(Log.GetLog(), "Log");
                            drawWhole = true;
                            break;
                        case "ChangeSoundFont":
                            AnsiConsole.Clear();
                            string[] soundFonts = SoundFont.GetSoundFonts();
                            string[] newSoundFonts = new string[soundFonts.Length + 3];
                            newSoundFonts[0] = "Cancel";
                            newSoundFonts[1] = "Link to a soundfont by path"; // TODO ADD LOCALE
                            newSoundFonts[2] = "Import soundfont by path"; // TODO ADD LOCALE

                            for (int i = 0; i < soundFonts.Length; i++)
                            {
                                newSoundFonts[i + 3] = soundFonts[i];
                            }

                            soundFonts = newSoundFonts;

                            string chosenSoundFont = Message.MultiSelect(soundFonts, "Choose a soundfont:"); // TODO ADD LOCALE

                            switch (chosenSoundFont)
                            {
                                case "Cancel":
                                    drawWhole = true;
                                    chosenSoundFont = Preferences.currentSf2;
                                    break;
                                case "Link to a soundfont by path":
                                    string path = Message.Input("Enter the path to the soundfont:", "Path to the soundfont");
                                    if (File.Exists(path))
                                    {
                                        SoundFont.MakeAbsoluteSfFile(path);
                                        chosenSoundFont = path;
                                    }
                                    else
                                    {
                                        Message.Data("The file does not exist", ":(", true);
                                        drawWhole = true;
                                        chosenSoundFont = Preferences.currentSf2;
                                    }
                                    break;
                                case "Import soundfont by path":
                                    string importPath = Message.Input("Enter the path to the soundfont:", "Path to the soundfont");
                                    if (File.Exists(importPath))
                                    {
                                        chosenSoundFont = Preferences.currentSf2;
                                        string importAf = SoundFont.ImportSoundFont(importPath);
                                        if (importAf != string.Empty)
                                        {
                                            chosenSoundFont = importAf;
                                        }
                                    }
                                    else
                                    {
                                        Message.Data("The file does not exist", ":(", true);
                                        drawWhole = true;
                                        chosenSoundFont = Preferences.currentSf2;
                                    }
                                    break;
                            }

                            Preferences.currentSf2 = chosenSoundFont;
                            Preferences.SaveSettings();
                            long position = Bass.ChannelGetPosition(Utils.currentMusic);
                            Play.StartPlaying();
                            // goto the position
                            Bass.ChannelSetPosition(Utils.currentMusic, position);
                            drawWhole = true;
                            break;
                    }
                Action = "";
                if (playerView == "all") {
                    drawWhole = true;
                }
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

        public static void OnKeyReleased(object? sender, KeyboardHookEventArgs e) {
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
