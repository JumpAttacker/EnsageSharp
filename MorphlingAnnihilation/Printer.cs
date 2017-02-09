using System;
using Ensage;

namespace MorphlingAnnihilation
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

        public static void Print(string str)
        {
            if (MenuManager.IsEnableDebugger)
                Game.PrintMessage(str);
        }
        
        #endregion
    }
}