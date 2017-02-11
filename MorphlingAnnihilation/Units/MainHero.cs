using System;
using System.Linq;
using System.Reflection;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.Common.Objects;
using Ensage.Common.Objects.UtilityObjects;
using log4net;
using MorphlingAnnihilation.Interface;
using PlaySharp.Toolkit.Logging;

namespace MorphlingAnnihilation.Units
{
    public class MainHero : ExtendedUnit
    {
        private static Sleeper _morphSleeper;
        private static Sleeper _safeTpSleeper;
        private static Sleeper _ethereal;
        private static MultiSleeper _comboSleeper;
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public override void UseItems(Hero target)
        {
            var inventory =
                Me.Inventory.Items.Where(
                    x => MenuManager.IsItemEnable(x.StoredName()) && x.CanBeCasted() && x.CanHit(target) && !_comboSleeper.Sleeping(x));
            foreach (var ability in inventory)
            {
                if (ability.StoredName() == "item_ethereal_blade")
                    _ethereal.Sleep(1000);
                if (ability.DamageType == DamageType.Magical || ability.StoredName().Contains("dagon"))
                    if (_ethereal.Sleeping && !target.HasModifier("modifier_item_ethereal_blade_ethereal"))
                    {
                        continue;
                    }
                if ((ability.AbilityBehavior & AbilityBehavior.Point) != 0)
                {
                    if (ability.IsSkillShot())
                    {
                        if (!ability.CastSkillShot(target))
                            continue;
                    }
                    else
                        ability.UseAbility(target.Position);
                }
                else if ((ability.AbilityBehavior & AbilityBehavior.UnitTarget) != 0)
                {
                    if (ability.TargetTeamType == TargetTeamType.Enemy || ability.TargetTeamType == TargetTeamType.All)
                        ability.UseAbility(target);
                    else
                        continue;
                }
                else
                    ability.UseAbility();
                var delay = Me.GetAbilityDelay(target, ability);
                Log.Debug($"Item: {ability.StoredName()} -> {delay}ms");
                _comboSleeper.Sleep(delay, ability);
            }
        }

        public MainHero(Hero me) : base(me)
        {
            _morphSleeper = new Sleeper();
            _safeTpSleeper = new Sleeper();
            _ethereal = new Sleeper();
            _comboSleeper = new MultiSleeper();
        }

        public void Morph()
        {
            if (_morphSleeper.Sleeping)
                return;
            var toAgi = Me.Spellbook.Spell3;
            var toStr = Me.Spellbook.Spell4;
            var minHp = MenuManager.MorphGetMinHealth;
            var minHpPercent = MenuManager.MorphGetMinHealthPercent;
            var minMp = MenuManager.MorphGetMinMana;
            var curentHp = Me.Health;
            var currentManaPercent = Me.Mana/Me.MaximumMana*100;
            var currentHealthPercent = (float)Me.Health/ (float)Me.MaximumHealth*100f;
            if (toAgi != null && toAgi.CanBeCasted() && minMp <= currentManaPercent)
            {
                if (curentHp < minHp || currentHealthPercent < minHpPercent)
                {
                    if (!Me.HasModifier("modifier_morphling_morph_str"))
                    {
                        toStr.ToggleAbility();
                        Log.Debug($"[Enable] [{curentHp} < {minHp}] or [{currentHealthPercent}% < {minHpPercent}%]");
                    }
                }
                else if (Me.HasModifier("modifier_morphling_morph_str"))
                {
                    toStr.ToggleAbility();
                    Log.Debug($"[Disable] [{curentHp} >= {minHp}] or [{currentHealthPercent}% >= {minHpPercent}%]");
                }
            }
            _morphSleeper.Sleep(300);
        }

        public void SafeTp()
        {
            if (_safeTpSleeper.Sleeping)
                return;
            var safetp = Me.FindSpell("morphling_morph_replicate");
            var repka = Me.FindSpell("morphling_replicate");
            var minHp = MenuManager.MinHpForSafeTp;
            var curentHp = Me.Health;

            if (safetp != null && repka != null && safetp.CanBeCasted() && minHp > curentHp && repka.Cooldown > 0)
            {
                Log.Debug($"[SafeTP] {minHp}>{curentHp}");
                safetp.UseAbility();
                _safeTpSleeper.Sleep(repka.Cooldown*1000);
            }
        }
    }
}