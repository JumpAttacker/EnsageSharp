using System.Threading;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.SDK.Extensions;
using Ensage.SDK.Handlers;
using Ensage.SDK.Helpers;

namespace SfAnnihilation.Features
{
    public class ShadowBladeComboWomboFantastic
    {
        private readonly Hero _me;
        private Hero _target;

        public ShadowBladeComboWomboFantastic()
        {
            _me = ObjectManager.LocalHero;
            _ultimate = Ensage.SDK.Extensions.UnitExtensions.GetAbilityById(_me, AbilityId.nevermore_requiem);
            /*UpdateManager.Subscribe(() =>
            {
                if (_target != null)
                    Game.PrintMessage($"Dist-> {_target.Distance2D(_me)}");
            },10);*/
            MenuManager.Menu.Item("sb.Key").ValueChanged += (sender, args) =>
            {
                var newOne = args.GetNewValue<KeyBind>().Active;
                var oldOne = args.GetOldValue<KeyBind>().Active;
                if (newOne != oldOne)
                {
                    if (newOne)
                    {
                        _target = null;
                        Execute();
                    }
                    else
                    {
                        Cancel();
                    }
                }
            };
            
        }
        private async Task ExecuteAsync(CancellationToken arg)
        {
            while (true)
            {
                while (_target == null || !_target.IsValid)
                {
                    _target = TargetSelector.ClosestToMouse(_me);
                    await Task.Delay(10, arg);
                }
                var shadowBlade = _me.GetItemById(AbilityId.item_invis_sword) ?? _me.GetItemById(AbilityId.item_silver_edge);
                if (shadowBlade != null && _ultimate.CanBeCasted())
                {
                    if (shadowBlade.CanBeCasted())
                    {
                        shadowBlade.UseAbility();
                        await Task.Delay(100, arg);
                    }
                }
                else
                {
                    return;
                }

                var distance = _target.Distance2D(_me);
                if (distance > 35 || _target.IsMoving)
                {
                    if (_ultimate.IsInAbilityPhase)
                        _me.Stop();
                    _me.Move(_target.NetworkPosition);
                    await Task.Delay(35, arg);
                }
                else
                {
                    if (!_target.IsMoving)
                    {
                        _me.Move(_target.NetworkPosition);
                        await Task.Delay(150, arg);
                    }
                    if (_ultimate.CanBeCasted())
                    {
                        _ultimate.UseAbility();
                        await Task.Delay(350, arg);
                        if (!_me.IsInRange(_target, MenuManager.InvisRange))
                        {
                            _me.Stop();
                            await Task.Delay(25, arg);
                        }
                    }
                }
                await Task.Delay(1, arg);
            }
            
        }

        private static ShadowBladeComboWomboFantastic _instance;
        private readonly Ability _ultimate;

        public static ShadowBladeComboWomboFantastic Init()
        {
            return _instance ?? (_instance = new ShadowBladeComboWomboFantastic());
        }

        public void Execute()
        {
            if (Handler == null)
            {
                Handler = UpdateManager.Run(ExecuteAsync, false, false);
            }

            Handler.RunAsync();
        }

        protected void Cancel()
        {
            Handler?.Cancel();
            _target = null;
        }

        public TaskHandler Handler { get; set; }
    }
}