using Ensage;

namespace ArcAnnihilation.Units.behaviour.Enabled
{
    internal class TempestAbilityChecker : IAbilityChecker
    {
        public bool IsItemEnabled(AbilityId x)
        {
            return MenuManager.IsItemEnabledTempest(x);
        }

        public bool IsAbilityEnabled(AbilityId id)
        {
            return MenuManager.IsAbilityEnabledTempest(id);
        }
    }
}