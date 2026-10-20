# Assignment 1 Evidence Map

This document maps the supplied project to the assessment criteria. It is supporting evidence; the lecturer's brief remains the authoritative marking document.

| Criterion | Project evidence |
|---|---|
| Completion, playability, and stability | `Emberfall.unity` contains a title screen, tutorial prompts, three chamber objectives, upgrade/healing interactions, boss encounter, victory/defeat states, pause, retry, and saved best level. |
| Motion and Euler integration | `EulerBody2D.cs` accumulates forces, calculates acceleration with `a = F / m`, then performs semi-implicit Euler updates: `v += a * dt` followed by `p += v * dt`. Live values are displayed in `PhysicsLab.unity`. |
| Forces and gravity | Physics Lab applies gravity continuously, accepts horizontal force with A/D, upward impulse with Space, drag, mass, and restitution. |
| Circle collision response | `CollisionAlgorithms2D.CircleCircle` calculates normal and penetration. Mode 1 displays and resolves the contact. |
| AABB collision response | `CollisionAlgorithms2D.AabbAabb` compares axis overlap and chooses the minimum-penetration axis. Mode 2 displays and resolves the contact. |
| SAT collision response | `CollisionAlgorithms2D.ObbObb` projects the four oriented-box axes and uses the minimum overlap as the collision normal. Mode 3 displays and resolves the contact. |
| Unity physics integration | Gameplay actors have `Rigidbody2D` and `CircleCollider2D` identity components. The Physics Lab also uses 2D colliders while keeping the required algorithms explicit and inspectable. |
| Code quality and reuse | Audio, UI, gameplay, custom motion, collision algorithms, editor setup, verification, and delivery are separated into focused scripts and folders. |
| Project organization | Assets are grouped under `Art`, `Audio`, `Documentation`, `Editor`, `Scenes`, `Scripts`, and `Settings`. Runtime scene objects use named system, camera, and content roots. The adventure level is serialized in the scene for inspection and editing. |
| Testing and problem resolution | `Tools > Testing > Run Gameplay Checks` verifies progression and the four mathematical physics cases. `Tools > Project > Validate Scenes` validates project structure and submission requirements. |

## Recommended evidence order

1. Show the project hierarchy and organized asset folders.
2. Play the adventure: movement, slash, dash, nova, enemy response, sigil, chest, upgrade, and boss.
3. Open Physics Lab from the title screen.
4. Demonstrate modes 1, 2, and 3. Apply force and impulse while pointing out the live position, velocity, acceleration, force, and collision values.
5. Show the relevant methods in `EulerBody2D.cs` and `CollisionAlgorithms2D.cs`.
6. Show the successful automated checks and Windows build.

## Student evidence still required

- Student and group member names, intake code, dates, and assigned role.
- A truthful account of each member's contribution with version-control screenshots or a contribution log.
- Screenshots captured from the final project and build.
- The required 1-2 minute videos and their shareable links.
- Any lecturer-specific Part 1 exercise that was distributed separately from this brief.
