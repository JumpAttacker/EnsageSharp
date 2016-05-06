using System;
using Ensage;

namespace OverylayInformationV2
{
    internal static class Devolp
    {
        public static void ConsoleCommands(EventArgs args)
        {
            if (!Members.Menu.Item("Dev.Hax.enable").GetValue<bool>()) return;
            if (Game.IsKeyDown(9))
            {
                Game.ExecuteCommand("dota_hero_refresh");
            }
        }
    }
}