using System.Reflection;
using Ensage;
using log4net;
using PlaySharp.Toolkit.Logging;

namespace ArcAnnihilation.Utils
{
    internal class Printer
    {
        private static readonly ILog Logger = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static void Print(string s, bool print = false)
        {
            if (print || MenuManager.DebugInGame)
                Game.PrintMessage(s);
        }
        public static void Log(object s, bool print = false)
        {
            if (print || MenuManager.DebugInConsole)
                Logger.Debug(s);
        }
        public static void Both(object s, bool print=false)
        {
            if (print || MenuManager.DebugInConsole)
                Logger.Debug(s);
            if (print || MenuManager.DebugInGame)
                Game.PrintMessage(s.ToString());
        }
    }
}