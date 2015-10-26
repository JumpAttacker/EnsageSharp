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
                        else if (_activated)
                        {
                            UseDisableStageOne(v, enumerable, null, false);
                        }
                        else if (Initiators.TryGetValue(v.ClassID, out spellString))
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
                            UsaSafeItems(v, enumerable, abilities);
                        }
                    }
                    if ((blink != null && blink.Cooldown > 11) || forcestaff != null && forcestaff.Cooldown > 18.6)
                    {
                        UseDisableStageOne(v, enumerable, abilities, true);
                    }
                    else if (_activated)
                    {
                        UseDisableStageOne(v, enumerable, abilities, true);
                    }
                    else if (Initiators.TryGetValue(v.ClassID, out spellString))
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

        #region Members

        private static bool _loaded;
        private static Hero _me;
        private static Player _player;
        private static bool _activated;
        private static bool _autoBlink;
        private const string Ver = "1.4";
        private static readonly Dictionary<ClassID, string> Initiators = new Dictionary<ClassID, string>();
        //static readonly Dictionary<ClassID, string> CounterSpells = new Dictionary<ClassID, string>();
        private static Font _infoText;
        private static readonly CounterHelp[] CounterSpells = new CounterHelp[50];

        #endregion

        #region Methods

        #region Init

        private static void Main()
        {
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
            _infoText = new Font(
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
            _infoText.Dispose();
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
            var sign = _activated
                ? "Auto Disable: for all | [INSERT] for toggle."
                : "Auto Disable: for initiators | [INSERT] for toggle.";
            var daggerOn = _autoBlink
                ? " Auto Dagger [on] to fountain | [DEL] for toggle"
                : " Auto Dagger [off] to fountain | [DEL] for toggle";
            _infoText.DrawText(null, sign + daggerOn, 5, 150, Color.YellowGreen);
        }

        private static void Drawing_OnPostReset(EventArgs args)
        {
            _infoText.OnResetDevice();
        }

        private static void Drawing_OnPreReset(EventArgs args)
        {
            _infoText.OnLostDevice();
        }

        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (Game.IsChatOpen || args.Msg != (ulong) Utils.WindowsMessages.WM_KEYUP) return;
            switch (args.WParam)
            {
                case 0x2D:
                    _activated = !_activated;
                    break;
                case 0x2E:
                    _autoBlink = !_autoBlink;
                    break;
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Game.IsInGame || _me == null || !_loaded) return;
        }

        private static void UsaSafeItems(Hero target, IEnumerable<Item> items, IEnumerable<Ability> abilities)
        {
            var enumerable = items as Item[] ?? items.ToArray();
            var safeItemSelf = enumerable.FirstOrDefault(
                x =>
                    x.Name == "item_blade_mail" /*|| x.Name == "item_black_king_bar" || x.Name == "item_ghost" ||
                        x.Name == "item_manta"*/);
            var safeItemTargetSelf = enumerable.FirstOrDefault(
                x =>
                    x.Name == "item_lotus_orb" || x.Name == "item_cyclone" || x.Name == "item_glimmer_cape");
            var safeItemTargetEnemy = enumerable.FirstOrDefault(
                x =>
                    x.Name == "item_sheepstick" || x.Name == "item_orchid" || x.Name == "item_abyssal_blade" ||
                    x.Name == "item_heavens_halberd");
            var safeItemPoint = enumerable.FirstOrDefault(
                x =>
                    x.Name == "item_blink");


            var safeSpell = abilities.FirstOrDefault(
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
                    /* x.Name == "winter_wyvern_cold_embrace" ||*/ x.Name == "life_stealer_rage" || x.Name == "silencer_global_silence");
            /*local v = entityList:GetEntities({classId = CDOTA_Unit_Fountain,team = me.team})[1]
				local vec = Vector((v.position.x - me.position.x) * 1100 / GetDistance2D(v,me) + me.position.x,(v.position.y - me.position.y) * 1100 / GetDistance2D(v,me) + me.position.y,v.position.z)
				if blinkfront then
					vec = Vector(me.position.x+1200*math.cos(me.rotR), me.position.y+1200*math.sin(me.rotR), me.position.z)
				end*/
            var v =
                ObjectMgr.GetEntities<Unit>()
                    .FirstOrDefault(x => x.Team == _me.Team && x.ClassID == ClassID.CDOTA_Unit_Fountain);
            if (safeItemSelf != null)
            {
                safeItemSelf.UseAbility();
                Utils.Sleep(250, target.GetHashCode().ToString());
                return;
            }
            //PrintError(String.Format("{0} {1}", SafeSpell.CastRange,SafeSpell.Name));
            if (safeItemPoint != null && _autoBlink)
            {
                if (v != null)
                {
                    safeItemPoint.UseAbility(v.Position);
                    Utils.Sleep(250, target.GetHashCode().ToString());
                    return;
                }
            }
            if (safeItemTargetEnemy != null && safeItemTargetEnemy.CanBeCasted(target))
            {
                if (_me.Distance2D(target) <= safeItemTargetEnemy.CastRange)
                {
                    safeItemTargetEnemy.UseAbility(target);
                    Utils.Sleep(250, target.GetHashCode().ToString());
                    return;
                }
            }
            if (safeItemTargetSelf != null)
            {
                safeItemTargetSelf.UseAbility(_me);
                Utils.Sleep(250, target.GetHashCode().ToString());
                return;

            }

            if (safeSpell != null && safeSpell.CanBeCasted())
            {
                if (safeSpell.CastRange > 0)
                {
                    if (_me.Distance2D(target) <= safeSpell.CastRange)
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
        }

        private static void UseDisableStageOne(Hero target, IEnumerable<Item> items, IEnumerable<Ability> abilities,
            bool stage)
        {
            if (_me != null && !(_me.Health/_me.MaximumHealth > 0.1)) return;
            Item disable;
            Ability ab = null;
            Ability withOutTarget = null;
            var enumerable = items as Item[] ?? items.ToArray();
            if (stage)
            {
                disable =
                    enumerable.FirstOrDefault( // wo puck_waning_rift
                        x =>
                            x.Name == "item_sheepstick" || x.Name == "item_orchid" ||
                            x.Name == "item_abyssal_blade" ||
                            x.Name == "item_ethereal_blade" ||
                            x.Name == "item_rod_of_atos" || x.Name == "item_heavens_halberd" ||
                            x.Name == "item_medallion_of_courage" ||
                            x.Name == "item_cyclone" || x.Name == "item_solar_crest");
                var enumerable1 = abilities as Ability[] ?? abilities.ToArray();
                ab =
                    enumerable1.FirstOrDefault(
                        x =>
                            x.Name == "lion_voodoo" || x.Name == "shadow_shaman_voodoo" ||
                            x.Name == "obsidian_destroyer_astral_imprisonment" || x.Name == "shadow_demon_disruption" ||
                            x.Name == "rubick_telekinesis" ||
                            x.Name == "dragon_knight_dragon_tail" ||
                            x.Name == "batrider_flaming_lasso" ||
                            x.Name == "legion_commander_duel" ||
                            x.Name == "skywrath_mage_ancient_seal");
                withOutTarget =
                    enumerable1.FirstOrDefault(
                        x =>
                            x.Name == "silencer_global_silence");
            }
            else
            {
                disable =
                    enumerable.FirstOrDefault(
                        x =>
                            x.Name == "item_rod_of_atos" || x.Name == "item_medallion_of_courage" ||
                            x.Name == "item_solar_crest");
            }

            if (disable != null && _me.Distance2D(target) <= disable.CastRange)
            {
                disable.UseAbility(target);
                Utils.Sleep(250, target.GetHashCode().ToString());
                return;
            }
            if (ab != null && _me.Distance2D(target) <= ab.CastRange)
            {
                ab.UseAbility(target);
                Utils.Sleep(250, target.GetHashCode().ToString());
                return;
            }
            if (withOutTarget != null)
            {
                withOutTarget.UseAbility();
                Utils.Sleep(250, target.GetHashCode().ToString());
                return;
            }
            var safeItemPoint =
                enumerable.FirstOrDefault(
                    x =>
                        x.Name == "item_blink");
            var v =
                ObjectMgr.GetEntities<Unit>()
                    .FirstOrDefault(x => x.Team == _me.Team && x.ClassID == ClassID.CDOTA_Unit_Fountain);
            if (safeItemPoint != null && _autoBlink)
            {
                if (v != null)
                {
                    safeItemPoint.UseAbility(v.Position);
                }
                Utils.Sleep(250, target.GetHashCode().ToString());
            }
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
            catch (Exception e)
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