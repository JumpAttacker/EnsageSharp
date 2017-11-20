using Ensage;
using Ensage.Common;
using Ensage.Common.Threading;
using SfAnnihilation.DrawingStuff;
using SfAnnihilation.Features;

namespace SfAnnihilation
{
    internal class Program
    {

        private static void Main()
        {
            Events.OnLoad += (sender, args) =>
            {
                if (ObjectManager.LocalHero.ClassId != ClassId.CDOTA_Unit_Hero_Nevermore)
                    return;
                MenuManager.Init();
                DelayAction.Add(250, () =>
                {
                    Game.OnUpdate += Core.OnUpdate;
                    //Game.OnIngameUpdate += EulCombo.OnUpdate;
                    Game.OnUpdate += RazeAim.OnUpdate;
                    Game.OnUpdate += RazeDrawing.OnUpdate;
                    Drawing.OnDraw += InfoDrawing.OnDraw;
                    GameDispatcher.OnIngameUpdate += EulCombo.TestCombo;
                    ShadowBladeComboWomboFantastic.Init();
                });
            };
        }
    }
}
