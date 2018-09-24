using System;
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
                    case (int)CourierConfig.OrderType.MoveItemsToStash:
                        cour.GetAbilityById(AbilityId.courier_return_stash_items).UseAbility();
                        break;
                    case (int)CourierConfig.OrderType.BlockForSelectedHero:
                        var state = cour.State;
                        var stateissuer = cour.StateIssuer;
                        
                        if (stateissuer!=null)
                            switch (state)
                            {
                                case CourierState.Init:
                                    break;
                                case CourierState.Idle:
                                    break;
                                case CourierState.AtBase:
                                    break;
                                case CourierState.Move:
                                    MutePlayer(cour, stateissuer);
                                    break;
                                case CourierState.Deliver:
                                    MutePlayer(cour, stateissuer);
                                    break;
                                case CourierState.BackToBase:
                                    break;
                                case CourierState.Dead:
                                    break;
                            }
                        break;
                }
            }
        }

        private static void MutePlayer(Courier cour, Hero hero)
        {
            if (BadGuy.Config.Courier.MutedHeroes.Value.IsEnabled(hero.Name))
            {
                if (BadGuy.Config.Courier.ExtraSettingsForMute &&
                    cour.Inventory.Items.Any(x => x.OldOwner.Equals(ObjectManager.LocalHero)))
                {
                    cour.GetAbilityById(AbilityId.courier_transfer_items).UseAbility();
                }
                else
                {
                    cour.GetAbilityById(AbilityId.courier_return_stash_items).UseAbility();
                }

            }
        }
    }
}