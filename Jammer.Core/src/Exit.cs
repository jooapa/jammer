using Spectre.Console;
using ManagedBass;

namespace Jammer {
    public static class Exit
    {
        public static void OnExit(object sender, ConsoleCancelEventArgs args)
        {
            Debug.dprint("OnExit");
            Bass.Free();
            AnsiConsole.Clear();
            AnsiConsole.Cursor.Show();
            Environment.Exit(0);
        }

        public static void OnProcessExit(object sender, EventArgs e)
        {
            Debug.dprint("OnProcessExit");
            Bass.Free();
            AnsiConsole.Clear();
            AnsiConsole.Cursor.Show();
        }
    }
}