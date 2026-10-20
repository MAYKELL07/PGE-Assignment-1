using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Emberfall
{
    public partial class EmberfallGame : MonoBehaviour
    {
        [Serializable]
        public class Foe
        {
            public Transform body;
            public SpriteRenderer art;
            public Vector2 home;
            public float hp;
            public float maxHp;
            public float speed;
            public float cooldown;
            public float windup;
            public bool boss;
            public int room;
        }

        [Serializable]
        class Pickup
        {
            public Transform body;
            public int type;
            public int room;
            public Vector2 position;
        }

        class Floater
        {
            public Vector2 pos;
            public string text;
            public Color color;
            public float life = 1;
        }

        public List<Foe> foes = new List<Foe>();
        [SerializeField]
        List<Pickup> pickups = new List<Pickup>();
        readonly List<Floater> floaters = new List<Floater>();
        readonly Dictionary<int, Sprite> spriteCache = new Dictionary<int, Sprite>();
        readonly HashSet<Vector2Int> walkableTiles = new HashSet<Vector2Int>();
        [SerializeField]
        List<Transform> embers = new List<Transform>();
        [SerializeField]
        bool authoredScene;
        [SerializeField, HideInInspector]
        Vector2Int[] floorCells;
        readonly Vector2[] roomCenters =
        {
            new Vector2(0, 0),
            new Vector2(-18, 0),
            new Vector2(18, 0),
            new Vector2(0, 15),
            new Vector2(0, 30)
        };
        readonly string[] roomNames =
        {
            "THE LAST HEARTH",
            "MOSSBOUND CRYPT",
            "THE SUNKEN ARMORY",
            "HALL OF WHISPERS",
            "THE EMBER THRONE"
        };
        public Transform hero;
        [SerializeField]
        SpriteRenderer heroArt;
        [SerializeField]
        Transform sword;
        Camera mainCamera;
        [SerializeField]
        Transform environmentRoot;
        [SerializeField]
        Transform actorRoot;
        [SerializeField]
        Transform itemRoot;
        [SerializeField]
        Transform effectRoot;
        // Player stats and progress for the current run.
        public int level = 1;
        public int xp;
        public int gold;
        public int potions = 3;
        public int sigils;
        public int kills;
        public int upgrade;
        public float hp = 100;
        public float MaxHP => 100 + (level - 1) * 20;

        public bool started;
        public bool won;
        public bool dead;
        public bool paused;
        bool dialog;
        bool bossAwake;
        bool[] collected = new bool[3];
        // Timers for attacks, dodging and damage feedback.
        float attackCooldown;
        float dashCooldown;
        float spellCooldown;
        float invulnerable;
        float slashTime;
        float dashTime;
        float clock;
        float shake;
        float messageTime;
        Vector2 facing = Vector2.right;
        Vector2 dashDirection;
        string message = "";
        string dialogText = "";
        public int CurrentRoom
        {
            get
            {
                int nearestRoom = 0;
                float nearestDistance = float.MaxValue;
                for (int i = 0; i < roomCenters.Length; i++)
                {
                    float distance = Vector2.Distance(hero.position, roomCenters[i]);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestRoom = i;
                    }
                }

                return nearestRoom;
            }
        }

        Color ink = new Color(.055f, .07f, .105f);
        Color panel = new Color(.075f, .095f, .14f, .97f);
        Color cream = new Color(.94f, .9f, .78f);
        Color amber = new Color(1, .69f, .29f);
        Color teal = new Color(.36f, .84f, .76f);
        GUIStyle label;
        GUIStyle title;
        GUIStyle small;
        GUIStyle button;
        Texture2D white;
        // Use the objects saved in the scene. The fallback builds an empty scene.
        public void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            mainCamera = Camera.main;
            if (!mainCamera)
            {
                mainCamera = new GameObject("Adventure Camera").AddComponent<Camera>();
                mainCamera.tag = "MainCamera";
            }

            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 7.4f;
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = new Color(.035f, .045f, .07f);
            mainCamera.transform.rotation = Quaternion.identity;
            mainCamera.transform.position = new Vector3(0, 0, -10);
            if (!mainCamera.GetComponent<AudioListener>())
            {
                mainCamera.gameObject.AddComponent<AudioListener>();
            }

            ProjectAudio.Create(transform);
            if (authoredScene)
            {
                foreach (var cell in floorCells)
                {
                    walkableTiles.Add(cell);
                }

                foreach (var foe in foes)
                {
                    foe.home = foe.body.position;
                }

                foreach (var pickup in pickups)
                {
                    pickup.position = pickup.body.position;
                }
            }
            else
            {
                CreateRuntimeHierarchy();
                BuildWorld();
                CreateHero();
            }

            Notify("Welcome to the last hearth.", 4);
        }

        void CreateHero()
        {
            hero = CreateSpriteObject("Warden", 96, Vector2.zero, 30, 1.25f).transform;
            heroArt = hero.GetComponent<SpriteRenderer>();
            hero.SetParent(actorRoot, true);
            AddPhysicsIdentity(hero.gameObject, .28f);
            sword = CreateSpriteObject("Blade", 106, new Vector2(.65f, 0), 32, .95f).transform;
            sword.SetParent(hero, true);
        }

#if UNITY_EDITOR
        // Called explicitly by the level authoring tool, never on editor startup.
        public void CreateEditableLevel()
        {
            if (transform.childCount > 0)
            {
                throw new InvalidOperationException("Rebuild requires an empty game root. Existing level objects were preserved.");
            }

            walkableTiles.Clear();
            foes.Clear();
            pickups.Clear();
            embers.Clear();
            CreateRuntimeHierarchy();
            BuildWorld();
            CreateHero();
            floorCells = new Vector2Int[walkableTiles.Count];
            walkableTiles.CopyTo(floorCells);
            authoredScene = true;
        }

#endif
        void CreateRuntimeHierarchy()
        {
            environmentRoot = NewRoot("01 Environment");
            actorRoot = NewRoot("02 Actors");
            itemRoot = NewRoot("03 Collectibles");
            effectRoot = NewRoot("04 Effects");
        }

        Transform NewRoot(string rootName)
        {
            var root = new GameObject(rootName).transform;
            root.SetParent(transform);
            return root;
        }

        void AddPhysicsIdentity(GameObject target, float radius)
        {
            var body = target.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.simulated = false;
            var collider = target.AddComponent<CircleCollider2D>();
            collider.radius = radius;
            collider.isTrigger = true;
        }

        Sprite GetSprite(int id)
        {
            if (spriteCache.TryGetValue(id, out var s))
            {
                return s;
            }

            var t = Resources.Load<Texture2D>("Dungeon/tile_" + id.ToString("0000"));
            if (!t)
            {
                throw new Exception("Missing Kenney tile " + id);
            }

            t.filterMode = FilterMode.Point;
            s = UnityEngine.Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(.5f, .5f), 16);
            spriteCache[id] = s;
            return s;
        }

        GameObject CreateSpriteObject(string name, int id, Vector2 p, int order = 0, float scale = 1)
        {
            var g = new GameObject(name);
            g.transform.SetParent(environmentRoot ? environmentRoot : transform);
            g.transform.position = p;
            g.transform.localScale = Vector3.one * scale;
            var sr = g.AddComponent<SpriteRenderer>();
            sr.sprite = GetSprite(id);
            sr.sortingOrder = order;
            return g;
        }

        void AddFloorRectangle(int x, int y, int w, int h)
        {
            for (int a = x; a < x + w; a++)
            {
                for (int b = y; b < y + h; b++)
                {
                    walkableTiles.Add(new Vector2Int(a, b));
                }
            }
        }

        void BuildWorld()
        {
            for (int i = 0; i < 5; i++)
            {
                AddFloorRectangle((int)roomCenters[i].x - 6, (int)roomCenters[i].y - 4, 13, 9);
            }

            AddFloorRectangle(-18, -1, 37, 3);
            AddFloorRectangle(-1, 0, 3, 31);
            // A fixed seed keeps the tile pattern the same when rebuilding the level.
            var random = new System.Random(41);
            foreach (var p in walkableTiles)
            {
                var a = CreateSpriteObject("Stone floor", random.NextDouble() < .14 ? 12 : 0, p, -20);
                a.GetComponent<SpriteRenderer>().color = new Color(.57f, .62f, .68f);
            }

            var walls = new HashSet<Vector2Int>();
            foreach (var p in walkableTiles)
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        var n = p + new Vector2Int(x, y);
                        if (!walkableTiles.Contains(n))
                        {
                            walls.Add(n);
                        }
                    }
                }
            }

            foreach (var p in walls)
            {
                CreateSpriteObject("Crypt wall", walkableTiles.Contains(p + Vector2Int.down) ? 14 : 40, p, -5);
            }

            for (int r = 0; r < 5; r++)
            {
                var c = roomCenters[r];
                CreateSpriteObject("Banner", 29, c + new Vector2(-4, 4), 0);
                CreateSpriteObject("Banner", 29, c + new Vector2(4, 4), 0);
                CreateSpriteObject("Carved pillar", 7, c + new Vector2(-5, 3), 1);
                CreateSpriteObject("Carved pillar", 8, c + new Vector2(5, 3), 1);
                CreateSpriteObject("Rubble", 24, c + new Vector2(-5, -3), 0);
                CreateSpriteObject("Rubble", 13, c + new Vector2(5, -3), 0);
                for (int j = 0; j < 2; j++)
                {
                    var t = CreateSpriteObject("Ember brazier", j == 0 ? 127 : 125, c + new Vector2(j == 0 ? -4 : 4, 2.7f), 4, .9f).transform;
                    embers.Add(t);
                }

                if (r > 0 && r < 4)
                {
                    AddPickup(c + new Vector2(0, 2.5f), 2, r);
                    AddPickup(c + new Vector2(4, -2), 3, r);
                }
            }

            CreateSpriteObject("Keeper Orin", 84, new Vector2(-2, 1.5f), 20, 1.25f);
            CreateSpriteObject("Hearth", 110, new Vector2(2, 1.5f), 20, 1.2f);
            CreateSpriteObject("Hearth carpet", 48, Vector2.zero, -10, 3);
            CreateSpriteObject("Sealed throne", 41, roomCenters[4] + new Vector2(0, 3), 3, 2);
            SpawnFoes();
        }

        void SpawnFoes()
        {
            for (int r = 1; r <= 3; r++)
            {
                for (int i = 0; i < 4; i++)
                {
                    SpawnFoe(roomCenters[r] + new Vector2(-3 + (i % 2) * 6, -1 + (i / 2) * 3), r, i % 2 == 0 ? 108 : 121, false);
                }
            }

            SpawnFoe(roomCenters[4], 4, 124, true);
        }

        void SpawnFoe(Vector2 p, int room, int tile, bool boss)
        {
            var g = CreateSpriteObject(boss ? "The Hollow King" : "Crypt wanderer", tile, p, 20, boss ? 2.3f : 1.15f);
            g.transform.SetParent(actorRoot, true);
            AddPhysicsIdentity(g, boss ? .55f : .3f);
            foes.Add(new Foe
            {
                body = g.transform,
                art = g.GetComponent<SpriteRenderer>(),
                home = p,
                hp = boss ? 240 : 42 + room * 8,
                maxHp = boss ? 240 : 42 + room * 8,
                speed = boss ? 1.35f : 1.35f + room * .15f,
                cooldown = .7f,
                room = room,
                boss = boss
            });
        }

        void AddPickup(Vector2 p, int type, int room = 0)
        {
            int id = type == 0 ? 101 : type == 1 ? 115 : type == 2 ? 56 : 89;
            var g = CreateSpriteObject(type == 2 ? "Ancient sigil" : type == 3 ? "Supply chest" : "Loot", id, p, 12, type == 2 ? 1.3f : .9f);
            g.transform.SetParent(itemRoot, true);
            pickups.Add(new Pickup { body = g.transform, type = type, room = room, position = p });
        }

        // Check all four corners so the player cannot clip through room edges.
        public bool Walkable(Vector2 p)
        {
            float radius = .26f;
            return walkableTiles.Contains(Vector2Int.RoundToInt(p + new Vector2(radius, radius)))
                && walkableTiles.Contains(Vector2Int.RoundToInt(p - new Vector2(radius, radius)))
                && walkableTiles.Contains(Vector2Int.RoundToInt(p + new Vector2(radius, -radius)))
                && walkableTiles.Contains(Vector2Int.RoundToInt(p + new Vector2(-radius, radius)));
        }

        public Vector2 Move(Vector2 p, Vector2 delta)
        {
            // Moving one axis at a time lets the player slide along walls.
            Vector2 nextPosition = p + new Vector2(delta.x, 0);
            if (Walkable(nextPosition))
            {
                p = nextPosition;
            }

            nextPosition = p + new Vector2(0, delta.y);
            if (Walkable(nextPosition))
            {
                p = nextPosition;
            }

            return p;
        }

        public void Begin()
        {
            started = true;
            dialog = false;
            ProjectAudio.Instance?.PlayConfirm();
            Notify("Find three sigils in the west, east, and north chambers.", 6);
        }

        void Update()
        {
            if (!hero)
            {
                return;
            }

            float deltaTime = Mathf.Min(Time.deltaTime, .05f);
            clock += deltaTime;
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame && started && !won && !dead)
            {
                if (dialog)
                {
                    dialog = false;
                }
                else
                {
                    paused = !paused;
                }
            }

            if (!started)
            {
                if (keyboard != null && keyboard.enterKey.wasPressedThisFrame)
                {
                    Begin();
                }

                AnimateWorld();
                return;
            }

            if (dead || won || paused || dialog)
            {
                AnimateWorld();
                return;
            }

            attackCooldown -= deltaTime;
            dashCooldown -= deltaTime;
            spellCooldown -= deltaTime;
            invulnerable -= deltaTime;
            slashTime -= deltaTime;
            dashTime -= deltaTime;
            messageTime -= deltaTime;
            Vector2 input = Vector2.zero;
            if (keyboard != null)
            {
                input.x = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1 : 0) - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1 : 0);
                input.y = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1 : 0) - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1 : 0);
            }

            input = Vector2.ClampMagnitude(input, 1);
            if (input.sqrMagnitude > .01f)
            {
                facing = input.normalized;
            }

            if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame && dashCooldown <= 0)
            {
                dashCooldown = 1.15f;
                dashTime = .16f;
                invulnerable = .25f;
                dashDirection = facing;
                ProjectAudio.Instance?.PlaySwordDraw();
            }

            hero.position = Move(hero.position, (dashTime > 0 ? dashDirection * 12 : input * 4.3f) * deltaTime);
            ProjectAudio.Instance?.TryPlayFootstep(input.magnitude + (dashTime > 0 ? 1f : 0f));
            if (keyboard != null && (keyboard.jKey.isPressed || (Mouse.current != null && Mouse.current.leftButton.isPressed)))
            {
                Attack();
            }

            if (keyboard != null && keyboard.qKey.wasPressedThisFrame)
            {
                Cast();
            }

            if (keyboard != null && keyboard.hKey.wasPressedThisFrame)
            {
                Heal();
            }

            if (keyboard != null && keyboard.eKey.wasPressedThisFrame)
            {
                Interact();
            }

            heroArt.flipX = facing.x < -.1f;
            heroArt.color = invulnerable > 0 ? new Color(1, .75f, .4f) : Color.white;
            hero.localScale = new Vector3(1.25f, 1.25f + Mathf.Sin(clock * 16) * .04f * input.magnitude, 1);
            sword.localPosition = (Vector3)(facing * .58f);
            sword.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg - 90 + (slashTime > 0 ? Mathf.Sin(slashTime * 28) * 65 : 0));
            for (int i = foes.Count - 1; i >= 0; i--)
            {
                UpdateFoe(foes[i], deltaTime);
            }

            for (int i = pickups.Count - 1; i >= 0; i--)
            {
                var p = pickups[i];
                p.body.position = p.position + Vector2.up * Mathf.Sin(clock * 3 + i) * .07f;
                if (p.type < 2 && Vector2.Distance(hero.position, p.position) < .8f)
                {
                    if (p.type == 0)
                    {
                        gold += 5;
                        ShowFloatingText(p.position, "+5 gold", amber);
                    }
                    else
                    {
                        potions++;
                        ShowFloatingText(p.position, "+potion", teal);
                    }

                    ProjectAudio.Instance?.PlayCoin();
                    Destroy(p.body.gameObject);
                    pickups.RemoveAt(i);
                }
            }

            for (int i = floaters.Count - 1; i >= 0; i--)
            {
                floaters[i].life -= deltaTime;
                floaters[i].pos += Vector2.up * deltaTime * .6f;
                if (floaters[i].life <= 0)
                {
                    floaters.RemoveAt(i);
                }
            }

            AnimateWorld();
        }

        void LateUpdate()
        {
            if (!hero || !mainCamera)
            {
                return;
            }

            var target = new Vector3(hero.position.x, hero.position.y + .4f, -10);
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, target, 1 - Mathf.Exp(-8 * Time.deltaTime));
            if (shake > 0)
            {
                shake -= Time.deltaTime;
                mainCamera.transform.position += new Vector3(Mathf.Sin(clock * 93), Mathf.Cos(clock * 71), 0) * shake * .18f;
            }
        }

        void AnimateWorld()
        {
            for (int i = 0; i < embers.Count; i++)
            {
                embers[i].localScale = Vector3.one * (.95f + Mathf.Sin(clock * 7 + i) * .07f);
            }
        }

        void UpdateFoe(Foe f, float deltaTime)
        {
            if (!f.body)
            {
                return;
            }

            if (f.boss && sigils < 3)
            {
                return;
            }

            float dist = Vector2.Distance(hero.position, f.body.position);
            if (dist > 8)
            {
                return;
            }

            if (f.boss && !bossAwake)
            {
                bossAwake = true;
                Notify("The Hollow King awakens. Dodge the red wind-up!", 5);
            }

            f.cooldown -= deltaTime;
            // Give the player a short warning before the enemy deals damage.
            if (f.windup > 0)
            {
                f.windup -= deltaTime;
                f.art.color = Color.Lerp(Color.white, new Color(1, .22f, .2f), .65f + Mathf.Sin(clock * 24) * .3f);
                if (f.windup <= 0)
                {
                    if (dist < (f.boss ? 2.3f : 1.3f))
                    {
                        Hurt(f.boss ? 28 : 12);
                    }

                    f.cooldown = f.boss ? 1.5f : 1.15f;
                    f.art.color = Color.white;
                }

                return;
            }

            if (dist < (f.boss ? 1.9f : 1.0f) && f.cooldown <= 0)
            {
                f.windup = f.boss ? .8f : .5f;
                return;
            }

            if (dist > .85f)
            {
                Vector2 direction = ((Vector2)hero.position - (Vector2)f.body.position).normalized;
                f.body.position = Move(f.body.position, direction * f.speed * deltaTime);
                f.art.flipX = direction.x < 0;
            }
        }

        public void Attack()
        {
            if (attackCooldown > 0 || dead || won)
            {
                return;
            }

            attackCooldown = .36f;
            slashTime = .2f;
            ProjectAudio.Instance?.PlaySword();
            bool struck = false;
            for (int i = foes.Count - 1; i >= 0; i--)
            {
                var f = foes[i];
                Vector2 d = f.body.position - hero.position;
                if (d.magnitude < 1.95f && Vector2.Dot(d.normalized, facing) > -.15f && (!f.boss || sigils == 3))
                {
                    Damage(f, 18 + level * 4 + upgrade * 10);
                    struck = true;
                }
            }

            if (struck)
            {
                shake = .2f;
            }
        }

        public void Cast()
        {
            if (spellCooldown > 0)
            {
                return;
            }

            spellCooldown = 5;
            ProjectAudio.Instance?.PlaySpell();
            Notify("EMBER NOVA", 1);
            for (int i = foes.Count - 1; i >= 0; i--)
            {
                var f = foes[i];
                if (Vector2.Distance(hero.position, f.body.position) < 3.5f && (!f.boss || sigils == 3))
                {
                    Damage(f, 32 + level * 6);
                }
            }

            CreateNovaRing(hero.position, 3.5f);
        }

        void CreateNovaRing(Vector2 p, float radius)
        {
            for (int i = 0; i < 24; i++)
            {
                float a = i * Mathf.PI * 2 / 24;
                var g = CreateSpriteObject("Nova spark", 107, p + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius, 40, .4f);
                g.transform.SetParent(effectRoot, true);
                Destroy(g, .22f);
            }
        }

        public void Damage(Foe f, float amount)
        {
            f.hp -= amount;
            ProjectAudio.Instance?.PlayHit();
            ShowFloatingText(f.body.position, amount.ToString(), amber);
            f.body.position = Move(f.body.position, ((Vector2)f.body.position - (Vector2)hero.position).normalized * .3f);
            if (f.hp > 0)
            {
                return;
            }

            var p = (Vector2)f.body.position;
            bool boss = f.boss;
            Destroy(f.body.gameObject);
            foes.Remove(f);
            kills++;
            AddXP(boss ? 100 : 25);
            AddPickup(p, 0);
            if (kills % 3 == 0)
            {
                AddPickup(p + Vector2.right * .4f, 1);
            }

            if (boss)
            {
                won = true;
                ProjectAudio.Instance?.PlayConfirm();
                PlayerPrefs.SetInt("Emberfall.BestLevel", Mathf.Max(level, PlayerPrefs.GetInt("Emberfall.BestLevel", 1)));
                PlayerPrefs.Save();
            }
        }

        public void AddXP(int amount)
        {
            xp += amount;
            while (xp >= level * 60)
            {
                xp -= level * 60;
                level++;
                hp = MaxHP;
                Notify("LEVEL " + level + "  /  Health restored. Blade strengthened.", 4);
                ProjectAudio.Instance?.PlayConfirm();
            }
        }

        public void Hurt(float amount)
        {
            if (invulnerable > 0)
            {
                return;
            }

            hp = Mathf.Max(0, hp - amount);
            invulnerable = .65f;
            shake = .3f;
            ProjectAudio.Instance?.PlayError();
            ShowFloatingText(hero.position, "-" + amount, new Color(1, .35f, .35f));
            if (hp <= 0)
            {
                dead = true;
            }
        }

        public void Heal()
        {
            if (potions == 0)
            {
                ProjectAudio.Instance?.PlayError();
                Notify("No potions. Look for supply chests.", 2);
                return;
            }

            if (hp >= MaxHP)
            {
                ProjectAudio.Instance?.PlayError();
                Notify("Health is already full.", 2);
                return;
            }

            potions--;
            hp = Mathf.Min(MaxHP, hp + 55);
            ShowFloatingText(hero.position, "+55 health", teal);
            ProjectAudio.Instance?.PlayConfirm();
        }

        public void Interact()
        {
            if (Vector2.Distance(hero.position, new Vector2(-2, 1.5f)) < 2)
            {
                ProjectAudio.Instance?.PlayClick();
                dialog = true;
                dialogText = "The Hollow King stole our flame. Three sigils bind his throne.\n\nClear the chambers to the WEST, EAST, and NORTH. Claim their sigils, then travel farther north.\n\nBring me 20 gold and I will temper your blade.";
                return;
            }

            if (Vector2.Distance(hero.position, new Vector2(2, 1.5f)) < 1.8f)
            {
                hp = MaxHP;
                ProjectAudio.Instance?.PlayConfirm();
                Notify("The last hearth restores your health.", 3);
                return;
            }

            for (int i = pickups.Count - 1; i >= 0; i--)
            {
                var p = pickups[i];
                if (p.type < 2 || Vector2.Distance(hero.position, p.position) > 1.5f)
                {
                    continue;
                }

                if (p.type == 2)
                {
                    if (foes.Exists(f => f.room == p.room))
                    {
                        ProjectAudio.Instance?.PlayError();
                        Notify("Defeat this chamber's guardians first.", 3);
                        return;
                    }

                    collected[p.room - 1] = true;
                    sigils++;
                    ProjectAudio.Instance?.PlayConfirm();
                    Notify(sigils == 3 ? "All sigils claimed. The northern throne is unsealed!" : "Ancient sigil claimed  /  " + sigils + " of 3", 4);
                }
                else
                {
                    gold += 10;
                    potions++;
                    ProjectAudio.Instance?.PlayChest();
                    Notify("Supply chest  /  +10 gold and a healing potion", 3);
                }

                Destroy(p.body.gameObject);
                pickups.RemoveAt(i);
                return;
            }
        }

        public bool BuyUpgrade()
        {
            if (gold < 20)
            {
                return false;
            }

            gold -= 20;
            upgrade++;
            return true;
        }

        public void Restart()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }

        void Notify(string s, float duration)
        {
            message = s;
            messageTime = duration;
        }

        void ShowFloatingText(Vector2 pos, string text, Color color)
        {
            floaters.Add(new Floater { pos = pos, text = text, color = color });
        }
    }
}
