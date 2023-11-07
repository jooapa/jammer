using System;
using NAudio.Wave;
using Spectre.Console;

namespace jammer
{
    internal class UI
    {
        static public void Ui(WaveOutEvent outputDevice)
        {
            string loopText = Program.isLoop ? "looping: true" : "looping: false";
            string isPlayingText = outputDevice.PlaybackState == PlaybackState.Playing ? "Playing" : "Paused";
            string ismuteText = Program.isMuted ? "Muted" : "";

            string songList = "";
            for (int i = 0; i < Program.songs.Length; i++)
            {
                string? item = Program.songs[i];
                if (i == Program.currentSongArgs)
                {
                    songList += "[green]";
                }
                songList += item;
                if (i == Program.currentSongArgs)
                {
                    songList += "[/]";
                }
                songList += "\n";
            }
            songList = "Playlist:\n" + songList;

            // currentPositionInSeconds
            int cupMinutes = (int)(Program.currentPositionInSeconds / 60);
            int cupSeconds = (int)(Program.currentPositionInSeconds % 60);

            string currentPositionInSecondsText = $"{cupMinutes}:{cupSeconds:D2}";

            // render table
            var tableJam = new Table();
            var table = new Table();
            var help = new Table();

            tableJam.AddColumn("♫ Jamming to: " + Program.audioFilePath + " ♫");
            if (Program.songs.Length != 1) {
                tableJam.AddRow(songList);
            }

            table.AddColumn("State");

            table.AddColumn("Current Position");

            table.AddColumn("Looping");

            table.AddColumn("Volume");

            table.AddColumn("Muted");

            table.AddRow(isPlayingText, currentPositionInSecondsText + " / " + Program.positionInSecondsText, loopText, Math.Round(outputDevice.Volume * 100) + " % " , ismuteText);

            if (!Program.helpText)
            {
                AnsiConsole.Write(tableJam);
                AnsiConsole.Write(table);
                AnsiConsole.Markup("Press [red]h[/] for help");
            }
            else
            {
                help.AddColumns("Controls", "Description");
                help.AddRow("Space", "Play/Pause");
                help.AddRow("Q", "Quit"); 
                help.AddRow("Left", "Rewind 5 seconds");
                help.AddRow("Right", "Forward 5 seconds");
                help.AddRow("Up", "Volume up");
                help.AddRow("Down", "Volume down");
                help.AddRow("L", "Toggle looping");
                help.AddRow("M", "Toggle mute");
                help.AddRow("Playlist", "");
                help.AddRow("P", "Previous song");
                help.AddRow("N", "Next song");
                help.AddRow("R", "Play random song");

                AnsiConsole.Write(help);
                AnsiConsole.Markup("Press [red]h[/] to hide help");
            }
        }
    }
}
