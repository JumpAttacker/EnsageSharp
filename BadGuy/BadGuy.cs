using System.ComponentModel.Composition;
using Ensage;
using Ensage.SDK.Service;
using Ensage.SDK.Service.Metadata;
using Config = BadGuy.Configs.Config;

namespace BadGuy
{
    [ExportPlugin("Bad Guy", StartupMode.Auto,"JumpAttacker")]
    internal class BadGuy : Plugin
    {
        private readonly Unit _me;
        public static Config Config;
        [ImportingConstructor]
        public BadGuy([Import] IServiceContext context)
        {
            _me = context.Owner;
        }
        protected override void OnActivate()
        {
            Config = new Config();
        }

        protected override void OnDeactivate()
        {
            Config.Dispose();
        }
    }

    internal class Program
    {
        private static void Main()
        {
            
        }
    }
}