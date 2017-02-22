using Ensage;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using SharpDX;

namespace Legion_Annihilation
{
    public static class Helper
    {
        public static int GetAbilityDelay(Unit target, Ability ability)
        {
            return (int)((ability.FindCastPoint() + Members.MyHero.GetTurnTime(target)) * 1000.0 + Game.Ping);
        }

        public static int GetAbilityDelay(Vector3 targetPosition, Ability ability)
        {
            return (int)((ability.FindCastPoint() + Members.MyHero.GetTurnTime(targetPosition)) * 1000.0 + Game.Ping);
        }
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
    }
}