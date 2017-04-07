using Ensage;
using Ensage.Common;
using Ensage.Common.Threading;
using SfAnnihilation.DrawingStuff;
using SfAnnihilation.Features;
using SfAnnihilation.Utils;

namespace SfAnnihilation
{
    internal class Program
    {
        private static void Main()
        {
            Events.OnLoad += (sender, args) =>
            {
                MenuManager.Init();
                DelayAction.Add(250, () =>
                {
                    Game.OnUpdate += Core.OnUpdate;
                    Game.OnIngameUpdate += EulCombo.OnUpdate;
                    Game.OnUpdate += RazeAim.OnUpdate;
                    Game.OnUpdate += RazeDrawing.OnUpdate;
                    Game.OnUpdate += RazeCancelSystem.Updater;
                    Drawing.OnDraw += InfoDrawing.OnDraw;
                    GameDispatcher.OnIngameUpdate += EulCombo.TestCombo;
                });
            };
        }
    }
}
