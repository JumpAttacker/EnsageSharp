using System;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;

namespace Auto_Dust
{
    class Program
    {

        #region Members

        private static bool _loaded;
        private static Hero _me;
        private static Player _player;
        private const string Ver = "1.1";
        private const int Range = 1000;

        #endregion


        private static void Main()
        {
            Game.OnUpdate += Game_OnUpdate;
            _loaded = false;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (!_loaded)
            {
                _me = ObjectManager.LocalHero;
                _player = ObjectManager.LocalPlayer;
                if (!Game.IsInGame || _me == null)
                {
                    return;
                }
                _loaded = true;
                PrintSuccess($"> Auto Dust Loaded v{Ver}"); 
            }

            if (!Game.IsInGame || _me == null)
            {
                _loaded = false;
                PrintInfo("> Auto Dust unLoaded");
                return;
            }

            if (Game.IsPaused || !Utils.SleepCheck("delay"))
            {
                return;
            }

            if (_player == null || _player.Team == Team.Observer)
                return;
            var dust = _me.FindItem("item_dust");
            if (dust==null|| !dust.CanBeCasted() || _me.IsInvisible())return;
            var enemy = ObjectManager.GetEntities<Hero>()
                .Where(
                    v =>
                        !v.IsIllusion && v.Team != _player.Team && v.IsAlive && v.IsVisible &&
                        _me.Distance2D(v) <= Range);
            foreach (var v in enemy)
            {
                if (v.Modifiers.Any(
                    x =>
                        (x.Name == "modifier_bounty_hunter_wind_walk" ||
                         x.Name == "modifier_riki_permanent_invisibility" ||
                         x.Name == "modifier_mirana_moonlight_shadow" || x.Name == "modifier_treant_natures_guise" ||
                         x.Name == "modifier_weaver_shukuchi" ||
                         x.Name == "modifier_broodmother_spin_web_invisible_applier" ||
                         x.Name == "modifier_item_invisibility_edge_windwalk" || x.Name == "modifier_rune_invis" ||
                         x.Name == "modifier_clinkz_wind_walk" || x.Name == "modifier_item_shadow_amulet_fade" ||
                         x.Name == "modifier_bounty_hunter_track" || x.Name == "modifier_bloodseeker_thirst_vision" ||
                         x.Name == "modifier_slardar_amplify_damage" || x.Name == "modifier_item_dustofappearance") ||
                        x.Name == "modifier_invoker_ghost_walk_enemy"))
                {
                    dust.UseAbility();
                    Utils.Sleep(250, "delay");
                }

                if ((v.Name == ("npc_dota_hero_templar_assassin") || v.Name == ("npc_dota_hero_sand_king")) &&
                    (v.Health/v.MaximumHealth < 0.3))
                {
                    dust.UseAbility();
                    Utils.Sleep(250, "delay");

                }
                if (v.Name != ("npc_dota_hero_nyx_assassin") || !v.Spellbook.Spell4.CanBeCasted()) continue;
                dust.UseAbility();
                Utils.Sleep(250, "delay");
            }
        }


        #region Helpers

        public static void PrintInfo(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.White, arguments);
        }

        public static void PrintSuccess(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.Green, arguments);
        }

        public static void PrintError(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.Red, arguments);
        }

        public static void PrintEncolored(string text, ConsoleColor color, params object[] arguments)
        {
            var clr = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text, arguments);
            Console.ForegroundColor = clr;
        }

        #endregion

    }
}
