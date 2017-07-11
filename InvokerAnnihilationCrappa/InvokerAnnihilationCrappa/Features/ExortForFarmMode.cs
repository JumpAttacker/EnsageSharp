using System;
using System.Linq;
using System.Reflection;
using Ensage.Common.Extensions;
using Ensage.SDK.Helpers;
using Ensage.SDK.Menu;
using Ensage.SDK.Orbwalker;
using Ensage.SDK.Orbwalker.Metadata;
using log4net;
using PlaySharp.Toolkit.Logging;

namespace InvokerAnnihilationCrappa.Features
{
    public class ExortForFarmMode
    {
        private readonly Config _main;
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public ExortForFarmMode(Config main)
        {
            _main = main;
            var panel = main.Factory.Menu("Exort for Orbwalk-farm mode");
            Enable = panel.Item("cast 3x exort when farm mode is activated", true);

            if (Enable)
            {
                UpdateManager.Subscribe(Tost, 100);
            }

            Enable.Item.ValueChanged += (sender, args) =>
            {
                if (args.GetNewValue<bool>())
                    UpdateManager.Subscribe(Tost, 100);
                else
                    UpdateManager.Unsubscribe(Tost);
            };
            FarmMode = _main.Invoker.OrbwalkerManager.Value.OrbwalkingModes.First(x => x.Value.ToString() == "Ensage.SDK.Orbwalker.Modes.Farm");
            if (FarmMode == null)
                Log.Error("cant find Ensage.SDK.Orbwalker.Modes.Farm");
        }

        public Lazy<IOrbwalkingMode, IOrbwalkingModeMetadata> FarmMode { get; set; }


        private void Tost()
        {
            if (FarmMode == null || !_main.Invoker.Owner.CanCast())
                return;
            var value = FarmMode.Value;
            if (value.CanExecute)
            {
                var eCount = _main.Invoker.SpCounter.E == 3;
                if (!eCount)
                {
                    var exort = _main.Invoker.Exort;
                    if (exort.Level > 0)
                    {
                        exort.UseAbility();
                        exort.UseAbility();
                        exort.UseAbility();
                        Log.Debug("cast exort x3 for orbwalking-farm mode");
                    }
                }
            }
        }

        public void OnDeactivate()
        {
            if (Enable)
            {
                UpdateManager.Unsubscribe(Tost);
            }
        }

        public MenuItem<bool> Enable { get; set; }
    }
}