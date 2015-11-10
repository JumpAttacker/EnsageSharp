using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using SharpDX;
using SharpDX.Direct3D9;

// ReSharper disable EmptyGeneralCatchClause
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace OverlayInformationLight
{
    internal class Program
    {
        #region Members

        private static bool _loaded;
        private static Hero _me;
        private static readonly string Ver = Assembly.GetExecutingAssembly().GetName().Version+" light";
        private static Unit _arrowUnit;
        private static bool _isOpen=true;
        private static bool _leftMouseIsPress;
        private static readonly Dictionary<Unit, ParticleEffect> Effects2 = new Dictionary<Unit, ParticleEffect>();
        private static readonly Dictionary<Unit, ParticleEffect> Effects1 = new Dictionary<Unit, ParticleEffect>();
        private static readonly ParticleEffect[] ArrowParticalEffects=new ParticleEffect[150];
        private static readonly Dictionary<Hero, Ability> SearchAbilities = new Dictionary<Hero, Ability>();
        //======================================
        public static bool ShowHealthOnTopPanel = true;
        public static bool ShowManaOnTopPanel = true;
        public static bool ShowRoshanTimer = true;
        public static bool ShowIllusions = true;
        public static bool ShowMeMore= true;
        public static bool DangItems = true;
        public static bool AutoItemsMenu = true;
        public static bool AutoItemsActive = true;
        public static bool AutoItemsMidas = true;
        public static bool AutoItemsPhase = true;
        public static bool AutoItemsStick = true;
        private static readonly Dictionary<Unit, ParticleEffect> BaraIndicator = new Dictionary<Unit, ParticleEffect>();
        //=====================================
        private static readonly Dictionary<string, DotaTexture> TextureCache = new Dictionary<string, DotaTexture>();
        //=====================================
        private static readonly ShowMeMoreHelper[] ShowMeMoreH = new ShowMeMoreHelper[5];

        private static readonly Dictionary<Unit, ParticleEffect> ShowMeMoreEffect =
            new Dictionary<Unit, ParticleEffect>();

        private static readonly Dictionary<Unit, ParticleEffect>[] Eff = new Dictionary<Unit, ParticleEffect>[141];
        private static readonly List<Unit> InSystem = new List<Unit>();
        private static Vector3 _arrowS;
        /*private static Unit Bara;
        private static Vector3 BaraStartPos;*/
        //=====================================
        //private static readonly InitHelper SaveLoadSysHelper = new InitHelper("C:"+ "\\jOverlay.ini");
        //=====================================
        private static float _deathTime;
        private static double _roshanMinutes;
        private static double _roshanSeconds;
        private static bool _roshIsAlive;
        //=====================================
        private static readonly Font[] FontArray = new Font[21];
        private static Line _line;
        private static Vector2 _sizer = new Vector2(110, 500);
        private static bool _letsDraw = true;

        #endregion

        #region Methods

        #region Init

        private static void Main()
        {
            #region Init

            Game.OnUpdate += Game_OnUpdate;
            _loaded = false;
            Drawing.OnDraw += Drawing_OnDraw;

            #region Init font & line

            for (var i = 0; i <= 20; i++)
            {
                FontArray[i] = new Font(
                    Drawing.Direct3DDevice9,
                    new FontDescription
                    {
                        FaceName = "Tahoma",
                        Height = 10 + i,
                        OutputPrecision = FontPrecision.Default,
                        Quality = FontQuality.Default
                    });
            }
            _line = new Line(Drawing.Direct3DDevice9);

            #endregion

            Drawing.OnPreReset += Drawing_OnPreReset;
            Drawing.OnPostReset += Drawing_OnPostReset;
            Drawing.OnEndScene += Drawing_OnEndScene;
            AppDomain.CurrentDomain.DomainUnload += CurrentDomainDomainUnload;
            Game.OnWndProc += Game_OnWndProc;
            Game.OnFireEvent += Game_OnGameEvent;

            #endregion

            #region ShowMeMore

            ShowMeMoreH[0] = new ShowMeMoreHelper("modifier_invoker_sun_strike",
                "hero_invoker/invoker_sun_strike_team",
                "hero_invoker/invoker_sun_strike_ring_b",
                175);
            ShowMeMoreH[1] = new ShowMeMoreHelper("modifier_lina_light_strike_array",
                "hero_lina/lina_spell_light_strike_array_ring_collapse",
                "hero_lina/lina_spell_light_strike_array_sphere",
                225);
            ShowMeMoreH[2] = new ShowMeMoreHelper("modifier_kunkka_torrent_thinker",
                "hero_kunkka/kunkka_spell_torrent_pool",
                "hero_kunkka/kunkka_spell_torrent_bubbles_b",
                225);
            ShowMeMoreH[3] = new ShowMeMoreHelper("modifier_leshrac_split_earth_thinker",
                "hero_leshrac/leshrac_split_earth_b",
                "hero_leshrac/leshrac_split_earth_c",
                225);

            for (var z = 1; z <= 140; z++)
            {
                Eff[z] = new Dictionary<Unit, ParticleEffect>();
            }

            #endregion
            
        }


        private static void Game_OnGameEvent(FireEventEventArgs args)
        {
            if (args.GameEvent.Name != "dota_roshan_kill") return;
            _deathTime = Game.GameTime;
            _roshIsAlive = false;
        }

        #endregion

        #region !

        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (args.WParam != 1 || Game.IsChatOpen || !Utils.SleepCheck("clicker"))
            {
                _leftMouseIsPress = false;
                return;
            }
            _leftMouseIsPress = true;
        }

        private static void CurrentDomainDomainUnload(object sender, EventArgs e)
        {
            for (var i = 0; i <= 20; i++)
                if (FontArray[i] != null)
                    FontArray[i].Dispose();
            if (_line != null)
                _line.Dispose();
        }

        private static void Drawing_OnPostReset(EventArgs args)
        {
            for (var i = 0; i <= 20; i++)
                if (FontArray[i] != null)
                    FontArray[i].OnLostDevice();
            if (_line != null)
                _line.OnLostDevice();
        }

        private static void Drawing_OnPreReset(EventArgs args)
        {
            for (var i = 0; i <= 20; i++)
                if (FontArray[i] != null)
                    FontArray[i].OnLostDevice();
            if (_line != null)
                _line.OnLostDevice();
        }

        #endregion

        #region Game_OnUpdate

        private static void Game_OnUpdate(EventArgs args)
        {
            
            #region Load/Unload
            _me = ObjectMgr.LocalHero;
            
            if (!_loaded)
            {
                
                if (!Game.IsInGame || _me == null)
                {
                    return;
                }
                _loaded = true;
                PrintSuccess("> OverlayInformation loaded! v" + Ver);
            }
            
            if (!Game.IsInGame || _me == null)
            {
                _loaded = false;
                PrintInfo("> OverlayInformation unLoaded");
            }

            #endregion
            
            #region Show me more cast
            
            //List<Unit> dummy;
            if (ShowMeMore)
            {
                var dummy = ObjectMgr.GetEntities<Unit>().Where(x => x.ClassID == ClassID.CDOTA_BaseNPC).ToList();
                foreach (var t in dummy)
                {
                    for (var n = 0; n <= 4; n++)
                    {
                        try
                        {
                            var mod = t.Modifiers.FirstOrDefault(x => x.Name == ShowMeMoreH[n].Modifier);
                            if (mod == null) continue;
                            
                            ParticleEffect effect;
                            if (!ShowMeMoreEffect.TryGetValue(t, out effect))
                            {
                                effect = t.AddParticleEffect(@"particles\ui_mouseactions\range_display.vpcf");
                                effect.SetControlPoint(1, new Vector3(ShowMeMoreH[n].Range, 0, 0));
                                ShowMeMoreEffect.Add(t, effect);
                                //ShowMeMoreEffect.Add(t, effect);
                                /*LifeSpan(
                                    new ParticleEffect(@"particles/units/heroes" + ShowMeMoreH[n].EffectName + ".vpcf",
                                        t.Position),
                                    new ParticleEffect(@"particles/units/heroes" + ShowMeMoreH[n].SecondeffectName + ".vpcf",
                                        t.Position));*/
                            }
                            break;
                        }
                        catch (Exception)
                        {

                        }
                    }
                    /*if (t.DayVision == 0 && !find)
                    {
                        Bara = t;
                        find = true;
                        BaraStartPos=Bara.Position;
                    }
                    else if (!find)
                    {
                        Bara = null;
                        BaraStartPos=new Vector3(0,0,0);
                    }*/
                }
            }

            #endregion
            
            #region ShowRoshanTimer

            if (ShowRoshanTimer)
            {
                var tickDelta = Game.GameTime - _deathTime;
                _roshanMinutes = Math.Floor(tickDelta / 60);
                _roshanSeconds = tickDelta % 60;
                var roshan =
                    ObjectMgr.GetEntities<Unit>()
                        .FirstOrDefault(unit => unit.ClassID == ClassID.CDOTA_Unit_Roshan && unit.IsAlive);
                if (roshan != null)
                {
                    _roshIsAlive = true;
                    //RoshanMinutes = 0;
                    //RoshanSeconds = 0;
                    //DeathTime = 0;
                }
            }

            #endregion

            #region ShowIllusions

            if (ShowIllusions)
            {
                var illusions = ObjectMgr.GetEntities<Hero>()
                    .Where(
                        x =>
                            x.IsIllusion && x.Team != _me.Team);
                foreach (var s in illusions)
                {
                    HandleEffect(s);
                }
            }

            #endregion
            return;
            #region AutoItems

            if (!AutoItemsActive || !Utils.SleepCheck("AutoItems")) return;
            if (AutoItemsMidas)
            {
                var midas = _me.FindItem("item_hand_of_midas");
                if (midas != null && midas.CanBeCasted() && !_me.IsInvisible())
                {
                    var creep =
                        ObjectMgr
                            .GetEntities<Unit>(
                            ).FirstOrDefault(x => ((x is Hero && !x.IsIllusion) || (x is Creep && x.IsSpawned)) && x.IsAlive &&
                                                  x.IsVisible && _me.Distance2D(x) <= midas.CastRange && x.Team != _me.Team);
                    midas.UseAbility(creep);
                    Utils.Sleep(250, "AutoItems");
                    //PrintError("midas.CastRange: " + midas.CastRange);
                }
            }
            if (AutoItemsPhase)
            {
                var phase = _me.FindItem("item_phase_boots");
                if (phase != null && phase.CanBeCasted() && !_me.IsAttacking() && !_me.IsInvisible())
                {
                    phase.UseAbility();
                    Utils.Sleep(250, "AutoItems");
                }
            }
            if (!AutoItemsStick) return;
            var stick = _me.FindItem("item_magic_stick");
            var wand = _me.FindItem("item_magic_wand");
            if (_me.Health * 100 / _me.MaximumHealth > 30 || !_me.IsAlive) return;
            if (stick != null && stick.CanBeCasted() && !_me.IsInvisible())
            {
                stick.UseAbility();
                Utils.Sleep(250, "AutoItems");
            }
            if (wand == null || !wand.CanBeCasted()) return;
            wand.UseAbility();
            Utils.Sleep(250, "AutoItems");

            #endregion
        }

        #endregion

        #region Drawing_OnEndScene

        private static void Drawing_OnEndScene(EventArgs args)
        {
            #region Checking

            if (Drawing.Direct3DDevice9 == null || Drawing.Direct3DDevice9.IsDisposed || !Game.IsInGame || !_loaded)
            {
                return;
            }
            var hero = ObjectMgr.LocalHero;
            if (hero == null || hero.Team == Team.Observer || Game.IsPaused)
            {
                return;
            }

            #endregion

            #region ShowRoshanTimer

            if (ShowRoshanTimer)
            {
                var text = "";
                if (!_roshIsAlive)
                {
                    if (_roshanMinutes < 8)
                        text = string.Format("Roshan: {0}:{1:0.} - {2}:{3:0.}", 7 - _roshanMinutes, 59 - _roshanSeconds,
                            10 - _roshanMinutes,
                            59 - _roshanSeconds);
                    else if (_roshanMinutes == 8)
                    {
                        text = string.Format("Roshan: {0}:{1:0.} - {2}:{3:0.}", 8 - _roshanMinutes, 59 - _roshanSeconds,
                            10 - _roshanMinutes,
                            59 - _roshanSeconds);
                    }
                    else if (_roshanMinutes == 9)
                    {
                        text = string.Format("Roshan: {0}:{1:0.} - {2}:{3:0.}", 9 - _roshanMinutes, 59 - _roshanSeconds,
                            10 - _roshanMinutes,
                            59 - _roshanSeconds);
                    }
                    else
                    {
                        text = string.Format("Roshan: {0}:{1:0.}", 0, 59 - _roshanSeconds);
                        if (59 - _roshanSeconds <= 1)
                        {
                            _roshIsAlive = true;
                        }
                    }
                }
                DrawShadowText(_roshIsAlive ? "Roshan alive" : _deathTime == 0 ? "Roshan death" : text, 217, 10,
                    _roshIsAlive ? Color.Green : Color.Red, FontArray[5]);
            }

            #endregion

            #region Menu
            /*
            if (_isOpen)
            {
                DrawFilledBox(_drawHelper.MenuPos.X - 2, _drawHelper.MenuPos.Y, 60, 500, new ColorBGRA(0, 0, 0, 100));
                //-----------------------------------------------------------------------------------------------------------------
                DrawButton(_drawHelper.MenuPos.X + 5, _drawHelper.MenuPos.Y + 25, 50, 20, 1, ref ShowCooldownOnTopPanel,
                    true,
                    new Color(100, 255, 0, 50), new Color(100, 0, 0, 50), "Show Cooldown on top panel");
                //-----------------------------------------------------------------------------------------------------------------
                DrawButton(_drawHelper.MenuPos.X + 5, _drawHelper.MenuPos.Y + 50, 50, 20, 1,
                    ref ShowCooldownOnTopPanelLikeText,
                    true,
                    new Color(100, 255, 0, 50), new Color(100, 0, 0, 50), "Show Cooldown on top panel (numbers)");
                //-----------------------------------------------------------------------------------------------------------------
                DrawButton(_drawHelper.MenuPos.X + 5, _drawHelper.MenuPos.Y + 75, 50, 20, 1, ref ShowHealthOnTopPanel,
                    true,
                    new Color(100, 255, 0, 50), new Color(100, 0, 0, 50), "Show Health on top panel");
                //-----------------------------------------------------------------------------------------------------------------
                DrawButton(_drawHelper.MenuPos.X + 5, _drawHelper.MenuPos.Y + 100, 50, 20, 1, ref ShowManaOnTopPanel,
                    true,
                    new Color(100, 255, 0, 50), new Color(100, 0, 0, 50), "Show Mana on top panel");
                //-----------------------------------------------------------------------------------------------------------------
                DrawButton(_drawHelper.MenuPos.X + 5, _drawHelper.MenuPos.Y + 125, 50, 20, 1, ref OverlayOnlyOnEnemy,
                    true,
                    new Color(100, 255, 0, 50), new Color(100, 0, 0, 50), "Overlay only on enemy");
                //-----------------------------------------------------------------------------------------------------------------
                DrawButton(_drawHelper.MenuPos.X + 5, _drawHelper.MenuPos.Y + 150, 50, 20, 1, ref ShowGlyph,
                    false,
                    new Color(100, 255, 0, 50), new Color(100, 0, 0, 50), "Show glyph cd");
                //-----------------------------------------------------------------------------------------------------------------
                DrawButton(_drawHelper.MenuPos.X + 5, _drawHelper.MenuPos.Y + 175, 50, 20, 1, ref ShowLastHit,
                    true,
                    new Color(100, 255, 0, 50), new Color(100, 0, 0, 50), "Show LastHit/Deny");
                //-----------------------------------------------------------------------------------------------------------------
                DrawButton(_drawHelper.MenuPos.X + 5, _drawHelper.MenuPos.Y + 200, 50, 20, 1, ref ShowIllusions,
                    true,
                    new Color(100, 255, 0, 50), new Color(100, 0, 0, 50), "Show Illusions");
                //-----------------------------------------------------------------------------------------------------------------
                DrawButton(_drawHelper.MenuPos.X + 5, _drawHelper.MenuPos.Y + 225, 50, 20, 1, ref ShowManabars,
                    false,
                    new Color(100, 255, 0, 50), new Color(100, 0, 0, 50), "Show manabars");
                //-----------------------------------------------------------------------------------------------------------------
                DrawButton(_drawHelper.MenuPos.X + 5, _drawHelper.MenuPos.Y + 250, 50, 20, 1, ref ShowRoshanTimer,
                    true,
                    new Color(100, 255, 0, 50), new Color(100, 0, 0, 50), "Show roshan timer");
                //-----------------------------------------------------------------------------------------------------------------
                DrawButton(_drawHelper.MenuPos.X + 5, _drawHelper.MenuPos.Y + 275, 50, 20, 1, ref ShowBuybackCooldown,
                    true,
                    new Color(100, 255, 0, 50), new Color(100, 0, 0, 50), "Show Buyback cooldown");
                //-----------------------------------------------------------------------------------------------------------------
                DrawButton(_drawHelper.MenuPos.X + 5, _drawHelper.MenuPos.Y + 300, 50, 20, 1, ref ShowMeMore,
                    true,
                    new Color(100, 255, 0, 50), new Color(100, 0, 0, 50), "Show Me more");
                //-----------------------------------------------------------------------------------------------------------------
                DrawButton(_drawHelper.MenuPos.X + 5, _drawHelper.MenuPos.Y + 325, 50, 20, 1, ref AutoItemsMenu,
                    true,
                    new Color(100, 255, 0, 50), new Color(100, 0, 0, 50), " > Auto items");
                //-----------------------------------------------------------------------------------------------------------------

                #region Extra menu for autoitems

                if (AutoItemsMenu)
                {
                    //-----------------------------------------------------------------------------------------------------------------
                    DrawButton(_drawHelper.MenuPos.X - 50, _drawHelper.MenuPos.Y + 325, 50, 20, 1, ref AutoItemsActive,
                        true,
                        new Color(100, 255, 0, 50), new Color(100, 0, 0, 50), "AutoItems Active");
                    //-----------------------------------------------------------------------------------------------------------------
                    DrawButton(_drawHelper.MenuPos.X - 50, _drawHelper.MenuPos.Y + 350, 50, 20, 1, ref AutoItemsPhase,
                        true,
                        new Color(100, 255, 0, 50), new Color(100, 0, 0, 50), "Auto use phase boots");
                    //-----------------------------------------------------------------------------------------------------------------
                    DrawButton(_drawHelper.MenuPos.X - 50, _drawHelper.MenuPos.Y + 375, 50, 20, 1, ref AutoItemsMidas,
                        true,
                        new Color(100, 255, 0, 50), new Color(100, 0, 0, 50), "Auto use midas");
                    DrawButton(_drawHelper.MenuPos.X - 50, _drawHelper.MenuPos.Y + 400, 50, 20, 1, ref AutoItemsStick,
                        true,
                        new Color(100, 255, 0, 50), new Color(100, 0, 0, 50), "Auto use stick");
                }
            
                #endregion
            }
            */
            #endregion
        }

        #endregion

        #region Off

        
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Game.IsInGame || _me == null || !_loaded || Game.IsPaused) return;
            /*if (_players == null || _players.Count() < 10)
                {
                    _players = ObjectMgr.GetEntities<Player>().Where(x => x != null && x.Hero != null && x.Hero.IsAlive);
                }
            var enumerable = _players as Player[] ?? _players.ToArray();*/
            //foreach (var s in enumerable)

            #region J - overlay

            var startLoc = new Vector2(Drawing.Width - 120, 50);
            var maxSize = new Vector2(110, 500);

            
            if (_isOpen)
            {
                _sizer.Y += 10;
                _sizer.Y = Math.Min(_sizer.Y, maxSize.Y);
                Drawing.DrawRect(startLoc, _sizer, new Color(0, 0, 0, 100));
                if (Equals(_sizer, maxSize))
                {
                    var pos = 35;
                    DrawButton(startLoc + new Vector2(5, 35), _sizer.X - 10, 20, ref ShowHealthOnTopPanel, true, new Color(0, 200, 0, 50), new Color(200, 0, 0, 50), "Show Health");
                    pos += 22;
                    DrawButton(startLoc + new Vector2(5, pos), _sizer.X - 10, 20, ref ShowManaOnTopPanel, true, new Color(0, 200, 0, 50), new Color(200, 0, 0, 50), "Show Mana");
                    pos += 22;
                    DrawButton(startLoc + new Vector2(5, pos), _sizer.X - 10, 20, ref ShowRoshanTimer, true, new Color(0, 200, 0, 50), new Color(200, 0, 0, 50), "Roshan Timer");
                    pos += 22;
                    DrawButton(startLoc + new Vector2(5, pos), _sizer.X - 10, 20, ref ShowMeMore, true, new Color(0, 200, 0, 50), new Color(200, 0, 0, 50), "Show Me More");
                    pos += 22;
                    DrawButton(startLoc + new Vector2(5, pos), _sizer.X - 10, 20, ref ShowIllusions, true, new Color(0, 200, 0, 50), new Color(200, 0, 0, 50), "Show Illusions");
                    pos += 22;
                    DrawButton(startLoc + new Vector2(5, pos), _sizer.X - 10, 20, ref DangItems, true, new Color(0, 200, 0, 50), new Color(200, 0, 0, 50), "Dangerous Items");
                }
            }
            else
            {
                _sizer.Y -= 10;
                _sizer.Y = Math.Max(_sizer.Y, 0);
                Drawing.DrawRect(startLoc, _sizer, new Color(0, 0, 0, 100));
            }
            DrawButton(startLoc, _sizer.X, 30, ref _isOpen, true, new Color(0, 0, 0, 50), new Color(0, 0, 0, 125), "J-Overlay Light");
            #endregion

            for (uint i = 0; i < 10; i++)
            {
                #region Init

                Hero v;
                try
                {
                    v = ObjectMgr.GetPlayerById(i).Hero;
                }
                catch
                {
                    continue;
                }
                if (v == null) continue;
                var pos = HUDInfo.GetTopPanelPosition(v);
                var sizeX = (float) HUDInfo.GetTopPanelSizeX(v);
                var sizeY = (float) HUDInfo.GetTopPanelSizeY(v);
                var healthDelta = new Vector2(v.Health*sizeX/v.MaximumHealth, 0);
                var manaDelta = new Vector2(v.Mana*sizeX/v.MaximumMana, 0);
                const int height = 7;

                #endregion

                #region Dang Items

                if (DangItems && v.Team != _me.Team && v.IsVisible)
                {
                    var invetory =
                        v.Inventory.Items.Where(
                            x =>
                                x.Name == "item_gem" || x.Name == "item_dust" || x.Name == "item_sphere" ||
                                x.Name == "item_blink");
                    var iPos = HUDInfo.GetHPbarPosition(v);
                    var iSize = new Vector2(HUDInfo.GetHPBarSizeX(v), HUDInfo.GetHpBarSizeY(v));
                    float count = 0;
                    foreach (var item in invetory)
                    {
                        var itemname = string.Format("materials/ensage_ui/items/{0}.vmat",
                            item.Name.Replace("item_", ""));
                        Drawing.DrawRect(iPos + new Vector2(count, 50), new Vector2(iSize.X/3, (float) (iSize.Y*2.5)),
                            GetTexture(itemname));
                        if (item.AbilityState == AbilityState.OnCooldown)
                        {
                            var cd=((int) item.Cooldown).ToString(CultureInfo.InvariantCulture);
                            Drawing.DrawText(cd, iPos + new Vector2(count, 40), Color.White, FontFlags.AntiAlias | FontFlags.DropShadow);
                        }
                        if (item.AbilityState == AbilityState.NotEnoughMana)
                        {
                            Drawing.DrawRect(iPos + new Vector2(count, 50), new Vector2(iSize.X /4, (float)(iSize.Y * 2.5)),new Color(0,0,200,100));
                        }
                        count += iSize.X / 4;
                    }
                }

                #endregion


                #region Health

                if (ShowHealthOnTopPanel)
                {
                    Drawing.DrawRect(pos + new Vector2(0, sizeY + 1), new Vector2(sizeX, height),
                        new Color(255, 0, 0, 255));
                    Drawing.DrawRect(pos + new Vector2(0, sizeY + 1), new Vector2(healthDelta.X, height),
                        new Color(0, 255, 0, 255));
                    Drawing.DrawRect(pos + new Vector2(0, sizeY + 1), new Vector2(sizeX, height), Color.Black, true);
                }

                #endregion

                #region Mana


                if (ShowManaOnTopPanel)
                {
                    Drawing.DrawRect(pos + new Vector2(0, sizeY + height), new Vector2(sizeX, height), Color.Gray);
                    Drawing.DrawRect(pos + new Vector2(0, sizeY + height), new Vector2(manaDelta.X, height),
                        new Color(0, 0, 255, 255));
                    Drawing.DrawRect(pos + new Vector2(0, sizeY + height), new Vector2(sizeX, height), Color.Black, true);
                }

                #endregion

                #region ShowMeMore
                if (ShowMeMore && v.Team == _me.Team)
                {
                    var mod = v.Modifiers.Any(x => x.Name == "modifier_spirit_breaker_charge_of_darkness_vision");
                    if (mod /* && Bara!=null*/)
                    {
                        /*Vector2 vPos;
                    if (Drawing.WorldToScreen(v.Position, out vPos))
                    {
                        Vector2 targetPos;
                        if (Drawing.WorldToScreen(Bara.Position, out targetPos))
                        {
                            Drawing.DrawLine(vPos,targetPos,Color.AliceBlue);
                        }
                    }
                    var dist = Bara.Distance2D(v);
                    var startDist = v.Distance2D(BaraStartPos);
                    var spellDelta =
                                new Vector2(
                                    dist * sizeX / startDist, 0);
                    Drawing.DrawRect(pos + new Vector2(0, sizeY + height * 2), new Vector2(sizeX, height), Color.Gray);
                    Drawing.DrawRect(pos + new Vector2(0, sizeY + height * 2), new Vector2(spellDelta.X, height), Color.Yellow);
                    Drawing.DrawRect(pos + new Vector2(0, sizeY + height * 2), new Vector2(sizeX, height), Color.Black, true);*/
                        var textPos = (pos + new Vector2(0, sizeY + height*2));
                        Drawing.DrawText("Spirit Breaker", textPos, new Vector2(15, 150), Color.White,
                            FontFlags.AntiAlias | FontFlags.DropShadow);
                        if (Equals(_me, v))
                        {
                            Drawing.DrawRect(new Vector2(0, 0), new Vector2(Drawing.Width, Drawing.Height),
                                new Color(255, 0, 0, 2));
                        }
                        ParticleEffect eff;
                        if (!BaraIndicator.TryGetValue(v, out eff))
                        {
                            eff = new ParticleEffect("particles/hw_fx/cursed_rapier.vpcf", v);
                            eff.SetControlPointEntity(1,v);
                            BaraIndicator.Add(v,eff);
                        }
                    }
                    else
                    {
                        ParticleEffect eff;
                        if (BaraIndicator.TryGetValue(v, out eff))
                        {
                            eff.Dispose();
                            BaraIndicator.Remove(v);
                        }
                    }
                }

                

                if (ShowMeMore && v.Team != _me.Team)
                {
                    switch (v.ClassID)
                    {
                        case ClassID.CDOTA_Unit_Hero_Mirana:
                            if (_arrowUnit == null)
                            {
                                /*foreach (var effect in ArrowParticalEffects)
                                {
                                    effect.Dispose();
                                }
                                ArrowParticalEffects[1] = null;*/
                                _arrowUnit =
                                    ObjectMgr.GetEntities<Unit>()
                                        .FirstOrDefault(x => x.ClassID == ClassID.CDOTA_BaseNPC && x.DayVision == 650
                                                             && x.Team != _me.Team);
                            }
                            if (_arrowUnit != null)
                            {
                                if (!_arrowUnit.IsValid)
                                {
                                    foreach (var effect in ArrowParticalEffects.Where(effect => effect != null))
                                    {
                                        effect.Dispose();
                                    }
                                    _letsDraw = true;
                                    _arrowUnit =
                                    ObjectMgr.GetEntities<Unit>()
                                        .FirstOrDefault(x => x.ClassID == ClassID.CDOTA_BaseNPC && x.DayVision == 650
                                                             && x.Team != _me.Team);
                                    break;
                                }
                                if (!InSystem.Contains(_arrowUnit))
                                {
                                    _arrowS = _arrowUnit.Position;
                                    InSystem.Add(_arrowUnit);
                                    Utils.Sleep(100,"kek");
                                }
                                else if (_letsDraw && Utils.SleepCheck("kek"))
                                {
                                    _letsDraw = false;
                                    /*PrintInfo(string.Format("{0}{1}{2}/{3}{4}{5}", _arrowS.X, _arrowS.Y, _arrowS.Z,
                                        _arrowUnit.Position.X, _arrowUnit.Position.Y, _arrowUnit.Position.Z));*/
                                    var ret = FindRet(_arrowS, _arrowUnit.Position);
                                    for (var z = 1; z <= 147; z++)
                                    {
                                        var p = FindVector(_arrowS, ret, 20*z + 60);
                                        ArrowParticalEffects[z] = new ParticleEffect(
                                            @"particles\ui_mouseactions\draw_commentator.vpcf", p);
                                        ArrowParticalEffects[z].SetControlPoint(1, new Vector3(255, 255, 255));
                                        ArrowParticalEffects[z].SetControlPoint(0, p);
                                    }
                                }
                            }
                            break;
                        case ClassID.CDOTA_Unit_Hero_SpiritBreaker:
                            
                            break;
                        case ClassID.CDOTA_Unit_Hero_Windrunner:
                            if (true) //(Utils.SleepCheck("ArrowWindRun"))
                            {
                                var spell = v.Spellbook.Spell2;
                                if (spell != null && spell.Cooldown != 0)
                                {
                                    var cd = Math.Floor(spell.Cooldown * 100);
                                    if (cd < 880)
                                    {
                                        if (!InSystem.Contains(v))
                                        {
                                            if (cd > 720)
                                            {
                                                var eff = new ParticleEffect[148];
                                                for (var z = 1; z <= 140; z++)
                                                {
                                                    try
                                                    {
                                                        var p = new Vector3(
                                                            v.Position.X + 100 * z * (float)Math.Cos(v.RotationRad),
                                                            v.Position.Y + 100 * z * (float)Math.Sin(v.RotationRad),
                                                            100);
                                                        eff[z] =
                                                            new ParticleEffect(
                                                                @"particles\ui_mouseactions\draw_commentator.vpcf",
                                                                p);
                                                        eff[z].SetControlPoint(1, new Vector3(255, 255, 255));
                                                        eff[z].SetControlPoint(0, p);
                                                        Eff[z].Add(v, eff[z]);
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        PrintSuccess("[WIND RUNNER EX]" + ex);
                                                    }

                                                }
                                                InSystem.Add(v);
                                            }
                                        }
                                        else if (cd < 720 || !v.IsAlive)
                                        {
                                            InSystem.Remove(v);
                                            for (var z = 1; z <= 140; z++)
                                            {
                                                ParticleEffect eff;
                                                if (Eff[z].TryGetValue(v, out eff))
                                                {
                                                    eff.Dispose();
                                                    Eff[z].Clear();
                                                }
                                            }

                                        }
                                    }
                                }
                            }
                            break;
                        case ClassID.CDOTA_Unit_Hero_Pudge:
                            break;
                        case ClassID.CDOTA_Unit_Hero_Kunkka:
                            break;
                    }
                }

                #endregion
            }
        }

        

        #endregion

        #endregion

        #region Helpers
        private static DotaTexture GetTexture(string name)
        {
            if (TextureCache.ContainsKey(name)) return TextureCache[name];

            return TextureCache[name] = Drawing.GetTexture(name);
        }
        private static Vector3 FindVector(Vector3 first, double ret, float distance)
        {
            var retVector = new Vector3(first.X + (float)Math.Cos(Utils.DegreeToRadian(ret)) * distance, first.Y + (float)Math.Sin(Utils.DegreeToRadian(ret)) * distance, 100);

            return retVector;
        }
        private static double FindRet(Vector3 first, Vector3 second)
        {
            var xAngle = Utils.RadianToDegree(Math.Atan((Math.Abs(second.X - first.X) / Math.Abs(second.Y - first.Y))));
            if (first.X <= second.X && first.Y >= second.Y)
            {
                return (270 + xAngle);
            }
            if (first.X >= second.X & first.Y >= second.Y)
            {
                return ((90 - xAngle) + 180);
            }
            if (first.X >= second.X && first.Y <= second.Y)
            {
                return (90 + xAngle);
            }
            if (first.X <= second.X && first.Y <= second.Y)
            {
                return (90 - xAngle);
            }
            return 0;
        }
        private static void HandleEffect(Unit unit)
        {
            ParticleEffect effect;
            ParticleEffect effect2;
            if (unit.IsAlive/* && unit.IsVisibleToEnemies*/)
            {
                if (Effects1.TryGetValue(unit, out effect)) return;
                effect = unit.AddParticleEffect("particles/items2_fx/smoke_of_deceit_buff.vpcf"); //particles/items_fx/diffusal_slow.vpcf
                effect2 = unit.AddParticleEffect("particles/items2_fx/shadow_amulet_active_ground_proj.vpcf");
                Effects1.Add(unit, effect);
                Effects2.Add(unit, effect2);
            }
            else
            {
                if (!Effects1.TryGetValue(unit, out effect)) return;
                if (!Effects2.TryGetValue(unit, out effect2)) return;
                effect.Dispose();
                effect2.Dispose();
                Effects1.Remove(unit);
                Effects2.Remove(unit);
            }
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

        #endregion

        #region Drawing Methods

        public static void DrawCircle(int x, int y, int radius, int numSides, int thickness, Color color)
        {
            var vector2S = new Vector2[128];
            var step = (float) Math.PI*2.0f/numSides;
            var count = 0;
            for (float a = 0; a < (float) Math.PI*2.0; a += step)
            {
                var x1 = radius*(float) Math.Cos(a) + x;
                var y1 = radius*(float) Math.Sin(a) + y;
                var x2 = radius*(float) Math.Cos(a + step) + x;
                var y2 = radius*(float) Math.Sin(a + step) + y;
                vector2S[count].X = x1;
                vector2S[count].Y = y1;
                vector2S[count + 1].X = x2;
                vector2S[count + 1].Y = y2;

                DrawLine(x1, y1, x2, y2, thickness, color);
                count += 2;
            }
        }

        public static void DrawFilledBox(Vector2 a, Vector2 b, Color color)
        {
            /*try
            {*/
            var vLine = new Vector2[2];
            _line.GLLines = true;
            _line.Antialias = false;
            _line.Width = b.X + 1;

            vLine[0].X = b.X + (b.X + 1)/2;
            vLine[0].Y = b.Y;
            vLine[1].X = b.X + (b.X + 1)/2;
            vLine[1].Y = b.Y + b.Y;

            _line.Begin();
            _line.Draw(vLine, color);
            _line.End();
            /*}
            catch (Exception)
            {

                PrintError("DrawFilledBox(Vector2 a,Vector2 b, Color color) - " +
                           string.Format("{0}/{1}/{2}/{3}", a.X, a.Y, b.X, b.Y));
            }*/
        }

        public static void DrawFilledBox(float x, float y, float w, float h, Color color)
        {
            /*try
            {*/
            var vLine = new Vector2[2];

            _line.GLLines = true;
            _line.Antialias = false;
            _line.Width = w + 1;

            vLine[0].X = x + (w + 1)/2;
            vLine[0].Y = y;
            vLine[1].X = x + (w + 1)/2;
            vLine[1].Y = y + h;

            _line.Begin();
            _line.Draw(vLine, color);
            _line.End();
            /*}
            catch
            {
                PrintError("DrawFilledBox(float x, float y, float w, float h, Color color) - " +
                           string.Format("{0}/{1}/{2}/{3}", x, y, w, h));
            }*/
        }

        public static void DrawLine(float x1, float y1, float x2, float y2, float w, Color color)
        {
            var vLine = new[] {new Vector2(x1, y1), new Vector2(x2, y2)};

            _line.GLLines = true;
            _line.Antialias = false;
            _line.Width = w;

            _line.Begin();
            _line.Draw(vLine, color);
            _line.End();
        }

        public static void DrawShadowText(string stext, int x, int y, Color color, Font f)
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

        public static void DrawBox(Vector2 a, Vector2 b, float px, Color color)
        {
            DrawFilledBox(a.X, a.Y + b.Y, b.X, px, color);
            DrawFilledBox(a.X - px, a.Y, px, b.Y, color);
            DrawFilledBox(a.X, a.Y - px, b.X, px, color);
            DrawFilledBox(a.X + b.X, a.Y, px, b.Y, color);
        }

        private static void DrawButton(Vector2 a, float w, float h, ref bool clicked, bool isActive, Color @on, Color off, string drawOnButtonText = "")
        {
            var isIn = Utils.IsUnderRectangle(Game.MouseScreenPosition, a.X, a.Y, w, h);
            if (isActive)
            {
                if (_leftMouseIsPress && Utils.SleepCheck("ClickButtonCd") && isIn)
                {
                    clicked = !clicked;
                    Utils.Sleep(250, "ClickButtonCd");
                }
                var newColor = isIn
                    ? new Color((int)(clicked ? @on.R : off.R), clicked ? @on.G : off.G, clicked ? @on.B : off.B, 150)
                    : clicked ? @on : off;
                Drawing.DrawRect(a, new Vector2(w, h), newColor);
                Drawing.DrawRect(a, new Vector2(w, h), new Color(0, 0, 0, 255), true);
                if (drawOnButtonText != "")
                {
                    Drawing.DrawText(drawOnButtonText, a + new Vector2(10, 2), Color.White,
                    FontFlags.AntiAlias | FontFlags.DropShadow);
                }
            }
            else
            {
                Drawing.DrawRect(a, new Vector2(w, h), Color.Gray);
                Drawing.DrawRect(a, new Vector2(w, h), new Color(0, 0, 0, 255), true);
            }
        }

        #endregion
    }
}