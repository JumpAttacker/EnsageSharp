using System.Threading.Tasks;
using ArcAnnihilation.Utils;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Objects.UtilityObjects;

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
            var flux = unitBase.Flux;
            var magneticField = unitBase.MagneticField;
            var spark = unitBase.Spark;
            if (!_multiSleeper.Sleeping(flux) && unitBase.AbilityChecker.IsAbilityEnabled(flux.GetAbilityId()) && flux.CanBeCasted() && flux.CanHit(Core.Target))
            {
                flux.UseAbility(Core.Target);
                Printer.Both("Flux usages " + flux.GetAbilityDelay());
                _multiSleeper.Sleep(500, flux);
                await Task.Delay(flux.GetAbilityDelay(), Core.ComboToken.Token);
            }
            var distance = unitBase.Hero.Distance2D(Core.Target);
            if (!_multiSleeper.Sleeping(magneticField) && magneticField != null && unitBase.AbilityChecker.IsAbilityEnabled(magneticField.GetAbilityId()) && magneticField.CanBeCasted() &&
                !unitBase.Hero.HasModifier("modifier_arc_warden_magnetic_field") && distance <= 600 && Core.Target.IsVisible)
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
            }
            if (!_multiSleeper.Sleeping(spark) && spark != null && unitBase.AbilityChecker.IsAbilityEnabled(spark.GetAbilityId()) && spark.CanBeCasted() && !Prediction.IsTurning(Core.Target) && unitBase.Hero.IsVisibleToEnemies)
            {
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