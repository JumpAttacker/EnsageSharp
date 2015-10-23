using System;
using System.Collections.Generic;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using SharpDX;

namespace SpellRanger
{
    internal class Program
    {
        private const string Ver = "1.1";
        private static bool _loaded;
        private static bool _blink;
        private static bool _leftMouseIsPress;
        private static readonly SpellSys[] Spell=new SpellSys[6];
        private static readonly Dictionary<int,ParticleEffect> Effect=new Dictionary<int, ParticleEffect>(); 


        private static void Main(string[] args)
        {
            _loaded = false;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnWndProc += Game_OnWndProc;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var me = ObjectMgr.LocalHero;
            if (!_loaded)
            {
                if (!Game.IsInGame || me == null)
                {
                    return;
                }
                _loaded = true;
                PrintSuccess("> Spell Ranger loaded! v" + Ver);
                Spell[0] = new SpellSys(me.Spellbook.Spell1, false);
                Spell[1] = new SpellSys(me.Spellbook.Spell2, false);
                Spell[2] = new SpellSys(me.Spellbook.Spell3, false);
                Spell[3] = new SpellSys(me.Spellbook.Spell4, false);
                Spell[4] = new SpellSys(me.Spellbook.Spell5, false);
                Spell[5] = new SpellSys(me.Spellbook.Spell6, false);
            }
            if (!Game.IsInGame || me == null)
            {
                _loaded = false;
                PrintInfo("> Spell Ranger unLoaded");
                return;
            }
            if (!Game.IsInGame || !_loaded) return;
            var start = new Vector2();
            for (var i = 0; i < 6; i++)
            {
                if (Spell[i].Spell == null) continue;
                start = new Vector2(100+i*50, 52);
                DrawButton(start, new Vector2(50, 50), ref Spell[i].Show, Spell[i].Spell.CastRange>0, new Color(100, 255, 0, 50),
                    new Color(100, 0, 0, 50));
                
                ParticleEffect effect;
                if (Spell[i].Show)
                {
                    if (Effect.TryGetValue(i, out effect)) continue;
                    effect = me.AddParticleEffect(@"particles\ui_mouseactions\range_display.vpcf");
                    effect.SetControlPoint(1, new Vector3(Spell[i].Spell.CastRange, 0, 0));
                    Effect.Add(i, effect);
                }
                else
                {
                    if (!Effect.TryGetValue(i, out effect)) continue;
                    effect.Dispose();
                    Effect.Remove(i);
                }
            }
            var blink = me.FindItem("item_blink");
            if (blink==null) return;
            DrawButton(start + new Vector2(70, 0), new Vector2(50, 50), ref _blink, true, new Color(100, 255, 0, 50),
                    new Color(100, 0, 0, 50));
            ParticleEffect eff;
            if (_blink)
            {
                if (Effect.TryGetValue(12, out eff)) return;
                eff = me.AddParticleEffect(@"particles\ui_mouseactions\range_display.vpcf");
                eff.SetControlPoint(1, new Vector3(1200, 0, 0));
                Effect.Add(12, eff);
            }
            else
            {
                if (!Effect.TryGetValue(12, out eff)) return;
                eff.Dispose();
                Effect.Remove(12);
            }
        }
        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (args.WParam != 1 || Game.IsChatOpen || !Utils.SleepCheck("clicker"))
            {
                _leftMouseIsPress = false;
                return;
            }
            _leftMouseIsPress = true;
        }
        #region printer
        private static void DrawButton(Vector2 a, Vector2 b,ref bool clicked, bool isActive, Color @on, Color off)
        {
            var isIn = Utils.IsUnderRectangle(Game.MouseScreenPosition,a.X,a.Y, b.X,b.Y);
            if (isActive)
            {
                if (_leftMouseIsPress && Utils.SleepCheck("ClickButtonCd") && isIn)
                {
                    clicked = !clicked;
                    Utils.Sleep(250, "ClickButtonCd");
                }
                var newColor = isIn
                    ? new Color((int)(clicked ? @on.R : off.R), clicked ? @on.G : off.G, clicked ? @on.B : off.B, 150)
                    : clicked ? @on : off;
                Drawing.DrawRect(a, b, newColor);
                Drawing.DrawRect(a, b, Color.Black,true);
            }
            else
            {
                Drawing.DrawRect(a, b, Color.Gray);
                Drawing.DrawRect(a, b, Color.Black, true);
            }
        }
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



