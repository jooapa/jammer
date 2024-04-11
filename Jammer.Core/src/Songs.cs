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
                if (Start.CLI) {
                AnsiConsole.MarkupLine($"[green]Jammer songs flushed.[/]");
                } else {
                
                    // TODO AVALONIA_UI
                }
            }
            else
            {
                if (Start.CLI) {
                AnsiConsole.MarkupLine($"[red]Jammer songs folder not found.[/]");
                } else {
                
                    // TODO AVALONIA_UI
                }
            }
        }
    }
}