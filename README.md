# Sled Surfers Gameplay Test

Unity prototype for the supplied gameplay assignment, using Miraculous Ladybug assets.

## Unity Version

Unity 2022.3.62f3 LTS.

## Setup

1. Clone the repository and open its root directory in Unity Hub.
2. Open `Assets/Game/Scenes/SampleScene.unity`.
3. Press Play to preview the character animation. Gameplay movement is not implemented yet.

## Character Animation

`Assets/Game/Animations/CatNoir.controller` is assigned to the Cat Noir model in the sample scene. It uses the supplied Humanoid clips for Idle, Jump and Slide.

In Play Mode, select Cat Noir and open the Animator window to change its parameters:

| IsRunning | IsGrounded | Animation |
| --- | --- | --- |
| false | Either | Idle |
| true | false | Jump |
| true | true | Slide |

Jump and Slide hold their final pose after their non-looping clips finish. Landing is controlled by `IsGrounded`, rather than animation duration. Root motion is disabled; character movement will be controlled separately.

## Project Structure

- `Assets/Game/Scenes/`: gameplay layout (`GameScene`) and animation preview (`SampleScene`).
- `Assets/Game/Prefabs/Player/Player.prefab`: player root with a nested Cat Noir visual.
- `Assets/Game/Art/Characters/CatNoir/`: selected character model, clips, material and texture.
- `Assets/Game/Animations/`: character animation controller.
- `Assets/Ladybug/`: supplied Miraculous content.
- Other imported folders under `Assets/`: supporting art and effects.
- `Packages/` and `ProjectSettings/`: Unity dependencies and project configuration.

## Current Status

The asset pack, player prefab and basic character animation controller are available. `GameScene` contains the first Frozen Paris chunk and a player instance; physics and movement are not configured. The gameplay loop, progression and playable build are not implemented yet.

## Asset Compatibility

Missing custom shaders in the supplied pack are replaced with Built-in shaders for surfaces, vegetation, UI and particles. TextMeshPro shaders are included from the installed package. Original shader-specific effects, including world bending and custom reflections, are not reproduced. Some source texture references are absent from the supplied pack.
