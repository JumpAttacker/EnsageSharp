using System.Threading.Tasks;

namespace ArcAnnihilation.Units.behaviour.Items
{
    internal class CanNotUseItems : ICanUseItems
    {
        public async Task<bool> UseItems(UnitBase unitBase)
        {
            return true;
        }
    }
}