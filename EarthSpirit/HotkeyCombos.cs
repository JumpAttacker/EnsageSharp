using System;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using Ensage.Common.Objects.UtilityObjects;
using SharpDX;

namespace EarthAn
{
    internal class HotkeyCombos
    {
        private static bool IsEnable => Members.Menu.Item("Enable").GetValue<bool>();
        private static bool Combo1 => Members.Menu.Item("HotKeyCombo.1.Enable").GetValue<KeyBind>().Active;
        private static bool Combo2 => Members.Menu.Item("HotKeyCombo.2.Enable").GetValue<KeyBind>().Active;
        private static bool Combo3 => Members.Menu.Item("HotKeyCombo.3.Enable").GetValue<KeyBind>().Active;
        private static bool Combo4 => Members.Menu.Item("HotKeyCombo.4.Enable").GetValue<KeyBind>().Active;
        private static MultiSleeper _combos;
        public static void Game_OnUpdate(EventArgs args)
        {
            if (!IsEnable)
                return;
            if (_combos==null)
                _combos=new MultiSleeper();
            var smash = Abilities.FindAbility("earth_spirit_boulder_smash");
            var geomagneticGrip = Abilities.FindAbility("earth_spirit_geomagnetic_grip");
            var magnetize = Abilities.FindAbility("earth_spirit_magnetize");
            var rollingBoulder = Abilities.FindAbility("earth_spirit_rolling_boulder");
            var stoneCaller = Abilities.FindAbility("earth_spirit_stone_caller");
            var petrify = Abilities.FindAbility("earth_spirit_petrify");
            if (Combo1)
            {
                if (smash.CanBeCasted() && !_combos.Sleeping(smash))
                {
                    var remnant = Helper.FindRemnant();
                    if (stoneCaller.CanBeCasted() && remnant == null)
                        stoneCaller.UseAbility(Prediction.InFront(Members.MyHero, 75));
                    //stoneCaller.UseAbility(Members.MyHero.Position);
                    smash.UseAbility(Game.MousePosition);
                    _combos.Sleep(500, smash);
                }
            }
            else if (Combo2)
            {
                if (rollingBoulder.CanBeCasted() && !_combos.Sleeping(rollingBoulder))
                {
                    var myPos = Members.MyHero.Position;
                    var mousePos = Game.MousePosition;

                    var angle = Members.MyHero.FindAngleBetween(mousePos, true);
                    var point = new Vector3(
                        (float)
                            (myPos.X +
                             75 *
                             Math.Cos(angle)),
                        (float)
                            (myPos.Y +
                             75 *
                             Math.Sin(angle)),
                        0);
                    var remnant = Helper.FindRemnant(range:100);
                    if (stoneCaller.CanBeCasted() && remnant == null)
                        stoneCaller.UseAbility(point);
                    rollingBoulder.UseAbility(Game.MousePosition);
                    _combos.Sleep(500, rollingBoulder);
                }
            }
            else if (Combo3)
            {
                var myPos = Members.MyHero.Position;
                var mousePos = Game.MousePosition;
                var distance = myPos.Distance2D(mousePos);
                var myInventory = Members.MyHero.Inventory.Items.ToList();
                var extraRange = myInventory.Any(x => x.StoredName() == "item_aether_lens") ? 200 : 0;
                if (geomagneticGrip.CanBeCasted() && !_combos.Sleeping(geomagneticGrip) &&
                    distance <= 1100 + extraRange)
                {
                    var remnant = Helper.FindRemnant(mousePos);
                    if (stoneCaller.CanBeCasted() && remnant == null)
                    {
                        stoneCaller.UseAbility(mousePos);
                        geomagneticGrip.UseAbility(mousePos);
                    }
                    else
                    {
                        geomagneticGrip.UseAbility(remnant.NetworkPosition);
                    }

                    _combos.Sleep(500, geomagneticGrip);
                }
            }
            if (Combo4)
            {
                var myPos = Members.MyHero.Position;
                var mousePos = Game.MousePosition;
                var distance = myPos.Distance2D(mousePos);
                var myInventory = Members.MyHero.Inventory.Items.ToList();
                var extraRange = myInventory.Any(x => x.StoredName() == "item_aether_lens") ? 200 : 0;
                /*Printer.Print(
                    $"{rollingBoulder.CanBeCasted()}/{geomagneticGrip.CanBeCasted()}/{!_combos.Sleeping(stoneCaller)}/{distance <= 1000 + extraRange}");*/
                if (rollingBoulder.CanBeCasted() && geomagneticGrip.CanBeCasted() && !_combos.Sleeping(stoneCaller) &&
                    distance <= 11000 + extraRange)
                {
                    var remnant = Helper.FindRemnant(mousePos);
                    if (remnant != null)
                    {
                        geomagneticGrip.UseAbility(remnant.NetworkPosition);
                        rollingBoulder.UseAbility(mousePos);
                        _combos.Sleep(500, stoneCaller);
                    }
                }
            }

        }
    }
}