using Raylib_cs;
using Spectre.Console;

namespace jammer
{
    public partial class Start
    {
        public static void CheckKeyboard()
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true).Key;
                switch (key)
                {
                    case ConsoleKey.Spacebar:
                        if (Raylib.IsMusicReady(Utils.currentMusic) && !Raylib.IsMusicStreamPlaying(Utils.currentMusic))
                        {
                            Play.PlaySong();
                            state = MainStates.playing;
                            drawOnce = true;
                        }
                        else if (Raylib.IsMusicStreamPlaying(Utils.currentMusic))
                        {
                            Console.WriteLine("Paused");
                            state = MainStates.pause;
                            drawOnce = true;
                        }
                        else
                        //NOTE(ra) Resumed is not called at all. PlaySong resumes after pause.
                        {
                            Console.WriteLine("Resumed");
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

                    case ConsoleKey.Escape:
                        AnsiConsole.Clear();
                        Console.WriteLine("Quit");
                        Environment.Exit(0);
                        break;
                    case ConsoleKey.N:
                        state = MainStates.next; // next song
                        break;
                    case ConsoleKey.P:
                        state = MainStates.previous; // previous song
                        break;
                    case ConsoleKey.RightArrow: // move forward 5 seconds
                        Play.SeekSong(5);
                        break;
                    case ConsoleKey.LeftArrow: // move backward 5 seconds
                        Play.SeekSong(-5);
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
                        Play.MuteSong();
                        break;
                }
                TUI.DrawPlayer();
                Preferences.SaveSettings();
            }
        }
    }
}