namespace ArcAnnihilation.Units.behaviour.Enabled
{
    internal class MainHeroAbilityChecker : IAbilityChecker
    {
        public bool IsItemEnabled(Ensage.AbilityId x)
        {
            return MenuManager.IsItemEnabled(x);
        }

        public bool IsAbilityEnabled(Ensage.AbilityId id)
        {
            return MenuManager.IsAbilityEnabled(id);
        }
    }
}