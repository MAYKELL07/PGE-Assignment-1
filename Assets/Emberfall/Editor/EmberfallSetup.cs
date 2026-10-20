using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Emberfall
{
    public static class EmberfallSetup
    {
        [MenuItem("Tools/Testing/Run Gameplay Checks")]
        public static void Test()
        {
            var game = UnityEngine.Object.FindFirstObjectByType<EmberfallGame>();
            var report = new List<string>();
            void Check(bool condition, string description)
            {
                if (!condition) throw new InvalidOperationException(description);
                report.Add("PASS: " + description);
            }
            try
            {
                Check(EditorApplication.isPlaying && game && game.hero, "Adventure is running");
                Check(game.foes.Count == 13 && game.level == 1 && game.kills == 0, "Fresh authored encounter has 12 enemies and a boss");
                int savedSprites = System.Text.RegularExpressions.Regex.Matches(File.ReadAllText("Assets/Emberfall/Scenes/Emberfall.unity"), "^SpriteRenderer:", System.Text.RegularExpressions.RegexOptions.Multiline).Count;
                Check(savedSprites > 0 && game.GetComponentsInChildren<SpriteRenderer>().Length == savedSprites, "Saved level sprites load without duplicating the world");
                Check(game.Walkable(Vector2.zero) && !game.Walkable(new Vector2(100, 100)), "Dungeon walkability rejects out-of-bounds movement");
                Check(game.Move(Vector2.zero, new Vector2(0, -100)) == Vector2.zero, "Movement cannot leave the dungeon");
                var circle = Physics.CollisionAlgorithms2D.CircleCircle(Vector2.zero, 1f, new Vector2(1.5f, 0f), 1f);
                Check(circle.Intersects && Mathf.Approximately(circle.Penetration, .5f), "Circle collision penetration");
                var aabb = Physics.CollisionAlgorithms2D.AabbAabb(Vector2.zero, Vector2.one, new Vector2(1.5f, 0f), Vector2.one);
                Check(aabb.Intersects && Mathf.Approximately(aabb.Penetration, .5f), "AABB collision penetration");
                Check(Physics.CollisionAlgorithms2D.ObbObb(Vector2.zero, Vector2.one, 25f, new Vector2(1f, .2f), Vector2.one, -18f).Intersects, "SAT rotated-box intersection");
                var body = new Physics.EulerBody2D(Vector2.zero, 2f);
                body.AddForce(new Vector2(4f, 0f)); body.Integrate(.5f, 0f);
                Check(Mathf.Approximately(body.Acceleration.x, 2f) && Mathf.Approximately(body.Velocity.x, 1f) && Mathf.Approximately(body.Position.x, .5f), "Euler force/mass integration");
                game.Begin();
                var target = game.foes[0]; game.hero.position = target.body.position - Vector3.right;
                float before = target.hp; game.Attack();
                Check(target.hp < before, "Sword damages a nearby enemy");
                before = target.hp; game.Attack();
                Check(target.hp == before, "Sword cooldown prevents instant repeated hits");
                game.Cast(); Check(target.hp < before, "Nova damages nearby enemies");
                game.hero.position = new Vector2(-18, 2.5f); game.Interact();
                Check(game.sigils == 0, "Guarded sigil cannot be collected");
                game.hp = 40; int potions = game.potions; game.Heal();
                Check(game.hp == 95 && game.potions == potions - 1, "Potion restores health and consumes a charge");
                game.gold = 25;
                Check(game.BuyUpgrade() && game.gold == 5 && game.upgrade == 1 && !game.BuyUpgrade(), "Upgrade spends gold and rejects insufficient funds");
                game.AddXP(60); Check(game.level == 2 && game.hp == game.MaxHP, "Level-up restores health");
                foreach (var foe in game.foes.ToArray().Where(foe => !foe.boss)) game.Damage(foe, 999);
                Check(game.foes.Count == 1, "All chamber enemies can be defeated");
                foreach (var position in new[] {new Vector2(-18, 2.5f), new Vector2(18, 2.5f), new Vector2(0, 17.5f)})
                { game.hero.position = position; game.Interact(); }
                Check(game.sigils == 3, "All three cleared-room sigils can be collected");
                game.Damage(game.foes[0], 999); Check(game.won, "Boss defeat triggers victory");
                game.Hurt(999); Check(game.dead, "Zero health triggers defeat");
                game.Restart();
                double deadline = EditorApplication.timeSinceStartup + 2;
                void VerifyRestart()
                {
                    if (EditorApplication.timeSinceStartup < deadline) return;
                    EditorApplication.update -= VerifyRestart;
                    try
                    {
                        var reloaded = UnityEngine.Object.FindFirstObjectByType<EmberfallGame>();
                        Check(reloaded && reloaded.hero && reloaded.foes.Count == 13 && reloaded.sigils == 0 && reloaded.hp == 100 && !reloaded.won && !reloaded.dead, "Retry reloads the authored scene with clean state");
                        report.Add("ALL GAMEPLAY CHECKS PASSED");
                    }
                    catch (Exception exception) { report.Add("FAILED: " + exception); }
                    WriteReport(report);
                }
                EditorApplication.update += VerifyRestart;
            }
            catch (Exception exception) { report.Add("FAILED: " + exception); WriteReport(report); }
        }

        static void WriteReport(List<string> report)
        {
            string text = string.Join(Environment.NewLine, report);
            File.WriteAllText("Temp/Emberfall-test-results.txt", text);
            if (report.Any(line => line.StartsWith("FAILED:"))) Debug.LogError(text);
            else Debug.Log(text);
        }
    }
}
