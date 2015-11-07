using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using SharpDX;

namespace Legion_Annihilation
{
    internal class Program
    {
        #region Members
        //============================================================
        private static bool _loaded;
        private const string Ver = "0.1";
        private const int WmKeyup = 0x0101;
        private static bool _leftMouseIsPress;
        private static bool _showMenu = true;
        private static bool _inAction;
        private static Vector2 _sizer = new Vector2(265, 300);
        //============================================================
        private static ulong _myKey = 'F';
        private static bool _timetochange;
        private static bool _shoulUseBkb;
        private static bool _shoulUseHeal;
        private static bool _buffme = true;
        private static bool _debuffenemy = true;
        //============================================================
        #endregion

        private static void Main()
        {
            Game.OnUpdate += Game_OnUpdate;
            _loaded = false;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnWndProc += Game_OnWndProc;
        }
        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (Game.IsChatOpen)
                return;
            if (_timetochange && args.Msg == WmKeyup && args.WParam >= 0x41 && args.WParam <= 0x5A)
            {
                _timetochange = false;
                _myKey = args.WParam;
                return;
            }
            if (args.WParam == _myKey)
            {
                _inAction = args.Msg != WmKeyup;
                Game.ExecuteCommand(string.Format("dota_player_units_auto_attack_after_spell {0}", _inAction ? 0 : 1));
            }
            if (args.WParam != 1 || !Utils.SleepCheck("clicker"))
            {
                _leftMouseIsPress = false;
                return;
            }
            _leftMouseIsPress = true;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var player = ObjectMgr.LocalPlayer;
            if (player == null || player.Team == Team.Observer || !_loaded)
            {
                return;
            }
            if (ObjectMgr.LocalHero.ClassID != ClassID.CDOTA_Unit_Hero_Legion_Commander) return;
            var startPos = new Vector2(50, 200);
            var maxSize = new Vector2(120, 300);
            if (_showMenu)
            {
                _sizer.X += 4;
                _sizer.Y += 4;
                _sizer.X = Math.Min(_sizer.X, maxSize.X);
                _sizer.Y = Math.Min(_sizer.Y, maxSize.Y);

                Drawing.DrawRect(startPos, _sizer, new Color(0, 155, 255, 100));
                Drawing.DrawRect(startPos, _sizer, new Color(0, 0, 0, 255), true);
                Drawing.DrawRect(startPos + new Vector2(-5, -5), _sizer + new Vector2(10, 10),
                    new Color(0, 0, 0, 255), true);
                DrawButton(startPos + new Vector2(_sizer.X - 20, -20), 20, 20, ref _showMenu, true, Color.Gray,
                    Color.Gray);
                if (!Equals(_sizer, maxSize)) return;

                DrawButton(startPos + new Vector2(10, 10), 100, 20, ref _shoulUseHeal, true,
                    new Color(0, 200, 150),
                    new Color(200, 0, 0, 100), "Use heal");
                DrawButton(startPos + new Vector2(10, 40), 100, 20, ref _shoulUseBkb, true,
                    new Color(0, 200, 150),
                    new Color(200, 0, 0, 100), "Use bkb");
                DrawButton(startPos + new Vector2(10, 70), 100, 20, ref _buffme, true,
                    new Color(0, 200, 150),
                    new Color(200, 0, 0, 100), "Buff me");
                DrawButton(startPos + new Vector2(10, 100), 100, 20, ref _debuffenemy, true,
                    new Color(0, 200, 150),
                    new Color(200, 0, 0, 100), "DeBuff enemy");
                DrawButton(startPos + new Vector2(10, _sizer.Y - 70), 100, 20, ref _timetochange, true,
                    new Color(0, 200, 150),
                    new Color(200, 0, 0, 100), "Change Hotkey");

                Drawing.DrawText(
                    string.Format("Status: [{0}]", _inAction ? "ON" : "OFF"),
                    startPos + new Vector2(10, 280), Color.White,
                    FontFlags.AntiAlias | FontFlags.DropShadow);
                Drawing.DrawText(string.Format("ComboKey {0}", (char)_myKey),
                    startPos + new Vector2(10, 265), Color.White,
                    FontFlags.AntiAlias | FontFlags.DropShadow);

            }
            else
            {
                _sizer.X -= 4;
                _sizer.Y -= 4;
                _sizer.X = Math.Max(_sizer.X, 20);
                _sizer.Y = Math.Max(_sizer.Y, 0);
                Drawing.DrawRect(startPos, _sizer, new Color(0, 0, 0, 255), true);
                DrawButton(startPos + new Vector2(_sizer.X - 20, -20), 20, 20, ref _showMenu, true, Color.Gray,
                    Color.Gray);
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            #region Init

            var me = ObjectMgr.LocalHero;
            if (!_loaded)
            {
                if (!Game.IsInGame || me == null || me.ClassID != ClassID.CDOTA_Unit_Hero_Legion_Commander)
                {
                    return;
                }
                _loaded = true;

                PrintSuccess(string.Format("> Legion Annihilation Loaded v{0}", Ver));
            }

            if (!Game.IsInGame || me == null)
            {
                _loaded = false;
                PrintInfo("> Legion Annihilation unLoaded");
                return;
            }

            if (Game.IsPaused)
            {
                return;
            }
            #endregion

            #region Lets combo

            var target = ClosestToMouse(me);
            if (_inAction && target != null && target.IsAlive)
            {
                ComboInAction(me, target);
            }
            #endregion
        }

        private static void ComboInAction(Hero me, Hero target)
        {
            if (!Utils.SleepCheck("nextAction")) return;
            var duel = me.Spellbook.Spell4;

            if (duel==null) return;
            if (!duel.CanBeCasted(target)) return;

            //var steal = me.Spellbook.Spell1;
            var heal = me.Spellbook.Spell2;
            
            var dagger = me.FindItem("item_blink");
            var neededMana = me.Mana-duel.ManaCost;

            var allitems = me.Inventory.Items.Where(x => x.CanBeCasted() && x.ManaCost <= neededMana);
            var dpActivated =
                target.Modifiers.Any(
                    x => x.Name == "modifier_slark_dark_pact" || x.Name == "modifier_slark_dark_pact_pulses");
            var enumerable = allitems as Item[] ?? allitems.ToArray();
            var itemOnTarget =
                enumerable.FirstOrDefault(
                    x =>
                        x.Name == "item_abyssal_blade" || x.Name == "item_orchid" ||
                        x.Name == "item_heavens_halberd" || x.Name == "item_sheepstick" ||
                        x.Name == "item_urn_of_shadows" || x.Name == "item_medallion_of_courage" ||
                        x.Name == "item_solar_crest");
            var itemWithOutTarget = enumerable.FirstOrDefault(
                    x =>
                        x.Name == "item_soul_ring" || (x.Name == "item_armlet" && !x.IsToggled) ||
                        x.Name == "item_mask_of_madness" || x.Name == "item_satanic" ||
                        x.Name == "item_blade_mail");
            var itemOnMySelf = enumerable.FirstOrDefault(
                x =>
                    x.Name == "item_mjollnir");
            Item bkb = null;
            if (_shoulUseBkb)
            {
                bkb = me.FindItem("item_black_king_bar");
            }
            var distance = me.Distance2D(target);
            if (dagger != null && dagger.CanBeCasted())
            {
                if (distance <= 1150)
                {
                    if (!me.IsMagicImmune() && heal.CanBeCasted() && heal.ManaCost <= neededMana && _shoulUseHeal && distance >= 500)
                    {
                        heal.UseAbility(me);
                        Utils.Sleep(500, "nextAction");
                        return;
                    }
                    dagger.UseAbility(target.Position);
                    Utils.Sleep(50, "nextAction");
                    return;
                }
                me.Move(target.Position);
                Utils.Sleep(200, "nextAction");
                return;
            }
            
            if (itemOnMySelf != null && _buffme)
            {
                itemOnMySelf.UseAbility(me);
                Utils.Sleep(50, "nextAction");
                return;
            }
            if (itemWithOutTarget != null && _buffme)
            {
                if (itemWithOutTarget.Name == "item_armlet")
                {
                    itemWithOutTarget.ToggleAbility();
                    Utils.Sleep(50, "nextAction");
                    return;
                }
                itemWithOutTarget.UseAbility();
                Utils.Sleep(50, "nextAction");
                return;
            }
            if (itemOnTarget != null && !dpActivated && _debuffenemy)
            {
                itemOnTarget.UseAbility(target);
                Utils.Sleep(50, "nextAction");
                return;
            }
            if (_shoulUseBkb && bkb != null)
            {
                bkb.UseAbility();
            }
            duel.UseAbility(target);
            Utils.Sleep(300, "nextAction");
        }


        public static Hero ClosestToMouse(Hero source, float range = 600)
        {
            var mousePosition = Game.MousePosition;
            var enemyHeroes =
                ObjectMgr.GetEntities<Hero>()
                    .Where(
                        x =>
                            x.Team == source.GetEnemyTeam() && !x.IsIllusion && x.IsAlive && x.IsVisible
                            && x.Distance2D(mousePosition) <= range && !x.IsMagicImmune());
            Hero[] closestHero = { null };
            foreach (var enemyHero in enemyHeroes.Where(enemyHero => closestHero[0] == null || closestHero[0].Distance2D(mousePosition) > enemyHero.Distance2D(mousePosition)))
            {
                closestHero[0] = enemyHero;
            }
            return closestHero[0];
        }

        #region Helpers
        private static void DrawButton(Vector2 a, float w, float h, ref bool clicked, bool isActive, Color @on, Color off, string drawOnButtonText = "")
        {
            var isIn = Utils.IsUnderRectangle(Game.MouseScreenPosition, a.X, a.Y, w, h);
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
                Drawing.DrawRect(a, new Vector2(w, h), newColor);
                Drawing.DrawRect(a, new Vector2(w, h), new Color(0, 0, 0, 255), true);
                if (drawOnButtonText != "")
                {
                    Drawing.DrawText(drawOnButtonText, a + new Vector2(10, 2), Color.White,
                    FontFlags.AntiAlias | FontFlags.DropShadow);
                }
            }
            else
            {
                Drawing.DrawRect(a, new Vector2(w, h), Color.Gray);
                Drawing.DrawRect(a, new Vector2(w, h), new Color(0, 0, 0, 255), true);
            }
        }
        public static void PrintInfo(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.White, arguments);
        }

        public static void PrintSuccess(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.Green, arguments);
        }

        public static void PrintError(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.Red, arguments);
        }

        public static void PrintEncolored(string text, ConsoleColor color, params object[] arguments)
        {
            var clr = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text, arguments);
            Console.ForegroundColor = clr;
        }
        #endregion

    }
}