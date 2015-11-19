using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using SharpDX;

namespace EarthSpirit
{
    internal class Program
    {
        #region Members
        //============================================================
        private static bool _loaded;
        private static readonly string Ver = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        //============================================================
        public static Ability Remnant;
        public static Ability Push;
        public static Ability Pull;
        public static Ability Roll;
        public static Ability Magnetize;
        private static int _stage;
        private static Hero _globalTarget;
        public static int Combo { get; set; }
        private static readonly Menu Menu = new Menu("Earth Spirit", "earthspirit", true);

        //============================================================
        enum DaggerStage
        {
            OnStartCombo,
            AfterSmash,
            Never
        }
        #endregion
        
        private static void Main()
        {
            Game.OnUpdate += Game_OnUpdate;
            _loaded = false;

            Menu.AddItem(
                new MenuItem("hotkey", "Combo hotkey").SetValue(new KeyBind('G', KeyBindType.Press))
                    .SetTooltip("just hold this key for combo"));
            Menu.AddItem(new MenuItem("asd", "for combo with Aghanim use CTRL").SetFontStyle(
                FontStyle.Bold,
                SharpDX.Color.Coral));
            Menu.AddItem(
                new MenuItem("pushKey", "SingleKey for SMASH").SetValue(new KeyBind('Z', KeyBindType.Press)));
            Menu.AddItem(
                new MenuItem("rollKey", "SingleKey for ROLL").SetValue(new KeyBind('X', KeyBindType.Press)));
            Menu.AddItem(
                new MenuItem("pullKey", "SingleKey for GRIP").SetValue(new KeyBind('C', KeyBindType.Press)));

            Menu.AddItem(new MenuItem("rolling", "Use Roll For Chasing").SetValue(true).SetTooltip("after combo"));

            Menu.AddItem(new MenuItem("supult", "Remnant For Ultimate").SetValue(true).SetTooltip("use remnant for supporting the ultimate (if combo is active)"));

            Menu.AddItem(new MenuItem("dagger", "Use Dagger").SetValue(new StringList(new[] { "on start combo", "after smash", "never" })));
            Menu.AddItem(new MenuItem("dsa", "only if you so far from enemy").SetFontStyle(
                FontStyle.Bold,
                SharpDX.Color.Coral));

            Menu.AddItem(new MenuItem("items", "Use Items").SetValue(true));

            Menu.AddItem(new MenuItem("killsteal", "Smash Stealer").SetValue(true));

            Menu.AddItem(new MenuItem("debug", "Print Debug Messages").SetValue(false));
            Menu.AddToMainMenu();
        }

        private static void LetsPull()
        {
            var me = ObjectMgr.LocalHero;
            if (!Remnant.CanBeCasted() || !Pull.CanBeCasted() || !Utils.SleepCheck("preComboW8")) return;
            var pos = Game.MousePosition;
            if (AnyStoneNear(me, pos))
            {

            }
            else if (Remnant.CanBeCasted())
            {
                Remnant.UseAbility(pos);
            }
            else
            {
                return;
            }
            Pull.UseAbility(pos);
            Utils.Sleep(500,"preComboW8");
        }

        private static void LetsRoll()
        {
            if (!Roll.CanBeCasted() || !Utils.SleepCheck("preComboW8")) return;
            var me = ObjectMgr.LocalHero;
            if (AnyStoneNear(me))
            {

            }
            else if (Remnant.CanBeCasted())
            {
                var ang = me.FindAngleBetween(Game.MousePosition, true);
                var p = new Vector2((float)(me.Position.X + 200 * Math.Cos(ang)), (float)(me.Position.Y + 100 * Math.Sin(ang)));
                Remnant.UseAbility(p.ToVector3(true));
            }
            else
            {
                return;
            }
            Roll.UseAbility(Game.MousePosition);
            Utils.Sleep(500, "preComboW8");
        }

        private static void LetsPush()
        {
            var me = ObjectMgr.LocalHero;
            if (!Push.CanBeCasted() || !Utils.SleepCheck("preComboW8")) return;
            if (AnyStoneNear(me,new Vector3(),200F))
            {
                
            }
            else if (Remnant.CanBeCasted())
            {
                var ang = me.FindAngleBetween(Game.MousePosition, true);
                var p = new Vector2((float)(me.Position.X + 50 * Math.Cos(ang)), (float)(me.Position.Y + 50 * Math.Sin(ang)));
                Remnant.UseAbility(p.ToVector3(true));
            }
            else
            {
                return;
            }
            var pos = Game.MousePosition;
            if (AnyStoneNear(me, pos))
            {
                var ang = me.FindAngleBetween(pos, true);
                pos = new Vector2((float)(pos.X + 300 * Math.Cos(ang)), (float)(pos.Y + 300 * Math.Sin(ang))).ToVector3(true);
            }
            Push.UseAbility(pos);
            Utils.Sleep(500, "preComboW8");
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            #region Init

            var me = ObjectMgr.LocalHero;
            if (!_loaded)
            {
                if (!Game.IsInGame || me == null || me.ClassID != ClassID.CDOTA_Unit_Hero_EarthSpirit)
                {
                    return;
                }
                _loaded = true;

                PrintSuccess(string.Format("> EarthSpirit Annihilation Loaded v{0}", Ver));

                Remnant = me.FindSpell("earth_spirit_stone_caller");
                Push = me.FindSpell("earth_spirit_boulder_smash");
                Pull = me.FindSpell("earth_spirit_geomagnetic_grip");
                Roll = me.FindSpell("earth_spirit_rolling_boulder");
                Magnetize = me.FindSpell("earth_spirit_magnetize");
            }

            if (!Game.IsInGame || me == null)
            {
                _loaded = false;
                PrintInfo("> EarthSpirit unLoaded");
                return;
            }

            if (Game.IsPaused)
            {
                return;
            }
            #endregion

            #region Lets combo

            if (Menu.Item("pushKey").GetValue<KeyBind>().Active)
            {
                LetsPush();
                if (Menu.Item("debug").GetValue<bool>()) PrintInfo("Lets push");
            }
            else if (Menu.Item("rollKey").GetValue<KeyBind>().Active)
            {
                LetsRoll();
                if (Menu.Item("debug").GetValue<bool>()) PrintInfo("Lets roll");
            }
            else if (Menu.Item("pullKey").GetValue<KeyBind>().Active)
            {
                LetsPull();
                if (Menu.Item("debug").GetValue<bool>()) PrintInfo("Lets smash");
            }

            if (!Menu.Item("hotkey").GetValue<KeyBind>().Active)
            {
                _globalTarget = null;
                return;
            }

            if (_globalTarget == null || !_globalTarget.IsValid)
            {
                _globalTarget = ClosestToMouse(me, 150);
                _stage = 0;
            }
            if (_globalTarget == null || !_globalTarget.IsValid || !_globalTarget.IsAlive || !me.CanCast()) return;
            if (Game.IsKeyDown(0x11))
            {
                LetAghanimCombo(me, _globalTarget);
            }
            else
            {
                ComboInAction(me, _globalTarget);
            }

            #endregion
        }

        private static void LetAghanimCombo(Hero me,Hero target)
        {
            if (!Utils.SleepCheck("nextAction")) return;
            var ability = me.FindSpell("earth_spirit_petrify");
            if (ability==null) return;
            if (ability.Level==0) return;

            var inStone = target.Modifiers.Any(x=>x.Name=="modifier_earthspirit_petrify");

            var dist = me.Distance2D(target);
            if (ability.CanBeCasted() && !inStone)
            {
                if (Menu.Item("dagger").GetValue<StringList>().SelectedIndex == (int)DaggerStage.OnStartCombo || Menu.Item("dagger").GetValue<StringList>().SelectedIndex == (int)DaggerStage.AfterSmash)
                {
                    var blink = me.FindItem("item_blink");
                    if (dist >= ability.CastRange && blink!=null && blink.CanBeCasted())
                    {
                        var ang = me.FindAngleBetween(target.Position, true);
                        var p = new Vector2((float)(target.Position.X - 100 * Math.Cos(ang)), (float)(target.Position.Y - 100 * Math.Sin(ang)));
                        blink.UseAbility(p.ToVector3(true));
                    }
                }
                ability.UseAbility(target);
                Utils.Sleep(150 + ability.FindCastPoint(), "nextAction");
                return;
            }
            if (Pull != null && Pull.CanBeCasted() && inStone)
            {
                Pull.UseAbility(target.Position);
                //PrintInfo(Roll.CastSkillShot(target).ToString());
                Utils.Sleep(300 + Pull.FindCastPoint(), "nextAction");
                return;
            }
            if (Push != null && Push.CanBeCasted() && inStone && dist<=150)
            {
                Push.UseAbility(Game.MousePosition);
                if (Roll != null && Roll.CanBeCasted())
                {
                    if (Remnant != null && Remnant.CanBeCasted())
                    {
                        var ang = me.FindAngleBetween(Game.MousePosition, true);
                        var p = new Vector2((float) (me.Position.X + 100 * Math.Cos(ang)), (float) (me.Position.Y + 100 * Math.Sin(ang)));
                        Remnant.UseAbility(p.ToVector3(true));
                    }
                    Roll.UseAbility(Game.MousePosition,true);
                }
                Utils.Sleep(300 + Push.FindCastPoint(), "nextAction");
            }
        }

        private static void ComboInAction(Hero me, Hero target)
        {
            
            if (!Utils.SleepCheck("nextAction")) return;
            var dist = me.Distance2D(target);
            switch (_stage)
            {
                case 0:
                    if (target.Modifiers.Any(x => x.Name == "modifier_earth_spirit_magnetize") && !CanCastCombo()) { _stage = 5; }
                    if (Menu.Item("dagger").GetValue<StringList>().SelectedIndex == (int)DaggerStage.OnStartCombo)
                    {
                        var blink = me.FindItem("item_blink");
                        if (dist >= Pull.CastRange && blink != null && blink.CanBeCasted())
                        {
                            if (dist >= Pull.CastRange + 1100)
                            {
                                me.Move(target.Position);
                                Utils.Sleep(200, "nextAction");
                                break;
                            }
                            var ang = me.FindAngleBetween(target.Position, true);
                            var p = new Vector2((float)(me.Position.X + 1100 * Math.Cos(ang)), (float)(me.Position.Y + 1100 * Math.Sin(ang)));
                            blink.UseAbility(p.ToVector3(true));
                            Utils.Sleep(100, "nextAction");
                            break;
                        }
                    }
                    if (AnyStoneNear(me) && dist <= 1900)
                    {
                        if (Menu.Item("debug").GetValue<bool>()) PrintInfo("stone near you finded");
                        _stage++;
                        break;
                    }
                    if (Remnant.CanBeCasted())
                    {
                        if (dist <= 1900)
                        {
                            if (me.NetworkActivity == NetworkActivity.Move)
                                me.Stop();
                            Remnant.UseAbility(Prediction.InFront(me, 100));
                            Utils.Sleep(50 + Remnant.FindCastPoint(), "nextAction");
                            _stage++;
                            if (Menu.Item("debug").GetValue<bool>()) PrintInfo("remnant create");
                        }
                        else
                        {
                            me.Move(target.Position);
                            Utils.Sleep(50 + Remnant.FindCastPoint(), "nextAction");
                        }

                    }
                    break;
                case 1:
                    if (Push.CanBeCasted())
                    {
                        var last = GetLastRemnant(me);

                        if (last != null)
                        {
                            if (Menu.Item("debug").GetValue<bool>()) PrintInfo("push casted");
                            Push.UseAbility(target.Position);
                            Utils.Sleep(100 + Push.FindCastPoint(), "nextAction");
                        }
                    }
                    else
                        _stage++;
                    break;
                case 2:
                    if (Pull.CanBeCasted())
                    {
                        var last = GetLastRemnant(me);
                        if (last != null)
                        {
                            if (target.Distance2D(last) <= 200)
                            {
                                if (me.Distance2D(target) <= Pull.CastRange)
                                {
                                    Pull.UseAbility(target.Position);
                                    //PrintInfo("last pos: "+last.Position.X);
                                    if (Menu.Item("debug").GetValue<bool>()) PrintInfo("pull casted");
                                    Utils.Sleep(100 + Pull.FindCastPoint(), "nextAction");

                                }
                                else /*if (_shouldUseDagger)*/
                                {
                                    var blink = me.FindItem("item_blink");
                                    if (dist >= Pull.CastRange && blink != null && blink.CanBeCasted())
                                    {
                                        if (dist >= Pull.CastRange + 1100)
                                        {
                                            me.Move(target.Position);
                                            Utils.Sleep(200, "nextAction");
                                            break;
                                        }
                                        if (Menu.Item("dagger").GetValue<StringList>().SelectedIndex ==
                                            (int) DaggerStage.AfterSmash)
                                        {
                                            var ang = me.FindAngleBetween(target.Position, true);
                                            var p = new Vector2((float) (me.Position.X + 1100*Math.Cos(ang)),
                                                (float) (me.Position.Y + 1100*Math.Sin(ang)));
                                            blink.UseAbility(p.ToVector3(true));
                                            Utils.Sleep(100, "nextAction");
                                            if (Menu.Item("debug").GetValue<bool>()) PrintInfo("dagger is used");
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (Remnant.CanBeCasted())
                            {
                                if (me.NetworkActivity == NetworkActivity.Move)
                                    me.Stop();
                                Remnant.UseAbility(target.Position);
                                if (Menu.Item("debug").GetValue<bool>()) PrintInfo("remnant create");

                            }
                        }
                    }
                    else
                        _stage++;
                    break;
                case 3:
                    if (Roll.CanBeCasted() && !Pull.CanBeCasted())
                    {
                        Roll.UseAbility(target.Position);
                        Utils.Sleep(100 + Roll.FindCastPoint(), "nextAction");
                        if (Menu.Item("debug").GetValue<bool>()) PrintInfo("roll casted");
                    }
                    else
                        _stage++;
                    break;
                case 4:
                    if (Magnetize.CanBeCasted())
                    {
                        if (me.Distance2D(target) < 300)
                        {
                            Magnetize.UseAbility();
                            Utils.Sleep(100 + Magnetize.FindCastPoint(), "nextAction");
                            _stage++;
                            if (Menu.Item("debug").GetValue<bool>()) PrintInfo("Magnetize casted");
                        }
                    }
                    break;
                case 5:
                    if (Remnant.CanBeCasted() && Menu.Item("supult").GetValue<bool>())
                    {
                        var mod = target.Modifiers.FirstOrDefault(x => x.Name == "modifier_earth_spirit_magnetize");
                        if (mod != null && mod.RemainingTime <= 0.5+Game.Ping && me.Distance2D(target)<=Remnant.CastRange)
                        {
                            Remnant.UseAbility(target.Position);
                            Utils.Sleep(1000, "nextAction");
                            me.Attack(target,true);
                            break;
                        }
                        if (Utils.SleepCheck("attackcd"))
                        {
                            me.Attack(target);
                            Utils.Sleep(150, "attackcd");
                        }
                    }
                    else if (Utils.SleepCheck("attackcd"))
                    {
                        me.Attack(target);
                        Utils.Sleep(200, "attackcd");
                    }
                    if (Menu.Item("killsteal").GetValue<bool>() && Push.CanBeCasted() && target.DamageTaken(50 * Push.Level, DamageType.Magical, me) > target.Health)
                    {
                        Push.UseAbility(target);
                        Utils.Sleep(500, "nextAction");
                        me.Attack(target, true);
                        break;
                    }
                    if (Menu.Item("rolling").GetValue<bool>() && Roll.CanBeCasted())
                    {
                        Roll.UseAbility(target.Position);
                        Utils.Sleep(500, "nextAction");
                    }
                    break;
                    
            }
            LetsUseItems(me, target);
        }

        private static void LetsUseItems(Hero me, Hero target)
        {
            if (Push!=null && Push.CanBeCasted()) return;
            var itemOnTarget =
                me.Inventory.Items.FirstOrDefault(
                    x =>
                        (x.Name == "item_abyssal_blade" || x.Name == "item_orchid" ||
                         x.Name == "item_heavens_halberd" || x.Name == "item_sheepstick" ||
                         (x.Name == "item_urn_of_shadows" && x.CurrentCharges>0)|| x.Name == "item_medallion_of_courage" ||
                         x.Name == "item_solar_crest") && x.CastRange >= me.Distance2D(target) && x.CanBeCasted() &&
                        Utils.SleepCheck(x.GetHashCode().ToString()));
            //Game.PrintMessage(me.FindItem("item_solar_crest").CastRange.ToString(), MessageType.LogMessage);
            if (itemOnTarget!=null)
            {
                itemOnTarget.UseAbility(target);
                Utils.Sleep(300,itemOnTarget.GetHashCode().ToString());
            }
            var dagon = me.GetDagon();
            if (dagon != null && dagon.CanBeCasted(target) && me.Distance2D(target) <= dagon.CastRange)
            {
                dagon.UseAbility(target);
            }
        }


        private static bool CanCastCombo()
        {
            return Push.CanBeCasted() && Pull.CanBeCasted() && Roll.CanBeCasted();
        }

        private static bool AnyStoneNear(Hero me,Vector3 pos=new Vector3(),float range=150)
        {
            if (pos.IsZero)
                return ObjectMgr.GetEntities<Unit>()
                    .Any(
                        x =>
                            x.ClassID == ClassID.CDOTA_Unit_Earth_Spirit_Stone && x.Team == me.Team &&
                            x.Distance2D(me) <= range && x.IsAlive && x.IsValid);
            return ObjectMgr.GetEntities<Unit>()
                .Any(
                    x =>
                        x.ClassID == ClassID.CDOTA_Unit_Earth_Spirit_Stone && x.Team == me.Team &&
                        x.Distance2D(pos) <= range);

        }

        private static Unit GetLastRemnant(Hero me)
        {
            try
            {
                var stones = ObjectMgr.GetEntities<Unit>()
                .Where(
                    x =>
                        x.ClassID == ClassID.CDOTA_Unit_Earth_Spirit_Stone && x.Team == me.Team);
                Unit[] last = { null };
                foreach (var enemyHero in stones.Where(enemyHero => last[0] == null || last[0].Modifiers.First().RemainingTime <= enemyHero.Modifiers.First().RemainingTime))
                {
                    last[0] = enemyHero;
                }
                return last[0];
            }
            catch (Exception)
            {

                return null;
            }
            
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