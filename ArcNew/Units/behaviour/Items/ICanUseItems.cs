using System.Threading.Tasks;

namespace ArcAnnihilation.Units.behaviour.Items
{
    public interface ICanUseItems
    {
        Task<bool> UseItems(UnitBase unitBase);
    }
}