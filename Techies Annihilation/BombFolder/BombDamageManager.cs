using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.Common.Objects;
using Ensage.Common.Objects.UtilityObjects;
using Ensage.SDK.Helpers;
using Techies_Annihilation.Features;
using UnitExtensions = Ensage.SDK.Extensions.UnitExtensions;

namespace Techies_Annihilation.BombFolder
{
    internal class BombDamageManager
    {
        public class HeroDamageContainer
        {
            public Hero Hero { get; set; }
            public int CurrentLandMines;
            public int MaxLandMines;
            public int CurrentRemoteMines;
            public int MaxRemoteMines;
            public bool CanKillWithSuicide;
            public bool IsAlive => Hero.IsAlive;
            public HeroDamageContainer(Hero hero)
            {
                Hero = hero;
            }

            public string GetLandDamage => CurrentLandMines >= 0 ? $"{CurrentLandMines}/{MaxLandMines}" : "∞";

            public string GetRemoteDamage
                => CurrentRemoteMines >= 0 ? $"{CurrentRemoteMines}/{MaxRemoteMines}" : "∞";

            public string GetSuicideStatus => IsAlive ? (CanKillWithSuicide ? "Yes" : "No") : "!";
            public float HealthAfterSuicide { get; set; }
        }

        public static List<HeroDamageContainer> DamageContainers = new List<HeroDamageContainer>();
        public static void Init()
        {
            var sleeper = new Sleeper();
            var landLevel = Core.LandMine.Level;
            var remoteLevel = Core.RemoteMine.Level;
            var aghState = Core.Me.AghanimState();
            
            Game.OnUpdate += args =>
            {
                if (sleeper.Sleeping || !MenuManager.IsEnable)
                    return;
                sleeper.Sleep(MenuManager.GetUpdateSpeed);
                // ReSharper disable once RedundantCheckBeforeAssignment
                if (!Core.ExtraDamageFromSuicide)
                {
                    var techiesExtraDamage = Core.Me.GetAbilityById(AbilityId.special_bonus_unique_techies);
                    if (techiesExtraDamage.Level > 0)
                    {
                        Core.ExtraDamageFromSuicide = true;
                    }
                }
                if (landLevel != Core.LandMine.Level)
                {
                    landLevel = Core.LandMine.Level;
                    foreach (var bomb in Core.Bombs.Where(x=>!x.IsRemoteMine))
                    {
                        bomb.UpdateDamage();
                    }
                }
                // ReSharper disable once RedundantCheckBeforeAssignment
                /*if (remoteLevel != Core.RemoteMine.Level)
                {
                    remoteLevel = Core.RemoteMine.Level;
                    foreach (var bomb in Core.Bombs.Where(x => x.IsRemoteMine))
                    {
                        bomb.UpdateDamage();
                    }
                }*/
                if (!aghState && Core.Me.AghanimState())
                {
                    aghState = true;
                    foreach (var bomb in Core.Bombs.Where(x => x.IsRemoteMine))
                    {
                        bomb.OnUltimateScepter();
                    }
                }
                var landDamage = Core.GetLandMineDamage;
                var remoteDamage = Core.GetRemoteMineDamage;
                var suicideDamage = Core.GetSuicideDamage + (Core.ExtraDamageFromSuicide ? 400 : 0);
                var spellAmp = UnitExtensions.GetSpellAmplification(Core.Me);
                foreach (var hero in Heroes.GetByTeam(Core.EnemyTeam))
                {
                    var heroCont = DamageContainers.Find(x => x.Hero.Equals(hero));
                    if (heroCont == null)
                    {
                        heroCont = new HeroDamageContainer(hero);
                        DamageContainers.Add(heroCont);
                    }
                    var heroHealth = hero.Health + hero.HealthRegeneration;
                    var maxHeroHealth = hero.MaximumHealth;
                    var reduction = Core.LandMine.GetDamageReduction(hero);
                    var healthPerTickFromLand = DamageHelpers.GetSpellDamage(landDamage, 0, reduction);
                    var healthPerTickFromRemote = DamageHelpers.GetSpellDamage(remoteDamage, 0, reduction);
                    var healthPerSucide = DamageHelpers.GetSpellDamage(suicideDamage, spellAmp, reduction);
                    //Utils.Printer.Print($"to {hero.GetRealName()} -> {suicideDamage} [res: {reduction}] [amp: {spellAmp}] [defDmg: {healthPerSucide}] [extra400: {Core.ExtraDamageFromSuicide}]");
                    heroCont.CurrentLandMines = (int) Math.Ceiling(heroHealth/healthPerTickFromLand);
                    heroCont.MaxLandMines = (int)Math.Ceiling(maxHeroHealth/healthPerTickFromLand);
                    heroCont.CurrentRemoteMines = (int)Math.Ceiling(heroHealth/healthPerTickFromRemote);
                    heroCont.MaxRemoteMines = (int)Math.Ceiling(maxHeroHealth/healthPerTickFromRemote);
                    heroCont.HealthAfterSuicide = heroHealth - healthPerSucide;
                    heroCont.CanKillWithSuicide = heroCont.HealthAfterSuicide <= 0;
                }
            };
        }
    }
}