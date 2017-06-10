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
                if (Core.Me.ClassId != ClassId.CDOTA_Unit_Hero_StormSpirit)
                    return;
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
