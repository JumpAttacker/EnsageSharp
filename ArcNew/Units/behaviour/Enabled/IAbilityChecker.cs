using Ensage;

namespace ArcAnnihilation.Units.behaviour.Enabled
{
    public interface IAbilityChecker
    {
        bool IsItemEnabled(AbilityId id);
        bool IsAbilityEnabled(AbilityId id);
    }
}