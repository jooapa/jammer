using Spectre.Console;

namespace Jammer.Components
{
    /// <summary>
    /// Component responsible for rendering the audio visualizer
    /// Extracted from TUI.DrawVisualizer method
    /// </summary>
    public class VisualizerComponent : IDirectRenderer, IStatefulComponent
    {
        private bool _isPlaying;

        public VisualizerComponent()
        {
            UpdateState();
        }

        public void UpdateState()
        {
            _isPlaying = Start.state == MainStates.playing || Start.state == MainStates.play;
        }

        public (int X, int Y) CalculatePosition(LayoutConfig layout)
        {
            return layout.GetVisualizerPosition();
        }

        public void RenderDirect(LayoutConfig layout)
        {
            var position = CalculatePosition(layout);
            AnsiConsole.Cursor.SetPosition(position.X, position.Y);

            int visualWidth = layout.CalculateVisualWidth();
            AnsiConsole.MarkupLine(Visual.GetSongVisual(visualWidth, _isPlaying));
        }

        /// <summary>
        /// Renders the visualizer directly to console at the calculated position
        /// </summary>
        /// <param name="layout">Layout configuration for positioning</param>
        public static void DrawVisualizerToConsole(LayoutConfig layout)
        {
            var component = new VisualizerComponent();
            component.RenderDirect(layout);
        }

        /// <summary>
        /// Checks if visualizer should be rendered based on preferences
        /// </summary>
        /// <returns>True if visualizer should be shown</returns>
        public static bool ShouldShowVisualizer()
        {
            return Preferences.isVisualizer;
        }
    }
}