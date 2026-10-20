using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Emberfall.Physics
{
    // Displays the physics examples and their values on screen.
    public sealed class PhysicsShowcase : MonoBehaviour
    {
        enum DemoMode
        {
            Circle = 1,
            Aabb = 2,
            Sat = 3
        }

        EulerBody2D body;
        Transform movingShape;
        Transform staticShape;
        SpriteRenderer movingRenderer;
        SpriteRenderer staticRenderer;
        Camera sceneCamera;
        DemoMode mode = DemoMode.Circle;
        float windForce;
        float rotation;
        string collisionState = "SEPARATED";
        Font font;
        Texture2D pixel;
        GUIStyle label;
        GUIStyle panel;
        GUIStyle button;
        const float Mass = 1.5f;
        static readonly Vector2 Gravity = new Vector2(0f, -9.81f);
        void Start()
        {
            sceneCamera = Camera.main;
            if (!sceneCamera)
            {
                sceneCamera = new GameObject("Physics Lab Camera").AddComponent<Camera>();
                sceneCamera.tag = "MainCamera";
            }

            sceneCamera.orthographic = true;
            sceneCamera.orthographicSize = 6.2f;
            sceneCamera.transform.position = new Vector3(0f, 0f, -10f);
            sceneCamera.backgroundColor = new Color(0.05f, 0.07f, 0.11f);
            ProjectAudio.Create(transform);
            BuildBackdrop();
            SelectMode(DemoMode.Circle);
        }

        void BuildBackdrop()
        {
            for (int x = -10; x <= 10; x++)
            {
                var line = CreateQuad("Grid line", new Vector2(x, 0f), new Vector2(0.018f, 12f), new Color(0.16f, 0.2f, 0.27f), -20);
                line.position = new Vector3(x, 0f, 0f);
            }

            for (int y = -6; y <= 6; y++)
            {
                CreateQuad("Grid line", new Vector2(0f, y), new Vector2(20f, 0.018f), new Color(0.16f, 0.2f, 0.27f), -20);
            }

            CreateQuad("Floor", new Vector2(0f, -4.5f), new Vector2(18f, 0.25f), new Color(0.31f, 0.37f, 0.45f), -5);
        }

        void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            windForce = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 8f : 0f) - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 8f : 0f);
            if (keyboard.spaceKey.wasPressedThisFrame)
            {
                body.AddImpulse(Vector2.up * 8.5f);
                ProjectAudio.Instance?.PlayConfirm();
            }

            if (keyboard.rKey.wasPressedThisFrame)
            {
                ResetBody();
            }

            if (keyboard.digit1Key.wasPressedThisFrame)
            {
                SelectMode(DemoMode.Circle);
            }

            if (keyboard.digit2Key.wasPressedThisFrame)
            {
                SelectMode(DemoMode.Aabb);
            }

            if (keyboard.digit3Key.wasPressedThisFrame)
            {
                SelectMode(DemoMode.Sat);
            }

            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                SceneManager.LoadScene("Emberfall");
            }
        }

        void FixedUpdate()
        {
            if (body == null)
            {
                return;
            }

            // Multiple forces are accumulated before the Euler integration step.
            body.AddForce(Gravity * Mass);
            body.AddForce(Vector2.right * windForce);
            body.AddForce(-body.Velocity * 0.25f);
            body.Integrate(Time.fixedDeltaTime, 0.08f);
            Vector2 position = body.Position;
            Vector2 velocity = body.Velocity;
            CollisionAlgorithms2D.Contact contact = DetectCollision(position);
            collisionState = contact.Intersects ? "CONTACT - RESPONSE APPLIED" : "SEPARATED";
            if (contact.Intersects)
            {
                CollisionAlgorithms2D.Resolve(ref position, ref velocity, contact);
                body.SetState(position, velocity);
                ProjectAudio.Instance?.PlayHit();
            }

            if (position.y < -4.05f)
            {
                position.y = -4.05f;
                velocity.y = Mathf.Abs(velocity.y) * 0.58f;
                body.SetState(position, velocity);
            }

            movingShape.position = position;
            if (mode == DemoMode.Sat)
            {
                rotation += 22f * Time.fixedDeltaTime;
                movingShape.rotation = Quaternion.Euler(0f, 0f, rotation);
            }
        }

        CollisionAlgorithms2D.Contact DetectCollision(Vector2 position)
        {
            Vector2 target = staticShape.position;
            switch (mode)
            {
                case DemoMode.Circle:
                    return CollisionAlgorithms2D.CircleCircle(position, 0.62f, target, 0.92f);
                case DemoMode.Aabb:
                    return CollisionAlgorithms2D.AabbAabb(position, new Vector2(0.7f, 0.7f), target, new Vector2(1.4f, 0.7f));
                default:
                    return CollisionAlgorithms2D.ObbObb(position, new Vector2(0.78f, 0.6f), rotation, target, new Vector2(1.35f, 0.65f), -24f);
            }
        }

        void SelectMode(DemoMode selectedMode)
        {
            mode = selectedMode;
            if (movingShape)
            {
                Destroy(movingShape.gameObject);
            }

            if (staticShape)
            {
                Destroy(staticShape.gameObject);
            }

            if (mode == DemoMode.Circle)
            {
                movingShape = CreateCircle("Dynamic Circle", new Vector2(-3.7f, 2.7f), 0.62f, new Color(0.98f, 0.55f, 0.24f), 5);
                staticShape = CreateCircle("Static Circle", new Vector2(2.3f, -2.5f), 0.92f, new Color(0.28f, 0.78f, 0.72f), 4);
                movingShape.gameObject.AddComponent<CircleCollider2D>().radius = 0.5f;
                staticShape.gameObject.AddComponent<CircleCollider2D>().radius = 0.5f;
            }
            else
            {
                movingShape = CreateQuad("Dynamic Box", new Vector2(-3.7f, 2.7f), new Vector2(1.4f, 1.4f), new Color(0.98f, 0.55f, 0.24f), 5);
                staticShape = CreateQuad("Static Box", new Vector2(2.3f, -2.45f), new Vector2(2.8f, 1.4f), new Color(0.28f, 0.78f, 0.72f), 4);
                movingShape.gameObject.AddComponent<BoxCollider2D>();
                staticShape.gameObject.AddComponent<BoxCollider2D>();
                if (mode == DemoMode.Sat)
                {
                    staticShape.rotation = Quaternion.Euler(0f, 0f, -24f);
                }
            }

            var rigidbody = movingShape.gameObject.AddComponent<Rigidbody2D>();
            rigidbody.bodyType = RigidbodyType2D.Kinematic;
            rigidbody.simulated = false;
            movingRenderer = movingShape.GetComponent<SpriteRenderer>();
            staticRenderer = staticShape.GetComponent<SpriteRenderer>();
            ResetBody();
            ProjectAudio.Instance?.PlayClick();
        }

        void ResetBody()
        {
            Vector2 start = new Vector2(-3.7f, 2.7f);
            body = new EulerBody2D(start, Mass);
            body.AddImpulse(new Vector2(5.7f, 1.5f));
            rotation = 18f;
            if (movingShape)
            {
                movingShape.position = start;
                movingShape.rotation = Quaternion.Euler(0f, 0f, mode == DemoMode.Sat ? rotation : 0f);
            }
        }

        Transform CreateQuad(string objectName, Vector2 position, Vector2 scale, Color color, int sortingOrder)
        {
            var gameObject = new GameObject(objectName);
            gameObject.transform.SetParent(transform);
            gameObject.transform.position = position;
            gameObject.transform.localScale = scale;
            var renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = SolidSprite();
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return gameObject.transform;
        }

        Transform CreateCircle(string objectName, Vector2 position, float radius, Color color, int sortingOrder)
        {
            var gameObject = new GameObject(objectName);
            gameObject.transform.SetParent(transform);
            gameObject.transform.position = position;
            gameObject.transform.localScale = Vector3.one * radius * 2f;
            var renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = CircleSprite();
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return gameObject.transform;
        }

        Sprite SolidSprite()
        {
            var texture = new Texture2D(1, 1)
            {
                filterMode = FilterMode.Point
            };
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }

        Sprite CircleSprite()
        {
            const int size = 64;
            var texture = new Texture2D(size, size)
            {
                filterMode = FilterMode.Bilinear
            };
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 point = new Vector2(x, y) - Vector2.one * (size - 1) * 0.5f;
                    texture.SetPixel(x, y, point.magnitude <= size * 0.48f ? Color.white : Color.clear);
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        void PrepareGui()
        {
            if (label != null)
            {
                return;
            }

            font = Resources.Load<Font>("Interface/Pixel");
            pixel = Texture2D.whiteTexture;
            label = new GUIStyle(GUI.skin.label)
            {
                font = font,
                fontSize = 22
            };
            label.normal.textColor = new Color(0.91f, 0.91f, 0.84f);
            panel = new GUIStyle(GUI.skin.box);
            panel.normal.background = pixel;
            button = new GUIStyle(GUI.skin.button)
            {
                font = font,
                fontSize = 22
            };
        }

        void OnGUI()
        {
            PrepareGui();
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1280 * scale) / 2f, (Screen.height - 720 * scale) / 2f), Quaternion.identity, Vector3.one * scale);
            GUI.color = new Color(0.05f, 0.07f, 0.11f, 0.94f);
            GUI.Box(new Rect(24, 20, 1232, 112), GUIContent.none, panel);
            GUI.color = Color.white;
            DrawText(48, 34, "PHYSICS LAB  /  ASSESSMENT SHOWCASE", 34, new Color(1f, 0.68f, 0.3f));
            DrawText(48, 80, "1 Circle-Circle    2 AABB    3 SAT / OBB    A-D Wind    SPACE Impulse    R Reset    ESC Adventure", 21, Color.white);
            GUI.color = new Color(0.05f, 0.07f, 0.11f, 0.94f);
            GUI.Box(new Rect(24, 512, 1232, 184), GUIContent.none, panel);
            GUI.color = Color.white;
            string algorithm = mode == DemoMode.Circle ? "CIRCLE-CIRCLE" : mode == DemoMode.Aabb ? "AABB-AABB" : "SEPARATING AXIS THEOREM";
            DrawText(48, 531, algorithm + "  /  " + collisionState, 27, collisionState.StartsWith("CONTACT") ? new Color(0.35f, 0.95f, 0.72f) : Color.white);
            DrawText(48, 570, $"POSITION  {body.Position.x, 6:0.00}, {body.Position.y, 6:0.00}       VELOCITY  {body.Velocity.x, 6:0.00}, {body.Velocity.y, 6:0.00}", 22, Color.white);
            DrawText(48, 607, $"ACCELERATION  {body.Acceleration.x, 6:0.00}, {body.Acceleration.y, 6:0.00}       MASS  {Mass:0.00} kg       dt  {Time.fixedDeltaTime:0.000}", 22, Color.white);
            DrawText(48, 645, $"FORCES  gravity {Gravity.y * Mass:0.00} N  +  wind {windForce:0.00} N  +  velocity drag    /    semi-implicit Euler integration", 21, new Color(0.75f, 0.82f, 0.9f));
            if (GUI.Button(new Rect(1040, 43, 180, 52), "ADVENTURE", button))
            {
                SceneManager.LoadScene("Emberfall");
            }
        }

        void DrawText(float x, float y, string text, int size, Color color)
        {
            var style = new GUIStyle(label)
            {
                fontSize = size
            };
            style.normal.textColor = color;
            GUI.Label(new Rect(x, y, 1140, 42), text, style);
        }
    }
}
