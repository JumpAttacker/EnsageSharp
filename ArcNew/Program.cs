using System;
using System.ComponentModel.Composition;
using ArcAnnihilation.Manager;
using ArcAnnihilation.Panels;
using Ensage;
using Ensage.Common;
using Ensage.SDK.Service;
using Ensage.SDK.Service.Metadata;

namespace ArcAnnihilation
{
    [ExportPlugin("Arc Annihilation", HeroId.npc_dota_hero_arc_warden)]
    class Program : Plugin
    {
        public static IServiceContext GetContext;
        [ImportingConstructor]
        public Program(IServiceContext context)
        {
            GetContext = context;
        }

        private static void Main()
        {

        }

        protected override void OnActivate()
        {
            MenuManager.Init();
            DelayAction.Add(100, () =>
            {
                Core.Init();
                TempestManager.Fresh();
                IllusionManager.GetCreepManager();
                NecronomiconManager.GetNecronomiconManager();
                InputBlocker.GetInputBlocker().Load();
                if (OrderManager.CurrentOrder == null)
                    Console.WriteLine("");
                if (MenuManager.IsInfoPanelEnabled)
                    InfoPanel.GetInfoPanel().Load();
            });
        }
    }
}