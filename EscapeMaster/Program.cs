using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;

namespace EscapeMaster
{
    internal static class Program
    {
        private static bool _loaded;
        private static readonly Menu Menu = new Menu("Escape Master", "escaper", true);
        private static bool _isIn;
        private static readonly List<string> ModifierList = new List<string>
        {
            "modifier_kunkka_x_marks_the_spot"/*,
            "modifier_disruptor_glimpse"*/
        };

        private static Item _dagger, _eul, _forceStaff;

        private static void Main()
        {
            Game.OnUpdate += Game_OnUpdate;
            //Drawing.OnDraw += Drawing_OnDraw;
            var dict=new Dictionary<string,bool>
            {
                {"item_blink",true},
                {"item_force_staff",true},
                {"item_cyclone",true}
            };
            Menu.AddItem(new MenuItem("isActive", "Is Active").SetValue(true));
            Menu.AddItem(new MenuItem("items", "Items:").SetValue(new AbilityToggler(dict)));

            Menu.AddToMainMenu();
        }

/*
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!_loaded) return;

        }
*/

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
            if (!me.IsAlive || Game.IsPaused || !Menu.Item("isActive").GetValue<bool>() || !Utils.SleepCheck("kek")) return;

            /*foreach (var m in me.Modifiers.AsEnumerable())
            {
                //Game.PrintMessage(m.Name,MessageType.ChatMessage);
            }*/

            var mod = me.Modifiers.FirstOrDefault(x => ModifierList.Contains(x.Name));
            if (_eul == null || !_eul.IsValid)
            {
                _eul=me.FindItem("item_cyclone");
            }
            if (mod!=null && !me.IsInvul())
            {
                var kun = ObjectMgr.GetEntities<Hero>().FirstOrDefault(x => x.ClassID == ClassID.CDOTA_Unit_Hero_Kunkka && x.IsVisible && x.IsAlive && x.Team!=me.Team);
                if (kun != null)
                {
                    var spell = kun.FindSpell("kunkka_return");
                    if (spell != null && spell.IsInAbilityPhase)
                    {
                        TryToHideMyAss(me);
                        return;
                    }
                }
                if (mod.RemainingTime <= 0.2)
                {
                    if (_eul != null && _eul.CanBeCasted() && Menu.Item("items").GetValue<AbilityToggler>().IsEnabled(_eul.Name))
                    {
                        TryToHideMyAss(me);
                        return;
                    }
                }
                _isIn = true;
                return;
            }
            if (!_isIn) return;
            if (_dagger == null || !_dagger.IsValid)
            {
                _dagger = me.FindItem("item_blink");
            }
            if (_forceStaff == null || !_forceStaff.IsValid)
            {
                _forceStaff = me.FindItem("item_force_staff");
            }
            var spellList = new List<string>
            {
                "puck_phase_shift",
                "slark_pounce",
                "mirana_leap"
            };
            var safeSpell = me.Spellbook.Spells.FirstOrDefault(x => spellList.Contains(x.Name));

            if (_dagger != null && _dagger.CanBeCasted() && Menu.Item("items").GetValue<AbilityToggler>().IsEnabled(_dagger.Name))
            {
                _dagger.UseAbility(Game.MousePosition);
                Utils.Sleep(400, "kek");
            }
            else if (_forceStaff != null && _forceStaff.CanBeCasted() && Menu.Item("items").GetValue<AbilityToggler>().IsEnabled(_forceStaff.Name))
            {
                _forceStaff.UseAbility(me);
                Utils.Sleep(400, "kek");
            }
            else if (_eul != null && _eul.CanBeCasted() && Menu.Item("items").GetValue<AbilityToggler>().IsEnabled(_eul.Name))
            {
                _eul.UseAbility(me);
                Utils.Sleep(400, "kek");
            }
            else if (safeSpell != null && safeSpell.CanBeCasted())
            {
                safeSpell.UseAbility();
                Utils.Sleep(400, "kek");  
            }
            _isIn = false;
        }

        private static void TryToHideMyAss(Hero me)
        {
            if (_eul != null && _eul.CanBeCasted() && Menu.Item("items").GetValue<AbilityToggler>().IsEnabled(_eul.Name))
            {
                _eul.UseAbility(me);
                Utils.Sleep(400, "kek");
                _isIn = false;
                return;
            }
            var spellList = new List<string>
            {
                "puck_phase_shift",
                "shadow_demon_disruption",
                "obsidian_destroyer_astral_imprisonment"
            };
            var spell = me.Spellbook.Spells.FirstOrDefault(x=>spellList.Contains(x.Name));
            if (spell == null) return;
            spell.UseAbility(me);
            spell.UseAbility();
            Utils.Sleep(400,"kek");
        }
    }
}
