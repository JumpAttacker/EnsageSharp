using System;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.AbilityInfo;
using Ensage.Common.Extensions;
using Ensage.Common.Objects;
using Ensage.Common.Objects.UtilityObjects;
using Ensage.SDK.Extensions;
using SfAnnihilation.Utils;

namespace SfAnnihilation.Features
{
    internal class RazeAim
    {
        public static readonly Hero Me = ObjectManager.LocalHero;
        public static readonly Sleeper KillStealer = new Sleeper();
        public static readonly Sleeper Updater = new Sleeper();

        static RazeAim()
        {
            Damage = new float[5];
            for (uint i = 1; i < 5; i++)
            {
                Damage[i] = Core.RazeLow.GetAbilitySpecialData("shadowraze_damage", i);
            }
            ExtraDamagePerStack = Core.RazeLow.GetAbilitySpecialData("stack_bonus_damage");
        }

        public static bool WillDie(Hero target)
        {
            var health = target.Health + target.HealthRegeneration;
            var damage = Damage[Core.RazeLow.Level];
            var modifier = target.GetModifierByName(ModifierStackName);
            if (modifier != null)
            {
                damage += modifier.StackCount * ExtraDamagePerStack;
            }
            var extra = Ensage.SDK.Extensions.UnitExtensions.GetAbilityById(Me,
                AbilityId.special_bonus_unique_nevermore_2);
            if (extra?.Level > 0)
            {
                damage += 150;
            }
            damage *= 1 - target.MagicDamageResist;
            return damage > health;
        }

        public static float ExtraDamagePerStack { get; set; }

        public static float[] Damage { get; set; }
        public static string ModifierStackName = "modifier_nevermore_shadowraze_counter";

        public static void OnUpdate(EventArgs args)
        {
            if (!MenuManager.AimIsActive || Updater.Sleeping || MenuManager.ComboIsActive)
                return;
            var target =
                Heroes.GetByTeam(Me.GetEnemyTeam())
                    .FirstOrDefault(
                        x =>
                            x.IsAlive && x.IsVisible && x.IsValidRazeTarget() &&
                            (!MenuManager.AimKillStealOnly || WillDie(x)
                             /*AbilityDamage.CalculateDamage(Core.RazeLow, Core.Me, x) > x.Health + x.HealthRegeneration*/));
            if (target != null)
            {
                if (!KillStealer.Sleeping && Core.Razes.Any(x=>x.CanBeCasted() && x.CanHit(target, x.GetAbilityDelay() + 50,false)) && !Core.Razes.Any(y => y.IsInAbilityPhase))
                {
                    /*var mePos = Me.Position;
                    var targetPos = Prediction.PredictedXYZ(Target, 550);//Target.Position;
                    var angle = Me.FindAngleBetween(targetPos, true);
                    var point = new Vector3(
                        (float)
                            (mePos.X +
                             100 *
                             Math.Cos(angle)),
                        (float)
                            (mePos.Y +
                             100 *
                             Math.Sin(angle)),
                        Target.Position.Z);
                    Me.Move(point);*/
                    Me.Attack(target);
                    KillStealer.Sleep(350);
                }
                if (Helper.RazeAimCaster(Core.RazeLow, target))
                    Updater.Sleep(1000);
                else if (Helper.RazeAimCaster(Core.RazeNormal, target))
                    Updater.Sleep(1000);
                else if (Helper.RazeAimCaster(Core.RazeHigh, target))
                    Updater.Sleep(1000);
            }
        }
    }
}