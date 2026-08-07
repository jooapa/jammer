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
                string v = Message.Input(
                    string.Format(Locale.UiMessages.ConfirmDeleteSongs, Preferences.songsPath, Locale.Miscellaneous.YesNo),
                    Locale.UiMessages.FlushSongsTitle,
                    true);
                if (!v.Equals(Locale.Miscellaneous.YesAnswer, StringComparison.OrdinalIgnoreCase))
                {
                    AnsiConsole.Clear();
                    AnsiConsole.MarkupLine($"[red]{Locale.UiMessages.SongsFlushCancelled}[/]");
                    return;
                }
                Directory.Delete(Preferences.songsPath, true);
                AnsiConsole.MarkupLine($"[green]{Locale.UiMessages.SongsFlushed}[/]");

            }
            else
            {
                AnsiConsole.MarkupLine($"[red]{Locale.UiMessages.SongsFolderNotFound}[/]");

            }
        }
    }
}
