using UnityEngine;

namespace Emberfall.Physics
{
    /// <summary>
    /// Student-written collision methods used by the Physics Lab.
    /// Keeping the algorithms separate makes them easy to test and explain.
    /// </summary>
    public static class CollisionAlgorithms2D
    {
        public readonly struct Contact
        {
            public readonly bool Intersects;
            public readonly Vector2 Normal;
            public readonly float Penetration;

            public Contact(bool intersects, Vector2 normal, float penetration)
            {
                Intersects = intersects;
                Normal = normal;
                Penetration = penetration;
            }
        }

        public static Contact CircleCircle(Vector2 a, float radiusA, Vector2 b, float radiusB)
        {
            Vector2 delta = b - a;
            float combinedRadius = radiusA + radiusB;
            float distanceSquared = delta.sqrMagnitude;
            if (distanceSquared >= combinedRadius * combinedRadius)
                return new Contact(false, Vector2.zero, 0f);

            float distance = Mathf.Sqrt(distanceSquared);
            Vector2 normal = distance > 0.0001f ? delta / distance : Vector2.right;
            return new Contact(true, normal, combinedRadius - distance);
        }

        public static Contact AabbAabb(Vector2 a, Vector2 halfA, Vector2 b, Vector2 halfB)
        {
            Vector2 delta = b - a;
            float overlapX = halfA.x + halfB.x - Mathf.Abs(delta.x);
            float overlapY = halfA.y + halfB.y - Mathf.Abs(delta.y);
            if (overlapX <= 0f || overlapY <= 0f)
                return new Contact(false, Vector2.zero, 0f);

            if (overlapX < overlapY)
                return new Contact(true, new Vector2(Mathf.Sign(delta.x), 0f), overlapX);
            return new Contact(true, new Vector2(0f, Mathf.Sign(delta.y)), overlapY);
        }

        public static Contact ObbObb(
            Vector2 centerA, Vector2 halfA, float angleA,
            Vector2 centerB, Vector2 halfB, float angleB)
        {
            Vector2[] verticesA = GetVertices(centerA, halfA, angleA);
            Vector2[] verticesB = GetVertices(centerB, halfB, angleB);
            Vector2[] axes =
            {
                EdgeNormal(verticesA[1] - verticesA[0]),
                EdgeNormal(verticesA[3] - verticesA[0]),
                EdgeNormal(verticesB[1] - verticesB[0]),
                EdgeNormal(verticesB[3] - verticesB[0])
            };

            float smallestOverlap = float.MaxValue;
            Vector2 smallestAxis = Vector2.zero;
            foreach (Vector2 axis in axes)
            {
                Project(verticesA, axis, out float minA, out float maxA);
                Project(verticesB, axis, out float minB, out float maxB);
                float overlap = Mathf.Min(maxA, maxB) - Mathf.Max(minA, minB);
                if (overlap <= 0f) return new Contact(false, Vector2.zero, 0f);
                if (overlap < smallestOverlap)
                {
                    smallestOverlap = overlap;
                    smallestAxis = axis;
                }
            }

            if (Vector2.Dot(centerB - centerA, smallestAxis) < 0f) smallestAxis = -smallestAxis;
            return new Contact(true, smallestAxis, smallestOverlap);
        }

        public static void Resolve(
            ref Vector2 position, ref Vector2 velocity, Contact contact,
            float restitution = 0.55f)
        {
            if (!contact.Intersects) return;
            position -= contact.Normal * contact.Penetration;
            float velocityAlongNormal = Vector2.Dot(velocity, contact.Normal);
            if (velocityAlongNormal > 0f)
                velocity -= (1f + restitution) * velocityAlongNormal * contact.Normal;
        }

        static Vector2[] GetVertices(Vector2 center, Vector2 halfSize, float angleDegrees)
        {
            Quaternion rotation = Quaternion.Euler(0f, 0f, angleDegrees);
            return new[]
            {
                center + (Vector2)(rotation * new Vector2(-halfSize.x, -halfSize.y)),
                center + (Vector2)(rotation * new Vector2(halfSize.x, -halfSize.y)),
                center + (Vector2)(rotation * new Vector2(-halfSize.x, halfSize.y)),
                center + (Vector2)(rotation * new Vector2(halfSize.x, halfSize.y))
            };
        }

        static Vector2 EdgeNormal(Vector2 edge) => new Vector2(-edge.y, edge.x).normalized;

        static void Project(Vector2[] vertices, Vector2 axis, out float min, out float max)
        {
            min = max = Vector2.Dot(vertices[0], axis);
            for (int i = 1; i < vertices.Length; i++)
            {
                float value = Vector2.Dot(vertices[i], axis);
                min = Mathf.Min(min, value);
                max = Mathf.Max(max, value);
            }
        }
    }
}
