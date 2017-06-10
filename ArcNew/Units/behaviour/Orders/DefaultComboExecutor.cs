using System.Threading;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.Common.Threading;
using SharpDX;

namespace ArcAnnihilation.Units.behaviour.Orders
{
    public class DefaultComboExecutor : ComboExecutor
    {
        private readonly UnitBase _myBase;

        public DefaultComboExecutor(UnitBase myBase)
        {
            _myBase = myBase;
        }

        public override async Task ExecuteAsync(CancellationToken token = new CancellationToken())
        {
            while (OrderManager.CanBeExecuted)
            {
                if (!_myBase.Hero.IsAlive || _myBase.Hero.IsInvisible())
                    return;
                /*await _myBase.TargetFinder(token);
                await _myBase.UseItems(token);
                await _myBase.UseAbilities(token);*/
                await Await.Delay(25, token);
            }
        }
    }

    public class SparkSpamComboExecutor : ComboExecutor
    {
        private readonly UnitBase _myBase;

        public SparkSpamComboExecutor(UnitBase myBase)
        {
            _myBase = myBase;
        }

        public override async Task ExecuteAsync(CancellationToken token = new CancellationToken())
        {
            while (OrderManager.CanBeExecuted)
            {
                if (!_myBase.Hero.IsAlive || _myBase.Hero.IsInvisible())
                    return;
                await Spammer();
                await Await.Delay(25, token);
            }
        }

        private async Task Spammer()
        {
            var pos = Game.MousePosition;
            while (true)
            {
                if (_myBase.IsAlive)
                {
                    CastSpark(_myBase.Hero, pos);
                }
                await Task.Delay(500, Core.ComboToken.Token);
            }
        }

        private void CastSpark(Hero me, Vector3 pos)
        {
            var spark = me.GetAbilityById(AbilityId.arc_warden_spark_wraith);
            if (spark.CanBeCasted())
            {
                spark.UseAbility(pos);
            }
        }
    }
}