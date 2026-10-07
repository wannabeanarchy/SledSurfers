# Design Notes

This document describes the implemented prototype. Setup, controls and potential improvements are documented in [README.md](../README.md).

## Scene composition and ownership

`LobbyCompositionRoot` loads progress, creates the lobby UI, validates purchases, saves changes and opens the gameplay scene. `GameplayCompositionRoot` owns serialized gameplay references, validates the scene setup, loads progress, clamps upgrade levels to the configured caps and assembles runtime dependencies. Dependencies are passed explicitly without a DI framework.

`GameplayRunCoordinator` owns pointer input, launch/steering flow, run metrics, rewards and the result transition. The gameplay root forwards Unity lifecycle callbacks and retains scene loading. Physics stays in `PlayerMotor.FixedUpdate`; presentation components own camera and visual behavior.

## Launch, steering and run phases

`LaunchSession` has four phases: `Ready`, `Pulling`, `Running` and `Stopped`. Dragging down increases launch power; horizontal dragging aims in the opposite direction. During the pull, the Rigidbody is kinematic and the player is positioned relative to the saved launch origin. Canceling or releasing with negligible power restores the starting pose and returns to `Ready`.

Release switches the Rigidbody to dynamic movement and applies the calculated velocity. Steering requires a new press after launch. `DragSteering` converts horizontal displacement into input; the motor changes the heading at separate ground and air turn rates, clamped to the configured steering angle. Releasing the gesture keeps the current heading.

`PointerInput` tracks the active pointer and ignores presses over UI. Losing focus or pausing the application cancels the gesture; it does not itself complete or restart the run. A visible result popup blocks gameplay input.

## Player movement and surface resistance

`PlayerMotor` owns Rigidbody lifecycle, track contact detection, heading limits and airborne movement. `PlayerFrictionModule` owns ground resistance, additional slope acceleration and slowdown zones. The motor calls the friction module in the same `FixedUpdate` step so force application order stays explicit; Rigidbody gravity remains enabled.

The skateboard's dedicated physical collider owns track contact. The tall character capsule ignores track colliders but still hits obstacles. Three short probes under the front, center and rear of the deck provide a small ground snap and a surface normal when the collider briefly loses contact. Airborne movement begins when neither contact nor a nearby walkable surface is available; extra gravity and an upward-speed limit control the trajectory. The visible skateboard asset's own collider remains disabled because its imported scale is unsuitable for gameplay physics.

New surface contacts project the incoming relative velocity along the slope once. Continuing contacts use the current tangential velocity rather than reapplying collision-relative velocity, allowing resistance to slow the player. Steering redirects the tangential velocity while preserving its magnitude. The visual character follows heading and ground alignment without animation root motion.

Terrain heights can be edited with Unity's Terrain tools. The gameplay terrain and its collider share `SnowTrack.asset`; track colliders are assigned explicitly.

## Obstacles and collectibles

`CrashObstacle` stops the player and can spawn an impact effect. `SlowdownObstacle` registers resistance only for the skateboard contact collider. Zones are keyed by collider to avoid duplicate registration; the strongest overlapping resistance is used. Disabling a zone unregisters it, and stopping or launching clears the motor's zone state.

`CoinPickup` accepts a running player once, raises `Collected` and disables its object. It has no economy or storage responsibility. The coordinator subscribes to the scene's explicit coin list and increments the run's pickup count. The current scene registers all nine coins in track order, with nine matching HUD markers. Adding or removing track coins requires updating both serialized lists.

HUD marker positions are calculated from coin world positions and normalized against the track distance from the actual launch position. `CoinRewardBurst` is decorative: its spawned coins have pickup behavior disabled and do not contribute to rewards.

## Run completion and retry

`RunMetrics` tracks the furthest forward distance from launch, speed in meters per second and pickup count. The target distance comes from the active track colliders unless overridden in the scene. Launch updates the distance origin and coin marker positions to account for the pulled-back starting position.

The motor stops on a crash or low grounded speed; the coordinator also detects reaching the track end. Before presenting results, it changes the session to `Stopped`, freezes the player and hides the HUD. Reaching the track end plays the victory animation; other endings hold the current pose. The flag rises before the result popup appears.

`RunRewardCalculator` floors the distance reward to whole coins and adds the pickup count multiplied by the configured per-pickup reward, both using the current Income level. Balance and best distance are saved once before the result buttons become available.

Retry reloads `GameScene`, recreating input, metrics, pickups and presentation while loading the saved upgrades. Continue opens `LobbyScene`. Scene-transition guards prevent repeated loading requests; disposal removes event subscriptions and releases the UI.

## Progression and persistence

`ProgressionConfig` groups upgrade effects, price growth and level caps. Lobby purchases check balance and caps before modifying progress; unavailable upgrades are reflected in the card state. Current tuning is defined in [ProgressionConfig.asset](../Assets/Game/Configs/ProgressionConfig.asset).

`PlayerProgressStorage` is the only boundary using `PlayerPrefs`. One JSON record stores coin balance, best distance and the three upgrade levels. Loading handles missing or malformed data and sanitizes invalid values; the composition roots clamp levels to the current configuration. The lobby reset clears this record and restores the configured initial balance.

## UI and presentation

`UIConfig` maps view types to prefabs. `UIFactory` creates typed views and controllers; `UIViewAttribute` on each controller declares its view type and UI layer and is checked during registration. `UIManager` creates each controller lazily and reuses it within the scene. It keeps at most one window and one popup visible; changing the window hides the current popup. Widgets can coexist. `UISafeArea` adjusts the UI to the device's safe area.

`GameplayHud` shows launch power before the run, distance/speed/track progress while running and hides on completion. `CameraFollow`, `PlayerAnimation`, `SlingshotRopeVisual`, `SteeringJoystickVisual`, `FinishFlagVisual` and `CoinRewardBurst` own presentation.
