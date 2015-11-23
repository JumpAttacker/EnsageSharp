using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using SharpDX;
using Color = SharpDX.Color;

namespace Techies_Annihilation
{
    internal class Program
    {
        #region Members

        private static bool _loaded;
        private static readonly Dictionary<Unit, ParticleEffect> Effects = new Dictionary<Unit, ParticleEffect>();
        private static readonly Dictionary<Unit, ParticleEffect> Visible = new Dictionary<Unit, ParticleEffect>();
        private static readonly Dictionary<Unit, float> GlobalHealthAfterSuicide = new Dictionary<Unit, float>();
        private static readonly Dictionary<Unit, float> GlobalHealthAfterMines = new Dictionary<Unit, float>();
        private static readonly Menu Menu = new Menu("Techies Annihilation", "Techies Annihilation", true, "npc_dota_hero_techies",true);
        private static readonly string Ver = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        private static ParticleEffect _forceStaffRange;
        private static readonly Dictionary<Unit, float> BombDamage = new Dictionary<Unit, float>();
        private static float _currentBombDamage;
        private static float _currentSuicDamage;
        public static uint LvlSpell3 { get; private set; }
        public static uint LvlSpell6 { get; private set; }
        public static bool ExtraMenu { get; set; }
        private static Item _aghStatus;
        private static readonly Dictionary<uint, Hero> PlayersDictionary = new Dictionary<uint, Hero>();
        //private static readonly Dictionary<Unit, int> DataWithMaxHealth = new Dictionary<Unit, int>();
        #endregion
        #region Methods

        #region Init

        private static void Main()
        {
            Game.OnUpdate += Game_OnUpdate;
            _loaded = false;
            Drawing.OnDraw += Drawing_OnDraw;
            Menu.AddItem(new MenuItem("AutoDetonate", "Auto Detonate").SetValue(true));
            Menu.AddItem(new MenuItem("AutoSuicide", "Auto Suicide").SetValue(false));
            Menu.AddItem(new MenuItem("DetonateDang", "Detonate hero with Aegis").SetValue(false).SetTooltip("or similar"));



            var force = new Menu("Forcestaff", "forcestaff", false, "item_force_staff", true);
            force.AddItem(new MenuItem("forceHotkey", "Hotkey For Auto ForceStaff").SetValue(new KeyBind(0x11, KeyBindType.Press)));
            force.AddItem(new MenuItem("showForceRange", "Auto forcestaff All TIme").SetValue(true));
            Menu.AddSubMenu(force);

            

            var extraMenu = new Menu("Hero Panel", "extraMenu");
            extraMenu.AddItem(new MenuItem("moveHotkey", "Hotkey For Moving").SetValue(new KeyBind(0x2D, KeyBindType.Toggle)).SetTooltip("toggle button for moving hero panel"));
            extraMenu.AddItem(new MenuItem("showPanel", "Show Hero Panel").SetValue(true));
            extraMenu.AddItem(new MenuItem("posX", "Hero Panel Position (x)").SetValue(new Slider(10, -2000, 2000)));
            extraMenu.AddItem(new MenuItem("posY", "Hero Panel Position (y)").SetValue(new Slider(200, -2000, 2000)));
            Menu.AddSubMenu(extraMenu);



            var other = new Menu("Other", "other");
            other.AddItem(new MenuItem("showDmgFromUlt", "Show Damage from ultimate").SetValue(true).SetTooltip("on hero"));
            other.AddItem(new MenuItem("showDmgFromSuic", "Show Damage from suicide").SetValue(true).SetTooltip("on hero"));
            other.AddItem(new MenuItem("fontSize", "Font Size on hero").SetValue(new Slider(15,1,30)));
            Menu.AddSubMenu(other);


            Menu.AddToMainMenu();
        }

        #endregion
        

        private static void Drawing_OnDraw(EventArgs args)
        {
            var me = ObjectMgr.LocalHero;

            if (me==null || !me.IsValid) return;
            if (Game.IsPaused || !Game.IsInGame || !_loaded || !Menu.Item("showPanel").GetValue<bool>())
            {
                return;
            }
            if (Menu.Item("moveHotkey").GetValue<KeyBind>().Active)
            {
                Menu.Item("posX").SetValue(new Slider((int)Game.MouseScreenPosition.X, -2000, 2000));
                Menu.Item("posY").SetValue(new Slider((int)Game.MouseScreenPosition.Y, -2000, 2000));
            }
            var percent = HUDInfo.RatioPercentage();
            var pos = new Vector2(Menu.Item("posX").GetValue<Slider>().Value, Menu.Item("posY").GetValue<Slider>().Value);
            var size=new Vector2(200*percent,300*percent);
            Drawing.DrawRect(pos, size, new Color(0, 0, 0, 100));
            Drawing.DrawRect(pos, size, new Color(0, 155, 255, 255), true);
            Drawing.DrawLine(pos + new Vector2(0, size.Y / 7), pos + new Vector2(size.X, size.Y / 7), new Color(0, 155, 255, 255));
            Drawing.DrawText("Techies Annihilation", pos + new Vector2(10, 10), new Vector2(20 * percent, 0), Color.LightBlue, FontFlags.AntiAlias | FontFlags.DropShadow);
            Drawing.DrawText("v " + Ver, pos + new Vector2(10, size.Y / 9), new Vector2(10 * percent, 0), Color.LightBlue, FontFlags.AntiAlias | FontFlags.DropShadow);
            var spellPos = pos + new Vector2(size.X/5 + 25, size.Y/7 + 10);
            var spellSize = new Vector2(size.X/8, size.Y/12);
            Drawing.DrawRect(spellPos, spellSize, new Color(0,0,0,155));
            Drawing.DrawRect(spellPos + new Vector2(spellSize.X + 20 * percent, 0), spellSize, new Color(0, 0, 0, 155));
            Drawing.DrawRect(spellPos + new Vector2(spellSize.X * 2 + 40 * percent, 0), spellSize, new Color(0, 0, 0, 155));
            Drawing.DrawRect(spellPos, spellSize, Drawing.GetTexture("materials/ensage_ui/other/npc_dota_techies_land_mine" + ".vmat"));
            Drawing.DrawRect(spellPos + new Vector2(spellSize.X + 20 * percent, 0), spellSize, Drawing.GetTexture("materials/ensage_ui/spellicons/techies_suicide" + ".vmat"));
            Drawing.DrawRect(spellPos + new Vector2(spellSize.X * 2 + 40 * percent, 0), spellSize, Drawing.GetTexture("materials/ensage_ui/other/npc_dota_techies_remote_mine" + ".vmat"));
            var i = 0;
            foreach (var v in from hero in PlayersDictionary select hero.Value into v where v != null where v.IsValid where v.Team != me.Team select v)
            {
                i++;
                var start = HUDInfo.GetHPbarPosition(v) + new Vector2(0, (float) (-HUDInfo.GetHpBarSizeY()*1.5));

                if (v.IsAlive)
                {
                    if (Menu.Item("showDmgFromUlt").GetValue<bool>())
                    {
                        float minesDmg;
                        var dmgAfterUlt = string.Format("{0}",
                            GlobalHealthAfterMines.TryGetValue(v, out minesDmg) ? (object) (int) minesDmg : "-");
                        Drawing.DrawText(dmgAfterUlt, start + new Vector2(-10, 0),
                            new Vector2(Menu.Item("fontSize").GetValue<Slider>().Value, 0), Color.White,
                            FontFlags.AntiAlias | FontFlags.DropShadow);
                    }
                    if (Menu.Item("showDmgFromSuic").GetValue<bool>())
                    {
                        float suicide;
                        var dmgAfterSuic = string.Format("{0}",
                            GlobalHealthAfterSuicide.TryGetValue(v, out suicide) ? (object) (int) suicide : "-");
                        Drawing.DrawText(dmgAfterSuic, start + new Vector2(HUDInfo.GetHPBarSizeX(), 0),
                            new Vector2(Menu.Item("fontSize").GetValue<Slider>().Value, 0), Color.White,
                            FontFlags.AntiAlias | FontFlags.DropShadow);
                    }
                }
                Drawing.DrawRect(pos + new Vector2(10, size.Y/7 + 10 + i*(size.Y/10 + 5)*percent),
                    new Vector2(size.X/5, size.Y/10),
                    Drawing.GetTexture("materials/ensage_ui/heroes_horizontal/" +
                                       v.Name.Substring("npc_dota_hero_".Length) + ".vmat"));
                Drawing.DrawRect(pos + new Vector2(10, size.Y/7 + 10 + i*(size.Y/10 + 5)*percent),
                    new Vector2(size.X/5, size.Y/10), new Color(0, 0, 0, 255), true);

                var ultDmg =
                    string.Format("{0}/{1}",
                        !v.IsAlive
                            ? 0
                            : Math.Abs(_currentBombDamage) <= 0 ? 0 : GetCount(v, v.Health, _currentBombDamage, me),
                        Math.Abs(_currentBombDamage) <= 0
                            ? 0
                            : GetCount(v, v.MaximumHealth, _currentBombDamage, me));
                Drawing.DrawText(ultDmg,
                    new Vector2(spellPos.X + spellSize.X*2 + 40*percent,
                        pos.Y + size.Y/7 + 10 + i*(size.Y/10 + 5)*percent), new Vector2(20*percent, 0), Color.LightBlue,
                    FontFlags.AntiAlias | FontFlags.DropShadow);
                var dummy = false;
                CanKillSuic(v, ref dummy, me);
                Drawing.DrawText(dummy ? "[+]" : "[-]",
                    new Vector2(spellPos.X + spellSize.X * 1 + 20 * percent,
                        pos.Y + size.Y / 7 + 10 + i * (size.Y / 10 + 5) * percent), new Vector2(20 * percent, 0), Color.LightBlue,
                    FontFlags.AntiAlias | FontFlags.DropShadow);

                Drawing.DrawText("-",
                    new Vector2(spellPos.X + 10 * percent,
                        pos.Y + size.Y / 7 + 10 + i * (size.Y / 10 + 5) * percent), new Vector2(20 * percent, 0), Color.LightBlue,
                    FontFlags.AntiAlias | FontFlags.DropShadow);
            }
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
                PrintSuccess(string.Format("> {1} Loaded v{0}", Ver, Menu.DisplayName));
                Game.PrintMessage(
                    "<font face='Comic Sans MS, cursive'><font color='#00aaff'>" + Menu.DisplayName + " By Jumpering" +
                    " loaded!</font> <font color='#aa0000'>v" + Ver, MessageType.LogMessage);
                PlayersDictionary.Clear();
            }

            if (!Game.IsInGame || me == null)
            {
                _loaded = false;
                PrintInfo(string.Format("> {0} unLoaded", Menu.DisplayName));
                return;
            }
            if (Game.IsPaused)
            {
                return;
            }
            SearchNewPlayers(me);
            try
            {

                #region UpdateInfo

                var ultimate = me.Spellbook.Spell6;
                var suic = me.Spellbook.Spell3;

                var bombLevel = (ultimate != null) ? ultimate.Level : 0;
                var suicideLevel = (suic != null) ? suic.Level : 0;

                if (LvlSpell3 != suicideLevel)
                {
                    Debug.Assert(suic != null, "suic != null");
                    var firstOrDefault = suic.AbilityData.FirstOrDefault(x => x.Name == "damage");
                    if (firstOrDefault != null)
                    {
                        _currentSuicDamage = firstOrDefault.GetValue(suicideLevel - 1);
                        //PrintError("_currentSuicDamage: " + _currentSuicDamage.ToString(CultureInfo.InvariantCulture));
                    }
                    LvlSpell3 = suicideLevel;
                }
                var agh = me.FindItem("item_ultimate_scepter");
                if (LvlSpell6 != bombLevel || !Equals(_aghStatus, agh))
                {
                    Debug.Assert(ultimate != null, "ultimate != null");
                    var firstOrDefault = ultimate.AbilityData.FirstOrDefault(x => x.Name == "damage");
                    if (firstOrDefault != null)
                    {
                        _currentBombDamage = firstOrDefault.GetValue(ultimate.Level - 1);
                        _currentBombDamage += agh != null
                            ? 150
                            : 0;
                        //PrintError("_currentBombDamage: " + _currentBombDamage.ToString(CultureInfo.InvariantCulture));
                    }
                    LvlSpell6 = bombLevel;
                    _aghStatus = agh;
                }

                #endregion

                var bombs = ObjectMgr.GetEntities<Unit>()
                    .Where(
                        x =>
                            x.ClassID == ClassID.CDOTA_NPC_TechiesMines && x.Team == me.Team);
                var bombsList = bombs as IList<Unit> ?? bombs.ToList();
                var enumerable = bombs as IList<Unit> ?? bombsList.ToList();
                //PrintError(Game.IsKeyDown(Key.RightCtrl).ToString());

                #region ForceStaffRange

                if (Game.IsKeyDown(Menu.Item("forceHotkey").GetValue<KeyBind>().Key) || Menu.Item("showForceRange").GetValue<bool>())
                {
                    if (_forceStaffRange == null)
                    {
                        _forceStaffRange = me.AddParticleEffect(@"particles\ui_mouseactions\range_display.vpcf");
                        _forceStaffRange.SetControlPoint(1, new Vector3(800, 0, 0));
                    }
                }
                else
                {
                    if (_forceStaffRange != null)
                    {
                        _forceStaffRange.Dispose();
                        _forceStaffRange = null;
                    }
                }

                #endregion

                foreach (var s in enumerable)
                {
                    //add effect
                    HandleEffect(s, s.Spellbook.Spell1 != null);


                    //Init bomb damage
                    if (!s.Spellbook.Spell1.CanBeCasted()) continue;
                    float dmg;
                    if (!BombDamage.TryGetValue(s, out dmg))
                    {
                        //PrintError("_currentBombDamage: "+_currentBombDamage.ToString());
                        BombDamage.Add(s, _currentBombDamage);
                    }

                }
                var enemies =
                    ObjectMgr.GetEntities<Hero>()
                        .Where(
                            x =>
                                x.Team != ObjectMgr.LocalPlayer.Team && !x.IsIllusion)
                        .ToList();
                var abilSuic = me.Spellbook.Spell3;
                var forcestaff = me.Inventory.Items.FirstOrDefault(x => x.ClassID == ClassID.CDOTA_Item_ForceStaff);
                foreach (var hero in from hero in PlayersDictionary let i = hero.Key select hero)
                {
                    try
                    {
                        var v = hero.Value;
                        if (v == null || v.Team == me.Team || Equals(v, me)) continue;
                        if (!Menu.Item("AutoDetonate").GetValue<bool>()) continue;
                        if (!Menu.Item("DetonateDang").GetValue<bool>())
                        {
                            var aegis = v.FindItem("item_aegis");
                            if (v.ClassID == ClassID.CDOTA_Unit_Hero_SkeletonKing)
                            {
                                var reinc = v.Spellbook.Spell4;
                                if (reinc != null && (int) reinc.Cooldown == 0)
                                {
                                    continue;
                                }
                            }
                            if (aegis != null)
                            {
                                continue;
                            }
                        }
                        var needToCastnew = new Dictionary<int, Ability>();
                        var inputDmg = 0f;
                        if (GlobalHealthAfterMines.ContainsKey(v))
                        {
                            GlobalHealthAfterMines.Remove(v);
                        }
                        var extraBombs = 0;
                        if (v.ClassID == ClassID.CDOTA_Unit_Hero_TemplarAssassin)
                        {
                            var refrection = v.Spellbook.Spell1;
                            if (refrection != null && refrection.Cooldown > 0)
                            {
                                extraBombs = (int) (2 + refrection.Level);
                                //PrintError("its templar: +" + extraBombs);
                            }
                        }
                        foreach (var b in bombsList)
                        {
                            float dmg;
                            if (!(v.Distance2D(b) <= 425) || !BombDamage.TryGetValue(b, out dmg) || !v.IsAlive ||
                                !b.Spellbook.Spell1.CanBeCasted() || !b.IsAlive) continue;
                            try
                            {
                                if (extraBombs > 0)
                                {
                                    extraBombs--;
                                }
                                else
                                {
                                    inputDmg += v.DamageTaken(dmg, DamageType.Magical, me, false);
                                }
                            }
                            catch
                            {
                                //PrintError("ErrorLevel 2");
                            }
                            needToCastnew.Add(needToCastnew.Count + 1, b.Spellbook.Spell1);
                            //PrintError("NEED: "+needToCastnew.Count);
                            var finalHealth = v.Health - inputDmg;
                            //PrintError(string.Format("{2}: inputDmg: {0} finalHealth: {1} (dmg: {3})", inputDmg, finalHealth, v.Name, dmg));
                            if (GlobalHealthAfterMines.ContainsKey(v))
                            {
                                GlobalHealthAfterMines.Remove(v);
                            }
                            GlobalHealthAfterMines.Add(v, finalHealth);
                            if (!(finalHealth <= 0)) continue;
                            foreach (
                                var ability in
                                    needToCastnew.Where(ability => Utils.SleepCheck(ability.Value.Handle.ToString())))
                            {
                                try
                                {
                                    ability.Value.UseAbility();
                                }
                                catch
                                {
                                    //PrintError("ErrorLevel 1");
                                }
                                Utils.Sleep(250, ability.Value.Handle.ToString());
                            }
                            break;
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
                foreach (var v in enemies.Where(v => me.IsAlive || v.IsAlive))
                {
                    if ((Game.IsKeyDown(Menu.Item("forceHotkey").GetValue<KeyBind>().Key) || Menu.Item("showForceRange").GetValue<bool>()) && forcestaff != null && forcestaff.CanBeCasted() &&
                        CheckForceStaff(v,me) && me.Distance2D(v) <= 800
                        /* && v.Modifiers.All(x => x.Name != "modifier_item_sphere_target")*/)
                    {
                        if (Utils.SleepCheck("force"))
                        {
                            forcestaff.UseAbility(v);
                            Utils.Sleep(250, "force");
                        }
                    }
                    if (!(me.Distance2D(v) <= 120) || abilSuic == null || !abilSuic.CanBeCasted() || !Menu.Item("AutoSuicide").GetValue<bool>())
                        continue;
                    if (CanKillSuic(v, me)) abilSuic.UseAbility(v);
                }
            }
            catch
            {
                // ignored
            }
        }

        #endregion

        
        #region Helpers

        private static void SearchNewPlayers(Hero me)
        {
            for (uint i = 0; i < 10; i++)
            {
                try
                {
                    var v = ObjectMgr.GetPlayerById(i).Hero;
                    if (v == null) continue;
                    if (!v.IsValid) continue;
                    if (v.Team == me.Team) continue;
                    PlayersDictionary.Add(i, v);
                }
                catch
                {
                    // ignored
                }
            }
        }

        private static bool CheckForceStaff(Hero hero,Hero me)
        {
            try
            {
                var pos = hero.NetworkPosition;
                pos.X += 600 * (float)Math.Cos(hero.RotationRad);
                pos.Y += 600 * (float)Math.Sin(hero.RotationRad);
                //PrintInfo(hero.FindAngleR().ToString(CultureInfo.InvariantCulture));
                var bombs = ObjectMgr.GetEntities<Unit>()
                    .Where(
                        x =>
                            x.ClassID == ClassID.CDOTA_NPC_TechiesMines && x.Team == me.Team &&
                            x.Spellbook.Spell1.CanBeCasted() &&
                            x.Distance2D(new Vector3(pos.X, pos.Y, hero.NetworkPosition.Z)) <= 425 && x.IsAlive);
                return (bombs.Count() >= GetCount(hero, hero.Health, _currentBombDamage, me));
            }
            catch
            {
                //PrintError("ErrorLevel 3");
                return false;
            }
            
        }

        private static void HandleEffect(Unit unit, bool isRange)
        {
            ParticleEffect effect;
            if (unit.IsAlive)
            {
                if (isRange)
                {
                    if (!Effects.TryGetValue(unit, out effect))
                    {
                        effect = unit.AddParticleEffect(@"particles\ui_mouseactions\range_display.vpcf");
                        effect.SetControlPoint(1, new Vector3(425, 0, 0));
                        Effects.Add(unit, effect);
                    }
                }
                if (unit.IsVisibleToEnemies)
                {
                    if (Visible.TryGetValue(unit, out effect)) return;
                    effect = unit.AddParticleEffect("particles/items_fx/aura_shivas.vpcf");
                    Visible.Add(unit, effect);
                }
                else
                {
                    if (!Visible.TryGetValue(unit, out effect)) return;
                    effect.Dispose();
                    Visible.Remove(unit);
                }
            }
            else
            {   //flush
                if (isRange)
                {
                    if (!Effects.TryGetValue(unit, out effect)) return;
                    effect.Dispose();
                    Effects.Remove(unit);
                    BombDamage.Remove(unit);
                }
                if (!Visible.TryGetValue(unit, out effect)) return;
                effect.Dispose();
                Visible.Remove(unit);
            }
        }

        private static string GetNameOfHero(Entity hero)
        {
            return hero.NetworkName.Substring(hero.NetworkName.LastIndexOf('_') + 1);
        }

        private static bool CanKillSuic(Unit target, Unit me)
        {
            float s;
            try
            {
                s = target.Health - target.DamageTaken(_currentSuicDamage, DamageType.Physical, me, true);
            }
            catch
            {
                //PrintError("ErrorLevel 4 (2)");
                s = 1;
            }
            return s <= 0;
        }

        private static string CanKillSuic(Unit target, ref bool killable, Unit me)
        {
            float s;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (target == null || target.Health <= 0 || _currentSuicDamage == 0 || !me.IsAlive) return " - ";
            try
            {
                s = target.Health - target.DamageTaken(_currentSuicDamage, DamageType.Physical, me, true);
            }
            catch
            {
                //PrintError("ErrorLevel 4");
                //PrintError(string.Format("me: {0} target{1} suicDamage: {2}", me.Name, target.Name, _currentSuicDamage));
                s = 1;
            }
            if (GlobalHealthAfterSuicide.ContainsKey(target))
            {
                GlobalHealthAfterSuicide.Remove(target);
            }
            GlobalHealthAfterSuicide.Add(target, s);
            
            killable = s <= 0;
            //return (killable) ? string.Format("Killable: {0:N}", s) : string.Format("need: {0:N}", s);
            return string.Format("{0:N}", s);
        }

        private static int GetCount(Unit v, uint health, float damage, Unit me)
        {
            var n = 0;
            float dmg = 0;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (v == null || health == 0 || damage == 0) return 30;
            try
            {
                do
                {
                    n++;
                    dmg += damage;
                    
                } while (health - v.DamageTaken(dmg, DamageType.Magical, me, false) > 0 && n < 30);
            }
            catch
            {
                //PrintError("ErrorLevel 5");
                // ignored
            }

            return n;
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

        private static bool CheckMouse(float x, float y, float sizeX, float sizeY)
        {
            var mousePos = Game.MouseScreenPosition;
            return mousePos.X >= x && mousePos.X <= x + sizeX && mousePos.Y >= y && mousePos.Y <= y + sizeY;
        }

        #endregion
    }
}
