using UnityEngine;

namespace Emberfall.Physics
{
    // Updates velocity before position using force / mass.
    public sealed class EulerBody2D
    {
        public Vector2 Position { get; private set; }
        public Vector2 Velocity { get; private set; }
        public Vector2 Acceleration { get; private set; }
        public Vector2 AccumulatedForce { get; private set; }
        public float Mass { get; }

        public EulerBody2D(Vector2 startPosition, float mass)
        {
            Position = startPosition;
            Mass = Mathf.Max(0.01f, mass);
        }

        public void AddForce(Vector2 force)
        {
            AccumulatedForce += force;
        }

        public void AddImpulse(Vector2 impulse)
        {
            Velocity += impulse / Mass;
        }

        public void Integrate(float deltaTime, float linearDrag)
        {
            Acceleration = AccumulatedForce / Mass;
            Velocity += Acceleration * deltaTime;
            Velocity *= 1f / (1f + linearDrag * deltaTime);
            Position += Velocity * deltaTime;
            AccumulatedForce = Vector2.zero;
        }

        public void SetState(Vector2 position, Vector2 velocity)
        {
            Position = position;
            Velocity = velocity;
            Acceleration = Vector2.zero;
            AccumulatedForce = Vector2.zero;
        }
    }
}
