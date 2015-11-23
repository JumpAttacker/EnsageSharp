using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;

// ReSharper disable EmptyGeneralCatchClause
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace Auto_Disable
{
    internal class Program
    {
        enum DisableType
        {
            OnlyInitiators,
            All
        }

        enum Using
        {
            All,
            OnlyItems,
            OnlyAblities
        }

        #region Members
        private static readonly Menu Menu = new Menu("Auto Disable", "autodisable", true);
        private static readonly Menu ItemMenu = new Menu("Item", "Items");
        private static readonly Menu AbilityMenu = new Menu("Ability", "Ability");
        private static readonly Menu [,]ExtraSubMenu = new Menu[10,2];
        private static bool _loaded;
        //private static Hero me;
        private static readonly string Ver = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        private static readonly Dictionary<ClassID, string> Initiators = new Dictionary<ClassID, string>();
        //static readonly Dictionary<ClassID, string> CounterSpells = new Dictionary<ClassID, string>();
        private static readonly CounterHelp[] CounterSpells = new CounterHelp[50];
        private static readonly Dictionary<string, bool> PreLoadItems = new Dictionary<string, bool>();
        private static readonly Dictionary<string, bool> PreLoadSpell = new Dictionary<string, bool>();
        private static readonly List<string> ItemList = new List<string>();
        private static readonly List<string> SpellList = new List<string>();
        static bool[]Create=new bool[10];
        #endregion

        private static void Game_OnUpdate(EventArgs args)
        {
            var me = ObjectMgr.LocalHero;

            if (!_loaded)
            {

                if (!Game.IsInGame || me == null)
                {
                    return;
                }
                _loaded = true;
                PrintSuccess(string.Format("> AutoDisable Loaded v{0}", Ver));
                Game.PrintMessage(
                    "<font face='Comic Sans MS, cursive'><font color='#00aaff'>" + Menu.DisplayName + " By Jumpering" +
                    " loaded!</font> <font color='#aa0000'>v" + Ver, MessageType.LogMessage);
                PreLoadItems.Clear();
                PreLoadSpell.Clear();
                for (var asd = 0; asd < 10; asd++)
                {
                    Create[asd] = false;
                }
            }

            if (!Game.IsInGame || me == null)
            {
                _loaded = false;
                PrintInfo("> AutoDisable unLoaded");
                return;
            }

            #region Dodge by mod

            if (Utils.SleepCheck("item_manta"))
            {
                var dodgeByManta = me.FindItem("item_manta");
                var mod =
                    me.Modifiers.FirstOrDefault(
                        x =>
                            x.Name == "modifier_orchid_malevolence_debuff" || x.Name == "modifier_lina_laguna_blade" ||
                        x.Name == "modifier_pudge_meat_hook" || x.Name == "modifier_skywrath_mage_ancient_seal" ||
                        x.Name == "modifier_lion_finger_of_death");
                if (dodgeByManta != null && dodgeByManta.CanBeCasted() && mod != null)
                {
                    dodgeByManta.UseAbility();
                    Utils.Sleep(250, "item_manta");
                    return;
                }
            }

            #endregion
            uint i;
            var isInvis = me.IsInvisible();
            var isChannel = me.IsChanneling();
            var items = me.Inventory.Items.Where(x => x.CanBeCasted() && ItemList.Contains(x.Name));
            var spells = me.Spellbook.Spells.Where(x => x.CanBeCasted() && SpellList.Contains(x.Name));
            var newItems = false;
            
            foreach (var item in items.Where(item => !PreLoadItems.ContainsKey(item.Name)))
            {
                PreLoadItems.Add(item.Name, true);
                Game.PrintMessage("[AutoDisable] [ITEM] " + item.Name + " added to menu", MessageType.LogMessage);
                newItems = true;
            }
            
            var newSpells = false;
            foreach (var spell in spells.Where(item => !PreLoadSpell.ContainsKey(item.Name)))
            {
                PreLoadSpell.Add(spell.Name, true);
                Game.PrintMessage("[AutoDisable] [SPELL] " + spell.Name + " added to menu ", MessageType.LogMessage);
                newSpells = true;
                
            }
            /*
            if (newSpells)
            {

                AbilityMenu.AddItem(
                    new MenuItem("SelectedSpells" + me.Name, "Abilities:").SetValue(new AbilityToggler(PreLoadSpell)));
            }
            if (newItems)
            {
                ItemMenu.AddItem(
                    new MenuItem("SelectedItems" + me.Name, "Items:").SetValue(new AbilityToggler(PreLoadItems)));
            }
            */
            /*foreach (var ability in spells.Where(item => PreLoadSpell.ContainsKey(item.Name)))
            {
                Game.PrintMessage(
                    ExtraSubMenu[i, 1].Item("SelectedSpells" + _me.Name).GetValue<AbilityToggler>().IsEnabled(ability.Name).ToString(),
                    MessageType.ChatMessage);
            }*/
            for (i = 0; i < 10; i++)
            {
                try
                {
                    var v = ObjectMgr.GetPlayerById(i).Hero;
                    if (v == null || v.Team == me.Team || Equals(v, me) || !v.IsAlive || !v.IsVisible ||
                        !Utils.SleepCheck(v.GetHashCode().ToString())) continue;
                    var isInvul = v.IsInvul();
                    var magicImmnune = v.IsMagicImmune();
                    //var LinkProt = v.LinkProtection();
                    var isStun = v.IsStunned();
                    var isHex = v.IsHexed();
                    var isSilence = v.IsSilenced();
                    var isDisarm = v.IsDisarmed();
                    
                    /*try
                    {
                        Game.PrintMessage("[AutoDisable] " + Menu.Item("ActiveSpell" + i).GetValue<bool>(), MessageType.LogMessage);
                    }
                    catch (Exception)
                    {
                    }
                    */
                    if (!Create[i])
                    {
                        ExtraSubMenu[i, 0] = new Menu("", i.ToString(), false, v.Name);
                        ExtraSubMenu[i, 0].AddItem(new MenuItem("ActiveItem" + i, "Active For This Hero").SetValue(true));
                        ItemMenu.AddSubMenu(ExtraSubMenu[i,0]);
                        ExtraSubMenu[i, 1] = new Menu("", i.ToString(), false, v.Name);
                        ExtraSubMenu[i, 1].AddItem(new MenuItem("ActiveSpell"+i, "Active For This Hero").SetValue(true));
                        AbilityMenu.AddSubMenu(ExtraSubMenu[i,1]);
                        Create[i] = true;
                    }
                    if (newSpells)
                    {
                        ExtraSubMenu[i, 1].AddItem(
                            new MenuItem("SelectedSpells" + me.Name, "Abilities:").SetValue(new AbilityToggler(PreLoadSpell)));
                    }
                    if (newItems)
                    {
                        ExtraSubMenu[i, 0].AddItem(
                            new MenuItem("SelectedItems" + me.Name, "Items:").SetValue(new AbilityToggler(PreLoadItems)));
                    }
                    if ((isInvis && !me.IsVisibleToEnemies)|| isChannel) continue;

                    
                    var blink = v.FindItem("item_blink");
                    var forcestaff = v.FindItem("item_force_staff");
                    var dpActivated =
                        v.Modifiers.Any(
                            x => x.Name == "modifier_slark_dark_pact" || x.Name == "modifier_slark_dark_pact_pulses");
                    var enumerable = items as IList<Item> ?? items.ToList();
                    var distance = me.Distance2D(v);
                    //var angle =
                    string spellString;
                    var angle = (float) (Math.Max(
                        Math.Abs(v.RotationRad - Utils.DegreeToRadian(v.FindAngleBetween(me.Position))) - 0.20, 0));

                    if (!enumerable.Any() || !(isInvul || magicImmnune || isInvis || isChannel || dpActivated))
                    {
                        
                        if ((blink != null && blink.Cooldown > 11) || forcestaff != null && forcestaff.Cooldown > 18.6)
                        {
                            UseDisableStageOne(v, enumerable, null, false, true, me, i);
                        }
                        else if (Menu.Item("onlyoninitiators").GetValue<StringList>().SelectedIndex == (int)DisableType.All)
                        {
                            
                            UseDisableStageOne(v, enumerable, null, false, false, me, i);
                        }
                        else if (Initiators.TryGetValue(v.ClassID, out spellString))
                        {
                            var initSpell = v.FindSpell(spellString);
                            if (initSpell != null && initSpell.Cooldown != 0)
                            {
                                UseDisableStageOne(v, enumerable, null, false, true, me, i);
                            }
                        }
                    }
                    if (isStun || isHex || isSilence || isDisarm) continue;
                    var abilities = spells as IList<Ability> ?? spells.ToList();
                    if (!abilities.Any() && !enumerable.Any()) continue;
                    //var 
                    /*if (v.ClassID == ClassID.CDOTA_Unit_Hero_Clinkz)
                    {
                        PrintError(String.Format("{0}", angle));
                    }*/
                    if (angle == 0 && distance < 1500)
                    {
                        var r = CheckForFirstSpell(v);
                        var r2 = CheckForSecondSpell(v);
                        //PrintError("kek");
                        var mustHave = false;
                        //PrintError("kek2");
                        var s = CheckForModifier(v, ref mustHave);
                        //PrintError("kek3");
                        var act = (CheckForNetworkAct(v) == v.NetworkActivity);
                        //PrintError("kek4");
                        //PrintError(String.Format("{0} must have: {1}", v.Name, mustHave));
                        var modifier = s != "" && v.Modifiers.FirstOrDefault(x => x.Name == s) != null;
                        //if (r != null) PrintInfo(String.Format("r:{0}", r.IsInAbilityPhase));
                        //if (r2 != null) PrintInfo(String.Format("r2:{0} ", r2.Name));
                        if ((r != null && r.IsInAbilityPhase) || (r2 != null && r2.IsInAbilityPhase) ||
                            mustHave && modifier && act || !mustHave && act)
                        {
                            CounterSpellAndItems(v, enumerable, abilities, me, i);
                        }
                    }
                    
                    if ((blink != null && blink.Cooldown > 11) || forcestaff != null && forcestaff.Cooldown > 18.6)
                    {
                        UseDisableStageOne(v, enumerable, abilities, true, true, me, i);
                    }
                    else if (Menu.Item("onlyoninitiators").GetValue<StringList>().SelectedIndex == (int)DisableType.All  && distance<=1200)
                    {
                        UseDisableStageOne(v, enumerable, abilities, true, false, me, i);
                    }
                    if (Initiators.TryGetValue(v.ClassID, out spellString) && distance < 1000)
                    {
                        var initSpell = v.FindSpell(spellString);
                        if (initSpell != null && initSpell.Cooldown != 0)
                        {
                            UseDisableStageOne(v, enumerable, abilities, true, true, me, i);
                        }
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        private struct CounterHelp
        {
            public readonly string StrKey;
            public readonly int IntKey;
            public readonly ClassID Hero;
            public readonly float ExtraRange;
            public readonly NetworkActivity Activity;
            public readonly string Modifer;

            public CounterHelp(ClassID hero, string strKey = "", int intKey = 0, float extraRange = 0,
                NetworkActivity activity = 0, string modifer = "")
                : this()
            {
                Hero = hero;
                Modifer = modifer;
                Activity = activity;
                IntKey = intKey;
                StrKey = strKey;
                ExtraRange = extraRange;
            }
        }

        #region Methods

        #region Init

        private static void Main()
        {
            #region init

            ItemList.Add("item_sheepstick");
            ItemList.Add("item_orchid");
            ItemList.Add("item_abyssal_blade");
            ItemList.Add("item_ethereal_blade");
            ItemList.Add("item_rod_of_atos");
            ItemList.Add("item_heavens_halberd");
            ItemList.Add("item_medallion_of_courage");
            ItemList.Add("item_cyclone");
            ItemList.Add("item_solar_crest");
            ItemList.Add("item_blade_mail");
            ItemList.Add("item_lotus_orb");
            ItemList.Add("item_glimmer_cape");

            SpellList.Add("lion_voodoo");
            SpellList.Add("shadow_shaman_voodoo");
            SpellList.Add("obsidian_destroyer_astral_imprisonment");
            SpellList.Add("shadow_demon_disruption");
            SpellList.Add("rubick_telekinesis");
            SpellList.Add("dragon_knight_dragon_tail");
            SpellList.Add("batrider_flaming_lasso");
            SpellList.Add("legion_commander_duel");
            SpellList.Add("skywrath_mage_ancient_seal");
            SpellList.Add("silencer_last_word");
            SpellList.Add("slark_shadow_dance");
            SpellList.Add("slark_dark_pact");
            SpellList.Add("puck_waning_rift");
            SpellList.Add("axe_berserkers_call");
            SpellList.Add("juggernaut_omni_slash");
            SpellList.Add("doombringer_doom");
            SpellList.Add("tusk_snowball");
            SpellList.Add("naga_siren_mirror_image");
            SpellList.Add("alchemist_chemical_rage");
            SpellList.Add("bounty_hunter_wind_walk");
            SpellList.Add("clinkz_skeleton_walk");
            SpellList.Add("sandking_sandstorm");
            SpellList.Add("weaver_shukuchi");
            SpellList.Add("nyx_assassin_vendetta");
            SpellList.Add("templar_assassin_refraction");
            SpellList.Add("templar_assassin_meld");
            SpellList.Add("juggernaut_blade_fury");
            SpellList.Add("life_stealer_rage");
            SpellList.Add("silencer_global_silence");
            SpellList.Add("nyx_assassin_spiked_carapace");

            
            Initiators.Add(ClassID.CDOTA_Unit_Hero_FacelessVoid, "faceless_void_time_walk");
            Initiators.Add(ClassID.CDOTA_Unit_Hero_Shredder, "shredder_timber_chain");
            Initiators.Add(ClassID.CDOTA_Unit_Hero_Phoenix, "phoenix_icarus_dive");
            Initiators.Add(ClassID.CDOTA_Unit_Hero_AntiMage, "antimage_blink");
            Initiators.Add(ClassID.CDOTA_Unit_Hero_Legion_Commander, "legion_commander_duel");
            Initiators.Add(ClassID.CDOTA_Unit_Hero_Mirana, "mirana_leap");
            Initiators.Add(ClassID.CDOTA_Unit_Hero_PhantomLancer, "phantom_lancer_doppelwalk");
            Initiators.Add(ClassID.CDOTA_Unit_Hero_Terrorblade, "terrorblade_sunder");
            Initiators.Add(ClassID.CDOTA_Unit_Hero_Huskar, "huskar_life_break");
            Initiators.Add(ClassID.CDOTA_Unit_Hero_Rattletrap, "rattletrap_hookshot");
            Initiators.Add(ClassID.CDOTA_Unit_Hero_EarthSpirit, "earth_spirit_rolling_boulder");
            Initiators.Add(ClassID.CDOTA_Unit_Hero_ChaosKnight, "chaos_knight_reality_rift");
            Initiators.Add(ClassID.CDOTA_Unit_Hero_Morphling, "morphling_waveform");
            Initiators.Add(ClassID.CDOTA_Unit_Hero_VengefulSpirit, "vengefulspirit_nether_swap");
            Initiators.Add(ClassID.CDOTA_Unit_Hero_PhantomAssassin, "phantom_assassin_phantom_strike");
            Initiators.Add(ClassID.CDOTA_Unit_Hero_Riki, "riki_blink_strike");
            Initiators.Add(ClassID.CDOTA_Unit_Hero_Weaver, "weaver_time_lapse");
            Initiators.Add(ClassID.CDOTA_Unit_Hero_SandKing, "sandking_epicenter");
            Initiators.Add(ClassID.CDOTA_Unit_Hero_Slark, "slark_pounce");
            Initiators.Add(ClassID.CDOTA_Unit_Hero_CrystalMaiden, "crystal_maiden_freezing_field");
            Initiators.Add(ClassID.CDOTA_Unit_Hero_Pudge, "pudge_dismember");
            Initiators.Add(ClassID.CDOTA_Unit_Hero_Bane, "bane_fiends_grip");
            Initiators.Add(ClassID.CDOTA_Unit_Hero_Enigma, "enigma_black_hole");
            Initiators.Add(ClassID.CDOTA_Unit_Hero_WitchDoctor, "witch_doctor_death_ward");
            Initiators.Add(ClassID.CDOTA_Unit_Hero_QueenOfPain, "queenofpain_blink");
            Initiators.Add(ClassID.CDOTA_Unit_Hero_StormSpirit, "storm_spirit_ball_lightning");
            Initiators.Add(ClassID.CDOTA_Unit_Hero_Puck, "puck_illusory_orb");
            Initiators.Add(ClassID.CDOTA_Unit_Hero_Magnataur, "magnataur_skewer");
            Initiators.Add(ClassID.CDOTA_Unit_Hero_EmberSpirit, "ember_spirit_fire_remnant");
            var c = 0;
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_ShadowShaman, "q");
            c++;
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_Bane, "r");
            c++;
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_SkeletonKing, "q");
            c++;
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_Earthshaker, "q");
            c++;
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_Axe, "q");
            c++;
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_Pudge, "q");
            c++;
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_FacelessVoid, "r");
            c++;
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_Puck, "w");
            c++;
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_Slardar, "w");
            c++;
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_Silencer, "r");
            c++;
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_DoomBringer, "f");
            c++;
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_Necrolyte, "f");
            c++;
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_Terrorblade, "f");
            c++;
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_Nevermore, "f");
            c++;
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_Sven, "q");
            c++;
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_Bloodseeker, "r");
            c++;
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_QueenOfPain, "r", 0, -0.70f);
            c++;
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_Lina, "r");
            c++;
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_Lion, "r");
            c++;
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_NightStalker, "q");
            c++;
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_Luna, "q");
            c++;
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_Magnataur, "q", 4);
            c++;
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_Tinker, "q");
            c++;
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_Ogre_Magi, "q");
            c++;
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_Zuus, "r", 2);
            c++;
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_PhantomAssassin, "", 0, 0, NetworkActivity.Crit);
            c++;
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_Clinkz, "", 0, 0, NetworkActivity.Attack,
                "modifier_clinkz_strafe");
            c++;
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_Tusk, "r");
            c++;
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_Ursa, "", 0, 0, NetworkActivity.Attack,
                "modifier_ursa_overpower");
            c++;
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_Undying, "q");
            c++;
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_Abaddon, "q");
            c++;
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_CrystalMaiden, "w", 1);
            c++;
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_Obsidian_Destroyer, "r");
            c++;
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_Lich, "q");
            c++;
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_Centaur, "q", 2);
            c++;
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_DragonKnight, "q", 2);
            c++;
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_Riki, "r");
            c++;
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_Mirana, "q");
            c++;
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_Legion_Commander, "q", 4);
            c++;
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_Necrolyte, "r");
            c++;
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_SpiritBreaker, "r");
            c++;
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_DoomBringer, "r");
            c++;
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_Bane, "r", 2);

            #endregion

            Game.OnUpdate += Game_OnUpdate;
            _loaded = false;
            Menu.AddItem(new MenuItem("onlyoninitiators", "Disable").SetValue(new StringList(new[] { "Only Initiators", "All" })));
            Menu.AddItem(new MenuItem("usedagger", "Use Dagger").SetValue(true).SetTooltip("use a dagger to escape"));
            Menu.AddItem(new MenuItem("oneenemy", "Disable One Enemy").SetValue(true).SetTooltip("use only one disable for one enemy at one time"));
            Menu.AddItem(new MenuItem("using", "Use").SetValue(new StringList(new[] { "All", "only items", "only abilities" })).SetTooltip("what should i use for disable"));
            Menu.AddSubMenu(ItemMenu);
            Menu.AddSubMenu(AbilityMenu);
            Menu.AddToMainMenu();
        }

        #endregion

        private static void CounterSpellAndItems(Hero target, IEnumerable<Item> items, IEnumerable<Ability> abilities, Unit me, uint i)
        {
            if (!me.IsValid) return;
            var enumerable = items as Item[] ?? items.ToArray();
            
            var safeItemSelf = enumerable.FirstOrDefault(
                x =>
                    (x.Name == "item_blade_mail" /*|| x.Name == "item_black_king_bar" || x.Name == "item_ghost" ||
                        x.Name == "item_manta"*/) && ExtraSubMenu[i, 0].Item("SelectedItems" + me.Name).GetValue<AbilityToggler>().IsEnabled(x.Name));
            var safeItemTargetSelf = enumerable.FirstOrDefault(
                x =>
                    (x.Name == "item_lotus_orb" || x.Name == "item_glimmer_cape") && ExtraSubMenu[i, 0].Item("SelectedItems" + me.Name).GetValue<AbilityToggler>().IsEnabled(x.Name));
            var safeItemTargetEnemy = enumerable.FirstOrDefault(
                x =>
                    (x.Name == "item_sheepstick" || x.Name == "item_orchid" || x.Name == "item_abyssal_blade" ||
                    x.Name == "item_heavens_halberd" || x.Name == "item_cyclone") && ExtraSubMenu[i, 0].Item("SelectedItems" + me.Name).GetValue<AbilityToggler>().IsEnabled(x.Name));
            var safeItemPoint = enumerable.FirstOrDefault(
                x =>
                    x.Name == "item_blink");


            var safeSpell = abilities.FirstOrDefault(
                x =>
                    (/*x.Name == "antimage_blink" || x.Name == "queenofpain_blink" ||*/
                    x.Name == "nyx_assassin_spiked_carapace" || x.Name == "silencer_last_word" ||
                    x.Name == "shadow_demon_disruption" || x.Name == "obsidian_destroyer_astral_imprisonment" ||
                    x.Name == "slark_shadow_dance" || x.Name == "slark_dark_pact" || x.Name == "puck_waning_rift" ||
                    x.Name == "axe_berserkers_call" || x.Name == "abaddon_aphotic_shield" ||
                    /*x.Name == "ember_spirit_flame_guard" ||*/ x.Name == "skywrath_mage_ancient_seal" ||
                    x.Name == "juggernaut_omni_slash" || x.Name == "doombringer_doom" ||
                    x.Name == "tusk_snowball" || x.Name == "naga_siren_mirror_image" ||
                    x.Name == "alchemist_chemical_rage" || x.Name == "bounty_hunter_wind_walk" ||
                    /*x.Name == "phantom_lancer_doppelwalk" ||*/ x.Name == "clinkz_skeleton_walk" ||
                    x.Name == "sandking_sandstorm" || x.Name == "weaver_shukuchi" ||
                    x.Name == "nyx_assassin_vendetta" || x.Name == "templar_assassin_refraction" ||
                    x.Name == "templar_assassin_meld" /*|| x.Name == "phoenix_supernova" */||
                    x.Name == "juggernaut_blade_fury" || x.Name == "life_stealer_rage" || x.Name == "lion_voodoo" ||
                    x.Name == "shadow_shaman_voodoo" /*|| x.Name == "oracle_fates_edict" */||
                    /* x.Name == "winter_wyvern_cold_embrace" ||*/ x.Name == "life_stealer_rage" || x.Name == "silencer_global_silence") && ExtraSubMenu[i, 1].Item("SelectedSpells" + me.Name).GetValue<AbilityToggler>().IsEnabled(x.Name));
            /*local v = entityList:GetEntities({classId = CDOTA_Unit_Fountain,team = me.team})[1]
				local vec = Vector((v.position.x - me.position.x) * 1100 / GetDistance2D(v,me) + me.position.x,(v.position.y - me.position.y) * 1100 / GetDistance2D(v,me) + me.position.y,v.position.z)
				if blinkfront then
					vec = Vector(me.position.x+1200*math.cos(me.rotR), me.position.y+1200*math.sin(me.rotR), me.position.z)
				end*/
            var v =
                ObjectMgr.GetEntities<Unit>()
                    .FirstOrDefault(x => x.Team == me.Team && x.ClassID == ClassID.CDOTA_Unit_Fountain);
            if (Menu.Item("using").GetValue<StringList>().SelectedIndex == (int) Using.All ||
                Menu.Item("using").GetValue<StringList>().SelectedIndex == (int) Using.OnlyItems)
            {
                if (safeItemSelf != null && Menu.Item("ActiveItem" + i).GetValue<bool>())
                {
                    safeItemSelf.UseAbility();
                    Utils.Sleep(250, target.GetHashCode().ToString());
                    if (Menu.Item("oneenemy").GetValue<bool>()) return;
                }
                //PrintError(String.Format("{0} {1}", SafeSpell.CastRange,SafeSpell.Name));
                if (safeItemPoint != null && Menu.Item("usedagger").GetValue<bool>() && me.Distance2D(target) <= 1000)
                {
                    if (v != null && v.IsValid)
                    {
                        safeItemPoint.UseAbility(v.Position);
                        Utils.Sleep(250, target.GetHashCode().ToString());
                        if (Menu.Item("oneenemy").GetValue<bool>()) return;
                    }
                }
                if (safeItemTargetEnemy != null && safeItemTargetEnemy.CanBeCasted(target) && Menu.Item("ActiveItem" + i).GetValue<bool>())
                {
                    if (me.Distance2D(target) <= safeItemTargetEnemy.CastRange)
                    {
                        safeItemTargetEnemy.UseAbility(target);
                        Utils.Sleep(250, target.GetHashCode().ToString());
                        if (Menu.Item("oneenemy").GetValue<bool>()) return;
                    }
                }
                if (safeItemTargetSelf != null && Menu.Item("ActiveItem" + i).GetValue<bool>())
                {
                    safeItemTargetSelf.UseAbility(me);
                    Utils.Sleep(250, target.GetHashCode().ToString());
                    if (Menu.Item("oneenemy").GetValue<bool>()) return;

                }
            }
            if (!Menu.Item("ActiveSpell" + i).GetValue<bool>() || safeSpell == null || !safeSpell.CanBeCasted() ||
                (Menu.Item("using").GetValue<StringList>().SelectedIndex != (int) Using.All &&
                 Menu.Item("using").GetValue<StringList>().SelectedIndex != (int) Using.OnlyAblities)) return;
            if (safeSpell.CastRange > 0)
            {
                if (me.Distance2D(target) <= safeSpell.CastRange)
                {
                    safeSpell.UseAbility(target);
                }
            }
            else
            {
                safeSpell.UseAbility();
            }
            Utils.Sleep(250, target.GetHashCode().ToString());
        }

        private static void UseDisableStageOne(Hero target, IEnumerable<Item> items, IEnumerable<Ability> abilities, bool stage, bool itsRealConterSpell, Hero me, uint i)
        {
            if (!me.IsValid) return;
            Item disable;
            Ability ab = null;
            Ability withOutTarget = null;
            var enumerable = items as Item[] ?? items.ToArray();
            if (stage)
            {
                disable =
                    enumerable.FirstOrDefault( // wo puck_waning_rift
                        x =>
                            (x.Name == "item_sheepstick" || x.Name == "item_orchid" ||
                            x.Name == "item_abyssal_blade" ||
                            x.Name == "item_ethereal_blade" ||
                            x.Name == "item_rod_of_atos" || x.Name == "item_heavens_halberd" ||
                            x.Name == "item_medallion_of_courage" ||
                            x.Name == "item_cyclone" || x.Name == "item_solar_crest") && ExtraSubMenu[i, 0].Item("SelectedItems" + me.Name).GetValue<AbilityToggler>().IsEnabled(x.Name));
                var enumerable1 = abilities as Ability[] ?? abilities.ToArray();
                ab =
                    enumerable1.FirstOrDefault(
                        x =>
                            (x.Name == "lion_voodoo" || x.Name == "shadow_shaman_voodoo" ||
                            x.Name == "obsidian_destroyer_astral_imprisonment" || x.Name == "shadow_demon_disruption" ||
                            x.Name == "rubick_telekinesis" ||
                            x.Name == "dragon_knight_dragon_tail" ||
                            x.Name == "batrider_flaming_lasso" ||
                            x.Name == "legion_commander_duel" ||
                            x.Name == "skywrath_mage_ancient_seal" /*|| x.Name == "vengefulspirit_magic_missile"*/ ) && ExtraSubMenu[i, 1].Item("SelectedSpells" + me.Name).GetValue<AbilityToggler>().IsEnabled(x.Name));
                withOutTarget =
                    enumerable1.FirstOrDefault(
                        x =>
                            x.Name == "silencer_global_silence" && ExtraSubMenu[i, 1].Item("SelectedSpells" + me.Name).GetValue<AbilityToggler>().IsEnabled(x.Name));
            }
            else
            {
                disable =
                    enumerable.FirstOrDefault(
                        x =>
                            (x.Name == "item_rod_of_atos" || x.Name == "item_medallion_of_courage" ||
                            x.Name == "item_solar_crest") && ExtraSubMenu[i, 0].Item("SelectedItems" + me.Name).GetValue<AbilityToggler>().IsEnabled(x.Name));
            }
            if (Menu.Item("using").GetValue<StringList>().SelectedIndex == (int) Using.All ||
                Menu.Item("using").GetValue<StringList>().SelectedIndex == (int) Using.OnlyItems)
            {
                if (disable != null && me.Distance2D(target) <= disable.CastRange && Menu.Item("ActiveItem" + i).GetValue<bool>())
                {
                    disable.UseAbility(target);
                    Utils.Sleep(250, target.GetHashCode().ToString());
                    if (Menu.Item("oneenemy").GetValue<bool>()) return;
                }
            }
            if (Menu.Item("using").GetValue<StringList>().SelectedIndex == (int) Using.All ||
                Menu.Item("using").GetValue<StringList>().SelectedIndex == (int) Using.OnlyAblities)
            {
                if (ab != null && me.Distance2D(target) <= ab.CastRange && Menu.Item("ActiveSpell" + i).GetValue<bool>())
                {
                    ab.UseAbility(target);
                    Utils.Sleep(250, target.GetHashCode().ToString());
                    if (Menu.Item("oneenemy").GetValue<bool>()) return;
                }

                if (withOutTarget != null && Menu.Item("ActiveSpell" + i).GetValue<bool>())
                {
                    withOutTarget.UseAbility();
                    Utils.Sleep(250, target.GetHashCode().ToString());
                    if (Menu.Item("oneenemy").GetValue<bool>()) return;
                }
            }
            if (Menu.Item("using").GetValue<StringList>().SelectedIndex != (int) Using.All ||
                Menu.Item("using").GetValue<StringList>().SelectedIndex != (int) Using.OnlyItems) return;
            if (!itsRealConterSpell) return;
            var
                safeItemPoint =
                    enumerable.FirstOrDefault(
                        x =>
                            x.Name == "item_blink");
            var v =
                ObjectMgr.GetEntities<Unit>()
                    .FirstOrDefault(x => x.Team == me.Team && x.ClassID == ClassID.CDOTA_Unit_Fountain);
            if (safeItemPoint == null || !Menu.Item("usedagger").GetValue<bool>() || !(me.Distance2D(target) <= 1000))
                return;
            if (v != null)
            {
                safeItemPoint.UseAbility(v.Position);
            }
            Utils.Sleep(250, target.GetHashCode().ToString());
        }


        #endregion

        #region Helpers

        private static NetworkActivity CheckForNetworkAct(Entity t)
        {
            for (var n = 0; n < CounterSpells.Length; n++)
                if (t.ClassID == CounterSpells[n].Hero)
                {
                    if (CounterSpells[n].Activity != 0)
                    {
                        return CounterSpells[n].Activity;
                    }
                }
            return 0;
        }

        private static string CheckForModifier(Entity t, ref bool mustHave)
        {
            for (var n = 0; n < CounterSpells.Length; n++)
                if (t.ClassID == CounterSpells[n].Hero)
                {
                    if (CounterSpells[n].Modifer != "")
                    {
                        mustHave = true;
                        return CounterSpells[n].Modifer;
                    }
                }
            return "";
        }

        private static Ability CheckForFirstSpell(Unit t)
        {
            try
            {
                for (var n = 0; n < CounterSpells.Length; n++)
                    if (t.ClassID == CounterSpells[n].Hero)
                    {
                        switch (CounterSpells[n].StrKey)
                        {
                            case "q":
                                return t.Spellbook.Spell1;
                            case "w":
                                return t.Spellbook.Spell2;
                            case "e":
                                return t.Spellbook.Spell3;
                            case "r":
                                //
                                return t.Spellbook.Spell4;
                            case "f":
                                return t.Spellbook.Spell5;
                        }
                    }
            }
            catch (Exception)
            {
                //PrintError(e.ToString());
            }
            return null;
        }

        private static Ability CheckForSecondSpell(Unit t)
        {
            try
            {
                for (var n = 0; n < CounterSpells.Length; n++)
                    if (t.ClassID == CounterSpells[n].Hero)
                    {
                        //PrintInfo("Hero catched: " + CounterSpells[n]._intKey);
                        switch (CounterSpells[n].IntKey)
                        {
                            case 1:
                                return t.Spellbook.Spell1;
                            case 2:
                                return t.Spellbook.Spell2;
                            case 3:
                                return t.Spellbook.Spell3;
                            case 4:
                                return t.Spellbook.Spell4;
                            case 5:
                                return t.Spellbook.Spell5;
                        }
                        break;
                    }
            }
            catch (Exception)
            {
                //PrintError(e.ToString());
            }
            //PrintInfo("Return: 0" );
            return null;
        }

        private static void PrintInfo(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.White, arguments);
        }

        private static void PrintSuccess(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.Green, arguments);
        }

        // ReSharper disable once UnusedMember.var
        private static void PrintError(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.Red, arguments);
        }

        private static void PrintEncolored(string text, ConsoleColor color, params object[] arguments)
        {
            var clr = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text, arguments);
            Console.ForegroundColor = clr;
        }

        #endregion
    }
}