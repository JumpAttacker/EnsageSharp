using System.Threading.Tasks;
using Ensage.Common.Extensions;
using Ensage.Common.Threading;

namespace ArcAnnihilation.Units.behaviour.Abilities
{
    class CanUseAbilitiesNecroArcher : ICanUseAbilties
    {
        public async Task UseAbilities(UnitBase unitBase)
        {
            var necr = unitBase as Necronomicon;

            var ability = necr?.ManaBurn;
            if (ability != null && ability.CanBeCasted(Core.Target) && ability.CanHit(Core.Target))
            {
                ability.UseAbility(Core.Target);
                await Await.Delay(1000, Core.ComboToken.Token);
            }
        }
    }
}