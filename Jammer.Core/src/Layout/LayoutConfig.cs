namespace Jammer
{
    public class LayoutConfig
    {
        // Magic number constants from TUI.cs analysis
        public const int MAIN_TABLE_WIDTH_OFFSET = 8;      // consoleWidth - 8
        public const int PLAYLIST_NAME_WIDTH_OFFSET = 20;  // consoleWidth - 20
        public const int PROGRESS_BAR_WIDTH_OFFSET = 10;   // consoleWidth - 10
        public const int DEFAULT_VIEW_MAGIC_INDEX = 18;    // magicIndex = 18 for default view
        public const int ALL_VIEW_MAGIC_INDEX = 22;        // magicIndex = 22 for all view
        public const int VISUALIZER_X_POSITION = 5;        // SetPosition(5, ...)
        public const int VISUALIZER_Y_OFFSET = 5;          // consoleHeight - 5
        public const int TIME_Y_OFFSET = 3;                // consoleHeight - 3
        public const int VISUAL_WIDTH_ADJUSTMENT = 35;     // consoleWidth + 35 for visualizer
        public const int MAGIC_INDEX_NO_PLAYLIST_ADJUSTMENT = 2;  // magicIndex -= 2
        public const int MAGIC_INDEX_VISUALIZER_ADJUSTMENT = 1;   // magicIndex++ for visualizer

        private readonly int _consoleWidth;
        private readonly int _consoleHeight;

        public LayoutConfig(int consoleWidth, int consoleHeight)
        {
            _consoleWidth = consoleWidth;
            _consoleHeight = consoleHeight;
        }

        public int ConsoleWidth => _consoleWidth;
        public int ConsoleHeight => _consoleHeight;

        // Responsive calculation methods
        public int CalculateMainTableWidth()
        {
            return _consoleWidth - MAIN_TABLE_WIDTH_OFFSET;
        }

        public int CalculatePlaylistNameWidth()
        {
            return _consoleWidth - PLAYLIST_NAME_WIDTH_OFFSET;
        }

        public int CalculateProgressBarWidth()
        {
            return _consoleWidth - PROGRESS_BAR_WIDTH_OFFSET;
        }

        public int CalculateTableRowCount(ViewType viewType, bool hasVisualizer, bool hasPlaylist = true, int songsCount = 0)
        {
            int magicIndex;
            
            if (viewType == ViewType.Default)
            {
                magicIndex = DEFAULT_VIEW_MAGIC_INDEX;
                if (!hasPlaylist)
                {
                    magicIndex -= MAGIC_INDEX_NO_PLAYLIST_ADJUSTMENT;
                }
                if (hasVisualizer)
                {
                    magicIndex += MAGIC_INDEX_VISUALIZER_ADJUSTMENT;
                }
            }
            else // ViewType.All
            {
                magicIndex = ALL_VIEW_MAGIC_INDEX;
                if (hasVisualizer)
                {
                    magicIndex += MAGIC_INDEX_VISUALIZER_ADJUSTMENT;
                }
                // Adjust for playlists with less than 5 songs
                if (songsCount < 5)
                {
                    magicIndex += songsCount - 5;
                }
            }

            int tableRowCount = _consoleHeight - magicIndex;
            return tableRowCount < 0 ? 0 : tableRowCount;
        }

        public (int X, int Y) GetVisualizerPosition()
        {
            return (VISUALIZER_X_POSITION, _consoleHeight - VISUALIZER_Y_OFFSET);
        }

        public (int X, int Y) GetTimePosition()
        {
            return (VISUALIZER_X_POSITION, _consoleHeight - TIME_Y_OFFSET);
        }

        public int CalculateVisualWidth()
        {
            return _consoleWidth + VISUAL_WIDTH_ADJUSTMENT;
        }

        public int CalculateTopMessageWidth(int messageLength)
        {
            return _consoleWidth - messageLength - 4; // 4 accounts for border/padding
        }
    }

    public enum ViewType
    {
        Default,
        All
    }
}