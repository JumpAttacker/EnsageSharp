using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;

using SharpDX;
using SharpDX.Direct3D9;

namespace Techies_Annihilation
{
    internal class Program
    {
        #region Members

        private static bool _loaded;
        private static Hero _me;
        private static Player _player;
        private static readonly Dictionary<Unit, ParticleEffect> Effects = new Dictionary<Unit, ParticleEffect>();
        private static readonly Dictionary<Unit, ParticleEffect> Visible = new Dictionary<Unit, ParticleEffect>();
        private static ParticleEffect _forceStaffRange;
        private static readonly Dictionary<Unit, float> BombDamage = new Dictionary<Unit, float>();
        private static float _currentBombDamage;
        private static float _currentSuicDamage;
        public static uint LvlSpell3 { get; private set; }
        public static uint LvlSpell6 { get; private set; }
        public static bool ExtraMenu { get; set; }
        private static Item _aghStatus;
        private static Font _fontSize3;
        private static Font _fontSize1;
        private static Font _fontSize2;
        private static Line _line;
        //private static readonly Dictionary<Unit, int> DataWithCurrentHealth = new Dictionary<Unit, int>();
        //private static readonly Dictionary<Unit, int> DataWithMaxHealth = new Dictionary<Unit, int>();
        private static bool IsClose { get; set; }
        private static bool WithAll { get; set; }
        public static bool AutoDetonate = true;
        public static bool ShowForceStaffRange=true;
        public static bool AutoForceStaff;
        public static bool LegitMode;
        public static bool AutoSuicide;
        public static bool LeftMouseIsPress;
        #endregion
        #region Methods

        #region Init

        private static void Main()
        {
            Game.OnUpdate += Game_OnUpdate;
            _loaded = false;
            //Drawing.OnDraw += Drawing_OnDraw;

            #region Init font & line

            _fontSize3 = new Font(
                Drawing.Direct3DDevice9,
                new FontDescription
                {
                    FaceName = "Tahoma",
                    Height = 13,
                    OutputPrecision = FontPrecision.Default,
                    Quality = FontQuality.Default
                });
            _fontSize1 = new Font(
                Drawing.Direct3DDevice9,
                new FontDescription
                {
                    FaceName = "Tahoma",
                    Height = 20,
                    OutputPrecision = FontPrecision.Default,
                    Quality = FontQuality.Default
                });
            _fontSize2 = new Font(
                Drawing.Direct3DDevice9,
                new FontDescription
                {
                    FaceName = "Tahoma",
                    Height = 16,
                    OutputPrecision = FontPrecision.Default,
                    Quality = FontQuality.Default
                });
            _line = new Line(Drawing.Direct3DDevice9);
            #endregion

            Drawing.OnPreReset += Drawing_OnPreReset;
            Drawing.OnPostReset += Drawing_OnPostReset;
            Drawing.OnEndScene += Drawing_OnEndScene;
            AppDomain.CurrentDomain.DomainUnload += CurrentDomainDomainUnload;
            Game.OnWndProc += Game_OnWndProc;
        }

        #endregion
        
        #region Events

        #region !

        private static void CurrentDomainDomainUnload(object sender, EventArgs e)
        {
            _fontSize3.Dispose();
            _fontSize2.Dispose();
            _fontSize1.Dispose();
            _line.Dispose();
        }

        private static void Drawing_OnPostReset(EventArgs args)
        {
            _fontSize3.OnResetDevice();
            _fontSize1.OnResetDevice();
            _fontSize2.OnResetDevice();
            _line.OnResetDevice();
        }

        private static void Drawing_OnPreReset(EventArgs args)
        {
            _fontSize3.OnLostDevice();
            _fontSize2.OnLostDevice();
            _fontSize1.OnLostDevice();
            _line.OnLostDevice();
        }

        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (args.WParam != 1 || Game.IsChatOpen || !Utils.SleepCheck("clicker"))
            {
                LeftMouseIsPress = false;
                return;
            }
            LeftMouseIsPress = true;
            if (!IsClose && CheckMouse(145, 380, 60, 50))
            {
                WithAll = !WithAll;
                Utils.Sleep(250, "clicker");
            }
            if (IsClose ? CheckMouse(20, 380, 50, 50) : CheckMouse(20, 380, 120, 50))
            {
                IsClose = !IsClose;
                Utils.Sleep(250, "clicker");
            }
            if (!IsClose && CheckMouse(20, 435, 180, 25))
            {
                ExtraMenu = !ExtraMenu;
                Utils.Sleep(250, "clicker");
            }
        }

        #endregion

        private static void Drawing_OnEndScene(EventArgs args)
        {
            if (Drawing.Direct3DDevice9 == null || Drawing.Direct3DDevice9.IsDisposed || !Game.IsInGame)
            {
                return;
            }
            var player = ObjectMgr.LocalPlayer;
            if (player == null || player.Team == Team.Observer)
            {
                return;
            }
            /*var screenX = Drawing.Width / (float)1600 * (float)0.8;
            Drawing.DrawRect(new Vector2(10, 200), new Vector2((float)15 * screenX, (float)15 * screenX), Drawing.GetTexture(@"vgui\hud\minimap_creep.vmat"));
            Drawing.DrawRect(new Vector2(10, 300), new Vector2((float)15 * screenX, (float)15 * screenX), Drawing.GetTexture(@"vgui\hud\minimap_glow.vmat"));
             * vgui\dashboard\dash_button_back
             * 
             * */
            if (!IsClose)
            {
                DrawFilledBox(10, 200, 200, ExtraMenu?400:270, new ColorBGRA(0, 0, 0, 100));
                DrawLine(10,250,200,250,1,Color.Black);
                DrawShadowText("Damage Helper", 20, 210, Color.White,_fontSize1);
                var enemies =
                    ObjectMgr.GetEntities<Hero>()
                        .Where(
                            x =>
                                x.Team != player.Team && !x.IsIllusion && x.ClassID != ClassID.CDOTA_Unit_Hero_Meepo)
                        .ToList();
                var counter = 0;
                DrawFilledBox(20, 380, 120, 50,
                    CheckMouse(20, 380, 120, 50) ? new ColorBGRA(255, 255, 0, 100) : new ColorBGRA(100 ,255 ,0, 100));
                DrawShadowText("Open / Hide", 30, 390, Color.White,_fontSize1);
                DrawFilledBox(145, 380, 55, 50,
                    CheckMouse(145, 380, 55, 50) ? new ColorBGRA(255, 255, 0, 100) : WithAll ? new ColorBGRA(100, 255, 0, 100) : new ColorBGRA(255, 0, 0, 100));
                DrawShadowText(WithAll ? "AllHero" : "WithVis", 150, 390, Color.White,_fontSize2);

                DrawFilledBox(20, 435, 180, 25,
                    CheckMouse(20, 435, 180, 25) ? new ColorBGRA(255, 255, 0, 100) : ExtraMenu ? new ColorBGRA(100, 255, 0, 100) : new ColorBGRA(255, 0, 0, 100));
                if (!ExtraMenu)
                {
                    DrawLine(90, 440, 100, 450, 3, Color.Black);
                    DrawLine(100, 450, 110, 440, 3, Color.Black);
                }
                else
                {
                    DrawLine(100, 440, 90, 450, 3, Color.Black);
                    DrawLine(110, 450, 100, 440, 3, Color.Black);

                    
                    DrawShadowText("Auto detonate", 20, 470, Color.White, _fontSize2);
                    DrawShadowText("Auto suicide", 20, 490, Color.White, _fontSize2);
                    DrawShadowText("Legit mode", 20, 510, Color.White, _fontSize2);
                    DrawShadowText("Auto Force Staff", 20, 530, Color.White, _fontSize2);
                    DrawShadowText("Show Force Staff Range", 20, 550, Color.White, _fontSize2);

                    DrawButton(180, 470, 15, 15, 2, ref AutoDetonate,true);
                    DrawButton(180, 490, 15, 15, 2, ref AutoSuicide, true);
                    DrawButton(180, 510, 15, 15, 2, ref LegitMode,false);
                    DrawButton(180, 530, 15, 15, 2, ref AutoForceStaff, true);
                    DrawButton(180, 550, 15, 15, 2, ref ShowForceStaffRange, true);
                    
                }
                //DrawShadowText(WithAll ? "AllHero" : "WithVis", 150, 390, Color.White, _fontSize2);
                var dummy = false;
                if (!WithAll)
                {
                    foreach (var v in enemies)
                    {
                        DrawShadowText(
                            string.Format("  {0}: {3} | {1}/{2}", GetNameOfHero(v),
                                Math.Abs(_currentBombDamage) <= 0 ? 0 : GetCount(v, v.Health, _currentBombDamage),
                                Math.Abs(_currentBombDamage) <= 0 ? 0 : GetCount(v, v.MaximumHealth, _currentBombDamage),
                                CanKillSuic(v, ref dummy)),
                            10, 250 + 25*counter++,
                            Color.YellowGreen,
                            _fontSize2);
                    }
                }
                else
                {
                    for (uint i = 0; i < 10; i++)
                    {
                        try
                        {
                            var v = ObjectMgr.GetPlayerById(i).Hero;
                            if (v!=null && v.Team != _me.Team && !Equals(v, _me))
                            {
                                DrawShadowText(
                                    string.Format("  {0}: {3} | {1}/{2}", GetNameOfHero(v),
                                        Math.Abs(_currentBombDamage) <= 0
                                            ? 0
                                            : GetCount(v, v.Health, _currentBombDamage),
                                        Math.Abs(_currentBombDamage) <= 0
                                            ? 0
                                            : GetCount(v, v.MaximumHealth, _currentBombDamage),
                                        CanKillSuic(v, ref dummy)),
                                    10, 250 + 25*counter++,
                                    Color.YellowGreen,
                                    _fontSize2);
                            }

                        }
                        catch (Exception) //not all 10 players in a game!
                        {
                            //PrintError("ErrorLevel6 ");
                            // ignored
                        }
                    }
                }
            }
            else
            {
                DrawFilledBox(20, 380, 50, 50,
                    CheckMouse(20, 380, 50, 50) ? new ColorBGRA(255, 255, 0, 100) : new ColorBGRA(0, 0, 0, 100));
            }
        }

        


        private static void Game_OnUpdate(EventArgs args)
        {
            if (!_loaded)
            {
                _me = ObjectMgr.LocalHero;
                _player = ObjectMgr.LocalPlayer;
                if (!Game.IsInGame || _me == null || _me.ClassID != ClassID.CDOTA_Unit_Hero_Techies)
                {
                    return;
                }
                _loaded = true;
                PrintSuccess("> Techies Annihilation loaded!");
            }

            if (!Game.IsInGame || _me == null || _me.ClassID != ClassID.CDOTA_Unit_Hero_Techies)
            {
                _loaded = false;
                PrintInfo("> Techies Annihilation Unloaded!");
                return;
            }
            if (Game.IsPaused)
            {
                return;
            }

            if (_player == null || _player.Team == Team.Observer)
                return;

            #region UpdateInfo
            var ultimate = _me.Spellbook.Spell6;
            var suic = _me.Spellbook.Spell3;

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
            var agh = _me.FindItem("item_ultimate_scepter");
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
                        x.ClassID == ClassID.CDOTA_NPC_TechiesMines && x.Team == _player.Team);
            var bombsList = bombs as IList<Unit> ?? bombs.ToList();
            var enumerable = bombs as IList<Unit> ?? bombsList.ToList();
            //PrintError(Game.IsKeyDown(Key.RightCtrl).ToString());

            #region ForceStaffRange

            if ((Game.IsKeyDown(0x11) || AutoForceStaff)&& ShowForceStaffRange)
            {
                if (_forceStaffRange == null)
                {
                    _forceStaffRange = _me.AddParticleEffect(@"particles\ui_mouseactions\range_display.vpcf");
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
            var abilSuic = _me.Spellbook.Spell3;
            var forcestaff = _me.Inventory.Items.FirstOrDefault(x => x.ClassID == ClassID.CDOTA_Item_ForceStaff);
            foreach (var v in enemies)
            {
                if (AutoDetonate)
                {
                    var needToCastnew = new Dictionary<int, Ability>();
                    var inputDmg = 0f;
                    foreach (var b in bombsList)
                    {
                        float dmg;
                        if (!(v.Distance2D(b) <= 425) || !BombDamage.TryGetValue(b, out dmg) || !v.IsAlive ||
                            !b.Spellbook.Spell1.CanBeCasted() || !b.IsAlive) continue;
                        try
                        {
                            inputDmg += v.DamageTaken(dmg, DamageType.Magical, _me, false);
                        }
                        catch
                        {
                            PrintError("ErrorLevel2");
                        }
                        needToCastnew.Add(needToCastnew.Count + 1, b.Spellbook.Spell1);

                        var finalHealth = v.Health - inputDmg;
                        //PrintError(string.Format("{2}: inputDmg: {0} finalHealth: {1} (dmg: {3})", inputDmg, finalHealth, v.Name, dmg));
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
                                PrintError("ErrorLevel1");
                            }
                            Utils.Sleep(250, ability.Value.Handle.ToString());
                        }
                        break;
                    }
                }
                //var link = v.Inventory.Items.FirstOrDefault(x => x.Name == "item_sphere");
                //var linkCd = link != null?true:Math.Abs(link.Cooldown) >= 0;
                if (!_me.IsAlive && !v.IsAlive) continue;
                if ((Game.IsKeyDown(0x11) || AutoForceStaff)&&forcestaff != null && forcestaff.CanBeCasted() && 
                    CheckForceStaff(v) && _me.Distance2D(v) <= 800/* && v.Modifiers.All(x => x.Name != "modifier_item_sphere_target")*/)
                {
                    if (Utils.SleepCheck("force"))
                    {
                        forcestaff.UseAbility(v);
                        Utils.Sleep(250, "force");
                    }
                }
                if (!(_me.Distance2D(v) <= 120) || abilSuic == null || !abilSuic.CanBeCasted() || AutoSuicide) continue;
                if (CanKillSuic(v)) abilSuic.UseAbility(v);
            }
        }

        #endregion

        #endregion
        #region Drawing Methods
        public static void DrawCircle(int x, int y, int radius, int numSides, int thickness, Color color)
        {
            var vector2S = new Vector2[128];
            var step = (float)Math.PI * 2.0f / numSides;
            var count = 0;
            for (float a = 0; a < (float)Math.PI * 2.0; a += step)
            {
                var x1 = radius * (float)Math.Cos(a) + x;
                var y1 = radius * (float)Math.Sin(a) + y;
                var x2 = radius * (float)Math.Cos(a + step) + x;
                var y2 = radius * (float)Math.Sin(a + step) + y;
                vector2S[count].X = x1;
                vector2S[count].Y = y1;
                vector2S[count + 1].X = x2;
                vector2S[count + 1].Y = y2;

                DrawLine(x1, y1, x2, y2, thickness, color);
                count += 2;
            }
        }
        public static void DrawFilledBox(float x, float y, float w, float h, Color color)
        {
            var vLine = new Vector2[2];

            _line.GLLines = true;
            _line.Antialias = false;
            _line.Width = w;

            vLine[0].X = x + w / 2;
            vLine[0].Y = y;
            vLine[1].X = x + w / 2;
            vLine[1].Y = y + h;

            _line.Begin();
            _line.Draw(vLine, color);
            _line.End();
        }
        public static void DrawLine(float x1, float y1, float x2, float y2, float w, Color color)
        {
            var vLine = new[] { new Vector2(x1, y1), new Vector2(x2, y2) };

            _line.GLLines = true;
            _line.Antialias = false;
            _line.Width = w;

            _line.Begin();
            _line.Draw(vLine, color);
            _line.End();

        }
        public static void DrawShadowText(string stext, int x,int y, Color color, Font f)
        {
            f.DrawText(null, stext, x + 1, y + 1, Color.Black);
            f.DrawText(null, stext, x, y, color);
        }
        public static void DrawBox(float x, float y, float w, float h, float px, Color color)
        {
            DrawFilledBox(x, y + h, w, px, color);
            DrawFilledBox(x - px, y, px, h, color);
            DrawFilledBox(x, y - px, w, px, color);
            DrawFilledBox(x + w, y, px, h, color);
        }
        private static void DrawButton(float x, float y, float w, float h, float px,ref bool clicked,bool isActive)
        {
            if (isActive)
            {
                var isIn = CheckMouse(x, y, w, h);
                if (LeftMouseIsPress && Utils.SleepCheck("ClickButtonCd") && isIn)
                {
                    clicked = !clicked;
                    Utils.Sleep(250, "ClickButtonCd");
                }
                Color newColor = isIn
                    ? new ColorBGRA(255, 255, 0, 100)
                    : clicked ? new ColorBGRA(100, 255, 0, 100) : new ColorBGRA(255, 0, 0, 100);
                DrawFilledBox(x, y, w, h, newColor);
                DrawBox(x, y, w, h, px, Color.Black);
            }
            else
            {
                DrawFilledBox(x, y, w, h, Color.Gray);
                DrawBox(x, y, w, h, px, Color.Black); 
            }
        }
        #endregion
        #region Helpers

        private static bool CheckForceStaff(Hero hero)
        {
            try
            {
                var pos = hero.NetworkPosition;
                pos.X += 600 * (float)Math.Cos(hero.FindAngleR());
                pos.Y += 600 * (float)Math.Sin(hero.FindAngleR());
                var bombs = ObjectMgr.GetEntities<Unit>()
                    .Where(
                        x =>
                            x.ClassID == ClassID.CDOTA_NPC_TechiesMines && x.Team == _player.Team &&
                            x.Spellbook.Spell1.CanBeCasted() &&
                            x.Distance2D(new Vector3(pos.X, pos.Y, hero.NetworkPosition.Z)) <= 425 && x.IsAlive);
                return (bombs.Count() >= GetCount(hero, hero.Health, _currentBombDamage));
            }
            catch
            {
                PrintError("ErrorLevel3");
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

        private static bool CanKillSuic(Unit target)
        {
            float s;
            try
            {
                s = target.Health - target.DamageTaken(_currentSuicDamage, DamageType.Physical, _me, true);
            }
            catch
            {
                PrintError("ErrorLevel4 (2)");
                s = target.Health;
            }
            return s <= 0;
        }

        private static string CanKillSuic(Unit target,ref bool killable)
        {
            float s;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (target == null || target.Health <= 0 || _currentSuicDamage == 0) return " - ";
            try
            {
                s = target.Health - target.DamageTaken(_currentSuicDamage, DamageType.Physical, _me, true);
            }
            catch
            {
                PrintError("ErrorLevel4");
                //PrintError(string.Format("me: {0} target{1} suicDamage: {2}", _me.Name, target.Name, _currentSuicDamage));
                s = target.Health;
            }
            killable = s <= 0;
            return (killable) ? string.Format("Killable: {0:N}", s) : string.Format("need: {0:N}", s);
        }

        private static int GetCount(Unit v, uint health, float damage)
        {
            var n = 0;
            float dmg = 0;
            try
            {
                do
                {
                    n++;
                    dmg += damage;
                } while (health - v.DamageTaken(dmg, DamageType.Magical, _me, false) > 0 && n < 30);
            }
            catch
            {
                PrintError("ErrorLevel5");
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
