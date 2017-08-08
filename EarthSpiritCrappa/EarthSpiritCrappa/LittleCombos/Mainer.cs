using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Ensage.SDK.Handlers;
using Ensage.SDK.Helpers;
using log4net;
using PlaySharp.Toolkit.Logging;

namespace EarthSpiritCrappa.LittleCombos
{
    public abstract class Mainer
    {
        
        public EarthSpiritCrappa Main { get; }
        protected Mainer(EarthSpiritCrappa main)
        {
            Main = main;
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
        }

        protected abstract Task ExecuteAsync(CancellationToken arg);

        public TaskHandler Handler { get; set; }
    }
}