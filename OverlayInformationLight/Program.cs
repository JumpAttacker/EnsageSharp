using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using SharpDX;
using SharpDX.Direct3D9;
using Color = SharpDX.Color;
using Font = SharpDX.Direct3D9.Font;

// ReSharper disable EmptyGeneralCatchClause
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace OverlayInformationLight
{
    internal static class Program
    {
        #region Members

        private static bool _loaded;
        private static Hero _me;
        private static readonly string Ver = Assembly.GetExecutingAssembly().GetName().Version+" light";
        private static Unit _arrowUnit;
/*
        private static bool _isOpen=true;
*/
        private static bool _leftMouseIsPress;
        private static readonly Dictionary<Unit, ParticleEffect> Effects2 = new Dictionary<Unit, ParticleEffect>();
        private static readonly Dictionary<Unit, ParticleEffect> Effects1 = new Dictionary<Unit, ParticleEffect>();
        private static readonly ParticleEffect[] ArrowParticalEffects=new ParticleEffect[150];

        private static readonly Dictionary<Hero, Ability> UltimateAbilities = new Dictionary<Hero, Ability>();
        //======================================
        private static readonly StatusInfo[] SInfo=new StatusInfo[10];
        //======================================
        private static bool _showHealthOnTopPanel = true;
        private static bool _showManaOnTopPanel = true;
        private static bool _showRoshanTimer = true;
        private static bool _showIllusions = true;
        private static bool _showMeMore = true;
        private static bool _dangItems = true;
        private static bool _autoItemsActive;
        private static bool _showUltimateCd = true;
        private static bool _showRunes = true;
        private static bool _statusEnemyOnMinimap;
        private static bool _statusEnemyTimer;
        private static bool _showStatusInfoActivated;

        private static bool _showPAonMinimap;
        //=====================================
        private static readonly Dictionary<Unit, ParticleEffect> BaraIndicator = new Dictionary<Unit, ParticleEffect>();
        //=====================================
        private static readonly Dictionary<string, DotaTexture> TextureCache = new Dictionary<string, DotaTexture>();
        //=====================================
        private static readonly ShowMeMoreHelper[] ShowMeMoreH = new ShowMeMoreHelper[5];

        private static readonly Dictionary<Unit, ParticleEffect> ShowMeMoreEffect =
            new Dictionary<Unit, ParticleEffect>();

        private static readonly Dictionary<Unit, ParticleEffect>[] Eff = new Dictionary<Unit, ParticleEffect>[141];
        private static readonly List<Entity> InSystem = new List<Entity>();
        private static Vector3 _arrowS;
        /*private static Unit Bara;
        private static Vector3 BaraStartPos;*/
        //=====================================
        //private static readonly InitHelper SaveLoadSysHelper = new InitHelper("C:"+ "\\jOverlay.ini");
        //=====================================
        private static float _deathTime;
/*
        private static float _aegisTime;
*/
        private static double _roshanMinutes;
        private static double _roshanSeconds;
        private static bool _roshIsAlive;

        //=====================================
        private static readonly Font[] FontArray = new Font[21];
        private static Line _line;
        //private static Vector2 _sizer = new Vector2(110, 500);
        private static bool _letsDraw = true;

        //private static bool _saveSettings;
        //private static bool _printPathLoc;
        //private static readonly string MyPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
        //static readonly InitHelper SaveLoadSysHelper = new InitHelper(MyPath + "\\jOverlayLight.ini");
        private static bool _findAa;
        private static readonly HeroCollector[] Recorder=new HeroCollector[10];
        private static readonly Menu Menu = new Menu("OverlayInformation", "OverlayInformation", true);

        private struct HeroCollector
        {
            public readonly Hero Hero;
            public float Health;
            public float Mana;
            public float MaximumMana;
            public float MaximumHealth;
            public bool IsAlive;
            public readonly float SizeX;
            public readonly float SizeY;
            public Inventory Inventory;

            public HeroCollector(Hero hero, float health, float mana, bool isAlive, float maximumMana, float maximumMana1, Inventory inventory)
                : this()
            {
                Hero = hero;
                Health = health;
                IsAlive = isAlive;
                MaximumMana = maximumMana;
                MaximumMana = maximumMana1;
                Inventory = inventory;
                Mana = mana;
                SizeX = (float)HUDInfo.GetTopPanelSizeX(hero);
                SizeY = (float)HUDInfo.GetTopPanelSizeY(hero);
            }
        }
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
            //ObjectMgr.OnAddEntity += ObjectMgr_OnAddEntity;
            //ObjectMgr.OnRemoveEntity += ObjectMgr_OnRemoveEntity;

            Menu.AddItem(new MenuItem("ShowHealthOnTopPanel", "Show Health on Top").SetValue(true));
            Menu.AddItem(new MenuItem("ShowManaOnTopPanel", "Show Mana on Top").SetValue(true));
            Menu.AddItem(new MenuItem("ShowRoshanTimer", "Show Roshan Timer").SetValue(true));
            Menu.AddItem(new MenuItem("ShowMeMore", "Show Me More").SetValue(true));
            Menu.AddItem(new MenuItem("ShowIllusions", "Show Illusions").SetValue(true));
            Menu.AddItem(new MenuItem("perfomance", "Perfomance").SetValue(new Slider(1,1,500)).SetTooltip("1 - good PC, 500 - wooden PC"));

            var enemyStatus = new Menu("Enemy Status", "enemystatus");
            enemyStatus.AddItem(new MenuItem("ShowStatusInfoActivated", "Activated").SetValue(true));
            enemyStatus.AddItem(new MenuItem("StatusEnemyTimer", "Show Enemy Status").SetValue(true).SetTooltip("show how long enemy in current status (in fog, in invis, under vision)"));
            enemyStatus.AddItem(new MenuItem("_showPAonMinimap", "Show PhantomAssasin on minimap").SetValue(true).SetFontStyle(FontStyle.Bold, Color.Gray));
            
            var dangitems=new Menu("Dangerous Items","dangitems");
            dangitems.AddItem(new MenuItem("DangItems", "Show Dangerous Items").SetValue(true));
            //dangitems.AddItem(new MenuItem("dangItemEnemy", "Only For Enemy").SetValue(true));
            var dict = new Dictionary<string, bool>
            {
                {"item_gem", true},
                {"item_dust", true},
                {"item_sphere", true},
                {"item_blink", true},
                {"item_ward_observer", true},
                {"item_ward_sentry", true},
                {"item_black_king_bar", true},
                {"item_ward_dispenser", true}
            };
            dangitems.AddItem(new MenuItem("dangItemsUsage", "Item List: ").SetValue(new AbilityToggler(dict)));

            var ultimates = new Menu("Ultimates", "ultimates");
            ultimates.AddItem(new MenuItem("ShowUltimateCd", "Show ultimate Icon").SetValue(true));
            ultimates.AddItem(new MenuItem("ShowUltimateCdText", "Show ultimate Text").SetValue(true).SetFontStyle(FontStyle.Bold, Color.Gray));

            var autoItems = new Menu("Auto Items", "autoitems");
            autoItems.AddItem(new MenuItem("AutoItemsActive", "Auto Items Active").SetValue(false));
            var autoitemlist = new Dictionary<string, bool>
            {
                {"item_magic_wand", false},
                {"item_phase_boots", true},
                {"item_hand_of_midas", true}
            };
            autoItems.AddItem(new MenuItem("autoitemsList", "Item List: ").SetValue(new AbilityToggler(autoitemlist)));
            autoItems.AddItem(new MenuItem("autoitemlistHealth", "Minimum Health (%)").SetValue(new Slider(30, 1)));
            autoItems.AddItem(new MenuItem("autoitemlistMana", "Minimum Mana (%)").SetValue(new Slider(30, 1)));

            var runes = new Menu("Rune Notification", "runenotification");
            runes.AddItem(new MenuItem("ShowRunes", "Print Info").SetValue(true));
            runes.AddItem(new MenuItem("ShowRunesMinimap", "Show on Minimap").SetValue(false).SetFontStyle(FontStyle.Bold, Color.Gray));

            var settings = new Menu("Settings", "settings");
            settings.AddItem(new MenuItem("BarPosX", "HP/MP bar Position X").SetValue(new Slider(0, -300, 300)));
            settings.AddItem(new MenuItem("BarPosY", "HP/MP bar Position Y").SetValue(new Slider(0, -300, 300)));
            settings.AddItem(new MenuItem("BarSizeY", "HP/MP bar Size Y").SetValue(new Slider(0, -10, 10)));

            var visibility = new Menu("Visibility", "Visibility");
            visibility.AddItem(new MenuItem("Visibility.Enable", "Enable").SetValue(false));
            visibility.AddItem(new MenuItem("Visibility.Red", "Red").SetValue(new Slider(0, 0, 255)).SetFontStyle(FontStyle.Bold, Color.Red));
            visibility.AddItem(new MenuItem("Visibility.Green", "Green").SetValue(new Slider(0, 0, 255)).SetFontStyle(FontStyle.Bold, Color.Green));
            visibility.AddItem(new MenuItem("Visibility.Blue", "Blue").SetValue(new Slider(100, 0, 255)).SetFontStyle(FontStyle.Bold, Color.Blue));
            visibility.AddItem(new MenuItem("Visibility.Alpha", "Alpha").SetValue(new Slider(50, 0, 255)).SetFontStyle(FontStyle.Bold, Color.WhiteSmoke));

            Menu.AddSubMenu(visibility);
            Menu.AddSubMenu(dangitems);
            Menu.AddSubMenu(ultimates);
            Menu.AddSubMenu(autoItems);
            Menu.AddSubMenu(runes);
            Menu.AddSubMenu(enemyStatus);
            Menu.AddSubMenu(settings);
            
            Menu.AddToMainMenu();

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

            #region SaveLoadSys
            /*
            try
            {
                LoadThis(out ShowHealthOnTopPanel, "ShowHealthOnTopPanel");
                LoadThis(out ShowManaOnTopPanel, "ShowManaOnTopPanel");
                LoadThis(out ShowRoshanTimer, "ShowRoshanTimer");
                LoadThis(out ShowIllusions, "ShowIllusions");
                LoadThis(out ShowMeMore, "ShowMeMore");
                LoadThis(out DangItems, "DangItems");
                LoadThis(out AutoItemsMenu, "AutoItemsMenu");
                LoadThis(out AutoItemsActive, "AutoItemsActive");
                LoadThis(out AutoItemsMidas, "AutoItemsMidas");
                LoadThis(out AutoItemsPhase, "AutoItemsPhase");
                LoadThis(out AutoItemsStick, "AutoItemsStick");
                LoadThis(out ShowUltimateCd, "ShowUltimateCd");
                LoadThis(out ExtraVisionPanel, "ExtraVisionPanel");
                LoadThis(out StatusEnemyTimer, "StatusEnemyTimer");
                LoadThis(out ShowStatusInfo, "ShowStatusInfo");
                LoadThis(out ShowStatusInfoActivated, "ShowStatusInfoActivated");
                LoadThis(out ShowExtraVisionPanel, "ShowExtraVisionPanel");
                LoadThis(out StatusEnemyOnMinimap, "StatusEnemyOnMinimap");
                LoadThis(out ShowRunes, "ShowRunes");
            }
            catch
            {
                SaveAll();

                Console.Beep(1000, 100);
                Console.Beep(1000, 100);
                Console.Beep(1000, 100);
            }
            */
            #endregion
        }
        /*
        private static void ObjectMgr_OnRemoveEntity(EntityEventArgs args)
        {
            var entity = args.Entity as Rune;
            if (entity == null) return;
            var rune = entity;
            Game.PrintMessage("Rune: " + rune.RuneType + " taken", MessageType.LogMessage);
        }

        private static void ObjectMgr_OnAddEntity(EntityEventArgs args)
        {
            var entity = args.Entity as Rune;
            if (entity == null) return;
            var rune = entity;
            /*while (rune.RuneType == RuneType.None)
            {
                
            }*/
            
        //}
    
        #region saveload staff
        /*
        private static void SaveAll()
        {
            SaveThis(ShowHealthOnTopPanel, "ShowHealthOnTopPanel");
            SaveThis(ShowManaOnTopPanel, "ShowManaOnTopPanel");
            SaveThis(ShowRoshanTimer, "ShowRoshanTimer");
            SaveThis(ShowIllusions, "ShowIllusions");
            SaveThis(ShowMeMore, "ShowMeMore");
            SaveThis(DangItems, "DangItems");
            SaveThis(AutoItemsMenu, "AutoItemsMenu");
            SaveThis(AutoItemsActive, "AutoItemsActive");
            SaveThis(AutoItemsMidas, "AutoItemsMidas");
            SaveThis(AutoItemsPhase, "AutoItemsPhase");
            SaveThis(AutoItemsStick, "AutoItemsStick");
            SaveThis(ShowUltimateCd, "ShowUltimateCd");
            SaveThis(ExtraVisionPanel, "ExtraVisionPanel");
            SaveThis(StatusEnemyTimer, "StatusEnemyTimer");
            SaveThis(ShowStatusInfo, "ShowStatusInfo");
            SaveThis(ShowStatusInfoActivated, "ShowStatusInfoActivated");
            SaveThis(ShowExtraVisionPanel, "ShowExtraVisionPanel");
            SaveThis(StatusEnemyOnMinimap, "StatusEnemyOnMinimap");
            SaveThis(ShowRunes, "ShowRunes");
        }

        private static void LoadThis(out bool boolka, string empty)
        {
            boolka = Convert.ToBoolean(SaveLoadSysHelper.IniReadValue("Booleans", empty));
        }

        private static void SaveThis(bool boolka, string empty)
        {
            SaveLoadSysHelper.IniWriteValue("Booleans", empty, boolka.ToString());
        }
        */
        #endregion

        private static void Game_OnGameEvent(FireEventEventArgs args)
        {
            if (args.GameEvent.Name == "dota_roshan_kill")
            {
                _deathTime = Game.GameTime;
                _roshIsAlive = false;
            }
            /*if (args.GameEvent.Name == "spec_item_pickup")
            {
                _aegisTime = Game.GameTime;
            }*/
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
                Game.PrintMessage(
                    "<font face='Comic Sans MS, cursive'><font color='#ff0000'>" +
                    "OverlayInformation loaded!</font><font color='#0055FF'> v" + Ver, MessageType.LogMessage);
                InSystem.Clear();
            }
            
            if (!Game.IsInGame || _me == null)
            {
                _loaded = false;
                PrintInfo("> OverlayInformation unLoaded");
            }

            /*
            if (_saveSettings)
            {
                _saveSettings = false;
                try
                {
                    SaveAll();
                    //PrintSuccess("Saved");
                }
                catch (Exception)
                {
                    PrintError("Can't save");
                }
            }
            if (_printPathLoc)
            {
                _printPathLoc = false;
                if (Utils.SleepCheck("path"))
                {
                    PrintInfo("Your path for settings: " + MyPath);
                    Utils.Sleep(1000, "path");
                }
            }
            */
            _showHealthOnTopPanel = Menu.Item("ShowHealthOnTopPanel").GetValue<bool>();
            _showManaOnTopPanel = Menu.Item("ShowManaOnTopPanel").GetValue<bool>();
            _showRoshanTimer = Menu.Item("ShowRoshanTimer").GetValue<bool>();
            _showIllusions = Menu.Item("ShowIllusions").GetValue<bool>();
            _showMeMore = Menu.Item("ShowMeMore").GetValue<bool>();
            _dangItems = Menu.Item("DangItems").GetValue<bool>();
            _autoItemsActive = Menu.Item("AutoItemsActive").GetValue<bool>();
            _showUltimateCd = Menu.Item("ShowUltimateCd").GetValue<bool>();
            _showRunes = Menu.Item("ShowRunes").GetValue<bool>();
            _statusEnemyTimer = Menu.Item("StatusEnemyTimer").GetValue<bool>();
            _showStatusInfoActivated = Menu.Item("ShowStatusInfoActivated").GetValue<bool>();
            _showPAonMinimap = Menu.Item("_showPAonMinimap").GetValue<bool>();
            /*
            enemyStatus.AddItem(new MenuItem("ShowStatusInfoActivated", "Activated").SetValue(true));
            enemyStatus.AddItem(new MenuItem("StatusEnemyTimer", "Show Enemy Status").SetValue(true).SetTooltip("show how long enemy in current status (in fog, in invis, under vision)"));
            enemyStatus.AddItem(new MenuItem("_showPAonMinimap", "Show PhantomAssasin on minimap").SetFontStyle(FontStyle.Bold, Color.Gray));*/
            #endregion

            if (Utils.SleepCheck("performance"))
            {

                #region Rune Notification

                if (_showRunes)
                {
                    var runes =
                        ObjectMgr.GetEntities<Rune>()
                            .Where(x => x.RuneType != RuneType.None && !InSystem.Contains(x))
                            .ToList();
                    foreach (var rune in runes)
                    {
                        InSystem.Add(rune);
                        Game.PrintMessage(
                            "<font size='20'> Rune: " + "<font face='Comic Sans MS, cursive'><font color='#00aaff'>"
                            + rune.RuneType + " on <font color='#FF0000'>" + (rune.Position.X < 0 ? "TOP" : "BOT") +
                            " </font>",
                            MessageType.LogMessage);

                    }
                }

                #endregion

                for (uint i = 0; i < 10; i++)
                {
                    Hero v;
                    try
                    {
                        v = ObjectMgr.GetPlayerById(i).Hero;
                        if (v == null || !v.IsValid || !v.Player.IsValid || v.Player.ConnectionState != ConnectionState.Connected) continue;
                    }
                    catch
                    {
                        continue;
                    }
                    if (!Equals(Recorder[i].Hero, v))
                    {
                        Recorder[i] = new HeroCollector(v, v.Health, v.Mana, v.IsAlive, v.MaximumHealth, v.MaximumMana,
                            v.Inventory);
                    }
                    Recorder[i].Health = v.Health;
                    Recorder[i].Mana = v.Mana;
                    Recorder[i].MaximumHealth = v.MaximumHealth;
                    Recorder[i].MaximumMana = v.MaximumMana;
                    Recorder[i].IsAlive = v.IsAlive;
                    Recorder[i].Inventory = v.Inventory;
                }
                Utils.Sleep(Menu.Item("perfomance").GetValue<Slider>().Value, "perfomance");
            }

            #region Show me more cast
            
            //List<Unit> dummy;
            if (_showMeMore)
            {
                var dummy = ObjectMgr.GetEntities<Unit>().Where(x => x.ClassID == ClassID.CDOTA_BaseNPC && x.Team!=_me.Team).ToList();
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
                    if (t.DayVision == 550)
                    {
                        if (_findAa) continue;
                        _findAa = true;
                        GenerateSideMessage("ancient_apparition", "ancient_apparition_ice_blast");
                    }
                    else
                    {
                        _findAa = false;
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

            if (_showRoshanTimer)
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

            if (_showIllusions)
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

            #region AutoItems

            if (!_autoItemsActive || !Utils.SleepCheck("AutoItems") || !_me.IsAlive) return;
            if (Menu.Item("autoitemsList").GetValue<AbilityToggler>().IsEnabled("item_hand_of_midas"))
            {
                var midas = _me.FindItem("item_hand_of_midas");
                if (midas != null && midas.CanBeCasted() && !_me.IsInvisible())
                {
                    var creep =
                        ObjectMgr.GetEntities<Unit>()
                            .FirstOrDefault(
                                x =>
                                    x.Team != _me.Team &&
                                    (x.ClassID == ClassID.CDOTA_BaseNPC_Creep_Lane ||
                                     x.ClassID == ClassID.CDOTA_BaseNPC_Creep_Siege ||
                                     x.ClassID == ClassID.CDOTA_BaseNPC_Creep_Neutral) && x.IsSpawned && x.IsAlive &&
                                    x.Distance2D(_me) <= 600);
                    midas.UseAbility(creep);
                    Utils.Sleep(250, "AutoItems");
                    //PrintError("midas.CastRange: " + midas.CastRange);
                }
            }
            if (Menu.Item("autoitemsList").GetValue<AbilityToggler>().IsEnabled("item_phase_boots"))
            {
                var phase = _me.FindItem("item_phase_boots");
                if (phase != null && phase.CanBeCasted() && !_me.IsAttacking() && !_me.IsInvisible() && !_me.IsChanneling() && _me.NetworkActivity==NetworkActivity.Move)
                {
                    phase.UseAbility();
                    Utils.Sleep(250, "AutoItems");
                }
            }
            if (!Menu.Item("autoitemsList").GetValue<AbilityToggler>().IsEnabled("item_magic_wand")) return;
            var stick = _me.Inventory.Items.FirstOrDefault(x => (x.Name == "item_magic_stick" || x.Name == "item_magic_wand") && x.CanBeCasted() && x.CurrentCharges>0);
            if (!_me.IsAlive) return;
            if (_me.Health*100/_me.MaximumHealth >= Menu.Item("autoitemlistHealth").GetValue<Slider>().Value &&
                !(_me.Mana*100/_me.MaximumMana < Menu.Item("autoitemlistMana").GetValue<Slider>().Value)) return;
            if (stick == null || _me.IsInvisible()) return;
            stick.UseAbility();
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

            if (!_showRoshanTimer) return;
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

            #region Testing

            if (Menu.Item("Visibility.Enable").GetValue<bool>())
            {
                for (uint i = 0; i < 10; i++)
                {
                    try
                    {
                        if (Recorder[i].Hero == null || !Recorder[i].Hero.IsValid || !Recorder[i].Hero.Player.IsValid ||
                            Recorder[i].Hero.Player.ConnectionState != ConnectionState.Connected ||
                            !Recorder[i].Hero.IsVisibleToEnemies || Recorder[i].Hero.Team != _me.Team) continue;
                        var v = Recorder[i].Hero; //ObjectMgr.GetPlayerById(i).Hero;
                        var pos = HUDInfo.GetTopPanelPosition(v);
                        var size = new Vector2((float) HUDInfo.GetTopPanelSizeX(v), (float) HUDInfo.GetTopPanelSizeY(v));
                        DrawFilledBox(pos, size,
                            new Color(Menu.Item("Visibility.Red").GetValue<Slider>().Value,
                                Menu.Item("Visibility.Green").GetValue<Slider>().Value,
                                Menu.Item("Visibility.Blue").GetValue<Slider>().Value,
                                Menu.Item("Visibility.Alpha").GetValue<Slider>().Value));
                    }
                    catch
                    {

                    }
                }
            }

            #endregion

        }

        #endregion

        #region Drawing_OnDraw

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Game.IsInGame || _me == null || !_loaded || Game.IsPaused) return;

            #region J - overlay
            /*
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
                    pos += 22;
                    DrawButton(startLoc + new Vector2(5, pos), _sizer.X - 10, 20, ref ShowUltimateCd, true, new Color(0, 200, 0, 50), new Color(200, 0, 0, 50), "Show ultimates");
                    pos += 22;
                    DrawButton(startLoc + new Vector2(5, pos), _sizer.X - 10, 20, ref AutoItemsMenu, true, new Color(0, 200, 0, 50), new Color(200, 0, 0, 50), "Auto Items Menu");
                    if (AutoItemsMenu)
                    {
                        ShowStatusInfo = false;
                        Drawing.DrawRect(startLoc - new Vector2(_sizer.X + 2, 0), _sizer, new Color(0, 0, 0, 100));
                        var pos2 = 5;
                        DrawButton(startLoc - new Vector2(_sizer.X + 2, 0) + new Vector2(5, pos2), _sizer.X - 10, 20, ref AutoItemsActive, true, new Color(0, 200, 0, 50), new Color(200, 0, 0, 50), "Activated");
                        pos2 += 22;
                        DrawButton(startLoc - new Vector2(_sizer.X + 2, 0) + new Vector2(5, pos2), _sizer.X - 10, 20, ref AutoItemsMidas, true, new Color(0, 200, 0, 50), new Color(200, 0, 0, 50), "Midas");
                        pos2 += 22;
                        DrawButton(startLoc - new Vector2(_sizer.X + 2, 0) + new Vector2(5, pos2), _sizer.X - 10, 20, ref AutoItemsPhase, true, new Color(0, 200, 0, 50), new Color(200, 0, 0, 50), "PhaseBoots");
                        pos2 += 22;
                        DrawButton(startLoc - new Vector2(_sizer.X + 2, 0) + new Vector2(5, pos2), _sizer.X - 10, 20, ref AutoItemsStick, true, new Color(0, 200, 0, 50), new Color(200, 0, 0, 50), "Sticks");
                    }
                    pos += 22;
                    DrawButton(startLoc + new Vector2(5, pos), _sizer.X - 10, 20, ref ShowStatusInfo, true, new Color(0, 200, 0, 50), new Color(200, 0, 0, 50), "Status Info");
                    if (ShowStatusInfo)
                    {
                        AutoItemsMenu = false;
                        Drawing.DrawRect(startLoc - new Vector2(_sizer.X + 2, 0), _sizer, new Color(0, 0, 0, 100));
                        var pos2 = 5;
                        DrawButton(startLoc - new Vector2(_sizer.X + 2, 0) + new Vector2(5, pos2), _sizer.X - 10, 20,
                            ref ShowStatusInfoActivated, true, new Color(0, 200, 0, 50), new Color(200, 0, 0, 50),
                            "Activated");
                        pos2 += 22;
                        DrawButton(startLoc - new Vector2(_sizer.X + 2, 0) + new Vector2(5, pos2), _sizer.X - 10, 20,
                            ref StatusEnemyTimer, true, new Color(0, 200, 0, 50), new Color(200, 0, 0, 50), "EnemyTimer");
                        pos2 += 22;
                        DrawButton(startLoc - new Vector2(_sizer.X + 2, 0) + new Vector2(5, pos2), _sizer.X - 10, 20,
                            ref StatusEnemyOnMinimap, false, new Color(0, 200, 0, 50), new Color(200, 0, 0, 50), "LastPosOnMinMap");
                        pos2 += 22;
                        DrawButton(startLoc - new Vector2(_sizer.X + 2, 0) + new Vector2(5, pos2), _sizer.X - 10, 20,
                            ref _showPAonMinimap, false, new Color(0, 200, 0, 50), new Color(200, 0, 0, 50), "Show PA");
                    }
                    pos += 22;
                    DrawButton(startLoc + new Vector2(5, pos), _sizer.X - 10, 20, ref ShowRunes, true, new Color(0, 200, 0, 50), new Color(200, 0, 0, 50), "Rune Notification");

                    DrawButton(startLoc + new Vector2(5, _sizer.Y - 35), _sizer.X - 10, 30, ref _saveSettings, true, new Color(0, 200, 0, 50), new Color(200, 200, 0, 50), "Save Settings");
                    DrawButton(startLoc + new Vector2(5, _sizer.Y - 57), _sizer.X - 10, 22, ref _printPathLoc, true, new Color(0, 200, 0, 50), new Color(200, 200, 0, 50), "Print Path Loc");
                }
            }
            else
            {
                _sizer.Y -= 10;
                _sizer.Y = Math.Max(_sizer.Y, 0);
                Drawing.DrawRect(startLoc, _sizer, new Color(0, 0, 0, 100));
            }
            DrawButton(startLoc, _sizer.X, 30, ref _isOpen, true, new Color(0, 0, 0, 50), new Color(0, 0, 0, 125), "J-Overlay Light");
            */
            #endregion

            for (uint i = 0; i < 10; i++)
            {
                #region Init
                /*
                Hero v;
                try
                {
                    v = ObjectMgr.GetPlayerById(i).Hero;
                }
                catch
                {
                    continue;
                }
                if (v == null) continue;*/
                if (Recorder[i].Hero == null || !Recorder[i].Hero.IsValid || !Recorder[i].Hero.Player.IsValid || Recorder[i].Hero.Player.ConnectionState!=ConnectionState.Connected) continue;

                var v = Recorder[i];
                var pos = HUDInfo.GetTopPanelPosition(v.Hero) + new Vector2(Menu.Item("BarPosX").GetValue<Slider>().Value, Menu.Item("BarPosY").GetValue<Slider>().Value);
                var sizeX = Recorder[i].SizeX;
                var sizeY = Recorder[i].SizeY+(Menu.Item("BarSizeY").GetValue<Slider>().Value);
                var healthDelta = new Vector2(Recorder[i].Health * sizeX / v.MaximumHealth, 0);
                var manaDelta = new Vector2(Recorder[i].Mana * sizeX / v.MaximumMana, 0);
                const int height = 7;

                #endregion

                #region Health

                if (_showHealthOnTopPanel)
                {
                    Drawing.DrawRect(pos + new Vector2(0, sizeY + 1), new Vector2(sizeX, height),
                        new Color(255, 0, 0, 255));
                    Drawing.DrawRect(pos + new Vector2(0, sizeY + 1), new Vector2(healthDelta.X, height),
                        new Color(0, 255, 0, 255));
                    Drawing.DrawRect(pos + new Vector2(0, sizeY + 1), new Vector2(sizeX, height), Color.Black, true);
                }

                #endregion

                #region Mana


                if (_showManaOnTopPanel)
                {
                    Drawing.DrawRect(pos + new Vector2(0, sizeY + height), new Vector2(sizeX, height), Color.Gray);
                    Drawing.DrawRect(pos + new Vector2(0, sizeY + height), new Vector2(manaDelta.X, height),
                        new Color(0, 0, 255, 255));
                    Drawing.DrawRect(pos + new Vector2(0, sizeY + height), new Vector2(sizeX, height), Color.Black, true);
                }

                #endregion

                #region StatusInfo

                if (_showStatusInfoActivated)
                {
                    if (v.Hero.Team != _me.Team)
                    {
                        if (_statusEnemyTimer)
                        {
                            if (SInfo[i] == null || !SInfo[i].GetHero().IsValid)
                            {
                                SInfo[i] = new StatusInfo(v.Hero, Game.GameTime);
                            }
                            Drawing.DrawRect(pos + new Vector2(0, sizeY + height*2), new Vector2(sizeX, height*2),
                                new Color(0, 0, 0, 100));
                            Drawing.DrawRect(pos + new Vector2(0, sizeY + height*2), new Vector2(sizeX, height*2),
                                new Color(0, 0, 0, 255), true);
                            var text = SInfo[i].GetTime();
                            Drawing.DrawText(text, pos + new Vector2(5, sizeY + height*2), Color.White,
                                FontFlags.AntiAlias | FontFlags.DropShadow);
                        }
                        if (_statusEnemyOnMinimap)
                        {

                        }
                        if (_showPAonMinimap)
                        {

                        }
                    }
                }

                #endregion

                #region Dang Items & Ultimates

                if (v.Hero.Team != _me.Team)
                {
                    #region DangBang

                    if (_dangItems && v.Hero.IsVisible && v.IsAlive)
                    {
                        var invetory =
                            v.Inventory.Items.Where(
                                x =>
                                    (Menu.Item("dangItemsUsage").GetValue<AbilityToggler>().IsEnabled(x.Name)));
                        var iPos = HUDInfo.GetHPbarPosition(v.Hero);
                        var iSize = new Vector2(HUDInfo.GetHPBarSizeX(v.Hero), HUDInfo.GetHpBarSizeY(v.Hero));
                        float count = 0;
                        foreach (var item in invetory)
                        {
                            var itemname = string.Format("materials/ensage_ui/items/{0}.vmat",
                                item.Name.Replace("item_", ""));
                            Drawing.DrawRect(iPos + new Vector2(count, 50),
                                new Vector2(iSize.X/3, (float) (iSize.Y*2.5)),
                                GetTexture(itemname));
                            if (item.AbilityState == AbilityState.OnCooldown)
                            {
                                var cd = ((int) item.Cooldown).ToString(CultureInfo.InvariantCulture);
                                Drawing.DrawText(cd, iPos + new Vector2(count, 40), Color.White,
                                    FontFlags.AntiAlias | FontFlags.DropShadow);
                            }
                            if (item.AbilityState == AbilityState.NotEnoughMana)
                            {
                                Drawing.DrawRect(iPos + new Vector2(count, 50),
                                    new Vector2(iSize.X/4, (float) (iSize.Y*2.5)), new Color(0, 0, 200, 100));
                            }
                            count += iSize.X/4;
                        }
                    }

                    #endregion

                    #region ShowUltimate

                    if (_showUltimateCd)
                    {
                        try
                        {
                            Ability ultimate;
                            if (!UltimateAbilities.TryGetValue(v.Hero, out ultimate))
                            {
                                var ult = v.Hero.Spellbook.Spells.First(x => x.AbilityType == AbilityType.Ultimate);
                                if (ult != null) UltimateAbilities.Add(v.Hero, ult);
                            }
                            else if (ultimate != null && ultimate.Level > 0 && ultimate.AbilityBehavior != AbilityBehavior.Passive)
                            {
                                var ultPos = pos + new Vector2(sizeX / 2 - 5, sizeY + 1);
                                string path;

                                switch (ultimate.AbilityState)
                                {
                                    case AbilityState.NotEnoughMana:
                                        path = "materials/ensage_ui/other/ulti_nomana.vmat";
                                        break;
                                    case AbilityState.OnCooldown:
                                        path = "materials/ensage_ui/other/ulti_cooldown.vmat";
                                        break;
                                    default:
                                        path = "materials/ensage_ui/other/ulti_ready.vmat";
                                        break;
                                }
                                Drawing.DrawRect(ultPos, new Vector2(14, 14), GetTexture(path));
                            }
                        }
                        catch (Exception)
                        {

                        }
                        
                    }

                    #endregion

                }

                #endregion

                #region ShowMeMore
                if (_showMeMore)
                {
                    var mod = v.Hero.Modifiers.Any(x => x.Name == "modifier_spirit_breaker_charge_of_darkness_vision");
                    if (mod /* && Bara!=null*/)
                    {
                        var textPos = (pos + new Vector2(0, sizeY + height*2));
                        Drawing.DrawText("Spirit Breaker", textPos, new Vector2(15, 150), Color.White,
                            FontFlags.AntiAlias | FontFlags.DropShadow);
                        if (Equals(_me, v.Hero))
                        {
                            Drawing.DrawRect(new Vector2(0, 0), new Vector2(Drawing.Width, Drawing.Height),
                                new Color(255, 0, 0, 2));
                        }
                        ParticleEffect eff;
                        if (!BaraIndicator.TryGetValue(v.Hero, out eff))
                        {
                            eff = new ParticleEffect("particles/hw_fx/cursed_rapier.vpcf", v.Hero);
                            eff.SetControlPointEntity(1, v.Hero);
                            BaraIndicator.Add(v.Hero, eff);
                            GenerateSideMessage(v.Hero.Name.Replace("npc_dota_hero_", ""), "spirit_breaker_charge_of_darkness");
                        }
                    }
                    else
                    {
                        ParticleEffect eff;
                        if (BaraIndicator.TryGetValue(v.Hero, out eff))
                        {
                            eff.Dispose();
                            BaraIndicator.Remove(v.Hero);
                        }
                    }
                }

                

                if (_showMeMore && v.Hero.Team != _me.Team)
                {
                    switch (v.Hero.ClassID)
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
                                    GenerateSideMessage(v.Hero.Name.Replace("npc_dota_hero_", ""), "mirana_arrow");
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
                                var spell = v.Hero.Spellbook.Spell2;
                                if (spell != null && spell.Cooldown != 0)
                                {
                                    var cd = Math.Floor(spell.Cooldown * 100);
                                    if (cd < 880)
                                    {
                                        if (!InSystem.Contains(v.Hero))
                                        {
                                            if (cd > 720)
                                            {
                                                var eff = new ParticleEffect[148];
                                                for (var z = 1; z <= 140; z++)
                                                {
                                                    try
                                                    {
                                                        var p = new Vector3(
                                                            v.Hero.Position.X + 100 * z * (float)Math.Cos(v.Hero.RotationRad),
                                                            v.Hero.Position.Y + 100 * z * (float)Math.Sin(v.Hero.RotationRad),
                                                            100);
                                                        eff[z] =
                                                            new ParticleEffect(
                                                                @"particles\ui_mouseactions\draw_commentator.vpcf",
                                                                p);
                                                        eff[z].SetControlPoint(1, new Vector3(255, 255, 255));
                                                        eff[z].SetControlPoint(0, p);
                                                        Eff[z].Add(v.Hero, eff[z]);
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        PrintSuccess("[WIND RUNNER EX]" + ex);
                                                    }

                                                }
                                                InSystem.Add(v.Hero);
                                            }
                                        }
                                        else if (cd < 720 || !v.Hero.IsAlive)
                                        {
                                            InSystem.Remove(v.Hero);
                                            for (var z = 1; z <= 140; z++)
                                            {
                                                ParticleEffect eff;
                                                if (Eff[z].TryGetValue(v.Hero, out eff))
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
        private static void GenerateSideMessage(string hero, string spellName)
        {
            var msg = new SideMessage(hero, new Vector2(200, 60));
            msg.AddElement(new Vector2(006, 06), new Vector2(72, 36), GetTexture("materials/ensage_ui/heroes_horizontal/" + hero + ".vmat"));
            msg.AddElement(new Vector2(078, 12), new Vector2(64, 32), GetTexture("materials/ensage_ui/other/arrow_usual.vmat"));
            msg.AddElement(new Vector2(142, 06), new Vector2(72, 36), GetTexture("materials/ensage_ui/spellicons/" + spellName + ".vmat"));
            msg.CreateMessage();
        }

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

            vLine[0].X = a.X + (b.X + 1)/2;
            vLine[0].Y = a.Y;
            vLine[1].X = a.X + (b.X + 1)/2;
            vLine[1].Y = a.Y + b.Y;

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
                if (drawOnButtonText != "")
                {
                    Drawing.DrawText(drawOnButtonText, a + new Vector2(10, 2), Color.White,
                        FontFlags.AntiAlias | FontFlags.DropShadow);
                }
            }
        }

        #endregion
    }
}