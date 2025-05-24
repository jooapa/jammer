using Spectre.Console;
using System.IO;

namespace Jammer
{
    public static class Songs
    {
        public static void Flush()
        {
            if (Directory.Exists(Preferences.songsPath))
            {
                string v = Message.Input("Are you sure you want to Recursively delete '" + Preferences.songsPath + "'? (y/n)", "Flush Jammer Songs", true);
                if (v != "y")
                {
                    AnsiConsole.Clear();
                    AnsiConsole.MarkupLine($"[red]Jammer songs flush cancelled.[/]");
                    return;
                }
                Directory.Delete(Preferences.songsPath, true);
                AnsiConsole.MarkupLine($"[green]Jammer songs flushed.[/]");

            }
            else
            {
                AnsiConsole.MarkupLine($"[red]Jammer songs folder not found.[/]");

            }
        }
    }
}