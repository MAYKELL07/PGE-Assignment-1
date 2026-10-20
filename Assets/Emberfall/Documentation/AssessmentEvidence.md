# Physics checklist

These are the main places to look when explaining the assignment.

| Topic | File / scene |
| --- | --- |
| Force and mass | `EulerBody2D.cs`: acceleration = force / mass |
| Euler integration | `EulerBody2D.Integrate`: update velocity, then position |
| Gravity and impulse | `PhysicsShowcase.cs`: Physics Lab controls |
| Circle collision | `CollisionAlgorithms2D.CircleCircle` |
| AABB collision | `CollisionAlgorithms2D.AabbAabb` |
| SAT collision | `CollisionAlgorithms2D.ObbObb` |
| Collision response | `CollisionAlgorithms2D.Resolve` |
| Game movement and combat | `EmberfallGame.cs` in the adventure scene |

PhysicsLab has three modes. Press 1, 2 or 3, then use A/D and Space to move the shape. The values on screen show what the calculations are doing.

The adventure uses grid checks for movement. Its Rigidbody2D components have simulation turned off, so the custom physics calculations are demonstrated separately in Physics Lab.

## Still to add to the submission

- Screenshots of the game and each physics mode.
- Student / group details and a description of actual contributions.
- Video links and any other items requested in the assignment brief.

Tools > Testing > Run Gameplay Checks runs a set of checks in Play mode. Tools > Project > Validate Scenes checks the project files. Compare the submission against the lecturer's brief as well.
