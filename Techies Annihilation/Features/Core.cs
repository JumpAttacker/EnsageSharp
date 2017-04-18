using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.Common.Objects;
using Ensage.Common.Objects.UtilityObjects;
using Ensage.SDK.Helpers;
using Techies_Annihilation.BombFolder;
using Techies_Annihilation.Utils;

namespace Techies_Annihilation.Features
{
    internal class Core
    {
        public static Hero Me;
        public static Team MeTeam;
        public static Team EnemyTeam;
        public static Ability LandMine;
        public static Ability RemoteMine;
        public static Ability Suicide;
        public static bool ExtraDamageFromSuicide=false;
        public static List<BombManager> Bombs = new List<BombManager>();
        public static List<BombManager> LandBombs = new List<BombManager>();
        public static List<BombManager> RamoteBombs = new List<BombManager>();

        public static float GetLandMineDamage => LandMine.GetAbilityData("damage", LandMine.Level);
        public static float GetRemoteMineDamage => RemoteMine.GetAbilityData("damage", RemoteMine.Level);
        public static float GetSuicideDamage => Suicide.GetAbilityData("damage", Suicide.Level);

        public static MultiSleeper HeroSleeper=new MultiSleeper();
        public static void OnUpdate(EventArgs args)
        {
            var spellAmp = 0;//UnitExtensions.GetSpellAmplification(Me);
            foreach (var hero in Heroes.GetByTeam(EnemyTeam))      
            {
                if (HeroSleeper.Sleeping(hero) || !hero.IsAlive || !hero.IsVisible || !hero.CanDie(MenuManager.CheckForAegis))
                    continue;
                var listForDetonation = new List<BombManager>();
                var heroHealth = hero.Health+hero.HealthRegeneration;
                var reduction = RemoteMine.GetDamageReduction(hero);
                foreach (var element in Bombs)
                {
                    if (element.IsRemoteMine && element.Active)
                    {
                        if (element.CanHit(hero))
                        {
                            heroHealth -= DamageHelpers.GetSpellDamage(element.Damage, spellAmp, reduction);
                            listForDetonation.Add(element);
                            if (heroHealth <= 0)
                            {
                                HeroSleeper.Sleep(300 + listForDetonation.Count*30, hero);
                                foreach (var manager in listForDetonation)
                                {
                                    manager.Detonate();
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }

        public static void Init(Hero me)
        {
            Me = me;
            LandMine = me.GetAbilityById(AbilityId.techies_land_mines);
            RemoteMine = me.GetAbilityById(AbilityId.techies_remote_mines);
            Suicide = me.GetAbilityById(AbilityId.techies_suicide);
            MeTeam = me.Team;
            EnemyTeam = me.GetEnemyTeam();
        }
    }
}