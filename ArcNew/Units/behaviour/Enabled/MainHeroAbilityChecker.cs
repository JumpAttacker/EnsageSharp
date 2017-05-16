using Ensage.Common.Enums;

namespace ArcAnnihilation.Units.behaviour.Enabled
{
    internal class MainHeroAbilityChecker : IAbilityChecker
    {
        public bool IsItemEnabled(ItemId x)
        {
            return MenuManager.IsItemEnabled(x);
        }

        public bool IsAbilityEnabled(AbilityId id)
        {
            return MenuManager.IsAbilityEnabled(id);
        }
    }
}