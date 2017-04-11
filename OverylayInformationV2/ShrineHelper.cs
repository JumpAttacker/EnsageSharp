using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Enums;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Common.Objects.UtilityObjects;
using SharpDX;
using AbilityId = Ensage.Common.Enums.AbilityId;

namespace OverlayInformation
{
    public static class ShrineHelper
    {
        private static bool Enable => Members.Menu.Item("shrineHelper.Range").GetValue<bool>();
        private static bool IsNumsEnable => Members.Menu.Item("shrineHelper.Nums.Enable").GetValue<bool>();
        private static float DigSize => (float)Members.Menu.Item("shrineHelper.Nums.Size").GetValue<Slider>().Value / 100;
        private static float BarSize => (float)Members.Menu.Item("shrineHelper.Size").GetValue<Slider>().Value;
        private static bool Draw => Members.Menu.Item("shrineHelper.DrawStatus").GetValue<bool>();
        private static int R => Members.Menu.Item("shrineHelper.Red").GetValue<Slider>().Value;
        private static int G => Members.Menu.Item("shrineHelper.Green").GetValue<Slider>().Value;
        private static int B => Members.Menu.Item("shrineHelper.Blue").GetValue<Slider>().Value;
        private static int Alpha => Members.Menu.Item("shrineHelper.Alpha").GetValue<Slider>().Value;
        private static bool _firstTime = true;
        private const ClassId SrineClass = ClassId.CDOTA_BaseNPC_Healer;
        private static List<Unit> _shrineList=new List<Unit>();
        private static readonly Dictionary<Unit, ParticleEffect> Effects = new Dictionary<Unit, ParticleEffect>();
        private static Sleeper _sleeper = new Sleeper();
        private static Dictionary<uint, Ability> _abilityDictinart;

        private static bool CheckForAbility(this Unit v)
        {
            Ability s;
            var handle = v.Handle;
            if (_abilityDictinart.TryGetValue(handle, out s))
                return s?.AbilityState == AbilityState.Ready || s?.Cooldown >= 295;
            s = v.Spellbook.Spells.FirstOrDefault(x => x.GetAbilityId() == AbilityId.filler_ability);
            if (s!=null)
                _abilityDictinart.Add(handle,s);
            return s != null && (s.AbilityState == AbilityState.Ready || s.Cooldown >= 295);
        }

        private static Ability GetFiller(this Unit v)
        {
            Ability s;
            var handle = v.Handle;
            if (_abilityDictinart.TryGetValue(handle, out s)) return s;
            s = v.Spellbook.Spells.FirstOrDefault(x => x.GetAbilityId() == AbilityId.filler_ability);
            _abilityDictinart.Add(handle, s);
            return s;
        }

        public static void Init()
        {
            Effects.Clear();
            _shrineList.Clear();
            _sleeper = new Sleeper();
            _abilityDictinart = new Dictionary<uint, Ability>();
            if (_firstTime)
            {
                _firstTime = false;
                _shrineList =
                    ObjectManager.GetEntities<Unit>()
                        .Where(x => x.IsValid && x.IsAlive && x.ClassId == SrineClass && x.Team == Members.MyPlayer.Team)
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

                            if (dist <= 700 && v.CheckForAbility())
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
                Drawing.OnDraw += args =>
                {
                    if (Draw || IsNumsEnable)
                        foreach (var v in _shrineList)
                        {
                            var pos = HUDInfo.GetHPbarPosition(v);
                            if (pos.IsZero)
                                continue;
                            var filler = v.GetFiller();
                            if (filler == null || filler.AbilityState == AbilityState.Ready)
                                continue;
                            var cd = filler.Cooldown;
                            var cdLength = filler.CooldownLength;
                            var hpBarSize = HUDInfo.GetHPBarSizeX();
                            var size = new Vector2(hpBarSize*2, BarSize);
                            var buff = v.FindModifier("modifier_filler_heal_aura");
                            var isBuff = buff != null;
                            var remTine = buff?.RemainingTime;
                            var cdDelta = isBuff ? buff.RemainingTime*size.X/5 : cd*size.X/cdLength;
                            pos += new Vector2(-hpBarSize/2, hpBarSize*1.5f);
                            if (Draw)
                            {
                                Drawing.DrawRect(pos, new Vector2(size.X, size.Y), Color.Black);
                                Drawing.DrawRect(pos, new Vector2(isBuff ? cdDelta : size.X - cdDelta, size.Y),
                                    isBuff ? Color.Orange : Color.YellowGreen);
                                Drawing.DrawRect(pos, new Vector2(size.X, size.Y), Color.Black, true);
                            }
                            if (IsNumsEnable)
                            {
                                var text = isBuff ? $"{(int) (remTine/5*100)}%" : $"{(int) (100 - cd/cdLength*100)}%";
                                var textSize = Drawing.MeasureText(text, "Arial",
                                    new Vector2((float) (size.Y*DigSize), size.Y/2), FontFlags.AntiAlias);
                                var textPos = pos + new Vector2(size.X/2 - textSize.X/2, size.Y - textSize.Y);
                                /*Drawing.DrawRect(textPos - new Vector2(0, 0),
                                new Vector2(textSize.X, textSize.Y),
                                new Color(0, 0, 0, 200));*/
                                Drawing.DrawText(
                                    text,
                                    textPos,
                                    new Vector2(textSize.Y, 0),
                                    Color.White,
                                    FontFlags.AntiAlias | FontFlags.StrikeOut);
                            }
                        }
                };
                ObjectManager.OnRemoveEntity += args =>
                {
                    var shrine = args.Entity;
                    if (shrine.ClassId == SrineClass)
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
                effect.SetControlPoint(1, new Vector3(500, Alpha, 0));
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
