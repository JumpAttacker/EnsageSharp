using System;
using System.Reflection;
using Ensage;
using log4net;
using PlaySharp.Toolkit.Logging;
using SharpDX;

namespace TemplarAnnihilation
{
    internal static class Printer
    {
        private static readonly ILog Logger = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        #region Helpers
        public static string PrintVector(this Vector3 vec)
        {
            return $"new Vector3({(int)vec.X},{(int)vec.Y},{(int)vec.Z}),";

        }
        public static string PrintVector(this Vector2 vec)
        {
            return $"({vec.X};{vec.Y})";
        }
        public static void PrintInfo(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.White, arguments); ;
        }

        public static void PrintSuccess(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.Green, arguments);
        }

        public static void PrintError(string text, params object[] arguments)
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

        public static void Print(string str, bool print = false)
        {
            if (print || Members.Menu.Item("Dev.Text.enable").GetValue<bool>())
                Game.PrintMessage(str);
        }

        public static void Both(object s, bool print = false)
        {
            if (print || Members.Menu.Item("Dev.Console.enable").GetValue<bool>())
                Logger.Debug(s);
            if (print || Members.Menu.Item("Dev.Text.enable").GetValue<bool>())
                Game.PrintMessage(s.ToString());
        }
        #endregion
    }
}