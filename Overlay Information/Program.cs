using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using Ensage;
using Ensage.Common;
using Ensage.Common.Menu;
using SharpDX.Direct3D9;
using Color = SharpDX.Color;
using Font = SharpDX.Direct3D9.Font;
using Menu = Ensage.Common.Menu.Menu;
using MenuItem = Ensage.Common.Menu.MenuItem;

namespace OverlayInformation
{
    internal static class Program
    {
        private static void Main()
        {
            Members.Menu.AddItem(new MenuItem("Enable", "Enable").SetValue(true));
            var topPanel = new Menu("Top Panel", "toppanel");
            var spellPanel = new Menu("Spell Panel", "spellPanel");
            var ultimate = new Menu("Ultimate", "ultimate");
            var health = new Menu("Health Panel", "health");
            var mana = new Menu("Mana Panel", "mana");
            var status = new Menu("Status panel", "status");
            var extraPos = new Menu("Extra Position", "extraPos");
            var itemPanel = new Menu("Item panel", "itempanel");

            var roshanTimer = new Menu("Roshan Timer", "roshanTimer");
            var showMeMore = new Menu("Show Me More", "showmemore");
            var showIllusion = new Menu("Show Illusion", "showillusion");
            var runevision = new Menu("Rune Vision", "runevision");

            var autoItems = new Menu("Auto Items", "autoitems");
            var settings = new Menu("Settings", "Settings");
            //===========================
            itemPanel.AddItem(new MenuItem("itempanel.Enable", "Enable").SetValue(true));
            itemPanel.AddItem(new MenuItem("itempanel.X", "Panel Position X").SetValue(new Slider(100, -2000, 2000)));
            itemPanel.AddItem(new MenuItem("itempanel.Y", "Panel Position Y").SetValue(new Slider(200, -2000, 2000)));
            itemPanel.AddItem(new MenuItem("itempanel.SizeX", "SizeX").SetValue(new Slider(255, 1, 255)));
            itemPanel.AddItem(new MenuItem("itempanel.SizeY", "SizeY").SetValue(new Slider(174, 1, 255)));
            itemPanel.AddItem(new MenuItem("itempanel.Red", "Red").SetValue(new Slider(141, 0, 255)).SetFontStyle(FontStyle.Bold, Color.Red));
            itemPanel.AddItem(new MenuItem("itempanel.Green", "Green").SetValue(new Slider(182, 0, 255)).SetFontStyle(FontStyle.Bold, Color.Green));
            itemPanel.AddItem(new MenuItem("itempanel.Blue", "Blue").SetValue(new Slider(98, 0, 255)).SetFontStyle(FontStyle.Bold, Color.Blue));
            //===========================
            topPanel.AddItem(new MenuItem("toppanel.Enable", "Enable").SetValue(true));
            topPanel.AddItem(
                new MenuItem("toppanel.Targets", "Draw For: ").SetValue(
                    new StringList(new[] { "Both", "Ally Team", "Enemy Team" })));
            //===========================
            spellPanel.AddItem(new MenuItem("spellpanel.Enable", "Enable").SetValue(true));
            spellPanel.AddItem(
                new MenuItem("spellpanel.Targets", "Draw For: ").SetValue(
                    new StringList(new[] { "Both", "Ally Team", "Enemy Team" })));
            spellPanel.AddItem(new MenuItem("spellPanel.distBetweenSpells", "Distance spells").SetValue(new Slider(36, 0, 200)));
            spellPanel.AddItem(new MenuItem("spellPanel.DistBwtweenLvls", "Distance lvls").SetValue(new Slider(6, 0, 200)));
            spellPanel.AddItem(new MenuItem("spellPanel.SizeSpell", "Level size").SetValue(new Slider(3, 1, 25)));
            spellPanel.AddItem(new MenuItem("spellPanel.ExtraPosX", "Extra Position X").SetValue(new Slider(25)));
            spellPanel.AddItem(new MenuItem("spellPanel.ExtraPosY", "Extra Position Y").SetValue(new Slider(125, 0, 400)));
            //===========================
            ultimate.AddItem(new MenuItem("ultimate.Enable", "Enable").SetValue(true));
            ultimate.AddItem(new MenuItem("ultimate.Icon.Enable", "Draw Icon").SetValue(true));
            ultimate.AddItem(
                new MenuItem("ultimate.Info", "Show details").SetValue(true)
                    .SetTooltip("show Ultimate's CD if u put ur mouse on icon"));
            ultimate.AddItem(
                new MenuItem("ultimate.InfoAlways", "Show details all time").SetValue(true)
                    .SetTooltip("Show Details should be enabled"));
            //===========================
            health.AddItem(new MenuItem("toppanel.Health.Enable", "Enable").SetValue(true));
            //===========================
            mana.AddItem(new MenuItem("toppanel.Mana.Enable", "Enable").SetValue(true));
            //===========================
            status.AddItem(new MenuItem("toppanel.Status.Enable", "Enable").SetValue(true));
            status.AddItem(new MenuItem("toppanel.AllyVision.Enable", "Vision on Ally Heroes").SetValue(true));
            status.AddItem(new MenuItem("toppanel.EnemiesStatus.Enable", "Enemies status").SetValue(true));
            //===========================
            extraPos.AddItem(
                new MenuItem("extraPos.X", "Extra Position for top panel: X").SetValue(new Slider(0, -25, 25)));
            extraPos.AddItem(
                new MenuItem("extraPos.Y", "Extra Position for top panel: Y").SetValue(new Slider(0, -25, 25)));
            //===========================
            roshanTimer.AddItem(new MenuItem("roshanTimer.Enable", "Enable").SetValue(true));
            //===========================
            showMeMore.AddItem(new MenuItem("showmemore.Enable", "Enable").SetValue(true));
            var charge = new Menu("", "charge", false, "spirit_breaker_charge_of_darkness", true);
            charge.AddItem(new MenuItem("tooltip", "When Charge on your Main Hero").SetFontStyle(FontStyle.Bold, Color.Red));
            charge.AddItem(new MenuItem("charge.Red", "Red").SetValue(new Slider(255, 0, 255)).SetFontStyle(FontStyle.Bold, Color.Red));
            charge.AddItem(new MenuItem("charge.Green", "Green").SetValue(new Slider(0, 0, 255)).SetFontStyle(FontStyle.Bold, Color.Green));
            charge.AddItem(new MenuItem("charge.Blue", "Blue").SetValue(new Slider(0, 0, 255)).SetFontStyle(FontStyle.Bold, Color.Blue));
            charge.AddItem(new MenuItem("charge.Alpha", "Alpha").SetValue(new Slider(4, 0, 255)).SetFontStyle(FontStyle.Bold, Color.WhiteSmoke));
            //===========================
            var blur = new Menu("", "blur", false, "phantom_assassin_blur", true);
            blur.AddItem(new MenuItem("blur.Enable", "Show PA on minimap").SetValue(true));
            //===========================
            var wr = new Menu("", "wr", false, "windrunner_powershot", true);
            wr.AddItem(new MenuItem("wr.Enable", "Enable").SetValue(true));
            wr.AddItem(new MenuItem("wr.Red", "Red").SetValue(new Slider(255, 0, 255)).SetFontStyle(FontStyle.Bold, Color.Red));
            wr.AddItem(new MenuItem("wr.Green", "Green").SetValue(new Slider(255, 0, 255)).SetFontStyle(FontStyle.Bold, Color.Green));
            wr.AddItem(new MenuItem("wr.Blue", "Blue").SetValue(new Slider(255, 0, 255)).SetFontStyle(FontStyle.Bold, Color.Blue));
            //===========================
            var mirana = new Menu("", "mirana", false, "mirana_arrow", true);
            mirana.AddItem(new MenuItem("mirana.Enable", "Enable").SetValue(true));
            mirana.AddItem(new MenuItem("mirana.Red", "Red").SetValue(new Slider(255, 0, 255)).SetFontStyle(FontStyle.Bold, Color.Red));
            mirana.AddItem(new MenuItem("mirana.Green", "Green").SetValue(new Slider(255, 0, 255)).SetFontStyle(FontStyle.Bold, Color.Green));
            mirana.AddItem(new MenuItem("mirana.Blue", "Blue").SetValue(new Slider(255, 0, 255)).SetFontStyle(FontStyle.Bold, Color.Blue));
            var apparition = new Menu("", "apparition", false, "ancient_apparition_ice_blast", true);
            apparition.AddItem(new MenuItem("apparition.Enable", "Enable").SetValue(true));
            //===========================
            showIllusion.AddItem(new MenuItem("showillusion.Enable", "Enable").SetValue(true));
            //===========================
            runevision.AddItem(new MenuItem("runevision.Enable", "Enable").SetValue(true));
            runevision.AddItem(new MenuItem("runevision.PrintText.Enable", "Print text on rune-spawning").SetValue(true));
            runevision.AddItem(new MenuItem("runevision.DrawOnMinimap.Enable", "Draw rune on minimap").SetValue(true));
            //===========================
            var dangItem = new Menu("Dangerous items", "dangitems");
            dangItem.AddItem(new MenuItem("dangitems.Enable", "Enable").SetValue(false)).SetTooltip("show if enemy has Dangerous items");
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
            dangItem.AddItem(new MenuItem("dangitems.List", "Items: ").SetValue(new AbilityToggler(dict)));
            //===========================
            autoItems.AddItem(new MenuItem("autoitems.Enable", "Enable").SetValue(true));
            var autoitemlist = new Dictionary<string, bool>
            {
                {"item_phase_boots", true},
                {"item_hand_of_midas", true}
            };
            autoItems.AddItem(new MenuItem("autoitems.List", "Items:").SetValue(new AbilityToggler(autoitemlist)));
            //===========================
            var devolper = new Menu("Developer", "Developer");
            devolper.AddItem(new MenuItem("Dev.Hax.enable", "Hax in lobby").SetValue(false));
            devolper.AddItem(new MenuItem("Dev.Text.enable", "Debug messages").SetValue(false));
            //===========================

            topPanel.AddSubMenu(ultimate);
            topPanel.AddSubMenu(health);
            topPanel.AddSubMenu(mana);
            topPanel.AddSubMenu(status);
            topPanel.AddSubMenu(extraPos);
            settings.AddSubMenu(topPanel);
            settings.AddSubMenu(spellPanel);
            settings.AddSubMenu(roshanTimer);
            settings.AddSubMenu(showMeMore);
            showMeMore.AddSubMenu(charge);
            showMeMore.AddSubMenu(blur);
            showMeMore.AddSubMenu(wr);
            showMeMore.AddSubMenu(mirana);
            showMeMore.AddSubMenu(apparition);
            settings.AddSubMenu(showIllusion);
            settings.AddSubMenu(runevision);
            settings.AddSubMenu(dangItem);
            settings.AddSubMenu(itemPanel);
            settings.AddSubMenu(autoItems);

            Members.Menu.AddSubMenu(settings);
            Members.Menu.AddSubMenu(devolper);

            Members.Menu.AddToMainMenu();
            if (Drawing.Direct3DDevice9 != null)
                Members.RoshanFont = new Font(
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
                Members.MyHero = ObjectManager.LocalHero;
                Members.MyPlayer = ObjectManager.LocalPlayer;
                
                Members.AbilityDictionary = new Dictionary<string, List<Ability>>();
                Members.ItemDictionary = new Dictionary<string, List<Item>>();

                Members.PAisHere = null;
                Members.BaraIsHere = false;
                Members.Apparition = false;
                Members.Mirana = null;
                Members.Windrunner = null;

                Game.OnUpdate += Updater.HeroList.Update;
                //Game.OnUpdate += Updater.PlayerList.Update;
                Game.OnUpdate += Updater.BaseList.Update;
                Game.OnUpdate += Devolp.ConsoleCommands;
                Game.OnUpdate += RoshanAction.Roshan;
                Game.OnUpdate += Game_OnUpdate;

                Drawing.OnDraw += DrawHelper.Overlay;
                Drawing.OnDraw += ItemPanel.Draw;
                Drawing.OnDraw += ShowMeMore.Draw;
                Drawing.OnDraw += Runes.Draw;

                Drawing.OnPreReset += DrawHelper.Render.Drawing_OnPreReset;
                Drawing.OnPostReset += DrawHelper.Render.Drawing_OnPostReset;
                Drawing.OnEndScene += DrawHelper.Render.Drawing_OnEndScene;

                AppDomain.CurrentDomain.DomainUnload += DrawHelper.Render.CurrentDomainDomainUnload;
                Game.OnFireEvent += FireEvent.Game_OnGameEvent;

                

                Game.PrintMessage(
                    "<font face='Comic Sans MS, cursive'><font color='#00aaff'>" + Members.Menu.DisplayName +
                    " By Jumpering" +
                    " loaded!</font> <font color='#aa0000'>v" + Assembly.GetExecutingAssembly().GetName().Version,
                    MessageType.LogMessage);
                Printer.PrintSuccess("> " + Members.Menu.DisplayName + " loaded v" +
                                     Assembly.GetExecutingAssembly().GetName().Version);
            };
            Events.OnClose += (sender, args) =>
            {
                Game.OnUpdate -= Updater.HeroList.Update;
                //Game.OnUpdate += Updater.PlayerList.Update;
                Game.OnUpdate -= Updater.BaseList.Update;
                Game.OnUpdate -= Devolp.ConsoleCommands;
                Game.OnUpdate -= RoshanAction.Roshan;
                Game.OnUpdate -= Game_OnUpdate;

                Drawing.OnDraw -= DrawHelper.Overlay;
                Drawing.OnDraw -= ItemPanel.Draw;
                Drawing.OnDraw -= ShowMeMore.Draw;
                Drawing.OnDraw -= Runes.Draw;
                Drawing.OnPreReset -= DrawHelper.Render.Drawing_OnPreReset;
                Drawing.OnPostReset -= DrawHelper.Render.Drawing_OnPostReset;
                Drawing.OnEndScene -= DrawHelper.Render.Drawing_OnEndScene;
                AppDomain.CurrentDomain.DomainUnload -= DrawHelper.Render.CurrentDomainDomainUnload;
                Game.OnFireEvent -= FireEvent.Game_OnGameEvent;
                Members.TopPanelPostiion.Clear();
                Members.Heroes.Clear();
                Members.EnemyHeroes.Clear();
                Members.AllyHeroes.Clear();
                Printer.PrintInfo("> " + Members.Menu.DisplayName + " unloaded");
            };
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (!Checker.IsActive()) return;
            ShowMeMore.ShowIllustion();
            ShowMeMore.ShowMeMoreSpells();
            AutoItems.Action();
            Runes.Action();
        }
    }
}