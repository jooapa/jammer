using Spectre.Console;
using Jammer;

namespace Jammer.Components
{
    /// <summary>
    /// Component responsible for rendering player time information including progress bar
    /// Extracted from TUI.UIComponent_Time and TUI.DrawTime methods
    /// </summary>
    public class PlayerTimeComponent : IUIComponent, IDirectRenderer, IStatefulComponent
    {
        private double _currentTime;
        private double _totalTime;

        public PlayerTimeComponent()
        {
            UpdateState();
        }

        public void UpdateState()
        {
            _currentTime = Utils.TotalMusicDurationInSec;
            _totalTime = Utils.SongDurationInSec;
        }

        public Table Render(LayoutConfig layout)
        {
            var table = new Table();
            table.Border = Themes.bStyle(Themes.CurrentTheme.Time.BorderStyle);
            table.BorderColor(Themes.bColor(Themes.CurrentTheme.Time.BorderColor));
            table.AddColumn(TUI.ProgressBar(_currentTime, _totalTime, layout));
            return table;
        }

        public (int X, int Y) CalculatePosition(LayoutConfig layout)
        {
            return layout.GetTimePosition();
        }

        public void RenderDirect(LayoutConfig layout)
        {
            var position = CalculatePosition(layout);
            AnsiConsole.Cursor.SetPosition(position.X, position.Y);
            AnsiConsole.MarkupLine(TUI.ProgressBar(_currentTime, _totalTime, layout));
        }

        /// <summary>
        /// Creates a table suitable for inclusion in the main UI table
        /// This is the primary method used by the main TUI orchestrator
        /// </summary>
        /// <param name="layout">Layout configuration</param>
        /// <returns>Configured table with progress bar</returns>
        public static Table CreateTimeTable(LayoutConfig layout)
        {
            var component = new PlayerTimeComponent();
            return component.Render(layout);
        }

        /// <summary>
        /// Renders time directly to console (for standalone time display)
        /// </summary>
        /// <param name="layout">Layout configuration</param>
        public static void DrawTimeToConsole(LayoutConfig layout)
        {
            var component = new PlayerTimeComponent();
            component.RenderDirect(layout);
        }
    }
}