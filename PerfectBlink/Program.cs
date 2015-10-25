using System;
using Ensage;
using Ensage.Common.Extensions;
using SharpDX;

namespace PerfectBlink
{
    class Program
    {
        private const string Ver = "1.0";

        static void Main()
        {
            Player.OnExecuteOrder += Player_OnExecuteAction;
            PrintSuccess(string.Format("> Perfect Blink Loaded v{0}", Ver));
        }
        static void Player_OnExecuteAction(Player sender, ExecuteOrderEventArgs args)
        {
            if (args.Order != Order.AbilityLocation) return;
            if (args.Ability.Name != "item_blink") return;
            var me = ObjectMgr.LocalHero;
            if (!(me.Distance2D(args.TargetPosition) > 1200)) return;
            var tpos = me.Position;
            var a = tpos.ToVector2().FindAngleBetween(args.TargetPosition.ToVector2(), true);
            var p = new Vector3(
                tpos.X + 1150 * (float)Math.Cos(a),
                tpos.Y + 1150 * (float)Math.Sin(a),
                100);
            args.Ability.UseAbility(p);
            args.Process = false;
        }
        private static void PrintInfo(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.White, arguments);
        }

        private static void PrintSuccess(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.Green, arguments);
        }

        // ReSharper disable once UnusedMember.Local
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
    }
}
