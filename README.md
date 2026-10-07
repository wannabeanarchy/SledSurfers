# Sled Surfers Gameplay Test

A Unity gameplay prototype inspired by Sled Surfers, using the supplied Miraculous Ladybug assets.

## Run the project

**Unity version:** 2022.3.62f3 LTS.

1. Open the repository root in Unity Hub and let Unity import the project.
2. Open `Assets/Game/Scenes/LobbyScene.unity` and press Play.
3. Press **TAP TO PLAY** in the lobby to enter the gameplay scene.

**Android APK (ARMv7):** [Download the test build](build-android/sled-surfers-test.apk).

**Gameplay video:** [Download the recording](video-gamplay/Screen%20Recording%202026-10-07%20at%2013.03.03.mov).

The prototype has been checked in Unity Editor and in the Android build.

## Controls

- **Launch:** hold the left mouse button or touch the screen, drag down to set power, and release. Dragging sideways aims the launch in the opposite direction, like a slingshot.
- **Steer:** press again after launch and drag horizontally. Releasing stops turning and keeps the current heading.
- **RETRY:** start a new run with the saved upgrades.
- **CONTINUE:** return to the lobby to purchase upgrades.
- **RESET:** clear local progress from the lobby.

## Gameplay and progression

Slide downhill, steer around obstacles and collect coins. Icebergs end the run; puddles increase resistance and slow the player. A run also ends when the player loses momentum on the ground or reaches the end of the track.

Distance and collected coins determine the reward, which is credited once at the end of the run. The HUD displays distance, speed, track progress and the best-distance marker.

The lobby offers three persistent upgrades:

- **Slingshot:** increases launch speed.
- **Skate:** improves ground/air turning and increases the maximum steering angle.
- **Income:** increases rewards for distance and collected coins.

Balance, upgrade levels and best distance persist between runs and application restarts. Retry resets the run and restores pickups while retaining progression.

## Implementation decisions

- **Scene composition:** separate lobby and gameplay scenes have composition roots that wire serialized references explicitly. `GameplayRunCoordinator` coordinates input, run transitions, metrics, rewards and results.
- **Run state:** `LaunchSession` separates ready, pulling, running and stopped phases, keeping launch and steering input tied to the current phase.
- **Physics:** `PlayerMotor` uses Rigidbody movement and a dedicated skateboard contact collider with ground probes. `PlayerFrictionModule` handles resistance and additional slope acceleration. Both movement and force application run in `FixedUpdate`.
- **Configuration:** serialized fields and `ProgressionConfig` keep gameplay tuning and upgrade formulas separate from run coordination.
- **Persistence:** `PlayerProgressStorage` wraps JSON storage in `PlayerPrefs`, isolating saving and loading from gameplay logic.
- **Presentation and UI:** camera, animation and visual effects are separate from movement logic. Typed UI views and controllers are created through `UIFactory` and managed by `UIManager`.
- **Assets:** supplied character and environment assets are reused, with materials adapted to Unity's Built-in rendering pipeline.

Project code is under `Assets/Game/Scripts/`, grouped into `Composition`, `Gameplay`, `Input`, `Progression`, `Persistence`, `Presentation` and `UI`. Scenes, configs and prefabs are under `Assets/Game/Scenes/`, `Configs/` and `Prefabs/`.

## With more time

- Add audio for launch, sliding, pickups, impacts and results, with a mute control.
- Add optional haptic feedback for launch, pickups and crashes.
- Refine steering, slope transitions and upgrade balance through repeatable playtests.
- Improve pickup/impact feedback, camera response and material consistency.
- Add focused automated tests for run transitions, rewards, upgrades and save loading.
- Profile frame time and allocations on target devices, and check more screen sizes and background/resume scenarios.
