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
using Ensage.Common.Objects;
using SharpDX;
using SharpDX.Direct3D9;
using Color = SharpDX.Color;
using Font = SharpDX.Direct3D9.Font;
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace OverlayInformation
{
    internal static class Program
    {
        #region Variables

        private static readonly Menu Menu = new Menu("OverlayInformation", "OI", true);

        private static Hero MeHero { get; set; }
        private static Player MePlayer { get; set; }

        private static readonly StatusInfo[] SInfo = new StatusInfo[10];

        private static readonly Dictionary<Unit, ParticleEffect> ShowMeMoreEffect =
            new Dictionary<Unit, ParticleEffect>();

        private static readonly Dictionary<Hero, Ability> UltimateAbilities = new Dictionary<Hero, Ability>();

        private static readonly List<Entity> InSystem = new List<Entity>();
        private static readonly Dictionary<Unit, ParticleEffect> BaraIndicator = new Dictionary<Unit, ParticleEffect>();
        private static Vector3 _arrowS;
        private static readonly Dictionary<Unit, ParticleEffect> Effects2 = new Dictionary<Unit, ParticleEffect>();
        private static readonly Dictionary<Unit, ParticleEffect> Effects1 = new Dictionary<Unit, ParticleEffect>();
        private static readonly ParticleEffect[] ArrowParticalEffects = new ParticleEffect[150];
        private static readonly Dictionary<Unit, ParticleEffect[]> Eff = new Dictionary<Unit, ParticleEffect[]>();
        private static bool _findAa;
        private static float _deathTime;
        private static double _roshanMinutes;
        private static double _roshanSeconds;
        private static bool _roshIsAlive;
        private static Font _roshanFont;
        private static bool _letsDraw = true;

        private static readonly ShowMeMoreStruct[] ShowMeMoreEffects =
        {
            new ShowMeMoreStruct("modifier_invoker_sun_strike",
                "hero_invoker/invoker_sun_strike_team", 175),
            new ShowMeMoreStruct("modifier_lina_light_strike_array",
                "hero_lina/lina_spell_light_strike_array_ring_collapse", 225),
            new ShowMeMoreStruct("modifier_kunkka_torrent_thinker",
                "hero_kunkka/kunkka_spell_torrent_pool", 225),
            new ShowMeMoreStruct("modifier_leshrac_split_earth_thinker",
                "hero_leshrac/leshrac_split_earth_b", 225)
        };

        #endregion

        private struct ShowMeMoreStruct
        {
            public readonly string Modifier;
            public readonly string Effect;
            public readonly int Range;

            public ShowMeMoreStruct(string mod, string eff, int i)
            {
                Modifier = mod;
                Effect = eff;
                Range = i;
            }
        }
        
        private static void Main()
        {
            var toppanel = new Menu("TopPanel", "TopPanel");
            toppanel.AddItem(new MenuItem("TopPanel.Hp", "Show Health on Top").SetValue(true));
            toppanel.AddItem(new MenuItem("TopPanel.Mana", "Show Mana on Top").SetValue(true));
            toppanel.AddItem(new MenuItem("TopPanel.Roshan", "Show Roshan Timer").SetValue(true));
            
            var extra = new Menu("Extra", "Extra");
            extra.AddItem(new MenuItem("ShowIllusions.Enable", "Show Illusions").SetValue(true));
            //Menu.AddItem(new MenuItem("perfomance", "Perfomance").SetValue(new Slider(1,1,500)).SetTooltip("1 - good PC, 500 - wooden PC"));

            var showmemore = new Menu("Show Me More", "showMeMore");
            showmemore.AddItem(new MenuItem("showMeMore.Enable", "Show Me More").SetValue(true));
            showmemore.AddItem(new MenuItem("SideMessage.Enable", "Show SideMessage").SetValue(true));
            
            var types = new Menu("Types", "Types");
            var sunstrike = new Menu("", "modifier_invoker_sun_strike", false, "invoker_sun_strike",true);
            var lightstrike = new Menu("", "modifier_lina_light_strike_array", false, "lina_light_strike_array", true);
            var torrent = new Menu("", "modifier_kunkka_torrent_thinker", false, "kunkka_torrent", true);
            var splitearth = new Menu("", "modifier_leshrac_split_earth_thinker", false, "leshrac_split_earth", true);
            var charge = new Menu("", "modifier_spirit_breaker_charge_of_darkness_vision", false, "spirit_breaker_charge_of_darkness", true);

            sunstrike.AddItem(new MenuItem("modifier_invoker_sun_strike.Red", "Red").SetValue(new Slider(255, 0, 255)).SetFontStyle(FontStyle.Bold, Color.Red));
            sunstrike.AddItem(new MenuItem("modifier_invoker_sun_strike.Green", "Green").SetValue(new Slider(0, 0, 255)).SetFontStyle(FontStyle.Bold, Color.Green));
            sunstrike.AddItem(new MenuItem("modifier_invoker_sun_strike.Blue", "Blue").SetValue(new Slider(0, 0, 255)).SetFontStyle(FontStyle.Bold, Color.Blue));

            lightstrike.AddItem(new MenuItem("modifier_lina_light_strike_array.Red", "Red").SetValue(new Slider(255, 0, 255)).SetFontStyle(FontStyle.Bold, Color.Red));
            lightstrike.AddItem(new MenuItem("modifier_lina_light_strike_array.Green", "Green").SetValue(new Slider(255, 0, 255)).SetFontStyle(FontStyle.Bold, Color.Green));
            lightstrike.AddItem(new MenuItem("modifier_lina_light_strike_array.Blue", "Blue").SetValue(new Slider(255, 0, 255)).SetFontStyle(FontStyle.Bold, Color.Blue));

            torrent.AddItem(new MenuItem("modifier_kunkka_torrent_thinker.Red", "Red").SetValue(new Slider(255, 0, 255)).SetFontStyle(FontStyle.Bold, Color.Red));
            torrent.AddItem(new MenuItem("modifier_kunkka_torrent_thinker.Green", "Green").SetValue(new Slider(255, 0, 255)).SetFontStyle(FontStyle.Bold, Color.Green));
            torrent.AddItem(new MenuItem("modifier_kunkka_torrent_thinker.Blue", "Blue").SetValue(new Slider(255, 0, 255)).SetFontStyle(FontStyle.Bold, Color.Blue));

            splitearth.AddItem(new MenuItem("modifier_leshrac_split_earth_thinker.Red", "Red").SetValue(new Slider(255, 0, 255)).SetFontStyle(FontStyle.Bold, Color.Red));
            splitearth.AddItem(new MenuItem("modifier_leshrac_split_earth_thinker.Green", "Green").SetValue(new Slider(255, 0, 255)).SetFontStyle(FontStyle.Bold, Color.Green));
            splitearth.AddItem(new MenuItem("modifier_leshrac_split_earth_thinker.Blue", "Blue").SetValue(new Slider(255, 0, 255)).SetFontStyle(FontStyle.Bold, Color.Blue));

            charge.AddItem(new MenuItem("tooltip", "When Charge on your Main Hero").SetFontStyle(FontStyle.Bold, Color.Red));
            charge.AddItem(new MenuItem("modifier_spirit_breaker_charge_of_darkness_vision.Red", "Red").SetValue(new Slider(255, 0, 255)).SetFontStyle(FontStyle.Bold, Color.Red));
            charge.AddItem(new MenuItem("modifier_spirit_breaker_charge_of_darkness_vision.Green", "Green").SetValue(new Slider(0, 0, 255)).SetFontStyle(FontStyle.Bold, Color.Green));
            charge.AddItem(new MenuItem("modifier_spirit_breaker_charge_of_darkness_vision.Blue", "Blue").SetValue(new Slider(0, 0, 255)).SetFontStyle(FontStyle.Bold, Color.Blue));
            charge.AddItem(new MenuItem("modifier_spirit_breaker_charge_of_darkness_vision.Alpha", "Alpha").SetValue(new Slider(2, 0, 255)).SetFontStyle(FontStyle.Bold, Color.WhiteSmoke));


            var enemyStatus = new Menu("Enemy Status", "enemystatus");
            enemyStatus.AddItem(new MenuItem("EnemyStatus.Enable", "Activated").SetValue(true));
            enemyStatus.AddItem(new MenuItem("EnemyStatus.Timer.Enable", "Show Enemy Status").SetValue(true).SetTooltip("show how long enemy in current status (in fog, in invis, under vision)"));
            enemyStatus.AddItem(new MenuItem("EnemyStatus.PaOnMinimap.Enable", "Show PhantomAssasin on minimap").SetValue(true).SetFontStyle(FontStyle.Bold, Color.Gray));
            enemyStatus.AddItem(new MenuItem("EnemyStatus.Map.Enable", "Show Enemy Last Position on Map").SetValue(true));
            enemyStatus.AddItem(new MenuItem("EnemyStatus.Vision.Enable", "Show on top panel Ally's vision status").SetValue(true));
            
            var dangitems=new Menu("Dangerous Items","dangitems");
            dangitems.AddItem(new MenuItem("DangItems.Enable", "Show Dangerous Items").SetValue(false));
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
            ultimates.AddItem(new MenuItem("Ultimates.Icon.Enable", "Show ultimate Icon").SetValue(true));
            ultimates.AddItem(new MenuItem("Ultimates.Text.Enable", "Show ultimate Text").SetValue(true).SetFontStyle(FontStyle.Bold, Color.Gray));

            var autoItems = new Menu("Auto Items", "autoitems");
            autoItems.AddItem(new MenuItem("AutoItems.Enable", "Auto Items Active").SetValue(false));
            var autoitemlist = new Dictionary<string, bool>
            {
                {"item_magic_wand", false},
                {"item_phase_boots", true},
                {"item_hand_of_midas", true}
            };
            autoItems.AddItem(new MenuItem("AutoItems.List", "Item List: ").SetValue(new AbilityToggler(autoitemlist)));
            autoItems.AddItem(new MenuItem("AutoItems.Health", "Minimum Health (%)").SetValue(new Slider(30, 1)));
            autoItems.AddItem(new MenuItem("AutoItems.Mana", "Minimum Mana (%)").SetValue(new Slider(30, 1)));

            var runes = new Menu("Rune Notification", "runenotification");
            runes.AddItem(new MenuItem("RuneNotification.Enable", "Print Info").SetValue(true));
            runes.AddItem(new MenuItem("RuneNotification.onMap.Enable", "Show on Minimap").SetValue(false).SetFontStyle(FontStyle.Bold, Color.Gray));

            var spellpanel = new Menu("SpellPanel", "SpellPanel");
            spellpanel.AddItem(new MenuItem("SpellPanel.Enable", "Show Ability").SetValue(true));
            spellpanel.AddItem(new MenuItem("SpellPanel.EnableForAlly", "Enable For Ally").SetValue(true));
            spellpanel.AddItem(new MenuItem("SpellPanel.distBetweenSpells", "Distance spells").SetValue(new Slider(0, 0, 200)));
            spellpanel.AddItem(new MenuItem("SpellPanel.DistBwtweenLvls", "Distance lvls").SetValue(new Slider(0, 0, 200)));
            spellpanel.AddItem(new MenuItem("SpellPanel.SizeSpell", "Level size").SetValue(new Slider(0, 1, 25)));
            spellpanel.AddItem(new MenuItem("SpellPanel.ExtraPosX", "Extra Position X").SetValue(new Slider(25)));
            spellpanel.AddItem(new MenuItem("SpellPanel.ExtraPosY", "Extra Position Y").SetValue(new Slider(125, 0, 400)));
            
            var settings = new Menu("Settings", "settings");
            settings.AddItem(new MenuItem("BarPosX", "HP/MP bar Position X").SetValue(new Slider(0, -300, 300)));
            settings.AddItem(new MenuItem("BarPosY", "HP/MP bar Position Y").SetValue(new Slider(0, -300, 300)));
            settings.AddItem(new MenuItem("BarSizeY", "HP/MP bar Size Y").SetValue(new Slider(0, -10, 10)));

            /*var visibility = new Menu("Visibility", "Visibility");
            visibility.AddItem(new MenuItem("Visibility.Enable", "Enable").SetValue(false).SetFontStyle(FontStyle.Bold, Color.Gray).SetTooltip("can cause game crashing"));
            visibility.AddItem(new MenuItem("Visibility.Red", "Red").SetValue(new Slider(0, 0, 255)).SetFontStyle(FontStyle.Bold, Color.Red));
            visibility.AddItem(new MenuItem("Visibility.Green", "Green").SetValue(new Slider(0, 0, 255)).SetFontStyle(FontStyle.Bold, Color.Green));
            visibility.AddItem(new MenuItem("Visibility.Blue", "Blue").SetValue(new Slider(100, 0, 255)).SetFontStyle(FontStyle.Bold, Color.Blue));
            visibility.AddItem(new MenuItem("Visibility.Alpha", "Alpha").SetValue(new Slider(50, 0, 255)).SetFontStyle(FontStyle.Bold, Color.WhiteSmoke));*/

            


            //Menu.AddSubMenu(visibility);
            types.AddSubMenu(charge);
            types.AddSubMenu(sunstrike);
            types.AddSubMenu(lightstrike);
            types.AddSubMenu(torrent);
            types.AddSubMenu(splitearth);
            showmemore.AddSubMenu(types);
            Menu.AddSubMenu(toppanel);
            toppanel.AddSubMenu(ultimates);
            Menu.AddSubMenu(spellpanel);
            Menu.AddSubMenu(showmemore);
            Menu.AddSubMenu(dangitems);
            extra.AddSubMenu(autoItems);
            extra.AddSubMenu(runes);
            toppanel.AddSubMenu(enemyStatus);
            Menu.AddSubMenu(extra);
            Menu.AddSubMenu(settings);
            
            Menu.AddToMainMenu();

            _roshanFont = new Font(
                Drawing.Direct3DDevice9,
                new FontDescription
                {
                    FaceName = "Tahoma",
                    Height = 15,
                    OutputPrecision = FontPrecision.Default,
                    Quality = FontQuality.Default
                });
            Events.OnLoad += (sender, args) =>
            {
                MePlayer = ObjectMgr.LocalPlayer;
                MeHero = ObjectMgr.LocalHero;
                Game.OnUpdate += GameOnOnUpdate;
                Drawing.OnDraw+=Drawing_OnDraw;
                Drawing.OnPreReset += Drawing_OnPreReset;
                Drawing.OnPostReset += Drawing_OnPostReset;
                Drawing.OnEndScene += Drawing_OnEndScene;
                AppDomain.CurrentDomain.DomainUnload += CurrentDomainDomainUnload;
                Game.OnFireEvent += Game_OnGameEvent;
                Game.PrintMessage(
                    "<font face='Comic Sans MS, cursive'><font color='#ff0000'>" +
                    "OverlayInformation loaded! </font><font color='#0055FF'> v" +
                    Assembly.GetExecutingAssembly().GetName().Version, MessageType.LogMessage);
            };

            Events.OnClose += (sender, args) =>
            {
                TopPos.Clear();
                InSystem.Clear();
                Game.OnUpdate -= GameOnOnUpdate;
                Drawing.OnDraw -= Drawing_OnDraw;
                Drawing.OnPreReset -= Drawing_OnPreReset;
                Drawing.OnPostReset -= Drawing_OnPostReset;
                Drawing.OnEndScene -= Drawing_OnEndScene;
                AppDomain.CurrentDomain.DomainUnload -= CurrentDomainDomainUnload;
                Game.OnFireEvent -= Game_OnGameEvent;
            };
            
        }

        private static void Game_OnGameEvent(FireEventEventArgs args)
        {
            if (args.GameEvent.Name == "dota_roshan_kill")
            {
                _deathTime = Game.GameTime;
                _roshIsAlive = false;
            }
        }

        private static void CurrentDomainDomainUnload(object sender, EventArgs e)
        {
            if (_roshanFont != null)
                _roshanFont.Dispose();
        }

        private static void Drawing_OnEndScene(EventArgs args)
        {
            if (Drawing.Direct3DDevice9 == null || Drawing.Direct3DDevice9.IsDisposed || !Game.IsInGame)
            {
                return;
            }
            #region ShowRoshanTimer

            if (!Menu.Item("TopPanel.Roshan").GetValue<bool>()) return;
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
                _roshIsAlive ? Color.Green : Color.Red, _roshanFont);

            #endregion
        }

        private static void Drawing_OnPostReset(EventArgs args)
        {
            if (_roshanFont != null)
                _roshanFont.OnLostDevice();
        }

        private static void Drawing_OnPreReset(EventArgs args)
        {
            if (_roshanFont != null)
                _roshanFont.OnLostDevice();
        }

        private static void GameOnOnUpdate(EventArgs args)
        {
            #region Init

            if (!Game.IsInGame) return;

            Update();

            #endregion

            #region Actions

            RuneAction();
            RoshanAction();
            ShowIllusion();
            AutoItemsAction();

            #endregion
        }

        private static void Update()
        {
            if (!MePlayer.IsValid)
                MePlayer = ObjectMgr.LocalPlayer;
            if (!MeHero.IsValid)
                MeHero = ObjectMgr.LocalHero;
        }

        private static void RoshanAction()
        {
            #region Roshan
            if (Menu.Item("TopPanel.Roshan").GetValue<bool>())
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
                }
            }

            #endregion
        }

        private static void ShowIllusion()
        {
            #region ShowIllusions

            if (Menu.Item("ShowIllusions.Enable").GetValue<bool>() && Utils.SleepCheck("ShowIllusions"))
            {
                Utils.Sleep(250, "ShowIllusions");
                var illusions = ObjectMgr.GetEntities<Hero>()
                    .Where(
                        x =>
                            x.IsIllusion && x.Team != MePlayer.Team);
                foreach (var s in illusions)
                    HandleEffect(s);
            }

            #endregion
        }

        private static void AutoItemsAction()
        {
            #region AutoItems

            if (!Menu.Item("AutoItems.Enable").GetValue<bool>() || !Utils.SleepCheck("AutoItems") || !MeHero.IsAlive || MeHero.IsInvisible()) return;
            var inventory = MeHero.Inventory.Items.ToList();
            if (Menu.Item("autoitemsList").GetValue<AbilityToggler>().IsEnabled("item_hand_of_midas"))
            {
                Utils.Sleep(250, "AutoItems");
                var midas = inventory.FirstOrDefault(x => x.Name == "item_hand_of_midas");
                if (midas != null && midas.CanBeCasted())
                {
                    var creep =
                        ObjectMgr.GetEntities<Unit>()
                            .FirstOrDefault(
                                x =>
                                    x.Team != MeHero.Team &&
                                    (x.ClassID == ClassID.CDOTA_BaseNPC_Creep_Lane ||
                                     x.ClassID == ClassID.CDOTA_BaseNPC_Creep_Siege ||
                                     x.ClassID == ClassID.CDOTA_BaseNPC_Creep_Neutral) && x.IsSpawned && x.IsAlive &&
                                    x.Distance2D(MeHero) <= 600);
                    midas.UseAbility(creep);
                }
            }
            if (Menu.Item("autoitemsList").GetValue<AbilityToggler>().IsEnabled("item_phase_boots"))
            {
                var phase = inventory.FirstOrDefault(x => x.Name == "item_phase_boots");
                if (phase != null && phase.CanBeCasted() && !MeHero.IsAttacking() && !MeHero.IsChanneling() && MeHero.NetworkActivity == NetworkActivity.Move)
                {
                    phase.UseAbility();
                }
            }
            if (!Menu.Item("autoitemsList").GetValue<AbilityToggler>().IsEnabled("item_magic_wand")) return;
            var stick = inventory.FirstOrDefault(x => (x.Name == "item_magic_stick" || x.Name == "item_magic_wand") && x.CanBeCasted() && x.CurrentCharges > 0);
            if (MeHero.Health * 100 / MeHero.MaximumHealth >= Menu.Item("autoitemlistHealth").GetValue<Slider>().Value &&
                !(MeHero.Mana * 100 / MeHero.MaximumMana < Menu.Item("autoitemlistMana").GetValue<Slider>().Value)) return;
            if (stick == null || !stick.CanBeCasted() || stick.CurrentCharges <= 0) return;
            stick.UseAbility();
            #endregion
        }

        private static void RuneAction()
        {
            #region Rune

            if (Menu.Item("RuneNotification.Enable").GetValue<bool>())
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
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Game.IsInGame) return;

            DrawForAllHero();
            DrawForEnemyHero();
            DrawForAllyHero();
            DrawForViableHero();

            #region Show me more
            ShowMeMore(Menu.Item("showMeMore.Enable").GetValue<bool>());
            #endregion
        }

        #region DrawForHeroes

        private static void DrawForAllHero()
        {
            foreach (var v in Heroes.All.Where(x => x != null && x.IsValid))
            {
                var pos = GetTopPanelPosition(v) +
                          new Vector2(Menu.Item("BarPosX").GetValue<Slider>().Value,
                              Menu.Item("BarPosY").GetValue<Slider>().Value);
                var size = GetTopPalenSize(v) + new Vector2(0, Menu.Item("BarSizeY").GetValue<Slider>().Value);
                var healthDelta = new Vector2(v.Health*size.X/v.MaximumHealth, 0);
                var manaDelta = new Vector2(v.Mana*size.X/v.MaximumMana, 0);
                DrawHealthPanel(pos, size, healthDelta);
                DrawManaPanel(pos, size, manaDelta);
            }
        }

        private static void DrawForAllyHero()
        {
            foreach (var v in Heroes.GetByTeam(MeHero.Team).Where(x => x != null && x.IsValid))
            {
                var pos = GetTopPanelPosition(v) +
                          new Vector2(Menu.Item("BarPosX").GetValue<Slider>().Value,
                              Menu.Item("BarPosY").GetValue<Slider>().Value);
                var size = GetTopPalenSize(v) + new Vector2(0, Menu.Item("BarSizeY").GetValue<Slider>().Value);
                DrawVisionChecker(v, pos, size);
                DrawShowMeMoreBara(v, pos, size);
            }
        }

        private static void DrawForEnemyHero()
        {
            foreach (
                var v in
                    Heroes.GetByTeam(MeHero.GetEnemyTeam()).Where(x => x != null && x.IsValid))
            {
                DrawEnemyStatus(v, GetTopPalenSize(v) + new Vector2(0, Menu.Item("BarSizeY").GetValue<Slider>().Value),
                    GetTopPanelPosition(v) +
                    new Vector2(Menu.Item("BarPosX").GetValue<Slider>().Value,
                        Menu.Item("BarPosY").GetValue<Slider>().Value), 7);
                DrawUltimates(v);
                DrawShowMeMoreLooping(v);
                DrawLastPosition(v);
            }
        }

        private static void DrawForViableHero()
        {
            foreach (
                var v in Heroes.All.Where(x => x != null && x.IsValid && x.IsVisible && x.IsAlive)
                )
            {
                DrawDangItems(v);
                DrawSpellPanel(v);
            }
        }

        #endregion

        #region DrawingHelpers

        private static void DrawShowMeMore(bool showMeMore)
        {
            if (!showMeMore || AAunit == null || !AAunit.IsValid) return;
            var aapos = Drawing.WorldToScreen(AAunit.Position);
            Drawing.DrawLine(Drawing.WorldToScreen(MeHero.Position), aapos, Color.AliceBlue);
            const string name = "materials/ensage_ui/spellicons/ancient_apparition_ice_blast.vmat";
            Drawing.DrawRect(aapos, new Vector2(50, 50), Drawing.GetTexture(name));
        }

        private static void ShowMeMore(bool showMeMore)
        {
            #region Show me more cast

            DrawShowMeMore(showMeMore);
            if (!showMeMore || !Utils.SleepCheck("showMeMore.Rate")) return;
            Utils.Sleep(250, "showMeMore.Rate");
            var dummy =
                ObjectMgr.GetEntities<Unit>()
                    .Where(x => x.ClassID == ClassID.CDOTA_BaseNPC && x.Team != MeHero.Team)
                    .ToList();
            foreach (var t in dummy)
            {
                for (var n = 0; n <= 3; n++)
                {
                    var mod = t.HasModifier(ShowMeMoreEffects[n].Modifier);
                    if (!mod) continue;

                    ParticleEffect effect;
                    if (!ShowMeMoreEffect.TryGetValue(t, out effect))
                    {
                        effect = t.AddParticleEffect(@"particles\ui_mouseactions\drag_selected_ring.vpcf");
                        effect.SetControlPoint(1,
                            new Vector3(Menu.Item(ShowMeMoreEffects[n].Modifier + ".Red").GetValue<Slider>().Value,
                                Menu.Item(ShowMeMoreEffects[n].Modifier + ".Green").GetValue<Slider>().Value,
                                Menu.Item(ShowMeMoreEffects[n].Modifier + ".Blue").GetValue<Slider>().Value));

                        effect.SetControlPoint(2, new Vector3(ShowMeMoreEffects[n].Range, 255, 0));
                        ShowMeMoreEffect.Add(t, effect);
                    }
                    break;
                }
                if (t.DayVision == 550)
                {
                    if (_findAa) continue;
                    _findAa = true;
                    AAunit = t;

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

            #endregion
        }

        private static Unit AAunit { get; set; }

        private static void DrawSpellPanel(Hero v)
        {
            #region Spells

            if (!Menu.Item("SpellPanel.Enable").GetValue<bool>() || Equals(v, MeHero) ||
                (v.Team != MeHero.GetEnemyTeam() && !Menu.Item("SpellPanel.EnableForAlly").GetValue<bool>())) return;
            Vector2 mypos;
            if (!Drawing.WorldToScreen(v.Position, out mypos)) return;
            if (mypos.X <= -5000 || mypos.X >= 5000) return;
            if (mypos.Y <= -5000 || mypos.Y >= 5000) return;
            var start = HUDInfo.GetHPbarPosition(v) +
                        new Vector2(-Menu.Item("SpellPanel.ExtraPosX").GetValue<Slider>().Value,
                            Menu.Item("SpellPanel.ExtraPosY").GetValue<Slider>().Value);
            //new Vector2(mypos.X-100, mypos.Y);

            var distBetweenSpells = Menu.Item("SpellPanel.distBetweenSpells").GetValue<Slider>().Value;
            var distBwtweenLvls = Menu.Item("SpellPanel.DistBwtweenLvls").GetValue<Slider>().Value;
            var sizeSpell = Menu.Item("SpellPanel.SizeSpell").GetValue<Slider>().Value;
            const int sizey = 9;
            foreach (
                var spell in
                    v.Spellbook.Spells.Where(
                        x => x.AbilityType != AbilityType.Attribute && x.AbilitySlot.ToString() != "-1")
                )
            {
                var size2 = distBetweenSpells;
                var extrarange = spell.Level > 4 ? spell.Level - 4 : 0;

                size2 = (int) (size2 + extrarange*7);
                var cd = spell.Cooldown;

                Drawing.DrawRect(start,
                    new Vector2(size2, spell.AbilityState != AbilityState.OnCooldown ? sizey : 22),
                    new ColorBGRA(0, 0, 0, 100));
                Drawing.DrawRect(start,
                    new Vector2(size2, spell.AbilityState != AbilityState.OnCooldown ? sizey : 22),
                    new ColorBGRA(255, 255, 255, 100), true);
                if (spell.AbilityState == AbilityState.NotEnoughMana)
                {
                    Drawing.DrawRect(start,
                        new Vector2(size2, spell.AbilityState != AbilityState.OnCooldown ? sizey : 22),
                        new ColorBGRA(0, 0, 150, 150));
                }
                if (spell.AbilityState == AbilityState.OnCooldown)
                {
                    var text = string.Format("{0:0.#}", cd);
                    var textSize = Drawing.MeasureText(text, "Arial", new Vector2(10, 200),
                        FontFlags.None);
                    var textPos = start +
                                  new Vector2(10 - textSize.X/2, -textSize.Y/2 + 12);
                    Drawing.DrawText(text, textPos, /*new Vector2(10, 150),*/ Color.White,
                        FontFlags.AntiAlias | FontFlags.DropShadow);
                }

                if (spell.Level > 0)
                {
                    for (var lvl = 1; lvl <= spell.Level; lvl++)
                    {
                        Drawing.DrawRect(start + new Vector2(distBwtweenLvls*lvl, sizey - 6),
                            new Vector2(sizeSpell, sizey - 6),
                            new ColorBGRA(255, 255, 0, 255));
                    }
                }
                start += new Vector2(size2, 0);
            }

            #endregion
        }

        private static void DrawDangItems(Hero v)
        {
            if (!Menu.Item("DangItems.Enable").GetValue<bool>()) return;
            var invetory =
                v.Inventory.Items.Where(
                    x =>
                        Menu.Item("dangItemsUsage").GetValue<AbilityToggler>().IsEnabled(x.Name));
            var iPos = HUDInfo.GetHPbarPosition(v);
            var iSize = new Vector2(HUDInfo.GetHPBarSizeX(v), HUDInfo.GetHpBarSizeY(v));
            float count = 0;
            foreach (var item in invetory)
            {
                var itemname = string.Format("materials/ensage_ui/items/{0}.vmat",
                    item.Name.Replace("item_", ""));
                Drawing.DrawRect(iPos + new Vector2(count, 50),
                    new Vector2(iSize.X/3, (float) (iSize.Y*2.5)),
                    Drawing.GetTexture(itemname));
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

        private static void DrawShowMeMoreBara(Hero v, Vector2 pos, Vector2 size, int height = 7)
        {
            if (!Menu.Item("showMeMore.Enable").GetValue<bool>()) return;
            var mod = v.HasModifier("modifier_spirit_breaker_charge_of_darkness_vision");
            if (mod)
            {
                var textPos = pos + new Vector2(0, size.Y + height*4);
                Drawing.DrawText("Spirit Breaker", textPos, new Vector2(15, 150), Color.White,
                    FontFlags.AntiAlias | FontFlags.DropShadow);
                if (Equals(MeHero, v))
                {
                    const string name = "modifier_spirit_breaker_charge_of_darkness_vision";
                    Drawing.DrawRect(new Vector2(0, 0), new Vector2(Drawing.Width, Drawing.Height),
                        new Color(Menu.Item(name + ".Red").GetValue<Slider>().Value,
                            Menu.Item(name + ".Green").GetValue<Slider>().Value,
                            Menu.Item(name + ".Blue").GetValue<Slider>().Value,
                            Menu.Item(name + ".Alpha").GetValue<Slider>().Value));
                }
                ParticleEffect eff;
                if (BaraIndicator.TryGetValue(v, out eff)) return;
                eff = new ParticleEffect("", v);
                eff.SetControlPointEntity(1, v);
                BaraIndicator.Add(v, eff);
                GenerateSideMessage(v.Name.Replace("npc_dota_hero_", ""),
                    "spirit_breaker_charge_of_darkness");
            }
            else
            {
                ParticleEffect eff;
                if (!BaraIndicator.TryGetValue(v, out eff)) return;
                eff.Dispose();
                BaraIndicator.Remove(v);
            }
        }

        private static void DrawVisionChecker(Hero v, Vector2 pos, Vector2 size, int height = 7)
        {
            if (!Menu.Item("EnemyStatus.Vision.Enable").GetValue<bool>() || !v.IsVisibleToEnemies) return;
            Drawing.DrawRect(pos + new Vector2(0, size.Y + height*2), new Vector2(size.X, height*2),
                new Color(0, 0, 0, 100));
            Drawing.DrawRect(pos + new Vector2(0, size.Y + height*2), new Vector2(size.X, height*2),
                new Color(0, 0, 0, 255), true);
            Drawing.DrawText("under vision", pos + new Vector2(5, size.Y + height*2), Color.White,
                FontFlags.AntiAlias | FontFlags.DropShadow);
        }

        private static void DrawLastPosition(Hero v)
        {
            if (!Menu.Item("EnemyStatus.Map.Enable").GetValue<bool>()) return;
            if (!v.IsAlive || v.IsVisible) return;
            Vector2 newPos;
            if (!Drawing.WorldToScreen(v.Position, out newPos)) return;
            var name = "materials/ensage_ui/heroes_horizontal/" +
                       v.Name.Replace("npc_dota_hero_", "") + ".vmat";
            var size2 = new Vector2(50, 50);
            Drawing.DrawRect(newPos + new Vector2(10, 35), size2 + new Vector2(13, -6),
                Drawing.GetTexture(name));
        }

        private static void DrawShowMeMoreLooping(Hero v)
        {
            if (!Menu.Item("showMeMore.Enable").GetValue<bool>()) return;
            switch (v.ClassID)
            {
                case ClassID.CDOTA_Unit_Hero_Mirana:
                    if (_arrowUnit == null)
                    {
                        _arrowUnit =
                            ObjectMgr.GetEntities<Unit>()
                                .FirstOrDefault(x => x.ClassID == ClassID.CDOTA_BaseNPC && x.DayVision == 650
                                                     && x.Team != MeHero.Team);
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
                                                         && x.Team != MeHero.Team);
                            break;
                        }
                        if (!InSystem.Contains(_arrowUnit))
                        {
                            _arrowS = _arrowUnit.Position;
                            InSystem.Add(_arrowUnit);
                            Utils.Sleep(100, "kek");
                            GenerateSideMessage(v.Name.Replace("npc_dota_hero_", ""), "mirana_arrow");
                        }
                        else if (_letsDraw && Utils.SleepCheck("kek") && _arrowUnit.IsVisible)
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
                    if (!Prediction.IsTurning(v)) //(Utils.SleepCheck("ArrowWindRun"))
                    {
                        var spell = v.Spellbook.Spell2;
                        if (spell != null && spell.Cooldown != 0)
                        {
                            var cd = Math.Floor(spell.Cooldown*100);
                            if (cd < 880)
                            {
                                if (!InSystem.Contains(v))
                                {
                                    if (cd > 720)
                                    {
                                        var eff = new ParticleEffect[148];
                                        for (var z = 1; z <= 140; z++)
                                        {
                                            var p = new Vector3(
                                                v.Position.X + 100*z*(float) Math.Cos(v.RotationRad),
                                                v.Position.Y + 100*z*(float) Math.Sin(v.RotationRad),
                                                100);
                                            eff[z] =
                                                new ParticleEffect(
                                                    @"particles\ui_mouseactions\draw_commentator.vpcf",
                                                    p);
                                            eff[z].SetControlPoint(1, new Vector3(255, 255, 255));
                                            eff[z].SetControlPoint(0, p);
                                        }
                                        Eff.Add(v, eff);
                                        InSystem.Add(v);
                                    }
                                }
                                else if (cd < 720 || !v.IsAlive && InSystem.Contains(v))
                                {
                                    InSystem.Remove(v);
                                    ParticleEffect[] eff;
                                    if (Eff.TryGetValue(v, out eff))
                                    {
                                        foreach (var particleEffect in eff.Where(x => x != null))
                                            particleEffect.ForceDispose();
                                        Eff.Clear();
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

        private static void DrawUltimates(Hero v)
        {
            if (!Menu.Item("Ultimates.Icon.Enable").GetValue<bool>() &&
                !Menu.Item("Ultimates.Text.Enable").GetValue<bool>()) return;
            try
            {
                Ability ultimate;
                if (!UltimateAbilities.TryGetValue(v, out ultimate))
                {
                    var ult = v.Spellbook.Spells.First(x => x.AbilityType == AbilityType.Ultimate);
                    if (ult != null) UltimateAbilities.Add(v, ult);
                }
                else if (ultimate != null && ultimate.Level > 0)
                {
                    var pos = GetTopPanelPosition(v) +
                              new Vector2(Menu.Item("BarPosX").GetValue<Slider>().Value,
                                  Menu.Item("BarPosY").GetValue<Slider>().Value);
                    var size = GetTopPalenSize(v) + new Vector2(0, Menu.Item("BarSizeY").GetValue<Slider>().Value);
                    var ultPos = pos + new Vector2(size.X/2 - 5, size.Y + 1);
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
                    if (Menu.Item("Ultimates.Icon.Enable").GetValue<bool>())
                        Drawing.DrawRect(ultPos, new Vector2(14, 14), Drawing.GetTexture(path));

                    if (Menu.Item("Ultimates.Text.Enable").GetValue<bool>())
                    {

                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private static void DrawEnemyStatus(Hero v, Vector2 size, Vector2 pos, int height)
        {
            if (Menu.Item("EnemyStatus.Timer.Enable").GetValue<bool>())
            {
                var handle = v.Player.ID;
                if (SInfo[handle] == null || !SInfo[handle].GetHero().IsValid)
                {
                    SInfo[handle] = new StatusInfo(v, Game.GameTime);
                }
                Drawing.DrawRect(pos + new Vector2(0, size.Y + height*2), new Vector2(size.X, height*2),
                    new Color(0, 0, 0, 100));
                Drawing.DrawRect(pos + new Vector2(0, size.Y + height*2), new Vector2(size.X, height*2),
                    new Color(0, 0, 0, 255), true);
                var text = SInfo[handle].GetTime();
                Drawing.DrawText(text, pos + new Vector2(5, size.Y + height*2), Color.White,
                    FontFlags.AntiAlias | FontFlags.DropShadow);
            }
        }

        private static void DrawManaPanel(Vector2 pos, Vector2 size, Vector2 manaDelta, int height = 7)
        {
            if (!Menu.Item("TopPanel.Mana").GetValue<bool>()) return;
            Drawing.DrawRect(pos + new Vector2(0, size.Y + height), new Vector2(size.X, height), Color.Gray);
            Drawing.DrawRect(pos + new Vector2(0, size.Y + height), new Vector2(manaDelta.X, height),
                new Color(0, 0, 255, 255));
            Drawing.DrawRect(pos + new Vector2(0, size.Y + height), new Vector2(size.X, height), Color.Black,
                true);
        }

        private static void DrawHealthPanel(Vector2 pos, Vector2 size, Vector2 healthDelta, int height = 7)
        {
            if (!Menu.Item("TopPanel.Hp").GetValue<bool>()) return;
            Drawing.DrawRect(pos + new Vector2(0, size.Y + 1), new Vector2(size.X, height),
                new Color(255, 0, 0, 255));
            Drawing.DrawRect(pos + new Vector2(0, size.Y + 1), new Vector2(healthDelta.X, height),
                new Color(0, 255, 0, 255));
            Drawing.DrawRect(pos + new Vector2(0, size.Y + 1), new Vector2(size.X, height), Color.Black, true);
        }

        #endregion

        #region Helpers

        private static void GenerateSideMessage(string hero, string spellName)
        {
            if (!Menu.Item("SideMessage.Enable").GetValue<bool>()) return;
            var msg = new SideMessage(hero, new Vector2(200, 60));
            msg.AddElement(new Vector2(006, 06), new Vector2(72, 36),
                Drawing.GetTexture("materials/ensage_ui/heroes_horizontal/" + hero + ".vmat"));
            msg.AddElement(new Vector2(078, 12), new Vector2(64, 32),
                Drawing.GetTexture("materials/ensage_ui/other/arrow_usual.vmat"));
            msg.AddElement(new Vector2(142, 06), new Vector2(72, 36),
                Drawing.GetTexture("materials/ensage_ui/spellicons/" + spellName + ".vmat"));
            msg.CreateMessage();
        }

        private static Vector2 GetTopPalenSize(Hero hero)
        {
            return new Vector2((float) HUDInfo.GetTopPanelSizeX(hero), (float) HUDInfo.GetTopPanelSizeY(hero));
        }

        private static readonly Dictionary<uint, Vector2> TopPos = new Dictionary<uint, Vector2>();
        private static Unit _arrowUnit;

        private static Vector2 GetTopPanelPosition(Hero v)
        {
            Vector2 vec2;
            var handle = v.Handle;
            if (TopPos.TryGetValue(handle, out vec2)) return vec2;
            vec2 = HUDInfo.GetTopPanelPosition(v);
            TopPos.Add(handle, vec2);
            return vec2;
        }

        private static void Print(string s, MessageType chatMessage = MessageType.ChatMessage)
        {
            if (true)
                Game.PrintMessage("[OerlayInfo]: " + s, chatMessage);
        }

        private static Vector3 FindVector(Vector3 first, double ret, float distance)
        {
            var retVector = new Vector3(first.X + (float) Math.Cos(Utils.DegreeToRadian(ret))*distance,
                first.Y + (float) Math.Sin(Utils.DegreeToRadian(ret))*distance, 100);

            return retVector;
        }

        private static double FindRet(Vector3 first, Vector3 second)
        {
            var xAngle = Utils.RadianToDegree(Math.Atan(Math.Abs(second.X - first.X)/Math.Abs(second.Y - first.Y)));
            if (first.X <= second.X && first.Y >= second.Y)
            {
                return 270 + xAngle;
            }
            if (first.X >= second.X & first.Y >= second.Y)
            {
                return 90 - xAngle + 180;
            }
            if (first.X >= second.X && first.Y <= second.Y)
            {
                return 90 + xAngle;
            }
            if (first.X <= second.X && first.Y <= second.Y)
            {
                return 90 - xAngle;
            }
            return 0;
        }

        private static void DrawShadowText(string stext, int x, int y, Color color, Font f)
        {

            f.DrawText(null, stext, x + 1, y + 1, Color.Black);
            f.DrawText(null, stext, x, y, color);
        }

        private static void HandleEffect(Unit unit)
        {
            ParticleEffect effect;
            ParticleEffect effect2;
            if (unit.IsAlive /* && unit.IsVisibleToEnemies*/)
            {
                if (Effects1.TryGetValue(unit, out effect)) return;
                effect = unit.AddParticleEffect("particles/items2_fx/smoke_of_deceit_buff.vpcf");
                    //particles/items_fx/diffusal_slow.vpcf
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

        #endregion
    }
}
