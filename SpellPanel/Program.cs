using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common;
using SharpDX;

namespace SpellPanel
{
    internal class Program
    {
        private const string Ver = "1.1";
        private const string Spellpanel = "SpellPanel";
        private static bool _loaded;

        private static Hero[] _heroes = new Hero[0];

        private static void Main(string[] args)
        {
            _loaded = false;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (!Utils.SleepCheck(Spellpanel))
                return;

            _heroes = new Hero[0];

            var me = ObjectMgr.LocalHero;
            if (!_loaded)
            {
                me = ObjectMgr.LocalHero;
                if (!Game.IsInGame || me == null)
                {
                    return;
                }
                _loaded = true;
                PrintSuccess("> OverlayInformation > SpellPanel loaded! v" + Ver);
            }
            if (!Game.IsInGame || me == null)
            {
                _loaded = false;
                PrintInfo("> OverlayInformation > SpellPanel unLoaded");
                return;
            }
            if (!Game.IsInGame || !_loaded) return;

            _heroes = Enumerable.Range(0, 10)
                .Select(i => ObjectMgr.GetPlayerById((uint)i).Hero)
                .Where(v => !Equals(v, me))
                .ToArray();

            Utils.Sleep(200, Spellpanel);
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (var v in _heroes)
            {
                try
                {
                    Vector2 screenPos;

                    #region GettingSPells
                    var spells = new Ability[7];
                    spells[1] = v.Spellbook.Spell1;
                    spells[2] = v.Spellbook.Spell2;
                    spells[3] = v.Spellbook.Spell3;
                    spells[4] = v.Spellbook.Spell4;
                    spells[5] = v.Spellbook.Spell5;
                    spells[6] = v.Spellbook.Spell6;
                    #endregion

                    if (!Drawing.WorldToScreen(v.Position, out screenPos)) continue;
                    var start = screenPos + new Vector2(-65, 20);
                    for (var g = 1; g <= 6; g++)
                    {
                        if (spells[g] == null) continue;

                        var cd = spells[g].Cooldown;
                        Drawing.DrawRect(start + new Vector2(g * 20 - 5, 0), new Vector2(20, (int)cd == 0 ? 6 : 20),
                            new ColorBGRA(0, 0, 0, 100), true);

                        if (spells[g].ManaCost > v.Mana)
                        {
                            Drawing.DrawRect(start + new Vector2(g * 20 - 5, 0),
                                new Vector2(20, (int)cd == 0 ? 6 : 20),
                                new ColorBGRA(0, 0, 150, 150));
                        }
                        if (cd > 0)
                        {
                            var text = string.Format("{0:0.#}", cd);
                            var textSize = Drawing.MeasureText(text, "Arial", new Vector2(10, 200),
                                FontFlags.None);
                            var textPos = (start + new Vector2(g * 20 - 5, 0) +
                                           new Vector2(10 - textSize.X / 2, -textSize.Y / 2 + 12));
                            Drawing.DrawText(text, textPos, new Vector2(10, 150), Color.White,
                                FontFlags.AntiAlias | FontFlags.DropShadow);
                        }
                        if (spells[g].Level == 0) continue;
                        for (var lvl = 1; lvl <= spells[g].Level; lvl++)
                        {
                            Drawing.DrawRect(start + new Vector2(g * 20 - 5 + 3 * lvl, 2), new Vector2(2, 2),
                                new ColorBGRA(255, 255, 0, 255), true);
                        }

                    }
                }
                catch
                {
                    // ignored
                }
            }
        }

        #region printer

        private static void PrintInfo(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.White, arguments);
        }

        private static void PrintSuccess(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.Green, arguments);
        }

        // ReSharper disable once UnusedMember.Local
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

        #endregion

    }
}
