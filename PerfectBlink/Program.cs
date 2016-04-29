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
            Game.OnUpdate += Game_OnUpdate;
            PrintSuccess(string.Format("> {1} Loaded v{0}", Assembly.GetExecutingAssembly().GetName().Version, Menu.DisplayName));
            Game.PrintMessage(
                    "<font face='Comic Sans MS, cursive'><font color='#00aaff'>" + Menu.DisplayName + " By Jumpering" +
                    " loaded!</font> <font color='#aa0000'>v" + Assembly.GetExecutingAssembly().GetName().Version,
                    MessageType.LogMessage);
            Menu.AddItem(new MenuItem("PB.Enable", "Enable")).SetValue(true);
            Menu.AddToMainMenu();
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (!Menu.Item("PB.Enable").GetValue<bool>()) return;
            if (!_shouldCheckForModifier || _me==null ||!_me.IsValid) return;
            var mod = _me.HasModifier("modifier_teleporting");
            if (mod) return;
            _shouldCheckForModifier = false;
            var safeRange = _me.FindItem("item_aether_lens") == null ? 1200 : 1400;
            var tpos = _me.Position;
            var a = tpos.ToVector2().FindAngleBetween(_mySelectedPos, true);
            safeRange -= (int)_me.HullRadius;
            var p = new Vector3(
                tpos.X + safeRange * (float)Math.Cos(a),
                tpos.Y + safeRange * (float)Math.Sin(a),
                100);
            _myAbility.UseAbility(p);
        }

        private static Hero _me;
        private static Ability _myAbility;
        private static Vector2 _mySelectedPos;
        private static bool _shouldCheckForModifier;

        private static void Player_OnExecuteAction(Player sender, ExecuteOrderEventArgs args)
        {
            if (!Menu.Item("PB.Enable").GetValue<bool>()) return;
            /*if (args.Order == Order.AbilityTarget || args.Order == Order.AbilityLocation)
                if (args.Ability.Name == "item_tpscroll" || args.Ability.Name == "item_travel_boots" ||
                    args.Ability.Name == "item_travel_boots_2")
                    TpPos = args.TargetPosition;*/
            if (args.Order == Order.Stop || args.Order == Order.Hold)
                _shouldCheckForModifier = false;
            if (args.Order != Order.AbilityLocation) return;
            if (args.Ability.Name != "item_blink") return;
            _me = args.Entities.FirstOrDefault() as Hero;//ObjectMgr.LocalHero);
            if (_me==null) return;
            var safeRange = _me.FindItem("item_aether_lens") == null ? 1200 : 1400;
            if (!(_me.Distance2D(args.TargetPosition) > safeRange))
                return;
            var tpos = _me.Position;
            var a = tpos.ToVector2().FindAngleBetween(args.TargetPosition.ToVector2(), true);
            
            safeRange -= (int)_me.HullRadius;
            var p = new Vector3(
                tpos.X + safeRange * (float)Math.Cos(a),
                tpos.Y + safeRange * (float)Math.Sin(a),
                100);
            if (_me.HasModifier("modifier_teleporting"))
            {
                _shouldCheckForModifier = true;
                _myAbility = args.Ability;
                args.Process = false;
                _mySelectedPos = args.TargetPosition.ToVector2();
                /*DelayAction.Add(new DelayActionItem((int) _me.FindModifier("modifier_teleporting").RemainingTime*1000+750, () =>
                {
                    tpos = _me.Position;;//TpPos;
                    a = tpos.ToVector2().FindAngleBetween(args.TargetPosition.ToVector2(), true);
                    p = new Vector3(
                        tpos.X + safeRange * (float)Math.Cos(a),
                        tpos.Y + safeRange * (float)Math.Sin(a),
                        100);
                    Print("extra action");
                    
                    args.Ability.UseAbility(p);
                }, CancellationToken.None));*/
                return;
            }
            _shouldCheckForModifier = false;
            args.Ability.UseAbility(p,_me.IsChanneling());
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

        private static void Print(string s)
        {
            Game.PrintMessage(s,MessageType.ChatMessage);
        }
    }
}
