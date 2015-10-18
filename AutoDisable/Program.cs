using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;

// ReSharper disable EmptyGeneralCatchClause
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace Auto_Disable
{
    internal class Program
    {
        #region Members

        private static bool _loaded;
        private static Hero _me;
        private static Player _player;
        private static bool Activated=true;
        private const float Ver = (float) 1.0;
        static readonly Dictionary<ClassID, string> Ititiators = new Dictionary<ClassID, string>(); 

        #endregion
        #region Methods

        #region Init

        private static void Main()
        {
            Ititiators.Add(ClassID.CDOTA_Unit_Hero_FacelessVoid, "faceless_void_time_walk");
            Ititiators.Add(ClassID.CDOTA_Unit_Hero_Shredder, "shredder_timber_chain");
            Ititiators.Add(ClassID.CDOTA_Unit_Hero_Phoenix, "phoenix_icarus_dive");
            Ititiators.Add(ClassID.CDOTA_Unit_Hero_AntiMage, "antimage_blink");
            Ititiators.Add(ClassID.CDOTA_Unit_Hero_Legion_Commander, "legion_commander_duel");
            Ititiators.Add(ClassID.CDOTA_Unit_Hero_Mirana, "mirana_leap");
            Ititiators.Add(ClassID.CDOTA_Unit_Hero_PhantomLancer, "phantom_lancer_doppelwalk");
            Ititiators.Add(ClassID.CDOTA_Unit_Hero_Terrorblade, "terrorblade_sunder");
            Ititiators.Add(ClassID.CDOTA_Unit_Hero_Huskar, "huskar_life_break");
            Ititiators.Add(ClassID.CDOTA_Unit_Hero_Rattletrap, "rattletrap_hookshot");
            Ititiators.Add(ClassID.CDOTA_Unit_Hero_EarthSpirit, "earth_spirit_rolling_boulder");
            Ititiators.Add(ClassID.CDOTA_Unit_Hero_ChaosKnight, "chaos_knight_reality_rift");
            Ititiators.Add(ClassID.CDOTA_Unit_Hero_Morphling, "morphling_waveform");
            Ititiators.Add(ClassID.CDOTA_Unit_Hero_VengefulSpirit, "vengefulspirit_nether_swap");
            Ititiators.Add(ClassID.CDOTA_Unit_Hero_PhantomAssassin, "phantom_assassin_phantom_strike");
            Ititiators.Add(ClassID.CDOTA_Unit_Hero_Riki, "riki_blink_strike");
            Ititiators.Add(ClassID.CDOTA_Unit_Hero_Weaver, "weaver_time_lapse");
            Ititiators.Add(ClassID.CDOTA_Unit_Hero_SandKing, "sandking_epicenter");
            Ititiators.Add(ClassID.CDOTA_Unit_Hero_Slark, "slark_pounce");
            Ititiators.Add(ClassID.CDOTA_Unit_Hero_CrystalMaiden, "crystal_maiden_freezing_field");
            Ititiators.Add(ClassID.CDOTA_Unit_Hero_Pudge, "pudge_dismember");
            Ititiators.Add(ClassID.CDOTA_Unit_Hero_Bane, "bane_fiends_grip");
            Ititiators.Add(ClassID.CDOTA_Unit_Hero_Enigma, "enigma_black_hole");
            Ititiators.Add(ClassID.CDOTA_Unit_Hero_WitchDoctor, "witch_doctor_death_ward");
            Ititiators.Add(ClassID.CDOTA_Unit_Hero_QueenOfPain, "queenofpain_blink");
            Ititiators.Add(ClassID.CDOTA_Unit_Hero_StormSpirit, "storm_spirit_ball_lightning");
            Ititiators.Add(ClassID.CDOTA_Unit_Hero_Puck, "puck_illusory_orb");
            Ititiators.Add(ClassID.CDOTA_Unit_Hero_Magnataur, "magnataur_skewer");
            Ititiators.Add(ClassID.CDOTA_Unit_Hero_EmberSpirit, "ember_spirit_fire_remnant");
            Game.OnUpdate += Game_OnUpdate;
            _loaded = false;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        #endregion

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Game.IsInGame || _me == null || !_loaded) return;
            uint i;
            for (i = 0; i < 10; i++)
            {
                try
                {
                    var v = ObjectMgr.GetPlayerById(i).Hero;
                    if (v == null || v.Team==_me.Team|| Equals(v, _me) ||!v.IsAlive || !v.IsVisible || !Utils.SleepCheck(v.GetHashCode().ToString())) continue;
                    var isInvul = v.IsInvul();
                    var magicImmnune = v.IsMagicImmune();
                    //var LinkProt = v.LinkProtection();
                    var isStun = v.IsStunned();
                    var isHex = v.IsHexed();
                    var isSilence = v.IsSilenced();
                    var isDisarm = v.IsDisarmed();

                    var isInvis = _me.IsInvisible();
                    var isChannel = _me.IsChanneling();
                    var items = _me.Inventory.Items.Where(x => x.CanBeCasted());

                    var blink = v.FindItem("item_blink");
                    var forcestaff = v.FindItem("item_force_staff");
                    var dpActivated = v.Modifiers.Any(x => x.Name == "modifier_slark_dark_pact" || x.Name == "modifier_slark_dark_pact_pulses");
                    var enumerable = items as IList<Item> ?? items.ToList();

                    if (!enumerable.Any() || (isInvul || magicImmnune || isInvis || isChannel || dpActivated)) continue;

                    string spellString;
                    if ((blink != null && blink.Cooldown > 11) || forcestaff != null && forcestaff.Cooldown > 18.6)
                    {
                        UseDisableStageOne(v, enumerable,false);
                    }
                    else if (Activated)
                    {
                        UseDisableStageOne(v, enumerable, false);
                    }
                    else if (Ititiators.TryGetValue(v.ClassID, out spellString))
                    {
                        var initSpell = v.FindSpell(spellString);
                        if (initSpell != null && initSpell.Cooldown != 0)
                        {
                            UseDisableStageOne(v, enumerable, false);
                        }
                    }
                    if (isStun || isHex || isSilence || isDisarm) continue;
                    if ((blink != null && blink.Cooldown > 11) || forcestaff != null && forcestaff.Cooldown > 18.6)
                    {
                        UseDisableStageOne(v, enumerable,true);
                    }
                    else if (Activated)
                    {
                        UseDisableStageOne(v, enumerable, true);
                    }
                    else if (Ititiators.TryGetValue(v.ClassID, out spellString))
                    {
                        var initSpell = v.FindSpell(spellString);
                        if (initSpell != null && initSpell.Cooldown != 0)
                        {
                            UseDisableStageOne(v, enumerable, true);
                        }
                    }

                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        private static void UseDisableStageOne(Hero target, IEnumerable<Item> items,bool stage)
        {
            if (!(_me.Health / _me.MaximumHealth > 0.1)) return;
            Item disable;
            if (stage)
            {
                disable =
                    items.FirstOrDefault( // wo puck_waning_rift
                        x =>
                            x.Name == "lion_voodoo" || x.Name == "item_sheepstick" || x.Name == "item_orchid" ||
                            x.Name == "item_abyssal_blade" || x.Name == "shadow_shaman_voodoo" ||
                            x.Name == "obsidian_destroyer_astral_imprisonment" ||
                            x.Name == "shadow_demon_disruption" ||
                            x.Name == "rubick_telekinesis" ||
                            x.Name == "dragon_knight_dragon_tail" ||
                            x.Name == "batrider_flaming_lasso" ||
                            x.Name == "legion_commander_duel" ||
                            x.Name == "skywrath_mage_ancient_seal" ||
                            x.Name == "item_ethereal_blade" ||
                            x.Name == "item_rod_of_atos" || x.Name == "item_heavens_halberd" ||
                            x.Name == "item_medallion_of_courage" ||
                            x.Name == "item_cyclone" || x.Name == "item_solar_crest");
            }
            else
            {
                disable = items.FirstOrDefault(x => x.Name == "item_rod_of_atos" || x.Name == "item_medallion_of_courage" || x.Name == "item_solar_crest");
            }
            
            if (disable != null && _me.Distance2D(target) <= disable.CastRange)
            {
                disable.UseAbility(target);
                Utils.Sleep(250, target.GetHashCode().ToString());
            }
        }


        #endregion

        private static void Game_OnUpdate(EventArgs args)
        {
            if (!_loaded)
            {
                _me = ObjectMgr.LocalHero;
                _player = ObjectMgr.LocalPlayer;
                if (!Game.IsInGame || _me == null)
                {
                    return;
                }
                _loaded = true;
                PrintSuccess(string.Format("> AutoDisable Loaded v{0}", Ver)); 
            }
            if (Game.IsInGame && _me != null) return;
            _loaded = false;
            PrintInfo("> AutoDisable unLoaded");
        }

        #region Helpers

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
