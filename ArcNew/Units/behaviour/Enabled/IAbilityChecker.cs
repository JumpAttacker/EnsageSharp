namespace ArcAnnihilation.Units.behaviour.Enabled
{
    public interface IAbilityChecker
    {
        bool IsItemEnabled(Ensage.AbilityId id);
        bool IsAbilityEnabled(Ensage.AbilityId id);
    }
}