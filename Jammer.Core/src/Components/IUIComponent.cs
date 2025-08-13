using Spectre.Console;

namespace Jammer.Components
{
    /// <summary>
    /// Base interface for all UI components that can render tables
    /// </summary>
    public interface IUIComponent
    {
        /// <summary>
        /// Renders the component using the provided layout configuration
        /// </summary>
        /// <param name="layout">Layout configuration for positioning and sizing</param>
        /// <returns>Rendered table for this component</returns>
        Table Render(LayoutConfig layout);
    }

    /// <summary>
    /// Interface for components that need specific positioning
    /// </summary>
    public interface IPositionable
    {
        /// <summary>
        /// Calculates the position where this component should be placed
        /// </summary>
        /// <param name="layout">Layout configuration for position calculations</param>
        /// <returns>X, Y coordinates for component placement</returns>
        (int X, int Y) CalculatePosition(LayoutConfig layout);
    }

    /// <summary>
    /// Interface for components that render directly to console (like visualizer)
    /// </summary>
    public interface IDirectRenderer : IPositionable
    {
        /// <summary>
        /// Renders the component directly to the console at calculated position
        /// </summary>
        /// <param name="layout">Layout configuration for positioning</param>
        void RenderDirect(LayoutConfig layout);
    }

    /// <summary>
    /// Interface for components that need state information for rendering
    /// </summary>
    public interface IStatefulComponent
    {
        /// <summary>
        /// Updates the component's internal state before rendering
        /// </summary>
        void UpdateState();
    }
}