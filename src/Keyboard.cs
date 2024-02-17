using NAudio.Wave;
using Spectre.Console;

namespace jammer
{
    public partial class Start
    {
        public static string playerView = "default"; // default, all, help, settings, fake
        public static void CheckKeyboard()
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true).Key;
                switch (key)
                {
                    case ConsoleKey.Spacebar:
                        if (Utils.audioStream == null) {
                            Debug.dprint("No audio");
                            break;
                        }
                        if (Utils.currentMusic.PlaybackState == PlaybackState.Playing)
                        {
                            Play.PauseSong();
                            state = MainStates.pause;
                            drawOnce = true;
                        }
                        else if (Utils.currentMusic.PlaybackState == PlaybackState.Paused && Utils.audioStream != null)
                        {
                            if (Utils.audioStream.Position == Utils.audioStream.Length)
                            {
                                Utils.audioStream.Position = 0;
                                lastSeconds = 0;
                                state = MainStates.playing;
                                Utils.currentMusic.Play();
                                drawOnce = true;
                                break;
                            }
                            Play.PlaySong();
                            state = MainStates.playing;
                            drawOnce = true;
                        }
                        else if (Utils.currentMusic.PlaybackState == PlaybackState.Stopped && Utils.audioStream != null)
                        {
                            state = MainStates.play;
                            drawOnce = true;
                        }
                        else
                        //NOTE(ra) Resumed is not called at all. PlaySong resumes after pause.
                        {
                            Play.ResumeSong();
                        }
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
                        if (Utils.audioStream == null) {
                            Debug.dprint("No Next");
                            break;
                        }
                        state = MainStates.next; // next song
                        break;
                    case ConsoleKey.P:
                        if (Utils.audioStream == null) {
                            Debug.dprint("No Prev");
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
                        break;
                    case ConsoleKey.DownArrow: // volume down
                        Play.ModifyVolume(-Preferences.GetChangeVolumeBy());
                        break;
                    case ConsoleKey.S: // suffle
                        Preferences.isShuffle = !Preferences.isShuffle;
                        break;
                    case ConsoleKey.L: // loop
                        Preferences.isLoop = !Preferences.isLoop;
                        break;
                    case ConsoleKey.M: // mute
                        Play.ToggleMute();
                        break;
                    case ConsoleKey.F: // show all view
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
                        AnsiConsole.Markup("\nEnter forward seconds: ");
                        string? forwardSecondsString = Console.ReadLine();
                        if (int.TryParse(forwardSecondsString, out int forwardSeconds))
                        {
                            Preferences.forwardSeconds = forwardSeconds;
                            TUI.RehreshCurrentView();
                        }
                        else
                        {
                            AnsiConsole.Markup("[red]\nInvalid input.[/] Press any key to continue.");
                            Console.ReadKey(true);
                        }
                        TUI.RehreshCurrentView();
                        break;
                    case ConsoleKey.D2: // set rewind seek to 2 seconds
                        AnsiConsole.Markup("\nEnter rewind seconds: ");
                        string? rewindSecondsString = Console.ReadLine();
                        if (int.TryParse(rewindSecondsString, out int rewindSeconds))
                        {
                            Preferences.rewindSeconds = rewindSeconds;
                            TUI.RehreshCurrentView();
                        }
                        else
                        {
                            AnsiConsole.Markup("[red]\nInvalid input.[/] Press any key to continue.");
                            Console.ReadKey(true);
                        }
                        TUI.RehreshCurrentView();
                        break;
                    case ConsoleKey.D3: // set volume change to 3
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
                        Preferences.isAutoSave = !Preferences.isAutoSave;
                        TUI.RehreshCurrentView();
                        break;
                    case ConsoleKey.Tab:
                        TUI.Help();

                        AnsiConsole.MarkupLine("\nPress any key to continue.");
                        Console.ReadKey(true);
                        break;
                    case ConsoleKey.Delete:
                        Play.DeleteSong(Utils.currentSongIndex);
                        break;
                }

                // clear id not help or settings
                if (playerView != "help" && playerView != "settings")
                {
                    AnsiConsole.Clear();
                }
                TUI.DrawPlayer();

                Preferences.SaveSettings();
            }
        }
    }
}
