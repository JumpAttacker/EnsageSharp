using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using SharpDX;
using SharpDX.Direct3D9;

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace OverlayInformation
{
    internal abstract class DrawHelper
    {
        public static void Overlay(EventArgs args)
        {
            if (!Checker.IsActive()) return;
            if (Members.Menu.Item("spellpanel.Enable").GetValue<bool>())
                DrawSpellPanel(Members.Menu.Item("spellpanel.Targets").GetValue<StringList>().SelectedIndex);
            if (Members.Menu.Item("toppanel.Enable").GetValue<bool>())
                DrawTopPanel(Members.Menu.Item("toppanel.Targets").GetValue<StringList>().SelectedIndex);
            if (Members.Menu.Item("dangitems.Enable").GetValue<bool>())
                DrawDangeItems();
        }

        private static void DrawDangeItems()
        {
            foreach (var hero in Manager.HeroManager.GetEnemyViableHeroes())
            {
                var iPos = HUDInfo.GetHPbarPosition(hero);
                var iSize = new Vector2(HUDInfo.GetHPBarSizeX(hero), HUDInfo.GetHpBarSizeY(hero));
                float count = 0;
                foreach (
                    var item in
                        Manager.HeroManager.GetItemList(hero)
                            .Where(x => Members.Menu.Item("dangitems.List").GetValue<AbilityToggler>().IsEnabled(x.Name))
                    )
                {
                    var itemname = string.Format("materials/ensage_ui/items/{0}.vmat",
                    item.Name.Replace("item_", ""));
                    Drawing.DrawRect(iPos + new Vector2(count, 50),
                        new Vector2(iSize.X / 3, (float)(iSize.Y * 2.5)),
                        Textures.GetTexture(itemname));
                    if (item.AbilityState == AbilityState.OnCooldown)
                    {
                        var cd = ((int)item.Cooldown).ToString(CultureInfo.InvariantCulture);
                        Drawing.DrawText(cd, iPos + new Vector2(count, 40), Color.White,
                            FontFlags.AntiAlias | FontFlags.DropShadow);
                    }
                    if (item.AbilityState == AbilityState.NotEnoughMana)
                    {
                        Drawing.DrawRect(iPos + new Vector2(count, 50),
                            new Vector2(iSize.X / 4, (float)(iSize.Y * 2.5)), new Color(0, 0, 200, 100));
                    }
                    count += iSize.X / 4;
                }
            }
        }

        private static readonly Dictionary<string,Ability> Ultimate=new Dictionary<string, Ability>();

        private static void DrawTopPanel(int type)
        {
            List<Hero> selectedHeroes = null;
            switch (type)
            {
                case 0:
                    selectedHeroes = Manager.HeroManager.GetHeroes();
                    break;
                case 1:
                    selectedHeroes = Members.AllyHeroes;
                    break;
                case 2:
                    selectedHeroes = Members.EnemyHeroes;
                    break;
            }
            if (selectedHeroes == null) return;
            if (Members.Menu.Item("toppanel.Status.Enable").GetValue<bool>() ||
                Members.Menu.Item("toppanel.Health.Enable").GetValue<bool>() ||
                Members.Menu.Item("toppanel.Mana.Enable").GetValue<bool>())
            {
                foreach (var v in selectedHeroes)
                {
                    var pos = Helper.GetTopPanelPosition(v) +
                              new Vector2(Members.Menu.Item("extraPos.X").GetValue<Slider>().Value,
                                  Members.Menu.Item("extraPos.Y").GetValue<Slider>().Value);
                    var temp = HUDInfo.GetTopPanelSize(v);
                    var size = new Vector2((float) temp[0], (float) temp[1]);
                    var healthDelta = new Vector2(v.Health*size.X/v.MaximumHealth, 0);
                    var manaDelta = new Vector2(v.Mana*size.X/v.MaximumMana, 0);
                    DrawHealthPanel(pos, size, healthDelta);
                    DrawManaPanel(pos, size, manaDelta);
                    DrawStatus(pos,v, size);
                }
            }
            if (!Members.Menu.Item("ultimate.Enable").GetValue<bool>()) return;
            if (!Members.Menu.Item("ultimate.Icon.Enable").GetValue<bool>() &&
                !Members.Menu.Item("ultimate.Info").GetValue<bool>() &&
                !Members.Menu.Item("ultimate.InfoAlways").GetValue<bool>()) return;
            foreach (var v in Members.EnemyHeroes)
            {
                Ability ultimate;
                if (!Ultimate.TryGetValue(v.StoredName(), out ultimate))
                {
                    var spell = Manager.HeroManager.GetAbilityList(v).FirstOrDefault(
                            x => x.IsAbilityType(AbilityType.Ultimate));
                    if (spell != null)
                        Ultimate.Add(v.StoredName(), spell);
                    continue;
                }
                if (ultimate.Level <= 0) continue;
                var pos = Helper.GetTopPanelPosition(v) +
                          new Vector2(Members.Menu.Item("extraPos.X").GetValue<Slider>().Value,
                              Members.Menu.Item("extraPos.Y").GetValue<Slider>().Value);
                var tempS = HUDInfo.GetTopPanelSize(v);
                var size = new Vector2((float) tempS[0], (float) tempS[1]);
                var ultPos = pos + new Vector2(size[0]/2 - 5, size[1] + 1);
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
                if (Members.Menu.Item("ultimate.Icon.Enable").GetValue<bool>())
                    Drawing.DrawRect(ultPos, new Vector2(14, 14), Drawing.GetTexture(path));
                if (Members.Menu.Item("ultimate.Info").GetValue<bool>() &&
                    (Members.Menu.Item("ultimate.InfoAlways").GetValue<bool>() && (
                        ultimate.AbilityState == AbilityState.OnCooldown ||
                        ultimate.AbilityState == AbilityState.NotEnoughMana) ||
                     Utils.IsUnderRectangle(Game.MouseScreenPosition, ultPos.X, ultPos.Y, 15, 15)))
                {
                    var texturename = string.Format("materials/ensage_ui/spellicons/{0}.vmat",
                        ultimate.StoredName());
                    pos = Helper.GetTopPanelPosition(v);
                    var startPos = pos + new Vector2(0, 7*4 + size.Y);
                    size = new Vector2(size.X, size.Y + 15);
                    Drawing.DrawRect(startPos,
                        size,
                        Textures.GetTexture(texturename));
                    string ultimateCd;
                    Vector2 textSize;
                    Vector2 textPos;
                    switch (ultimate.AbilityState)
                    {
                        case AbilityState.OnCooldown:
                            ultimateCd =
                                ((int) Math.Min(ultimate.Cooldown, 999)).ToString(CultureInfo.InvariantCulture);
                            textSize = Drawing.MeasureText(ultimateCd, "Arial",
                                new Vector2((float) (size.Y*.50), size.Y/2), FontFlags.AntiAlias);
                            //Print(v.Name + " cd: " + ultimateCd);
                            textPos = startPos + new Vector2(0, size.Y - textSize.Y);
                            Drawing.DrawRect(textPos - new Vector2(0, 0),
                                new Vector2(textSize.X, textSize.Y),
                                new Color(0, 0, 0, 200));
                            Drawing.DrawText(
                                ultimateCd,
                                textPos,
                                new Vector2(textSize.Y, 0),
                                Color.White,
                                FontFlags.AntiAlias | FontFlags.StrikeOut);
                            break;
                        case AbilityState.NotEnoughMana:
                            ultimateCd =
                                ((int) Math.Min(Math.Abs(v.Mana - ultimate.ManaCost), 999)).ToString(
                                    CultureInfo.InvariantCulture);
                            textSize = Drawing.MeasureText(ultimateCd, "Arial",
                                new Vector2((float) (size.Y*.50), size.Y/2), FontFlags.AntiAlias);
                            textPos = startPos + new Vector2(0, size.Y - textSize.Y);
                            Drawing.DrawRect(textPos - new Vector2(0, 0),
                                new Vector2(textSize.X, textSize.Y),
                                new Color(0, 0, 0, 200));
                            Drawing.DrawText(
                                ultimateCd,
                                textPos,
                                new Vector2(textSize.Y, 0),
                                Color.White,
                                FontFlags.AntiAlias | FontFlags.StrikeOut);
                            Drawing.DrawRect(startPos,
                                new Vector2(size.X, size.Y),
                                new Color(0, 50, 155, 100));
                            break;
                    }
                }
            }
        }
        
        internal class StatusInfo
        {
            private readonly Hero _hero;
            public readonly bool Ally;
            private float _time;
            private string _status;

            public StatusInfo(Hero hero, float time, bool ally)
            {
                _hero = hero;
                _time = time;
                Ally = ally;
                _status = "";
            }

            public Hero GetHero()
            {
                return _hero;
            }

            public string GetTime()
            {
                var curStat = GetStatus();
                if (_status != GetStatus())
                {
                    _time = Game.GameTime;
                    _status = curStat;
                }
                if (curStat == "visible") return curStat;
                return curStat + " " + (int)(Game.GameTime - _time);
            }

            private string GetStatus()
            {
                return !_hero.IsValid ? "heh" : _hero.IsInvisible() ? "invis" : _hero.IsVisible ? "visible" : "in fog";
            }

        }
        private static void DrawStatus(Vector2 pos, Hero hero, Vector2 size, int height = 7)
        {
            if (!Members.Menu.Item("toppanel.Status.Enable").GetValue<bool>()) return;
            var info = Members.StatInfo.Find(x => Equals(x.GetHero(), hero));
            if (info == null)
            {
                Members.StatInfo.Add(new StatusInfo(hero, Game.GameTime, hero.Team == Members.MyHero.Team));
                return;
            }
            if (info.Ally)
            {
                if (!hero.IsVisibleToEnemies || !Members.Menu.Item("toppanel.AllyVision.Enable").GetValue<bool>()) return;
                var newpos = pos + new Vector2(0, size.Y + height*2);
                Drawing.DrawRect(newpos, new Vector2(size.X, height*2), new Color(0, 0, 0, 100));
                Drawing.DrawRect(newpos, new Vector2(size.X, height*2), Color.Black, true);
                const string text = "under vision";
                var textSize = Drawing.MeasureText(text, "Arial",
                    new Vector2((float) (size.Y*.3), size.Y/2), FontFlags.AntiAlias);
                var textPos = newpos + new Vector2(5, 2);
                Drawing.DrawText(
                    text,
                    textPos,
                    new Vector2(textSize.Y, 0),
                    Color.White,
                    FontFlags.AntiAlias | FontFlags.StrikeOut);
            }
            else if (Members.Menu.Item("toppanel.EnemiesStatus.Enable").GetValue<bool>())
            {
                var newpos = pos + new Vector2(0, size.Y + height*2);
                Drawing.DrawRect(newpos, new Vector2(size.X, height*2), new Color(0, 0, 0, 100));
                Drawing.DrawRect(newpos, new Vector2(size.X, height*2), Color.Black, true);
                var text = info.GetTime();
                var textSize = Drawing.MeasureText(text, "Arial",
                    new Vector2((float) (size.Y*.3), size.Y/2), FontFlags.AntiAlias);
                var textPos = newpos + new Vector2(textSize.Y, 2)/*new Vector2(5, 2)*/;
                Drawing.DrawText(
                    text,
                    textPos,
                    new Vector2(textSize.Y, 0),
                    Color.White,
                    FontFlags.AntiAlias | FontFlags.StrikeOut);
            }
        }

        private static void DrawManaPanel(Vector2 pos, Vector2 size, Vector2 manaDelta, int height = 7)
        {
            if (!Members.Menu.Item("toppanel.Mana.Enable").GetValue<bool>()) return;
            var newpos = pos + new Vector2(0, size.Y + height);
            Drawing.DrawRect(newpos, new Vector2(size.X, height), Color.Gray);
            Drawing.DrawRect(newpos, new Vector2(manaDelta.X, height), new Color(0, 0, 255, 255));
            Drawing.DrawRect(newpos, new Vector2(size.X, height), Color.Black, true);
        }

        private static void DrawHealthPanel(Vector2 pos, Vector2 size, Vector2 healthDelta, int height = 7)
        {
            if (!Members.Menu.Item("toppanel.Health.Enable").GetValue<bool>()) return;
            var newpos = pos + new Vector2(0, size.Y + 1);
            Drawing.DrawRect(newpos, new Vector2(size.X, height), new Color(255, 0, 0, 255));
            Drawing.DrawRect(newpos, new Vector2(healthDelta.X, height), new Color(0, 255, 0, 255));
            Drawing.DrawRect(newpos, new Vector2(size.X, height), Color.Black, true);
        }

        private static void DrawSpellPanel(int type)
        {
            List<Hero> selectedHeroes = null;
            switch (type)
            {
                case 0:
                    selectedHeroes = Manager.HeroManager.GetViableHeroes().ToList();
                    break;
                case 1:
                    selectedHeroes = Manager.HeroManager.GetAllyViableHeroes().ToList();
                    break;
                case 2:
                    selectedHeroes = Manager.HeroManager.GetEnemyViableHeroes().ToList();
                    break;
            }
            if (selectedHeroes == null) return;
            foreach (var v in selectedHeroes)
            {
                Vector2 mypos;
                if (!Drawing.WorldToScreen(v.Position, out mypos)) continue;
                if (mypos.X <= -5000 || mypos.X >= 5000) continue;
                if (mypos.Y <= -5000 || mypos.Y >= 5000) continue;
                var start = HUDInfo.GetHPbarPosition(v) +
                            new Vector2(-Members.Menu.Item("spellPanel.ExtraPosX").GetValue<Slider>().Value,
                                Members.Menu.Item("spellPanel.ExtraPosY").GetValue<Slider>().Value);
                var distBetweenSpells = Members.Menu.Item("spellPanel.distBetweenSpells").GetValue<Slider>().Value;
                var distBwtweenLvls = Members.Menu.Item("spellPanel.DistBwtweenLvls").GetValue<Slider>().Value;
                var sizeSpell = Members.Menu.Item("spellPanel.SizeSpell").GetValue<Slider>().Value;
                const int sizey = 9;
                var spells = Manager.HeroManager.GetAbilityList(v);//Members.AbilityDictionary[v.StoredName()];
                if (spells == null || spells.Count==0) continue;
                foreach (var spell in spells/*.Where(x => x.AbilitySlot.ToString() != "-1")*/)
                {
                    var size2 = distBetweenSpells;
                    var extrarange = spell.Level > 4 ? spell.Level - 4 : 0;
                    size2 = (int)(size2 + extrarange * 7);
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
                                      new Vector2(10 - textSize.X / 2, -textSize.Y / 2 + 12);
                        Drawing.DrawText(text, textPos, /*new Vector2(10, 150),*/ Color.White,
                            FontFlags.AntiAlias | FontFlags.DropShadow);
                    }
                    if (spell.Level > 0)
                    {
                        for (var lvl = 1; lvl <= spell.Level; lvl++)
                        {
                            Drawing.DrawRect(start + new Vector2(distBwtweenLvls * lvl, sizey - 6),
                                new Vector2(sizeSpell, sizey - 6),
                                new ColorBGRA(255, 255, 0, 255));
                        }
                    }
                    start += new Vector2(size2, 0);
                }
            }
        }

        internal static class Render
        {
            private static void DrawShadowText(string stext, int x, int y, Color color, Font f)
            {
                f.DrawText(null, stext, x + 1, y + 1, Color.Black);
                f.DrawText(null, stext, x, y, color);
            }

            public static void CurrentDomainDomainUnload(object sender, EventArgs e)
            {
                if (!Checker.IsActive()) return;
                if (Members.RoshanFont != null)
                    Members.RoshanFont.Dispose();
            }

            public static void Drawing_OnEndScene(EventArgs args)
            {
                if (!Checker.IsActive()) return;
                if (Drawing.Direct3DDevice9 == null || Drawing.Direct3DDevice9.IsDisposed)
                {
                    return;
                }
                #region ShowRoshanTimer

                if (!Members.Menu.Item("roshanTimer.Enable").GetValue<bool>()) return;
                var text = "";
                if (!Members.RoshIsAlive)
                {
                    if (Members.RoshanMinutes < 8)
                        text = string.Format("Roshan: {0}:{1:0.} - {2}:{3:0.}", 7 - Members.RoshanMinutes, 59 - Members.RoshanSeconds,
                            10 - Members.RoshanMinutes,
                            59 - Members.RoshanSeconds);
                    else if (Members.RoshanMinutes == 8)
                    {
                        text = string.Format("Roshan: {0}:{1:0.} - {2}:{3:0.}", 8 - Members.RoshanMinutes, 59 - Members.RoshanSeconds,
                            10 - Members.RoshanMinutes,
                            59 - Members.RoshanSeconds);
                    }
                    else if (Members.RoshanMinutes == 9)
                    {
                        text = string.Format("Roshan: {0}:{1:0.} - {2}:{3:0.}", 9 - Members.RoshanMinutes, 59 - Members.RoshanSeconds,
                            10 - Members.RoshanMinutes,
                            59 - Members.RoshanSeconds);
                    }
                    else
                    {
                        text = string.Format("Roshan: {0}:{1:0.}", 0, 59 - Members.RoshanSeconds);
                        if (59 - Members.RoshanSeconds <= 1)
                        {
                            Members.RoshIsAlive = true;
                        }
                    }
                }
                DrawShadowText(Members.RoshIsAlive ? "Roshan alive" : Members.DeathTime == 0 ? "Roshan death" : text, 217, 10,
                    Members.RoshIsAlive ? Color.Green : Color.Red, Members.RoshanFont);

                #endregion
            }

            public static void Drawing_OnPostReset(EventArgs args)
            {
                if (!Checker.IsActive()) return;
                if (Members.RoshanFont != null)
                    Members.RoshanFont.OnLostDevice();
            }

            public static void Drawing_OnPreReset(EventArgs args)
            {
                if (!Checker.IsActive()) return;
                if (Members.RoshanFont != null)
                    Members.RoshanFont.OnLostDevice();
            }
        }
    }
}
