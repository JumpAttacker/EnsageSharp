using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using Ensage.Common.Objects.UtilityObjects;
using SharpDX;

namespace OverlayInformation
{
    internal static class ShowMeMore
    {
        private static readonly Sleeper Sleeper=new Sleeper();
        private static Unit AAunit { get; set; }
        private static readonly List<Unit> InSys = new List<Unit>();
        private static readonly Dictionary<Unit, ParticleEffect[]> Eff = new Dictionary<Unit, ParticleEffect[]>();
        private static Unit _arrowUnit;
        private static bool _letsDraw=true;
        private static Vector3 _arrowS;
        private static readonly ParticleEffect[] ArrowParticalEffects = new ParticleEffect[150];

        public static void ShowIllustion()
        {
            if (!Members.Menu.Item("showillusion.Enable").GetValue<bool>()) return;
            if (Sleeper.Sleeping) return;
            Sleeper.Sleep(300);
            var illusions = ObjectManager.GetEntities<Hero>()
                .Where(x => x.IsValid && x.IsIllusion).ToList();
            foreach (var s in illusions)
                Helper.HandleEffect(s);
        }
        
        public static void ShowMeMoreSpells()
        {
            if (!Members.Menu.Item("showmemore.Enable").GetValue<bool>()) return;
            //Printer.Print(Manager.BaseManager.GetBaseList().Count.ToString());
            //Manager.BaseManager.GetBaseList().ForEach(x=>Printer.Print(x.Handle+": "+x.DayVision));
            if (Members.Apparition)
            {
                foreach (var t in Manager.BaseManager.GetBaseList().Where(t => t.DayVision == 550).Where(t => !Members.AAlist.Contains(t.Handle)))
                {
                    Members.AAlist.Add(t.Handle);
                    AAunit = t;
                    Helper.GenerateSideMessage("ancient_apparition", "ancient_apparition_ice_blast");
                }
            }
            if (!Members.Menu.Item("showmemore.Enable").GetValue<bool>()) return;
            if (Members.Windrunner != null)
            {
                DrawForWr(Members.Windrunner);
            }
            if (Members.Mirana != null)
            {
                DrawForMirana(Members.Mirana);
            }
        }

        private static void DrawForMirana(Hero mirana)
        {
            if (_arrowUnit == null)
            {
                _arrowUnit =
                    Manager.BaseManager.GetBaseList()
                        .Find(x => x.DayVision == 650 && x.Team == Members.MyHero.GetEnemyTeam());
            }
            if (_arrowUnit != null)
            {
                if (!_arrowUnit.IsValid)
                {
                    foreach (var effect in ArrowParticalEffects.Where(effect => effect != null))
                    {
                        effect.Dispose();
                    }
                    _letsDraw = true;
                    _arrowUnit =
                        Manager.BaseManager.GetBaseList()
                            .Find(x => x.DayVision == 650 && x.Team == Members.MyHero.GetEnemyTeam());
                    return;
                }
                if (!InSys.Contains(_arrowUnit))
                {
                    _arrowS = _arrowUnit.Position;
                    InSys.Add(_arrowUnit);
                    Utils.Sleep(100, "kek");
                    Helper.GenerateSideMessage(mirana.StoredName().Replace("npc_dota_hero_", ""), "mirana_arrow");
                }
                else if (_letsDraw && Utils.SleepCheck("kek") && _arrowUnit.IsVisible)
                {
                    _letsDraw = false;
                    var ret = Helper.FindRet(_arrowS, _arrowUnit.Position);
                    for (var z = 1; z <= 147; z++)
                    {
                        var p = Helper.FindVector(_arrowS, ret, 20 * z + 60);
                        ArrowParticalEffects[z] = new ParticleEffect(
                            @"particles\ui_mouseactions\draw_commentator.vpcf", p);
                        ArrowParticalEffects[z].SetControlPoint(1,
                            new Vector3(Members.Menu.Item("mirana.Red").GetValue<Slider>().Value,
                                Members.Menu.Item("mirana.Green").GetValue<Slider>().Value,
                                Members.Menu.Item("mirana.Blue").GetValue<Slider>().Value));
                        ArrowParticalEffects[z].SetControlPoint(0, p);
                    }
                }
            }
        }

        private static void DrawForWr(Hero v)
        {
            if (Prediction.IsTurning(v)) return;
            var spell = v.Spellbook.Spell2;
            if (spell == null || spell.Cooldown == 0) return;
            var cd = Math.Floor(spell.Cooldown * 100);
            if (!(cd < 880)) return;
            if (!InSys.Contains(v))
            {
                if (cd > 720)
                {
                    var eff = new ParticleEffect[148];
                    for (var z = 1; z <= 26; z++)
                    {
                        var p = new Vector3(
                            v.Position.X + 100 * z * (float)Math.Cos(v.RotationRad),
                            v.Position.Y + 100 * z * (float)Math.Sin(v.RotationRad),
                            100);
                        eff[z] =
                            new ParticleEffect(
                                @"particles\ui_mouseactions\draw_commentator.vpcf",
                                p);
                        eff[z].SetControlPoint(1,
                            new Vector3(Members.Menu.Item("wr.Red").GetValue<Slider>().Value,
                                Members.Menu.Item("wr.Green").GetValue<Slider>().Value,
                                Members.Menu.Item("wr.Blue").GetValue<Slider>().Value));
                        eff[z].SetControlPoint(0, p);
                    }
                    Eff.Add(v, eff);
                    InSys.Add(v);
                }
            }
            else if (cd < 720 || !v.IsAlive && InSys.Contains(v))
            {
                InSys.Remove(v);
                ParticleEffect[] eff;
                if (!Eff.TryGetValue(v, out eff)) return;
                foreach (var particleEffect in eff.Where(x => x != null))
                    particleEffect.ForceDispose();
                Eff.Clear();
            }
        }

        public static void Draw(EventArgs args)
        {
            if (!Checker.IsActive()) return;
            if (!Members.Menu.Item("showmemore.Enable").GetValue<bool>()) return;
            if (AAunit != null && AAunit.IsValid)
            {
                var aapos = Drawing.WorldToScreen(AAunit.Position);
                if (aapos.X > 0 && aapos.Y > 0)
                {
                    Drawing.DrawLine(Drawing.WorldToScreen(Members.MyHero.Position), aapos, Color.AliceBlue);
                    const string name = "materials/ensage_ui/spellicons/ancient_apparition_ice_blast.vmat";
                    Drawing.DrawRect(aapos, new Vector2(50, 50), Drawing.GetTexture(name));
                }
            }
            if (Members.BaraIsHere)
                foreach (var v in Manager.HeroManager.GetAllyViableHeroes())
                {
                    var mod = v.HasModifier("modifier_spirit_breaker_charge_of_darkness_vision");
                    if (mod)
                    {
                        if (Equals(Members.MyHero, v))
                        {
                            Drawing.DrawRect(new Vector2(0, 0), new Vector2(Drawing.Width, Drawing.Height),
                                new Color(Members.Menu.Item("charge" + ".Red").GetValue<Slider>().Value,
                                    Members.Menu.Item("charge" + ".Green").GetValue<Slider>().Value,
                                    Members.Menu.Item("charge" + ".Blue").GetValue<Slider>().Value,
                                    Members.Menu.Item("charge" + ".Alpha").GetValue<Slider>().Value));
                        }
                        if (!InSys.Contains(v))
                        {
                            Helper.GenerateSideMessage(v.Name.Replace("npc_dota_hero_", ""),
                                "spirit_breaker_charge_of_darkness");
                            InSys.Add(v);
                        }
                    }
                    else
                    {
                        if (InSys.Contains(v))
                            InSys.Remove(v);
                    }
                }
            if (Members.PAisHere != null)
            {
                var mod = Members.PAisHere.HasModifier("modifier_phantom_assassin_blur_active");
                if (mod && Members.PAisHere.StoredName() == "npc_dota_hero_phantom_assassin")
                {
                    var size3 = new Vector2(10, 20) + new Vector2(13, -6);
                    var w2M = Helper.WorldToMinimap(Members.PAisHere.NetworkPosition);
                    var name = "materials/ensage_ui/miniheroes/" +
                               Members.PAisHere.StoredName().Replace("npc_dota_hero_", "") + ".vmat";
                    Drawing.DrawRect(w2M - new Vector2(size3.X/2, size3.Y/2), size3,
                        Drawing.GetTexture(name));
                }
            }

        }
    }
}