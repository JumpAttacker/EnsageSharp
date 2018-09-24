using System;
using System.ComponentModel.Composition;
using System.Linq;
using Ensage;
using Ensage.SDK.Helpers;
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

            /*var cour = EntityManager<Courier>.Entities.FirstOrDefault();

            UpdateManager.Subscribe(() =>
            {
                if (cour == null)
                    return;
                var items = cour.Inventory.Items;
                Console.WriteLine("-----------------------");
                foreach (var item in items)
                {
                    Console.WriteLine($"{item.Name} {item.OldOwner?.Name} | {item.Owner?.Name}");
                }
            }, 1000);*/
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