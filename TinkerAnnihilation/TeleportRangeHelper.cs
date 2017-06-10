using System.Threading;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using SharpDX;

namespace TinkerAnnihilation
{
    internal class TeleportRangeHelper
    {
        private static bool _inTp;
        public static void UnitOnOnModifierRemoved(Unit sender, ModifierChangedEventArgs args)
        {
            var name = args.Modifier.Name;
            if (name.Contains("teleporting"))
            {
                if (sender.ClassId == ClassId.CDOTA_Unit_Hero_Tinker)
                {
                    DelayAction.Add(new DelayActionItem(100, () =>
                    {
                        _teleportCaster = null;
                        _inTp = false;
                    }, CancellationToken.None));

                }
            }
            else if (name.Contains("boots"))
            {
                if (_teleportCaster != null)
                {
                    Members.TowerRangEffectHelper.RemoveEffect(sender);
                    Members.TowerRangEffectHelper2.RemoveEffect(sender);
                }
            }
        }

        private static Unit _teleportCaster;

        public static void Unit_OnModifierAdded(Unit sender, ModifierChangedEventArgs args)
        {
            if (_inTp)
                return;
            var name = args.Modifier.Name;
            if (name.Contains("teleporting"))
            {
                //Printer.Print($"{sender.Name}: teleporting");
                if (sender.ClassId == ClassId.CDOTA_Unit_Hero_Tinker)
                {
                    _teleportCaster = sender;
                }
            }
            else if (name.Contains("modifier_boots_of_travel_incoming"))
            {
                //Printer.Print($"{sender.Name}: boots");
                if (sender.Team==Members.MyTeam)
                DelayAction.Add(new DelayActionItem(100, () =>
                {
                    if (_teleportCaster != null)
                    {
                        var blink = Members.MyHero.FindItem("item_blink", true);
                        if (blink != null)
                        {
                            var effect =
                                sender.AddParticleEffect("materials/ensage_ui/particles/range_display_mod.vpcf");
                            var lense = Members.MyHero.FindItem("item_aether_lens", true);
                            var range = 1150 + (lense != null ? 200 : 0);
                            effect.SetControlPoint(0, sender.Position);
                            effect.SetControlPoint(1, new Vector3(range, 255, 0));
                            effect.SetControlPoint(2, new Vector3(255, 255, 255));
                            Members.TowerRangEffectHelper.AddEffect(sender, effect);
                            //if (sender is Building)
                            //{
                                effect =
                                    sender.AddParticleEffect("materials/ensage_ui/particles/line.vpcf");
                                var frontPoint = Helper.InFront(Members.MyHero, sender, range);
                                effect.SetControlPoint(1, sender.Position);
                                effect.SetControlPoint(2, frontPoint);
                                effect.SetControlPoint(3, new Vector3(255, 50, 0));
                                effect.SetControlPoint(4, new Vector3(255, 255, 255));
                                Members.TowerRangEffectHelper2.AddEffect(sender, effect,range);
                            //}
                            _inTp = true;
                        }
                    }
                }, CancellationToken.None));
            }
        }
    }
}