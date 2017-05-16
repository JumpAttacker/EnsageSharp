using System.Threading.Tasks;

namespace ArcAnnihilation.Units.behaviour.Abilities
{
    public interface ICanUseAbilties
    {
        Task UseAbilities(UnitBase myBase);
    }
}