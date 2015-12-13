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

namespace SlarkAnnihilation
{
    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    internal static class Program
    {
        private static bool _loaded;
        private static readonly Menu Menu = new Menu("Slark Annihilation", "slark", true, "npc_dota_hero_slark", true);
        private static Hero _globalTarget;

        private static readonly List<string> Items = new List<string>
        {
            "item_mask_of_madness",
            "item_mjollnir",
            "item_abyssal_blade",
            "item_blink",
            "item_orchid",
            "item_sheepstick"
        };

        private static void Main()
        {
            Game.OnUpdate+=Game_OnUpdate;
            Drawing.OnDraw+=Drawing_OnDraw;

            var dict=new Dictionary<string,bool>
            {
                {Items[0],true},
                {Items[1],true},
                {Items[2],true},
                {Items[3],true},
                {Items[4],true},
                {Items[5],true}
            };
            Menu.AddItem(new MenuItem("hotkey", "Hotkey").SetValue(new KeyBind('G', KeyBindType.Press)));
            Menu.AddItem(new MenuItem("Items", "Items:").SetValue(new AbilityToggler(dict)));
            Menu.AddItem(new MenuItem("minHp", "Min Hp %").SetValue(new Slider(15)));
            Menu.AddItem(new MenuItem("LockTarget", "Lock Target").SetValue(true));

            Menu.AddToMainMenu();
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
                if (!Game.IsInGame || me == null)
                {
                    return;
                }
                _loaded = true;
                Game.PrintMessage(
                    "<font face='Comic Sans MS, cursive'><font color='#00aaff'>" + Menu.DisplayName + " By Jumpering" +
                    " loaded!</font> <font color='#aa0000'>v" + Assembly.GetExecutingAssembly().GetName().Version, MessageType.LogMessage);
            }

            if (!Game.IsInGame || me == null)
            {
                _loaded = false;
                return;
            }
            if (!me.IsAlive || Game.IsPaused) return;

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

            DoCombo(me,_globalTarget);
        }

        private static void DoCombo(Hero me, Hero target)
        {
            var darkPact = me.Spellbook.SpellQ;
            var pounce = me.Spellbook.SpellW;
            var shadowDance = me.Spellbook.SpellR;
            var distance = me.Distance2D(target);

            var inSb = me.Modifiers.Any(x => x.Name == "modifier_item_silver_edge_windwalk" || x.Name == "modifier_item_invisibility_edge_windwalk");
            if (inSb)
            {
                if (!Utils.SleepCheck("attacking")) return;
                me.Attack(target);
                Utils.Sleep(200, "attacking");
                return;
            }
            if (Utils.SleepCheck("items"))
            {
                /*foreach (var item in me.Inventory.Items)
                {
                    Game.PrintMessage(item.Name+": "+item.AbilityBehavior,MessageType.ChatMessage);
                }*/
                var items =
                    me.Inventory.Items.Where(
                        x =>
                            Items.Contains(x.Name) && x.CanBeCasted() && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(x.Name) &&
                            (x.CastRange==0 || x.CastRange >= distance) && Utils.SleepCheck(x.Name)).ToList();
                foreach (var item in items)
                {
                    switch (item.ClassID)
                    {
                        case ClassID.CDOTA_Item_BlinkDagger:
                            var p = Prediction.InFront(target, 100);
                            var dist = me.Distance2D(p);
                            if (dist <= 1150 && dist >= 400 && pounce != null && pounce.CanBeCasted())
                            {
                                item.UseAbility(p);
                                Utils.Sleep(200, item.Name);
                            }
                            break;
                        default:
                            if (item.IsAbilityBehavior(AbilityBehavior.UnitTarget))
                            {
                                if (!target.IsStunned() && !target.IsHexed())
                                {
                                    item.UseAbility(target);
                                }
                                item.UseAbility(me);
                            }
                            else
                            {
                                item.UseAbility();
                            }
                            Utils.Sleep(200, item.Name);
                            break;
                    }
                }
            }

            if (Utils.SleepCheck("dp") && darkPact != null && darkPact.CanBeCasted() && darkPact.CanHit(target))
            {
                darkPact.UseAbility();
                Utils.Sleep(100, "dp");
            }
            var angle = (float) Math.Max(
                Math.Abs(me.RotationRad - Utils.DegreeToRadian(me.FindAngleBetween(target.Position))) - 0.20, 0);
            if (Utils.SleepCheck("pounce") && pounce != null && pounce.CanBeCasted() && distance <= 300 && angle == 0)
            {
                pounce.UseAbility();
                Utils.Sleep(100, "pounce");
            }

            if (me.Health <= me.MaximumHealth / 100 * Menu.Item("minHp").GetValue<Slider>().Value && Utils.SleepCheck("dance") && shadowDance != null && shadowDance.CanBeCasted())
            {
                shadowDance.UseAbility();
                Utils.Sleep(100, "dance");
            }

            if (!Utils.SleepCheck("attacking")) return;
            me.Attack(target);
            Utils.Sleep(200, "attacking");
        }

        private static Hero ClosestToMouse(Hero source, float range = 600)
        {
            var mousePosition = Game.MousePosition;
            var enemyHeroes =
                ObjectMgr.GetEntities<Hero>()
                    .Where(
                        x =>
                            x.Team == source.GetEnemyTeam() && !x.IsIllusion && x.IsAlive && x.IsVisible
                            && x.Distance2D(mousePosition) <= range && !x.IsMagicImmune()).OrderBy(source.Distance2D);
            return enemyHeroes.FirstOrDefault();
        }
    }
}
