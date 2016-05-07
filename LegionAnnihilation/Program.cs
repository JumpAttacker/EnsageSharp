using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using SharpDX;

namespace Legion_Annihilation
{
    internal static class Program
    {
        #region Members
        //============================================================
        private static readonly Menu Menu = new Menu("Legion Annihilation", "LegionAnnihilation", true, "npc_dota_hero_legion_commander", true);
        private static Hero _globalTarget;

        #endregion

        private static void Main()
        {
            Menu.AddItem(new MenuItem("combokey", "Combo key").SetValue(new KeyBind('F', KeyBindType.Press)).SetTooltip("just hold this key for combo"));
            var dict = new Dictionary<string, bool>
            {
                {"item_black_king_bar", false}
                /*{"legion_commander_press_the_attack", true},
                {"legion_commander_overwhelming_odds", true}*/
            };
            Menu.AddItem(new MenuItem("enabledAbilities", "Abilities:").SetValue(new AbilityToggler(dict)));
            /*Menu.AddItem(new MenuItem("buff", "Buff Me").SetValue(true).SetTooltip("use items on myself"));
            Menu.AddItem(new MenuItem("debuff", "Debuff enemy").SetValue(true).SetTooltip("use items on enemy"));*/
            Menu.AddToMainMenu();
            Events.OnLoad += (sender, args) =>
            {
                Game.PrintMessage(
                    "<font face='Comic Sans MS, cursive'><font color='#00aaff'>" + Menu.DisplayName + " By Jumpering" +
                    " loaded!</font> <font color='#aa0000'>v" + Assembly.GetExecutingAssembly().GetName().Version,
                    MessageType.LogMessage);
                PrintSuccess(Menu.DisplayName + " loaded v" + Assembly.GetExecutingAssembly().GetName().Version);
                MyHero = ObjectManager.LocalHero;
                Game.OnUpdate += Game_OnUpdate;
                Drawing.OnDraw += OnDraw;
            };
            Events.OnClose += (sender, args) =>
            {
                Game.OnUpdate -= Game_OnUpdate;
                Drawing.OnDraw -= OnDraw;
            };
        }

        private static Hero MyHero { get; set; }
        private static Item Bkb { get; set; }
        private static Item Dagger { get; set; }
        private static Ability Spell1 { get; set; }
        private static Ability Spell2 { get; set; }
        private static Ability Spell4 { get; set; }

        private static void OnDraw(EventArgs args)
        {
            if (_globalTarget == null || !_globalTarget.IsValid || !_globalTarget.IsAlive) return;

            var start = HUDInfo.GetHPbarPosition(_globalTarget) + new Vector2(-HUDInfo.GetHPBarSizeX(_globalTarget) / 2, -HUDInfo.GetHpBarSizeY(_globalTarget) * 5);
            var size = new Vector2(HUDInfo.GetHPBarSizeX(_globalTarget), HUDInfo.GetHpBarSizeY(_globalTarget) / 2) * 2;

            const string text = "TARGET";
            var textSize = Drawing.MeasureText(text, "Arial", new Vector2(size.Y * 2, size.X), FontFlags.AntiAlias);
            var textPos = start + new Vector2(size.X / 2 - textSize.X / 2, -textSize.Y / 2 + 2);
            Drawing.DrawText(
                text,
                textPos,
                new Vector2(size.Y * 2, size.X),
                Color.White,
                FontFlags.AntiAlias | FontFlags.DropShadow | FontFlags.Additive);
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            #region Init

            if (Bkb == null || !Bkb.IsValid)
            {
                Bkb = MyHero.FindItem("item_black_king_bar");
            }
            if (Dagger == null || !Dagger.IsValid)
            {
                Dagger = MyHero.FindItem("item_blink");
            }
            if (Spell1 == null || !Spell1.IsValid)
            {
                Spell1 = MyHero.Spellbook.Spell1;
            }
            if (Spell2 == null || !Spell2.IsValid)
            {
                Spell2 = MyHero.Spellbook.Spell2;
            }
            if (Spell4 == null || !Spell4.IsValid)
            {
                Spell4 = MyHero.Spellbook.Spell4;
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
                _globalTarget = ClosestToMouse(MyHero);
            }
            if (_globalTarget == null || !_globalTarget.IsValid || !_globalTarget.IsAlive || !MyHero.CanCast()) return;

            ComboInAction(_globalTarget);

            #endregion
        }

        #region ItemList

        private static readonly Dictionary<string, byte> Items = new Dictionary<string, byte>
        {
            {"item_abyssal_blade", 6},
            {"item_orchid", 5},
            {"item_heavens_halberd", 4},
            {"item_sheepstick", 6},
            {"item_urn_of_shadows", 1},
            {"item_medallion_of_courage", 1},
            {"item_solar_crest", 1},
            {"item_armlet", 1},
            {"item_soul_ring", 7},
            {"item_mask_of_madness", 1},
            {"item_satanic", 1},
            {"item_blade_mail", 1},
            {"item_silver_edge", 8},
            {"item_invis_sword", 8},
            {"item_mjollnir", 1},
        };

        private static readonly Dictionary<string, byte> ItemsLinker = new Dictionary<string, byte>
        {
            {"item_abyssal_blade", 2},
            {"item_orchid", 4},
            {"item_heavens_halberd", 5},
            {"item_sheepstick", 3},
            {"item_urn_of_shadows", 5},
        };
        #endregion

        private static void ComboInAction(Hero target)
        {
            if (!Spell4.CanBeCasted() || Spell4.Level == 0 || !Utils.SleepCheck(Spell2.StoredName())) return;

            var neededMana = MyHero.Mana - Spell4.ManaCost;

            var allitems = MyHero.Inventory.Items.Where(x => x.CanBeCasted() && x.ManaCost <= neededMana);

            var isInvise = MyHero.IsInvisible();

            var inventory =
                allitems.Where(x => Utils.SleepCheck(x.Name + MyHero.Handle)).ToList();
            var underLink = target.IsLinkensProtected();
            var distance = MyHero.Distance2D(target) - MyHero.HullRadius - target.HullRadius;
            if (underLink)
            {
                var linkerItems = inventory.Where(x => x.CanHit(target) && ItemsLinker.Keys.Contains(x.Name)).OrderByDescending(y => ItemsLinker[y.StoredName()]);
                foreach (var item in linkerItems)
                {
                    item.UseAbility(target);
                    Utils.Sleep(250, item.Name + MyHero.Handle);
                }
                if (linkerItems.Any(x => Utils.SleepCheck(x.Name + MyHero.Handle))) return;
            }
            var items =
                inventory.Where(
                    x =>
                        Items.Keys.Contains(x.Name) &&
                        ((x.CastRange == 0 && distance <= 800) ||
                         x.CastRange >= distance)).OrderByDescending(y => Items[y.StoredName()]);

            if (Dagger != null && Dagger.CanBeCasted() && !isInvise && Utils.SleepCheck("dagger") && distance <= 1200 && distance > 150)
            {
                if (UseHeal()) return;
                var point = new Vector3(
                    (float)(target.Position.X - 20 * Math.Cos(MyHero.FindAngleBetween(target.Position, true))),
                    (float)(target.Position.Y - 20 * Math.Sin(MyHero.FindAngleBetween(target.Position, true))),
                    0);
                Dagger.UseAbility(point);
                Utils.Sleep(500, "dagger");
            }
            else if (Utils.SleepCheck("attack_cd"))
            {
                Utils.Sleep(500, "attack_cd");
                MyHero.Attack(target);
            }
            if (Bkb != null && Menu.Item("enabledAbilities").GetValue<AbilityToggler>().IsEnabled(Bkb.StoredName()) &&
                Bkb.CanBeCasted() && Utils.SleepCheck(Bkb.StoredName()) && Spell4.CanHit(target))
            {
                Bkb.UseAbility();
                Utils.Sleep(500, Bkb.StoredName());
            }
            foreach (var item in items)
            {
                if (item.StoredName() == "item_armlet")
                {
                    if (!MyHero.HasModifier("modifier_item_armlet_unholy_strength"))
                    {
                        item.ToggleAbility();
                        Utils.Sleep(500, item.Name + MyHero.Handle);
                    }
                    continue;
                }
                if (item.IsAbilityBehavior(AbilityBehavior.NoTarget))
                {
                    item.UseAbility();
                }
                if (item.IsAbilityBehavior(AbilityBehavior.UnitTarget))
                {
                    if (item.TargetTeamType == TargetTeamType.Enemy || item.TargetTeamType == TargetTeamType.All)
                    {
                        item.UseAbility(target);
                    }
                    else
                    {
                        item.UseAbility(MyHero);
                    }
                }
                Utils.Sleep(500, item.Name + MyHero.Handle);
            }
            if (isInvise && Utils.SleepCheck("attack_cd_2"))
            {
                MyHero.Attack(target);
                Utils.Sleep(500 + Game.Ping, "attack_cd_2");
            }
            else if (Utils.SleepCheck("ult"))
            {
                if (distance <= 200/*Spell4.CanHit(target)*/)
                {
                    UseHeal();
                    //if (items.Any(x => Utils.SleepCheck(x.Name + MyHero.Handle))) return;
                    Utils.Sleep(100 + Game.Ping, "ult");
                    Spell4.UseAbility(target);
                }
            }
        }

        private static bool UseHeal()
        {
            if (Spell2 == null || !Spell2.CanBeCasted() || !Utils.SleepCheck(Spell2.StoredName()) || MyHero.Mana - Spell4.ManaCost <= Spell2.ManaCost) return false;
            Spell2.UseAbility(MyHero);
            Utils.Sleep(500 + Game.Ping, Spell2.StoredName());
            return true;
        }

        private static Hero ClosestToMouse(Hero source, float range = 600)
        {
            var mousePosition = Game.MousePosition;
            var enemyHeroes =
                ObjectManager.GetEntities<Hero>()
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
        private static void PrintInfo(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.White, arguments);
        }

        private static void PrintSuccess(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.Green, arguments);
        }

        public static void PrintError(string text, params object[] arguments)
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

        private static void Print(string str)
        {
            Game.PrintMessage(str, MessageType.ChatMessage);
        }
        #endregion

    }
}