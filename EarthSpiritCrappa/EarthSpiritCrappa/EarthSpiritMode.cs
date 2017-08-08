using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Ensage;
using Ensage.Common.Threading;
using Ensage.SDK.Extensions;
using Ensage.SDK.Helpers;
using Ensage.SDK.Orbwalker.Modes;
using Ensage.SDK.Service;
using log4net;
using PlaySharp.Toolkit.Logging;
using SharpDX;
using AbilityExtensions = Ensage.Common.Extensions.AbilityExtensions;
using UnitExtensions = Ensage.Common.Extensions.UnitExtensions;

namespace EarthSpiritCrappa
{
    public class EarthSpiritMode : KeyPressOrbwalkingModeAsync
    {
        public EarthSpiritCrappa Main { get; }
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private Unit _target;
        private int GetDelay => (int) Game.Ping + 50;
        public EarthSpiritMode(IServiceContext context, Key key, EarthSpiritCrappa main) : base(context, key)
        {
            Main = main;
        }

        public override async Task ExecuteAsync(CancellationToken token)
        {
            if (_target == null || !_target.IsValid || !_target.IsAlive)
            {
                if (!Main.Context.TargetSelector.IsActive)
                    Main.Context.TargetSelector.Activate();
                _target = Main.Context.TargetSelector.Active.GetTargets().FirstOrDefault() as Hero;
                if (_target != null)
                {
                    Log.Info("target detected");
                    if (Main.Context.TargetSelector.IsActive)
                        Main.Context.TargetSelector.Deactivate();
                }
                else
                {
                    Log.Info("Cant find target");
                }
            }
            if (_target != null)
            {
                if (Owner.CanCastAbilities())
                {
                    var grip = Main.Grip;
                    var roll = Main.Boulder;
                    var push = Main.Smash;
                    var ultimate = Main.Magnetize;
                    Unit stone = null;
                    if (IsAbilityEnable(push) && AbilityExtensions.CanBeCasted(push) &&
                        AbilityExtensions.CanHit(push, _target))
                    {
                        stone = Main.StoneManager.FindStone(Owner.NetworkPosition, 160f);
                        if (stone == null)
                        {
                            /*var turnTime = UnitExtensions.GetTurnTime(Owner,
                                _target.NetworkPosition) * 1000 + Game.Ping;*/
                            await CreateStone(_target.NetworkPosition, token);
                            //Game.PrintMessage($"{turnTime}");
                            //await Await.Delay((int) turnTime, token);
                            //await Await.Delay(GetDelay, token);
                            stone = Main.StoneManager.FindStone(Owner.NetworkPosition, 160f);
                            Log.Info($"({GetDelay}) stone -> {stone != null}");
                        }
                        if (stone != null)
                        {
                            if (Main.Config.EnablePrediction)
                            {
                                if (!AbilityExtensions.CastSkillShot(push, _target))
                                {
                                    Log.Error("Smash can't be casted due cant hit with prediction");
                                    return;
                                }
                            }
                            else
                            {
                                push.UseAbility(_target.NetworkPosition);
                                Log.Info("Push");
                            }
                            await Await.Delay(GetDelay, token);
                        }
                    }

                    if (IsAbilityEnable(grip) && AbilityExtensions.CanBeCasted(grip) &&
                        AbilityExtensions.CanHit(grip, _target))
                    {
                        if (stone != null)
                        {
                            await StoneTracker(stone, token);
                            stone = Main.StoneManager.FindStone(_target.NetworkPosition, 180);
                            if (stone == null)
                            {
                                Log.Info("Stone == null (1)");
                                await CreateStone(_target.NetworkPosition, token);
                                stone = Main.StoneManager.FindStone(_target.NetworkPosition, 180);
                                Log.Info($"Stone -> {stone == null} (1)");
                            }
                            Log.Info("Grip");
                            grip.UseAbility(stone == null ? _target.NetworkPosition : stone.NetworkPosition);
                            await Await.Delay(GetDelay, token);
                        }
                        else
                        {
                            Log.Error("stone == null");
                            var created = await CreateStoneOnTarget(_target.NetworkPosition, token);
                            if (created)
                            {
                                grip.UseAbility(_target.NetworkPosition);
                                Log.Info("grip");
                            }
                            else
                            {
                                Log.Info("cant create new stone for grip");
                            }
                        }
                    }

                    if (IsAbilityEnable(roll) && AbilityExtensions.CanBeCasted(roll) &&
                        AbilityExtensions.CanHit(roll, _target))
                    {
                        if (stone == null)
                        {
                            if (!Main.StoneManager.AnyStoneInRange(Owner.NetworkPosition, 180))
                            {
                                await CreateStone(_target.NetworkPosition, token);
                                roll.UseAbility(_target.NetworkPosition);
                                Log.Info("new stone for rolling");
                            }
                        }
                        roll.UseAbility(_target.NetworkPosition);
                        Log.Info("roll");

                        await Await.Delay(GetDelay, token);
                    }

                    if (IsAbilityEnable(ultimate) && AbilityExtensions.CanBeCasted(ultimate) &&
                        AbilityExtensions.CanHit(ultimate, _target))
                    {
                        ultimate.UseAbility();
                        Log.Info("ultimate");
                        await Await.Delay(GetDelay, token);
                    }
                }
                if (UnitExtensions.CanUseItems(Owner))
                {
                    await UseItems(token);

                }
                if (_target.IsAttackImmune() || _target.IsInvulnerable())
                    Orbwalker.Move(Game.MousePosition);
                else
                    Orbwalker.OrbwalkTo(_target);
            }
            else
            {
                Orbwalker.Move(Game.MousePosition);
            }
        }

        private async Task UseItems(CancellationToken token)
        {
            var normalState = CheckForState(_target.UnitState);
            var blink = Main.Blink;
            var dist = _target.Distance2D(Owner);
            if (blink != null && IsItemEnable(blink.Item) && dist <= blink.CastRange && dist >= 350)
            {
                blink.UseAbility(_target.Position);
                await Task.Delay(blink.GetCastDelay(), token);
            }

            var hex = Main.Hex;
            if (hex != null && IsItemEnable(hex.Item) && normalState && hex.CanHit(_target))
            {
                hex.UseAbility(_target);
                await Task.Delay(hex.GetCastDelay(), token);
            }

            var eul = Main.Eul;
            if (eul != null && IsItemEnable(eul.Item) && normalState && eul.CanHit(_target))
            {
                eul.UseAbility(_target);
                await Task.Delay(eul.GetCastDelay(), token);
            }

            var shiva = Main.Shivas;
            if (shiva != null && IsItemEnable(shiva.Item) && _target.IsInRange(Owner,700))
            {
                shiva.UseAbility();
                await Task.Delay(shiva.GetCastDelay(), token);
            }

            var lotus = Main.Lotus;
            if (lotus != null && IsItemEnable(lotus.Item) && lotus.CanHit(_target))
            {
                lotus.UseAbility(Owner);
                await Task.Delay(lotus.GetCastDelay(), token);
            }

            var veil = Main.Veil;
            if (veil != null && IsItemEnable(veil.Item) && veil.CanHit(_target))
            {
                veil.UseAbility(_target.Position);
                await Task.Delay(veil.GetCastDelay(), token);
            }

            var halbert = Main.Halbert;
            if (halbert != null && IsItemEnable(halbert.Item) && halbert.CanHit(_target))
            {
                halbert.UseAbility(_target);
                await Task.Delay(halbert.GetCastDelay(), token);
            }
        }

        private bool IsAbilityEnable(Ability ability)
        {
            return Main.Config.AbilitiesInCombo.Value.IsEnabled(ability.Id.ToString());
        }
        private bool IsItemEnable(Item ability)
        {
            return Main.Config.ItemsInCombo.Value.IsEnabled(ability.Id.ToString());
        }

        private async Task StoneTracker(Unit stone, CancellationToken token)
        {
            var startDistance = stone.Distance2D(_target);
            var startTime = Game.RawGameTime;
            while (true)
            {
                if (_target == null || !_target.IsValid || stone == null || !stone.IsValid)
                {
                    Log.Info($"SHIIIT => {_target == null} | {!_target?.IsValid} | {stone == null} | {!stone?.IsValid}");
                    return;
                }
                var dist = _target.Distance2D(stone.NetworkPosition);
                if (dist <= 170)
                {
                    Log.Info($"dist -> {dist}");
                    return;
                }
                var time = Game.RawGameTime - startTime >= 3.5f;
                if (time)
                {
                    Log.Info($"time -> {Game.RawGameTime} - {startTime}");
                    return;
                }
                if (_target.Distance2D(stone.NetworkPosition) - startDistance > 300)
                {
                    Log.Info("too far away");
                    return;
                }
                await Task.Delay(1, token);
            }
        }

        public bool CheckForState(UnitState state) => (state & UnitState.Hexed) == 0;

        protected override void OnActivate()
        {
            UpdateManager.Subscribe(TargetUpdater, 25);
            UpdateManager.Subscribe(IndicatorUpdater);
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            UpdateManager.Unsubscribe(TargetUpdater);
            UpdateManager.Unsubscribe(IndicatorUpdater);
            base.OnDeactivate();
        }

        private void IndicatorUpdater()
        {
            if (_target != null)
            {
                Main.Context.Particle.DrawDangerLine(Owner, "target_line", _target.Position);
                Main.Context.Particle.DrawRange(_target, "target_range", 75, Color.YellowGreen);
            }
        }

        private void TargetUpdater()
        {
            if (!CanExecute && _target != null)
            {
                if (!Main.Context.TargetSelector.IsActive)
                {
                    Main.Context.TargetSelector.Activate();
                    Log.Info("re Enable _targetManager");
                }
                _target = null;
                Main.Context.Particle.Remove("target_line");
                Main.Context.Particle.Remove("target_range");
            }
            /*else if (CanExecute && _target != null)
            {
                if (_target.IsAttackImmune() || _target.IsInvulnerable())
                    Orbwalker.Move(Game.MousePosition);
                else
                    Orbwalker.OrbwalkTo(_target);
            }*/
        }

        public async Task<bool> CreateStone(Vector3 targetPos, CancellationToken arg)
        {
            var myPos = Owner.NetworkPosition;
            var remnantCaller = Main.StoneCaller;
            if (IsAbilityEnable(remnantCaller) && AbilityExtensions.CanBeCasted(remnantCaller))
            {
                if (Main.Owner.IsMoving)
                {
                    Main.Owner.Stop();
                    await Task.Delay(1, arg);
                    myPos = Main.Owner.NetworkPosition;
                }
                var pos = (myPos - targetPos).Normalized();
                pos *= 100;
                pos = myPos - pos;
                remnantCaller.UseAbility(pos);
                await Await.Delay(GetDelay, arg);
                Main.Owner.Stop(true);
                var extra = UnitExtensions.GetTurnTime(Owner, _target.NetworkPosition) * 1000;
                await Await.Delay((int) (GetDelay + extra), arg);
                return true;
            }
            return false;
        }
        public async Task<bool> CreateStoneOnTarget(Vector3 targetPos, CancellationToken arg)
        {
            var remnantCaller = Main.StoneCaller;
            if (AbilityExtensions.CanBeCasted(remnantCaller))
            {
                remnantCaller.UseAbility(targetPos);
                await Await.Delay(1, arg);
                Main.Owner.Stop();
                return true;
            }
            return false;
        }
    }
}