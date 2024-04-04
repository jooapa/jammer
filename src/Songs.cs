using Spectre.Console;


namespace jammer
{
    public static class Songs
    {
        public static void Flush()
        {
            if (Directory.Exists(Preferences.songsPath))
            {
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