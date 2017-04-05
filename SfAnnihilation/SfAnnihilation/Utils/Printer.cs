using System.Reflection;
using Ensage;
using log4net;
using PlaySharp.Toolkit.Logging;

namespace SfAnnihilation.Utils
{
    internal class Printer
    {
        private static readonly ILog Logger = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static void Print(string s)
        {
            if (MenuManager.DebugInGame)
                Game.PrintMessage(s);
        }
        public static void Log(object s)
        {
            if (MenuManager.DebugInConsole)
                Logger.Debug(s);
        }
        public static void Both(object s)
        {
            if (MenuManager.DebugInConsole)
                Logger.Debug(s);
            if (MenuManager.DebugInGame)
                Game.PrintMessage(s.ToString());
        }
    }
}