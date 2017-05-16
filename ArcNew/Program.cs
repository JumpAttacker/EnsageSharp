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
                Game.PrintMessage("ObjectManager.LocalHero.ClassId: " + ObjectManager.LocalHero.ClassId);
                if (ObjectManager.LocalHero.ClassId != ClassId.CDOTA_Unit_Hero_ArcWarden)
                    return;
                MenuManager.Init();
                DelayAction.Add(100, () =>
                {
                    Core.Init();
                    TempestManager.Fresh();
                    IllusionManager.GetCreepManager();
                    NecronomiconManager.GetNecronomiconManager();
                    if (MenuManager.IsInfoPanelEnabled)
                        InfoPanel.GetInfoPanel().Load();
                });
            };
        }
    }
}