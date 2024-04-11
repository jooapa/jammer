using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Diagnostics;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;
using Avalonia;
using Jammer;

namespace Jammer.Avalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        Start.Run(Array.Empty<string>());
        InitializeComponent();
    }

    public void PlayPauseSong(object sender, RoutedEventArgs args) {
        System.Diagnostics.Debug.WriteLine("Play/Pause button clicked");
    }
    public void PlayNextSong(object sender, RoutedEventArgs args) {
        System.Diagnostics.Debug.WriteLine("PlayNextSong button clicked");
    }
    public void PlayPreviousSong(object sender, RoutedEventArgs args) {
        System.Diagnostics.Debug.WriteLine("PlayPreviousSong button clicked");
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