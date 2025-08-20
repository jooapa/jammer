using Spectre.Console;

namespace Jammer.Components
{
    /// <summary>
    /// Component responsible for rendering playlist information and songs
    /// Extracted from TUI.UIComponent_Normal and TUI.UIComponent_Songs methods
    /// </summary>
    public class PlaylistComponent : IUIComponent, IStatefulComponent
    {
        private ViewType _viewType;
        private bool _isInsideRssFeed;
        private string _currentPlaylist = string.Empty;
        private string? _backupPlaylistName;
        private string[] _songs = Array.Empty<string>();

        public PlaylistComponent(ViewType viewType)
        {
            _viewType = viewType;
            UpdateState();
        }

        public void UpdateState()
        {
            _isInsideRssFeed = Funcs.IsInsideOfARssFeed();
            _currentPlaylist = Utils.CurrentPlaylist;
            _backupPlaylistName = Utils.BackUpPlaylistName;
            
            if (_viewType == ViewType.All)
            {
                _songs = Funcs.GetAllSongs();
            }
        }

        public Table Render(LayoutConfig layout)
        {
            var table = new Table();
            
            if (_viewType == ViewType.Default)
            {
                RenderNormalView(table, layout);
            }
            else // ViewType.All
            {
                RenderAllSongsView(table, layout);
            }

            return table;
        }

        private void RenderNormalView(Table table, LayoutConfig layout)
        {
            table.Border = Themes.bStyle(Themes.CurrentTheme?.GeneralPlaylist?.BorderStyle ?? "rounded");
            table.BorderColor(Themes.bColor(Themes.CurrentTheme?.GeneralPlaylist?.BorderColor ?? new int[] {255, 255, 255}));

            if (_isInsideRssFeed)
            {
                RenderRssFeedContent(table, layout);
            }
            else
            {
                RenderNormalPlaylistContent(table, layout);
            }
        }

        private void RenderAllSongsView(Table table, LayoutConfig layout)
        {
            table.Border = Themes.bStyle(Themes.CurrentTheme?.WholePlaylist?.BorderStyle ?? "rounded");
            table.BorderColor(Themes.bColor(Themes.CurrentTheme?.WholePlaylist?.BorderColor ?? new int[] {255, 255, 255}));

            if (_isInsideRssFeed)
            {
                RenderRssFeedContent(table, layout);
            }
            else
            {
                RenderAllSongsContent(table, layout);
            }

            // Add all songs to the table
            if (_songs != null)
            {
                for (int i = 0; i < _songs.Length; i++)
                {
                    table.AddRow(_songs[i]);
                }
            }
        }

        private void RenderRssFeedContent(Table table, LayoutConfig layout)
        {
            if (_backupPlaylistName == "")
            {
                table.AddColumn(
                    Themes.sColor(Utils.RssFeedSong.Title, Themes.CurrentTheme?.Rss?.TitleColor ?? "white") + " - " +
                    Themes.sColor(Utils.RssFeedSong.Author, Themes.CurrentTheme?.Rss?.AuthorColor ?? "white") +
                    " [i]" + Themes.sColor("(Exit Rss Feed with " + Keybindings.ExitRssFeed + ")", Themes.CurrentTheme?.Rss?.ExitRssFeedColor ?? "white") + "[/]"
                );
            }
            else
            {
                string playlistInfo = Themes.sColor(Locale.Player.Playlist, Themes.CurrentTheme?.Playlist?.RandomTextColor ?? "white") + " " +
                    Themes.sColor(_backupPlaylistName, Themes.CurrentTheme?.Playlist?.PlaylistNameColor ?? "white") + " -> " +
                    Themes.sColor(Utils.RssFeedSong.Title, Themes.CurrentTheme?.Rss?.TitleColor ?? "white") + " - " +
                    Themes.sColor(Utils.RssFeedSong.Author, Themes.CurrentTheme?.Rss?.AuthorColor ?? "white") +
                    " [i]" + Themes.sColor("(Exit Rss Feed with " + Keybindings.ExitRssFeed + ")", Themes.CurrentTheme?.Rss?.ExitRssFeedColor ?? "white") + "[/]";

                if (_currentPlaylist != "")
                {
                    playlistInfo += Themes.sColor(" saved as: ", Themes.CurrentTheme?.Rss?.DescriptionColor ?? "white") +
                        Themes.sColor(_currentPlaylist, Themes.CurrentTheme?.Rss?.DescriptionColor ?? "white");
                }

                table.AddColumn(playlistInfo);
            }
            
            string current = string.Empty;
            string next = string.Empty;
            string previous = string.Empty;

            // get the previous, current, and next songs PubDate. and assign it to the variables
            if (Utils.CurrentSongIndex >= 0 && Utils.CurrentSongIndex < Utils.Songs.Length && Utils.Songs[Utils.CurrentSongIndex] != null)
                current = SongExtensions.ToSong(Utils.Songs[Utils.CurrentSongIndex])?.PubDate?.ToString() ?? string.Empty;
            else
                current = Locale.Player.Current;

            if (Utils.CurrentSongIndex + 1 < Utils.Songs.Length && Utils.Songs[Utils.CurrentSongIndex + 1] != null)
                next = SongExtensions.ToSong(Utils.Songs[Utils.CurrentSongIndex + 1])?.PubDate?.ToString() ?? string.Empty;
            else
                next = Locale.Player.Next;
                
            if (Utils.CurrentSongIndex - 1 >= 0 && Utils.Songs[Utils.CurrentSongIndex - 1] != null)
                previous = SongExtensions.ToSong(Utils.Songs[Utils.CurrentSongIndex - 1])?.PubDate?.ToString() ?? string.Empty;
            else
                previous = Locale.Player.Previos;

            table.AddRow(Funcs.GetPrevCurrentNextSong(current, previous, next));
        }

        private void RenderNormalPlaylistContent(Table table, LayoutConfig layout)
        {
            if (_currentPlaylist == "")
            {
                table.AddColumn(Funcs.GetPrevCurrentNextSong());
            }
            else
            {
                string playlistName = Funcs.GetSongWithDots(
                    Playlists.GetJammerPlaylistVisualPath(_currentPlaylist),
                    layout.CalculatePlaylistNameWidth());
                
                table.AddColumn(
                    Themes.sColor(Locale.Player.Playlist, Themes.CurrentTheme?.Playlist?.RandomTextColor ?? "white") + " " +
                    Themes.sColor(playlistName, Themes.CurrentTheme?.Playlist?.PlaylistNameColor ?? "white") +
                    Themes.sColor(Funcs.GetPlaylistPositionText(playlistName), Themes.CurrentTheme?.Playlist?.RandomTextColor ?? "white")
                );
                table.AddRow(Funcs.GetPrevCurrentNextSong());
            }
        }

        private void RenderAllSongsContent(Table table, LayoutConfig layout)
        {
            if (_currentPlaylist == "")
            {
                table.AddColumn("No Specific Playlist Name");
            }
            else
            {
                string playlistName = Funcs.GetSongWithDots(
                    Playlists.GetJammerPlaylistVisualPath(_currentPlaylist),
                    layout.CalculatePlaylistNameWidth());
                
                table.AddColumn(
                    Themes.sColor(Locale.Player.Playlist, Themes.CurrentTheme?.Playlist?.RandomTextColor ?? "white") + " " +
                    Themes.sColor(playlistName, Themes.CurrentTheme?.Playlist?.PlaylistNameColor ?? "white") +
                    Themes.sColor(Funcs.GetPlaylistPositionText(playlistName), Themes.CurrentTheme?.Playlist?.RandomTextColor ?? "white")
                );
            }
        }

        /// <summary>
        /// Creates a playlist table for normal view
        /// </summary>
        public static Table CreateNormalPlaylistTable(LayoutConfig layout)
        {
            var component = new PlaylistComponent(ViewType.Default);
            return component.Render(layout);
        }

        /// <summary>
        /// Creates a playlist table for all songs view
        /// </summary>
        public static Table CreateAllSongsPlaylistTable(LayoutConfig layout)
        {
            var component = new PlaylistComponent(ViewType.All);
            return component.Render(layout);
        }
    }
}