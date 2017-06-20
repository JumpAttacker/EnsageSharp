using System.Threading.Tasks;

namespace ArcAnnihilation.Units.behaviour.Abilities
{
    class CanNotUseAbilties : ICanUseAbilties
    {
        public async Task UseAbilities(UnitBase myBase)
        {
            return;
        }
    }
}
