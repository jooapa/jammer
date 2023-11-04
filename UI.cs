using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace jammer
{
    internal class UI
    {
        static public string Ui(WaveOutEvent outputDevice)
        {
            var loopText = Program.isLoop ? "looping: true" : "looping: false";
            var isPlayingText = outputDevice.PlaybackState == PlaybackState.Playing ? "Playing" : "Paused";
            var ismuteText = Program.isMuted ? "Muted" : "";

            // currentPositionInSeconds
            int cupMinutes = (int)(Program.currentPositionInSeconds / 60);
            int cupSeconds = (int)(Program.currentPositionInSeconds % 60);

            string currentPositionInSecondsText = $"{cupMinutes}:{cupSeconds:D2}";


            return "Current Position: " + currentPositionInSecondsText + " / " + Program.positionInSecondsText + " minutes\n" +
            "\nPress 'Up Arrow' to increase volume, 'Down Arrow' to decrease volume, and 'Q' to quit.\n" +
            "Press 'Left Arrow' to rewind 5 seconds, 'Right Arrow' to fast forward 5 seconds.\n" +
            loopText + "\n" +
            isPlayingText + "\n" +
            "Volume: " + Math.Round(outputDevice.Volume * 100) + "%" + "\n" +
            ismuteText;
        }
    }
}
