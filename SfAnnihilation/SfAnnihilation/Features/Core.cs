using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Objects.UtilityObjects;
using Ensage.SDK.Orbwalker;
using Ensage.SDK.Service;
using SfAnnihilation.Utils;
//using AbilityId = Ensage.Common.Enums.AbilityId;

namespace SfAnnihilation.Features
{
    /// <summary>
    /// Default Combo
    /// </summary>
    internal class Core
    {
        public static readonly Hero Me = ObjectManager.LocalHero;
        public static readonly Ability RazeLow = Me.GetAbilityById(AbilityId.nevermore_shadowraze1);
        public static readonly Ability RazeNormal = Me.GetAbilityById(AbilityId.nevermore_shadowraze2);
        public static readonly Ability RazeHigh = Me.GetAbilityById(AbilityId.nevermore_shadowraze3);
        public static readonly List<Ability> Razes = new List<Ability> {RazeLow, RazeNormal, RazeHigh};
        public static Hero Target;
        //private static readonly Orbwalker Orbwalker = new Orbwalker(Me);
        private static readonly IOrbwalkerManager Orbwalker;
        private static readonly Sleeper MoveSleeper = new Sleeper();
        public static readonly MultiSleeper RazeCanceler = new MultiSleeper();

        static Core()
        {
            var context = new EnsageServiceContext(ObjectManager.LocalHero);
            context.TargetSelector.Activate();
            var orbwalker = context.GetExport<IOrbwalkerManager>().Value;
            orbwalker.Activate();
            Orbwalker = orbwalker;
            context.TargetSelector.Deactivate();
        }

        public static void OnUpdate(EventArgs args)
        {
            if (!MenuManager.ComboIsActive)
            {
                // ReSharper disable once RedundantCheckBeforeAssignment
                if (Target != null)
                    Target = null;
                return;
            }
            if (Target == null || !Target.IsValid || !Target.IsAlive)
            {
                Target = TargetSelector.ClosestToMouse(Me);
                return;
            }
            if (MenuManager.UseRazeInCombo && Target.IsValidRazeTarget() && !Me.IsInvisible() &&
                (!Orbwalker.CanAttack(Target) || Me.GetAttackRange() <= Me.Distance2D(Target)) && !RazeCancelSystem.IsValid)
            {
                var r = Razes.OrderBy(x => Target.Distance2D(Prediction.InFront(Me, x.GetCastRange())));
                foreach (var ability in r)
                {
                    
                    var razeStatus = Helper.RazeAimCasterTemp(ability, Target);
                    //var razeStatus = Helper.RazeCaster(ability, Target);
                    if (razeStatus)
                        break;
                }
                /*if (!Helper.RazeCaster(RazeLow, Target))
                    if (!Helper.RazeCaster(RazeNormal, Target))
                        Helper.RazeCaster(RazeHigh, Target);*/
            }
            if (!Target.HasModifier("modifier_abaddon_borrowed_time") && Me.CanAttack() && Target.IsVisible)
                Orbwalker.OrbwalkTo(Target);
            else if (!MoveSleeper.Sleeping)
            {
                MoveSleeper.Sleep(250);
                Me.Move(Game.MousePosition);
            }
        }
    }
}