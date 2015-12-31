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
        private static Hero _globalTarget2;
        private static int _tick;
        private static readonly Dictionary<uint,int> LastAttackStart = new Dictionary<uint, int>();
        private static readonly Dictionary<uint, NetworkActivity> LastActivity = new Dictionary<uint, NetworkActivity>();
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
            "item_mjollnir",

            "item_sheepstick"
            /*"item_refresher"*/
        };

        private static readonly List<string> CloneOnlyItems = new List<string>
        {
            "item_diffusal_blade",
            "item_flask",
            "item_clarity",
            "item_enchanted_mango",
            "item_bottle",
            "item_diffusal_blade_2"
        };

        private static readonly List<string> CloneOnlyComboItems = new List<string>
        {
            "item_diffusal_blade",
            "item_diffusal_blade_2"
        };
        private enum Orders
        {
            Monkey,
            Caster,
            Nothing
        }
        private enum BkbUsage
        {
            Me, 
            Clones, 
            All,
            NoOne
        }
        private static void Main()
        {
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Player.OnExecuteOrder += Player_OnExecuteAction;
            //var dict = Items.ToDictionary(item => item, item => true);
            Menu.AddItem(new MenuItem("hotkey", "Hotkey").SetValue(new KeyBind('G', KeyBindType.Press)));
            Menu.AddItem(new MenuItem("spamHotkey", "Spark Spam").SetValue(new KeyBind('H', KeyBindType.Press)));
            Menu.AddItem(new MenuItem("hotkeyClone", "ComboKey with Clones").SetValue(new KeyBind('Z', KeyBindType.Toggle)));
            //Menu.AddItem(new MenuItem("Items", "Items:").SetValue(new AbilityToggler(dict)));
            Menu.AddItem(new MenuItem("LockTarget", "Lock Target").SetValue(true));
            Menu.AddItem(new MenuItem("AutoMidas", "Auto Midas").SetValue(true));
            Menu.AddItem(new MenuItem("FirstClone", "Ez Heal").SetValue(true).SetTooltip("when you use some heal-items, at the beginning of the clone will use this"));
            //Menu.AddItem(new MenuItem("AutoHeal", "Auto Heal/Bottle").SetValue(true).SetTooltip("clone use heal items on main hero if there are no enemies in 500(800) range"));
            Menu.AddItem(new MenuItem("usePrediction", "Use Prediction For Spark").SetValue(true));
            Menu.AddItem(new MenuItem("BkbUsage", "Bkb Selection").SetValue(new StringList(new[] { "me", "clones", "all","no one" }, 1)));
            //var il=new Menu("Illusion","il");
            //il.AddItem(new MenuItem("orderList", "Use order list").SetValue(false));
            Menu.AddItem(new MenuItem("order", "Clone Order Selection").SetValue(new StringList(new[] { "monkey", "caster", "nothing" }, 1)));
            
            //Menu.AddSubMenu(il);
            Menu.AddToMainMenu();
        }

        private static void Player_OnExecuteAction(Player sender, ExecuteOrderEventArgs args)
        {
            if (Menu.Item("order").GetValue<StringList>().SelectedIndex == (int) Orders.Monkey)
            {
                //Game.PrintMessage(args.Order.ToString(), MessageType.ChatMessage);
                if (args.Order != Order.Stop && args.Order != Order.AttackLocation && args.Order != Order.AttackTarget &&
                    args.Order != Order.Ability && args.Order != Order.AbilityTarget &&
                    args.Order != Order.AbilityLocation &&
                    args.Order != Order.MoveLocation && args.Order != Order.MoveTarget && args.Order != Order.Hold)
                    return;

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
            else if (Menu.Item("FirstClone").GetValue<bool>())
            {
                if (args.Order != Order.Ability && args.Order != Order.AbilityTarget &&
                    args.Order != Order.AbilityLocation)
                    return;
                if (!CloneOnlyItems.Contains(args.Ability.Name)) return;
                foreach (var hero in GetCloneList(sender.Hero).Where(x=>x.Distance2D(sender.Hero)<=1000))
                {
                    Ability spell;
                    Ability needed;
                    switch (args.Order)
                    {
                        case Order.Ability:
                            spell = args.Ability;
                            needed = hero.FindSpell(spell.Name) ?? hero.FindItem(spell.Name);
                            if (needed != null && needed.CanBeCasted())
                            {
                                needed.UseAbility(sender.Hero);
                                args.Process = false;
                            }
                            break;
                        case Order.AbilityTarget:
                            spell = args.Ability;
                            needed = hero.FindSpell(spell.Name) ?? hero.FindItem(spell.Name);
                            if (needed != null && needed.CanBeCasted())
                            {
                                needed.UseAbility(args.Target as Unit);
                                args.Process = false;
                            }
                            break;
                        case Order.AbilityLocation:
                            spell = args.Ability;
                            needed = hero.FindSpell(spell.Name) ?? hero.FindItem(spell.Name);
                            if (needed != null && needed.CanBeCasted())
                            {
                                needed.UseAbility(args.TargetPosition);
                                args.Process = false;
                            }
                            break;
                    }
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!_loaded) return;

            Vector2 pos;
            if (_globalTarget2 != null && _globalTarget2.IsAlive)
            {
                pos = Drawing.WorldToScreen(_globalTarget2.Position);
                Drawing.DrawText("CloneTarget", pos, new Vector2(0, 50), Color.Red,
                    FontFlags.AntiAlias | FontFlags.DropShadow);
            }

            if (_globalTarget == null || !_globalTarget.IsAlive) return;
            pos = Drawing.WorldToScreen(_globalTarget.Position);
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
                Game.PrintMessage(
                    "<font face='Comic Sans MS, cursive'><font color='#00aaff'>" + Menu.DisplayName + " By Jumpering" +
                    " loaded!</font> <font color='#aa0000'>v" + Assembly.GetExecutingAssembly().GetName().Version,
                    MessageType.LogMessage);
            }

            if (!Game.IsInGame || me == null)
            {
                _loaded = false;
                return;
            }
            if (Game.IsPaused) return;

            _tick = Environment.TickCount;
            NetworkActivity act;
            var handle = me.Handle;
            if (!LastActivity.TryGetValue(handle, out act) || me.NetworkActivity != act)
            {
                LastActivity.Remove(handle);
                LastActivity.Add(handle, me.NetworkActivity);
                if (me.IsAttacking())
                {
                    LastAttackStart.Remove(handle);
                    LastAttackStart.Add(handle,_tick);
                }
            }
            foreach (var clone in GetCloneList(me))
            {
                handle = clone.Handle;
                if (LastActivity.TryGetValue(handle, out act) && clone.NetworkActivity == act) continue;
                LastActivity.Remove(handle);
                LastActivity.Add(handle, clone.NetworkActivity);
                if (!clone.IsAttacking()) continue;
                LastAttackStart.Remove(handle);
                LastAttackStart.Add(handle, _tick);
            }
            if (Menu.Item("spamHotkey").GetValue<KeyBind>().Active)
            {
                SparkSpam(me);
                return;
            }
            if (Menu.Item("hotkeyClone").GetValue<KeyBind>().Active)
            {
                if (_globalTarget2 == null || !_globalTarget2.IsValid)
                {
                    _globalTarget2 = ClosestToMouse(me, 300);
                }
                if (_globalTarget2 != null && _globalTarget2.IsValid && _globalTarget2.IsAlive)
                {
                    DoCombo2(me, _globalTarget2);
                }
            }
            else
            {
                _globalTarget2 = null;
            }

            //if (!me.IsAlive) return;

            var midas = me.FindItem("item_hand_of_midas");
            if (midas != null && Menu.Item("AutoMidas").GetValue<bool>())
            {
                if (midas.CanBeCasted() && Utils.SleepCheck(me.Handle + "midas") && me.IsAlive)
                {
                    var enemy =
                        ObjectMgr.GetEntities<Unit>()
                            .FirstOrDefault(
                                x =>
                                    x.Team != me.Team && (x.ClassID == ClassID.CDOTA_BaseNPC_Creep_Lane || x.ClassID == ClassID.CDOTA_BaseNPC_Creep_Siege || x.ClassID == ClassID.CDOTA_BaseNPC_Creep_Neutral) && x.IsSpawned && x.IsAlive &&
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
                                    x.Team != me.Team && (x.ClassID == ClassID.CDOTA_BaseNPC_Creep_Lane || x.ClassID == ClassID.CDOTA_BaseNPC_Creep_Siege || x.ClassID == ClassID.CDOTA_BaseNPC_Creep_Neutral) && x.IsSpawned && x.IsAlive &&
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
            if (_globalTarget == null || !_globalTarget.IsValid || !_globalTarget.IsAlive) return;

            DoCombo(me, _globalTarget);
        }

        private static void DoCombo2(Hero me, Hero target)
        {
            foreach (var hero in GetCloneList(me))
            {
                var d = hero.Distance2D(target);
                var inv = hero.Inventory.Items;
                var enumerable = inv as Item[] ?? inv.ToArray();
                var dagger = enumerable.Any(x => x.Name == "item_blink" && x.Cooldown == 0);
                SpellsUsage(hero, target, d, dagger);
                ItemUsage(hero, enumerable, target, d,
                    Menu.Item("BkbUsage").GetValue<StringList>().SelectedIndex == (int) BkbUsage.Clones ||
                    Menu.Item("BkbUsage").GetValue<StringList>().SelectedIndex == (int) BkbUsage.All, true);
                
                Orbwalk(hero, target);/*
                if (!Utils.SleepCheck("clone_attacking" + hero.Handle)) continue;
                hero.Attack(target);
                Utils.Sleep(350, "clone_attacking" + hero.Handle);*/

                /*
                if (Utils.SleepCheck("clone_attacking" + hero.Handle))
                {
                    hero.Attack(target);
                    Utils.Sleep(UnitDatabase.GetAttackPoint(me) * 1000 - Game.Ping + 50, "clone_attacking" + hero.Handle);
                }
                else if (Utils.SleepCheck("clone_moving" + hero.Handle) && me.NetworkActivity != NetworkActivity.Attack && me.NetworkActivity != NetworkActivity.Attack2)
                {
                    hero.Move(target.Position);
                    Utils.Sleep(300, "clone_moving" + hero.Handle);
                }
                else
                {
                    break;
                }*/
            }
            var illusions = ObjectMgr.GetEntities<Hero>().Where(x => x.IsAlive && x.IsControllable && x.Team == me.Team && x.IsIllusion && x.Modifiers.Any(y => y.Name != "modifier_kill")).ToList();
            foreach (var illusion in illusions.TakeWhile(illusion => Utils.SleepCheck("clone_attacking" + illusion.Handle) && illusion.Distance2D(target) <= 1500))
            {
                illusion.Attack(target);
                Utils.Sleep(350, "clone_attacking" + illusion.Handle);
            }
            var necr = ObjectMgr.GetEntities<Unit>().Where(x => x.IsAlive && x.IsControllable && x.Team == me.Team && x.IsSummoned).ToList();
            foreach (var necronomicon in necr.TakeWhile(illusion => Utils.SleepCheck("clone_attacking" + illusion.Handle) && illusion.Distance2D(target) <= 1500 && !Equals(illusion, me)))
            {
                necronomicon.Attack(target);
                var spell = necronomicon.Spellbook.Spell1;
                if (spell != null && spell.CanBeCasted(target) && necronomicon.Distance2D(target) <= spell.CastRange && Utils.SleepCheck(spell.Name + "clone_attacking" + necronomicon.Handle))
                {
                    spell.UseAbility(target);
                    Utils.Sleep(300, spell.Name + "clone_attacking" + necronomicon.Handle);
                }
                Utils.Sleep(600, "clone_attacking" + necronomicon.Handle);
            }
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
            IEnumerable<Item> inv;
            Item[] enumerable;
            bool dagger;
            if (Menu.Item("order").GetValue<StringList>().SelectedIndex == (int)Orders.Caster && !Menu.Item("hotkeyClone").GetValue<KeyBind>().Active)
            {
                foreach (var hero in GetCloneList(me))
                {
                    var d = hero.Distance2D(target);
                    inv = hero.Inventory.Items;
                    enumerable = inv as Item[] ?? inv.ToArray();
                    dagger = enumerable.Any(x => x.Name == "item_blink" && x.Cooldown == 0);
                    SpellsUsage(hero, target, d, dagger);
                    ItemUsage(hero, enumerable, target, d,
                        Menu.Item("BkbUsage").GetValue<StringList>().SelectedIndex == (int) BkbUsage.Clones ||
                        Menu.Item("BkbUsage").GetValue<StringList>().SelectedIndex == (int) BkbUsage.All, true);
                    Orbwalk(hero, target);
                    /*
                    if (!Utils.SleepCheck("clone_attacking" + hero.Handle)) continue;
                    hero.Attack(target);
                    Utils.Sleep(350, "clone_attacking" + hero.Handle);*/
                }
            }
            var illusions = ObjectMgr.GetEntities<Hero>().Where(x => x.IsAlive && x.IsControllable && x.Team == me.Team && x.IsIllusion && x.Modifiers.Any(y => y.Name != "modifier_kill")).ToList();
            foreach (var illusion in illusions.TakeWhile(illusion => Utils.SleepCheck("clone_attacking" + illusion.Handle)))
            {
                illusion.Attack(target);
                Utils.Sleep(350, "clone_attacking" + illusion.Handle);
            }
            var necr = ObjectMgr.GetEntities<Unit>().Where(x => x.IsAlive && x.IsControllable && x.Team == me.Team && x.IsSummoned).ToList();
            foreach (var necronomicon in necr.TakeWhile(illusion => Utils.SleepCheck("clone_attacking" + illusion.Handle) && illusion.Distance2D(target) <= 1500 && !Equals(illusion, me)))
            {
                necronomicon.Attack(target);
                var spell = necronomicon.Spellbook.Spell1;
                if (spell != null && spell.CanBeCasted(target) && necronomicon.Distance2D(target) <= spell.CastRange && Utils.SleepCheck(spell.Name + "clone_attacking" + necronomicon.Handle))
                {
                    spell.UseAbility(target);
                    Utils.Sleep(300, spell.Name + "clone_attacking" + necronomicon.Handle);
                }
                Utils.Sleep(600, "clone_attacking" + necronomicon.Handle);
            }
            if (!me.IsAlive) return;
            inv = me.Inventory.Items;
            enumerable = inv as Item[] ?? inv.ToArray();
            dagger = enumerable.Any(x=>x.Name=="item_blink" && x.Cooldown==0);
            SpellsUsage(me, target, distance, dagger);
            ItemUsage(me,enumerable, target, distance,
                Menu.Item("BkbUsage").GetValue<StringList>().SelectedIndex == (int) BkbUsage.Me ||
                Menu.Item("BkbUsage").GetValue<StringList>().SelectedIndex == (int) BkbUsage.All);
            Orbwalk(me,target);
            /*
            if (!Utils.SleepCheck("attacking")) return;
            me.Attack(target);
            Utils.Sleep(200, "attacking");*/
        }

        private static void SpellsUsage(Hero me, Hero target, float distance,bool daggerIsReady)
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
            if (w != null && w.CanBeCasted() && Utils.SleepCheck(w.Name) && me.Modifiers.All(x => x.Name != "modifier_arc_warden_magnetic_field") && distance <= 600 && !daggerIsReady)
            {
                w.UseAbility(Prediction.InFront(me,200));
                Utils.Sleep(500, w.Name);
            }
            if (e != null && e.CanBeCasted() && Utils.SleepCheck(me.Handle + e.Name))
            {
                var predVector3 = target.NetworkActivity == NetworkActivity.Move && Menu.Item("usePrediction").GetValue<bool>()
                        ? Prediction.InFront(target, target.MovementSpeed * 3 + Game.Ping / 1000)
                        : target.Position;
                e.UseAbility(predVector3);
                Utils.Sleep(500, me.Handle + e.Name);
            }
            var r = me.Spellbook.SpellR;
            if (r == null || !r.CanBeCasted() || !Utils.SleepCheck(me.Handle + r.Name) || distance>900) return;
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

        private static void ItemUsage(Hero me, IEnumerable<Item> inv, Hero target, float distance, bool useBkb, bool byIllusion = false)
        {
            if (me.IsChanneling()) return;
            var inventory = inv.Where(x => Utils.SleepCheck(x.Name + me.Handle) && x.CanBeCasted()/* && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(x.Name)*/).ToList();
            var items = inventory.Where(x => Items.Contains(x.Name) && ((x.CastRange == 0 && distance <= 1150) || x.CastRange >= distance)).ToList();
            foreach (var item in items)
            {
                item.UseAbility();
                item.UseAbility(target);
                item.UseAbility(target.Position);
                item.UseAbility(me);
                Utils.Sleep(500, item.Name + me.Handle);
            }
            var underDiff = target.Modifiers.Any(x => x.Name == "modifier_item_diffusal_blade_slow");
            //Game.PrintMessage("Under SLow?: "+target.Modifiers.Any(x=>x.Name=="modifier_item_diffusal_blade_slow"),MessageType.ChatMessage);
            if (byIllusion && !underDiff && !target.IsStunned() && !target.IsHexed())
            {
                var items2 = inventory.Where(x => CloneOnlyComboItems.Contains(x.Name) && ((x.CastRange == 0 && distance <= 650) || x.CastRange >= distance)).ToList();
                foreach (var item in items2)
                {
                    item.UseAbility(target);
                    Utils.Sleep(500, item.Name + me.Handle);
                }
            }
            if (useBkb)
            {
                var bkb = inventory.FirstOrDefault(x => x.Name == "item_black_king_bar");
                if (bkb != null && bkb.CanBeCasted() && Utils.SleepCheck(bkb.Name + me.Handle))
                {
                    bkb.UseAbility();
                    Utils.Sleep(500, bkb.Name + me.Handle);
                }
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

        private static void Orbwalk(
            Hero me,
            Unit target,
            float bonusWindupMs = 0,
            float bonusRange = 0)
        {
            if (me == null)
            {
                return;
            }
            var targetHull = 0f;
            if (target != null)
            {
                targetHull = target.HullRadius;
            }
            float distance = 0;
            if (target != null)
            {
                var pos = Prediction.InFront(
                    me,
                    (float)((Game.Ping / 1000 + me.GetTurnTime(target.Position)) * me.MovementSpeed));
                distance = pos.Distance2D(target) - me.Distance2D(target);
            }
            var isValid = target != null && target.IsValid && target.IsAlive && target.IsVisible && !target.IsInvul()
                          && !target.Modifiers.Any(
                              x => x.Name == "modifier_ghost_state" || x.Name == "modifier_item_ethereal_blade_slow")
                          && target.Distance2D(me)
                          <= (me.GetAttackRange() + me.HullRadius + 50 + targetHull + bonusRange + Math.Max(distance, 0));
            if (isValid || (target != null && me.IsAttacking() && me.GetTurnTime(target.Position) < 0.1))
            {
                var canAttack = !AttackOnCooldown(me,target, bonusWindupMs)
                                && !target.IsAttackImmune() && !target.IsInvul() && me.CanAttack();
                if (canAttack && Utils.SleepCheck("Orbwalk.Attack"))
                {
                    me.Attack(target);
                    Utils.Sleep(
                        UnitDatabase.GetAttackPoint(me) * 1000 + me.GetTurnTime(target) * 1000,
                        "Orbwalk.Attack");
                    return;
                }
            }
            var canCancel = (CanCancelAnimation(me) && AttackOnCooldown(me,target, bonusWindupMs))
                            || (!isValid && !me.IsAttacking() && CanCancelAnimation(me));
            if (!canCancel || !Utils.SleepCheck("Orbwalk.Move") || !Utils.SleepCheck("Orbwalk.Attack"))
            {
                return;
            }
            if (target != null) me.Move(target.Position);
            Utils.Sleep(100, "Orbwalk.Move");
        }

        private static bool AttackOnCooldown(Hero me, Entity target = null, float bonusWindupMs = 0)
        {
            if (me == null)
            {
                return false;
            }
            var turnTime = 0d;
            if (target != null)
            {
                turnTime = me.GetTurnTime(target);
            }
            int lastAttackStart;
            LastAttackStart.TryGetValue(me.Handle,out lastAttackStart);
            return lastAttackStart + UnitDatabase.GetAttackRate(me)*1000 - Game.Ping - turnTime*1000 - 75
                   + bonusWindupMs > _tick;
        }

        private static bool CanCancelAnimation(Hero me, float delay = 0f)
        {
            int lastAttackStart;
            LastAttackStart.TryGetValue(me.Handle, out lastAttackStart);
            var time = _tick - lastAttackStart;
            var cancelDur = UnitDatabase.GetAttackPoint(me) * 1000 - Game.Ping + 50 - delay;
            return time > cancelDur;
        }

        private static Hero ClosestToMouse(Hero source, float range = 600)
        {
            var mousePosition = Game.MousePosition;
            var enemyHeroes = ObjectMgr.GetEntities<Hero>().Where(x => x.Team == source.GetEnemyTeam() && !x.IsIllusion && x.IsAlive && x.IsVisible && x.Distance2D(mousePosition) <= range /*&& !x.IsMagicImmune()*/).OrderBy(source.Distance2D);
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
