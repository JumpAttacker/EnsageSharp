using System.Linq;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;

namespace Wisp_Annihilation
{
    public static class Helper
    {
        private static int MinPercHealthForRelocate => Members.Menu.Item("autoSaver.Percent").GetValue<Slider>().Value;

        public static Hero ClosestToMouse(Hero source, float range = 600)
        {
            var mousePosition = Game.MousePosition;
            var enemyHeroes = ObjectManager.GetEntities<Hero>()
                .Where(
                    x =>
                        x.Team == source.GetEnemyTeam() && !x.IsIllusion && x.IsAlive && x.IsVisible &&
                        EntityExtensions.Distance2D(x, mousePosition) <= range /*&& !x.IsMagicImmune()*/)
                .OrderBy(x => x.Distance2D(mousePosition));
            return enemyHeroes.FirstOrDefault();
        }

        public static bool IsItemEnable(string name)
        {
            return Members.Menu.Item("itemEnable").GetValue<AbilityToggler>().IsEnabled(name);
        }
        public static bool IsHeroEnableForRelocate(string name)
        {
            return Members.Menu.Item("autoSaver.EnableFor").GetValue<HeroToggler>().IsEnabled(name);
        }
        public static bool IsHeroEnableForTether(string name)
        {
            return Members.Menu.Item("autoTether.EnableFor").GetValue<HeroToggler>().IsEnabled(name);
        }

        public static bool CheckForPercents(Hero x)
        {
            var health = x.Health;
            var maxHealth = x.MaximumHealth;
            var percent = (float)((float)health / (float)maxHealth * 100);
            //Printer.Print($"H[{health}]MH[{maxHealth}]Min%[{MinPercHealthForRelocate}]Final%[{percent}]");
            return percent <= MinPercHealthForRelocate;
        }
    }
}