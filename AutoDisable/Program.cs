using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using SharpDX;
using SharpDX.Direct3D9;

// ReSharper disable EmptyGeneralCatchClause
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace Auto_Disable
{
    internal class Program
    {
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
            if (!Game.IsInGame || _me == null)
            {
                _loaded = false;
                PrintInfo("> AutoDisable unLoaded");
                return;
            }

            uint i;
            for (i = 0; i < 10; i++)
            {
                try
                {
                    var v = ObjectMgr.GetPlayerById(i).Hero;
                    if (v == null || v.Team == _me.Team || Equals(v, _me) || !v.IsAlive || !v.IsVisible ||
                        !Utils.SleepCheck(v.GetHashCode().ToString())) continue;
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
                    var spells = _me.Spellbook.Spells.Where(x => x.CanBeCasted());
                    var blink = v.FindItem("item_blink");
                    var forcestaff = v.FindItem("item_force_staff");
                    var dpActivated =
                        v.Modifiers.Any(
                            x => x.Name == "modifier_slark_dark_pact" || x.Name == "modifier_slark_dark_pact_pulses");
                    var enumerable = items as IList<Item> ?? items.ToList();
                    var distance = _me.Distance2D(v);
                    //var angle =
                    string spellString;
                    var angle = (float) (Math.Max(
                        Math.Abs(v.RotationRad - Utils.DegreeToRadian(v.FindAngleBetween(_me.Position))) - 0.20, 0));

                    if (!enumerable.Any() || (isInvul || magicImmnune || isInvis || isChannel || dpActivated))
                    {
                        if ((blink != null && blink.Cooldown > 11) || forcestaff != null && forcestaff.Cooldown > 18.6)
                        {
                            UseDisableStageOne(v, enumerable, null, false);
                        }
                        else if (Activated)
                        {
                            UseDisableStageOne(v, enumerable, null, false);
                        }
                        else if (Ititiators.TryGetValue(v.ClassID, out spellString))
                        {
                            var initSpell = v.FindSpell(spellString);
                            if (initSpell != null && initSpell.Cooldown != 0)
                            {
                                UseDisableStageOne(v, enumerable, null, false);
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
                        Ability r = null;
                        Ability r2 = null;
                        r = CheckForFirstSpell(v);
                        r2 = CheckForSecondSpell(v);
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
                            UsaSafeItems(v, enumerable, abilities);
                        }
                    }
                    if ((blink != null && blink.Cooldown > 11) || forcestaff != null && forcestaff.Cooldown > 18.6)
                    {
                        UseDisableStageOne(v, enumerable, abilities, true);
                    }
                    else if (Activated)
                    {
                        UseDisableStageOne(v, enumerable, abilities, true);
                    }
                    else if (Ititiators.TryGetValue(v.ClassID, out spellString))
                    {
                        var initSpell = v.FindSpell(spellString);
                        if (initSpell != null && initSpell.Cooldown != 0)
                        {
                            UseDisableStageOne(v, enumerable, abilities, true);
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
            public readonly string _strKey;
            public readonly int _intKey;
            public readonly ClassID _hero;
            public readonly float _extraRange;
            public readonly NetworkActivity _activity;
            public readonly string _modifer;

            public CounterHelp(ClassID hero, string strKey = "", int intKey = 0, float extraRange = 0,
                NetworkActivity activity = 0, string modifer = "")
                : this()
            {
                _hero = hero;
                _modifer = modifer;
                _activity = activity;
                _intKey = intKey;
                _strKey = strKey;
                _extraRange = extraRange;
            }
        }

        #region Members

        private static bool _loaded;
        private static Hero _me;
        private static Player _player;
        private static bool Activated;
        private const float Ver = (float) 1.3;
        private static readonly Dictionary<ClassID, string> Ititiators = new Dictionary<ClassID, string>();
        //static readonly Dictionary<ClassID, string> CounterSpells = new Dictionary<ClassID, string>();
        private static Font text;
        private static readonly CounterHelp[] CounterSpells = new CounterHelp[50];

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
            CounterSpells[c] = new CounterHelp(ClassID.CDOTA_Unit_Hero_Lion, "r", 1);
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
            //int Spell = _me.Spellbook.Spells.FirstOrDefault(ability => ability.ClassID==CounterSpells.TryGetValue())
            text = new Font(
                Drawing.Direct3DDevice9,
                new FontDescription
                {
                    FaceName = "Tahoma",
                    Height = 13,
                    OutputPrecision = FontPrecision.Default,
                    Quality = FontQuality.Default
                });
            Game.OnUpdate += Game_OnUpdate;
            _loaded = false;
            //Drawing.OnDraw += Drawing_OnDraw;

            Drawing.OnPreReset += Drawing_OnPreReset;
            Drawing.OnPostReset += Drawing_OnPostReset;
            Drawing.OnEndScene += Drawing_OnEndScene;
            AppDomain.CurrentDomain.DomainUnload += CurrentDomainDomainUnload;
            Game.OnWndProc += Game_OnWndProc;
        }

        #endregion

        private static void CurrentDomainDomainUnload(object sender, EventArgs e)
        {
            text.Dispose();
        }

        //private static float angle;
        private static void Drawing_OnEndScene(EventArgs args)
        {
            if (Drawing.Direct3DDevice9 == null || Drawing.Direct3DDevice9.IsDisposed || !Game.IsInGame)
            {
                return;
            }

            var player = ObjectMgr.LocalPlayer;
            if (player == null || player.Team == Team.Observer)
            {
                return;
            }
            var sign = Activated
                ? "Auto Disable: for all | [INSERT] for toggle"
                : "Auto Disable: for initiators | [INSERT] for toggle";
            text.DrawText(null, sign, 5, 150, Color.YellowGreen);
        }

        private static void Drawing_OnPostReset(EventArgs args)
        {
            text.OnResetDevice();
        }

        private static void Drawing_OnPreReset(EventArgs args)
        {
            text.OnLostDevice();
        }

        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (!Game.IsChatOpen && args.Msg == (ulong) Utils.WindowsMessages.WM_KEYUP && args.WParam == 0x2D)
            {
                Activated = !Activated;
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Game.IsInGame || _me == null || !_loaded) return;
        }

        private static void UsaSafeItems(Hero target, IEnumerable<Item> items, IEnumerable<Ability> abilities)
        {
            Item SafeItemSelf = null;
            Item SafeItemTargetSelf = null;
            Item SafeItemTargetEnemy = null;
            Item SafeItemPoint = null;
            Ability SafeSpell = null;
            var enumerable = items as Item[] ?? items.ToArray();
            SafeItemSelf =
                enumerable.FirstOrDefault(
                    x =>
                        x.Name == "item_blade_mail" /*|| x.Name == "item_black_king_bar" || x.Name == "item_ghost" ||
                        x.Name == "item_manta"*/);
            SafeItemTargetSelf =
                enumerable.FirstOrDefault(
                    x =>
                        x.Name == "item_lotus_orb" || x.Name == "item_cyclone" || x.Name == "item_glimmer_cape");
            SafeItemTargetEnemy =
                enumerable.FirstOrDefault(
                    x =>
                        x.Name == "item_sheepstick" || x.Name == "item_orchid" || x.Name == "item_abyssal_blade" ||
                        x.Name == "item_heavens_halberd");
            SafeItemPoint =
                enumerable.FirstOrDefault(
                    x =>
                        x.Name == "item_blink");


            SafeSpell =
                abilities.FirstOrDefault(
                    x =>
                        /*x.Name == "antimage_blink" || x.Name == "queenofpain_blink" ||*/
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
                       /* x.Name == "winter_wyvern_cold_embrace" ||*/ x.Name == "life_stealer_rage");
            /*local v = entityList:GetEntities({classId = CDOTA_Unit_Fountain,team = me.team})[1]
				local vec = Vector((v.position.x - me.position.x) * 1100 / GetDistance2D(v,me) + me.position.x,(v.position.y - me.position.y) * 1100 / GetDistance2D(v,me) + me.position.y,v.position.z)
				if blinkfront then
					vec = Vector(me.position.x+1200*math.cos(me.rotR), me.position.y+1200*math.sin(me.rotR), me.position.z)
				end*/
            var v =
                ObjectMgr.GetEntities<Unit>()
                    .FirstOrDefault(x => x.Team == _me.Team && x.ClassID == ClassID.CDOTA_Unit_Fountain);

            if (SafeItemSelf != null)
            {
                SafeItemSelf.UseAbility();
                Utils.Sleep(250, target.GetHashCode().ToString());
            }
            PrintError(String.Format("{0} {1}", SafeSpell.CastRange,SafeSpell.Name));
            /*if (SafeItemPoint != null)
            {
                if (v != null)
                {
                    SafeItemPoint.UseAbility(v.Position);
                }
                Utils.Sleep(250, target.GetHashCode().ToString());
            }*/
            if (SafeItemTargetEnemy != null && SafeItemTargetEnemy.CanBeCasted(target))
            {
                if (_me.Distance2D(target) <= SafeItemTargetEnemy.CastRange)
                {
                    SafeItemTargetEnemy.UseAbility(target);
                    Utils.Sleep(250, target.GetHashCode().ToString());
                }
            }
            if (SafeItemTargetSelf != null)
            {
                SafeItemTargetSelf.UseAbility(_me);
                Utils.Sleep(250, target.GetHashCode().ToString());
            }

            if (SafeSpell != null && SafeSpell.CanBeCasted())
            {
                if (SafeSpell.CastRange > 0)
                {
                    if (_me.Distance2D(target) <= SafeSpell.CastRange)
                    {
                        SafeSpell.UseAbility(target);
                    }
                }
                else
                {
                    SafeSpell.UseAbility();
                }
                Utils.Sleep(250, target.GetHashCode().ToString());
            }
        }

        private static void UseDisableStageOne(Hero target, IEnumerable<Item> items, IEnumerable<Ability> abilities,
            bool stage)
        {
            if (!(_me.Health/_me.MaximumHealth > 0.1)) return;
            Item disable;
            Ability ab = null;
            if (stage)
            {
                disable =
                    items.FirstOrDefault( // wo puck_waning_rift
                        x =>
                            x.Name == "item_sheepstick" || x.Name == "item_orchid" ||
                            x.Name == "item_abyssal_blade" ||
                            x.Name == "item_ethereal_blade" ||
                            x.Name == "item_rod_of_atos" || x.Name == "item_heavens_halberd" ||
                            x.Name == "item_medallion_of_courage" ||
                            x.Name == "item_cyclone" || x.Name == "item_solar_crest");
                ab =
                    abilities.FirstOrDefault(
                        x =>
                            x.Name == "lion_voodoo" || x.Name == "shadow_shaman_voodoo" ||
                            x.Name == "obsidian_destroyer_astral_imprisonment" || x.Name == "shadow_demon_disruption" ||
                            x.Name == "rubick_telekinesis" ||
                            x.Name == "dragon_knight_dragon_tail" ||
                            x.Name == "batrider_flaming_lasso" ||
                            x.Name == "legion_commander_duel" ||
                            x.Name == "skywrath_mage_ancient_seal" || x.Name == "silencer_global_silence");
            }
            else
            {
                disable =
                    items.FirstOrDefault(
                        x =>
                            x.Name == "item_rod_of_atos" || x.Name == "item_medallion_of_courage" ||
                            x.Name == "item_solar_crest");
            }

            if (disable != null && _me.Distance2D(target) <= disable.CastRange)
            {
                disable.UseAbility(target);
                Utils.Sleep(250, target.GetHashCode().ToString());
            }
            if (ab != null && _me.Distance2D(target) <= ab.CastRange)
            {
                ab.UseAbility(target);
                Utils.Sleep(250, target.GetHashCode().ToString());
            }
        }

        #endregion

        #region Helpers

        private static NetworkActivity CheckForNetworkAct(Unit t)
        {
            for (var n = 0; n < CounterSpells.Length; n++)
                if (t.ClassID == CounterSpells[n]._hero)
                {
                    if (CounterSpells[n]._activity != 0)
                    {
                        return CounterSpells[n]._activity;
                    }
                }
            return 0;
        }

        private static string CheckForModifier(Unit t, ref bool mustHave)
        {
            for (var n = 0; n < CounterSpells.Length; n++)
                if (t.ClassID == CounterSpells[n]._hero)
                {
                    if (CounterSpells[n]._modifer != "")
                    {
                        mustHave = true;
                        return CounterSpells[n]._modifer;
                    }
                }
            return "";
        }

        private static Ability CheckForFirstSpell(Unit t)
        {
            try
            {
                for (var n = 0; n < CounterSpells.Length; n++)
                    if (t.ClassID == CounterSpells[n]._hero)
                    {
                        switch (CounterSpells[n]._strKey)
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
            catch (Exception e)
            {
                PrintError(e.ToString());
            }
            return null;
        }

        private static Ability CheckForSecondSpell(Unit t)
        {
            try
            {
                for (var n = 0; n < CounterSpells.Length; n++)
                    if (t.ClassID == CounterSpells[n]._hero)
                    {
                        //PrintInfo("Hero catched: " + CounterSpells[n]._intKey);
                        switch (CounterSpells[n]._intKey)
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
            catch (Exception e)
            {
                PrintError(e.ToString());
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