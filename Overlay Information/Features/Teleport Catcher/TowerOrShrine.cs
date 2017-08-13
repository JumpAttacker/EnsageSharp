using System.Reflection;
using Ensage;
using Ensage.Common;
using log4net;
using PlaySharp.Toolkit.Logging;

namespace OverlayInformation.Features.Teleport_Catcher
{
    public class TowerOrShrine
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public Unit Unit { get; }
        public bool IsAlive => Unit.IsAlive;
        public int TpCounter { get; private set; }
        public Team Team;
        public TowerOrShrine(Unit unit)
        {
            Unit = unit;
            TpCounter = 0;
            Team = unit.Team;
            //Log.Debug($"new. {unit.Name} {unit.ClassId} isAlly: {IsAlly}");
        }

        public void Inc()
        {
            TpCounter++;
            //Log.Info($"Counter -> {TpCounter} [inc]");
            DelayAction.Add(25000, () =>
            {
                TpCounter--;
                //Log.Info($"Counter -> {TpCounter} [dec]");
            });
        }

        public float CalculateLifeTime()
        {
            var time = TpCounter == 1 ? 3 : 4 + 0.5f * TpCounter;
            
            return time;
        }
    }
}