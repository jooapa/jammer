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
                Directory.Delete(Preferences.songsPath, true);
                #if CLI_UI
                AnsiConsole.MarkupLine($"[green]Jammer songs flushed.[/]");
                #endif
                #if AVALONIA_UI
                // TODO AVALONIA_UI
                #endif
            }
            else
            {
                #if CLI_UI
                AnsiConsole.MarkupLine($"[red]Jammer songs folder not found.[/]");
                #endif
                #if AVALONIA_UI
                // TODO AVALONIA_UI
                #endif
            }
        }
    }
}