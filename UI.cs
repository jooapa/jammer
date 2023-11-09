using System;
using NAudio.Wave;
using Spectre.Console;

namespace jammer
{
    internal class UI
    {
        static public bool updated = false;
        static string songList = "";
        static bool updatedSongList = false;
        static int songListTimes = 0;
        static int times = 0;
        static public int refreshTimes = JammerFolder.GetRefreshTimes();
        static public void Ui(WaveOutEvent outputDevice)
        {
            var help = new Table();
            if (!updated || !updatedSongList) {
                if (Program.textRenderedType == "normal")
                {
                    string loopText = Program.isLoop ? "looping: true" : "looping: false";
                    string isPlayingText = outputDevice.PlaybackState == PlaybackState.Playing ? "Playing" : "Paused";
                    string ismuteText = Program.isMuted ? "Muted" : "";

                    songList = "";
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

                    AnsiConsole.Clear();
                    AnsiConsole.Write(tableJam);
                    AnsiConsole.Write(table);
                    AnsiConsole.Markup("Press [red]h[/] for help");
                    AnsiConsole.Markup("\nPress [yellow]c[/] for settings");

                    updated = true;
                    updatedSongList = true;
                }
                else if (Program.textRenderedType == "help")
                {
                    // render help
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

                    AnsiConsole.Clear();
                    AnsiConsole.Write(help);
                    AnsiConsole.Markup("Press [red]h[/] to hide help");
                    AnsiConsole.Markup("\nPress [yellow]c[/] for settings");
                }
                else if (Program.textRenderedType == "settings") {
                    var settings = new Table();

                    settings.AddColumns("Settings", "value", "Change Value");

                    settings.AddRow("refresh UI every", refreshTimes + "", "[green]1[/] to change");
                    settings.AddRow("Forward seconds", Program.forwardSeconds + " sec", "[green]2[/] to change");
                    settings.AddRow("Rewind seconds", Program.rewindSeconds + " sec", "[green]3[/] to change");

                    AnsiConsole.Clear();
                    AnsiConsole.Write(settings);
                    AnsiConsole.Markup("Press [yellow]c[/] to hide settings");
                }
            }
        }

        static public void Update()
        {
            if (times > refreshTimes)
            {
                updated = false;
                times = 0;
            }
            else
            {
                times++;
            }
        }
        static public void UpdateSongList()
        {
            if (songListTimes == 1000)
            {
                updatedSongList = false;
                songListTimes = 0;
            }
            else
            {
                songListTimes++;
            }
        }

        static public void ForceUpdate()
        {
            updated = false;
            updatedSongList = false;
        }
    }
}
