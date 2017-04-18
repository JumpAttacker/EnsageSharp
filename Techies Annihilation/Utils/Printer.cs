using System.Reflection;
using Ensage;
using log4net;
using PlaySharp.Toolkit.Logging;

namespace Techies_Annihilation.Utils
{
    internal class Printer
    {
        private static readonly ILog Logger = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static void Print(string s, bool print = false)
        {
            if (MenuManager.DebugInGame || print)
                Game.PrintMessage(s);
        }
        public static void Log(object s, bool print = false)
        {
            if (MenuManager.DebugInConsole || print)
                Logger.Debug(s);
        }
        public static void Both(object s, bool print=false)
        {
            if (MenuManager.DebugInConsole || print)
                Logger.Debug(s);
            if (MenuManager.DebugInGame || print)
                Game.PrintMessage(s.ToString());
        }
    }
}