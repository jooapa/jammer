namespace Jammer
{
    public static class LayoutCalculator
    {
        /// <summary>
        /// Calculates the magic index for table row count based on current view state
        /// </summary>
        public static int CalculateMagicIndex(ViewType viewType, bool hasVisualizer, bool hasPlaylist, int songsCount = 0)
        {
            int magicIndex;
            
            if (viewType == ViewType.Default)
            {
                magicIndex = LayoutConfig.DEFAULT_VIEW_MAGIC_INDEX;
                
                // Adjust for playlist state - corresponds to original logic:
                // if ((Utils.CurrentPlaylist == "" && !Funcs.IsInsideOfARssFeed()))
                if (!hasPlaylist)
                {
                    magicIndex -= LayoutConfig.MAGIC_INDEX_NO_PLAYLIST_ADJUSTMENT;
                }
                
                if (hasVisualizer)
                {
                    magicIndex += LayoutConfig.MAGIC_INDEX_VISUALIZER_ADJUSTMENT;
                }
            }
            else // ViewType.All
            {
                magicIndex = LayoutConfig.ALL_VIEW_MAGIC_INDEX;
                
                if (hasVisualizer)
                {
                    magicIndex += LayoutConfig.MAGIC_INDEX_VISUALIZER_ADJUSTMENT;
                }
                
                // Handle playlists with less than 5 songs - corresponds to original logic:
                // if (Utils.Songs.Length < 5) { magicIndex += Utils.Songs.Length; magicIndex -= 5; }
                if (songsCount < 5)
                {
                    magicIndex += songsCount - 5;
                }
            }
            
            return magicIndex;
        }

        /// <summary>
        /// Calculates the final table row count ensuring it's never negative
        /// </summary>
        public static int CalculateTableRowCount(int consoleHeight, ViewType viewType, bool hasVisualizer, bool hasPlaylist, int songsCount = 0)
        {
            int magicIndex = CalculateMagicIndex(viewType, hasVisualizer, hasPlaylist, songsCount);
            int tableRowCount = consoleHeight - magicIndex;
            return tableRowCount < 0 ? 0 : tableRowCount;
        }

        /// <summary>
        /// Determines view type from string representation
        /// </summary>
        public static ViewType GetViewType(string playerView)
        {
            return playerView switch
            {
                "all" => ViewType.All,
                "default" => ViewType.Default,
                _ => ViewType.Default
            };
        }

        /// <summary>
        /// Calculates cursor position for visualizer
        /// </summary>
        public static (int X, int Y) GetVisualizerCursorPosition(int consoleHeight)
        {
            return (LayoutConfig.VISUALIZER_X_POSITION, consoleHeight - LayoutConfig.VISUALIZER_Y_OFFSET);
        }

        /// <summary>
        /// Calculates cursor position for time display
        /// </summary>
        public static (int X, int Y) GetTimeCursorPosition(int consoleHeight)
        {
            return (LayoutConfig.VISUALIZER_X_POSITION, consoleHeight - LayoutConfig.TIME_Y_OFFSET);
        }

        /// <summary>
        /// Calculates width adjustments for various UI components
        /// </summary>
        public static class WidthCalculator
        {
            public static int MainTableWidth(int consoleWidth) => 
                consoleWidth - LayoutConfig.MAIN_TABLE_WIDTH_OFFSET;

            public static int PlaylistNameWidth(int consoleWidth) => 
                consoleWidth - LayoutConfig.PLAYLIST_NAME_WIDTH_OFFSET;

            public static int ProgressBarWidth(int consoleWidth) => 
                consoleWidth - LayoutConfig.PROGRESS_BAR_WIDTH_OFFSET;

            public static int VisualizerWidth(int consoleWidth) => 
                consoleWidth + LayoutConfig.VISUAL_WIDTH_ADJUSTMENT;
        }
    }
}