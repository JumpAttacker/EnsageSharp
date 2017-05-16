using System.Threading;
using System.Threading.Tasks;
using Ensage.SDK.Handlers;
using Ensage.SDK.Helpers;
using Ensage.SDK.Service;

namespace ArcAnnihilation.Units.behaviour.Orders
{
    public abstract class ComboExecutor : IExecutableAsync
    {
        protected TaskHandler Handler { get; set; }

        public void Execute()
        {
            if (this.Handler == null)
            {
                this.Handler = UpdateManager.Run(this.ExecuteAsync,false);
            }
            this.Handler.RunAsync();
        }

        public abstract Task ExecuteAsync(CancellationToken token = default(CancellationToken));

        public void Cancel()
        {
            this.Handler?.Cancel();
        }

        public bool CanExecute => OrderManager.CanBeExecuted;
    }
}