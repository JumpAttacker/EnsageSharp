using System;
using Ensage;
using Ensage.Common.Objects.UtilityObjects;

namespace OverlayInformation
{
    internal static class Devolp
    {
        //private static Sleeper sleep=new Sleeper();
        //private static double panelHeroSizeX= 53.8900000000002;
        public static void ConsoleCommands(EventArgs args)
        {
            if (!Members.Menu.Item("Dev.Hax.enable").GetValue<bool>()) return;
            if (Game.IsKeyDown(9))
            {
                Game.ExecuteCommand("dota_hero_refresh");
                /*foreach (var ability in Members.MyHero.Spellbook.Spells)
                {
                    Console.WriteLine("ability: "+ ability.Name + $" [{ability.Level} lvl]");
                }*/

            }
            /*if (sleep.Sleeping)
                return;
            if (Game.IsKeyDown(107) )
            {
                panelHeroSizeX += 0.01;
                HudInfoNew.X = panelHeroSizeX * HudInfoNew.Monitor;
                Console.WriteLine($" panelHeroSizeX: { panelHeroSizeX}");
                Game.PrintMessage($" panelHeroSizeX: { panelHeroSizeX}");
                sleep.Sleep(50);
            }
            if (Game.IsKeyDown(109))
            {
                panelHeroSizeX -= 0.01;
                HudInfoNew.X = panelHeroSizeX * HudInfoNew.Monitor;
                Console.WriteLine($" panelHeroSizeX: { panelHeroSizeX}");
                Game.PrintMessage($" panelHeroSizeX: { panelHeroSizeX}");
                sleep.Sleep(50);
            }
            if (Game.IsKeyDown(97))
            {
                HudInfoNew.DireCompare += 0.01;
                HudInfoNew.X = panelHeroSizeX * HudInfoNew.Monitor;
                Console.WriteLine($" DireCompare: { HudInfoNew.DireCompare}");
                Game.PrintMessage($" DireCompare: { HudInfoNew.DireCompare} ");
                sleep.Sleep(50);
            }
            if (Game.IsKeyDown(98))
            {
                HudInfoNew.DireCompare -= 0.01;
                HudInfoNew.X = panelHeroSizeX * HudInfoNew.Monitor;
                Console.WriteLine($" DireCompare: { HudInfoNew.DireCompare}");
                Game.PrintMessage($" DireCompare: { HudInfoNew.DireCompare}");
                sleep.Sleep(50);
            }*/
        }
    }
}