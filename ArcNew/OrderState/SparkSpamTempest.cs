using System.Threading.Tasks;
using ArcAnnihilation.Units;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using SharpDX;

namespace ArcAnnihilation.OrderState
{
    public class SparkSpamTempest : Order
    {
        public override bool CanBeExecuted => MenuManager.SparkSpamTempestOnlyCombo.GetValue<KeyBind>().Active;
        private static async Task Spammer(UnitBase unit)
        {
            var pos = Game.MousePosition;
            while (true)
            {
                if (unit.IsAlive)
                {
                    CastSpark(unit.Hero, pos);
                }
                await Task.Delay(500, Core.ComboToken.Token);
            }
        }
        private static void CastSpark(Hero me, Vector3 pos)
        {
            var spark = me.GetAbilityById(AbilityId.arc_warden_spark_wraith);
            if (spark.CanBeCasted())
            {
                spark.UseAbility(pos);
            }
        }

        public override void Execute()
        {
            if (Core.TempestHero != null && Core.TempestHero.CanCallCombo)
                Core.TempestHero.ExTask(Spammer(Core.TempestHero));
        }
    }
}