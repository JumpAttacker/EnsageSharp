using System;
using System.Collections.Generic;
using System.Globalization;
using Ensage;
using Ensage.Common;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using SharpDX;

namespace OverlayInformation
{
    public class AbilityOverlay
    {
        private static bool _loaded;
        private static bool IsEnable => Members.Menu.Item("spellpanel.NewMethod.Enable").GetValue<bool>();
        private static int SelectedIndex => Members.Menu.Item("spellpanel.Targets").GetValue<StringList>().SelectedIndex;
        private static float TextSizeLevel => (float) Members.Menu.Item("spellpanel.NewMethod.SizeLevel").GetValue<Slider>().Value/100;
        private static float TextSize => (float) Members.Menu.Item("spellpanel.NewMethod.Size").GetValue<Slider>().Value/100;
        private static float IconSize => (float) Members.Menu.Item("spellpanel.NewMethod.IconSize").GetValue<Slider>().Value;
        private static float ExtraX => (float) Members.Menu.Item("spellpanel.NewMethod.ExtraX").GetValue<Slider>().Value;
        private static float ExtraY => (float) Members.Menu.Item("spellpanel.NewMethod.ExtraY").GetValue<Slider>().Value;
        public AbilityOverlay()
        {
            _loaded = false;
            Events.OnLoad += (sender, args) =>
            {
                if (_loaded)
                {
                    return;
                }
                Load();
                _loaded = true;
            };
            if (!_loaded && ObjectManager.LocalHero != null && Game.IsInGame)
            {
                Load();
                _loaded = true;
            }

            Events.OnClose += (sender, args) =>
            {
                Drawing.OnDraw -= Drawing_OnDraw;
                _loaded = false;
            };
        }

        private static void Load()
        {
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Checker.IsActive())
                return;
            if (!IsEnable)
                return;
            List<Hero> selectedHeroes = null;
            switch (SelectedIndex)
            {
                case 0:
                    selectedHeroes = Manager.HeroManager.GetViableHeroes();
                    break;
                case 1:
                    selectedHeroes = Manager.HeroManager.GetAllyViableHeroes();
                    break;
                case 2:
                    selectedHeroes = Manager.HeroManager.GetEnemyViableHeroes();
                    break;
            }
            if (selectedHeroes == null) return;
            foreach (var v in selectedHeroes)
            {
                try
                {
                    var pos = HUDInfo.GetHPbarPosition(v);
                    if (pos.IsZero)
                        continue;
                    var spells = Manager.HeroManager.GetAbilityList(v);
                    pos += new Vector2(0,HUDInfo.GetHPBarSizeX());
                    pos += new Vector2(ExtraX, ExtraY);
                    var counter = 0;
                    var size = new Vector2(IconSize, IconSize);
                    foreach (var ability in spells)
                    {
                        var itemPos = pos + new Vector2(-2 + size.X * counter, 2);
                        Drawing.DrawRect(itemPos, size,
                            Textures.GetSpellTexture(ability.StoredName()));
                        Drawing.DrawRect(itemPos, size,
                            Color.Black, true);
                        var abilityState = ability.AbilityState;
                        if (abilityState == AbilityState.NotEnoughMana)
                        {
                            Drawing.DrawRect(itemPos, size,
                                new Color(0, 0, 155, 155));
                            var neededMana = ((int)Math.Min(Math.Abs(v.Mana - ability.ManaCost), 99)).ToString(
                                CultureInfo.InvariantCulture);
                            var textSize = Drawing.MeasureText(neededMana, "Arial",
                                new Vector2(
                                    (float)(size.Y * TextSize),
                                    size.Y / 2), FontFlags.AntiAlias);
                            var textPos = itemPos + new Vector2(/*size.X-textSize.X*/1, 0);
                            Drawing.DrawRect(textPos - new Vector2(0, 0),
                                new Vector2(textSize.X, textSize.Y),
                                new Color(0, 0, 0, 200));
                            Drawing.DrawText(
                                neededMana,
                                textPos,
                                new Vector2(textSize.Y, 0),
                                Color.White,
                                FontFlags.AntiAlias | FontFlags.StrikeOut);
                        }
                        if (abilityState != AbilityState.NotLearned)
                        {
                            var level = ability.Level;
                            var levelString = level.ToString();
                            var textSize = Drawing.MeasureText(levelString, "Arial",
                                new Vector2(
                                    (float) (size.Y* TextSizeLevel),
                                    size.Y/2), FontFlags.AntiAlias);
                            var textPos = itemPos + new Vector2(1, size.Y - textSize.Y);
                            Drawing.DrawRect(textPos - new Vector2(0, 0),
                                new Vector2(textSize.X, textSize.Y),
                                new Color(0, 0, 0, 240));
                            Drawing.DrawText(
                                levelString,
                                textPos,
                                new Vector2(textSize.Y, 0),
                                Color.White,
                                FontFlags.AntiAlias | FontFlags.StrikeOut);
                            if (ability.IsInAbilityPhase)
                            {
                                Drawing.DrawRect(itemPos,
                                    size,
                                    new Color(255, 255, 50, 50));
                            }
                        }
                        else
                        {
                            Drawing.DrawRect(itemPos, size,
                                new Color(0,0,0,150));
                        }
                        if (abilityState == AbilityState.OnCooldown)
                        {
                            var remTime = ability.Cooldown;
                            var cooldown = Math.Min(remTime + 0.1, 99).ToString("0.0");
                            var textSize = Drawing.MeasureText(cooldown, "Arial",
                                new Vector2(
                                    (float) (size.Y*TextSize),
                                    size.Y/2), FontFlags.AntiAlias);
                            var textPos = itemPos + new Vector2(0, 0);
                            Drawing.DrawRect(textPos - new Vector2(1, 0),
                                new Vector2(textSize.X, textSize.Y),
                                new Color(0, 0, 0, 200));
                            Drawing.DrawText(
                                cooldown,
                                textPos,
                                new Vector2(textSize.Y, 0),
                                Color.White,
                                FontFlags.AntiAlias | FontFlags.StrikeOut);
                        }
                        

                        counter++;
                    }
                }
                catch (Exception e)
                {
                    Printer.Print($"[AbilityOverlay]: {v.StoredName()} : {e.HelpLink}");
                }
            }
        }
    }
}
