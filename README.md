# Sled Surfers Gameplay Test

Unity prototype for the supplied gameplay assignment, using Miraculous Ladybug assets.

## Unity Version

Unity 2022.3.62f3 LTS.

## Setup

1. Clone the repository and open its root directory in Unity Hub.
2. Open `Assets/Game/Scenes/LobbyScene.unity`, press Play, then click or tap `TAP TO PLAY`.
3. Open `Assets/Game/Scenes/SampleScene.unity` to preview character animation.

## Character Animation

`Assets/Game/Animations/CatNoir.controller` is assigned to the gameplay character. It uses the supplied Humanoid clips for Idle, Jump, Slide, looping Jump_Fall while pulling, Slide_Run_to_Slide when launching and Victory when the player reaches the end of the track. The lobby character uses a separate controller with the looping Ilde_TurningLeftRight clip.

In Play Mode, select Cat Noir and open the Animator window to change its parameters:

| IsRunning | IsGrounded | Animation |
| --- | --- | --- |
| false | Either | Idle |
| true | false | Jump |
| true | true | Slide |

Jump_Fall loops while the pull gesture is held. Victory plays only for a successful finish and holds its final pose. Landing is controlled by `IsGrounded`, rather than animation duration. Root motion is disabled; character movement is controlled by the gameplay motor.

## Project Structure

- `Assets/Game/Scenes/`: gameplay layout (`GameScene`) and animation preview (`SampleScene`).
- `Assets/Game/Prefabs/Player/Player.prefab`: player root with a nested Cat Noir visual.
- `Assets/Game/Art/Characters/CatNoir/`: selected character model, clips, material and texture.
- `Assets/Game/Animations/`: character animation controller.
- `Assets/Ladybug/`: supplied Miraculous content.
- Other imported folders under `Assets/`: supporting art and effects.
- `Packages/` and `ProjectSettings/`: Unity dependencies and project configuration.

## Controls and Movement

Open `Assets/Game/Scenes/LobbyScene.unity`, press Play, then click or tap `TAP TO PLAY`. Hold the left mouse button or touch the screen, then drag down to pull the slingshot back; vertical pull distance sets launch power up to 100% and moves the player backward. Drag left or right independently to aim the launch direction. Releasing launches forward with a small sideways component opposite the aim. Canceling returns the player to the start. A new press during the run begins steering; drag horizontally to turn the player's heading, and the player and skateboard follow that direction while preserving horizontal speed. A joystick appears under the steering touch and hides on release. Releasing the drag stops turning but keeps the current course; steer the other way to turn back. At level 0, steering starts deliberately slow (25°/s on ground, 4°/s in air). The skate upgrade compounds turn rate by 20% on ground and 15% in air per level, and increases the maximum steering angle by 4° per level. Canceled gestures do not launch.

`GameplayCompositionRoot` wires input and movement manually. Aim angle, pull distance, player lateral offset, joystick radius and track limits are configured on this component. `ProgressionConfig` supplies maximum launch speed and skate handling from saved upgrade levels. `PlayerMotor` owns Rigidbody movement, track contact, steering and air control; `PlayerFrictionModule` applies ground resistance, slope acceleration and slowdown zones. The camera follows the player while preserving its initial scene framing, rotation and FOV. Set the camera Transform before Play to adjust framing. `Maximum Backward Distance` limits camera retreat from its starting position (2 units by default).

The road BoxColliders form the track floor. A white Terrain overlays the road with snow rises; terrain below the road is hidden by the road surface. Both road and snow colliders are registered as movement surfaces. Terrain heights can be edited in the Unity Terrain tools. A dedicated thin collider under the skateboard handles physical track contact; the tall character capsule ignores the track but still collides with obstacles. Two short probes under the front and rear of the board provide a small ground snap and surface normal if physical contact briefly drops. Contacts remove velocity into or away from the surface while preserving motion along the slope. Slope Gravity Multiplier on PlayerFrictionModule amplifies gravity along slopes; the visual follows the ground incline. Ground resistance reduces momentum until the player stops in Idle.

## Current Status

Player movement, pointer steering, camera following, drag-to-launch, obstacles, collectible coins, the result-to-lobby flow and three purchasable upgrades are implemented. Coin balance, best distance and upgrade levels persist locally between launches. Coin pickup effects and a playable build remain to be implemented.

## Asset Compatibility

Missing custom shaders in the supplied pack are replaced with Built-in shaders for surfaces, vegetation, UI and particles. TextMeshPro shaders are included from the installed package. Original shader-specific effects, including world bending and custom reflections, are not reproduced. Some source texture references are absent from the supplied pack.

## UI Module

`Assets/Game/Scripts/UI/` separates views, typed controllers, prefab lookup and visibility management. Create a UI Catalog asset and assign one prefab per concrete `UIView` type. Mark controllers with `[UIView(typeof(MyView), ViewType.Widget)]` (or `Window` / `Popup`). Register explicit controller constructors through `UIFactory.Register`, supplying dependencies manually. The factory receives separate Canvas layer roots for windows, widgets and popups; place the popup layer above the others. Keep prefab roots inactive to avoid premature `OnEnable`.

`UIManager.Show` creates each registered controller and view once, then updates typed arguments on subsequent calls. Controllers subscribe in `OnShow` and unsubscribe in `OnHide`; repeated Show does not repeat subscriptions. Windows are exclusive, popups are exclusive and close when their window closes, and widgets coexist. Dispose the manager when its scene owner is destroyed. `BlocksGameplayInput` reports modal visibility; the gameplay owner must cancel active gestures and suspend pointer input while it is true. GameScene wires the UI catalog and Canvas layers through `GameplayCompositionRoot`. At the end of a run, a result popup shows earned coins, and the finish flag rises beside the player at the stopping point. `RETRY` reloads the gameplay scene with saved upgrades; `CONTINUE` returns to the lobby. `PlayerProgressStorage` keeps coin balance and best run distance as JSON in `PlayerPrefs`, loading both in the gameplay scene and saving after a run. The `BEST` marker on the side progress bar shows the saved distance record. The same storage boundary also persists slingshot, skate and income upgrade levels. Lobby cards show current effects and next cost, and are disabled when the balance is insufficient or the upgrade has reached its configured maximum level.

HUD prefabs in `Assets/Game/Prefabs/UI/` display launch power before launching, then forward distance (meters), Rigidbody speed (meters per second) and track progress. Distance starts at the release position and retains the furthest forward position. `HUD Target Distance` on `GameplayCompositionRoot` sets a distance goal; zero uses the end of active road colliders. A run ends on a crash, when momentum is lost, or when the distance goal is reached. `ProgressionConfig` sets the default distance reward to 1,000 coins per kilometer (one coin per full meter); each collected coin pickup adds 100 coins. Income levels apply a configurable multiplier to that rate. HUD graphics use plain Images without sprites and do not intercept input. The widget layer respects the screen safe area.

## Obstacles

`Assets/Game/Prefabs/Obstacles/IcebergObstacle_01.prefab` uses the supplied iceberg visual and a solid BoxCollider. Player collisions stop and freeze the attempt, then show the run result. `PuddleObstacle.prefab` reuses one mesh from `Park_BushObstacles_Shadows` with a shallow trigger. `Additional Resistance` controls continuous ground deceleration inside the puddle (30 m/s² by default). Exiting or disabling the puddle removes its resistance; airborne movement is unaffected. Overlapping puddles use the strongest resistance. Place each prefab on the road or snow surface.

Four scene coins use trigger colliders and disappear when the player reaches them. Each collected coin adds 100 coins to the run reward. Their icons on the progress widget use the same world-distance positions as the placed coins.
