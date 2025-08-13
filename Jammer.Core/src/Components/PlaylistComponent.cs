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
        private string _currentPlaylist;
        private string _backupPlaylistName;
        private string[] _songs;

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
            table.Border = Themes.bStyle(Themes.CurrentTheme.GeneralPlaylist.BorderStyle);
            table.BorderColor(Themes.bColor(Themes.CurrentTheme.GeneralPlaylist.BorderColor));

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
            table.Border = Themes.bStyle(Themes.CurrentTheme.WholePlaylist.BorderStyle);
            table.BorderColor(Themes.bColor(Themes.CurrentTheme.WholePlaylist.BorderColor));

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
                    Themes.sColor(Utils.RssFeedSong.Title, Themes.CurrentTheme.Rss.TitleColor) + " - " +
                    Themes.sColor(Utils.RssFeedSong.Author, Themes.CurrentTheme.Rss.AuthorColor) +
                    " [i]" + Themes.sColor("(Exit Rss Feed with " + Keybindings.ExitRssFeed + ")", Themes.CurrentTheme.Rss.ExitRssFeedColor) + "[/]"
                );
            }
            else
            {
                string playlistInfo = Themes.sColor(Locale.Player.Playlist, Themes.CurrentTheme.Playlist.RandomTextColor) + " " +
                    Themes.sColor(_backupPlaylistName, Themes.CurrentTheme.Playlist.PlaylistNameColor) + " -> " +
                    Themes.sColor(Utils.RssFeedSong.Title, Themes.CurrentTheme.Rss.TitleColor) + " - " +
                    Themes.sColor(Utils.RssFeedSong.Author, Themes.CurrentTheme.Rss.AuthorColor) +
                    " [i]" + Themes.sColor("(Exit Rss Feed with " + Keybindings.ExitRssFeed + ")", Themes.CurrentTheme.Rss.ExitRssFeedColor) + "[/]";

                if (_currentPlaylist != "")
                {
                    playlistInfo += Themes.sColor(" saved as: ", Themes.CurrentTheme.Rss.DescriptionColor) +
                        Themes.sColor(_currentPlaylist, Themes.CurrentTheme.Rss.DescriptionColor);
                }

                table.AddColumn(playlistInfo);
            }
            
            table.AddRow(Funcs.GetPrevCurrentNextSong());
        }

        private void RenderNormalPlaylistContent(Table table, LayoutConfig layout)
        {
            if (_currentPlaylist == "")
            {
                table.AddColumn(Funcs.GetPrevCurrentNextSong());
            }
            else
            {
                table.AddColumn(
                    Themes.sColor(Locale.Player.Playlist, Themes.CurrentTheme.Playlist.RandomTextColor) + " " +
                    Themes.sColor(
                        Funcs.GetSongWithDots(
                            Playlists.GetJammerPlaylistVisualPath(_currentPlaylist),
                            layout.CalculatePlaylistNameWidth()),
                        Themes.CurrentTheme.Playlist.PlaylistNameColor)
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
                table.AddColumn(
                    Themes.sColor(Locale.Player.Playlist, Themes.CurrentTheme.Playlist.RandomTextColor) + " " +
                    Themes.sColor(
                        Funcs.GetSongWithDots(
                            Playlists.GetJammerPlaylistVisualPath(_currentPlaylist),
                            layout.CalculatePlaylistNameWidth()),
                        Themes.CurrentTheme.Playlist.PlaylistNameColor)
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