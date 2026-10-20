# Emberfall

A 2D dungeon action game and physics demonstration built with Unity 6.3 LTS (6000.3.9f1).

## Open and play

1. Clone this repository. The bundled art, audio, and font files are stored directly in Git.
2. Add the repository folder in Unity Hub and open with Unity 6000.3.9f1.
3. Open `Assets/Emberfall/Scenes/Emberfall.unity` and press Unity's standard Play button.
4. Press Enter or select Begin Adventure on the title screen.

The adventure scene contains saved environment, actor, collectible, and effect groups under `__SYSTEMS/Adventure`. Expand these in the Hierarchy to inspect and edit the starting level, sprites, enemies, and player. The camera is under `__CAMERAS`. Retry reloads this saved scene. Scene walkability is stored on the Adventure component; changing the floor layout requires updating its walkability data in code.

`Assets/Emberfall/Scenes/PhysicsLab.unity` is the separate physics demonstration. Both scenes are enabled in Build Settings, with the adventure first.

## Controls

| Action | Input |
| --- | --- |
| Move | WASD / arrow keys |
| Attack | J / left mouse |
| Dash | Space |
| Nova | Q |
| Heal | H |
| Interact | E |
| Pause | Escape |

## Development

- Use Scene view, the Hierarchy, Inspector, and Unity's Play controls for everyday work.
- `Tools > Testing > Run Gameplay Checks`: run from a fresh adventure in Play mode. Checks combat, progression, collision algorithms, and scene reload. The check resets the play session.
- `Tools > Project > Validate Scenes`: validate required scenes, build configuration, assets, and documentation.
- `Tools > Project > Prepare Scenes`: explicit project setup; preserves existing adventure objects, recreates the generated Physics Lab, and configures rendering. Ordinary editing does not require running it.
- `Tools > Build > Windows Game`: creates a Windows build at `Builds/Windows/Emberfall.exe`. Unity's standard Build Profiles window also works.

Game logic is in `Assets/Emberfall/Scripts/Gameplay`; presentation, audio, and physics have separate UI, Core, and Physics folders. The current HUD uses IMGUI. Actor movement and dungeon walkability are handled by gameplay code; actor Rigidbody2D components do not drive movement. A full conversion to prefab-based actor behaviours and Canvas/Tilemap authoring would be a separate refactor.

Asset credits and licenses are under `Assets/Emberfall/Documentation`. The technical report is a draft with student-specific placeholders.

Only source assets, their `.meta` files, packages, and project settings are versioned. Unity caches, local editor settings, generated IDE files, and build outputs are ignored.
