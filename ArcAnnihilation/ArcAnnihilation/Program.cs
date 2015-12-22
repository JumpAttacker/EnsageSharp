using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using SharpDX;
// ReSharper disable UnusedMember.Local

namespace ArcAnnihilation
{
    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    internal static class Program
    {
        private static bool _loaded;
        private static readonly Menu Menu = new Menu("Arc Annihilation", "arc", true, "npc_dota_hero_arc_warden", true);
        private static Hero _globalTarget;

        private static readonly List<string> Items = new List<string>
        {
            "item_mask_of_madness",
            "item_dagon",
            "item_dagon_2",
            "item_dagon_3",
            "item_dagon_4",
            "item_dagon_5",
            "item_blink",
            "item_orchid",
            "item_manta",
            "item_arcane_boots",
            "item_guardian_greaves",
            "item_shivas_guard",

            "item_soul_ring",
            "item_blade_mail",
            "item_veil_of_discord",
            "item_heavens_halberd",

            "item_necronomicon",
            "item_necronomicon_2",
            "item_necronomicon_3",

            "item_sheepstick"
            /*"item_refresher"*/
        };
        private enum Orders
        {
            Monkey,
            Caster,
            Nothing
        }
        private static void Main()
        {
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Player.OnExecuteOrder += Player_OnExecuteAction;
            //var dict = Items.ToDictionary(item => item, item => true);

            Menu.AddItem(new MenuItem("hotkey", "Hotkey").SetValue(new KeyBind('G', KeyBindType.Press)));
            Menu.AddItem(new MenuItem("spamHotkey", "Spark Spam").SetValue(new KeyBind('H', KeyBindType.Press)));
            //Menu.AddItem(new MenuItem("Items", "Items:").SetValue(new AbilityToggler(dict)));
            Menu.AddItem(new MenuItem("LockTarget", "Lock Target").SetValue(true));
            Menu.AddItem(new MenuItem("AutoMidas", "Auto Midas").SetValue(true));
            //var il=new Menu("Illusion","il");
            //il.AddItem(new MenuItem("orderList", "Use order list").SetValue(false));
            Menu.AddItem(new MenuItem("order", "Clone Order Selection").SetValue(new StringList(new[] { "monkey", "caster", "nothing" }, 1)));
            
            //Menu.AddSubMenu(il);
            Menu.AddToMainMenu();
        }

        private static void Player_OnExecuteAction(Player sender, ExecuteOrderEventArgs args)
        {
            if (Menu.Item("order").GetValue<StringList>().SelectedIndex != (int)Orders.Monkey) return;
            //Game.PrintMessage(args.Order.ToString(), MessageType.ChatMessage);
            if (args.Order != Order.Stop && args.Order != Order.AttackLocation && args.Order != Order.AttackTarget &&
                args.Order != Order.Ability && args.Order != Order.AbilityTarget && args.Order != Order.AbilityLocation &&
                args.Order != Order.MoveLocation && args.Order != Order.MoveTarget && args.Order != Order.Hold) return;
            
            foreach (var hero in GetCloneList(sender.Hero))
            {
                Ability spell;
                Ability needed;
                switch (args.Order)
                {
                    case Order.Stop:
                        hero.Stop();
                        break;
                    case Order.AttackLocation:
                        hero.Attack(args.TargetPosition);
                        break;
                    case Order.AttackTarget:
                        var target = args.Target;
                        hero.Attack(target as Unit);
                        break;
                    case Order.Ability:
                        spell = args.Ability;
                        needed = hero.FindSpell(spell.Name) ?? hero.FindItem(spell.Name);
                        if (needed != null && needed.CanBeCasted())
                        {
                            needed.UseAbility();
                        }
                        break;
                    case Order.AbilityTarget:
                        spell = args.Ability;
                        needed = hero.FindSpell(spell.Name) ?? hero.FindItem(spell.Name);
                        if (needed != null && needed.CanBeCasted())
                        {
                            needed.UseAbility(args.Target as Unit);
                        }
                        break;
                    case Order.AbilityLocation:
                        spell = args.Ability;
                        needed = hero.FindSpell(spell.Name) ?? hero.FindItem(spell.Name);
                        if (needed != null && needed.CanBeCasted())
                        {
                            needed.UseAbility(args.TargetPosition);
                        }
                        break;
                    case Order.None:
                        break;
                    case Order.MoveLocation:
                        hero.Move(args.TargetPosition);
                        break;
                    case Order.MoveTarget:
                        hero.Move(args.TargetPosition);
                        break;
                    case Order.AbilityTargetTree:
                        break;
                    case Order.ToggleAbility:
                        break;
                    case Order.Hold:
                        hero.Stop();
                        break;
                    case Order.UpgradeAbility:
                        break;
                    case Order.DropItem:
                        break;
                    case Order.TransferItem:
                        break;
                    case Order.PickItem:
                        break;
                    case Order.ConsumeRune:
                        break;
                    case Order.BuyItem:
                        break;
                    case Order.SellItem:
                        break;
                    case Order.DisassembleItem:
                        break;
                    case Order.MoveItem:
                        break;
                    case Order.ToggleAutoCast:
                        break;
                    case Order.Taunt:
                        break;
                    case Order.Buyback:
                        break;
                    case Order.GlyphOfFortification:
                        break;
                    case Order.DropFromStash:
                        break;
                    case Order.AbilityTargetRune:
                        break;
                    case Order.Announce:
                        break;
                    case Order.MoveToDirection:
                        break;
                    case Order.Patrol:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!_loaded) return;

            if (_globalTarget == null || !_globalTarget.IsAlive) return;
            var pos = Drawing.WorldToScreen(_globalTarget.Position);
            Drawing.DrawText("Target", pos, new Vector2(0, 50), Color.Red, FontFlags.AntiAlias | FontFlags.DropShadow);
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            var me = ObjectMgr.LocalHero;

            if (!_loaded)
            {
                if (!Game.IsInGame || me == null || me.ClassID != ClassID.CDOTA_Unit_Hero_ArcWarden)
                {
                    return;
                }
                _loaded = true;
                Game.PrintMessage("<font face='Comic Sans MS, cursive'><font color='#00aaff'>" + Menu.DisplayName + " By Jumpering" + " loaded!</font> <font color='#aa0000'>v" + Assembly.GetExecutingAssembly().GetName().Version, MessageType.LogMessage);
            }

            if (!Game.IsInGame || me == null)
            {
                _loaded = false;
                return;
            }
            if (Game.IsPaused) return;

            if (Menu.Item("spamHotkey").GetValue<KeyBind>().Active)
            {
                SparkSpam(me);
                return;
            }
            
            if (!me.IsAlive) return;
            var midas = me.FindItem("item_hand_of_midas");
            if (midas != null && Menu.Item("AutoMidas").GetValue<bool>())
            {
                if (midas.CanBeCasted() && Utils.SleepCheck(me.Handle + "midas"))
                {
                    var enemy =
                        ObjectMgr.GetEntities<Unit>()
                            .FirstOrDefault(
                                x =>
                                    x.Team != me.Team && (x.ClassID == ClassID.CDOTA_BaseNPC_Creep_Lane || x.ClassID == ClassID.CDOTA_BaseNPC_Creep_Siege) && x.IsSpawned && x.IsAlive &&
                                    x.Distance2D(me) <= 600);
                    if (enemy != null)
                    {
                        Utils.Sleep(1000, me.Handle + "midas");
                        midas.UseAbility(enemy);
                    }
                }
                foreach (var clone in GetCloneList(me).Where(x=>Utils.SleepCheck(x.Handle+"midas")))
                {
                    midas = clone.FindItem("item_hand_of_midas");
                    if (midas == null || !midas.CanBeCasted()) continue;
                    var enemy =
                        ObjectMgr.GetEntities<Unit>()
                            .FirstOrDefault(
                                x =>
                                    x.Team != me.Team && (x.ClassID == ClassID.CDOTA_BaseNPC_Creep_Lane || x.ClassID == ClassID.CDOTA_BaseNPC_Creep_Siege) && x.IsSpawned && x.IsAlive &&
                                    x.Distance2D(clone) <= 600);
                    if (enemy == null) continue;
                    Utils.Sleep(1000, clone.Handle + "midas");
                    midas.UseAbility(enemy);
                }
            }

            /*foreach (var modifier in me.Modifiers)
            {
                Game.PrintMessage(modifier.Name, MessageType.ChatMessage);
            }*/

            if (!Menu.Item("hotkey").GetValue<KeyBind>().Active)
            {
                _globalTarget = null;
                return;
            }

            if (_globalTarget == null || !_globalTarget.IsValid || !Menu.Item("LockTarget").GetValue<bool>())
            {
                _globalTarget = ClosestToMouse(me, 300);
            }
            if (_globalTarget == null || !_globalTarget.IsValid || !_globalTarget.IsAlive || !me.CanCast()) return;

            DoCombo(me, _globalTarget);
        }

        private static void SparkSpam(Hero me)
        {
            foreach (var hero in GetCloneList(me))
            {
                var spell = hero.Spellbook.Spell3;
                if (spell == null || !spell.CanBeCasted() || !Utils.SleepCheck("spam" + hero.Handle)) continue;
                spell.UseAbility(Game.MousePosition);
                Utils.Sleep(400, "spam" + hero.Handle);
            }
            if (!me.IsAlive || !Utils.SleepCheck("spam" + me.Handle)) return;
            {
                var spell = me.Spellbook.Spell3;
                if (spell == null || !spell.CanBeCasted()) return;
                spell.UseAbility(Game.MousePosition);
                Utils.Sleep(400, "spam" + me.Handle);
            }
        }

        private static void DoCombo(Hero me, Hero target)
        {
            var distance = me.Distance2D(target);
            if (Menu.Item("order").GetValue<StringList>().SelectedIndex == (int) Orders.Caster)
            {
                foreach (var hero in GetCloneList(me))
                {
                    SpellsUsage(hero, target, distance);
                    ItemUsage(hero, target, distance);
                    
                    if (!Utils.SleepCheck("clone_attacking" + hero.Handle)) break;
                    hero.Attack(target);
                    Utils.Sleep(350, "clone_attacking" + hero.Handle);
                }
            }
            var illusions = ObjectMgr.GetEntities<Hero>().Where(x => x.IsAlive && x.IsControllable && x.Team == me.Team && x.IsIllusion && x.Modifiers.Any(y => y.Name != "modifier_kill")).ToList();
            foreach (var illusion in illusions.TakeWhile(illusion => Utils.SleepCheck("clone_attacking" + illusion.Handle)))
            {
                illusion.Attack(target);
                Utils.Sleep(350, "clone_attacking" + illusion.Handle);
            }
            var necr = ObjectMgr.GetEntities<Unit>().Where(x => x.IsAlive && x.IsControllable && x.Team == me.Team).ToList();
            foreach (var illusion in necr.TakeWhile(illusion => Utils.SleepCheck("clone_attacking" + illusion.Handle)))
            {
                illusion.Attack(target);
                Utils.Sleep(350, "clone_attacking" + illusion.Handle);
            }

            SpellsUsage(me, target, distance);
            ItemUsage(me, target, distance);
            
            if (!Utils.SleepCheck("attacking")) return;
            me.Attack(target);
            Utils.Sleep(200, "attacking");
        }

        private static void SpellsUsage(Hero me, Hero target, float distance)
        {
            var spellbook = me.Spellbook;
            var q = spellbook.SpellQ;
            var w = spellbook.SpellW;
            var e = spellbook.SpellE;
            if (q != null && q.CanBeCasted() && q.CastRange >= distance && Utils.SleepCheck(me.Handle+q.Name))
            {
                q.UseAbility(target);
                Utils.Sleep(500, me.Handle + q.Name);
            }
            if (w != null && w.CanBeCasted() && Utils.SleepCheck(w.Name) && me.Modifiers.All(x => x.Name != "modifier_arc_warden_magnetic_field") && distance <= 700)
            {
                w.UseAbility(me.Position);
                Utils.Sleep(500, w.Name);
            }
            if (e != null && e.CanBeCasted() && Utils.SleepCheck(me.Handle + e.Name))
            {
                e.UseAbility(target.Position);
                Utils.Sleep(500, me.Handle + e.Name);
            }
            var r = me.Spellbook.SpellR;
            if (r == null || !r.CanBeCasted() || !Utils.SleepCheck(me.Handle + r.Name)) return;
            r.UseAbility();
            Utils.Sleep(500, me.Handle + r.Name);
        }

        private static IEnumerable<Hero> _clones;
        private static IEnumerable<Hero> GetCloneList(Hero me)
        {
            if (!Utils.SleepCheck("get_clones")) return _clones;
            _clones = ObjectMgr.GetEntities<Hero>()
                .Where(
                    x =>
                        x.IsAlive && x.IsControllable && x.Team == me.Team &&
                        Utils.SleepCheck("spam" + x.Handle.ToString()) &&
                        x.Modifiers.Any(y => y.Name == "modifier_kill"))
                .ToList();
            if (_clones.Any())
                Utils.Sleep(100, "get_clones");
            return _clones;

        }

        private static void ItemUsage(Hero me, Hero target, float distance)
        {
            if (me.IsChanneling()) return;
            var inventory = me.Inventory.Items.Where(x => Utils.SleepCheck(x.Name + me.Handle) && x.CanBeCasted()/* && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(x.Name)*/).ToList();
            var items = inventory.Where(x => Items.Contains(x.Name) && ((x.CastRange == 0 && distance <= 700) || x.CastRange >= distance)).ToList();
            foreach (var item in items)
            {
                item.UseAbility();
                item.UseAbility(target);
                item.UseAbility(target.Position);
                Utils.Sleep(500, item.Name + me.Handle);
            }
            if (!items.Any()) return;
            {
                var r = me.Spellbook.SpellR;
                if (r == null || r.CanBeCasted()) return;
                var refresher = inventory.FirstOrDefault(x => x.Name == "item_refresher");
                refresher?.UseAbility();
                Utils.Sleep(500, refresher?.Name + me.Handle);
            }
        }

        private static Hero ClosestToMouse(Hero source, float range = 600)
        {
            var mousePosition = Game.MousePosition;
            var enemyHeroes = ObjectMgr.GetEntities<Hero>().Where(x => x.Team == source.GetEnemyTeam() && !x.IsIllusion && x.IsAlive && x.IsVisible && x.Distance2D(mousePosition) <= range && !x.IsMagicImmune()).OrderBy(source.Distance2D);
            return enemyHeroes.FirstOrDefault();
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
    }
}
