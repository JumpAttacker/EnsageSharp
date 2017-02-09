using Ensage;
using Ensage.Common.Extensions;
using SharpDX;

namespace MorphlingAnnihilation
{
    public static class Helper
    {
        public static int GetAbilityDelay(this Hero me,Unit target, Ability ability)
        {
            return (int)((ability.FindCastPoint() + me.GetTurnTime(target)) * 1000.0 + Game.Ping);
        }

        public static int GetAbilityDelay(this Hero me, Vector3 targetPosition, Ability ability)
        {
            return (int)((ability.FindCastPoint() + me.GetTurnTime(targetPosition)) * 1000.0 + Game.Ping);
        }
    }
}