using System;
using System.Linq;
using System.Reflection;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using SharpDX;

namespace PerfectBlink
{
    internal static class Program
    {
        private static readonly Menu Menu=new Menu("Perfect Blink","PerfectBlink",true,"item_blink",true);
        private static void Main()
        {
            Player.OnExecuteOrder += Player_OnExecuteAction;
            PrintSuccess(string.Format("> {1} Loaded v{0}", Assembly.GetExecutingAssembly().GetName().Version, Menu.DisplayName));
            Game.PrintMessage(
                    "<font face='Comic Sans MS, cursive'><font color='#00aaff'>" + Menu.DisplayName + " By Jumpering" +
                    " loaded!</font> <font color='#aa0000'>v" + Assembly.GetExecutingAssembly().GetName().Version,
                    MessageType.LogMessage);
            Menu.AddItem(new MenuItem("PB.Enable", "Enable")).SetValue(true);
            Menu.AddToMainMenu();
        }

        private static void Player_OnExecuteAction(Player sender, ExecuteOrderEventArgs args)
        {
            if (!Menu.Item("PB.Enable").GetValue<bool>()) return;
            if (args.Order == Order.AbilityTarget || args.Order == Order.AbilityLocation)
                if (args.Ability.Name == "item_tpscroll" || args.Ability.Name == "item_travel_boots" ||
                    args.Ability.Name == "item_travel_boots_2")
                    TpPos = args.TargetPosition;
                    
            if (args.Order != Order.AbilityLocation) return;

            if (args.Ability.Name != "item_blink") return;
            var me = args.Entities.FirstOrDefault() as Hero;//ObjectMgr.LocalHero);
            if (me==null) return;
            var safeRange = me.FindItem("item_aether_lens") == null ? 1200 : 1400;
            if (!(me.Distance2D(args.TargetPosition) > safeRange))
                return;
            var tpos = me.Position;
            var a = tpos.ToVector2().FindAngleBetween(args.TargetPosition.ToVector2(), true);
            
            safeRange -= (int)me.HullRadius;
            var p = new Vector3(
                tpos.X + safeRange * (float)Math.Cos(a),
                tpos.Y + safeRange * (float)Math.Sin(a),
                100);
            if (me.Modifiers.Any(x => x.Name == "modifier_teleporting"))
            {
                tpos = TpPos;
                a = tpos.ToVector2().FindAngleBetween(args.TargetPosition.ToVector2(), true);
                p = new Vector3(
                    tpos.X + safeRange * (float)Math.Cos(a),
                    tpos.Y + safeRange * (float)Math.Sin(a),
                    100);
            }
            args.Ability.UseAbility(p,me.IsChanneling());
            args.Process = false;
        }

        private static Vector3 TpPos { get; set; }


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
