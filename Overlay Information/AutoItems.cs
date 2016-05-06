using System.Drawing.Text;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using Ensage.Common.Objects.UtilityObjects;

namespace OverylayInformationV2
{
    internal static class AutoItems
    {
        private static Item _midas;
        private static Item _phase;
        private static readonly Sleeper Sleeper=new Sleeper();

        public static void Action()
        {
            if (!Members.Menu.Item("autoitems.Enable").GetValue<bool>()) return;
            if (Sleeper.Sleeping || Members.MyHero.IsInvisible() || !Members.MyHero.IsAlive) return;
            Sleeper.Sleep(250);
            if (Members.Menu.Item("autoitems.List").GetValue<AbilityToggler>().IsEnabled("item_hand_of_midas"))
            {
                if (_midas == null || !_midas.IsValid)
                {
                    _midas = Members.MyHero.FindItem("item_hand_of_midas");
                }
                if (_midas!=null && _midas.CanBeCasted())
                {
                    var creep = Creeps.All.Where(x => x != null && x.IsValid && x.IsAlive && x.Distance2D(Members.MyHero) <= 600).OrderByDescending(y=>y.Health).FirstOrDefault();
                    if (creep != null)
                        _midas.UseAbility(creep);
                }
            }
            if (Members.Menu.Item("autoitems.List").GetValue<AbilityToggler>().IsEnabled("item_phase_boots"))
            {
                if (_phase == null || !_phase.IsValid)
                    _phase = Members.MyHero.FindItem("item_phase_boots");
                if (_phase != null && _phase.CanBeCasted() && !Members.MyHero.IsAttacking() && !Members.MyHero.IsChanneling() && Members.MyHero.NetworkActivity == NetworkActivity.Move)
                {
                    _phase.UseAbility();
                }
            }
        }
    }
}