using ManagedBass;
using Spectre.Console;
using System.IO;

namespace jammer
{
    public partial class Start
    {
        public static string playerView = "default"; // default, all, help, settings, fake
        public static void CheckKeyboard()
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.Spacebar:
                        PauseSong();
                        break;
                    case ConsoleKey.F12:
                        Console.WriteLine("CurrentState: " + state);
                        break;
                    case ConsoleKey.Q:
                        Console.WriteLine("Quit");
                        AnsiConsole.Clear();
                        Environment.Exit(0);
                        break;
                    case ConsoleKey.N:
                        state = MainStates.next; // next song
                        break;
                    case ConsoleKey.P:
                        if (IfHoldingDownSHIFT(key))
                        {
                            TUI.PlaySingleSong();
                            break;
                        }
                        state = MainStates.previous; // previous song
                        break;
                    case ConsoleKey.RightArrow: // move forward 5 seconds
                        Play.SeekSong(Preferences.forwardSeconds, true);
                        break;
                    case ConsoleKey.LeftArrow: // move backward 5 seconds
                        Play.SeekSong(-Preferences.rewindSeconds, true);
                        break;
                    case ConsoleKey.UpArrow: // volume up
                        Play.ModifyVolume(Preferences.GetChangeVolumeBy());
                        Preferences.SaveSettings();
                        break;
                    case ConsoleKey.DownArrow: // volume down
                        Play.ModifyVolume(-Preferences.GetChangeVolumeBy());
                        Preferences.SaveSettings();
                        break;
                    case ConsoleKey.S: // suffle or save
                        if (IfHoldingDownSHIFTandALT(key))
                        {
                            TUI.SaveAsPlaylist();
                            break;
                        }
                        if (IfHoldingDownSHIFT(key))
                        {
                            TUI.SaveCurrentPlaylist();
                            break;
                        }
                        if (IfHoldingDownALT(key))
                        {
                            TUI.ShufflePlaylist();
                            break;
                        }
                        Preferences.isShuffle = !Preferences.isShuffle;
                        Preferences.SaveSettings();
                        break;
                    case ConsoleKey.L: // loop
                        Preferences.isLoop = !Preferences.isLoop;
                        Preferences.SaveSettings();
                        break;
                    case ConsoleKey.M: // mute
                        Play.ToggleMute();
                        Preferences.SaveSettings();
                        break;
                    case ConsoleKey.F: // show all view
                        if (IfHoldingDownSHIFT(key))
                        {
                            TUI.ListAllPlaylists();
                            break;
                        }
                        AnsiConsole.Clear();
                        if (playerView == "default")
                        {
                            playerView = "all";
                            var table = new Table();
                            // TUI.UIComponent_Songs(table);
                            AnsiConsole.Write(table);
                            AnsiConsole.Markup("Press [red]h[/] for help");
                            AnsiConsole.Markup("\nPress [yellow]c[/] to show settings");
                            AnsiConsole.Markup("\nPress [green]f[/] to show playlist");
                        }
                        else
                        {
                            playerView = "default";
                        }
                        break;
                    case ConsoleKey.H: // show help
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
                    case ConsoleKey.C: // show settings
                        if (playerView == "settings")
                        {
                            playerView = "default";
                            TUI.DrawPlayer();
                            break;
                        }
                        playerView = "settings";
                        TUI.DrawSettings();
                        break;
                    case ConsoleKey.D0: // goto song start
                        Play.SeekSong(0, false);
                        lastSeconds = 0;
                        drawOnce = true;
                        break;
                    case ConsoleKey.D9: // goto song end
                        Play.MaybeNextSong();
                        break;
                    case ConsoleKey.F2: // playlist options
                        TUI.PlaylistInput();
                        break;
                    case ConsoleKey.D1: // set forward seek to 1 second
                        if (!IfHoldingDownCTRL(key)) break;

                        AnsiConsole.Markup("\nEnter forward seconds: ");
                        string? forwardSecondsString = Console.ReadLine();
                        if (int.TryParse(forwardSecondsString, out int forwardSeconds))
                        {
                            Preferences.forwardSeconds = forwardSeconds;
                        }
                        else
                        {
                            AnsiConsole.Markup("[red]\nInvalid input.[/] Press any key to continue.");
                            Console.ReadKey(true);
                        }
                        break;
                    case ConsoleKey.D2: // set rewind seek to 2 seconds
                        if (!IfHoldingDownCTRL(key)) break;

                        AnsiConsole.Markup("\nEnter rewind seconds: ");
                        string? rewindSecondsString = Console.ReadLine();
                        if (int.TryParse(rewindSecondsString, out int rewindSeconds))
                        {
                            Preferences.rewindSeconds = rewindSeconds;
                        }
                        else
                        {
                            AnsiConsole.Markup("[red]\nInvalid input.[/] Press any key to continue.");
                            Console.ReadKey(true);
                        }
                        break;
                    case ConsoleKey.D3: // set volume change to 3
                        if (!IfHoldingDownCTRL(key)) break;
                        AnsiConsole.Markup("\nEnter volume change (%): ");
                        string? volumeChangeString = Console.ReadLine();
                        if (int.TryParse(volumeChangeString, out int volumeChange))
                        {
                            float changeVolumeByFloat = float.Parse(volumeChange.ToString()) / 100;
                            Preferences.changeVolumeBy = changeVolumeByFloat;
                        }
                        else
                        {
                            AnsiConsole.Markup("[red]\nInvalid input.[/] Press any key to continue.");
                            Console.ReadKey(true);
                        }
                        break;
                    case ConsoleKey.D4: // autosave or not
                        if (!IfHoldingDownCTRL(key)) break;

                        Preferences.isAutoSave = !Preferences.isAutoSave;
                        break;
                    case ConsoleKey.Tab:
                        TUI.Help();

                        AnsiConsole.MarkupLine("\nPress any key to continue.");
                        Console.ReadKey(true);
                        break;
                    case ConsoleKey.Delete:
                        Play.DeleteSong(Utils.currentSongIndex);
                        break;
                    
                    // Case For A
                    case ConsoleKey.A:
                        if (IfHoldingDownSHIFT(key))
                        {
                            TUI.AddSongToPlaylist();
                        }
                        break;
                    // Case For ?
                    case ConsoleKey.D:
                        if (IfHoldingDownSHIFT(key))
                        {
                            TUI.ShowSongsInPlaylist();
                        }
                        break;
                    case ConsoleKey.O:
                        if (IfHoldingDownSHIFT(key))
                        {
                            TUI.PlayOtherPlaylist();
                        }
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

        public static void PauseSong()
        {
            if (Bass.ChannelIsActive(Utils.currentMusic) == PlaybackState.Playing)
                Bass.ChannelPause(Utils.currentMusic);
            else
                Bass.ChannelPlay(Utils.currentMusic);
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
