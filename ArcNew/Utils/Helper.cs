using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common;
using Ensage.Common.Enums;
using Ensage.Common.Extensions;
using Ensage.SDK.Helpers;
using SharpDX;
using UnitExtensions = Ensage.SDK.Extensions.UnitExtensions;

namespace ArcAnnihilation.Utils
{
    internal static class Helper
    {
        public static string PrintVector(this Vector3 vec)
        {
            return $"new Vector3({(int)vec.X},{(int)vec.Y},{(int)vec.Z}),";
            
        }
        public static string PrintVector(this Vector2 vec)
        {
            return $"({vec.X};{vec.Y})";
        }
        public static int GetAbilityDelay(this Ability ability, Unit target)
        {
            return (int)((ability.FindCastPoint() + Core.MainHero.Hero.GetTurnTime(target)) * 1000.0 + (Math.Abs(Game.Ping) < 5 ? 50 : Game.Ping));
        }
        public static int GetAbilityDelay(this Ability ability)
        {
            return (int) ((ability.FindCastPoint()) * 1000.0 + (Math.Abs(Game.Ping) < 5 ? 50 : Game.Ping));
        }

        public static int GetAbilityDelay(this Ability ability, Vector3 targetPosition)
        {
            return (int)((ability.FindCastPoint() + Core.MainHero.Hero.GetTurnTime(targetPosition)) * 1000.0 + (Math.Abs(Game.Ping) < 5 ? 50 : Game.Ping));
        }

        public static bool HasItem(this Unit unit, Ensage.AbilityId classId)
        {
            return unit.Inventory.Items.Any(item => item.Id == classId);
        }
        public static bool HasAbility(this Unit unit, Ensage.AbilityId classId)
        {
            return unit.Spellbook.Spells.Any(item => item.Id == classId);
        }

        public static bool CanDie(this Hero hero,bool checkForAegis=false)
        {
            var mod = !hero.HasModifiers(
                new[]
                {
                    "modifier_dazzle_shallow_grave", "modifier_oracle_false_promise",
                    "modifier_skeleton_king_reincarnation_scepter_active", "modifier_abaddon_borrowed_time"
                },
                false) &&
                      (hero.ClassId != ClassId.CDOTA_Unit_Hero_Abaddon ||
                       !hero.GetAbilityById(Ensage.AbilityId.abaddon_borrowed_time).CanBeCasted());
            if (checkForAegis)
                return mod && !hero.HasItem(Ensage.AbilityId.item_aegis);
            return mod;
        }

        public static async Task TargetFinder(CancellationToken cancellationToken)
        {
            while (Core.Target == null || !Core.Target.IsValid || !Core.Target.IsAlive)
            {
                Core.Target = TargetSelector.ClosestToMouse(Core.MainHero.Hero, 500);
                Printer.Both($"[TargetFinder] new target: {Core.Target.Name} | {Core.Target.Handle}");
                await Task.Delay(100, cancellationToken);
            }
        }
        public static bool TargetFinder()
        {
            if (Core.Target != null && Core.Target.IsValid && Core.Target.IsAlive)
            {
                if (Core.Target.ClassId == ClassId.CDOTA_Unit_Hero_Phoenix)
                {
                    if (!Core.Target.IsVisible)
                    {
                        var egg =
                            EntityManager<Unit>.Entities.FirstOrDefault(
                                x =>
                                    UnitExtensions.IsInRange(x, Core.Target, 150) && x.Name == "npc_dota_phoenix_sun" &&
                                    x.IsAlive && x.Team == Core.MainHero.Hero.GetEnemyTeam());
                        if (egg != null)
                        {
                            Core.Target = egg;
                            Printer.Both(
                                $"[TargetFinder] new target: phoenix to egg {Core.Target.Name} | {Core.Target.Handle}");
                        }
                    }
                }
                return true;
            }
            //modifier_morphling_replicate
            var mousePos = Game.MousePosition;
            Core.Target = EntityManager<Hero>.Entities.Where(x =>
                    x.Team != Core.MainHero.Hero.Team && x.IsAlive && x.IsVisible && x.Distance2D(mousePos) <= 500 &&
                    (!x.IsIllusion || x.HasModifier("modifier_morphling_replicate")))
                .OrderBy(x => x.Distance2D(mousePos)).FirstOrDefault();
            //TargetSelector.ClosestToMouse(Core.MainHero.Hero, 500);
            var tempTarget =
                EntityManager<Unit>.Entities.FirstOrDefault(
                    x =>
                        (x.NetworkName == "CDOTA_Unit_SpiritBear" || x.Name == "npc_dota_phoenix_sun") && x.IsAlive &&
                        x.Team == Core.MainHero.Hero.GetEnemyTeam() && x.Distance2D(mousePos) <= 500);
            if (Core.Target != null && tempTarget != null)
            {
                if (Core.Target.Distance2D(mousePos) > tempTarget.Distance2D(mousePos))
                    Core.Target = tempTarget;
            }
            else if (Core.Target == null && tempTarget != null)
                Core.Target = tempTarget;
            if (Core.Target == null) return false;
            Printer.Both($"[TargetFinder] new target: {Core.Target.Name} | {Core.Target.Handle}");
            return true;
        }

        /// <summary>
        ///     Waits until the target has a certain modifier.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="name">Name of the modifier.</param>
        /// <param name="time">Time out</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<bool> WaitGainModifierAsync(
            this Unit target,
            string name,
            float time,
            CancellationToken ct = default(CancellationToken))
        {
            var startTime = Game.RawGameTime;
            try
            {
                while (!target.HasModifier(name))
                {
                    await Task.Delay(100, ct);
                    if (startTime + time - Game.RawGameTime <= 0)
                    {
                        return false;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                return false;
            }

            return true;
        }

        public static bool IsDagon(this Item item)
        {
            var id = item.GetItemId();
            return id == ItemId.item_dagon || id == ItemId.item_dagon_2 || id == ItemId.item_dagon_3 || id == ItemId.item_dagon_4 || id == ItemId.item_dagon_5;
        }
    }
}