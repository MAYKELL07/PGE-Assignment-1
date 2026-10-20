# PGE Assignment 1 - report draft

Student: [name and ID]
Group: [members, if applicable]
Date: [date]
Video: [link]

## The game

Emberfall is a small top-down dungeon game made in Unity 6. The player fights enemies in three rooms, collects sigils and then fights a boss. There are potions, a healing point and an attack upgrade bought with gold.

WASD moves the player, J attacks, Space dashes, Q uses nova, H heals and E interacts. Nova has a cooldown. The game has a title screen, pause screen, win/lose screens and a retry button.

[Add a screenshot of the game.]

## How the code is organised

`EmberfallGame.cs` handles movement, enemies, combat and progress. `EmberfallUI.cs` draws the menus and HUD using OnGUI. `ProjectAudio.cs` plays the music and sound effects.

The adventure level objects are saved in the scene. Movement uses a floor grid rather than Unity's collision solver. The Rigidbody2D components on actors have simulation disabled.

The physics work is in a separate PhysicsLab scene so the calculations can be shown clearly.

## Forces and Euler integration

`EulerBody2D.cs` stores the body's position, velocity, acceleration, force and mass.

Each step uses:

```
acceleration = force / mass
velocity += acceleration * deltaTime
position += velocity * deltaTime
```

Velocity is updated first, so this is semi-implicit Euler integration. The code also applies drag and clears the accumulated force after each step. Physics Lab applies gravity, A/D adds horizontal force, and Space adds an impulse divided by the mass.

[Add a screenshot showing the values and explain one example.]

## Collision detection

The methods are in `CollisionAlgorithms2D.cs`.

- Circle-circle: compare the distance between centres with the sum of the radii.
- AABB: calculate overlap on X and Y. If both overlap, use the smaller overlap to separate the shapes.
- SAT: project both rotated boxes onto four axes. A gap on any axis means there is no collision. Otherwise, use the smallest overlap.

The response moves the moving shape out of overlap and changes the velocity along the collision normal. Restitution controls the bounce.

[Add screenshots of the three modes and describe the results.]

## Checks and problems

The gameplay checks cover attacking, cooldowns, healing, upgrades, sigils, the boss and retry. They also check example results for circle, AABB, SAT and Euler integration. The Windows build was generated successfully; its build report included package shader warnings. Add the results of a manual playthrough here as well.

[Describe a problem you actually worked on: what happened, how you investigated it and what changed.]

## Contribution and reflection

[Explain your actual contribution, with files or commits where useful.]

[Explain what you learned and what you found difficult.]

Some possible improvements are splitting up the large game script, using a Canvas for the UI, and making floor collision easier to edit alongside the level. Include improvements based on your own testing too.

## Assets and references

The art, font and audio sources are listed in `ThirdPartyAssets.md`. Include any course material or documentation you used, in the reference format required by the assignment.
