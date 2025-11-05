# Project Information

This folder contains Claude Code project documentation and metadata.

## Project Name
**VR Water Resource Serious Game (MVP)**

## Created
2025-10-25

## Description
An educational VR game for Meta Quest that teaches water resource management through immersive experience. Players draw water from a well and make decisions about usage (drinking, farming, disposal) while managing Water Volume, Water Quality, and Stamina parameters.

## Key Features
- VR hand tracking with pinch gesture interaction (Carriable system)
- Parameter management (Water, Quality, Stamina)
- Scoring system (Hygiene & Efficiency)
- Obi fluid physics for simple water visualization
- Built-in Render Pipeline (Quest optimized)

## Development History

### 2025-10-25 - Phase 0: Specification Complete

**Technical Update: Meta XR SDK Native API**
- Changed from OpenXR to Meta XR SDK native API (OVRCameraRig, OVRHand, OVRInput)
- Updated all documentation to reflect Meta XR Plugin priority
- OpenXR Plugin should be disabled or deprioritized

### 2025-10-25 - Phase 0: Specification Phase
- **Project Planning**:
  - Created SPECIFICATION.md (game mechanics, parameters)
  - Created IMPLEMENTATION_PLAN.md (10-week development plan)
  - Created DESIGN.md (system architecture, class design)
  - Created TASKS.md (detailed task checklist)
  - Created README.md (project overview)

- **Environment Setup**:
  - Configured .vscode settings (Level 3: Program files only)
  - Created .gitignore for Unity project
  - Organized dot files and folders (.claude, .vscode, .git)
  - Initialized local Git repository

- **Cleanup**:
  - Removed incompatible Boat Attack Water System (CS0619 error)
  - Deleted unnecessary files (TempAssembly.dll, .vs, .utmp)
  - Updated CLAUDE.md with new project information

### 2025-10-25 - Documentation Consolidation
- **Consolidated 3 documents into DEVELOPMENT_GUIDE.md**:
  - SPECIFICATION.md (5.9K) - Game specifications
  - DESIGN.md (19K) - System architecture and class design
  - IMPLEMENTATION_PLAN.md (8.8K) - Development phases
- **Benefits**: Single integrated guide for specifications → design → implementation
- **Updated references**: README.md, CLAUDE.md now point to DEVELOPMENT_GUIDE.md

### 2025-10-25 - Documentation Simplification
- **Simplified for small-scale development**:
  - Core functionality only, no effects/animations (handled by other programs)
  - Simple 2-layer architecture (Manager + Objects)
  - Angle-based pouring detection (`transform.eulerAngles`)
  - Parameter types: all `float`

### 2025-10-25 - Further Simplification
- **Water quality**: Displayed as numbers only (no color changes)
- **Carriable integration**: WaterContainer has no grabbing logic (implemented separately)
- **Obi Fluid usage**:
  - ✅ One-way sync only: `WaterVolume` → Obi particle count
  - ✅ Water management via `float` parameters only
  - ✅ Obi is for visuals only (cannot monitor water volume from Obi)

### 2025-10-25 - Water System Redesign (Binary State)
- **Binary water state**: `bool isFull` (Full/Empty only)
  - Full capacity: Bucket 80, Cup 10 (Inspector configurable)
  - Visual: Obi effect handles continuous display
  - Logic: 2-state management only
- **Tilt-based detection**: `IsPouringAngle()` for consumption/disposal
  - Tilt threshold: Inspector configurable (default 45°)
  - Auto-disposal: Tilt anywhere to discard water
- **Task-specific consumption**: Inspector configurable
  - WaterTap: +80 (Bucket), +10 (Cup)
  - Field: -50
  - Drinking: -10
- **All effects removed**: Numbers only, no audio/particles/animations

### 2025-10-25 - Task-Based Architecture (Automatic Result Triggering)
- **Game structure**: Task-count-based simulation (3-10 tasks, Inspector configurable)
- **2-layer architecture**: Manager Layer + Object Layer
  - Manager: GameManager only (task management, statistics)
  - Object: Containers, Tasks, UI
- **Core classes**: Simple 3-class architecture
  - `GameManager.cs` - Game control + score calculation + automatic completion check
  - `GameData.cs` - Data storage
  - `ActionHistory.cs` - Game session history
- **Automatic task management**:
  - Each action (draw, farm, drink, waste) = 1 task
  - GameManager tracks total task count automatically
  - When task count reaches `MaxTasks`, results display automatically
  - **NO buttons** - VR spawn → free actions → auto-results
- **Result screen**: Shows total water usage and pollution for entire session

**Key clarification**:
- User confirmed: NO completion button, NO start button
- VR spawns player in village, free to act
- Auto-show results when configured task count reached
- Task = individual action (not mission)

### 2025-10-25 - Water Tap System & Documentation Cleanup
- **Water source changed**: Well → WaterTap (water faucet system)
  - Obi water effect: toggle GameObject visibility to show/hide water
  - Trigger modes (Inspector configurable):
    - Button mode: Press button (e.g., A button) to start water flow
    - Collision mode: Water flows automatically when container approaches
  - Flexible trigger system for easy customization
- **Documentation cleanup**:
  - Removed all "deleted files" records (current state only)
  - Removed implementation plan section (use TASKS.md instead)
  - Simplified history records

**Next**: Phase 1 - Environment construction and VR interaction foundation

## File Structure

```
docs/                      # 📚 Project documentation
  ├── DEVELOPMENT_GUIDE.md # Integrated guide (spec + design + plan)
  └── TASKS.md             # Development task checklist

.claude/                   # Claude Code project info
  └── project-info.md      # This file

.vscode/                   # VSCode settings
  ├── settings.json        # Editor & file filter settings
  └── extensions.json      # Recommended extensions

.git/                      # Local Git repository
.gitignore                 # Git exclusions
.vsconfig                  # Visual Studio for Unity config

CLAUDE.md                  # Claude Code main documentation
README.md                  # Project overview (entry point)
```

## Notes
- Project uses Built-in Render Pipeline (not URP/HDRP)
- Target platform: Android (Meta Quest)
- Unity version: 6000.2.7f2
