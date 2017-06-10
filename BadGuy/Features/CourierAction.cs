using System.Linq;
using BadGuy.Configs;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Common.Objects.UtilityObjects;
using Ensage.SDK.Helpers;

namespace BadGuy.Features
{
    public static class CourierAction
    {
        private static readonly Sleeper RateSleeper = new Sleeper();
        public static void Updater()
        {
            if (RateSleeper.Sleeping)
                return;
            RateSleeper.Sleep(BadGuy.Config.Courier.Rate);
            foreach (
                var cour in
                EntityManager<Courier>.Entities.Where(x => x != null && x.IsValid && x.IsAlive && x.IsControllable))
            {
                switch (BadGuy.Config.Courier.Type.Item.GetValue<StringList>().SelectedIndex)
                {
                    case (int) CourierConfig.OrderType.BlockingOnBase:
                        cour.GetAbilityById(AbilityId.courier_return_to_base).UseAbility();
                        break;
                    case (int) CourierConfig.OrderType.GiveItemsToMainHero:
                        cour.GetAbilityById(AbilityId.courier_take_stash_and_transfer_items).UseAbility();
                        break;
                    case (int) CourierConfig.OrderType.GoToEnemyBase:
                        var fount = Helpers.GetEnemyFountain();
                        if (fount != null)
                            cour.Move(fount.Position);
                        break;
                    case (int) CourierConfig.OrderType.MoveItemsToStash:
                        cour.GetAbilityById(AbilityId.courier_return_stash_items).UseAbility();
                        break;
                }
            }
        }
    }
}