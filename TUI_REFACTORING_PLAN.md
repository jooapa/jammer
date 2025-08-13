# TUI Refactoring Plan: Magic Number Elimination & Architecture Modernization

Based on analysis of the current TUI.cs implementation, this plan eliminates magic numbers and creates a maintainable, component-based architecture.

## Current State Analysis

**Problems Identified:**
- 863-line monolithic TUI class with extensive magic numbers
- Hardcoded positioning values: 18, 22, 5, 8, 10, 20, 35, 3, 2, 4
- Complex layout calculations scattered throughout methods
- Difficult to maintain and modify positioning logic
- Console positioning with hardcoded offsets like `SetPosition(5, Start.consoleHeight - 5)`

## Architecture Overview

```
CURRENT STATE:           PROPOSED STATE:
                        
TUI.cs (863 lines)  -->  TUI.cs (orchestrator)
├─ Magic numbers        ├─ Layout/
├─ Hardcoded pos.       │  ├─ LayoutConfig.cs
├─ Mixed concerns       │  └─ LayoutCalculator.cs
└─ Hard to maintain     └─ Components/
                           ├─ IUIComponent.cs
                           ├─ PlayerTimeComponent.cs
                           ├─ PlaylistComponent.cs
                           ├─ HelpMenuComponent.cs
                           ├─ SettingsComponent.cs
                           └─ VisualizerComponent.cs
```

## Phase-by-Phase Implementation Plan

### Phase 1: Layout Configuration Foundation

**1.1 Create LayoutConfig Class**
```csharp
public class LayoutConfig
{
    // Replace magic numbers with named constants
    public const int MAIN_TABLE_WIDTH_OFFSET = 8;      // consoleWidth - 8
    public const int PLAYLIST_NAME_WIDTH_OFFSET = 20;  // consoleWidth - 20
    public const int PROGRESS_BAR_WIDTH_OFFSET = 10;   // consoleWidth - 10
    public const int DEFAULT_VIEW_MAGIC_INDEX = 18;
    public const int ALL_VIEW_MAGIC_INDEX = 22;
    public const int VISUALIZER_X_POSITION = 5;
    public const int VISUALIZER_Y_OFFSET = 5;          // consoleHeight - 5
    public const int TIME_Y_OFFSET = 3;                // consoleHeight - 3
    public const int VISUAL_WIDTH_ADJUSTMENT = 35;
    public const int TOP_TEXT_SPACING_OFFSET = 4;
}
```

**1.2 Add Responsive Calculation Methods**
- `CalculateMainTableWidth(int consoleWidth)`
- `CalculatePlaylistNameWidth(int consoleWidth)`
- `CalculateProgressBarWidth(int consoleWidth)`
- `CalculateTableRowCount(int consoleHeight, ViewType viewType, bool hasVisualizer)`
- `GetVisualizerPosition(int consoleHeight)`
- `GetTimePosition(int consoleHeight)`

### Phase 2: Component Extraction

**2.1 Create UI Component Interfaces**
```csharp
public interface IUIComponent
{
    Table Render(LayoutConfig layout);
}

public interface IPositionable
{
    Position CalculatePosition(LayoutConfig layout);
}
```

**2.2 Extract Core Components**
```
Component Breakdown:
├─ PlayerTimeComponent    -> UIComponent_Time, ProgressBar
├─ PlaylistComponent      -> UIComponent_Normal, UIComponent_Songs
├─ HelpMenuComponent      -> DrawHelp
├─ SettingsComponent      -> DrawSettings
└─ VisualizerComponent    -> DrawVisualizer
```

**2.3 Create Layout Calculator**
- Centralize all positioning math in `LayoutCalculator` class
- Move complex `magicIndex` logic into dedicated methods
- Handle conditional adjustments (visualizer, playlist states)

### Phase 3: Magic Number Elimination & Integration

**3.1 Systematic Replacement**
```
Before:                           After:
Start.consoleWidth - 8       -->  layout.CalculateMainTableWidth()
Start.consoleWidth - 20      -->  layout.CalculatePlaylistNameWidth()
Start.consoleHeight - 5      -->  layout.GetVisualizerPosition().Y
magicIndex = 18/22          -->  layout.CalculateTableRowCount()
```

**3.2 Refactor TUI.cs Main Methods**
```csharp
public static void DrawPlayer()
{
    var layout = new LayoutConfig(Start.consoleWidth, Start.consoleHeight);
    var playlistComponent = new PlaylistComponent();
    var timeComponent = new PlayerTimeComponent();
    
    // Orchestrate component rendering
    mainTable.AddRow(playlistComponent.Render(layout));
    mainTable.AddRow(timeComponent.Render(layout));
}
```

**3.3 Update Method Signatures**
- All positioning methods accept `LayoutConfig` parameter
- Remove hardcoded calculations from method bodies
- Centralize layout logic in `LayoutCalculator`

### Phase 4: Testing & Validation

**4.1 Visual Regression Testing**
```
Test Matrix:
├─ UI States
│  ├─ Default player view
│  ├─ All songs view
│  ├─ Help menu
│  ├─ Settings menu
│  └─ With/without visualizer
└─ Console Sizes
   ├─ 80x25 (standard)
   ├─ 120x30 (large)
   └─ Edge cases (very small/large)
```

**4.2 Functional Testing**
- Console resize scenarios
- Theme system integration
- View transitions (default ↔ help ↔ settings ↔ all)
- Dynamic content updates
- Edge case handling

**4.3 Performance & Quality Validation**
- Rendering performance unchanged
- Memory usage similar
- Zero magic numbers remaining (grep validation)
- Consistent naming conventions
- Proper separation of concerns

## Implementation Strategy

**Sequential Approach:**
1. **Foundation First**: Layout system before components
2. **Simplest Component First**: PlayerTimeComponent (most isolated)
3. **Incremental Integration**: One component at a time
4. **Maintain Backward Compatibility**: Public API unchanged
5. **Visual Validation**: Screenshots at each step

**Risk Mitigation:**
- Git branch for each phase
- Incremental commits for selective rollback
- Visual regression testing throughout
- Preserve existing public API

## Success Metrics

- [ ] Zero magic numbers in TUI code
- [ ] Visual output identical to original
- [ ] All existing functionality preserved
- [ ] Console resizing works correctly
- [ ] Layout changes become significantly easier

## Final File Structure

```
Jammer.Core/src/
├── TUI.cs (refactored orchestrator)
├── Layout/
│   ├── LayoutConfig.cs (constants & calculations)
│   └── LayoutCalculator.cs (positioning logic)
└── Components/
    ├── IUIComponent.cs (interface)
    ├── PlayerTimeComponent.cs
    ├── PlaylistComponent.cs
    ├── HelpMenuComponent.cs
    ├── SettingsComponent.cs
    └── VisualizerComponent.cs
```

## Implementation Order

1. Create LayoutConfig + LayoutCalculator foundation
2. Extract PlayerTimeComponent (simplest, most isolated)
3. Extract PlaylistComponent (most complex logic)
4. Extract remaining components (Help, Settings, Visualizer)
5. Integration, testing, and polish

**Rollback Strategy:**
- Git branch for each phase
- Maintain original TUI.cs as backup
- Incremental commits allow selective rollback

The refactoring will transform a 863-line monolithic TUI class with scattered magic numbers into a clean, maintainable component-based architecture.