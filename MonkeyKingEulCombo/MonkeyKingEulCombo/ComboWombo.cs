using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.SDK.Abilities.Items;
using Ensage.SDK.Helpers;
using Ensage.SDK.Orbwalker.Modes;
using Ensage.SDK.Service;
using log4net;
using PlaySharp.Toolkit.Logging;
using SharpDX;

namespace MonkeyKingEulCombo
{
    public class ComboWombo : KeyPressOrbwalkingModeAsync
    {
        private readonly MonkeyKingEulCombo _main;
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public Hero Target;
        public Ability Ultimate;
        public Ability Stun;
        public item_cyclone Eul => _main.Eul;
        public ComboWombo(IServiceContext context, Key key, MonkeyKingEulCombo monkeyKingEulCombo) : base(context, key)
        {
            _main = monkeyKingEulCombo;
            Ultimate = context.Owner.GetAbilityById(AbilityId.monkey_king_wukongs_command);
            Stun = context.Owner.GetAbilityById(AbilityId.monkey_king_boundless_strike);
        }

        public override async Task ExecuteAsync(CancellationToken token)
        {
            if (Target == null || !Target.IsValid || !Target.IsAlive)
            {
                if (!Context.TargetSelector.IsActive)
                    Context.TargetSelector.Activate();
                Target = Context.TargetSelector.Active.GetTargets().FirstOrDefault() as Hero;
                if (Target != null)
                {
                    Log.Info("target detected");
                    Context.Particle.DrawRange(Target, "Mk_target", 125, Color.YellowGreen);
                    if (Context.TargetSelector.IsActive)
                        Context.TargetSelector.Deactivate();
                }
                else
                {
                    Log.Info("Cant find target");
                }
            }

            if (Target != null && Target.IsVisible)
            {
                if (Eul != null)
                {
                    if (Eul.CanBeCasted && Eul.CanHit(Target))
                    {
                        Eul.UseAbility(Target);
                        await Task.Delay(Eul.GetHitTime(Target), token);
                        if (Ultimate.CanBeCasted())
                        {
                            Ultimate.UseAbility(Target.Position);
                            await Task.Delay(530, token);
                        }

                        var modifier = Target.FindModifier("modifier_eul_cyclone");
                        if (modifier != null)
                        {
                            var time = modifier.RemainingTime;
                            const double timing = 0.4;
                            if (time <= timing + Game.Ping / 1000)
                            {
                                Stun.UseAbility(Target.Position);
                            }
                            else
                            {
                                var timeForCast = timing + Config.ExtraTime / 100f + Game.Ping / 1000;
                                var delayTime = (int)((time - timeForCast) * 1000);
                                Log.Warn($"delay time: {delayTime} rem time: {time} Time for cast: {timeForCast}");
                                await Task.Delay(Math.Max(delayTime, 1), token);
                                Log.Debug("after delay -> try to use ability");
                                Stun.UseAbility(Target.Position);
                            }
                        }
                    }
                }
            }
            Orbwalker.OrbwalkTo(Target);
        }

        public void Load()
        {
            UpdateManager.Subscribe(TargetUpdater, 25);
            UpdateManager.Subscribe(IndicatorUpdater);
        }
        private void IndicatorUpdater()
        {
            if (Target != null)
                Context.Particle.DrawDangerLine(Owner, "Line_target", Target.Position);
        }

        public void Unload()
        {
            UpdateManager.Unsubscribe(TargetUpdater);
            UpdateManager.Unsubscribe(IndicatorUpdater);
        }

        private void TargetUpdater()
        {
            if (!CanExecute && Target != null)
            {
                if (!Context.TargetSelector.IsActive)
                {
                    Context.TargetSelector.Activate();
                    Log.Info("re Enable _targetManager");
                }
                Cancel();
                Target = null;
                Context.Particle.Remove("Mk_target");
                Context.Particle.Remove("Line_target");
            }
        }

        public void UpdateConfig(Config config)
        {
            Config = config;
        }

        public Config Config { get; set; }
    }
}