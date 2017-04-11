using System.Linq;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Common.Objects.UtilityObjects;

namespace OverlayInformation
{
    internal static class AutoItems
    {
        private static Item _midas;
        private static Item _phase;
        private static Item _stick;
        private static Sleeper _sleeper;

        public static void Flush()
        {
            _sleeper = new Sleeper();
        }

        public static void Action()
        {
            if (!Members.Menu.Item("autoitems.Enable").GetValue<bool>()) return;
            if (_sleeper.Sleeping || Members.MyHero.IsInvisible() || !Members.MyHero.IsAlive) return;
            _sleeper.Sleep(250);
            if (Members.Menu.Item("autoitems.List").GetValue<AbilityToggler>().IsEnabled("item_hand_of_midas"))
            {
                if (_midas == null || !_midas.IsValid)
                {
                    _midas = Members.MyHero.FindItem("item_hand_of_midas");
                }
                if (_midas!=null && _midas.CanBeCasted())
                {
                    var creep = ObjectManager.GetEntities<Unit>()
                            .Where(
                                x =>
                                    !x.IsMagicImmune() && x.Team != Members.MyHero.Team &&
                                    (x.ClassId == ClassId.CDOTA_BaseNPC_Creep_Lane ||
                                     x.ClassId == ClassId.CDOTA_BaseNPC_Creep_Siege ||
                                     x.ClassId == ClassId.CDOTA_BaseNPC_Creep_Neutral) && x.IsSpawned && x.IsAlive &&
                                    x.Distance2D(Members.MyHero) <= 600 && !x.IsAncient).OrderByDescending(x => x.Health)
                            .DefaultIfEmpty(null)
                            .FirstOrDefault();
                    if (creep != null)
                        _midas.UseAbility(creep);
                }
            }
            if (Members.Menu.Item("autoitems.List").GetValue<AbilityToggler>().IsEnabled("item_phase_boots"))
            {
                if (_phase == null || !_phase.IsValid)
                    _phase = Members.MyHero.FindItem("item_phase_boots");
                if (_phase != null && _phase.CanBeCasted() && !Members.MyHero.IsAttacking() &&
                    !Members.MyHero.IsChanneling() && Members.MyHero.NetworkActivity == NetworkActivity.Move)
                {
                    _phase.UseAbility();
                }
            }
            if (Members.Menu.Item("autoitems.List").GetValue<AbilityToggler>().IsEnabled("item_magic_wand"))
            {
                if (_stick == null || !_stick.IsValid)
                    _stick = Members.MyHero.FindItem("item_magic_wand") ?? Members.MyHero.FindItem("item_magic_stick");
                if (_stick != null && _stick.CanBeCasted() && _stick.CurrentCharges > 0 && (Helper.CheckForPercents(Members.MyHero) || Helper.CheckForPercents(Members.MyHero,false)))
                {
                    _stick.UseAbility();
                }
            }
        }
    }
}