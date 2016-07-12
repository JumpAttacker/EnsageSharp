using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Common.Objects.UtilityObjects;

namespace MorphlingAnnihilation
{
    internal static class Program
    {
        private static bool _loaded;
        private static readonly string Ver = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        private static readonly Menu Menu = new Menu("Morphling Annihilation", "morph", true, "npc_dota_hero_morphling",true);
        private static readonly Dictionary<Unit,Orbwalker> OrbDictinary=new Dictionary<Unit, Orbwalker>();
        enum Orders
        {
            Fight,
            GoBack,
            DoNothing
        };

        static void Main()
        {
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += OnUpdate;
            Menu.AddItem(new MenuItem("hotkey", "HotKey").SetValue(new KeyBind('F', KeyBindType.Press)));
            var dick = new Dictionary<string, bool>
            {
                {"item_ethereal_blade",true},
                {"morphling_adaptive_strike",true},
                {"item_dagon",true},
                {"morphling_waveform",true}
            };
            Menu.AddItem(new MenuItem("Use", "List of Use").SetValue(new AbilityToggler(dick)));
            Menu.AddItem(new MenuItem("replicateAction", "Replicate").SetValue(new StringList(new[] { "Fight", "Go Back", "Do nothing" })));
            Menu.AddItem(new MenuItem("hybridAction", "Hybrid").SetValue(new StringList(new[] { "Fight", "Go Back","Do nothing" })));

            var autoBalance = new Menu("Auto balance", "Auto balance", false, "morphling_morph_agi", true);
            autoBalance.AddItem(new MenuItem("autoBalance", "Auto balance").SetValue(true));
            autoBalance.AddItem(new MenuItem("minHp", "Minimum HP").SetValue(new Slider(100, 100, 5000)));
            autoBalance.AddItem(new MenuItem("minMp", "Minimum MP percent").SetValue(new Slider(0)));
            Menu.AddSubMenu(autoBalance);

            var safetp = new Menu("Safe Tp", "Safetpout", false, "morphling_morph_replicate", true);
            safetp.AddItem(new MenuItem("safetp", "Use replicate on low hp").SetValue(true));
            safetp.AddItem(new MenuItem("minHpForSafeTp", "Minimum HP").SetValue(new Slider(100, 100, 5000)));
            Menu.AddSubMenu(safetp);
            Menu.AddToMainMenu();
            Orbwalking.Load();
        }

        private static void OnUpdate(EventArgs args)
        {
            var me = ObjectManager.LocalHero;

            if (!_loaded)
            {

                if (!Game.IsInGame || me == null || me.ClassID!=ClassID.CDOTA_Unit_Hero_Morphling)
                {
                    return;
                }
                _loaded = true;
                Game.PrintMessage(
                    "<font face='Comic Sans MS, cursive'><font color='#00aaff'>" + Menu.DisplayName + " By Jumpering" +
                    " loaded!</font> <font color='#aa0000'>v" + Ver, MessageType.LogMessage);
            }

            if (!Game.IsInGame || me == null)
            {
                _loaded = false;
                return;
            }
            /*var spells = me.Spellbook.Spells.Where(x=>x.AbilityType==AbilityType.Ultimate);
            foreach (var ability in spells)
            {
                Game.PrintMessage(ability.Name,MessageType.ChatMessage);
            }*/
            
            var illus = ObjectManager.GetEntities<Hero>().Where(x => x.IsIllusion && x.IsControllable && x.IsAlive).ToArray();
            var replicate = illus.FirstOrDefault(x => x.HasModifier("modifier_morph_replicate"));
            var hybrid = illus.FirstOrDefault(x => x.HasModifier("modifier_morph_hybrid_special"));
            Hero target;
            if (replicate != null && Utils.SleepCheck("orderTimer"))
            {
                Orbwalker orb;
                if (!OrbDictinary.TryGetValue(replicate,out orb))
                {
                    OrbDictinary.Add(replicate,new Orbwalker(replicate));
                }
                var index = Menu.Item("replicateAction").GetValue<StringList>().SelectedIndex;
                switch (index)
                {
                    case (int)Orders.Fight:
                        target = ClosestToMouse(me, 350);
                        if (target != null)
                        {
                            if (orb != null)
                                orb.OrbwalkOn(target);
                            else
                                replicate.Attack(target);
                        }
                        else replicate.Move(Game.MousePosition);
                        break;
                    case (int)Orders.GoBack:
                        var fount =
                            ObjectManager.GetEntities<Unit>()
                                .FirstOrDefault(x => x.Team == me.Team && x.ClassID == ClassID.CDOTA_Unit_Fountain);
                        if (fount != null) replicate.Move(fount.Position);
                        break;
                }
                Utils.Sleep(300, "orderTimer");
            }

            
            if (hybrid != null && Utils.SleepCheck("orderTimer2"))
            {
                Orbwalker orb;
                if (!OrbDictinary.TryGetValue(hybrid, out orb))
                {
                    OrbDictinary.Add(hybrid, new Orbwalker(hybrid));
                }
                var index = Menu.Item("hybridAction").GetValue<StringList>().SelectedIndex;
                switch (index)
                {
                    case (int)Orders.Fight:
                        target = ClosestToMouse(me,350);
                        if (target != null && !hybrid.IsChanneling())
                        {
                            var anySpell =
                                hybrid.Spellbook.Spells.Any(
                                    x =>
                                        x.CanBeCasted(target) && GetRealCastRange(x) > 0 &&
                                        x.AbilityType != AbilityType.Ultimate && x.AbilityBehavior != AbilityBehavior.Passive);
                            var spellForNow = hybrid.Spellbook.Spells.FirstOrDefault(
                                    x =>
                                        x.CanBeCasted(target) && GetRealCastRange(x) > 0 &&
                                        x.AbilityType != AbilityType.Ultimate && x.AbilityBehavior != AbilityBehavior.Passive && GetRealCastRange(x) >= hybrid.Distance2D(target));
                            if (anySpell)
                            {
                                if (spellForNow != null)
                                {
                                    spellForNow.UseAbility(target);
                                    spellForNow.UseAbility(target.Position);
                                    spellForNow.UseAbility();
                                }
                                else
                                    hybrid.Move(target.Position);
                            }
                            else
                                if (orb != null)
                                    orb.OrbwalkOn(target);
                                else
                                    hybrid.Attack(target);
                        }
                        else if (!hybrid.IsChanneling())
                            hybrid.Move(Game.MousePosition);
                        break;
                    case (int)Orders.GoBack:
                        var fount =
                            ObjectManager.GetEntities<Unit>()
                                .FirstOrDefault(x => x.Team == me.Team && x.ClassID == ClassID.CDOTA_Unit_Fountain);
                        if (fount != null) hybrid.Move(fount.Position);
                        break;
                }
                Utils.Sleep(300, "orderTimer2");
            }
            if (!me.IsAlive) return;
            if (Menu.Item("autoBalance").GetValue<bool>() && Utils.SleepCheck("trans"))
            {
                var toAgi = me.Spellbook.Spell3;
                var toStr = me.Spellbook.Spell4;
                var minHp = Menu.Item("minHp").GetValue<Slider>().Value;
                var minMp = Menu.Item("minMp").GetValue<Slider>().Value;
                var curentHp = me.Health;
                var curentMp = me.Mana;
                if (toAgi != null && toAgi.CanBeCasted() && minMp <= curentMp)
                {
                    if (curentHp < minHp)
                    {
                        if (!me.HasModifier("modifier_morphling_morph_str"))
                        {
                            //Game.PrintMessage("need more hp", MessageType.ChatMessage);
                            toStr.ToggleAbility();
                        }
                    }
                    else if (me.HasModifier("modifier_morphling_morph_str"))
                    {
                        //Game.PrintMessage("disable hp", MessageType.ChatMessage);
                        toStr.ToggleAbility();
                    }
                }
                Utils.Sleep(150, "trans");
            }
            if (Menu.Item("safetp").GetValue<bool>() && Utils.SleepCheck("safetp"))
            {
                var safetp = me.FindSpell("morphling_morph_replicate");
                var repka = me.FindSpell("morphling_replicate");
                var minHp = Menu.Item("minHpForSafeTp").GetValue<Slider>().Value;
                var curentHp = me.Health;

                if (safetp != null && repka!=null && safetp.CanBeCasted() && minHp > curentHp && repka.Cooldown > 0)
                {
                    safetp.UseAbility();
                    Utils.Sleep(repka.Cooldown*1000, "safetp");
                }
            }
            
            if (!Menu.Item("hotkey").GetValue<KeyBind>().Active || !Utils.SleepCheck("comboAct")) return;

            target = ClosestToMouse(me,350);

            if (target == null)
            {
                if (Utils.SleepCheck("move_Cd"))
                {
                    Utils.Sleep(100, "move_Cd");
                    me.Move(Game.MousePosition);
                }
                return;
            }

            var eb = me.FindItem("item_ethereal_blade");
            var dist = me.Distance2D(target);
            if (eb != null && eb.CanBeCasted() && Menu.Item("Use").GetValue<AbilityToggler>().IsEnabled(eb.Name))
            {
                if (dist >= 800)
                {
                    me.Move(target.Position);
                    Utils.Sleep(150, "comboAct");
                    return;
                }
                eb.UseAbility(target);
                Utils.Sleep(100+Game.Ping, "comboAct");
                return;
            }
            var wave = me.Spellbook.Spell1;
            var strike = me.Spellbook.Spell2;
            var dagon = me.GetDagon();
            if (strike != null && strike.CanBeCasted() && dist <= strike.CastRange && Menu.Item("Use").GetValue<AbilityToggler>().IsEnabled(strike.Name))
            {
                if (dist >= strike.CastRange)
                {
                    me.Move(target.Position);
                    Utils.Sleep(150, "comboAct");
                    return;
                }
                strike.UseAbility(target);
                Utils.Sleep(Game.Ping + 350, "comboAct");
                return;
            }
            if (dagon != null && dagon.CanBeCasted() && Menu.Item("Use").GetValue<AbilityToggler>().IsEnabled(dagon.Name.Substring(0,10)))
            {
                if (dist >= dagon.CastRange)
                {
                    me.Move(target.Position);
                    Utils.Sleep(150, "comboAct");
                    return;
                }
                dagon.UseAbility(target);
                Utils.Sleep(Game.Ping + 100, "comboAct");
                return;
            }
            if (wave != null && wave.CanBeCasted() && Menu.Item("Use").GetValue<AbilityToggler>().IsEnabled(wave.Name))
            {
                wave.UseAbility(target.Position);
                me.Attack(target, true);
                Utils.Sleep(800 + Game.Ping, "comboAct");
                return;
            }
            Orbwalking.Orbwalk(target);
            //me.Move(Game.MousePosition);
            Utils.Sleep(250 + Game.Ping, "comboAct");
        }

        private static float GetRealCastRange(Ability ability)
        {
            var range = ability.CastRange;
            if (range >= 1) return range;
            var data = ability.AbilityData.FirstOrDefault(x => x.Name.Contains("radius") || (x.Name.Contains("range") && !x.Name.Contains("ranged")));
            if (data == null) return range;
            var level = ability.Level == 0 ? 0 : ability.Level - 1;
            range = (uint) (data.Count > 1 ? data.GetValue(level) : data.Value);
            return range;
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            
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
    }
}
