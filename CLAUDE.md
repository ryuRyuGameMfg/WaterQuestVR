# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Project Name**: VR Water Resource Serious Game (MVP)

This is a Unity 6 (6000.2.7f2) **educational VR game** for Meta Quest that teaches water resource management through immersive experience.

**Purpose**: Players learn about water scarcity and resource management by drawing water from a well and making decisions about its usage (drinking, farming, disposal).

**Key Features**:
- Hand-tracking interaction with buckets and cups
- Parameter management system (Water Volume, Water Quality, Stamina)
- Scoring system (Hygiene & Efficiency)
- Simple fluid simulation using Obi Fluid
- Built-in Render Pipeline for Quest optimization

**Current Status**: Specification phase completed (2025-10-25). Ready for Phase 1 development.

## Key Technologies

- **Unity Version**: 6000.2.7f2
- **VR Framework**: Meta XR SDK v78.0.0 (**ネイティブAPI使用、OpenXR非推奨**)
  - OVRCameraRig（VRカメラ）
  - OVRHand（ハンドトラッキング）
  - OVRInput（コントローラ入力）
  - OVRManager（XR設定管理）
- **Render Pipeline**: Built-in（Quest最適化）
- **Obi Fluid**: Advanced fluid/liquid physics simulation
- **Hand Tracking**: OVRHand-based interaction system with pinch gestures
- **Physics Plugins**: Obi physics engine for cloth/rope/fluid simulation
- **Burst Compiler**: Enabled for performance optimization

**⚠️ 重要**: このプロジェクトは **Meta XR SDK のネイティブAPI** を使用します。OpenXR Plugin は使用しない、または優先度を下げてください。

## Project Documentation

**Essential Reading**:
- `docs/DEVELOPMENT_GUIDE.md` - **Integrated development guide** (specifications, design, implementation plan)
- `docs/TASKS.md` - Detailed development tasks checklist

**Quick Reference**:
- Main Scene: `Assets/Scenes/MainGameScene.unity` (to be created)
- Scripts: `Assets/Scripts/` (organized by system)
- Existing Assets: Viking Village (environment), Obi Fluid (water physics)

## Build Commands

### Android/Quest Build
```bash
# The project builds to Android APK for Meta Quest devices
# Build output is in: /Builds/demo.apk
# Ensure Android Build Support is installed in Unity Hub
```

### Opening the Project
```bash
# Open Unity Hub and add this project folder
# Unity 6000.2.7f2 is required
# Ensure Meta XR SDK package is properly imported
```

## Development Workflow

### Running in Editor
- Main scene: `Assets/Scenes/HandScene.unity`
- Test scenes available: `SampleSeane2.unity`, `testEmmiter.unity`
- Use Meta Quest Link or Unity XR Simulator for testing VR functionality

### Testing
- Test hand tracking interactions in `HandScene.unity`
- Verify pinch gesture detection for object manipulation
- Test liquid physics with Obi/Zibra systems

## Code Architecture

### Custom Scripts Location

**Existing Scripts** (`Assets/Scripts/`):
- **Carriable.cs**: VR object grabbing with pinch gestures (will be extended for water containers)
- **TouchHandler.cs**: Collision detection utility
- **Dish.cs**: Example parameter management (reference for water quality system)

**To Be Implemented** (see DESIGN.md for details):
```
Assets/Scripts/
├── GameManagement/        # Core game systems
│   ├── GameManager.cs
│   ├── ParameterManager.cs
│   ├── ScoreCalculator.cs
│   └── StatisticsTracker.cs
│
├── WaterSystem/          # Water containers
│   ├── WaterProperties.cs
│   ├── WaterContainer.cs
│   ├── Bucket.cs
│   └── Cup.cs
│
├── Interactions/         # Interactive objects
│   ├── IWaterInteractable.cs
│   ├── Well.cs
│   ├── Field.cs
│   ├── DrinkingPoint.cs
│   └── WaterDisposalZone.cs
│
└── UI/                   # HUD and scoring
    ├── HUDManager.cs
    ├── ParameterDisplay.cs
    └── ScoreDisplay.cs
```

### VR Interaction System
The project uses a hand-tracking based interaction model:
1. Objects with `Carriable` component detect hand proximity via trigger colliders
2. Pinch gesture (index finger) picks up objects
3. Objects follow hand position/rotation while carried
4. Release pinch to drop objects
5. Visual feedback through material swaps (original → touch → carried)

### Third-Party Asset Structure
- `Assets/Plugins/Zibra/`: Zibra Liquid/Smoke simulation system
- `Assets/Obi/`: Obi physics engine for fluids, cloth, ropes
- `Assets/MetaXR/`: Meta XR SDK integration
- `Assets/Oculus/`: Oculus platform settings
- `Assets/ALP_Assets/`: Oak tree asset pack
- `Assets/VillagePack/`: Environmental assets
- `Assets/Mugs, Bowls and Plates/`: Dishware 3D models

### Key Directories
- `Assets/Resources/`: Runtime-loadable assets, OVR configuration
- `Assets/Materials/`: Custom materials for objects
- `Assets/Scenes/`: Scene files
- `Assets/StreamingAssets/`: Platform-specific streaming data
- `ProjectSettings/`: Unity project configuration
- `Builds/`: Output directory for APK builds

## Game Parameters & Mechanics

### Core Parameters
| Parameter | Range | Description |
|:--|:--:|:--|
| Water Volume | 0-100 | Current water amount in containers |
| Water Quality | 0-100 | Water cleanliness (80+ = safe) |
| Stamina | 0-100 | Player energy (actions consume, drinking restores) |

### Container Capacities
- **Bucket**: 80 (for farming, drawing water)
- **Cup**: 10 (for drinking)

### Action Effects
| Action | Water Δ | Quality Δ | Stamina Δ | Tool |
|:--|:--:|:--:|:--:|:--|
| Draw from Well | +80 | +10 | -10 | Bucket/Cup |
| Water Crops | -50 | -5 | -15 | Bucket |
| Drink | -10 | ±0 | +10 (safe) / -10 (unsafe) | Cup |
| Dispose | -20 | -10 | -2 | Any |

### Scoring
- **Hygiene**: `1 - (unsafe_drinking / total_usage)` × 100
- **Efficiency**: `(living_usage / total_drawn)` × 100

See `docs/DEVELOPMENT_GUIDE.md` for complete parameter details.

## Important Notes

### VR Development
- This project is configured for **Meta Quest** standalone VR (Android platform)
- **使用フレームワーク**: Meta XR SDK ネイティブAPI（OVRHand, OVRInput, OVRCameraRig）
- **OpenXR は使用しない**: Meta XR Plugin（Oculus）を優先
- Hand tracking must be enabled in OculusProjectConfig
- XR Plugin Management で "Oculus" を有効化、"OpenXR" は無効化推奨
- Test on actual Quest hardware when possible; simulator has limitations
- Hand tracking requires proper lighting conditions on device

**XR設定チェックリスト**:
1. `Edit > Project Settings > XR Plug-in Management`
2. Android タブで **Oculus** にチェック
3. OpenXR のチェックを外す（または優先度を下げる）
4. `Assets/Oculus/OculusProjectConfig.asset` で Hand Tracking を有効化

### Performance Considerations
- Burst compiler is enabled for performance-critical code
- Obi and Zibra physics systems are computationally expensive
- Target platform is mobile VR (Quest), so optimization is critical
- Monitor performance using `Assets/Resources/PerformanceTestRunInfo.json`

### Physics System Integration
- Multiple physics systems in use: Unity Physics, Obi, and Zibra
- Obi is primarily used for fluid simulation
- Ensure colliders are properly configured for VR hand interactions
- Terrain physics enabled for outdoor scenes

### Build Configuration
- Platform: Android
- Graphics API: Vulkan (typical for Quest)
- Build path: `Builds/demo.apk`
- Burst debug information: `Builds/WaterProject2_BurstDebugInformation_DoNotShip/`

## Package Dependencies

Key Unity packages:
- `com.meta.xr.sdk.all` (78.0.0) - Meta XR SDK
- `com.unity.xr.oculus` (4.5.2) - Oculus XR plugin
- `com.unity.burst` (1.8.25) - Performance optimization
- `com.unity.shadergraph` (17.2.0) - Visual shader authoring
- `com.unity.timeline` (1.8.9) - Cinematic sequencing

## Project Cleanup

### Unnecessary Files (Safe to Delete)

The following directories are auto-generated and can be safely deleted to reduce project size:
- `Library/` (14GB) - Unity cache, regenerated on startup
- `Temp/` - Temporary files
- `Logs/` - Build and shader compilation logs
- `obj/` - Build intermediate files
- `.vs/` - Visual Studio cache
- `.utmp/` - Temporary files
- `*.blob` files - Crash dumps
- `.DS_Store` - macOS metadata

### Recommended .gitignore

If using Git version control, add these to `.gitignore`:
```
/Library/
/Temp/
/Logs/
/obj/
/Builds/
/.vs/
/.utmp/
/UserSettings/
*.blob
.DS_Store
*.apk
```

### Optional Cleanup (Development Complete)

Remove sample/demo content if not needed:
- `Assets/Plugins/Zibra/*/Samples/` (148MB total)
- `Assets/Obi/Samples/` (17MB)
- `Assets/Plugins/Zibra/Common/Documentation/` (7MB)
- `Assets/AssetsStore/` (41MB) - if unused
- `Assets/Standard Assets/` (4MB) - legacy assets

## Common Build Issues

### Render Pipeline Errors

**Problem**: Build fails with shader errors like "Couldn't open include file 'Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl'"

**Cause**: This project uses **Built-in Render Pipeline**, but some assets (Obi, Zibra) include URP shaders that fail to compile without URP installed.

**Solution**:
1. Disable URP shaders in Obi:
   ```bash
   # Rename URP folder to disable it
   mv "Assets/Obi/Resources/ObiMaterials/URP" "Assets/Obi/Resources/ObiMaterials/URP.disabled"
   mv "Assets/Obi/Resources/ObiMaterials/URP.meta" "Assets/Obi/Resources/ObiMaterials/URP.disabled.meta"
   ```

2. Verify Graphics Settings:
   - Open `Edit > Project Settings > Graphics`
   - Ensure `Scriptable Render Pipeline Settings` is set to `None` (Built-in RP)

3. Check Obi settings:
   - Open `Assets/ObiEditorSettings.asset`
   - Verify Render Pipeline is set to `Built-in`

### Vulkan Shader Errors

**Problem**: ALP Wind shader fails with "BLENDINDICES" error on Vulkan

**Solution** (if needed):
- Temporarily switch Graphics API to OpenGL ES3:
  - `Edit > Project Settings > Player > Android > Other Settings`
  - Uncheck `Auto Graphics API`
  - Remove `Vulkan` and add `OpenGL ES3`
- Or update/replace the ALP asset pack

### Obsolete API Errors (CS0619)

**Problem**: Build fails with "RenderTargetHandle is obsolete: Deprecated in favor of RTHandle"

**Cause**: Some assets (like Boat Attack Water System) use old URP APIs incompatible with Unity 6.

**Solution**: The incompatible Boat Attack Water System has been removed from `Assets/Viking Village/`. The rest of Viking Village assets remain intact and functional.
