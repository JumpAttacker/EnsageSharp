using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using SharpDX;
using SharpDX.Direct3D9;

// ReSharper disable EmptyGeneralCatchClause
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace Overlay_information
{
    internal class Program
    {
        #region Members
        private static bool _loaded;
        private static Hero _me;
        private static Player _player;
        private const string Ver =  "0.8";
        private static Vector2 _screenSizeVector2;
        private static ScreenSizer _drawHelper;
        private static bool _isOpen;
        private static bool _leftMouseIsPress;
        private static readonly Dictionary<Unit, ParticleEffect> Effects2 = new Dictionary<Unit, ParticleEffect>();
        private static readonly Dictionary<Unit, ParticleEffect> Effects1 = new Dictionary<Unit, ParticleEffect>();
        //======================================
        public static bool ShowCooldownOnTopPanel;
        public static bool ShowCooldownOnTopPanelLikeText; //working only with ShowCooldownOnTopPanel
        public static bool ShowHealthOnTopPanel;
        public static bool ShowManaOnTopPanel;
        public static bool OverlayOnlyOnEnemy;
        public static bool ShowGlyph;
        public static bool ShowIllusions;
        public static bool ShowLastHit;
        public static bool ShowManabars;
        public static bool ShowRoshanTimer;
        public static bool ShowBuybackCooldown;
        public static bool ShowMeMore;
        public static bool AutoItemsMenu;
        public static bool AutoItemsActive;
        public static bool AutoItemsMidas;
        public static bool AutoItemsPhase;
        public static bool AutoItemsStick;
        //=====================================
        private static readonly ShowMeMoreHelper[] _showMeMore=new ShowMeMoreHelper[5];
        private static readonly Dictionary<Unit, ParticleEffect> ShowMeMoreEffect = new Dictionary<Unit, ParticleEffect>();
        private static readonly Dictionary<Unit, ParticleEffect>[] Eff = new Dictionary<Unit, ParticleEffect>[141];
        private static readonly List<Unit> InSystem=new List<Unit>();
        private static Vector3 _arrowS;
        //=====================================
        //static readonly InitHelper SaveLoadSysHelper = new InitHelper(Game.AppDataPath + "\\jOverlay.ini");
        //=====================================
        private static Single _deathTime;
        private static double _roshanMinutes;
        private static double _roshanSeconds;
        private static bool _roshIsAlive;
        //=====================================
        private static readonly Font[] FontArray=new Font[21];
        private static Line _line;
        #endregion
        #region Methods

        #region Init

        private static void Main()
        {
            ///////////////////////////////////////////////////////////////////////
            // TODO: DELETE THIS SHIT (MAY BE? OR NOT?)///////////////////////////
            PrintEncolored("Useless w8 4 some test. 500ms", ConsoleColor.Cyan);//
            Thread.Sleep(500);///////////////////////////////////////////////////
            PrintEncolored("We do this!", ConsoleColor.Cyan);////////////////////////
            //////////////////////////////////////////////////////////////////////
            #region Init

            Game.OnUpdate += Game_OnUpdate;
            _loaded = false;
            Drawing.OnDraw += Drawing_OnDraw;
            ShowCooldownOnTopPanel = true;
            ShowHealthOnTopPanel = true;
            ShowManaOnTopPanel = true;
            ShowCooldownOnTopPanelLikeText = true;
            ShowRoshanTimer = true;
            ShowBuybackCooldown = true;
            
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
            _showMeMore[0]=new ShowMeMoreHelper("modifier_invoker_sun_strike",
                "hero_invoker/invoker_sun_strike_team",
                "hero_invoker/invoker_sun_strike_ring_b",
                175);
            _showMeMore[1]=new ShowMeMoreHelper("modifier_lina_light_strike_array",
                "hero_lina/lina_spell_light_strike_array_ring_collapse",
                "hero_lina/lina_spell_light_strike_array_sphere",
                225);
            _showMeMore[2]=new ShowMeMoreHelper("modifier_kunkka_torrent_thinker",
                "hero_kunkka/kunkka_spell_torrent_pool",
                "hero_kunkka/kunkka_spell_torrent_bubbles_b",
                225);
            _showMeMore[3]=new ShowMeMoreHelper("modifier_leshrac_split_earth_thinker",
                "hero_leshrac/leshrac_split_earth_b",
                "hero_leshrac/leshrac_split_earth_c",
                225);

            for (var z = 1; z <= 140; z++)
            {
                Eff[z]=new Dictionary<Unit, ParticleEffect>();
            }
            #endregion

            #region Save/load
            /*
            try
            {
                ShowHealthOnTopPanel =
                    Convert.ToBoolean(SaveLoadSysHelper.IniReadValue("Booleans", "Show Health on top panel")); //
                ShowManaOnTopPanel =
                    Convert.ToBoolean(SaveLoadSysHelper.IniReadValue("Booleans", "Show mana on top panel"));
                ShowCooldownOnTopPanel =
                    Convert.ToBoolean(SaveLoadSysHelper.IniReadValue("Booleans", "Show Cooldown on top panel")); //
                ShowCooldownOnTopPanelLikeText =
                    Convert.ToBoolean(SaveLoadSysHelper.IniReadValue("Booleans", "Show Cooldown on top panel (numbers)"));
                    //
                OverlayOnlyOnEnemy =
                    Convert.ToBoolean(SaveLoadSysHelper.IniReadValue("Booleans", "Overlay only on enemy")); //

                ShowGlyph = Convert.ToBoolean(SaveLoadSysHelper.IniReadValue("Booleans", "Show glyph cd")); //
                ShowIllusions = Convert.ToBoolean(SaveLoadSysHelper.IniReadValue("Booleans", "Show Illusions"));
                ShowLastHit = Convert.ToBoolean(SaveLoadSysHelper.IniReadValue("Booleans", "Show LastHit/Deny"));
                ShowManabars = Convert.ToBoolean(SaveLoadSysHelper.IniReadValue("Booleans", "Show manabars"));
                ShowRoshanTimer = Convert.ToBoolean(SaveLoadSysHelper.IniReadValue("Booleans", "Show roshan timer"));
                ShowBuybackCooldown = Convert.ToBoolean(SaveLoadSysHelper.IniReadValue("Booleans", "Show Buyback cooldown"));

                AutoItemsActive = Convert.ToBoolean(SaveLoadSysHelper.IniReadValue("Booleans", "AutoItems Active"));
                AutoItemsPhase = Convert.ToBoolean(SaveLoadSysHelper.IniReadValue("Booleans", "Auto use phase boots"));
                AutoItemsMidas = Convert.ToBoolean(SaveLoadSysHelper.IniReadValue("Booleans", "Auto use midas"));
                AutoItemsStick = Convert.ToBoolean(SaveLoadSysHelper.IniReadValue("Booleans", "Auto use stick"));
            }
            catch
            {
                SaveLoadSysHelper.IniWriteValue("Booleans", "Show health on top panel", true.ToString());
                SaveLoadSysHelper.IniWriteValue("Booleans", "Show mana on top panel", true.ToString());
                SaveLoadSysHelper.IniWriteValue("Booleans", "Show cooldown on top panel", true.ToString());
                SaveLoadSysHelper.IniWriteValue("Booleans", "Show cooldown on top panel (numbers)", true.ToString());
                SaveLoadSysHelper.IniWriteValue("Booleans", "Overlay only on enemy", false.ToString());
                SaveLoadSysHelper.IniWriteValue("Booleans", "Show glyph cd", true.ToString());
                SaveLoadSysHelper.IniWriteValue("Booleans", "Show Illusions", true.ToString());
                SaveLoadSysHelper.IniWriteValue("Booleans", "Show LastHit/Deny", true.ToString());
                SaveLoadSysHelper.IniWriteValue("Booleans", "Show manabars", true.ToString());
                SaveLoadSysHelper.IniWriteValue("Booleans", "Show roshan timer", true.ToString());
                SaveLoadSysHelper.IniWriteValue("Booleans", "Show Buyback cooldown", true.ToString());

                SaveLoadSysHelper.IniWriteValue("Booleans", "AutoItems Active", true.ToString());
                SaveLoadSysHelper.IniWriteValue("Booleans", "Auto use phase boots", true.ToString());
                SaveLoadSysHelper.IniWriteValue("Booleans", "Auto use midas", true.ToString());
                SaveLoadSysHelper.IniWriteValue("Booleans", "Auto use stick", true.ToString());

                Console.Beep(1000, 100);
                Console.Beep(1000, 100);
                Console.Beep(1000, 100);
            }
            */
            #endregion
            
        }


        static void Game_OnGameEvent(FireEventEventArgs args)
        {
            if (args.GameEvent.Name == "dota_roshan_kill") 
            {
                //PrintError("roshan kill");
                //Thread roshanThread=new Thread();
                _deathTime = Game.GameTime;
                _roshIsAlive = false;
                //RoshanMinutes = 0;
                //RoshanSeconds = 0;
                //DeathTime = 0;
            }
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
        private static void Drawing_OnEndScene(EventArgs args)
        {
            #region Checking

            if (Drawing.Direct3DDevice9 == null || Drawing.Direct3DDevice9.IsDisposed || !Game.IsInGame || !_loaded)
            {
                return;
            }
            var player = ObjectMgr.LocalPlayer;
            if (player == null || player.Team == Team.Observer || _drawHelper == null)
            {
                return;
            }

            #endregion

            #region J - overlay

            DrawButton(_drawHelper.MenuPos.X - 2, _drawHelper.MenuPos.Y, 60, 20, 1, ref _isOpen, true,
                new Color(0, 0, 0, 50), new Color(0, 0, 0, 50), new Color(0, 0, 0, 125));
            DrawShadowText("J-Overlay", (int) _drawHelper.MenuPos.X, (int) _drawHelper.MenuPos.Y, Color.White,
                FontArray[5]);

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

            #region ShowLastHit || ShowBuybackCooldown

            if (ShowLastHit || ShowBuybackCooldown)
            {
                for (uint i = 0; i < 10; i++)
                {
                    Player p = null;
                    try
                    {
                        p = ObjectMgr.GetPlayerById(i);
                    }
                    catch
                    {
                    }
                    if (p == null) continue;

                    var initPos = (int) (i >= 5
                        ? (_drawHelper.RangeBetween + _drawHelper.FloatRange*i) + _drawHelper.Space
                        : (_drawHelper.RangeBetween + _drawHelper.FloatRange*i));
                    if (ShowLastHit)
                    {
                        var text = string.Format("{0}/{1}", p.LastHitCount, p.DenyCount);
                        DrawShadowText(text, initPos + 10, _drawHelper.BotRange + 1 - _drawHelper.Height*5, Color.White,
                            FontArray[5]);
                    }
                    if (ShowBuybackCooldown && p.BuybackCooldownTime > 0)
                    {
                        var text = string.Format("{0:0.}", p.BuybackCooldownTime);
                        DrawFilledBox(initPos + 2, _drawHelper.BotRange + 1 - _drawHelper.Height*5 + 22, 35, 15,
                            new Color(0, 0, 0, 150));
                        DrawShadowText(text, initPos + 5, _drawHelper.BotRange + 1 - _drawHelper.Height*5 + 22,
                            Color.White,
                            FontArray[3]);
                        //PrintError(i+" p.BuybackCooldownTime: " + p.BuybackCooldownTime);
                    }
                }
            }

            #endregion

            #region Menu

            if (!_isOpen) return;
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
                false,
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

            #endregion
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Game.IsInGame || _me == null || !_loaded) return;

            #region Set Screen Size init

            if (_screenSizeVector2.X == 0)
            {
                _screenSizeVector2 = new Vector2(Drawing.Width, Drawing.Height);
                PrintSuccess(string.Format(">> OI: init screen size: {0}x{1} -> {2}", _screenSizeVector2.X,
                    _screenSizeVector2.Y, (int) Math.Floor((decimal) (_screenSizeVector2.X/_screenSizeVector2.Y*100))));
                switch ((int) Math.Floor((decimal) (_screenSizeVector2.X/_screenSizeVector2.Y*100)))
                {
                    case 177:
                        _drawHelper = _screenSizeVector2.X == 1600
                            ? new ScreenSizer(494 - 439, 885 - 713, 7, 35, 439, new Vector2(1524, 46)) //1600x1900
                            : _drawHelper =
                                _screenSizeVector2.X == 1360
                                    ? new ScreenSizer(418 - 371, 753 - 605, 7, 29, 371, new Vector2(1288, 43))
                                    //1360x768
                                    : _drawHelper =
                                        _screenSizeVector2.X == 1280
                                            ? new ScreenSizer(395 - 351, 709 - 568, 5, 28, 350, new Vector2(1216, 40))
                                            //1280x720
                                            : _screenSizeVector2.X == 1366
                                                ? new ScreenSizer(422-374, 756 - 609, 7, 29, 374, new Vector2(1314, 39)) //1366

                                                : new ScreenSizer(66, 1063 - 855, 7, 43, 528, new Vector2(1860, 49)); //1920x1080


                        break;
                    case 166:
                        _drawHelper = new ScreenSizer(66, 1063 - 855, 7, 43, 528, new Vector2(1860, 49));
                        break;
                    case 160:
                        _drawHelper = new ScreenSizer(482 - 419, 941 - 738, 7, 42, 419, new Vector2(1610, 49));
                            //1680 x 1050
                        break;
                    case 133:
                        _drawHelper = new ScreenSizer(66, 1063 - 855, 7, 43, 528, new Vector2(1860, 49));
                        break;
                    case 125:
                        _drawHelper = new ScreenSizer(60, 1036 - 855, 7, 43, 250, new Vector2(1210, 49)); //1280x1024
                        break;
                    default:
                        _drawHelper = new ScreenSizer(66, 1063 - 855, 7, 43, 528, new Vector2(1860, 49));
                        break;
                }
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
                            var mod = t.Modifiers.FirstOrDefault(x => x.Name == _showMeMore[n].Modifier);
                            if (mod == null) continue;
                            ParticleEffect effect;
                            if (!ShowMeMoreEffect.TryGetValue(t, out effect))
                            {
                                effect = t.AddParticleEffect(@"particles\ui_mouseactions\range_display.vpcf");
                                effect.SetControlPoint(1, new Vector3(_showMeMore[n].Range, 0, 0));
                                ShowMeMoreEffect.Add(t, effect);
                                new ParticleEffect(@"particles/units/heroes" + _showMeMore[n].EffectName + ".vpcf",
                                    t.Position);
                            }
                            break;
                        }
                        catch (Exception)
                        {
                            
                        }
                        
                    }
                }
            }

            #endregion


            uint i;
            for (i = 0; i < 10; i++)
                {
                    try
                    {
                        #region Settings and Checking

                        var v = ObjectMgr.GetPlayerById(i).Hero;
                        if (OverlayOnlyOnEnemy && v.Team == _me.Team) continue;
                        if (v == null || !v.IsAlive) continue;
                        var initPos = i >= 5
                            ? (_drawHelper.RangeBetween + _drawHelper.FloatRange*i) + _drawHelper.Space
                            : (_drawHelper.RangeBetween + _drawHelper.FloatRange*i);
                        var healthDelta = new Vector2((float) v.Health*_drawHelper.FloatRange/v.MaximumHealth, 0);
                        var manaDelta = new Vector2(v.Mana*_drawHelper.FloatRange/v.MaximumMana, 0);

                        #endregion

                        #region ShowHealthOnTopPanel

                        if (ShowHealthOnTopPanel)
                        {
                            Drawing.DrawRect(new Vector2(initPos, _drawHelper.BotRange + 1),
                                new Vector2(_drawHelper.FloatRange, _drawHelper.Height),
                                Color.Red);
                            Drawing.DrawRect(new Vector2(initPos, _drawHelper.BotRange + 1),
                                healthDelta + new Vector2(0, _drawHelper.Height),
                                Color.Green);
                            Drawing.DrawRect(new Vector2(initPos, _drawHelper.BotRange + 1),
                                new Vector2(_drawHelper.FloatRange, _drawHelper.Height),
                                Color.Black, true);
                        }

                        #endregion

                        #region ShowManaOnTopPanel

                        if (ShowManaOnTopPanel)
                        {
                            Drawing.DrawRect(new Vector2(initPos, _drawHelper.BotRange + 1 + _drawHelper.Height),
                                new Vector2(_drawHelper.FloatRange, _drawHelper.Height), Color.Gray);
                            Drawing.DrawRect(new Vector2(initPos, _drawHelper.BotRange + 1 + _drawHelper.Height),
                                manaDelta + new Vector2(0, _drawHelper.Height), Color.Blue);
                            Drawing.DrawRect(new Vector2(initPos, _drawHelper.BotRange + 1 + _drawHelper.Height),
                                new Vector2(_drawHelper.FloatRange, _drawHelper.Height),
                                Color.Black, true);
                        }

                        #endregion

                        #region ShowMeMore

                        if (ShowMeMore)
                        {
                            switch (v.ClassID)
                            {
                                case ClassID.CDOTA_Unit_Hero_Mirana:
                                    //ArrowF(cast, team, v.visible, "mirana");
                                    var arrow = ObjectMgr.GetEntities<Unit>().FirstOrDefault(x => x.ClassID == ClassID.CDOTA_BaseNPC && x.DayVision == 650/* && x.Team!=_me.Team*/);

                                    if (arrow != null)
                                    {
                                        if (!InSystem.Contains(arrow))
                                        {
                                            _arrowS = arrow.Position;
                                            InSystem.Add(arrow);
                                        }
                                        else if (Utils.SleepCheck("Arrow"))
                                        {
                                            var e = new ParticleEffect[148];
                                            var ret = FindRet(_arrowS, arrow.Position);
                                            for (var z = 1; z <= 147; z++)
                                            {
                                                var p = FindVector(_arrowS, ret, 20*z + 60);
                                                e[z] = new ParticleEffect(@"particles\ui_mouseactions\draw_commentator.vpcf", p);
                                                e[z].SetControlPoint(1, new Vector3(255, 255, 255));
                                                e[z].SetControlPoint(0, p);
                                            }
                                            
                                            Utils.Sleep(300, "Arrow");
                                        }
                                    }
                                    break;
                                case ClassID.CDOTA_Unit_Hero_SpiritBreaker:
                                    //ChargeF(cast, team, v.visible, v: GetAbility(1), hero, "spirit_breaker");
                                    break;
                                case ClassID.CDOTA_Unit_Hero_Windrunner:
                                    if (true)//(Utils.SleepCheck("ArrowWindRun"))
                                    {
                                        var spell = v.Spellbook.Spell2;
                                        if (spell != null && spell.Cooldown != 0)
                                        {
                                            var cd = Math.Floor(spell.Cooldown*100);
                                            if (cd < 880)
                                            {
                                                ParticleEffect effect;
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
                                                                PrintSuccess(ex.ToString());
                                                            }
                                                            
                                                        }
                                                        InSystem.Add(v);
                                                    }
                                                }
                                                else if( cd < 720 || !v.IsAlive)
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
                                    //InfestF(team, hero, "life_stealer");
                                    break;
                                case ClassID.CDOTA_Unit_Hero_Kunkka:
                                    //InfestF(team, hero, "life_stealer");
                                    break;
                            }
                        }

                        #endregion

                        #region Spell Panel

                        if (Equals(v, _me)) continue;
                        Vector2 screenPos;

                        

                        #region GettingSPells

                        
                        var spells = new Ability[7];
                        try
                        {
                            spells[1] = v.Spellbook.Spell1;
                        }
                        catch
                        {
                        }
                        try
                        {
                            spells[2] = v.Spellbook.Spell2;
                        }
                        catch
                        {
                        }
                        try
                        {
                            spells[3] = v.Spellbook.Spell3;
                        }
                        catch
                        {
                        }
                        try
                        {
                            spells[4] = v.Spellbook.Spell4;
                        }
                        catch
                        {
                        }
                        try
                        {
                            spells[5] = v.Spellbook.Spell5;
                        }
                        catch
                        {
                        }
                        try
                        {
                            spells[6] = v.Spellbook.Spell6;
                        }
                        catch
                        {
                        }

                        #endregion
                        var counter = 0;
                        if (Drawing.WorldToScreen(v.Position, out screenPos))
                        {
                            var start = screenPos + new Vector2(-75, 20);
                            if (v.IsVisible)
                            {
                                for (var g = 6; g >= 1; g--)
                                {
                                    if (spells[g] == null) continue;
                                    counter++;
                                    var cd = spells[g].Cooldown;
                                    Drawing.DrawRect(start + new Vector2(g*20 - 5, 0), new Vector2(20, cd == 0 ? 6 : 20),
                                        new ColorBGRA(0, 0, 0, 100), true);
                                    //PrintError(String.Format("Spell # {0}:{1}", g, spells[g].AbilityState));
                                    if (spells[g].AbilityState == AbilityState.NotEnoughMana)
                                    {
                                        Drawing.DrawRect(start + new Vector2(g*20 - 5, 0),
                                            new Vector2(20, cd == 0 ? 6 : 20),
                                            new ColorBGRA(0, 0, 150, 150));
                                    }
                                    if (cd > 0)
                                    {
                                        var text = string.Format("{0:0.#}", cd);
                                        var textSize = Drawing.MeasureText(text, "Arial", new Vector2(10, 200),
                                            FontFlags.None);
                                        var textPos = (start + new Vector2(g*20 - 5, 0) +
                                                       new Vector2(10 - textSize.X/2, -textSize.Y/2 + 12));
                                        Drawing.DrawText(text, textPos, new Vector2(10, 150), Color.White,
                                            FontFlags.AntiAlias | FontFlags.DropShadow);
                                    }
                                    if (spells[g].Level == 0) continue;
                                    for (var lvl = 1; lvl <= spells[g].Level; lvl++)
                                    {

                                        Drawing.DrawRect(start + new Vector2(g*20 - 5 + 3*lvl, 2), new Vector2(2, 2),
                                            new ColorBGRA(255, 255, 0, 255), true);
                                    }
                                }
                            }
                        }

                        #endregion

                        #region Ultimate Cooldown

                        if (counter >= 4 && ShowCooldownOnTopPanel && spells[counter].Level > 0)
                        {
                            var spellDelta =
                                new Vector2(
                                    spells[counter].Cooldown*_drawHelper.FloatRange/spells[counter].CooldownLength, 0);
                            Drawing.DrawRect(new Vector2(initPos, _drawHelper.BotRange + 1 + _drawHelper.Height*2),
                                new Vector2(_drawHelper.FloatRange, _drawHelper.Height), Color.Gray);
                            Drawing.DrawRect(new Vector2(initPos, _drawHelper.BotRange + 1 + _drawHelper.Height*2),
                                spellDelta + new Vector2(0, _drawHelper.Height), Color.Yellow);
                            Drawing.DrawRect(new Vector2(initPos, _drawHelper.BotRange + 1 + _drawHelper.Height*2),
                                new Vector2(_drawHelper.FloatRange, _drawHelper.Height), Color.Black, true);
                            var text = string.Format("{0:0.}", spells[counter].Cooldown);
                            var textSize = Drawing.MeasureText(text, "Arial", new Vector2(10, 200), FontFlags.None);
                            var textPos = (new Vector2(initPos, _drawHelper.BotRange + 1 + _drawHelper.Height*2) +
                                           new Vector2(textSize.X/2 + 5, -textSize.Y/2 + 10));
                            if (ShowCooldownOnTopPanelLikeText)
                            {
                                Drawing.DrawText(text == "0" ? "Ready" : text, textPos, new Vector2(10, 150),
                                    Color.White,
                                    FontFlags.AntiAlias | FontFlags.DropShadow);
                            }
                        }

                        #endregion
                        
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
        }
        #endregion
        #region Method
        private static void Game_OnUpdate(EventArgs args)
        {
            #region Load/Unload

            if (!_loaded)
            {
                _me = ObjectMgr.LocalHero;
                _player = ObjectMgr.LocalPlayer;
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
                return;
            }

            #endregion

            #region AutoItems

            if (AutoItemsActive && Utils.SleepCheck("AutoItems"))
            {
                if (AutoItemsMidas)
                {
                    var midas = _me.FindItem("item_hand_of_midas");
                    if (midas != null && midas.CanBeCasted() && !_me.IsInvisible())
                    {
                        var creep =
                            ObjectMgr
                                .GetEntities<Unit>(
                                    ).FirstOrDefault(x => ((x is Hero && !x.IsIllusion) || (x is Creep && x.IsSpawned)) && x.IsAlive &&
                                        x.IsVisible && _me.Distance2D(x)<=midas.CastRange && x.Team!=_me.Team);
                        midas.UseAbility(creep);
                        Utils.Sleep(250, "AutoItems");
                        //PrintError("midas.CastRange: " + midas.CastRange);
                    }
                }
                if (AutoItemsPhase)
                {
                    var phase = _me.FindItem("item_phase_boots");
                    if (phase!=null && phase.CanBeCasted() && !_me.IsAttacking() && !_me.IsInvisible())
                    {
                        phase.UseAbility();
                        Utils.Sleep(250,"AutoItems");
                    }
                }
                if (AutoItemsStick)
                {
                    var stick = _me.FindItem("item_magic_stick");
                    var wand = _me.FindItem("item_magic_wand");
                    if (_me.Health*100/_me.MaximumHealth <= 30)
                    {
                        if (stick != null && stick.CanBeCasted() && !_me.IsInvisible())
                        {
                            stick.UseAbility();
                            Utils.Sleep(250, "AutoItems");
                        }
                        if (wand != null && wand.CanBeCasted())
                        {
                            wand.UseAbility();
                            Utils.Sleep(250, "AutoItems");
                        }
                    }
                }
            }

            #endregion

            #region ShowRoshanTimer

            if (ShowRoshanTimer)
            {
                var tickDelta = Game.GameTime - _deathTime;
                _roshanMinutes = Math.Floor(tickDelta/60);
                _roshanSeconds = tickDelta%60;
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
                            x.IsIllusion && x.Team != _player.Team);
                foreach (var s in illusions)
                {
                    HandleEffect(s);
                }
            }

            #endregion
        }
        
        #endregion
        #region Helpers

        private static Vector3 FindVector(Vector3 first , double ret , float distance )
        {
            var retVector = new Vector3(first.X +(float) Math.Cos(Utils.DegreeToRadian(ret)) * distance, first.Y +(float) Math.Sin(Utils.DegreeToRadian(ret))*distance, 100);
            
            return retVector;
        }

        private static double FindRet(Vector3 first, Vector3 second)
        {
            var xAngle = Utils.RadianToDegree(Math.Atan((Math.Abs(second.X - first.X)/Math.Abs(second.Y - first.Y))));
	        if (first.X <= second.X && first.Y >= second.Y)
	        {
	            return (270 + xAngle);
	        }
            if(first.X >= second.X & first.Y >= second.Y)
            {
                return ((90 - xAngle) + 180);
            }
            if(first.X >= second.X && first.Y <= second.Y)
            {
                return(90 + xAngle);
            }
            if(first.X <= second.X && first.Y <= second.Y)
            {
                return (90 - xAngle);
            }
            return 0;
        }
        private static void HandleEffect(Unit unit)
        {
            ParticleEffect effect;
            ParticleEffect effect2;
            if (unit.IsAlive && unit.IsVisibleToEnemies)
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

        private static bool CheckMouse(float x, float y, float sizeX, float sizeY)
        {
            var mousePos = Game.MouseScreenPosition;
            return mousePos.X >= x && mousePos.X <= x + sizeX && mousePos.Y >= y && mousePos.Y <= y + sizeY;
        }

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
        private static void DrawButton(float x, float y, float w, float h, float px, ref bool clicked, bool isActive, Color @on, Color off, Color @select)
        {
            if (isActive)
            {
                var isIn = CheckMouse(x, y, w, h);
                if (_leftMouseIsPress && Utils.SleepCheck("ClickButtonCd") && isIn)
                {
                    clicked = !clicked;
                    //SaveLoadSysHelper.IniWriteValue("Booleans", "Show health on top panel", true.ToString());
                    Utils.Sleep(250, "ClickButtonCd");
                }
                var newColor = isIn
                    ? @select
                    : clicked ? @on : off;
                DrawFilledBox(x, y, w, h, newColor);
                DrawBox(x, y, w, h, px, Color.Black);
            }
            else
            {
                DrawFilledBox(x, y, w, h, Color.Gray);
                DrawBox(x, y, w, h, px, Color.Black);
            }
        }
        /// <summary>
        /// Рисуем кнопку
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="px">ширина</param>
        /// <param name="clicked">булку для тогла</param>
        /// <param name="isActive">активна ли кнопка</param>
        /// <param name="on">цвет при активе</param>
        /// <param name="off">цвет при оффе</param>
        /// <param name="description">описание</param>
        private static void DrawButton(float x, float y, float w, float h, float px, ref bool clicked, bool isActive, Color @on, Color off, string description)
        {
            var isIn = CheckMouse(x, y, w, h);
            if (isActive)
            {
                if (_leftMouseIsPress && Utils.SleepCheck("ClickButtonCd") && isIn)
                {
                    clicked = !clicked;
                    //SaveLoadSysHelper.IniWriteValue("Booleans",description,clicked.ToString());
                    Utils.Sleep(250, "ClickButtonCd");
                }
                var newColor = isIn
                    ? new Color((int) (clicked ? @on.R:off.R),clicked ? @on.G:off.G,clicked ? @on.B:off.B,150)
                    : clicked ? @on : off;
                DrawFilledBox(x, y, w, h, newColor);
                DrawBox(x, y, w, h, px, Color.Black);
            }
            else
            {
                DrawFilledBox(x, y, w, h, Color.Gray);
                DrawBox(x, y, w, h, px, Color.Black);
            }
            if (isIn)
            {
                DrawFilledBox(x - description.Length * 6 - 40, y, description.Length * 6 + 35, 20, new Color(0, 0, 0, 100));
                DrawShadowText(description, (int)x - description.Length * 6 - 40, (int)y, Color.White, FontArray[5]);
            }
        }
        #endregion
    }
}
