using System;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;

namespace SfEulCombo
{
    class Program
    {

        #region Members
        private const int WmKeyup = 0x0101;
        private static bool _loaded;
        private const string Ver = "1.0";
        private static bool _enabled;

        #endregion


        private static void Main()
        {
            Game.OnUpdate += Game_OnUpdate;
            Game.OnWndProc += Game_OnWndProc;
            _loaded = false;
        }
        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (args.WParam != 'F' || Game.IsChatOpen)
                return;
            _enabled = args.Msg != WmKeyup;
            Game.ExecuteCommand(string.Format("dota_player_units_auto_attack_after_spell {0}", _enabled?0:1));
        }
        private static void Game_OnUpdate(EventArgs args)
        {
            Hero me = ObjectMgr.LocalHero;
            Player player = ObjectMgr.LocalPlayer;
            if (!_loaded)
            {
                if (!Game.IsInGame || me == null){return;}
                _loaded = true;
                PrintSuccess(string.Format("> Sf eul combo v{0}", Ver));
            }

            if (!Game.IsInGame || me == null)
            {
                _loaded = false;
                PrintInfo("> Sf eul combo unLoaded");
                return;
            }

            if (Game.IsPaused || !Utils.SleepCheck("delay"))
            {
                return;
            }

            if (player == null || player.Team == Team.Observer || !_enabled)
            {
                return;
            }
            var eul = me.FindItem("item_cyclone");
            var dagger = me.FindItem("item_blink");
            var ultimate = me.Spellbook.Spell6;
            if (ultimate == null || !ultimate.CanBeCasted()) return;
            var target = ClosestToMouse(me);
            if (target == null) return;
            if (eul.CanBeCasted() && me.Distance2D(target)<=eul.CastRange)
            {
                eul.UseAbility(target);
                Utils.Sleep(100, "delay");
                return;
            }
            var eulmodif = target.Modifiers.FirstOrDefault(x => x.Name == "modifier_eul_cyclone");
            if(eulmodif==null)return;
            if (dagger.CanBeCasted() && me.Distance2D(target) <= 1200)
            {
                dagger.UseAbility(target.Position);
                Utils.Sleep(200, "delay");
                return;
            }
            /*if (me.Distance2D(target)/me.MovementSpeed<=0.8)
            {
                me.Move(target.Position);
                Utils.Sleep(200, "delay");
            }*/

            if (ultimate.CanBeCasted())
            {
                if (eulmodif.RemainingTime < 1.67 + Game.Ping/1000)
                {
                    ultimate.UseAbility();
                    Utils.Sleep(200, "delay");
                }
                else if (Utils.SleepCheck("moving") && me.Distance2D(target)>=50)
                {
                    var phase = me.FindItem("item_phase_boots");
                    if (phase != null && phase.CanBeCasted()) { phase.UseAbility();}
                    me.Move(target.NetworkPosition);
                    Utils.Sleep(500, "moving");
                }
            }
        }
        public static Hero ClosestToMouse(Hero source, float range = 1000)
        {
            var mousePosition = Game.MousePosition;
            var enemyHeroes =
                ObjectMgr.GetEntities<Hero>()
                    .Where(
                        x =>
                            x.Team == source.GetEnemyTeam() && !x.IsIllusion && x.IsAlive && x.IsVisible
                            && x.Distance2D(mousePosition) <= range && !x.IsMagicImmune());
            Hero[] closestHero = { null };
            foreach (var enemyHero in enemyHeroes.Where(enemyHero => closestHero[0] == null || closestHero[0].Distance2D(mousePosition) > enemyHero.Distance2D(mousePosition)))
            {
                closestHero[0] = enemyHero;
            }
            return closestHero[0];
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
