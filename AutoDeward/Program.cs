using System;
using System.Linq;
using Ensage;
using SharpDX;
using Ensage.Common;
using Ensage.Common.Extensions;

namespace AutoDeward
{
    class Program
    {

        #region Members

        private static bool _loaded;
        private static Hero _me;
        private static Player _player;
        private const int TangoRange=450;
        private const int QbladeRange = 350;

        #endregion

        #region Methods

        private static void Main()
        {
            Game.OnUpdate += Game_OnUpdate;
            _loaded = false;
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
                PrintSuccess("> AutoDeward Loaded");
            }

            if (!Game.IsInGame || _me == null)
            {
                _loaded = false;
                PrintInfo("> AutoDeward unLoaded");
                return;
            }

            if (Game.IsPaused || !Utils.SleepCheck(_me.Handle.ToString()))
            {
                return;
            }

            if (_player == null || _player.Team == Team.Observer)
                return;
            
            if (!_me.IsAlive)
                return;

            var wards = ObjectMgr.GetEntities<Unit>()
                .Where(
                    x =>
                        (x.ClassID == ClassID.CDOTA_NPC_Observer_Ward ||
                         x.ClassID == ClassID.CDOTA_NPC_Observer_Ward_TrueSight)
                        && x.Team != _player.Team && GetDistance2D(x.NetworkPosition, _me.NetworkPosition) < TangoRange &&
                        x.IsVisible && x.IsAlive);
            var enumerable = wards as Unit[] ?? wards.ToArray();
            if (!enumerable.Any()) return;
            var tango =
                _me.Inventory.Items.FirstOrDefault(
                    x => x.ClassID == ClassID.CDOTA_Item_Tango || x.ClassID == ClassID.CDOTA_Item_Tango_Single);
            var qblade = _me.Inventory.Items.FirstOrDefault(x => x.ClassID == ClassID.CDOTA_Item_QuellingBlade);
            if (qblade != null && qblade.CanBeCasted() &&
                GetDistance2D(enumerable.First().NetworkPosition, _me.NetworkPosition) < QbladeRange)
            {
                qblade.UseAbility(enumerable.First());
            }
            else if (tango != null && tango.CanBeCasted())
            {
                tango.UseAbility(enumerable.First());
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
