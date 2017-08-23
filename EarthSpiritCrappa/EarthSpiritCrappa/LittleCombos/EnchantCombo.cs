using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.SDK.Extensions;
using log4net;
using PlaySharp.Toolkit.Logging;

namespace EarthSpiritCrappa.LittleCombos
{
    public class EnchantCombo : Mainer
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public EnchantCombo(EarthSpiritCrappa main) : base(main)
        {
            main.Config.EnchantKey.Item.ValueChanged += (sender, args) =>
            {
                var newOne = args.GetNewValue<KeyBind>().Active;
                var oldOne = args.GetOldValue<KeyBind>().Active;
                if (newOne != oldOne && newOne)
                {
                    Cancel();
                    Execute();
                }
            };
        }

        protected override async Task ExecuteAsync(CancellationToken arg)
        {
            var push = Main.Smash;
            var grip = Main.Grip;
            var enchant = Main.EnchantRemnant;
            var target = Main.Context?.TargetSelector?.Active?.GetTargets().FirstOrDefault();
            while (Main.Config.EnchantKey.Value.Active)
            {
                if (target == null || !target.IsAlive)
                {
                    target = Main.Context?.TargetSelector?.Active?.GetTargets().FirstOrDefault();
                    await Task.Delay(1, arg);
                    continue;
                }

                if (enchant.CanBeCasted())
                {
                    if (enchant.CanHit(target))
                    {
                        enchant.UseAbility(target);
                        var delay = enchant.GetCastDelay(Owner, target, true);
                        Log.Error($"delay (enchant) -> {delay}");
                        await Task.Delay((int) delay+100, arg);
                    }
                    else
                    {
                        if (Main.Blink != null && Main.Blink.CanBeCasted)
                        {
                            Log.Info($"use blink {Main.Blink.GetCastDelay(target)}");
                            Main.Blink.UseAbility(target.Position);
                            await Task.Delay(Main.Blink.GetCastDelay(target), arg);
                        }
                        else
                        {
                            Log.Info($"move");
                            Owner.Move(target.Position);
                        }
                        await Task.Delay(50, arg);
                        continue;
                    }
                }

                if (Ensage.SDK.Extensions.UnitExtensions.HasModifier(target,"modifier_earthspirit_petrify"))
                {
                    var lens = Main.Context.Inventory.Items.Any(x => x.Id == AbilityId.item_aether_lens);
                    var pushRange = 200 + (lens ? 220 : 0);
                    if (grip.CanBeCasted() && !target.IsInRange(Owner, pushRange))
                    {
                        if (grip.CanHit(target))
                        {
                            grip.UseAbility(target.NetworkPosition);
                            var delay = grip.GetCastDelay(Owner, target, true);
                            Log.Error($"delay (grip) -> {delay} {grip.Name}");
                            await Task.Delay((int) delay, arg);
                        }
                    }

                    if (push.CanBeCasted())
                    {
                        if (target.IsInRange(Owner, pushRange))
                        {
                            push.UseAbility(Game.MousePosition);
                            var delay = push.GetCastDelay(Owner, target, true);
                            Log.Error($"delay (push) -> {delay} {push.Name}");
                            await Task.Delay((int) delay, arg);
                            Cancel();
                        }
                    }
                }
                else
                {
                    Log.Error("skip cuz not find modifier");
                }

                await Task.Delay(1, arg);
            }
        }
    }
}