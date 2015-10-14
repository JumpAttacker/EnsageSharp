using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using SharpDX;

namespace Techies_Annihilation
{
    internal class Program
    {
        #region Members

        private static bool _loaded;
        private static Hero _me;
        private static Player _player;
        private static readonly Dictionary<Unit, ParticleEffect> Effects = new Dictionary<Unit, ParticleEffect>();
        private static readonly Dictionary<Unit, ParticleEffect> Visible = new Dictionary<Unit, ParticleEffect>();
        private static bool IsClose { get; set; }
        private static bool WithAll { get; set; }

        #endregion

        #region Methods

        #region Init

        private static void Main()
        {
            Game.OnUpdate += Game_OnUpdate;
            _loaded = false;
            Drawing.OnDraw += Drawing_OnDraw;
            //Drawing.OnEndScene += Drawing_OnEndScene;
            Game.OnWndProc += Game_OnWndProc;
        }

        #endregion

        #region Events

        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (args.WParam != 1 || Game.IsChatOpen || !Utils.SleepCheck("clicker"))
            {
                return;
            }
            if (!IsClose && CheckMouse(145, 380, 60, 50))
            {
                WithAll = !WithAll;
                Utils.Sleep(250, "clicker");
            }
            else if (IsClose ? CheckMouse(20, 380, 50, 50) : CheckMouse(20, 380, 120, 50))
            {
                IsClose = !IsClose;
                Utils.Sleep(250, "clicker");
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Game.IsInGame &&!_loaded)
                return;
            var player = ObjectMgr.LocalPlayer;
            if (player == null || player.Team == Team.Observer)
                return;

            /*var screenX = Drawing.Width / (float)1600 * (float)0.8;
            Drawing.DrawRect(new Vector2(10, 200), new Vector2((float)15 * screenX, (float)15 * screenX), Drawing.GetTexture(@"vgui\hud\minimap_creep.vmat"));
            Drawing.DrawRect(new Vector2(10, 300), new Vector2((float)15 * screenX, (float)15 * screenX), Drawing.GetTexture(@"vgui\hud\minimap_glow.vmat"));
             * vgui\dashboard\dash_button_back
             * 
             * */
            
            if (!IsClose)
            {
                Drawing.DrawRect(new Vector2(10, 200), new Vector2(200, 250), new ColorBGRA(128, 0, 128, 100));
                DrawEzLine(10, 250, 200, 250);
                Drawing.DrawText("Damage Helper", new Vector2(20, 210), new Vector2(30, 30), Color.White,
                    FontFlags.Custom);
                var enemies =
                    ObjectMgr.GetEntities<Hero>()
                        .Where(
                            x =>
                                x.Team != player.Team && !x.IsIllusion && x.ClassID != ClassID.CDOTA_Unit_Hero_Meepo)
                        .ToList();
                var counter = 0;
                var ultimate = _me.Spellbook.Spell6;
                float damage = 0;
                var data = ultimate.AbilityData.FirstOrDefault(x => x.Name == "damage");
                if (data != null)
                {
                    damage = data.GetValue(ultimate.Level - 1);
                    damage += _me.FindItem("item_ultimate_scepter") != null
                        ? 150
                        : 0;
                }
                Drawing.DrawRect(new Vector2(20, 380), new Vector2(120, 50),
                    CheckMouse(20, 380, 120, 50) ? new ColorBGRA(255, 255, 0, 100) : new ColorBGRA(255, 0, 128, 100));
                Drawing.DrawText("Open / Hide", new Vector2(30, 390), new Vector2(20, 30), Color.White,
                    FontFlags.Custom);

                Drawing.DrawRect(new Vector2(145, 380), new Vector2(60, 50),
                    CheckMouse(145, 380, 60, 50) ? new ColorBGRA(255, 255, 0, 100) : WithAll ? new ColorBGRA(255, 0, 128, 100) : new ColorBGRA(255, 0, 255, 100));
                Drawing.DrawText(WithAll?"AllHero":"WithVis", new Vector2(150, 390), new Vector2(20, 30), Color.White,
                    FontFlags.Custom);
                if (!WithAll)
                {
                    foreach (var v in enemies)
                    {
                        Drawing.DrawText(
                            string.Format("  {0}: {1}/{2}", GetNameOfHero(v),
                                Math.Abs(damage) <= 0 ? 0 : GetCount(v, v.Health, damage),
                                Math.Abs(damage) <= 0 ? 0 : GetCount(v, v.MaximumHealth, damage)),
                            new Vector2(10, 250 + 25*counter++),
                            new Vector2(20, 30),
                            Color.YellowGreen,
                            FontFlags.Custom);
                        /*
                    Vector2 screenPos;
                    if (!Drawing.WorldToScreen(v.Position, out screenPos)) continue;
                    PrintError(String.Format("{0}|{1}",screenPos.X, screenPos.Y));
                    var text = string.Format("{0} - {1}", counter, Game.Localize(v.Name));
                    var textSize = Drawing.MeasureText(text, "Arial", Drawing.DefaultTextSize, FontFlags.DropShadow);
                    Drawing.DrawText(text, new Vector2(screenPos.X - textSize.X / 2, screenPos.Y - textSize.Y / 2),
                        Color.White, FontFlags.DropShadow);
                    */
                    }
                }
                else
                {
                    for (uint i = 0; i <= 10; i++)
                    {
                        try
                        {
                            var v = ObjectMgr.GetPlayerById(i).Hero;
                            if (v.Team != _me.Team && !Equals(v, _me))
                            {
                                Drawing.DrawText(
                                    string.Format("  {0}: {1}/{2}", GetNameOfHero(v),
                                        Math.Abs(damage) <= 0 ? 0 : GetCount(v, v.Health, damage),
                                        Math.Abs(damage) <= 0 ? 0 : GetCount(v, v.MaximumHealth, damage)),
                                    new Vector2(10, 250 + 25 * counter++),
                                    new Vector2(20, 20),
                                    Color.YellowGreen,
                                    FontFlags.Custom);
                                //Drawing.WorldToScreen()
                            }

                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }
            }
            else
            {
                Drawing.DrawRect(new Vector2(20, 380), new Vector2(50, 50),
                    CheckMouse(20, 380, 50, 50) ? new ColorBGRA(255, 255, 0, 100) : new ColorBGRA(255, 0, 128, 100));
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (!_loaded)
            {
                _me = ObjectMgr.LocalHero;
                _player = ObjectMgr.LocalPlayer;
                if (!Game.IsInGame || _me == null || _me.ClassID != ClassID.CDOTA_Unit_Hero_Techies)
                {
                    return;
                }
                _loaded = true;
                PrintSuccess("> Techies Annihilation loaded!");
            }

            if (!Game.IsInGame || _me == null || _me.ClassID != ClassID.CDOTA_Unit_Hero_Techies)
            {
                _loaded = false;
                PrintInfo("> Techies Annihilation Unloaded!");
                return;
            }

            if (Game.IsPaused)
            {
                return;
            }

            if (_player == null || _player.Team == Team.Observer)
                return;
            var bombs = ObjectMgr.GetEntities<Unit>()
                .Where(
                    x =>
                        x.ClassID == ClassID.CDOTA_NPC_TechiesMines && x.Team == _player.Team);
            foreach (var s in bombs)
            {
                HandleEffect(s, s.Spellbook.Spell1 != null);
            }
        }

        #endregion

        #endregion

        #region Helpers

        private static void HandleEffect(Unit unit, bool isRange)
        {
            ParticleEffect effect;
            if (unit.IsAlive)
            {
                if (isRange)
                {
                    if (!Effects.TryGetValue(unit, out effect))
                    {
                        effect = unit.AddParticleEffect(@"particles\ui_mouseactions\range_display.vpcf");
                        effect.SetControlPoint(1, new Vector3(425, 0, 0));
                        Effects.Add(unit, effect);
                    }
                }
                if (unit.IsVisibleToEnemies)
                {
                    if (Visible.TryGetValue(unit, out effect)) return;
                    effect = unit.AddParticleEffect("particles/items_fx/aura_shivas.vpcf");
                    Visible.Add(unit, effect);
                }
                else
                {
                    if (!Visible.TryGetValue(unit, out effect)) return;
                    effect.Dispose();
                    Visible.Remove(unit);
                }
            }
            else
            {
                if (isRange)
                {
                    if (!Effects.TryGetValue(unit, out effect)) return;
                    effect.Dispose();
                    Effects.Remove(unit);
                }
                if (!Visible.TryGetValue(unit, out effect)) return;
                effect.Dispose();
                Visible.Remove(unit);
            }
        }

        private static string GetNameOfHero(Entity hero)
        {
            return hero.NetworkName.Substring(hero.NetworkName.LastIndexOf('_') + 1);
        }

        private static int GetCount(Unit v, uint health, float damage)
        {
            var n = 0;
            float dmg = 0;
            do
            {
                n++;
                dmg += damage;
            } while (health - v.DamageTaken(dmg, DamageType.Magical, _me, false) > 0 && n < 30);
            return n;
        }

        private static void PrintInfo(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.White, arguments);
        }

        private static void PrintSuccess(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.Green, arguments);
        }

        private static void PrintError(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.Red, arguments);
        }

        private static void PrintEncolored(string text, ConsoleColor color, params object[] arguments)
        {
            var clr = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text, arguments);
            Console.ForegroundColor = clr;
        }

        private static void DrawEzLine(float x, float y, float x1, float y1)
        {
            Drawing.DrawLine(new Vector2(x, y), new Vector2(x1, y1), Color.Black);
        }

        private static bool CheckMouse(float x, float y, float sizeX, float sizeY)
        {
            var mousePos = Game.MouseScreenPosition;
            return mousePos.X >= x && mousePos.X <= x + sizeX && mousePos.Y >= y && mousePos.Y <= y + sizeY;
        }

        #endregion

    }
}
