# Sled Surfers Gameplay Test

Unity prototype for the supplied gameplay assignment, using Miraculous Ladybug assets.

## Unity Version

Unity 2022.3.62f3 LTS.

## Setup

1. Clone the repository and open its root directory in Unity Hub.
2. Open `Assets/Game/Scenes/LobbyScene.unity`, press Play, then click or tap `TAP TO PLAY`.
3. Open `Assets/Game/Scenes/SampleScene.unity` to preview character animation.

## Character Animation

`Assets/Game/Animations/CatNoir.controller` is assigned to the Cat Noir model in the sample scene. It uses the supplied Humanoid clips for Idle, Jump, Slide, FootBreak (pulling) and Slide_Run_to_Slide (launch transition).

In Play Mode, select Cat Noir and open the Animator window to change its parameters:

| IsRunning | IsGrounded | Animation |
| --- | --- | --- |
| false | Either | Idle |
| true | false | Jump |
| true | true | Slide |

FootBreak loops while the pull gesture is held. Jump and Slide hold their final pose after their non-looping clips finish. Landing is controlled by `IsGrounded`, rather than animation duration. Root motion is disabled; character movement will be controlled separately.

## Project Structure

- `Assets/Game/Scenes/`: gameplay layout (`GameScene`) and animation preview (`SampleScene`).
- `Assets/Game/Prefabs/Player/Player.prefab`: player root with a nested Cat Noir visual.
- `Assets/Game/Art/Characters/CatNoir/`: selected character model, clips, material and texture.
- `Assets/Game/Animations/`: character animation controller.
- `Assets/Ladybug/`: supplied Miraculous content.
- Other imported folders under `Assets/`: supporting art and effects.
- `Packages/` and `ProjectSettings/`: Unity dependencies and project configuration.

## Controls and Movement

Open `Assets/Game/Scenes/LobbyScene.unity`, press Play, then click or tap `TAP TO PLAY`. Hold the left mouse button or touch the screen and pull downward to charge the launch. Pulling moves the player backward proportionally to launch power. Canceling returns the player to the start; release launches from the pulled position. Horizontal pull aims slightly in the opposite direction. Release to launch; a new press begins lateral steering. Drag horizontally to choose a lateral position, then release to stop steering. Airborne steering uses lower acceleration. Steering redirects horizontal momentum without adding speed. The gesture remains active until release, including at zero tension. Moving toward or away from the initial press changes tension; launch speed uses tension at release. Canceled gestures do not launch.

`GameplayCompositionRoot` wires input and movement manually. Maximum launch speed, aim angle, pull distance, player pullback distance and lateral limits are configured on this component. `PlayerMotor` exposes steering and resistance tuning; the camera follows the player while preserving its initial scene framing, rotation and FOV. Set the camera Transform before Play to adjust framing. `Maximum Backward Distance` limits camera retreat from its starting position (2 units by default).

The road BoxColliders form the track floor. A white Terrain overlays the road with snow rises; terrain below the road is hidden by the road surface. Both road and snow colliders are registered as movement surfaces. Terrain heights can be edited in the Unity Terrain tools. Ground detection uses contact normals and velocity relative to the surface. Road and snow contacts remove outward collision velocity while preserving motion along the slope; genuine takeoff remains possible when contact ends. Slope Gravity Multiplier on PlayerMotor amplifies gravity along slopes; the visual follows the ground incline. Ground resistance reduces momentum until the player stops in Idle.

## Current Status

Player movement, pointer steering, camera following, drag-to-launch, obstacles, collectible coins and the result-to-lobby flow are implemented. The coin balance persists locally between launches. Upgrade purchasing and persistence, coin pickup effects, and a playable build remain to be implemented.

## Asset Compatibility

Missing custom shaders in the supplied pack are replaced with Built-in shaders for surfaces, vegetation, UI and particles. TextMeshPro shaders are included from the installed package. Original shader-specific effects, including world bending and custom reflections, are not reproduced. Some source texture references are absent from the supplied pack.

## UI Module

`Assets/Game/Scripts/UI/` separates views, typed controllers, prefab lookup and visibility management. Create a UI Catalog asset and assign one prefab per concrete `UIView` type. Mark controllers with `[UIView(typeof(MyView), ViewType.Widget)]` (or `Window` / `Popup`). Register explicit controller constructors through `UIFactory.Register`, supplying dependencies manually. The factory receives separate Canvas layer roots for windows, widgets and popups; place the popup layer above the others. Keep prefab roots inactive to avoid premature `OnEnable`.

`UIManager.Show` creates each registered controller and view once, then updates typed arguments on subsequent calls. Controllers subscribe in `OnShow` and unsubscribe in `OnHide`; repeated Show does not repeat subscriptions. Windows are exclusive, popups are exclusive and close when their window closes, and widgets coexist. Dispose the manager when its scene owner is destroyed. `BlocksGameplayInput` reports modal visibility; the gameplay owner must cancel active gestures and suspend pointer input while it is true. GameScene wires the UI catalog and Canvas layers through `GameplayCompositionRoot`. At the end of a run, a result popup shows earned coins; its green `CONTINUE` button returns to the lobby. `PlayerProgressStorage` keeps the coin balance as JSON in `PlayerPrefs`, loading it in either scene and saving after a run reward is applied. Upgrade levels are not stored until upgrade behavior is implemented.

HUD prefabs in `Assets/Game/Prefabs/UI/` display launch power before launching, then forward distance (meters), Rigidbody speed (km/h) and track progress. Distance starts at the release position and retains the furthest forward position. `HUD Target Distance` on `GameplayCompositionRoot` sets a distance goal; zero uses the end of active road colliders. A run ends on a crash, when momentum is lost, or when the distance goal is reached. The default distance reward is one coin per full meter; each collected coin pickup adds 100 coins. HUD graphics use plain Images without sprites and do not intercept input. The widget layer respects the screen safe area.

## Obstacles

`Assets/Game/Prefabs/Obstacles/IcebergObstacle_01.prefab` uses the supplied iceberg visual and a solid BoxCollider. Player collisions stop and freeze the attempt, then show the run result. `PuddleObstacle.prefab` reuses one mesh from `Park_BushObstacles_Shadows` with a shallow trigger. `Additional Resistance` controls continuous ground deceleration inside the puddle (30 m/s² by default). Exiting or disabling the puddle removes its resistance; airborne movement is unaffected. Overlapping puddles use the strongest resistance. Place each prefab on the road or snow surface.

Four scene coins use trigger colliders and disappear when the player reaches them. Each collected coin adds one coin to the run reward. Their icons on the progress widget use the same world-distance positions as the placed coins.
