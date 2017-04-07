using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Enums;
using Ensage.Common.Extensions;
using Ensage.Common.Objects.UtilityObjects;
using SfAnnihilation.Features;
using SharpDX;
using AbilityId = Ensage.Common.Enums.AbilityId;

namespace SfAnnihilation.Utils
{
    internal static class Helper
    {
        public static string PrintVector(this Vector3 vec)
        {
            return $"({vec.X};{vec.Y};{vec.Z})";
        }
        public static string PrintVector(this Vector2 vec)
        {
            return $"({vec.X};{vec.Y})";
        }
        public static int GetAbilityDelay(this Ability ability, Unit target)
        {
            return (int)((ability.FindCastPoint() + Core.Me.GetTurnTime(target)) * 1000.0 + Game.Ping);
        }
        public static int GetAbilityDelay(this Ability ability)
        {
            return (int)((ability.FindCastPoint()) * 1000.0 + Game.Ping);
        }

        public static int GetAbilityDelay(this Ability ability, Vector3 targetPosition)
        {
            return (int)((ability.FindCastPoint() + Core.Me.GetTurnTime(targetPosition)) * 1000.0 + Game.Ping);
        }

        /// <summary>
        /// Razers Extensions
        /// </summary>
        /// <param name="ability"></param>
        /// <param name="target"></param>
        /// <param name="usePrediction"></param>
        /// <param name="checkForFace"></param>
        /// <returns></returns>
        public static bool CanHit(this Ability ability, Hero target, bool usePrediction = false, bool checkForFace = true)
        {
            var aId = ability.GetAbilityId();
            if (aId >= AbilityId.nevermore_shadowraze1 && aId <= AbilityId.nevermore_shadowraze3)
            {
                var radius = ability.GetRadius();
                var range = ability.GetCastRange();
                if (checkForFace)
                {
                    var predFontPos = Prediction.InFront(Core.Me, range);
                    var pred = ability.GetPrediction(target);
                    //var inRange = (usePrediction ? pred.Distance2D(predFontPos) : target.Distance2D(predFontPos)) <= radius + target.HullRadius;
                    var inRange = target.Distance2D(predFontPos) <= radius + target.HullRadius;

                    return inRange;
                }
                else
                {
                    var dist = target.Distance2D(Core.Me);
                    var inRange = dist <= range + target.HullRadius + radius &&
                                  dist >= range - target.HullRadius - radius;
                    return inRange;
                }
            }
            return AbilityExtensions.CanHit(ability, target);
        }
        /// <summary>
        /// with custom delay
        /// </summary>
        /// <param name="ability"></param>
        /// <param name="target"></param>
        /// <param name="customDelay"></param>
        /// <returns></returns>
        public static bool CanHit(this Ability ability, Hero target, float customDelay)
        {
            var aId = ability.GetAbilityId();
            if (aId >= AbilityId.nevermore_shadowraze1 && aId <= AbilityId.nevermore_shadowraze3)
            {
                var radius = ability.GetRadius();
                var range = ability.GetCastRange();
                var predFontPos = Prediction.InFront(Core.Me, range);
                var pred = Prediction.PredictedXYZ(target, customDelay);
                //var inRange = (usePrediction ? pred.Distance2D(predFontPos) : target.Distance2D(predFontPos)) <= radius + target.HullRadius;
                var inRange = pred.Distance2D(predFontPos) <= radius + target.HullRadius;

                return inRange;
            }
            return AbilityExtensions.CanHit(ability, target);
        }

        public static bool IsValidRazeTarget(this Hero target)
        {
            return target.IsVisible && !target.IsMagicImmune() && Core.Me.CanCast() && !target.HasModifiers(
                new[]
                {
                    "modifier_templar_assassin_refraction_absorb",
                    "modifier_templar_assassin_refraction_absorb_stacks",
                    "modifier_oracle_fates_edict",
                    "modifier_abaddon_borrowed_time"
                }, false);
        }
        private static readonly MultiSleeper StopSleeper=new MultiSleeper();
        public static bool RazeCaster(Ability raze, Hero target, bool checkForAngle = true)
        {
            if (!raze.CanBeCasted()) return false;
            if (Core.RazeCanceler.Sleeping(raze) || raze.IsInAbilityPhase)
            {
                if (raze.CanHit(target)) return true;
                if (StopSleeper.Sleeping(raze))
                    return true;
                Core.Me.Stop();
                Core.RazeCanceler.Reset(raze);
                StopSleeper.Sleep(Game.Ping+10, raze);
                Printer.Print($"stop: [sl: {Core.RazeCanceler.Sleeping(raze)}] [ph: {raze.IsInAbilityPhase}]");
            }
            else if (raze.CanHit(target,true, checkForAngle))
            {
                if (Core.Razes.Any(x => x.IsInAbilityPhase))
                    return false;
                raze.UseAbility();
                //StopSystem.New(raze, target);
                Core.RazeCanceler.Sleep(raze.GetAbilityDelay()+50, raze);
                Printer.Print($"cast: [{raze.Name}]->{raze.GetAbilityDelay() + 50}");
                return true;
            }
            return false;
        }
        public static bool RazeAimCasterTemp(Ability raze, Hero target, bool checkForAngle = true)
        {
            if (!raze.CanBeCasted()) return false;
            if (raze.IsInAbilityPhase) return true;
            if (!raze.CanHit(target, raze.GetAbilityDelay())) return false;
            raze.UseAbility();
            RazeCancelSystem.New(raze, target);
            return true;
        }
        public static bool RazeAimCaster(Ability raze, Hero target, bool checkForAngle = true)
        {
            if (!raze.CanBeCasted()) return false;
            if (raze.IsInAbilityPhase) return true;
            if (!raze.CanHit(target, true, checkForAngle)) return false;
            if (RazeCancelSystem.New(raze, target))
            {
                raze.UseAbility();
            }
            return true;

        }
        public static bool HasItem(this Unit unit, ItemId classId)
        {
            return unit.Inventory.Items.Any(item => item.GetItemId() == classId);
        }
    }
}