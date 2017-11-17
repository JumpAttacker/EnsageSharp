using ArcAnnihilation.Manager;
using ArcAnnihilation.Panels;
using Ensage;
using Ensage.Common;

namespace ArcAnnihilation
{
    internal class Program
    {
        private static void Main()
        {
            Events.OnLoad += (sender, args) =>
            {
                if (ObjectManager.LocalHero.HeroId != HeroId.npc_dota_hero_arc_warden)
                    return;
                MenuManager.Init();
                DelayAction.Add(100, () =>
                {
                    Core.Init();
                    TempestManager.Fresh();
                    IllusionManager.GetCreepManager();
                    NecronomiconManager.GetNecronomiconManager();
                    InputBlocker.GetInputBlocker().Load();
                    if (MenuManager.IsInfoPanelEnabled)
                        InfoPanel.GetInfoPanel().Load();
                });
            };
        }
    }
}