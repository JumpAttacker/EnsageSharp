using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.AbilityInfo;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using Ensage.Common.Objects.UtilityObjects;
using SharpDX;

namespace OverlayInformation
{
    public class DamageCalculation
    {
        private static bool _loaded;
        private static bool IsEnable => Members.Menu.Item("dmgCalc.Enable").GetValue<bool>();
        private static int R => Members.Menu.Item("dmgCalc.Red").GetValue<Slider>().Value;
        private static int G => Members.Menu.Item("dmgCalc.Green").GetValue<Slider>().Value;
        private static int B => Members.Menu.Item("dmgCalc.Blue").GetValue<Slider>().Value;

        private static Sleeper _sleeper;
        public static List<Ability> InSys;
        public DamageCalculation()
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
                Game.OnUpdate -= GameOnOnUpdate;
                _loaded = false;
            };
        }

        private static void Load()
        {
            _sleeper = new Sleeper();
            InSys = new List<Ability>();
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += GameOnOnUpdate;
        }

        private static void GameOnOnUpdate(EventArgs args)
        {
            if (!Checker.IsActive())
                return;
            if (!IsEnable)
                return;
            if (_sleeper.Sleeping)
                return;
            _sleeper.Sleep(500);
            InSys = InSys.Where(x => x != null && x.IsValid).ToList();
            var randomEnemy = Manager.HeroManager.GetEnemyViableHeroes().FirstOrDefault();
            if (randomEnemy==null || !randomEnemy.IsValid)
                return;
            foreach (
                var spell in
                    Members.MyHero.Spellbook.Spells.Where(
                        x =>
                            !x.IsAbilityBehavior(AbilityBehavior.Passive) && !InSys.Contains(x) && x.Level > 0 &&
                            AbilityDamage.CalculateDamage(x, Members.MyHero, randomEnemy) > 0))
            {
                InSys.Add(spell);
            }
            try
            {
                var items = Manager.HeroManager.GetItemList(Members.MyHero);
                items =
                    items.Where(
                        x =>
                            !InSys.Contains(x) && AbilityDamage.CalculateDamage(x, Members.MyHero, randomEnemy) > 0)
                        .ToList();
                foreach (var spell in items)
                {
                    InSys.Add(spell);
                }
            }
            catch (Exception)
            {
                //
            }
            
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Checker.IsActive())
                return;
            if (!IsEnable)
                return;
            if (InSys == null || InSys.Count==0)
                return;
            var haveEb = InSys.Any(x => x.StoredName() == "item_ethereal_blade" && x.CanBeCasted());
            foreach (var v in Manager.HeroManager.GetEnemyViableHeroes())
            {
                try
                {
                    var pos = HUDInfo.GetHPbarPosition(v);
                    if (pos.IsZero)
                        continue;
                    var myDmg = InSys.Where(x => x.CanBeCasted())
                        .Sum(
                            x =>
                                AbilityDamage.CalculateDamage(x, Members.MyHero, v,
                                    minusMagicResistancePerc: haveEb ? 40 : 0));
                    var health = v.Health;
                    var extraLife =
                        (uint) (Manager.HeroManager.GetItemList(v)
                            .Any(x => x.StoredName() == "item_infused_raindrop" && x.Cooldown <= 0)
                            ? 120
                            : 0);
                    if (extraLife > 100)
                    {
                        var needToCalcExtraLife =
                            InSys.Any(
                                x =>
                                    x.DamageType == DamageType.Magical &&
                                    AbilityDamage.CalculateDamage(x, Members.MyHero, v,
                                        minusMagicResistancePerc: haveEb ? 40 : 0) > 120);
                        health += needToCalcExtraLife ? extraLife : 0;
                    }
                    
                    var healthAfterShit = (int) (health - myDmg);
                    var size = HUDInfo.GetHpBarSizeY();
                    var text = $"{healthAfterShit} ({myDmg})";
                    var textSize = Drawing.MeasureText(text, "Arial",
                            new Vector2((float)(size * 1.5), 500), FontFlags.AntiAlias);
                    var textPos = pos + new Vector2(HUDInfo.GetHPBarSizeX()+4,0);
                    Drawing.DrawText(
                        text,
                        textPos,
                        new Vector2(textSize.Y, 0),
                        new Color(R, G, B, 255),
                        FontFlags.AntiAlias | FontFlags.StrikeOut);
                }
                catch (Exception)
                {
                    Printer.Print($"[DamageCalculation]: {v.StoredName()}");
                }
            }
        }
    }
}
