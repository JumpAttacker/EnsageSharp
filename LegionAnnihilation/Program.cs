using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using SharpDX;

namespace Legion_Annihilation
{
    internal class Program
    {
        #region Members
        //============================================================
        private static bool _loaded;
        private static readonly string Ver = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        //============================================================
        private static readonly Menu Menu = new Menu("LegionAnnihilation", "LegionAnnihilation", true);
        private static Hero _globalTarget;

        #endregion

        private static void Main()
        {
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += OnDraw;
            _loaded = false;

            Menu.AddItem(new MenuItem("combokey", "Combo key").SetValue(new KeyBind('F',KeyBindType.Press)).SetTooltip("just hold this key for combo"));
            var dict = new Dictionary<string, bool>
            {
                {"item_black_king_bar", false},
                {"legion_commander_press_the_attack", true},
                {"legion_commander_overwhelming_odds", true}
            };
            Menu.AddItem(new MenuItem("enabledAbilities", "Abilities:").SetValue(new AbilityToggler(dict)));
            Menu.AddItem(new MenuItem("buff", "Buff Me").SetValue(true).SetTooltip("use items on myself"));
            Menu.AddItem(new MenuItem("debuff", "Debuff enemy").SetValue(true).SetTooltip("use items on enemy"));
            Menu.AddToMainMenu();
        }

        private static void OnDraw(EventArgs args)
        {
            if (!_loaded || _globalTarget == null || !_globalTarget.IsValid || !_globalTarget.IsAlive) return;

            var start = HUDInfo.GetHPbarPosition(_globalTarget) + new Vector2(-HUDInfo.GetHPBarSizeX(_globalTarget)/2, -HUDInfo.GetHpBarSizeY(_globalTarget)*5);
            var size = new Vector2(HUDInfo.GetHPBarSizeX(_globalTarget), HUDInfo.GetHpBarSizeY(_globalTarget) / 2)*2;
                            
            const string text = "TARGET";
            var textSize = Drawing.MeasureText(text, "Arial", new Vector2(size.Y * 2, size.X), FontFlags.AntiAlias);
            var textPos = start + new Vector2(size.X / 2 - textSize.X / 2, -textSize.Y / 2 + 2);
            Drawing.DrawText(
                text,
                textPos,
                new Vector2(size.Y*2, size.X),
                Color.White,
                FontFlags.AntiAlias | FontFlags.DropShadow | FontFlags.Additive);
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

                PrintSuccess(string.Format("> {1} v{0}", Ver, Menu.DisplayName));
                Game.PrintMessage(
                    "<font face='Comic Sans MS, cursive'><font color='#00aaff'>" + Menu.DisplayName+
                    " loaded!</font> <font color='#aa0000'>v" + Ver, MessageType.LogMessage);
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


            if (!Menu.Item("combokey").GetValue<KeyBind>().Active)
            {
                _globalTarget = null;
                return;
            }
            if (_globalTarget == null || !_globalTarget.IsValid)
            {
                _globalTarget = ClosestToMouse(me);
            }
            if (_globalTarget == null || !_globalTarget.IsValid || !_globalTarget.IsAlive || !me.CanCast()) return;
            
            ComboInAction(me, _globalTarget);

            #endregion
        }

        private static void ComboInAction(Hero me, Hero target)
        {
            if (!Utils.SleepCheck("nextAction")) return;
            var duel = me.Spellbook.Spell4;
            if (duel==null) return;
            if (!duel.CanBeCasted()) return;

            var haras = me.Spellbook.Spell1;
            var heal = me.Spellbook.Spell2;
            
            var dagger = me.FindItem("item_blink");
            var neededMana = me.Mana-duel.ManaCost;

            var allitems = me.Inventory.Items.Where(x => x.CanBeCasted() && x.ManaCost <= neededMana);
            var dpActivated =
                target.Modifiers.Any(
                    x => x.Name == "modifier_slark_dark_pact" || x.Name == "modifier_slark_dark_pact_pulses");
            var enumerable = allitems as Item[] ?? allitems.ToArray();
            var isInvise = me.IsInvisible();
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
                        x.Name == "item_blade_mail" || x.Name == "item_silver_edge" || x.Name == "item_invis_sword");
            var itemOnMySelf = enumerable.FirstOrDefault(
                x =>
                    x.Name == "item_mjollnir");
            Item bkb = null;
            if (Menu.Item("enabledAbilities").GetValue<AbilityToggler>().IsEnabled("item_black_king_bar"))
            {
                bkb = me.FindItem("item_black_king_bar");
            }
            
            var distance = me.Distance2D(target);
            if (distance >= 1150)
            {
                me.Move(target.Position);
                Utils.Sleep(200 + Game.Ping, "nextAction");
                return;
            }
            if (Menu.Item("enabledAbilities").GetValue<AbilityToggler>().IsEnabled("legion_commander_overwhelming_odds") && haras != null && haras.CanBeCasted() && distance <= haras.CastRange)
            {
                haras.UseAbility(target.Position);
                Utils.Sleep(300 + Game.Ping, "nextAction");
                return;
            }
            if (!me.IsMagicImmune() && heal.CanBeCasted() && heal.ManaCost <= neededMana && Menu.Item("enabledAbilities").GetValue<AbilityToggler>().IsEnabled("legion_commander_press_the_attack"))
            {
                heal.UseAbility(me);
                Utils.Sleep(200 + Game.Ping, "nextAction");
                return;
            }
            if (itemOnMySelf != null && Menu.Item("buff").GetValue<bool>())
            {
                itemOnMySelf.UseAbility(me);
                Utils.Sleep(50 + Game.Ping, "nextAction");
                return;
            }
            if (itemWithOutTarget != null && Menu.Item("buff").GetValue<bool>())
            {
                if (itemWithOutTarget.Name == "item_armlet")
                {
                    itemWithOutTarget.ToggleAbility();
                    Utils.Sleep(50 + Game.Ping, "nextAction");
                    return;
                }
                itemWithOutTarget.UseAbility();
                Utils.Sleep(100 + Game.Ping, "nextAction");
                return;
            }

            if (dagger != null && dagger.CanBeCasted() && !isInvise && Utils.SleepCheck("dagger"))
            {
                var point = new Vector3(
                    (float)(target.Position.X - 20 * Math.Cos(me.FindAngleBetween(target.Position, true))),
                    (float)(target.Position.Y - 20 * Math.Sin(me.FindAngleBetween(target.Position, true))),
                    target.Position.Z);
                dagger.UseAbility(point);
                Utils.Sleep(200 + Game.Ping, "dagger");
                return;
            }
            if (distance > duel.CastRange + 100 && Utils.SleepCheck("moving"))
            {
                if (isInvise)
                    me.Attack(target);
                else
                    me.Move(target.Position);
                Utils.Sleep(150 + Game.Ping, "moving");
                return;
            }
            if (itemOnTarget != null && !dpActivated && Menu.Item("debuff").GetValue<bool>() && !isInvise)
            {
                itemOnTarget.UseAbility(target);
                Utils.Sleep(50 + Game.Ping, "nextAction");
                return;
            }
            if (Menu.Item("enabledAbilities").GetValue<AbilityToggler>().IsEnabled("item_black_king_bar") && bkb != null && bkb.CanBeCasted() && Utils.SleepCheck("bkb") && !isInvise)
            {
                bkb.UseAbility();
                Utils.Sleep(35+Game.Ping, "bkb");
                return;
            }
            if (isInvise)
            {
                me.Attack(target);
                Utils.Sleep(200 + Game.Ping, "nextAction");
            }
            else if (Utils.SleepCheck("ult"))
            {
                if (distance >= 100)
                    Utils.Sleep(200 + Game.Ping, "ult");
                duel.UseAbility(target);
            }

            Utils.Sleep(10, "nextAction");
        }

        public static Hero ClosestToMouse(Hero source, float range = 600)
        {
            var mousePosition = Game.MousePosition;
            var enemyHeroes =
                ObjectMgr.GetEntities<Hero>()
                    .Where(
                        x =>
                            x.Team == source.GetEnemyTeam() && !x.IsIllusion && x.IsAlive && x.IsVisible
                            && x.Distance2D(mousePosition) <= range);
            Hero[] closestHero = { null };
            foreach (var enemyHero in enemyHeroes.Where(enemyHero => closestHero[0] == null || closestHero[0].Distance2D(mousePosition) > enemyHero.Distance2D(mousePosition)))
            {
                closestHero[0] = enemyHero;
            }
            return closestHero[0];
        }

        #region Helpers
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