using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Common.Threading;
using log4net;
using PlaySharp.Toolkit.Logging;

namespace EarthSpiritCrappa.LittleCombos
{
    public class PullCombo : Mainer
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public PullCombo(EarthSpiritCrappa main) : base(main)
        {
            main.Config.PullKey.Item.ValueChanged += (sender, args) =>
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
            var mousePos = Game.MousePosition;
            var myPos = Main.Owner.NetworkPosition;
            var dist = myPos.Distance2D(mousePos);
            if (myPos.Distance2D(mousePos) >= 1100)
            {
                Log.Debug($"out of range. [{dist}]");
                return;
            }
            var push = Main.Grip;
            if (push.CanBeCasted())
            {
                if (!Main.StoneManager.AnyStoneInRange(mousePos, 180f))
                {
                    var remnantCaller = Main.StoneCaller;
                    if (remnantCaller.CanBeCasted())
                    {
                        remnantCaller.UseAbility(mousePos);
                        Log.Debug("Stone not found -> use remnant");
                        await Await.Delay(1, arg);
                    }
                    else
                    {
                        return;
                    }
                }
                push.UseAbility(mousePos);
                Log.Debug("Stone not found -> use grip");
                await Await.Delay(1, arg);
            }
        }
    }
}