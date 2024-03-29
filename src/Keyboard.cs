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

                var pressed_key = key.Key;
                
                string Action = ReadWriteFile.FindMatch_KeyData(
                    pressed_key,
                    isAlt,
                    isCtrl,
                    isShift,
                    isShiftAlt,
                    isShiftCtrl
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
                if(playerView.Equals("editkeybindings") || ReadWriteFile.EditingKeybind){
                    if(key.Key == ConsoleKey.Delete && isShiftAlt && !ReadWriteFile.EditingKeybind){
                        ReadWriteFile.Create_KeyDataIni(true);
                        Message.Data("Keybinds resetted","Keybinds have been resetted");
                    }
                    if(key.Key == ConsoleKey.DownArrow && !ReadWriteFile.EditingKeybind){
                        // nullifu action
                        Action = "";
                        if(ReadWriteFile.ScrollIndexKeybind + 1 > ReadWriteFile.KeybindAmount){
                            ReadWriteFile.ScrollIndexKeybind = -1;
                        } else {
                            ReadWriteFile.ScrollIndexKeybind += 1;
                        }
                    } 
                    else if(key.Key == ConsoleKey.UpArrow && !ReadWriteFile.EditingKeybind){
                        Action = "";
                        if(ReadWriteFile.ScrollIndexKeybind - 1 < 0 ){
                            ReadWriteFile.ScrollIndexKeybind = ReadWriteFile.KeybindAmount - 2;
                        } else {
                            ReadWriteFile.ScrollIndexKeybind -= 1;
                        }
                    } 
                    if (key.Key == ConsoleKey.Enter && !ReadWriteFile.EditingKeybind){
                        Action = "";
                        ReadWriteFile.EditingKeybind = true;
                    }
                    else if (key.Key == ConsoleKey.Enter && ReadWriteFile.EditingKeybind){
                        Action = "";
                        ReadWriteFile.EditingKeybind = false;
                        ReadWriteFile.WriteIni_KeyData();
                        ReadWriteFile.isAlt = false;
                        ReadWriteFile.isCtrl = false;
                        ReadWriteFile.isShift = false;
                        ReadWriteFile.isShiftAlt = false;
                        ReadWriteFile.isShiftCtrl = false;
                    }
                    if (key.Key == ConsoleKey.Escape && ReadWriteFile.EditingKeybind){
                        Action = "";
                        ReadWriteFile.EditingKeybind = false;
                    }
                    
                    ReadWriteFile.isAlt = IfHoldingDownALT(key);
                    ReadWriteFile.isCtrl = IfHoldingDownCTRL(key);
                    ReadWriteFile.isShift = IfHoldingDownSHIFT(key);
                    ReadWriteFile.isShiftAlt = IfHoldingDownSHIFTandALT(key);
                    ReadWriteFile.isShiftCtrl = IfHoldingDownSHIFTandCTRL(key);
                    ReadWriteFile.previousClick = key.Key;

                    if(ReadWriteFile.EditingKeybind){
                        Action = "";
                    }
                }
                if(playerView.Equals("changelanguage")){
                    if(key.Key == ConsoleKey.DownArrow){
                        if(ReadWriteFile.ScrollIndexLanguage + 1 > ReadWriteFile.LocaleAmount){
                            ReadWriteFile.ScrollIndexLanguage = -1;
                        } else {
                            ReadWriteFile.ScrollIndexLanguage += 1;
                        }
                    } else if(key.Key == ConsoleKey.UpArrow){
                        Action = "";
                        if(ReadWriteFile.ScrollIndexLanguage - 1 < 0 ){
                            ReadWriteFile.ScrollIndexLanguage = ReadWriteFile.LocaleAmount;
                        } else {
                            ReadWriteFile.ScrollIndexLanguage -= 1;
                        }
                    } 
                    if(key.Key == ConsoleKey.Enter){
                        ReadWriteFile.Ini_LoadNewLocale();
                    }
                }

                switch (Action)
                    {
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
                            Play.ModifyVolume(Preferences.GetChangeVolumeBy());
                            Preferences.SaveSettings();
                            break;
                        case "VolumeDown": // volume down
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
                                // TUI.UIComponent_Songs(table);
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

                            AnsiConsole.Markup($"\n{Locale.OutsideItems.EnterForwardSeconds}: ");
                            string? forwardSecondsString = Console.ReadLine();
                            if (int.TryParse(forwardSecondsString, out int forwardSeconds))
                            {
                                Preferences.forwardSeconds = forwardSeconds;
                            }
                            else
                            {
                                AnsiConsole.Markup($"[red]\n{Locale.OutsideItems.InvalidInput}.[/] {Locale.OutsideItems.PressToContinue}.");
                                Console.ReadKey(true);
                            }
                            break;
                        case "BackwardSecondAmount": // set rewind seek to 2 seconds

                            AnsiConsole.Markup("\nEnter rewind seconds: ");
                            string? rewindSecondsString = Console.ReadLine();
                            if (int.TryParse(rewindSecondsString, out int rewindSeconds))
                            {
                                Preferences.rewindSeconds = rewindSeconds;
                            }
                            else
                            {
                                AnsiConsole.Markup($"[red]\n{Locale.OutsideItems.InvalidInput}.[/] {Locale.OutsideItems.PressToContinue}.");
                                Console.ReadKey(true);
                            }
                            break;
                        case "ChangeVolumeAmount": // set volume change to 3
                            AnsiConsole.Markup("\nEnter volume change (%): ");
                            string? volumeChangeString = Console.ReadLine();
                            if (int.TryParse(volumeChangeString, out int volumeChange))
                            {
                                float changeVolumeByFloat = float.Parse(volumeChange.ToString()) / 100;
                                Preferences.changeVolumeBy = changeVolumeByFloat;
                            }
                            else
                            {
                                AnsiConsole.Markup($"[red]\n{Locale.OutsideItems.InvalidInput}.[/] {Locale.OutsideItems.PressToContinue}.");
                                Console.ReadKey(true);
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
                            ReadWriteFile.ScrollIndexKeybind = 0;
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
                            ReadWriteFile.ScrollIndexLanguage = 0;
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
                            // TODO 
                            break;
                        // case ConsoleKey.J:
                        //     Message.Input();
                        //     break;
                        // case ConsoleKey.K:
                        //     Message.Data(Playlists.GetList());
                        //     break;
                    }
            

                TUI.RehreshCurrentView();
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
