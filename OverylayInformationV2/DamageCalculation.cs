using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ensage;
using Ensage.Common;
using Ensage.Common.AbilityInfo;
using Ensage.Common.Extensions;
using Ensage.Common.Extensions.Damage;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using Ensage.Common.Objects.UtilityObjects;
using log4net;
using PlaySharp.Toolkit.Logging;
using SharpDX;

namespace OverlayInformation
{
    public class DamageCalculation
    {
        private static bool _loaded;
        private static bool IsEnable => Members.Menu.Item("dmgCalc.Enable").GetValue<bool>();
        private static int R(string b) => Members.Menu.Item($"{b}.Red").GetValue<Slider>().Value;
        private static int G(string b) => Members.Menu.Item($"{b}.Green").GetValue<Slider>().Value;
        private static int B(string b) => Members.Menu.Item($"{b}.Blue").GetValue<Slider>().Value;
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
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

        private static void AddToCalcMenu(string s)
            => Members.Menu.Item("dmgCalc.Abilities").GetValue<AbilityToggler>().Add(s);

        private static bool IsAbilityEnable(string s)
            => Members.Menu.Item("dmgCalc.Abilities").GetValue<AbilityToggler>().IsEnabled(s);

        private static readonly List<string> WhiteList = new List<string>
        {
            "monkey_king_boundless_strike"
        };
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
            var randomEnemy = Manager.HeroManager.GetEnemyViableHeroes().FirstOrDefault(x => !x.IsMagicImmune());
            if (randomEnemy==null || !randomEnemy.IsValid)
                return;
            foreach (
                var spell in
                    Members.MyHero.Spellbook.Spells.Where(
                        x =>
                            !x.IsAbilityBehavior(AbilityBehavior.Passive) && !InSys.Contains(x) && 
                            (x.GetDamage(0) > 0 || WhiteList.Contains(x.Name) || AbilityDamage.CalculateDamage(x, Members.MyHero, randomEnemy) > 0)))
            {
                InSys.Add(spell);
                AddToCalcMenu(spell.StoredName());
                Log.Debug($"dmgCalc.ability.new [{spell.Name}] [{spell.GetDamage(0)}]");
            }
            try
            {
                var items = Manager.HeroManager.GetItemList(Members.MyHero);
                items =
                    items.Where(
                        x =>
                            !InSys.Contains(x) && AbilityDamage.CalculateDamage(x, Members.MyHero, randomEnemy) > 0 || x.GetDamage(0)>0)
                        .ToList();
                foreach (var spell in items)
                {
                    InSys.Add(spell);
                    AddToCalcMenu(spell.StoredName());
                    Log.Debug($"dmgCalc.item.new [{spell.Name}]");
                }
            }
            catch (Exception)
            {
                //
            }
            
        }

        private static readonly int[] MkDmg = {
            0,80,120,160,200
        };
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Checker.IsActive())
                return;
            if (!IsEnable)
                return;
            if (InSys == null || InSys.Count==0)
                return;
            var haveEb =
                InSys.Any(
                    x => IsAbilityEnable(x.StoredName()) && x.StoredName() == "item_ethereal_blade" && x.CanBeCasted());
            var haveVeil =
                InSys.Any(
                    x => IsAbilityEnable(x.StoredName()) && x.StoredName() == "item_veil_of_discord" && x.CanBeCasted());
            var myPhysDmg = 0f;
            if (Members.MyHero.ClassId == ClassId.CDOTA_Unit_Hero_MonkeyKing)
            {
                var extraMkAbility = Members.MyHero.FindSpell("special_bonus_unique_monkey_king", true)?.Level == 1;
                var passiveDmg = MkDmg[Members.MyHero.FindSpell("monkey_king_jingu_mastery", true).Level];
                myPhysDmg = Members.MyHero.MinimumDamage + Members.MyHero.BonusDamage +
                                (Members.MyHero.HasModifier("modifier_monkey_king_quadruple_tap_bonuses")
                                    ? passiveDmg
                                    : 0);
                var boundless = Members.MyHero.FindSpell("monkey_king_boundless_strike", true).Level;
                var coef = 1.2f + 0.20f*boundless;
                myPhysDmg *= extraMkAbility ? 3f : coef;
            }
            foreach (var v in Manager.HeroManager.GetEnemyViableHeroes())
            {
                try
                {
                    var pos = HUDInfo.GetHPbarPosition(v);
                    if (pos.IsZero)
                        continue;
                    var extraDamage = haveEb && !v.HasModifier("modifier_item_ethereal_blade_ethereal") ? 40 : 0;
                    extraDamage += haveVeil && !v.HasModifier("modifier_item_veil_of_discord_debuff") ? 25 : 0;
                    var myDmg = InSys.Where(x => x.CanBeCasted() && IsAbilityEnable(x.StoredName()))
                        .Sum(
                            x => WhiteList.Contains(x.Name)
                                ? Calculations.DamageTaken(v,
                                    myPhysDmg,
                                    DamageType.Physical, Members.MyHero)
                                : AbilityDamage.CalculateDamage(x, Members.MyHero, v,
                                    minusMagicResistancePerc: extraDamage));
                    
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
                    var text = $"{healthAfterShit} ({(int) myDmg})";
                    var textSize = Drawing.MeasureText(text, "Arial",
                            new Vector2((float)(size * 1.5), 500), FontFlags.AntiAlias);
                    var textPos = pos + new Vector2(HUDInfo.GetHPBarSizeX()+4,0);
                    var isEno = healthAfterShit < 0;
                    var name = isEno ? "killableCol" : "defCol";
                    Drawing.DrawText(
                        text,
                        textPos,
                        new Vector2(textSize.Y, 0),
                        new Color(R(name), G(name), B(name), 255),
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
