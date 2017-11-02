namespace ArcAnnihilation.Units.behaviour.Enabled
{
    internal class TempestAbilityChecker : IAbilityChecker
    {
        public bool IsItemEnabled(Ensage.AbilityId x)
        {
            return MenuManager.IsItemEnabledTempest(x);
        }

        public bool IsAbilityEnabled(Ensage.AbilityId id)
        {
            return MenuManager.IsAbilityEnabledTempest(id);
        }
    }
}