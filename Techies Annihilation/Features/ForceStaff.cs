using System;
using Ensage;
using Ensage.Common;
using Ensage.Common.Objects;
using Ensage.Common.Objects.UtilityObjects;
using Ensage.SDK.Extensions;
using Ensage.SDK.Helpers;
using Techies_Annihilation.Utils;
using AbilityExtensions = Ensage.Common.Extensions.AbilityExtensions;
using ItemExtensions = Ensage.Common.Extensions.ItemExtensions;

namespace Techies_Annihilation.Features
{
    internal class ForceStaff
    {
        private static readonly Sleeper Updater = new Sleeper();
        public static void OnUpdate(EventArgs args)
        {
            if (!MenuManager.IsEnableForceStaff || Updater.Sleeping || !MenuManager.IsEnable)
                return;
            Updater.Sleep(1);
            var forceStuff = Core.Me.GetItemById(AbilityId.item_force_staff);
            if (forceStuff != null && ItemExtensions.CanBeCasted(forceStuff))
            {
                foreach (var hero in Heroes.GetByTeam(Core.EnemyTeam))
                {
                    if (Core.HeroSleeper.Sleeping(hero) || !hero.IsAlive || !hero.IsVisible ||
                        !hero.CanDie(MenuManager.CheckForAegis) || Prediction.IsTurning(hero) ||
                        !AbilityExtensions.CanHit(forceStuff, hero))
                        continue;
                    var heroPos = Prediction.InFront(hero, 600);
                    var heroHealth = hero.Health + hero.HealthRegeneration;
                    var reduction = Core.RemoteMine.GetDamageReduction(hero);
                    foreach (var element in Core.Bombs)
                    {
                        if (element.IsRemoteMine && element.Active)
                        {
                            if (element.CanHit(heroPos, hero))
                            {
                                heroHealth -= DamageHelpers.GetSpellDamage(element.Damage, 0, reduction);
                                if (heroHealth <= 0)
                                {
                                    forceStuff.UseAbility(hero);
                                    Core.HeroSleeper.Sleep(250, hero);
                                    Updater.Sleep(250);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}