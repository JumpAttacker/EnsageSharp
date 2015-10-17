using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using SharpDX;
using SharpDX.Direct3D9;

namespace Overlay_information
{
    internal class Program
    {
        #region Members

        private static bool _loaded;
        private static Hero _me;
        private static Player _player;
        private const float Ver =  1.0f;
        
        #endregion
        #region Methods

        #region Init

        private static void Main()
        {
            Game.OnUpdate += Game_OnUpdate;
            _loaded = false;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        #endregion

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Game.IsInGame || _me == null) return;
            uint i;
            for (i = 0; i < 10; i++)
                {
                    try
                    {
                        var v = ObjectMgr.GetPlayerById(i).Hero;
                        if (v == null || Equals(v, _me)) continue;
                        Vector2 screenPos;

                        if (!Drawing.WorldToScreen(v.Position, out screenPos))
                            continue;
                        
                        var start = screenPos + new Vector2(-75, 20);
                        var spells = new Ability[7];
                        
                        try{spells[1] = v.Spellbook.Spell1;} 
                        catch{}
                        try { spells[2] = v.Spellbook.Spell2; }
                        catch { }
                        try { spells[3] = v.Spellbook.Spell3; }
                        catch { }
                        try { spells[4] = v.Spellbook.Spell4; }
                        catch { }
                        try { spells[5] = v.Spellbook.Spell5; }
                        catch { }
                        try { spells[6] = v.Spellbook.Spell6; }
                        catch { }
                        
                        for (var g = 1; g <= 6; g++)
                        {
                            if (spells[g]==null) continue;
                            Drawing.DrawRect(start + new Vector2(g * 20 - 5, 0), new Vector2(20, 6),
                                new ColorBGRA(0, 0, 0, 100), true);
                            if (spells[g].Level==0) continue;
                            for (var lvl = 1; lvl <= spells[g].Level; lvl++)
                            {
                                Drawing.DrawRect(start + new Vector2(g * 20 - 5 + 3 * lvl, 2), new Vector2(2, 2),
                                    new ColorBGRA(255, 255, 0, 255),true);
                            }
                        }
                        
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
        }

        #endregion

        private static void Game_OnUpdate(EventArgs args)
        {
            if (!_loaded)
            {
                _me = ObjectMgr.LocalHero;
                _player = ObjectMgr.LocalPlayer;
                if (!Game.IsInGame || _me == null)
                {
                    return;
                }
                _loaded = true;
                PrintSuccess("> OverlayInformation loaded! v"+Ver);
            }
            if (!Game.IsInGame || _me == null)
            {
                _loaded = false;
                PrintInfo("> OverlayInformation unLoaded");
                return;
            }
        }

        #region Helpers

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

        private static bool CheckMouse(float x, float y, float sizeX, float sizeY)
        {
            var mousePos = Game.MouseScreenPosition;
            return mousePos.X >= x && mousePos.X <= x + sizeX && mousePos.Y >= y && mousePos.Y <= y + sizeY;
        }

        #endregion
    }
}
