# Sunset Stroll (Unity 2D Endless Runner)

Quick-start project scaffolding for a side-view endless runner with parallax, obstacle spawner, player controller (jump/slide), difficulty ramp, score UI, and placeholder art/animations.

## Requirements
- Unity 2021.3+ (or any recent 2020/2021/2022 LTS)

## Folder
- Open the `SunsetStroll` folder in Unity.

## Generate a Playable Demo
- In Unity, open any scene or a blank project.
- Menu: `Sunset Stroll` → `Create Demo Scene`.
  - This creates a scene with:
    - `GameManager` for speed/spawn difficulty ramp and score
    - Parallax background layers (sky, buildings, lamps)
    - Infinite scrolling ground
    - `Jogger` player with placeholder controller and animations
    - `ObstacleSpawner` with placeholder obstacles
    - Score UI (top-left)

Press Play to run.

Controls
- Jump: Space / W / Up Arrow
- Slide: S / Down Arrow

## Scripts Overview
- `Assets/Scripts/GameManager.cs` — difficulty ramp, spawn interval, score
- `Assets/Scripts/Player/JoggerController.cs` — input, jump/slide, collisions
- `Assets/Scripts/World/ParallaxLayer.cs` — parallax scrolling + looping
- `Assets/Scripts/World/GroundTiler.cs` — infinite ground recycling
- `Assets/Scripts/World/ScrollingMover.cs` — leftward movement helper
- `Assets/Scripts/World/Obstacle.cs` — marker for collisions
- `Assets/Scripts/Spawning/ObstacleSpawner.cs` — timed random spawns
- `Assets/Scripts/Spawning/ObstaclePlaceholderFactory.cs` — quick placeholder prefabs
- `Assets/Scripts/World/FollowCamera.cs` — optional camera follow
- `Assets/Scripts/Editor/BootstrapSceneCreator.cs` — builds a demo scene and placeholder animator

## Notes
- Placeholder obstacles are colored boxes sized to approximate real assets and move with the world speed.
- Replace placeholders with your own sprites/animations; keep colliders similar sizes.
- To create reusable placeholder prefabs in your project, Menu: `Sunset Stroll` → `Create Placeholder Obstacles`.