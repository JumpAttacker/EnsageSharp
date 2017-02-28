using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common.Enums;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using Ensage.Common.Objects.UtilityObjects;
using SharpDX;

namespace Auto_Disable
{
    public static class MenuManager
    {
        public static Menu Menu;
        public static bool IsEnable => Menu.Item("Menu.Enable").GetValue<bool>();
        public static bool IsDisableChannelGuys => Menu.Item("Menu.DisableChannel").GetValue<bool>();
        public static bool IsAntiInitiators => Menu.Item("Menu.AntiInitiators").GetValue<bool>();
        public static bool HelpAllyHeroes => Menu.Item("Menu.HelpAllyHeroes").GetValue<bool>();
        public static bool IsUseOnlyOne => Menu.Item("Menu.OneEventOneAbility").GetValue<bool>();
        public static bool IsNinjaMode => Menu.Item("Menu.NinjaMode").GetValue<bool>();
        public static bool IsEnableDebugger => Menu.Item("Dev.Text.enable").GetValue<bool>();
        public static bool ComboKey => Menu.Item("Key.Enable").GetValue<KeyBind>().Active;

        public static bool IsItemEnable(Hero hero, string item)
            =>
                Menu.Item("itemEnable").GetValue<AbilityToggler>().IsEnabled(item);
        public static bool IsItemEnable(Hero hero, string item, string enemyAbility)
            => 
                Menu.Item("itemEnable" + hero.ClassID + enemyAbility).GetValue<AbilityToggler>().IsEnabled(item);

        public static bool IsAbilityEnable(Hero hero, string item)
            =>
                Menu.Item("abilityEnable").GetValue<AbilityToggler>().IsEnabled(item);
        public static bool IsAbilityEnable(Hero hero, string item, string enemyAbility)
            =>
                Menu.Item("abilityEnable" + hero.ClassID + enemyAbility).GetValue<AbilityToggler>().IsEnabled(item);


        private static Menu _heroes;
        public static void Init()
        {
            Menu = new Menu("AutoDisable", "AutoDisable2", true);
            var settings = new Menu("Settings", "Settings");
            var usage = new Menu("Using", "Using");
            _heroes = new Menu("Heroes", "_heroes");
            Menu.AddItem(new MenuItem("Menu.Enable", "Enable").SetValue(true));
            settings.AddItem(new MenuItem("Menu.DisableChannel", "Anti Channeling").SetValue(true));
            settings.AddItem(new MenuItem("Menu.HelpAllyHeroes", "Help Ally Heroes").SetValue(true));
            settings.AddItem(new MenuItem("Menu.NinjaMode", "Ninja mode").SetValue(true))
                .SetTooltip(
                    "Do not use any disable if ur not visible for enemy. but still hero will use items for escaping");
            settings.AddItem(new MenuItem("Menu.AntiInitiators", "Anti Initiators").SetValue(true));
            settings.AddItem(new MenuItem("Menu.OneEventOneAbility", "1 Event -> 1 CounterSpell").SetValue(true));
            usage.AddItem(
                new MenuItem("text1", "Abilities:")).SetFontColor(Color.GhostWhite);
            usage.AddItem(
                new MenuItem("abilityEnable", "").SetValue(
                    new AbilityToggler(new Dictionary<string, bool>())));
            usage.AddItem(
                new MenuItem("text2", "Items:")).SetFontColor(Color.GhostWhite);
            usage.AddItem(
                new MenuItem("itemEnable", "").SetValue(
                    new AbilityToggler(new Dictionary<string, bool>())));
            var devolper = new Menu("Developer", "Developer");
            devolper.AddItem(new MenuItem("Dev.Text.enable", "Debug messages").SetValue(false));

            settings.AddSubMenu(usage);
            usage.AddSubMenu(_heroes);
            Menu.AddSubMenu(settings);
            Menu.AddSubMenu(devolper);

            #region Initiators
            Members.Initiators.Add(ClassID.CDOTA_Unit_Hero_FacelessVoid, AbilityId.faceless_void_time_walk);
            Members.Initiators.Add(ClassID.CDOTA_Unit_Hero_Shredder, AbilityId.shredder_timber_chain);
            Members.Initiators.Add(ClassID.CDOTA_Unit_Hero_Phoenix, AbilityId.phoenix_icarus_dive);
            Members.Initiators.Add(ClassID.CDOTA_Unit_Hero_AntiMage, AbilityId.antimage_blink);
            Members.Initiators.Add(ClassID.CDOTA_Unit_Hero_Legion_Commander, AbilityId.legion_commander_duel);
            Members.Initiators.Add(ClassID.CDOTA_Unit_Hero_Mirana, AbilityId.mirana_leap);
            Members.Initiators.Add(ClassID.CDOTA_Unit_Hero_PhantomLancer, AbilityId.phantom_lancer_doppelwalk);
            Members.Initiators.Add(ClassID.CDOTA_Unit_Hero_Terrorblade, AbilityId.terrorblade_sunder);
            Members.Initiators.Add(ClassID.CDOTA_Unit_Hero_Huskar, AbilityId.huskar_life_break);
            Members.Initiators.Add(ClassID.CDOTA_Unit_Hero_Rattletrap, AbilityId.rattletrap_hookshot);
            Members.Initiators.Add(ClassID.CDOTA_Unit_Hero_EarthSpirit, AbilityId.earth_spirit_rolling_boulder);
            Members.Initiators.Add(ClassID.CDOTA_Unit_Hero_ChaosKnight, AbilityId.chaos_knight_reality_rift);
            Members.Initiators.Add(ClassID.CDOTA_Unit_Hero_Morphling, AbilityId.morphling_waveform);
            Members.Initiators.Add(ClassID.CDOTA_Unit_Hero_VengefulSpirit, AbilityId.vengefulspirit_nether_swap);
            Members.Initiators.Add(ClassID.CDOTA_Unit_Hero_PhantomAssassin, AbilityId.phantom_assassin_phantom_strike);
            Members.Initiators.Add(ClassID.CDOTA_Unit_Hero_Riki, AbilityId.riki_blink_strike);
            Members.Initiators.Add(ClassID.CDOTA_Unit_Hero_Weaver, AbilityId.weaver_time_lapse);
            Members.Initiators.Add(ClassID.CDOTA_Unit_Hero_SandKing, AbilityId.sandking_epicenter);
            Members.Initiators.Add(ClassID.CDOTA_Unit_Hero_Slark, AbilityId.slark_pounce);
            Members.Initiators.Add(ClassID.CDOTA_Unit_Hero_CrystalMaiden, AbilityId.crystal_maiden_freezing_field);
            Members.Initiators.Add(ClassID.CDOTA_Unit_Hero_Pudge, AbilityId.pudge_dismember);
            Members.Initiators.Add(ClassID.CDOTA_Unit_Hero_Bane, AbilityId.bane_fiends_grip);
            Members.Initiators.Add(ClassID.CDOTA_Unit_Hero_Enigma, AbilityId.enigma_black_hole);
            Members.Initiators.Add(ClassID.CDOTA_Unit_Hero_WitchDoctor, AbilityId.witch_doctor_death_ward);
            Members.Initiators.Add(ClassID.CDOTA_Unit_Hero_QueenOfPain, AbilityId.queenofpain_blink);
            Members.Initiators.Add(ClassID.CDOTA_Unit_Hero_StormSpirit, AbilityId.storm_spirit_ball_lightning);
            Members.Initiators.Add(ClassID.CDOTA_Unit_Hero_Puck, AbilityId.puck_illusory_orb);
            Members.Initiators.Add(ClassID.CDOTA_Unit_Hero_Magnataur, AbilityId.magnataur_skewer);
            Members.Initiators.Add(ClassID.CDOTA_Unit_Hero_EmberSpirit, AbilityId.ember_spirit_fire_remnant);
            //Members.Initiators.Add(ClassID.CDOTA_Unit_Hero_Tidehunter, AbilityId.tidehunter_ravage);
            #endregion
        }

        public static void Handle()
        {
            Menu.AddToMainMenu();
            Members.Updater = new Sleeper();
            Core.Me = ObjectManager.LocalHero;
            HeroesInSystem = new List<Hero>();
            Game.OnUpdate += Core.Updater;
            Game.OnUpdate += Core.UpdateLogic;
        }
        public static void UnHandle()
        {
            Game.OnUpdate -= Core.Updater;
            Game.OnUpdate -= Core.UpdateLogic;
            for (var i = 0; i < 10; i++)
            {
                try
                {
                    if (Menus[i] != null)
                        Menus[i].RemoveSubMenu("_heroes");
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            Menu.RemoveFromMainMenu();
        }

        public static List<Hero> HeroesInSystem;
        public static Menu[] Menus = new Menu[10];
        

        public static void TryToInitNewHero(Hero hero)
        {
            if (HeroesInSystem.Contains(hero))
                return;
            if (Menus[hero.Player.ID] != null)
                return;
            HeroesInSystem.Add(hero);
            Menus[hero.Player.ID] = new Menu("", hero.ClassID.ToString(), false, hero.StoredName());
            _heroes.AddSubMenu(Menus[hero.Player.ID]);
            foreach (var ability in hero.Spellbook.Spells.Where(x=>x.IsShield() || x.IsDisable()))
            {
                var item = new Menu("", hero.ClassID + ability.StoredName(), false, ability.StoredName());
                Menus[hero.Player.ID].AddSubMenu(item);
                item.AddItem(
                    new MenuItem("text1" + hero.ClassID + ability.StoredName(), "Abilities:")).SetFontColor(Color.GhostWhite);
                item.AddItem(
                    new MenuItem("abilityEnable" + hero.ClassID + ability.StoredName(), "").SetValue(
                        new AbilityToggler(new Dictionary<string, bool>())));
                foreach (var spell in Members.Spells)
                {
                    item.Item("abilityEnable" + hero.ClassID + ability.StoredName()).GetValue<AbilityToggler>().Add(spell);
                }
                item.AddItem(
                    new MenuItem("text2" + hero.ClassID + ability.StoredName(), "Items:")).SetFontColor(Color.GhostWhite);
                item.AddItem(
                    new MenuItem("itemEnable" + hero.ClassID + ability.StoredName(), "").SetValue(
                        new AbilityToggler(new Dictionary<string, bool>())));
                foreach (var spell in Members.Items)
                {
                    item.Item("itemEnable" + hero.ClassID + ability.StoredName()).GetValue<AbilityToggler>().Add(spell);
                }
            }
            InitAbility(hero,"item_force_staff");
            InitAbility(hero,"item_blink");
        }
        private static void InitAbility(Hero hero,string name)
        {
            var item = new Menu("", hero.ClassID + name, false, name);
            Menus[hero.Player.ID].AddSubMenu(item);
            item.AddItem(
                new MenuItem("text1" + hero.ClassID + name, "Abilities:")).SetFontColor(Color.GhostWhite);
            item.AddItem(
                new MenuItem("abilityEnable" + hero.ClassID + name, "").SetValue(
                    new AbilityToggler(new Dictionary<string, bool>())));
            foreach (var spell in Members.Spells)
            {
                item.Item("abilityEnable" + hero.ClassID + name).GetValue<AbilityToggler>().Add(spell);
            }
            item.AddItem(
                    new MenuItem("text2" + hero.ClassID + name, "Items:")).SetFontColor(Color.GhostWhite);
            item.AddItem(
                new MenuItem("itemEnable" + hero.ClassID + name, "").SetValue(
                    new AbilityToggler(new Dictionary<string, bool>())));
            foreach (var spell in Members.Items)
            {
                item.Item("itemEnable" + hero.ClassID + name).GetValue<AbilityToggler>().Add(spell);
            }
        }
        public static void UpdateAbility(string storedName)
        {
            foreach (var hero in HeroesInSystem)
            {
                foreach (var ability in hero.Spellbook.Spells.Where(x=>x.IsShield() || x.IsDisable()))
                {
                    Menu.Item("abilityEnable" + hero.ClassID+ ability.StoredName()).GetValue<AbilityToggler>().Add(storedName);
                }
                
            }
        }
        public static void UpdateItem(string storedName)
        {
            if (storedName == null)
            {
                Printer.PrintError("[AutoDisable] UpdateItem");
                return;
            }
            foreach (var hero in HeroesInSystem)
            {
                foreach (var ability in hero.Spellbook.Spells.Where(x => x.IsShield() || x.IsDisable()))
                {
                    Menu.Item("itemEnable" + hero.ClassID + ability.StoredName()).GetValue<AbilityToggler>().Add(storedName);
                }
                Menu.Item("itemEnable" + hero.ClassID + "item_blink").GetValue<AbilityToggler>().Add(storedName);
                Menu.Item("itemEnable" + hero.ClassID + "item_force_staff").GetValue<AbilityToggler>().Add(storedName);
            }
        }

        public static void RemoveItem(string storedName)
        {
            foreach (var hero in HeroesInSystem)
            {
                foreach (var ability in hero.Spellbook.Spells.Where(x => x.IsShield() || x.IsDisable()))
                {
                    Menu.Item("itemEnable" + hero.ClassID + ability.StoredName()).GetValue<AbilityToggler>().Remove(storedName);
                }
                Menu.Item("itemEnable" + hero.ClassID + "item_blink").GetValue<AbilityToggler>().Remove(storedName);
                Menu.Item("itemEnable" + hero.ClassID + "item_force_staff").GetValue<AbilityToggler>().Remove(storedName);
            }
        }

        public static bool CheckForMoveItem(Hero hero, ref IEnumerable<Item> myItems, ref IEnumerable<Ability> myAbilities,string moveItem)
        {
            myItems = myItems.Where(x => IsItemEnable(hero, x.StoredName(), moveItem));
            myAbilities = myAbilities.Where(x => IsAbilityEnable(hero, x.StoredName(), moveItem));
            return myItems.Any() || myAbilities.Any();
        }
    }
}