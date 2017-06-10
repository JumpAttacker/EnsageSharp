using Ensage.Common.Enums;

namespace ArcAnnihilation.Units.behaviour.Enabled
{
    internal class TempestAbilityChecker : IAbilityChecker
    {
        public bool IsItemEnabled(ItemId x)
        {
            return MenuManager.IsItemEnabledTempest(x);
        }

        public bool IsAbilityEnabled(AbilityId id)
        {
            return MenuManager.IsAbilityEnabledTempest(id);
        }
    }
}