# Emberfall - PGE Assignment 1

A small 2D dungeon game made in Unity 6.3 (6000.3.9f1) for a game physics assignment.

The aim is to clear three rooms, collect their sigils and beat the boss. There is also a separate scene for trying out Euler integration and circle, AABB and SAT collisions.

## Running it

Open the project in Unity Hub, then open `Assets/Emberfall/Scenes/Emberfall.unity` and press Play. The level objects are under `__SYSTEMS > Adventure` in the Hierarchy.

`Assets/Emberfall/Scenes/PhysicsLab.unity` is the physics demo. It can also be opened from the game's title screen.

## Controls

- WASD / arrow keys: move
- J / left click: attack
- Space: dash
- Q: nova attack
- H: use a potion
- E: interact
- Escape: pause

In Physics Lab, use 1/2/3 to change collision mode, A/D to apply force, Space for an impulse, and R to reset.

## Project notes

Most game logic is in `EmberfallGame.cs`. The UI is in `EmberfallUI.cs`, sound is in `ProjectAudio.cs`, and the physics calculations are in the Physics folder.

The level is saved in the scene. Floor collision still uses a grid stored by the game script, so moving a floor sprite alone will not change where the player can walk. The UI uses OnGUI rather than a Canvas.

There are a few helper options under Tools for checking the game, setting up the scenes and building for Windows. Run gameplay checks from a fresh Play session; they reset the level afterwards. A Windows build goes in `Builds/Windows`.

Art and audio credits are in `Assets/Emberfall/Documentation/ThirdPartyAssets.md`. The report draft still needs personal details, screenshots and contribution notes.
