using System.Linq;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using SharpDX;
using WindRunner_Annihilation.Logic;

namespace WindRunner_Annihilation
{
    public static class Helper
    {
        public static bool IsItemEnable(string name)
        {
            return Members.Menu.Item("itemEnable").GetValue<AbilityToggler>().IsEnabled(name);
        }
        public static bool IsItemEnable(Item name,bool isInCombo=true)
        {
            return isInCombo
                ? Members.Menu.Item("itemEnable").GetValue<AbilityToggler>().IsEnabled(name.StoredName())
                : Members.Menu.Item("itemEnableLinken").GetValue<AbilityToggler>().IsEnabled(name.StoredName());
        }

        public static bool IsAbilityEnable(string name)
        {
            return Members.Menu.Item("abilityEnable").GetValue<AbilityToggler>().IsEnabled(name);
        }
        public static bool IsAbilityEnable(Ability name)
        {
            return Members.Menu.Item("abilityEnable").GetValue<AbilityToggler>().IsEnabled(name.StoredName());
        }

        public static uint PriorityHelper(Item item)
        {
            return Members.Menu.Item("itemEnable").GetValue<PriorityChanger>().GetPriority(item.StoredName());
        }

        private static bool CheckRangeForUltimate => Members.Menu.Item("Blink.UltimateCheck").GetValue<bool>();
        public static Vector3 GetBestPositionForStun()
        {
            return
                Members.BestPoinits.Where(x => !CheckRangeForUltimate || ShackleshotCalculation.Target.Distance2D(x) <= 600)
                    .OrderBy(x => x.Distance2D(Members.MyHero))
                    .FirstOrDefault();
        }

        public static Vector3 GetClosestPositionForStun()
        {
            return ShackleshotCalculation.Target == null
                ? new Vector3()
                : Members.BestPoinits.OrderBy(x => x.Distance2D(ShackleshotCalculation.Target)).FirstOrDefault();
        }
    }
}