using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Common.Threading;
using Ensage.SDK.Extensions;
using log4net;
using PlaySharp.Toolkit.Logging;

namespace EarthSpiritCrappa.LittleCombos
{
    public class RollingCombo : Mainer
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public RollingCombo(EarthSpiritCrappa main) : base(main)
        {
            main.Config.RollingKey.Item.ValueChanged += (sender, args) =>
            {
                var newOne = args.GetNewValue<KeyBind>().Active;
                var oldOne = args.GetOldValue<KeyBind>().Active;
                if (newOne != oldOne && newOne)
                {
                    Execute();
                }
            };
        }

        protected override async Task ExecuteAsync(CancellationToken arg)
        {
            var mousePos = Game.MousePosition;
            var myPos = Main.Owner.NetworkPosition;
            var dist = myPos.Distance2D(mousePos);
            if (myPos.Distance2D(mousePos) >= 3000)
            {
                Log.Debug($"out of range. [{dist}]");
                return;
            }
            var ability = Main.Boulder;
            if (ability.CanBeCasted())
            {
                if (!Main.StoneManager.AnyStoneInRange(myPos, 160f))
                {
                    var remnantCaller = Main.StoneCaller;
                    if (remnantCaller.CanBeCasted())
                    {
                        if (Main.Owner.IsMoving)
                        {
                            Main.Owner.Stop();
                            await Task.Delay(1, arg);
                            myPos = Main.Owner.NetworkPosition;
                        }
                        var pos = (myPos - mousePos).Normalized();
                        pos *= 100;
                        pos = myPos - pos;
                        remnantCaller.UseAbility(pos);
                        Log.Debug("Stone not found -> use remnant");
                        await Await.Delay(1, arg);
                    }
                }
                ability.UseAbility(mousePos);
                Log.Debug("use roll");
                await Await.Delay(1, arg);
            }
        }
    }
}