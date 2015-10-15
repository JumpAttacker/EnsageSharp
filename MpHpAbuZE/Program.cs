using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using SharpDX;


namespace MpHpAbuZE
{
    class Program
    {

        #region Members

        private static bool _loaded;
        private static Hero _me;
        private static Player _player;
        private const int TangoRange = 450;
        private const int QbladeRange = 350;
        public static bool Drop { get; set; }
        #endregion

        #region Methods

        private static void Main()
        {
            Game.OnUpdate += Game_OnUpdate;
            _loaded = false;
            Game.OnWndProc += Game_OnWndProc;
        }
        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (args.WParam != 'V' || Game.IsChatOpen)
            {
                return;
            }
            Drop = args.Msg != (ulong) Utils.WindowsMessages.WM_KEYUP;
            //PrintInfo(Drop.ToString());
        }

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
                PrintSuccess("> MpHpArbus Loaded");
            }

            if (!Game.IsInGame || _me == null)
            {
                _loaded = false;
                PrintInfo("> MpHpArbus unLoaded");
                return;
            }

            if (Game.IsPaused || !Utils.SleepCheck(_me.Handle.ToString()))
            {
                return;
            }

            if (_player == null || _player.Team == Team.Observer || !Drop)
                return;
            var items = _me.Inventory.Items.Where(
                    x =>
                        x.Name != "item_soul_ring" && x.Name != "item_arcane_boots");
            var inInvis = _me.IsInvisible();
            var inStun = _me.IsStunned();
            var isChannel = _me.IsChanneling();
            var arcaneBoots = _me.Inventory.Items.FirstOrDefault(x => x.Name == "item_arcane_boots");
            var soulRing = _me.Inventory.Items.FirstOrDefault(x => x.Name == "item_soul_ring");
            if (inStun || inInvis || isChannel) return;
            //PrintError("-----------------");
            foreach (var s in items)
            {
                _me.DropItem(s,_me.NetworkPosition);
            }
            if (arcaneBoots != null && arcaneBoots.CanBeCasted())
            {
                arcaneBoots.UseAbility();
                _me.DropItem(arcaneBoots, _me.NetworkPosition);
            }
            if (soulRing != null && soulRing.CanBeCasted())
            {
                soulRing.UseAbility();
            }
            var forPick = ObjectMgr.GetEntities<PhysicalItem>();/*.Where(
                x =>
                    x.Distance2D(_me.NetworkPosition) <= 150);*/
            PrintInfo(forPick.Count().ToString());
            foreach (var s in forPick)
            {
                PrintInfo(s.Name);
                _me.PickUpItem(s);
            }
            Utils.Sleep(250, _me.Handle.ToString());
        }

        #endregion

        #region Helpers
        private static float GetDistance2D(Vector3 p1, Vector3 p2)
        {
            return (float)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }
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
