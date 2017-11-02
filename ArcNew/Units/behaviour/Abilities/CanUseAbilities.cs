using System.Linq;
using System.Threading.Tasks;
using ArcAnnihilation.Utils;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Objects.UtilityObjects;
using Ensage.SDK.Helpers;
using UnitExtensions = Ensage.SDK.Extensions.UnitExtensions;

namespace ArcAnnihilation.Units.behaviour.Abilities
{
    class CanUseAbilities : ICanUseAbilties
    {
        private readonly MultiSleeper _multiSleeper;
        public CanUseAbilities()
        {
            _multiSleeper = new MultiSleeper();
        }

        public async Task UseAbilities(UnitBase unitBase)
        {
            if (unitBase.Hero.IsInvisible() || unitBase.Hero.Modifiers.Any(x=>x.Name.Contains("windwalk")))
                return;
            var flux = unitBase.Flux;
            var magneticField = unitBase.MagneticField;
            var spark = unitBase.Spark;
            if (!_multiSleeper.Sleeping(flux) && unitBase.AbilityChecker.IsAbilityEnabled(flux.Id) &&
                flux.CanBeCasted() && flux.CanHit(Core.Target))
            {
                if (Core.Target.IsLinkensProtected() || !MenuManager.SmartFlux || !EntityManager<Unit>.Entities.Any(
                        x =>
                            !x.Equals(Core.Target) && x.Team == Core.Target.Team && x.Name != "npc_dota_thinker" &&
                            x.IsAlive && x.IsVisible &&
                            Ensage.SDK.Extensions.EntityExtensions.Distance2D(x, Core.Target) <= 225))
                {
                    flux.UseAbility(Core.Target);
                    Printer.Both("Flux usages " + flux.GetAbilityDelay());
                    _multiSleeper.Sleep(500, flux);
                    await Task.Delay(flux.GetAbilityDelay(), Core.ComboToken.Token);
                    return;
                }
            }
            var distance = unitBase.Hero.Distance2D(Core.Target);
            if (!_multiSleeper.Sleeping(magneticField) && magneticField != null &&
                unitBase.AbilityChecker.IsAbilityEnabled(magneticField.Id) && magneticField.CanBeCasted() &&
                !unitBase.Hero.HasModifier("modifier_arc_warden_magnetic_field") && distance <= 600 &&
                Core.Target.IsVisible)
            {
                if (!MenuManager.MagneticField && Core.Target.IsMelee)
                {
                    magneticField.UseAbility(Prediction.InFront(unitBase.Hero, -250));
                }
                else
                    magneticField.UseAbility(Prediction.InFront(unitBase.Hero, 250));
                _multiSleeper.Sleep(500, magneticField);
                Printer.Both("MagneticField usages");
                await Task.Delay(magneticField.GetAbilityDelay(), Core.ComboToken.Token);
                return;
            }
            if (!_multiSleeper.Sleeping(spark) && spark != null &&
                unitBase.AbilityChecker.IsAbilityEnabled(spark.Id) && spark.CanBeCasted() &&
                !Prediction.IsTurning(Core.Target) && unitBase.Hero.IsVisibleToEnemies)
            {
                if (UnitExtensions.IsInAttackRange(unitBase.Hero, Core.Target) && MenuManager.SmartSpark)
                {
                    return;
                }
                var delay = spark.GetAbilityDelay();
                var predVector3 = Prediction.PredictedXYZ(Core.Target, 2000 + delay);
                spark.UseAbility(predVector3);
                _multiSleeper.Sleep(500, spark);
                Printer.Both("spark usages");
                await Task.Delay(spark.GetAbilityDelay(), Core.ComboToken.Token);
            }
        }
    }
}