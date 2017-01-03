using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using Ensage.Common.Objects.UtilityObjects;
using SharpDX;

namespace OverlayInformation
{
    public static class ShrineHelper
    {
        private static bool Enable => Members.Menu.Item("shrineHelper.Range").GetValue<bool>();
        private static int R => Members.Menu.Item("shrineHelper.Red").GetValue<Slider>().Value;
        private static int G => Members.Menu.Item("shrineHelper.Green").GetValue<Slider>().Value;
        private static int B => Members.Menu.Item("shrineHelper.Blue").GetValue<Slider>().Value;
        private static int Alpha => Members.Menu.Item("shrineHelper.Alpha").GetValue<Slider>().Value;
        private static bool _firstTime = true;
        private const ClassID SrineClass = ClassID.CDOTA_BaseNPC_Healer;
        private static List<Unit> _shrineList=new List<Unit>();
        private static readonly Dictionary<Unit, ParticleEffect> Effects = new Dictionary<Unit, ParticleEffect>();
        private static Sleeper _sleeper = new Sleeper();
        public static void Init()
        {
            Effects.Clear();
            _shrineList.Clear();
            _sleeper=new Sleeper();
            if (_firstTime)
            {
                _firstTime = false;
                _shrineList =
                    ObjectManager.GetEntities<Unit>()
                        .Where(x => x.IsValid && x.IsAlive && x.ClassID == SrineClass && x.Team == Members.MyPlayer.Team)
                        .ToList();
                Game.OnUpdate += args =>
                {
                    if (_sleeper.Sleeping)
                        return;
                    _sleeper.Sleep(100);
                    if (Enable)
                    {
                        foreach (var v in _shrineList)
                        {
                            var dist = v.Distance2D(Members.MyHero);
                            if (dist <= 700 &&
                                v.Spellbook.Spells.Any(
                                    x =>
                                        x.StoredName() == "filler_ability" &&
                                        (x.AbilityState == AbilityState.Ready || x.Cooldown >= 295)))
                            {
                                HandleEffect(v);
                            }
                            else
                            {
                                UnHandleEffect(v);
                            }
                        }
                    }
                    else
                    {
                        if (Effects.Any())
                        {
                            Effects.ToDictionary(x => x.Key, y => y.Value).ForEach(x => UnHandleEffect(x.Key));
                        }
                    }
                };
                ObjectManager.OnRemoveEntity += args =>
                {
                    var shrine = args.Entity;
                    if (shrine.ClassID == SrineClass)
                        _shrineList.Remove(shrine as Unit);
                };
            }
        }

        private static void UnHandleEffect(Unit unit)
        {
            ParticleEffect effect;
            if (Effects.TryGetValue(unit, out effect))
            {
                effect.Dispose();
            }
            Effects.Remove(unit);
        }

        private static void HandleEffect(this Unit unit)
        {
            ParticleEffect effect;
            if (!Effects.TryGetValue(unit, out effect))
            {
                //effect=unit.AddParticleEffect("materials/ensage_ui/particles/drag_selected_ring_mod.vpcf");
                effect=unit.AddParticleEffect("materials/ensage_ui/particles/range_display_mod.vpcf");
                effect.SetControlPoint(1, new Vector3(596, Alpha, 0));
                effect.SetControlPoint(2, new Vector3(R, G, B));
                /*effect.SetControlPoint(1, new Vector3(0, 155, 255));
                effect.SetControlPoint(2, new Vector3(500, 255, 255));
                effect.SetControlPoint(3, new Vector3(0, 255, 0));*/
                Effects.Add(unit,effect);
            }
        }

        public static void OnChange(object sender, OnValueChangeEventArgs e)
        {
            foreach (var effect in Effects.Values)
            {
                effect.SetControlPoint(1, new Vector3(596, Alpha, 0));
                effect.SetControlPoint(2, new Vector3(R, G, B));
            }

        }
    }
}
