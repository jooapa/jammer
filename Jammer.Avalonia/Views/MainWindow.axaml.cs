using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Diagnostics;
using Avalonia.Controls.Primitives;
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
    /// 1. Playlist - Clicked on the left panel onto a playlist
    /// 2. Settings
    /// 3. Song folder
    /// 4. Current playlist
    private static string CurrentView = "Playlist";
    /// </summary>/// <summary>
    /// Current view in screen.
    /// 1. Playlist - Clicked on the left panel onto a playlist
    /// 2. -/-Settings-/-
    /// 3. Song folder
    /// 4. Current playlist
    /// </summary>
    private static string CurrentSongPlayingIn = "Playlist";
    public Grid? mainGridArea = null;
    public StackPanel? mainGridStackPanelArea = null;
    public Slider? SongTimeSlider_elem = null;
    public Slider? SongVolumeSlider_elem = null;
    private static bool hasInitialized = false;


    /// For sliders
    private static bool isClickingVolumeSlider = false;
    private static bool isClickingSongTimeSlider = false;
    private static Thread loopThread = new(() => { });

    public MainWindow() {
        /// TEMPORARY
        Start.Run(new string[] { "-p", "a" });

        InitializeComponent();
        hasInitialized = true;

        CurrentView = "Current playlist";
        CurrentSongPlayingIn = "Current playlist";

        /// Subscribe to the events
        mainGridArea = this?.FindControl<Grid>("MainArea");
        mainGridStackPanelArea = this?.FindControl<StackPanel>("MainAreaStackPanel");

        SongTimeSlider_elem = this?.FindControl<Slider>("SongTimeSlider");

        SongVolumeSlider_elem = this?.FindControl<Slider>("SongVolumeSlider");


        this.Closing += MainWindow_Closing;
        this.Resized += MainWindow_Resized;

        //loopThread = new Thread(Loop);
        //loopThread.Start();
    }

    private void Loop() {
        while (Start.LoopRunning) {                
            UpdateScreen();
            Thread.Sleep(500);
        }
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
        // 100, 40 values from MainWindow.axaml - HEADER, FOOTER respectively
        double newHeight = Height - 100 - 40;
        mainGridArea.Height = newHeight;
    }

    public void PlayPauseSong(object sender, RoutedEventArgs args) {
        Start.PauseSong();
        UpdateScreen();
    }
    public void PlayNextSong(object sender, RoutedEventArgs args) {
        Start.state = MainStates.next;
        UpdateScreen();
    }
    public void PlayPreviousSong(object sender, RoutedEventArgs args) {
        Start.state = MainStates.previous;
        UpdateScreen();
    }
    public void ShowCurrentPlaylist(object sender, RoutedEventArgs args) {
        CurrentView = "Current playlist";
        UpdateScreen();
    }
    public void ShowSongFolder(object sender, RoutedEventArgs args) {
        CurrentView = "Song folder";
        UpdateScreen();
    }
    public void ShowSettings(object sender, RoutedEventArgs args) {
        CurrentView = "Settings";
        UpdateScreen();
    }
    public void MuteUnmute(object sender, RoutedEventArgs args) {
        Play.ToggleMute();
        Preferences.SaveSettings();
        UpdateScreen();
    }


    /// <summary>
    /// Displays songs to main area
    /// </summary>
    /// <param name="songs">String array of song names in path</param>
    public void DisplaySongs(string[] songs) {
        mainGridStackPanelArea?.Children.Clear();
        int i = 0;
        foreach (string song in songs) {
            TextBlock textBlock = new() {
                Margin = new Thickness(5),
                Foreground = Brushes.White,
            };

            if(i == Utils.currentSongIndex && CurrentSongPlayingIn == CurrentView) {
                textBlock.Foreground = Brushes.Green;
            }

            string song_name = song;
            if (song.Contains('½')) {
                song_name = Play.Title(song, "get");
                textBlock.Text = $"{song_name} — {Play.Author(song.Substring(0, song.LastIndexOf('½')))}";
            } else {
                textBlock.Text = $"{Play.Title(song, "getMeta")} — {Play.Author(song)}";
            }

            // Tag, containing index and song name excluding ½
            textBlock.Tag = $"{i}/{song}";

            textBlock.PointerEntered += (sender, args) => {
                if (sender is TextBlock textBlock) {
                    try {
                        if (int.Parse(textBlock.Tag.ToString().Split('/')[0]) == Utils.currentSongIndex) {
                            textBlock.Foreground = Brushes.Chartreuse;
                        } else { 
                            textBlock.Foreground = Brushes.Yellow;
                        }
                    } catch (Exception ex) {
                        System.Diagnostics.Debug.WriteLine($"An error occurred: {ex.Message}");
                    }
                }
            };
            textBlock.PointerExited += (sender, args) => {
                if (sender is TextBlock textBlock) {
                    try {
                        if (int.Parse(textBlock.Tag.ToString().Split('/')[0] ) == Utils.currentSongIndex) {
                            textBlock.Foreground = Brushes.Green;
                        } else {
                            textBlock.Foreground = Brushes.White;
                        }
                    } catch (Exception ex) {
                        System.Diagnostics.Debug.WriteLine($"An error occurred: {ex.Message}");
                        textBlock.Foreground = Brushes.White;
                    }
                }
            };
            textBlock.PointerPressed += (sender, args) => {
                if (sender is TextBlock textBlock) {
                    Utils.currentSongIndex = int.Parse(textBlock.Tag.ToString().Split('/')[0]);
                    List<string> arg = new();
                    switch(CurrentView) {
                        case "Song folder": 
                            if(CurrentSongPlayingIn == "Current playlist") {
                                Utils.oldPlaylist = Utils.songs.ToList();
                            }
                            string dir = Preferences.GetSongsPath();
                            // Check if the directory exists
                            if (Directory.Exists(dir)) {
                                // Get all files in the folder
                                string[] filepaths = Directory.GetFiles(dir);
                                List<string> files = new();
                                // Process each file
                                foreach (string filepath in filepaths) {
                                    arg.Add(filepath);
                                }
                                Utils.songs = arg.ToArray();
                                Play.PlaySong(Utils.songs, Utils.currentSongIndex);
                                Start.state = MainStates.play;

                                ChangeMainAreaDisplay();
                            } else {
                                // TODO ERROR
                            }

                            // Update screen where song is playing
                            UpdateScreen();
                            CurrentSongPlayingIn = CurrentView;
                            break;
                        case "Current playlist":
                            if(CurrentSongPlayingIn == "Song folder") {
                                Utils.songs = Utils.oldPlaylist.ToArray();
                            }
                            Play.PlaySong(Utils.songs, Utils.currentSongIndex);    
                            Start.state = MainStates.play;
                            UpdateScreen();
                            CurrentSongPlayingIn = CurrentView;
                            break;
                    }
                }
            };

            mainGridStackPanelArea.Children.Add(textBlock);
            i++;
        }
    }

    public void ChangeMainAreaDisplay() {
        List<string> songs = new();
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
                            songs.Add(filepath);
                        }
                    } else {
                        Console.WriteLine($"The directory '{dir}' does not exist.");
                        System.Diagnostics.Debug.WriteLine($"The directory '{dir}' does not exist.");
                    }
                } catch (Exception ex) {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"An error occurred: {ex.Message}");
                }
                DisplaySongs(songs.ToArray());
                break;
            case "Current playlist":
                if (CurrentSongPlayingIn == "Song folder") {
                    DisplaySongs(Utils.oldPlaylist.ToArray());
                } else {
                    DisplaySongs(Utils.songs);
                }
                break;
        }
    }


    public void UpdateScreen() {
        ChangeMainAreaDisplay();

        
        if (Preferences.isMuted) {
            SongVolumeSlider_elem.Value = 0;
        } else {
            SongVolumeSlider_elem.Value = Preferences.volume * 100;
        }

        // Update time in sides
        TextBlock CurrSongTime = this?.FindControl<TextBlock>("CurrentSongTime");
        CurrSongTime.Text= (Math.Floor(Utils.MusicTimePlayed)).ToString();
        TextBlock FullSongTime = this?.FindControl<TextBlock>("FullSongTime");
        FullSongTime.Text = Math.Floor(Utils.currentMusicLength).ToString();

        Preferences.SaveSettings();
    }
    public static void ChangeVolume(object sender, AvaloniaPropertyChangedEventArgs args) {
        if(sender is Slider slider && hasInitialized && (args.Property.GetType() == typeof(StyledProperty<double>))) {
            Play.SetVolume((float)slider.Value / 100);
            Preferences.SaveSettings();
        }
    }

    public static void ChangeSongTimeSlider(object sender, AvaloniaPropertyChangedEventArgs args) {
        if (sender is Slider slider && hasInitialized && (args.Property.GetType() == typeof(StyledProperty<double>))) {
            double song_length = Utils.currentMusicLength;
            if (song_length != 0) {
                double new_time = slider.Value * song_length / 100;
                Utils.preciseTime = new_time;
                Play.SeekSong((float)new_time, false);
            }
        }
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