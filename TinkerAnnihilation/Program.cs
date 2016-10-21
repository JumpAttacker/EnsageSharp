using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using Ensage.Common.Objects.UtilityObjects;
using SharpDX;

namespace TinkerAnnihilation
{
    internal class Program
    {
        private static bool _loaded;
        private static Sleeper _updater;

        private static void Main()
        {
            Members.Menu.AddItem(new MenuItem("Enable", "Enable").SetValue(true));
            var settings = new Menu("Settings", "Settings");
            
            settings.AddItem(new MenuItem("KillSteal.Enable", "Enable KillSteal").SetValue(true));
            settings.AddItem(
                new MenuItem("Combo.Enable", "Do Combo Fore Selected Enemy").SetValue(new KeyBind('D', KeyBindType.Press)));
            var drawing = new Menu("Drawing", "Drawing");
            drawing.AddItem(
                new MenuItem("Drawing.EnableDamage", "Draw damage: ").SetValue(
                    new StringList(new[] {"only for combo Target", "for all enemy heroes"})));
            var aAttack = new Menu("Auto Attack", "Auto Attack");
            var targetEfffect = new Menu("Target effect", "Target effect");
            targetEfffect.AddItem(new MenuItem("Target.Red", "Red").SetValue(new Slider(141, 0, 255)).SetFontColor(Color.Red));
            targetEfffect.AddItem(new MenuItem("Target.Green", "Green").SetValue(new Slider(182, 0, 255)).SetFontColor(Color.Green));
            targetEfffect.AddItem(new MenuItem("Target.Blue", "Blue").SetValue(new Slider(98, 0, 255)).SetFontColor(Color.Blue));
            aAttack.AddItem(new MenuItem("AutoAttack.Enable", "Enable AutoAttacking").SetValue(true));
            aAttack.AddItem(new MenuItem("OrbWalker.Enable", "Orbwalking").SetValue(true));
            var items = new Menu("Using", "Items");
            /*items.AddItem(
                new MenuItem("itemEnable", "Items in combo:").SetValue(
                    new AbilityToggler(new List<string> {"item_blink"}.ToDictionary(item => item, item => true))));*/
            items.AddItem(
                new MenuItem("abilityEnable", "Abilities in combo:").SetValue(
                    new AbilityToggler(Members.AbilityList.ToDictionary(item => item, item => true))));
            items.AddItem(
                new MenuItem("itemEnable", "Items in combo:").SetValue(
                    new PriorityChanger(new List<string>() /*{ "item_blink" }*/, useAbilityToggler: true)));
            var daggerSelection = new Menu("Dagger", "dagger",textureName:"item_blink");
            daggerSelection.AddItem(
                new MenuItem("Dagger.CloseRange", "Extra Distance for blink").SetValue(
                    new Slider(2500, 100, 3000))).SetTooltip("1200 (dagger's default range) + your value");
            daggerSelection.AddItem(
                new MenuItem("Dagger.MinDistance", "Min distance for blink").SetValue(new Slider(400, 100, 800))).SetTooltip("dont use blink if you are in this range");
            daggerSelection.AddItem(
                new MenuItem("Dagger.ExtraDistance", "Min distance between target & blink pos").SetValue(new Slider(50, 50, 800)));
            var devolper = new Menu("Developer", "Developer");
            devolper.AddItem(new MenuItem("Dev.Text.enable", "Debug messages ingame").SetValue(false));
            devolper.AddItem(new MenuItem("Dev.Console.enable", "Debug messages in Console").SetValue(false));
            Members.Menu.AddSubMenu(settings);
            settings.AddSubMenu(aAttack);
            settings.AddSubMenu(drawing);
            drawing.AddSubMenu(targetEfffect);
            settings.AddSubMenu(items);
            items.AddSubMenu(daggerSelection);
            Members.Menu.AddSubMenu(devolper);
            _loaded = false;
            Events.OnLoad += (sender, args) =>
            {
                if (_loaded)
                    return;
                Load();
                _loaded = true;
            };
            if (!_loaded && ObjectManager.LocalHero != null &&
                ObjectManager.LocalHero.ClassID == ClassID.CDOTA_Unit_Hero_Tinker && Game.IsInGame)
            {
                Load();
                _loaded = true;
            }

            Events.OnClose += (sender, args) =>
            {
                if (!_loaded)
                    return;
                Members.Menu.RemoveFromMainMenu();
                Game.OnUpdate -= ComboAction.Game_OnUpdate;
                Game.OnUpdate-= UpdateItems;
                Drawing.OnDraw -= ComboAction.Drawing_OnDraw;
                Unit.OnModifierAdded -= TeleportRangeHelper.Unit_OnModifierAdded;
                Unit.OnModifierRemoved -= TeleportRangeHelper.UnitOnOnModifierRemoved;
                _loaded = false;
            };
        }

        private static void Load()
        {
            if (ObjectManager.LocalHero.ClassID != ClassID.CDOTA_Unit_Hero_Tinker)
                return;
            if (Members.MyHero == null || !Members.MyHero.IsValid)
            {
                Members.MyHero = ObjectManager.LocalHero;
                Members.MyTeam = ObjectManager.LocalHero.Team;
            }
            _updater = new Sleeper();
            Members.TowerRangEffectHelper = new ParticleEffectHelper("name 1");
            Members.TowerRangEffectHelper2 = new ParticleEffectHelper("name 2");
            Orbwalking.Load();
            Members.Items = new List<string>();
            Members.Menu.AddToMainMenu();
            Game.OnUpdate += ComboAction.Game_OnUpdate;
            Game.OnUpdate += UpdateItems;
            Drawing.OnDraw += ComboAction.Drawing_OnDraw;
            Unit.OnModifierAdded += TeleportRangeHelper.Unit_OnModifierAdded;
            Unit.OnModifierRemoved += TeleportRangeHelper.UnitOnOnModifierRemoved;
        }

        private static void UpdateItems(EventArgs args)
        {
            if (_updater.Sleeping)
                return;
            _updater.Sleep(500);
            var inventory = Members.MyHero.Inventory.Items;
            foreach (var item in inventory.Where(item => !Members.Items.Contains(item.StoredName()) &&
                                                         (item.IsDisable() || item.IsNuke() || item.IsPurge() ||
                                                          item.IsHeal() || item.IsShield() || item.IsSilence() ||
                                                          item.IsSlow() || item.IsSkillShot() ||
                                                          Members.WhiteList.Contains(item.StoredName()))))
            {
                var itsDagon = item.StoredName().Contains("dagon");
                if (itsDagon)
                {
                    var oldDagon = Members.Items.FirstOrDefault(x => x.Contains("dagon"));
                    if (oldDagon != null)
                    {
                        Members.Items.Remove(oldDagon);
                        Members.Menu.Item("itemEnable")
                            .GetValue<PriorityChanger>().Remove(oldDagon);
                    }
                }
                Members.Items.Add(item.StoredName());
                Members.Menu.Item("itemEnable")
                    .GetValue<PriorityChanger>().Add(item.StoredName());
                Printer.Print($"[NewItem]: {item.StoredName()}");

            }
        }
    }
}
