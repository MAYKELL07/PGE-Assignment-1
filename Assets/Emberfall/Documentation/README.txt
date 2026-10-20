EMBERFALL - ASSIGNMENT 1 EDITION

PROJECT OVERVIEW
Emberfall is a compact single-player 2D action RPG built in Unity 6. The player clears three dungeon chambers, activates their sigils, upgrades at Keeper Orin, and defeats the Hollow King. A separate Physics Lab provides visible evidence of the motion and collision algorithms required by the assignment brief.

SCENES
Assets/Emberfall/Scenes/Emberfall.unity   - complete playable RPG
Assets/Emberfall/Scenes/PhysicsLab.unity  - interactive physics demonstration

ADVENTURE CONTROLS
WASD / arrows  Move
J / left mouse Slash
Space          Dash
Q              Arcane nova
H              Use potion
E              Interact / activate sigil
Escape         Pause

PHYSICS LAB CONTROLS
1 / 2 / 3  Circle / AABB / SAT (oriented box) mode
A / D      Apply horizontal force
Space      Apply upward impulse
R          Reset demonstration
Escape     Return to adventure

PROJECT STRUCTURE
Art             Imported visual assets and runtime sprite resources
Audio           Music and sound effects loaded by ProjectAudio
Documentation   Credits, licenses, rubric evidence, report draft, video plan
Editor          Setup, validation, automated checks, and Windows build tools
Scenes          Adventure and Physics Lab scenes
Scripts         Core, gameplay, physics, and UI code
Settings        URP 2D renderer and pipeline assets

EDITOR TOOLS
Tools > Project > Prepare Scenes
Tools > Testing > Run Gameplay Checks
Tools > Build > Windows Game

TESTING
The automated gameplay checks cover progression, combat values, economy, boss state, Circle/AABB/SAT collision detection, penetration response, and semi-implicit Euler integration. Validation results are written to the Temp folder and Unity Console.

THIRD-PARTY CONTENT
All bundled art, UI, fonts, music, and sound effects are free assets with attribution and license details in ThirdPartyAssets.md. Original license files are stored in this folder.

SUBMISSION NOTE
Replace every square-bracket placeholder in TechnicalReportDraft.md with the student/group details and personal contribution evidence before submission. Record the requested videos using DemoVideoShotList.md.
