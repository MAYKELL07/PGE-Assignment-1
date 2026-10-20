# EMBERFALL
## Physics for Games — Assignment 1 Technical Report Draft

**Student:** [NAME]
**Student ID:** [STUDENT ID]
**Intake:** [INTAKE CODE]
**Group members:** [GROUP MEMBER NAMES]
**Assigned date:** [DATE]
**Completed date:** [DATE]
**Video links:** [PART 1 VIDEO] / [PART 2 VIDEO]

> Replace all square-bracket placeholders and insert final screenshots before submission. Describe only work the named student actually completed.

## 1. Game overview

Emberfall is a single-player 2D dungeon action RPG created in Unity 6. The player explores a compact dungeon, defeats enemies in three chambers, activates each chamber sigil, spends collected gold on a permanent run upgrade, and confronts the Hollow King. The design keeps the scope small enough to polish while still demonstrating a complete gameplay loop, progression, combat feedback, user interface, audio, collision, and a final win condition.

The visual direction uses a consistent pixel-art dungeon palette. The interface uses readable framed panels, clear resource bars, short tutorial prompts, and immediate sound and color feedback. All third-party content is free and is documented in the project.

## 2. Core mechanics and gameplay loop

The player moves in four directions, attacks with a short-range sword slash, dashes to reposition, and casts an area nova with a cooldown. Enemies pursue the player, attack within range, take damage, and drop rewards. The player can open chests, activate objective sigils, drink potions, heal at the hearth, and improve attack damage through Keeper Orin.

The main loop is: explore a chamber, manage health and ability cooldowns, defeat enemies, collect gold, activate its sigil, improve the character, and unlock the boss route. Victory records the highest level locally. Defeat and victory screens both support a clean retry flow.

## 3. System integration and architecture

The project separates responsibilities into core, gameplay, UI, and physics scripts. `EmberfallGame` owns the adventure state and runtime encounter objects. `EmberfallUI` builds and updates the presentation layer. `ProjectAudio` owns persistent music and effect sources and provides reusable event methods. `EulerBody2D` and `CollisionAlgorithms2D` are independent mathematical components used by `PhysicsShowcase`.

Unity scenes and assets follow a predictable folder structure. Runtime objects are grouped under named system, camera, and content roots. Editor utilities prepare scenes, validate submission evidence, run deterministic checks, and make the Windows build. The adventure level is saved in the scene, including sprite assets and serialized actor and collectible references. Retry reloads the saved scene.

## 4. Motion, forces, and numerical integration

The Physics Lab makes the motion model visible. A body stores mass **m**, position **p**, velocity **v**, accumulated force **F**, and acceleration **a**. Newton's second law gives:

`a = F / m`

Gravity and player-controlled horizontal force are accumulated each fixed step. The project then applies semi-implicit Euler integration:

`v(t + dt) = v(t) + a(t) * dt`

`p(t + dt) = p(t) + v(t + dt) * dt`

Velocity is updated before position. This is more stable for simple game motion than explicit Euler while remaining easy to inspect. Drag reduces velocity over time, and Space applies an instantaneous impulse by changing velocity with `impulse / mass`. The live display shows position, velocity, acceleration, net force, and collision state so the calculation can be related directly to the observed motion.

## 5. Collision detection and response

### 5.1 Circle versus circle

For two circles, the algorithm compares the squared distance between centers with the squared sum of radii. An intersection exists when `distanceSquared < (rA + rB)^2`. The normalized center difference provides the contact normal and `rA + rB - distance` provides penetration.

### 5.2 Axis-aligned bounding boxes

For AABBs, the algorithm calculates overlap on the world X and Y axes. If either overlap is zero or negative, the boxes are separated. Otherwise, the smaller overlap identifies the minimum translation axis and becomes the collision normal.

### 5.3 Separating Axis Theorem

For oriented boxes, SAT tests the local X and Y axes of both boxes. Each box is projected onto each candidate axis. A separating axis means there is no collision. When all projections overlap, the axis with the smallest overlap becomes the contact normal and penetration depth.

### 5.4 Response

The response first moves the dynamic body out of penetration along the contact normal. It then reflects the velocity component pointing into the surface, scaled by restitution. This prevents persistent overlap and produces a visible bounce. All three modes use the same contact structure and response function, showing reusable design.

## 6. Unity 6 implementation

The project uses Unity 6 with the Universal Render Pipeline's 2D renderer. Gameplay actors carry `Rigidbody2D` and `Collider2D` components for scene identity and standard engine integration. The Physics Lab keeps its mathematical algorithms explicit so the assignment concepts can be inspected rather than hidden entirely inside the engine. The build settings contain both the adventure and evidence scenes.

## 7. Testing and problem resolution

Automated checks exercise health, mana, combat damage, economy, room progression, boss unlocking, victory, Circle and AABB penetration values, SAT intersection, and a known Euler integration result. Submission validation checks the 2D project setting, both scenes, build settings, audio count, physics scripts, and evidence documents. The final Windows build is also launched as a standalone player and checked for runtime exceptions.

During development, [DESCRIBE A REAL PROBLEM YOU PERSONALLY SOLVED]. Explain the symptom, evidence used to locate the cause, the change made, and the result. Include a before/after screenshot or code extract if useful.

## 8. Individual contribution and reflection

**My role:** [ROLE]

**My completed work:** [TRUTHFUL BULLET LIST WITH COMMITS, FILES, OR OTHER EVIDENCE]

**What I learned:** [DISCUSS MOTION INTEGRATION, COLLISION RESPONSE, UNITY WORKFLOW, AND TESTING]

**What I would improve:** [SPECIFIC FUTURE IMPROVEMENTS BASED ON TESTING OR FEEDBACK]

## 9. Conclusion

Emberfall combines a complete, playable RPG loop with a focused physics evidence scene. The project demonstrates force-based motion, semi-implicit Euler integration, Circle and AABB collision, SAT for oriented boxes, penetration correction, restitution, Unity 2D integration, organized production assets, automated checks, and a distributable Windows build.

## References and asset credits

See `ThirdPartyAssets.md` for asset sources and license details. Cite the supplied assignment brief, Unity 6 documentation consulted during development, and any additional academic sources using the institution's required reference style.
