# Sled Surfers Gameplay Test

A Unity gameplay prototype for the supplied technical assignment, built with the supplied Miraculous Ladybug assets.

## Open and play

- Unity version: **2022.3.62f3 LTS**.
- Open the repository root in Unity Hub.
- Open `Assets/Game/Scenes/LobbyScene.unity` and press Play.
- In the lobby, press **TAP TO PLAY** to start a run. The `SampleScene` is an animation preview.

## Controls

- **Launch:** hold the left mouse button or touch the screen, drag down to pull the slingshot, and release to launch. Pull distance controls launch power; horizontal offset aims the launch.
- **Steer:** while moving, drag horizontally. The on-screen joystick appears while steering. Releasing stops input and keeps the current heading.
- The same pointer gesture is used for launch setup before the run and steering after launch.

## Run and progression

The run tracks forward distance, speed in meters per second, track progress, collected coins, and best distance. A run ends on a crash, when the player loses momentum, or upon reaching the end of the active track. On finish, movement is frozen, the flag rises, coins burst around the player, and the result popup offers **RETRY** or **CONTINUE**.

The lobby has three upgrades, configured in `Assets/Game/Configs/ProgressionConfig.asset`:

- **Slingshot:** maximum launch speed starts at 40 m/s and increases by 8 m/s per level.
- **Skate:** ground and air turn rates start at 25°/s and 4°/s, then grow by 10% per level. Maximum steering angle starts at 15° and grows by 2° per level (up to level 10).
- **Income:** increases both distance and collected-coin rewards by 20% per level. Base distance reward is 1,000 coins per kilometer (one coin per meter); each collected coin is worth 300 coins before the income multiplier.

Upgrade prices, growth and level caps are also configured there. Coin balance, upgrade levels and best distance are stored locally through `PlayerProgressStorage` in `PlayerPrefs`. The lobby reset button clears this saved progress.

## Project layout

- `Assets/Game/Scenes/`: `LobbyScene` and `GameScene`.
- `Assets/Game/Scripts/Composition/`: scene setup and run-flow coordination.
- `Assets/Game/Scripts/Gameplay/`: player movement, launch/run metrics, collectibles and obstacles.
- `Assets/Game/Scripts/Input/`: pointer input and drag steering.
- `Assets/Game/Scripts/Progression/` and `Persistence/`: upgrade formulas/configuration and local save boundary.
- `Assets/Game/Scripts/Presentation/` and `UI/`: camera, animation, rope/flag/joystick visuals, HUD, lobby and result UI.
- `Assets/Game/Configs/`: ScriptableObject configuration assets.
- `Assets/Game/Prefabs/`: project prefabs grouped by gameplay role.
- `Assets/Game/Art/Environment/Terrain/SnowTrack.asset`: shared terrain data used by the gameplay terrain and its collider; the hill profile has been gently smoothed.
- `Assets/Ladybug/`: supplied source assets.
- `Packages/` and `ProjectSettings/`: Unity package and project configuration.

## Implementation notes

`GameplayCompositionRoot` validates scene references and wires dependencies explicitly; there is no dependency-injection framework. `GameplayRunCoordinator` owns run input and transitions. `PlayerMotor` owns Rigidbody movement and skateboard-based track contact; new surface contacts are projected along the slope once, while continuing contacts retain the current velocity so ground resistance can slow the player. `PlayerFrictionModule` applies ground resistance, slope acceleration and slowdown zones. `LaunchSession` handles pull-and-launch state. The visible skateboard mesh collider is disabled; a dedicated collider on the board handles physical contact, while probes help maintain ground contact over uneven sections.

The UI is built from typed views and controllers, registered through `UIFactory` and managed by `UIManager`. `PlayerProgressStorage` isolates persistence from gameplay systems. Terrain heights can be edited with Unity's Terrain tools; the scene's terrain and TerrainCollider use the same `SnowTrack` data.

## Asset compatibility

Some shaders and source texture references from the supplied asset pack are unavailable. Missing custom shaders are replaced with Built-in shaders where needed; shader-specific effects such as world bending and custom reflections are not reproduced.

## With more time

Add regression tests for progression, reward calculations and run transitions, then do more repeatable physics and orientation checks on target devices. A playable build is optional; the local Android test APK is not tracked in the repository.
