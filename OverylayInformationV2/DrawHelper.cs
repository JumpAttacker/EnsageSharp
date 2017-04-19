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
        private static readonly float Percent = HUDInfo.RatioPercentage();
        public static void Overlay(EventArgs args)
        {
            if (!Checker.IsActive()) return;
            //Printer.Print($"x: {HudInfoNew.ScreenSizeX()}");
            if (Members.Menu.Item("spellpanel.Enable").GetValue<bool>() && Members.Menu.Item("spellpanel.OldMethod.Enable").GetValue<bool>())
                DrawSpellPanel(Members.Menu.Item("spellpanel.Targets").GetValue<StringList>().SelectedIndex);
            if (Members.Menu.Item("toppanel.Enable").GetValue<bool>())
                DrawTopPanel(Members.Menu.Item("toppanel.Targets").GetValue<StringList>().SelectedIndex);
            /*if (Members.Menu.Item("dangitems.Enable").GetValue<bool>())
                DrawDangeItems();*/
            if (Members.Menu.Item("lastPosition.Enable").GetValue<bool>())
                DrawLastPosition();
            if (Members.Menu.Item("netWorth.Enable").GetValue<bool>())
                DrawNetWorth();
            if (Members.Menu.Item("netWorthBar.Enable").GetValue<bool>())
                DrawNetWorthBar();
        }

        private static void DrawNetWorthBar()
        {
            var startPos = HudInfoNew.GetFakeTopPanelPosition(0, Team.Radiant) +
                           new Vector2((float)HudInfoNew.GetTopPanelSizeX() * 5, (float)HudInfoNew.GetTopPanelSizeY());
            var endPos = HudInfoNew.GetFakeTopPanelPosition(5, Team.Dire);
            var size = new Vector2(endPos.X - startPos.X,
                Members.Menu.Item("netWorthBar.Size").GetValue<Slider>().Value*Percent);
            DrawNetWorthBarStageOne(startPos,size);
            
        }

        private static bool IsDrawPercents => Members.Menu.Item("netWorthBar.Percents.Enable").GetValue<bool>();
        private static bool IsDrawTeamWorth => Members.Menu.Item("netWorthBar.TeamWorth.Enable").GetValue<bool>();
        private static float GetNetworthBarCoef => Members.Menu.Item("netWorthBar.coef").GetValue<Slider>().Value;

        private static Color GetNetworthBarColorForRadiant
            =>
                new Color(
                    Members.Menu.Item("netWorthBar.Radiant.Red").GetValue<Slider>().Value,
                    Members.Menu.Item("netWorthBar.Radiant.Green").GetValue<Slider>().Value,
                    Members.Menu.Item("netWorthBar.Radiant.Blue").GetValue<Slider>().Value,
                    Members.Menu.Item("netWorthBar.Radiant.Alpha").GetValue<Slider>().Value);
        private static Color GetNetworthBarColorForDire
            =>
                new Color(
                    Members.Menu.Item("netWorthBar.Dire.Red").GetValue<Slider>().Value,
                    Members.Menu.Item("netWorthBar.Dire.Green").GetValue<Slider>().Value,
                    Members.Menu.Item("netWorthBar.Dire.Blue").GetValue<Slider>().Value,
                    Members.Menu.Item("netWorthBar.Dire.Alpha").GetValue<Slider>().Value);

        private static void DrawNetWorthBarStageOne(Vector2 startPos, Vector2 size)
        {
            if (Members.ItemDictionary.Count == 0)
            {
                return;
            }
            long radiantNetworh = 0, direNetwoth = 0;
            foreach (var v in Members.Heroes)
            {
                try
                {
                    long worth;
                    if (!Members.NetWorthDictionary.TryGetValue(v.StoredName(), out worth))
                    {
                        continue;
                    }
                    /*var dividedWeStand = v.FindSpell("meepo_divided_we_stand") as DividedWeStand;
                    if (dividedWeStand != null && (v.ClassID == ClassID.CDOTA_Unit_Hero_Meepo) && dividedWeStand.UnitIndex > 0)
                    {
                        continue;
                    }*/
                    if (Members.MeepoIgnoreList.Contains(v))
                        continue;
                    if (v.Team == Team.Radiant)
                        radiantNetworh += worth;
                    else
                        direNetwoth += worth;
                }
                catch (Exception)
                {
                    Printer.Print("[NetWorthBar][findMaxNetWorth]: " + v.StoredName());
                    continue;
                }
            }

            var percent = 100 * radiantNetworh / Math.Max(1, radiantNetworh+direNetwoth);
            var currentSize = size.X / 100 * percent;
            var lineSize = new Vector2(currentSize, size.Y);
            var endOfGreen = startPos + new Vector2(lineSize.X, 0);
            //var color2 = worth == maxWorth ? Color.Yellow : Color.Black;
            Color leftClr, rightClr;
            if (Members.MyPlayer.Team != Team.Radiant)
            {
                rightClr = GetNetworthBarColorForRadiant;
                leftClr = GetNetworthBarColorForDire;
                percent = 100 - percent;
            }
            else
            {
                leftClr = GetNetworthBarColorForRadiant;
                rightClr = GetNetworthBarColorForDire;
            }
            Drawing.DrawRect(startPos, lineSize, leftClr);
            Drawing.DrawRect(endOfGreen, new Vector2(size.X - lineSize.X,lineSize.Y), rightClr);
            Drawing.DrawRect(startPos, size, new Color(0, 0, 0, 255), true);
            var text = $"{percent}%";
            var textSize = Drawing.MeasureText(text, "Arial",
                new Vector2((float)(size.Y * .95), size.Y / 2), FontFlags.AntiAlias);
            var textPos = endOfGreen - new Vector2(textSize.X/2, /*lineSize.Y / 2 - textSize.Y / 2*/0);
            var coef = GetNetworthBarCoef/10;
            if (IsDrawPercents)
                Drawing.DrawText(
                    text,
                    textPos,
                    new Vector2(textSize.Y, 0),
                    Color.White,
                    FontFlags.AntiAlias | FontFlags.StrikeOut);
            if (IsDrawTeamWorth)
            {
                text = $"{radiantNetworh}";
                Drawing.DrawText(
                    text,
                    startPos + new Vector2(0, size.Y),
                    new Vector2(textSize.Y/coef, 0),
                    Color.White,
                    FontFlags.AntiAlias | FontFlags.StrikeOut);
                text = $"{direNetwoth}";
                Drawing.DrawText(
                    text,
                    startPos + new Vector2(size.X - textSize.X/coef, size.Y),
                    new Vector2(textSize.Y/coef, 0),
                    Color.White,
                    FontFlags.AntiAlias | FontFlags.StrikeOut);
            }
        }

        private static void DrawNetWorth()
        {
            var startPos = new Vector2(Members.Menu.Item("netWorth.X").GetValue<Slider>().Value,
                Members.Menu.Item("netWorth.Y").GetValue<Slider>().Value);
            var size = new Vector2(Members.Menu.Item("netWorth.SizeX").GetValue<Slider>().Value * Percent,
                Members.Menu.Item("netWorth.SizeY").GetValue<Slider>().Value * Percent);
            Drawing.DrawRect(startPos, size + new Vector2(0, 34), new Color(0, 0, 0, 100));
            var r = Members.Menu.Item("netWorth.Red").GetValue<Slider>().Value;
            var g = Members.Menu.Item("netWorth.Green").GetValue<Slider>().Value;
            var b = Members.Menu.Item("netWorth.Blue").GetValue<Slider>().Value;
            Drawing.DrawRect(startPos, size + new Vector2(0, 34), new Color(r, g, b, 255), true);
            DrawPlayer(startPos + new Vector2(2, 2), size);

        }
        private static void DrawPlayer(Vector2 pos, Vector2 size)
        {
            var i = 0;
            if (Members.ItemDictionary.Count == 0)
            {
                return;
            }
            long maxWorth = 0;
            var playersWithWorth=new List<Hero>();
            foreach (var v in Members.Heroes)
            {
                try
                {
                    long worth;
                    if (!Members.NetWorthDictionary.TryGetValue(v.StoredName(), out worth))
                    {
                        continue;
                    }
                    /*var dividedWeStand = v.FindSpell("meepo_divided_we_stand") as DividedWeStand;
                    if (dividedWeStand != null && (v.ClassID == ClassID.CDOTA_Unit_Hero_Meepo) && dividedWeStand.UnitIndex > 0)
                    {
                        continue;
                    }*/
                    if (Members.MeepoIgnoreList.Contains(v))
                        continue;
                    if (maxWorth < worth)
                        maxWorth = worth;
                    playersWithWorth.Add(v);
                }
                catch (Exception)
                {
                    Printer.Print("[NetWorth][findMaxNetWorth]: " + v.StoredName());
                    continue;
                }
            }

            if (Members.Menu.Item("netWorth.Order").GetValue<bool>())
                playersWithWorth =
                    new List<Hero>(playersWithWorth.OrderByDescending(x => Members.NetWorthDictionary[x.StoredName()]));
            foreach (var v in playersWithWorth)
            {
                long worth;
                try
                {
                    if (!Members.NetWorthDictionary.TryGetValue(v.Name, out worth))
                        continue;
                }
                catch (Exception)
                {
                    Printer.Print("[NetWorth]: " + v.StoredName());
                    continue;
                }

                var heroPos = pos + new Vector2(0, (size.Y / 10 + 3) * i + 2);
                Drawing.DrawRect(heroPos, size / 10,
                    Textures.GetTexture("materials/ensage_ui/heroes_horizontal/" +
                                        v.StoredName().Substring("npc_dota_hero_".Length) + ".vmat"));
                var defaultSize = size.X - (size.X/10 + 10);
                var percent = 100*worth/Math.Max(1,maxWorth);
                var currentSize = defaultSize/100*percent;
                var color = v.Team==Members.MyHero.Team ? new Color(0,155,0,155) : new Color(155, 0, 0, 155);
                var lineStartPos = heroPos + new Vector2(size.X/10 + 5, 0);
                var lineSize = new Vector2(currentSize, size.Y/10);
                //var color2 = worth == maxWorth ? Color.Yellow : Color.Black;
                Drawing.DrawRect(lineStartPos, lineSize, color);
                Drawing.DrawRect(lineStartPos, lineSize, Color.Black, true);
                var heroWorthText = worth.ToString();
                var textSize = Drawing.MeasureText(heroWorthText, "Arial",
                    new Vector2((float)(lineSize.Y * .95), lineSize.Y / 2), FontFlags.AntiAlias);
                var textPos = lineStartPos + new Vector2(2, lineSize.Y / 2 - textSize.Y/2);
                Drawing.DrawText(
                    heroWorthText,
                    textPos,
                    new Vector2(textSize.Y, 0),
                    Color.White,
                    FontFlags.AntiAlias | FontFlags.StrikeOut);   
                i++;
            }
        }

        private static void DrawLastPosition()
        {
            //var particleAttachment = ParticleAttachment.OverheadFollow;
           // new ParticleEffect(@"", target, ParticleAttachment.OverheadFollow);
            foreach (var hero in Members.EnemyHeroes.Where(x => x.IsAlive && !x.IsVisible))
            {
                if (Members.Menu.Item("lastPosition.Enable.Minimap").GetValue<bool>())
                {
                    var size = new Vector2(Members.Menu.Item("lastPosition.Minimap.X").GetValue<Slider>().Value,
                        Members.Menu.Item("lastPosition.Minimap.X").GetValue<Slider>().Value);
                    if (Members.Menu.Item("lastPosition.Enable.Prediction").GetValue<bool>())
                    {
                        if (Members.PredictionTimes.ContainsKey(hero.StoredName()))
                            Drawing.DrawRect(Helper.WorldToMinimap(Prediction.InFront(hero,
                                hero.MovementSpeed*(Game.GameTime - Members.PredictionTimes[hero.StoredName()]))) +
                                             new Vector2(-size.X/2, -size.Y/2), size,
                                Helper.GetHeroTextureMinimap(hero.StoredName()));
                    }
                    else
                    {
                        Drawing.DrawRect(Helper.WorldToMinimap(hero.Position) + new Vector2(-size.X/2, -size.Y/2), size,
                            Helper.GetHeroTextureMinimap(hero.StoredName()));
                    }
                }
                if (Members.Menu.Item("lastPosition.Enable.Map").GetValue<bool>())
                {
                    Vector2 newPos;
                    if (Drawing.WorldToScreen(hero.Position, out newPos))
                    {
                        var size = new Vector2(Members.Menu.Item("lastPosition.Map.X").GetValue<Slider>().Value,
                                Members.Menu.Item("lastPosition.Map.X").GetValue<Slider>().Value);
                        if (Members.Menu.Item("lastPosition.Enable.Prediction").GetValue<bool>())
                        {
                            if (Members.PredictionTimes.ContainsKey(hero.StoredName()))
                                Drawing.DrawRect(
                                    Drawing.WorldToScreen(Prediction.InFront(hero,
                                        hero.MovementSpeed*(Game.GameTime - Members.PredictionTimes[hero.StoredName()]))) +
                                    new Vector2(-size.X/2, (float) (-size.Y*2.5)),
                                    size,
                                    Textures.GetHeroTexture(hero.StoredName()));
                        }
                        else
                        {
                            Drawing.DrawRect(newPos + new Vector2(-size.X / 2, -size.Y * 2), size,
                                Textures.GetHeroTexture(hero.StoredName()));
                        }
                    }
                }
            }
        }

        private static void DrawDangeItems()
        {
            foreach (var hero in Manager.HeroManager.GetEnemyViableHeroes())
            {
                try
                {
                    if (Manager.HeroManager.GetItemList(hero) == null) continue;
                    var iPos = HUDInfo.GetHPbarPosition(hero);
                    var iSize = new Vector2(HUDInfo.GetHPBarSizeX(hero), HUDInfo.GetHpBarSizeY(hero));
                    float count = 0;
                    List<Item> items;
                    try
                    {
                        if (!Members.ItemDictionary.TryGetValue(hero.Handle, out items))
                            continue;
                    }
                    catch (Exception)
                    {
                        Printer.Print("[DrawDangeItems]: " + hero.StoredName());
                        continue;
                    }
                    foreach (
                        var item in
                            items
                                .Where(x => x != null && x.IsValid && Members.Menu.Item("dangitems.List").GetValue<AbilityToggler>().IsEnabled(x.Name))
                        )
                    {
                        var itemname = $"materials/ensage_ui/items/{item.Name.Replace("item_", "")}.vmat";
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
                catch (Exception e)
                {
                    Printer.Print($"[DrawDangeItems]: all --> {e.StackTrace}");
                }
                
            }
        }

        private static readonly Dictionary<string, Ability> Ultimate = new Dictionary<string, Ability>();
        private static readonly Dictionary<uint, float> LastTimeDictionary = new Dictionary<uint, float>();
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
                    try
                    {
                        var pos = Helper.GetTopPanelPosition(v) +
                                  new Vector2(Members.Menu.Item("extraPos.X").GetValue<Slider>().Value,
                                      Members.Menu.Item("extraPos.Y").GetValue<Slider>().Value);
                        var temp = HudInfoNew.GetTopPanelSize(v);
                        //var temp = HUDInfo.GetTopPanelSize(v);
                        var size = new Vector2((float)temp[0], (float)temp[1]);
                        var healthDelta = new Vector2(v.Health * size.X / v.MaximumHealth, 0);
                        var manaDelta = new Vector2(v.Mana * size.X / v.MaximumMana, 0);
                        DrawHealthPanel(pos, size, healthDelta);
                        DrawManaPanel(pos, size, manaDelta);
                        DrawStatus(pos, v, size);
                    }
                    catch (Exception)
                    {
                        Printer.Print($"[DrawTopPanel: selectedHeroes] --> {v!=null && v.IsValid}");
                    }
                    
                }
            }
            if (!Members.Menu.Item("ultimate.Enable").GetValue<bool>()) return;
            /*if (!Members.Menu.Item("ultimate.Icon.Enable").GetValue<bool>() &&
                !Members.Menu.Item("ultimate.Info").GetValue<bool>() &&
                !Members.Menu.Item("ultimate.InfoAlways").GetValue<bool>()) return;*/
            foreach (var v in Members.EnemyHeroes)
            {
                var ablist = Manager.HeroManager.GetAbilityList(v);
                if (ablist == null) continue;
                try
                {
                    Ability ultimate;
                    if (!Ultimate.TryGetValue(v.StoredName(), out ultimate))
                    {
                        var spell = ablist.FirstOrDefault(x => x.IsAbilityType(AbilityType.Ultimate));
                        if (spell != null)
                        {
                            Ultimate.Remove(v.StoredName());
                            Ultimate.Add(v.StoredName(), spell);
                        }
                        continue;
                    }
                    if (ultimate == null || !ultimate.IsValid || ultimate.Level <= 0) continue;
                    float lastTime;
                    if (v.IsVisible)
                    {
                        if (!LastTimeDictionary.TryGetValue(v.Handle, out lastTime))
                        {
                            LastTimeDictionary.Add(v.Handle, Game.RawGameTime);
                        }
                        else
                        {
                            LastTimeDictionary[v.Handle] = Game.RawGameTime;
                        }
                    }
                    else
                    {
                        LastTimeDictionary.TryGetValue(v.Handle, out lastTime);
                    }
                    var pos = Helper.GetTopPanelPosition(v) +
                              new Vector2(Members.Menu.Item("extraPos.X").GetValue<Slider>().Value,
                                  Members.Menu.Item("extraPos.Y").GetValue<Slider>().Value);
                    var tempS = HudInfoNew.GetTopPanelSize(v);
                    //var tempS = HUDInfo.GetTopPanelSize(v);
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
                    var cooldown = v.IsVisible ? ultimate.Cooldown : ultimate.Cooldown - Game.RawGameTime + lastTime;
                    cooldown = Math.Max(0, cooldown);
                    if (Members.Menu.Item("ultimate.Type").GetValue<StringList>().SelectedIndex == 0 &&
                        Members.Menu.Item("ultimate.Info").GetValue<bool>() &&
                        (Members.Menu.Item("ultimate.InfoAlways").GetValue<bool>() && (
                            ultimate.AbilityState == AbilityState.OnCooldown ||
                            ultimate.AbilityState == AbilityState.NotEnoughMana) ||
                         Utils.IsUnderRectangle(Game.MouseScreenPosition, ultPos.X, ultPos.Y, 15, 15)))
                    {
                        var texturename = $"materials/ensage_ui/spellicons/{ultimate.StoredName()}.vmat";
                        pos = Helper.GetTopPanelPosition(v) +
                              new Vector2(Members.Menu.Item("extraPos.X").GetValue<Slider>().Value,
                                  Members.Menu.Item("extraPos.Y").GetValue<Slider>().Value);
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
                                    ((int) Math.Min(cooldown, 999)).ToString(CultureInfo.InvariantCulture);
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
                                if (Members.Menu.Item("ultimate.Icon.Extra.Enable").GetValue<bool>() &&
                                    ultimate.ManaCost > v.Mana)
                                {
                                    ultimateCd =
                                        ((int) Math.Min(Math.Abs(v.Mana - ultimate.ManaCost), 999)).ToString(
                                            CultureInfo.InvariantCulture);
                                    textSize = Drawing.MeasureText(ultimateCd, "Arial",
                                        new Vector2((float) (size.Y*.50), size.Y/2), FontFlags.AntiAlias);
                                    textPos = startPos + new Vector2(size.X - textSize.X, 0);
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
                                }
                                break;
                            case AbilityState.NotEnoughMana:
                                ultimateCd =
                                    ((int) Math.Min(Math.Abs(v.Mana - ultimate.ManaCost), 999)).ToString(
                                        CultureInfo.InvariantCulture);
                                textSize = Drawing.MeasureText(ultimateCd, "Arial",
                                    new Vector2((float) (size.Y*.50), size.Y/2), FontFlags.AntiAlias);
                                textPos = startPos + new Vector2(size.X - textSize.X, 0);
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
                    else if (ultimate.AbilityState == AbilityState.OnCooldown)
                    {
                        pos = Helper.GetTopPanelPosition(v) +
                              new Vector2(Members.Menu.Item("extraPos.X").GetValue<Slider>().Value,
                                  Members.Menu.Item("extraPos.Y").GetValue<Slider>().Value);
                        var startPos = pos + new Vector2(0, 7*4 + size.Y);
                        var cd = cooldown;
                        var manaDelta = new Vector2(cd*size.X/ultimate.CooldownLength, 0);
                        //size = new Vector2(manaDelta.X, 7);
                        DrawUltimatePanel(startPos, size, manaDelta, (int) cd,
                            Members.Menu.Item("ultimate.Line.Size").GetValue<Slider>().Value);
                        /*Drawing.DrawRect(startPos,
                            size, Color.Yellow);*/
                    }
                }
                catch (Exception)
                {
                    Printer.Print("[DrawTopPanel:ultimate] " + v.StoredName() + " Type: " + type);
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
        public static bool IsEnable
            => true/*Members.Menu.Item("toppanel.AllyVision.Type").GetValue<StringList>().SelectedIndex == 1*/;
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
                if (!hero.IsVisibleToEnemies || !Members.Menu.Item("toppanel.AllyVision.Enable").GetValue<bool>() || !IsEnable) return;
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

        private static void DrawUltimatePanel(Vector2 newpos, Vector2 size, Vector2 ultimateDelta, int cd, int height = 10)
        {
            if (Members.Menu.Item("ultimate.Type").GetValue<StringList>().SelectedIndex != 1) return;
            Drawing.DrawRect(newpos, new Vector2(size.X, height), new Color(0, 0, 0, 255));
            Drawing.DrawRect(newpos, new Vector2(ultimateDelta.X, height), Color.Yellow);
            Drawing.DrawRect(newpos, new Vector2(size.X, height), Color.Black, true);
            var ultimateCd = Math.Min(Math.Abs(cd), 999).ToString(
                                        CultureInfo.InvariantCulture);
            var textSize = Drawing.MeasureText(ultimateCd, "Arial",
                                    new Vector2((float)(height * .95), (float)(height * .95)), FontFlags.AntiAlias);
            var textPos = newpos + new Vector2(size.X / 2 - textSize.Y/2, 1);
            Drawing.DrawRect(textPos - new Vector2(0, 0),
                new Vector2(textSize.X, textSize.Y),
                new Color(0, 0, 0, 200));
            Drawing.DrawText(
                ultimateCd,
                textPos,
                new Vector2(textSize.Y, 0),
                Color.White,
                FontFlags.AntiAlias | FontFlags.StrikeOut);
        }

        private static void DrawSpellPanel(int type)
        {
            List<Hero> selectedHeroes = null;
            switch (type)
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
                    var spells = Manager.HeroManager.GetAbilityList(v); //Members.AbilityDictionary[v.StoredName()];
                    if (spells == null || spells.Count == 0) continue;
                    foreach (var spell in spells /*.Where(x => x.AbilitySlot.ToString() != "-1")*/)
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
                            var text = $"{cd:0.#}";
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
                }
                catch
                {
                    Printer.Print("[SpellPanel]: "+v.StoredName());
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
                        text =
                            $"Roshan: {7 - Members.RoshanMinutes}:{59 - Members.RoshanSeconds:0.} - {10 - Members.RoshanMinutes}:{59 - Members.RoshanSeconds:0.}";
                    else if (Members.RoshanMinutes == 8)
                    {
                        text =
                            $"Roshan: {8 - Members.RoshanMinutes}:{59 - Members.RoshanSeconds:0.} - {10 - Members.RoshanMinutes}:{59 - Members.RoshanSeconds:0.}";
                    }
                    else if (Members.RoshanMinutes == 9)
                    {
                        text =
                            $"Roshan: {9 - Members.RoshanMinutes}:{59 - Members.RoshanSeconds:0.} - {10 - Members.RoshanMinutes}:{59 - Members.RoshanSeconds:0.}";
                    }
                    else
                    {
                        text = $"Roshan: {0}:{59 - Members.RoshanSeconds:0.}";
                        if (59 - Members.RoshanSeconds <= 1)
                        {
                            Members.RoshIsAlive = true;
                        }
                    }
                }
                DrawShadowText(Members.RoshIsAlive ? "Roshan alive" : Members.DeathTime == 0 ? "Roshan death" : text, 217, 10,
                    Members.RoshIsAlive ? Color.Green : Color.Red, Members.RoshanFont);

                if (Members.AegisEvent)
                {
                    text = $"Aegis Timer: {4 - Members.AegisMinutes}:{59 - Members.AegisSeconds:0.}";
                    DrawShadowText(text, 217, 27,Color.GreenYellow, Members.RoshanFont);
                }

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
