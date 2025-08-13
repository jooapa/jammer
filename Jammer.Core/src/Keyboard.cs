using ManagedBass;
using Spectre.Console;
using System.IO;
using SharpHook;
using System.Diagnostics.CodeAnalysis;
using Jammer.Components;

namespace Jammer
{
    public partial class Start
    {
        public static string Action = "";
        public static string playerView = "default"; // default, all, help, settings, fake, editkeybindings, changelanguage
        public static async Task CheckKeyboardAsync()
        {
            // Increment keyboard check counter for performance monitoring
            PerformanceMonitor.IncrementKeyboardChecks();
            
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

                if (playerView.Equals("editkeybindings") || IniFileHandling.EditingKeybind)
                {
                    Console.Clear();
                    if (key.Key == ConsoleKey.Delete && isShiftAlt && !IniFileHandling.EditingKeybind)
                    {
                        IniFileHandling.Create_KeyDataIni(1);
                        Message.Data(Locale.LocaleKeybind.KeybindResettedMessage1, Locale.LocaleKeybind.KeybindResettedMessage2);
                    }

                    if (Action == "PlaylistViewScrolldown" && !IniFileHandling.EditingKeybind)
                    {
                        // nullifu action
                        Action = "";
                        if (IniFileHandling.ScrollIndexKeybind + 1 > IniFileHandling.KeybindAmount)
                        {
                            IniFileHandling.ScrollIndexKeybind = -1;
                        }
                        else
                        {
                            IniFileHandling.ScrollIndexKeybind += 1;
                        }
                    }
                    else if (Action == "PlaylistViewScrollup" && !IniFileHandling.EditingKeybind)
                    {
                        Action = "";
                        if (IniFileHandling.ScrollIndexKeybind - 1 < 0)
                        {
                            IniFileHandling.ScrollIndexKeybind = IniFileHandling.KeybindAmount - 1;
                        }
                        else
                        {
                            IniFileHandling.ScrollIndexKeybind -= 1;
                        }
                    }
                    if (Action == "Choose" && !IniFileHandling.EditingKeybind)
                    {
                        Action = "";
                        IniFileHandling.EditingKeybind = true;
                    }
                    else if (Action == "Choose" && IniFileHandling.EditingKeybind)
                    {
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
                    if (key.Key == ConsoleKey.Escape && isShift && IniFileHandling.EditingKeybind)
                    {
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

                    if (IniFileHandling.EditingKeybind)
                    {
                        Action = "";
                    }

                    drawWhole = true;
                }
                else if (playerView.Equals("changelanguage"))
                {
                    AnsiConsole.Clear();
                    // Jammer.Message.Data("A", $"{IniFileHandling.ScrollIndexLanguage}");
                    if (Action == "PlaylistViewScrolldown")
                    {
                        Action = "";
                        if (IniFileHandling.ScrollIndexLanguage + 1 >= IniFileHandling.LocaleAmount)
                        {
                            IniFileHandling.ScrollIndexLanguage = 0;
                        }
                        else
                        {
                            IniFileHandling.ScrollIndexLanguage += 1;
                        }
                    }
                    if (Action == "PlaylistViewScrollup")
                    {
                        Action = "";
                        if (IniFileHandling.ScrollIndexLanguage - 1 < 0)
                        {
                            IniFileHandling.ScrollIndexLanguage = IniFileHandling.LocaleAmount - 1;
                        }
                        else
                        {
                            IniFileHandling.ScrollIndexLanguage -= 1;
                        }
                    }
                    if (Action == "Choose")
                    {
                        IniFileHandling.Ini_LoadNewLocale();
                        AnsiConsole.Clear();
                        Action = "";
                    }

                    drawWhole = true;
                }
                else if (playerView.Equals("all"))
                {
                    Console.Clear();
                    if (Action == "PlaylistViewScrolldown")
                    {
                        Action = "";
                        if (Utils.CurrentPlaylistSongIndex + 1 >= Utils.Songs.Length)
                        {
                            Utils.CurrentPlaylistSongIndex = 0;
                        }
                        else
                        {
                            Utils.CurrentPlaylistSongIndex += 1;
                        }
                    }
                    if (Action == "PlaylistViewScrollup")
                    {
                        Action = "";
                        if (Utils.CurrentPlaylistSongIndex - 1 < 0)
                        {
                            Utils.CurrentPlaylistSongIndex = Utils.Songs.Length - 1;
                        }
                        else
                        {
                            Utils.CurrentPlaylistSongIndex -= 1;
                        }
                    }
                    if (Action == "Choose")
                    {
                        // EDIT MENU
                        Action = "";
                        Utils.CurrentSongIndex = Utils.CurrentPlaylistSongIndex;
                        Play.PlaySong(Utils.Songs, Utils.CurrentSongIndex);
                    }
                    else if (Action == "DeleteCurrentSong" || Action == "HardDeleteCurrentSong")
                    {
                        bool hardDelete = false;
                        if (Action == "HardDeleteCurrentSong")
                            hardDelete = true;

                        Action = "";
                        int new_value = Utils.CurrentPlaylistSongIndex;

                        // Message.Data(Utils.CurrentPlaylistSongIndex.ToString() + " " + Utils.CurrentSongIndex.ToString(), "Deleting song", true);
                        // If deleting a song before the currently playing song
                        if (Utils.CurrentPlaylistSongIndex < Utils.CurrentSongIndex)
                        {
                            // Message.Data("[red]You are deleting a song before the current song..[/]", "Deleting song", true);
                            Play.DeleteSong(Utils.CurrentPlaylistSongIndex, true, hardDelete);
                        }
                        // If deleting the currently playing song
                        else if (Utils.CurrentPlaylistSongIndex == Utils.CurrentSongIndex)
                        {
                            // Message.Data("[red]You are deleting the current song.[/]", "Deleting current song", true);
                            Play.DeleteSong(Utils.CurrentPlaylistSongIndex, false, hardDelete);
                        }
                        else if (Utils.CurrentPlaylistSongIndex > Utils.CurrentSongIndex)
                        {
                            // Message.Data("[red]You are deleting a song after the current song.[/]", "Deleting song", true);
                            Utils.CurrentSongIndex++;
                            Play.DeleteSong(Utils.CurrentPlaylistSongIndex, true, hardDelete);
                        }

                        Utils.CurrentPlaylistSongIndex = Utils.CurrentSongIndex;
                    }
                    else if (Action == "AddSongToQueue")
                    {
                        // Utils.queueSongs.Add(Utils.songs[Utils.currentPlaylistSongIndex]);
                    }

                }
                else if (playerView.Equals("help"))
                {
                    // Handle help page navigation
                    if (key.Key == ConsoleKey.PageDown || key.Key == ConsoleKey.RightArrow)
                    {
                        HelpMenuComponent.NextHelpPage();
                        drawWhole = true;
                        Action = ""; // Clear action
                    }
                    else if (key.Key == ConsoleKey.PageUp || key.Key == ConsoleKey.LeftArrow)
                    {
                        HelpMenuComponent.PreviousHelpPage();
                        drawWhole = true;
                        Action = ""; // Clear action
                    }
                    else if (Action == "ToMainMenu" || Action == "Help")
                    {
                        playerView = "default";
                        drawWhole = true;
                        Action = ""; // Clear action
                    }
                }
                if (playerView.Equals("settings"))
                {
                    switch (key.Key)
                    {
                        case Keybindings.SettingsKeys.ForwardSecondAmount:
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

                            drawWhole = true;
                            break;
                        case Keybindings.SettingsKeys.BackwardSecondAmount:

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
                            drawWhole = true;
                            break;
                        case Keybindings.SettingsKeys.ChangeVolumeAmount:
                            string volumeChangeString = Jammer.Message.Input(Locale.OutsideItems.EnterVolumeChange, "");
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
                            drawWhole = true;
                            break;
                        case Keybindings.SettingsKeys.SoundCloudClientID:
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
                            Utils.SCClientIdAlreadyLookedAndItsIncorrect = false;

                            Preferences.SaveSettings();

                            drawWhole = true;
                            break;
                        case Keybindings.SettingsKeys.Autosave:
                            Preferences.isAutoSave = !Preferences.isAutoSave;
                            Preferences.SaveSettings();
                            drawWhole = true;
                            break;
                        case Keybindings.SettingsKeys.LoadEffects:
                            Effects.ReadEffects();
                            if (Utils.Songs.Length > 0)
                            {
                                Play.SetEffectsToChannel();
                            }
                            break;
                        case Keybindings.SettingsKeys.ToggleMediaButtons:
                            Preferences.isMediaButtons = !Preferences.isMediaButtons;
                            Preferences.SaveSettings();
                            drawWhole = true;
                            break;
                        case Keybindings.SettingsKeys.ToggleVisualizer:
                            Preferences.isVisualizer = !Preferences.isVisualizer;
                            Preferences.SaveSettings();
                            drawWhole = true;
                            break;
                        case Keybindings.SettingsKeys.LoadVisualizer:
                            Visual.Read();
                            break;
                        case Keybindings.SettingsKeys.KeyModifierHelper:
                            Preferences.isModifierKeyHelper = !Preferences.isModifierKeyHelper;
                            Preferences.SaveSettings();
                            drawWhole = true;
                            break;
                        case Keybindings.SettingsKeys.FetchClientID:
                            string clientID = await SCClientIdFetcher.GetClientId();
                            if (string.IsNullOrEmpty(clientID))
                            {
                                Message.Data("Client ID not found", "Error :(", true, false);
                                drawWhole = true;
                                break;
       
                            }
                            Preferences.clientID = clientID;
                            Utils.SCClientIdAlreadyLookedAndItsIncorrect = false;
                            Preferences.SaveSettings();
                            Message.Data("Client ID fetched and set as: " + clientID, "Success!", false, false);
                            drawWhole = true;
                            break;
                        case Keybindings.SettingsKeys.SkipErrors:
                            Preferences.isSkipErrors = !Preferences.isSkipErrors;
                            Preferences.SaveSettings();
                            drawWhole = true;
                            break;
                    }

                    // able to return to default view
                    if (Action == "ToMainMenu")
                    {
                        playerView = "default";
                        drawWhole = true;
                    }
                }
                else
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
                            state = MainStates.pause;
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
                            drawWhole = true;
                            break;
                        case "Backwards5s": // move backward 5 seconds
                            Play.SeekSong(-Math.Abs(Preferences.rewindSeconds), true);
                            drawTime = true;
                            drawWhole = true;
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
                        case "VolumeUpByOne":
                            if (Preferences.isMuted)
                            {
                                Play.ToggleMute();
                            }
                            Play.ModifyVolume(0.01f);
                            drawTime = true;
                            break;
                        case "VolumeDownByOne":
                            if (Preferences.isMuted)
                            {
                                Play.ToggleMute();
                            }
                            Play.ModifyVolume(-0.01f);
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
                            Preferences.loopType = Preferences.loopType switch
                            {
                                LoopType.None => LoopType.Always,
                                LoopType.Always => LoopType.Once,
                                _ => LoopType.None
                            };
                            
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
                            HelpMenuComponent.SetHelpPage(1); // Reset to page 1 when opening help
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
                        case "SearchInPlaylist": // playlist options
                            Search.SearchForSongInPlaylistAsync();
                            drawWhole = true;
                            break;
                        case "RenameSong": // rename song

                            // Message.Data(

                            // Funcs.SmartRename(
                            //     SongExtensions.ToSong(Utils.Songs[Utils.CurrentSongIndex])
                            // ).Author
                            // + " - " +
                            // Funcs.SmartRename(
                            //     SongExtensions.ToSong(Utils.Songs[Utils.CurrentSongIndex])
                            // ).Title
                            // , "Renamed song");

                            var smartSong = Funcs.SmartRename(
                                SongExtensions.ToSong(Utils.Songs[Utils.CurrentSongIndex])
                            );
                            var smartTitle = smartSong.Title;
                            var smartAuthor = smartSong.Author;

                            var ogSongTitle = SongExtensions.Title(Utils.Songs[Utils.CurrentSongIndex]);

                            string[] name = new[] {
                                ogSongTitle,
                                smartAuthor + " - " + smartTitle,
                                smartTitle + " - " + smartAuthor,
                            };

                            // remove duplicates
                            name = name.Distinct().ToArray();

                            string newName = Message.Input(
                                "New name: ", $"Go up in History to see current name and Jammer's Smart Renames\nLeave empty to keep current name\nSeperating with 'author - title' will set the author and title",
                                false, name
                            );

                            if (string.IsNullOrEmpty(newName))
                            {
                                drawWhole = true;
                                break;
                            }

                            // if -
                            // newNameTitle
                            // newNameAuthor
                            var newNewName = newName.Split(" - ");
                            string newNameAuthor = "";
                            string newNameTitle = "";
                            if (newNewName.Length > 1)
                            {
                                newNameAuthor = newNewName[0];
                                newNameTitle = newNewName[1];

                                Song newRenamedSong = SongExtensions.ToSong(Utils.Songs[Utils.CurrentSongIndex]);
                                newRenamedSong.Author = newNameAuthor;
                                newRenamedSong.Title = newNameTitle;
                                newRenamedSong.ToSongString();
                                Utils.Songs[Utils.CurrentSongIndex] = newRenamedSong.ToSongString();
                            }
                            else
                            {
                                Song newRenamedSong = SongExtensions.ToSong(Utils.Songs[Utils.CurrentSongIndex]);
                                newRenamedSong.Title = newName;
                                newRenamedSong.ToSongString();

                                Utils.Songs[Utils.CurrentSongIndex] = newRenamedSong.ToSongString();
                            }


                            drawWhole = true;
                            break;
                        case "CommandHelpScreen":
                            TUI.CliHelp();

                            AnsiConsole.MarkupLine($"\n{Locale.OutsideItems.PressToContinue}.");
                            Console.ReadKey(true);
                            drawWhole = true;
                            break;
                        case "DeleteCurrentSong":
                            Play.DeleteSong(Utils.CurrentSongIndex, false, false, true);

                            drawWhole = true;
                            break;
                        case "HardDeleteCurrentSong":
                            Play.DeleteSong(Utils.CurrentSongIndex, false, true, true);
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
                            Funcs.ResetRssExitVariables();
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
                            newThemes[0] = "Create a new theme";
                            newThemes[1] = "Jammer Default";
                            for (int i = 0; i < themes.Length; i++)
                            {
                                newThemes[i + 2] = themes[i];
                            }
                            themes = newThemes;
                            string chosen = Message.MultiSelect(themes, Locale.Miscellaneous.ChooseTheme);


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
                                string themeName = Message.Input(Locale.Miscellaneous.EnterThemeName, Locale.Miscellaneous.NameOfYourAwesomeTheme);
                                if (Play.EmptySpaces(themeName) || themeName == "Create a new theme" || themeName == "Jammer Default")
                                {
                                    drawWhole = true;
                                    break;
                                }
                                Themes.CreateTheme(themeName);
                                Message.Input(Locale.Miscellaneous.GoEditThemeFile, Locale.Miscellaneous.ThemeFileCreatedInJammerFolder);
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
                            else
                            {
                                Preferences.theme = chosen;
                            }

                            Preferences.SaveSettings();
                            Themes.SetTheme(Preferences.theme);
                            drawWhole = true;
                            break;
                        case "Search":
                            Search.SearchSongOnMediaPlatform();
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
                        case "ShowPerformanceLog": // New performance log action
                            AnsiConsole.Clear();
                            Message.Data(PerformanceMonitor.GetPerformanceLog(), "Performance Log");
                            drawWhole = true;
                            break;
                        case "ExportPerformanceLog": // New performance log export action
                            PerformanceMonitor.WritePerformanceLogToFile();
                            drawWhole = true;
                            break;
                        case "Choose":

                            if (!Funcs.IsCurrentSongARssFeed())
                            {
                                break;
                            }

                            // when opening the new view its actually gonna save the playlist aand come back to it to the same position it left.
                            Utils.lastPositionInPreviousPlaylist = Utils.CurrentSongIndex;
                            Utils.BackUpSongs = Utils.Songs;
                            Utils.RssFeedSong = SongExtensions.ToSong(Utils.Songs[Utils.CurrentPlaylistSongIndex]);
                            Utils.BackUpPlaylistName = Utils.CurrentPlaylist;
                            Utils.CurrentPlaylist = "";
                            // Message.Data(Utils.BackUpPlaylistName, "a");

                            
                            // convert all the rssfeeds to songs
                            RootRssData rssFeed = Rss.GetRssData(Utils.RssFeedSong.URI).GetAwaiter().GetResult();
                            // state = MainStates.next;
                            // break;
                            Utils.Songs = Array.Empty<string>();
                            Utils.CurrentPlaylistSongIndex = 0;
                            Utils.CurrentSongIndex = 0;

                            foreach (var i in rssFeed.Content)
                            {
                                // Convert each RSS feed item to a song
                                Song song = new Song
                                {
                                    Title = i.Title,
                                    Author = i.Author,
                                    URI = i.Link,
                                    Description = i.Description,
                                    PubDate = i.PubDate
                                };

                                song.ExtractSongDetails();
                                Utils.Songs = Utils.Songs.Concat(new[] { song.ToSongString() }).ToArray();
                            }

                            // Start.state = MainStates.play;
                            // Play.PlayDrawReset();
                            // Play.PlaySong(Utils.Songs, Utils.Songs.Length - 1);
                            // TUI.RefreshCurrentView();
                            // lastPlaybackTime = -1;
                            // lastSeconds = -1;
                            // Play.PlayDrawReset();
                            // Play.PlaySong(Utils.Songs, 0);
                            // AnsiConsole.Clear();
                            Play.PlaySong(Utils.Songs, 0);
                            // debug = true;
                            break;
                        case "ExitRssFeed":
                            if (!Funcs.IsInsideOfARssFeed())
                            {
                                break;
                            }

                            // do the oppisite of what it does when going in
                            Utils.Songs = Utils.BackUpSongs;
                            Utils.CurrentPlaylist = Utils.BackUpPlaylistName;
                            Utils.CurrentSongIndex = Utils.lastPositionInPreviousPlaylist;
                            Utils.CurrentPlaylistSongIndex = Utils.lastPositionInPreviousPlaylist;

                            Funcs.ResetRssExitVariables();

                            Play.PlaySong(Utils.Songs, Utils.CurrentSongIndex);

                            break;
                        case "ChangeSoundFont":
                            AnsiConsole.Clear();
                            string[] soundFonts = SoundFont.GetSoundFonts();
                            string[] newSoundFonts = new string[soundFonts.Length + 3];
                            newSoundFonts[0] = "Cancel";
                            newSoundFonts[1] = "Link to a soundfont by path";
                            newSoundFonts[2] = "Import soundfont by path";

                            for (int i = 0; i < soundFonts.Length; i++)
                            {
                                newSoundFonts[i + 3] = soundFonts[i];
                            }

                            soundFonts = newSoundFonts;

                            string chosenSoundFont = Message.MultiSelect(soundFonts, Locale.Miscellaneous.ChooseSoundFont);

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
                            long position = Bass.ChannelGetPosition(Utils.CurrentMusic);
                            Play.StartPlaying();
                            // goto the position
                            Bass.ChannelSetPosition(Utils.CurrentMusic, position);
                            drawWhole = true;
                            break;
                    }
                Action = "";
                if (playerView == "all")
                {
                    drawWhole = true;
                }

                Playlists.AutoSave(); // TODO: BEST WAY TO DO IT

                if (debug)
                    Message.Data(Environment.StackTrace, "sd");
            }
        }

        public static void PauseSong(bool onlyPause = false)
        {
            if (onlyPause)
            {
                Bass.ChannelPause(Utils.CurrentMusic);
                state = MainStates.pause;
                return;
            }
            if (Bass.ChannelIsActive(Utils.CurrentMusic) == PlaybackState.Playing)
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

        public static void OnKeyReleased(object? sender, KeyboardHookEventArgs e)
        {
            if (Preferences.isMediaButtons)
            {
                switch (e.Data.KeyCode)
                {
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
        public static async void InitializeSharpHook()
        {
            var hook = new TaskPoolGlobalHook();
            hook.KeyReleased += OnKeyReleased;     // EventHandler<KeyboardHookEventArgs>
            await hook.RunAsync();
        }

    }
}
