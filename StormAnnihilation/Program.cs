using System.Linq;
using Ensage;
using Ensage.Common;

namespace StormAnnihilation
{
    internal class Program
    {
        private static void Main()
        {
            Events.OnLoad += (sender, args) =>
            {
                Core.Me = ObjectManager.LocalHero;
                MenuManager.Init();
                MenuManager.Handle();
                Game.PrintMessage($"{MenuManager.Menu.DisplayName} loaded! v.{MenuManager.Ver}");
            };
            Events.OnClose += (sender, args) =>
            {
                MenuManager.UnHandle();
            };
        }
    }
}
