using Ensage.Common.Enums;
using AbilityId = Ensage.Common.Enums.AbilityId;

namespace ArcAnnihilation.Units.behaviour.Enabled
{
    public interface IAbilityChecker
    {
        bool IsItemEnabled(ItemId id);
        bool IsAbilityEnabled(AbilityId id);
    }
}