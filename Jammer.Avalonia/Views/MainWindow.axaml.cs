using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Diagnostics;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;
using Avalonia;
using Jammer;
using Avalonia.Data;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace Jammer.Avalonia.Views;

public partial class MainWindow : Window
{
    /// <summary>
    /// Current view in screen.
    /// 1. Playlist
    /// 2. Settings
    /// 3. Song folder
    /// 4. Current playlist
    /// </summary>
    private static string CurrentView = "Playlist";
    public Grid mainGridArea;
    public StackPanel mainGridStackPanelArea;

    public MainWindow() {
        /// TEMPORARY
        Start.Run(new string[] { "-p a" });

        InitializeComponent();

        CurrentView = "Song folder";
        mainGridArea = this?.FindControl<Grid>("MainArea");
        mainGridStackPanelArea = this?.FindControl<StackPanel>("MainAreaStackPanel");

        ChangeMainAreaDisplay();

        this.Closing += MainWindow_Closing;
        this.Resized += MainWindow_Resized;
    }
    /// <summary>
    /// Called when main window is closed. Shuts down loop inside start.
    /// </summary>
    /// <param name="sender">no use for now</param>
    /// <param name="e">no use for now</param>
    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e) {
        Start.LoopRunning = false;
    }

    /// <summary>
    /// Called when window gets resized
    /// </summary>
    /// <param name="sender">no use for now</param>
    /// <param name="e">no use for now</param>
    private void MainWindow_Resized(object? sender, EventArgs e) {
        // 100, 40 values from MainWindow.axaml
        // Calculate the new height for mainGridArea
        double newHeight = Height - 100 - 40; // Assuming 100 is the top margin and 40 is the bottom margin

        // Update the Height property of mainGridArea
        mainGridArea.Height = newHeight;
    }

    public void PlayPauseSong(object sender, RoutedEventArgs args) {
        Start.PauseSong();
    }
    public void PlayNextSong(object sender, RoutedEventArgs args) {
        Start.state = MainStates.next;
    }
    public void PlayPreviousSong(object sender, RoutedEventArgs args) {
        Start.state = MainStates.previous;
    }


    /// <summary>
    /// Displays songs to main area
    /// </summary>
    /// <param name="songs">String array of song names in path</param>
    public static void DisplaySongs(string[] songs) {

    }

    public void ChangeMainAreaDisplay() {
        switch (CurrentView) {
            case "Playlist": 
                break;
            case "Settings": 
                break;

            /// Get all songs in current jammer song path
            case "Song folder":
                try {
                    string dir = Preferences.GetSongsPath();
                    // Check if the directory exists
                    if (Directory.Exists(dir)) {
                        // Get all files in the folder
                        string[] filepaths = Directory.GetFiles(dir);

                        // Process each file
                        foreach (string filepath in filepaths) {
                            System.Diagnostics.Debug.WriteLine($"File found: {filepath}, aka {Play.Title(filepath, "getMeta")}");
                            TextBlock textBlock = new() {
                                Margin = new Thickness(5),
                                Text = $"{Play.Title(filepath, "getMeta")} — {Play.Author(filepath)}",
                                Foreground = Brushes.Red
                            };
                            mainGridStackPanelArea.Children.Add(textBlock);
                        }
                    } else {
                        Console.WriteLine($"The directory '{dir}' does not exist.");
                        System.Diagnostics.Debug.WriteLine($"The directory '{dir}' does not exist.");
                    }
                } catch (Exception ex) {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"An error occurred: {ex.Message}");
                }
                break;
            case "Current playlist": 
                break;
        }
    }

    public static void ChangeVolume(object sender, AvaloniaPropertyChangedEventArgs args) {
        //System.Diagnostics.Debug.WriteLine(args.GetNewValue<int>());
    }



    /// <summary>
    /// Ensures that the divider keeps the playlist column width within the allowed range of 100 to (some) pixels.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public static void PlaylistDividerMovement(object sender, PointerEventArgs args) {
        int maxColumnWidth = 200;
        int minColumnWidth = 100;

        // Casting the 'sender' to GridSplitter
        GridSplitter splitter = sender as GridSplitter;
        // Get the parent grid of the splitter
        Grid parentGrid = splitter.Parent as Grid;
        // Find the index of the column where the splitter is located
        int columnIndex = Grid.GetColumn(splitter);
        // Get the total number of columns in the grid
        int totalColumns = parentGrid.ColumnDefinitions.Count;
        // Calculate the width of each column
        double columnWidth = parentGrid.Bounds.Width / totalColumns;
        // Calculate the position of the splitter relative to its parent grid
        double positionX = args.GetPosition(parentGrid).X;
        // Ensure the position is within the allowed range
        if (positionX < minColumnWidth) {
            // Set the width of the first column to ensure the splitter stays at 100
            parentGrid.ColumnDefinitions[0].Width = new GridLength(minColumnWidth, GridUnitType.Pixel);
        } else if (positionX > maxColumnWidth) {
            // Set the width of the second column to ensure the splitter stays at 350
            parentGrid.ColumnDefinitions[0].Width = new GridLength(maxColumnWidth, GridUnitType.Pixel);
        }
    }   

}