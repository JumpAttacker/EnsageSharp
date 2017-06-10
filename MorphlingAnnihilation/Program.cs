using System.Reflection;
using Ensage;
using Ensage.Common;

namespace MorphlingAnnihilation
{
    internal static class Program
    {
        private static bool _loaded;

        private static void Main()
        {
            Draw.Init();
            MenuManager.Init();
            Events.OnLoad += (sender, eventArgs) =>
            {
                if (!_loaded)
                {
                    if (ObjectManager.LocalHero.ClassId!=ClassId.CDOTA_Unit_Hero_Morphling)
                        return;
                    MenuManager.Handle();
                    _loaded = true;
                    var info =
                        $"{MenuManager.Menu.DisplayName} loaded. v[{Assembly.GetExecutingAssembly().GetName().Version}]";
                    Game.PrintMessage(info);
                    Printer.PrintSuccess($"> {info}");
                }
            };
            Events.OnClose += (sender, eventArgs) =>
            {
                if (_loaded)
                {
                    MenuManager.UnHandle();
                    _loaded = false;
                }
            };
        }

        
    }
}
