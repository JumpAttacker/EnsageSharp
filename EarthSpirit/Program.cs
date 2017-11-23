using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using Ensage.Common.Objects.UtilityObjects;
using SharpDX;

namespace EarthAn
{
    internal class Draw
    {
        private static bool IsEnable => Members.Menu.Item("Enable").GetValue<bool>();
        private static bool IsDrawingEnable => Members.Menu.Item("Drawing.Enable").GetValue<bool>();
        private static float Size => (float) Members.Menu.Item("Drawing.Size").GetValue<Slider>().Value;
        private static float ExtraPosX => (float) Members.Menu.Item("Drawing.ExtraPosX").GetValue<Slider>().Value;
        private static float ExtraPosY => (float) Members.Menu.Item("Drawing.ExtraPosY").GetValue<Slider>().Value;
        public static void Drawing(EventArgs args)
        {
            if (!IsEnable)
                return;
            if (!IsDrawingEnable)
                return;
            var heroes =
                Heroes.GetByTeam(Members.MyHero.GetEnemyTeam())
                    .Where(x => x.IsVisible && x.IsAlive && x.HasModifier("modifier_earth_spirit_magnetize"))
                    .ToList();
            foreach (var hero in heroes)
            {
                var pos = HUDInfo.GetHPbarPosition(hero);
                if (pos.IsZero)
                    continue; 
                pos+=new Vector2(ExtraPosX,ExtraPosY);
                var size = Size;
                var mod = hero.FindModifier("modifier_earth_spirit_magnetize");
                if (mod==null || !mod.IsValid)
                    continue;
                var ultimateCd =
                    ((int)Math.Min(mod.RemainingTime+1, 99)).ToString(CultureInfo.InvariantCulture);
                var textSize = Ensage.Drawing.MeasureText(ultimateCd, "Arial",
                    new Vector2((float)(size * .75), size / 2), FontFlags.AntiAlias);
                var textPos = pos;
                Ensage.Drawing.DrawRect(textPos - new Vector2(0, 0),
                    new Vector2(textSize.X, textSize.Y),
                    new Color(0, 0, 0, 200));
                Ensage.Drawing.DrawText(
                    ultimateCd,
                    textPos,
                    new Vector2(textSize.Y, 0),
                    Color.White,
                    FontFlags.AntiAlias | FontFlags.StrikeOut);
            }
        }
    }
    internal class Program
    {
        private static bool _loaded;
        private static Sleeper _updater;
        private static float RealCastRange = 1200;

        private static void Main()
        {
            Members.Menu.AddItem(new MenuItem("Enable", "Enable").SetValue(true));
            var settings = new Menu("Settings", "Settings");
            var singleCombos = new Menu("SingleCombos", "SingleCombos");
            singleCombos.AddItem(
                new MenuItem("HotKeyCombo.1.Enable", "Smash").SetValue(new KeyBind('Z', KeyBindType.Press)));
            singleCombos.AddItem(
                new MenuItem("HotKeyCombo.2.Enable", "Rolling").SetValue(new KeyBind('X', KeyBindType.Press)));
            singleCombos.AddItem(
                new MenuItem("HotKeyCombo.3.Enable", "Grip").SetValue(new KeyBind('C', KeyBindType.Press)));
            singleCombos.AddItem(
                new MenuItem("HotKeyCombo.4.Enable", "Grip+Rolling").SetValue(new KeyBind('V', KeyBindType.Press)));

            //settings.AddItem(new MenuItem("KillSteal.Enable", "Enable KillSteal with Q").SetValue(true));
            settings.AddItem(new MenuItem("Range.Enable", "Draw range for Grip").SetValue(true)).ValueChanged +=
                (sender, args) =>
                {
                    if (args.GetNewValue<bool>())
                    {
                        Members.SpellRange =
                            Members.MyHero.AddParticleEffect(@"particles\ui_mouseactions\drag_selected_ring.vpcf");
                        var extraRange = Members.MyHero.Inventory.Items.Any(x => x.StoredName() == "item_aether_lens")
                            ? 200
                            : 0;
                        var spellE = RealCastRange + extraRange + Members.MyHero.HullRadius;
                        Members.SpellRange.SetControlPoint(1, new Vector3(99, 255, 255));
                        Members.SpellRange.SetControlPoint(2, new Vector3(spellE, 255, 0));
                    }
                    else
                    {
                        if (Members.SpellRange != null && Members.SpellRange.IsValid && !Members.SpellRange.IsDestroyed)
                            Members.SpellRange.Dispose();
                    }
                };
            settings.AddItem(
                new MenuItem("Combo.Enable", "Do Combo Fore Selected Enemy").SetValue(new KeyBind('D', KeyBindType.Press)));
            var aAttack = new Menu("Auto Attack", "Auto Attack");
            aAttack.AddItem(new MenuItem("AutoAttack.Enable", "Enable AutoAttacking").SetValue(true));
            aAttack.AddItem(new MenuItem("OrbWalker.Enable", "Orbwalking").SetValue(true));
            var items = new Menu("Using", "Items");
            var drawing = new Menu("drawing", "drawing");
            drawing.AddItem(new MenuItem("Drawing.Enable", "Enable Drawing").SetValue(true));
            drawing.AddItem(new MenuItem("Drawing.Size", "Text Size").SetValue(new Slider(65, 1)));
            drawing.AddItem(new MenuItem("Drawing.ExtraPosX", "Position X").SetValue(new Slider(36, -200,200)));
            drawing.AddItem(new MenuItem("Drawing.ExtraPosY", "Position Y").SetValue(new Slider(-80, -200,200)));
            items.AddItem(new MenuItem("Prediction.Enable", "Prediction for abilities").SetValue(true));
            items.AddItem(
                new MenuItem("abilityEnable", "Abilities in combo:").SetValue(
                    new AbilityToggler(Members.AbilityList.ToDictionary(item => item, item => true))));
            items.AddItem(
                new MenuItem("itemEnable", "Items in combo:").SetValue(
                    new PriorityChanger(new List<string> { "item_blink" }, useAbilityToggler: true)));
            var daggerSelection = new Menu("Dagger", "dagger", textureName: "item_blink");
            daggerSelection.AddItem(
                new MenuItem("Dagger.CloseRange", "Extra Distance for blink").SetValue(
                    new Slider(200, 100, 800))).SetTooltip("1200 (dagger's default range) + your value");
            daggerSelection.AddItem(
                new MenuItem("Dagger.MinDistance", "Min distance for blink").SetValue(new Slider(400, 100, 800))).SetTooltip("dont use blink if you are in this range");
            daggerSelection.AddItem(
                new MenuItem("Dagger.ExtraDistance", "Min distance between target & blink pos").SetValue(new Slider(50, 50, 800)));
            var devolper = new Menu("Developer", "Developer");
            devolper.AddItem(new MenuItem("Dev.Text.enable", "Debug messages").SetValue(false));
            Members.Menu.AddSubMenu(settings);
            settings.AddSubMenu(singleCombos);
            settings.AddSubMenu(items);
            settings.AddSubMenu(aAttack);
            settings.AddSubMenu(drawing);
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
                ObjectManager.LocalHero.HeroId == Members.MyHeroId && Game.IsInGame)
            {
                Load();
                _loaded = true;
            }

            Events.OnClose += (sender, args) =>
            {
                if (!_loaded)
                    return;
                Members.Menu.RemoveFromMainMenu();
                Game.OnUpdate -= Action.Game_OnUpdate;
                Game.OnUpdate -= UpdateItems;
                Game.OnUpdate -= HotkeyCombos.Game_OnUpdate;
                Drawing.OnDraw -= Draw.Drawing;
                _loaded = false;
            };
        }

        private static void Load()
        {
            if (ObjectManager.LocalHero.HeroId != Members.MyHeroId)
                return;
            if (Members.MyHero == null || !Members.MyHero.IsValid)
            {
                Members.MyHero = ObjectManager.LocalHero;
                Members.MyTeam = ObjectManager.LocalHero.Team;
            }
            _updater = new Sleeper();
            Orbwalking.Load();
            Members.Items = new List<string>();
            Members.Menu.AddToMainMenu();
            Game.OnUpdate += Action.Game_OnUpdate;
            Game.OnUpdate += UpdateItems;
            Game.OnUpdate += HotkeyCombos.Game_OnUpdate;
            Drawing.OnDraw += Draw.Drawing;
            if (Members.Menu.Item("Range.Enable").GetValue<bool>())
            {
                Members.SpellRange =
                    Members.MyHero.AddParticleEffect(@"particles\ui_mouseactions\drag_selected_ring.vpcf");
                var extraRange = Members.MyHero.Inventory.Items.Any(x => x.StoredName() == "item_aether_lens")
                    ? 200
                    : 0;
                var spellE = RealCastRange + extraRange + Members.MyHero.HullRadius;
                Members.SpellRange.SetControlPoint(1, new Vector3(99, 255, 255));
                Members.SpellRange.SetControlPoint(2, new Vector3(spellE, 255, 0));
            }
        }

        private static bool _lastShit;
        private static bool IsEnable => Members.Menu.Item("Enable").GetValue<bool>();
        private static void UpdateItems(EventArgs args)
        {
            if (!IsEnable)
                return;
            if (_updater.Sleeping)
                return;
            _updater.Sleep(500);
            var inventory = Members.MyHero.Inventory.Items;
            var enumerable = inventory as IList<Item> ?? inventory.ToList();
            var gotIt = enumerable.Any(x => x.StoredName() == "item_aether_lens");
            if (_lastShit != gotIt)
            {
                var oldValue = Members.Menu.Item("Range.Enable").GetValue<bool>();
                _lastShit = gotIt;
                Members.Menu.Item("Range.Enable").SetValue(!oldValue);
                Members.Menu.Item("Range.Enable").SetValue(oldValue);
            }
            var needToUpdate = false;
            foreach (var item in enumerable.Where(item => !Members.Items.Contains(item.StoredName()) &&
                                                         (item.IsDisable() || item.IsNuke() || item.IsPurge() ||
                                                          item.IsHeal() || item.IsShield() || item.IsSilence() ||
                                                          item.IsSlow() || item.IsSkillShot() || item.StoredName()== "item_heavens_halberd" || item.Id == AbilityId.item_nullifier)))
            {
                Members.Items.Add(item.StoredName());
                needToUpdate = true;
                Printer.Print($"[NewItem]: {item.StoredName()}");
            }
            if (needToUpdate)
            {
                Members.Menu.Item("itemEnable")
                    .SetValue(new PriorityChanger(Members.Items, useAbilityToggler: true));
            }
        }
    }
}
