using System.Threading;
using System.Threading.Tasks;
using Ensage;
using Ensage.SDK.Handlers;
using Ensage.SDK.Helpers;

namespace EarthSpiritCrappa.LittleCombos
{
    public abstract class Mainer
    {
        
        public EarthSpiritCrappa Main { get; }
        public Hero Owner;
        protected Mainer(EarthSpiritCrappa main)
        {
            Main = main;
            Owner = main.Owner;
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