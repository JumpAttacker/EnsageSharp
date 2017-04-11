using System;
using Ensage;

namespace TemplarAnnihilation
{
    internal static class Printer
    {
        #region Helpers
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
        #endregion
    }
}