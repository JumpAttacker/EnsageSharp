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
        private static readonly Dictionary<Unit, ParticleEffect> ShowMeMoreEffect =
            new Dictionary<Unit, ParticleEffect>();

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
            var baseList = Manager.BaseManager.GetBaseList().Where(x => x.IsValid && x.IsAlive).ToList();
            /*foreach (var source in ObjectManager.GetEntities<Unit>().Where(x => x.Distance2D(Members.MyHero) <= 350 && !(x is Hero)))
            {
                Printer.Print(source.Name + "-->" + source.DayVision+" & "+source.NightVision);
                foreach (var modifier in source.Modifiers)
                {
                    Printer.Print(modifier.Name);
                }
            }*/
            if (Members.Menu.Item("scan.Enable").GetValue<bool>())
            {
                if (Members.ScanEnemy == null || !Members.ScanEnemy.IsValid)
                {
                    Members.ScanEnemy = baseList.Find(x => !InSys.Contains(x) && x.HasModifier("modifier_radar_thinker"));
                }
                if (Members.ScanEnemy != null)
                {
                    InSys.Add(Members.ScanEnemy);
                    ParticleEffect effect;
                    if (!ShowMeMoreEffect.TryGetValue(Members.ScanEnemy, out effect))
                    {
                        effect = Members.ScanEnemy.AddParticleEffect(@"particles\ui_mouseactions\range_display.vpcf");
                        effect.SetControlPoint(1, new Vector3(900, 0, 0));
                        ShowMeMoreEffect.Add(Members.ScanEnemy, effect);
                    }
                }
            }
            if (Members.Menu.Item("apparition.Enable").GetValue<bool>() && Members.Apparition)
            {
                foreach (var t in baseList.Where(t => !InSys.Contains(t) && t.DayVision == 550).Where(t => !Members.AAlist.Contains(t.Handle)))
                {
                    InSys.Add(t);
                    Members.AAlist.Add(t.Handle);
                    AAunit = t;
                    Helper.GenerateSideMessage("ancient_apparition", "ancient_apparition_ice_blast");
                }
            }
            if (Members.Menu.Item("kunkka.Enable").GetValue<bool>() && Members.Kunkka != null && Members.Kunkka.IsValid)
            {
                const string modname = "modifier_kunkka_torrent_thinker";
                foreach (var t in baseList.Where(x => !InSys.Contains(x) && x.HasModifier(modname)))
                {
                    InSys.Add(t);
                    ParticleEffect effect;
                    if (!ShowMeMoreEffect.TryGetValue(t, out effect))
                    {
                        effect = t.AddParticleEffect(@"particles\ui_mouseactions\range_display.vpcf");
                        effect.SetControlPoint(1, new Vector3(225, 0, 0));
                        ShowMeMoreEffect.Add(t, effect);
                    }
                }
            }
            if (Members.Menu.Item("invoker.Enable").GetValue<bool>() && Members.Invoker != null && Members.Invoker.IsValid)
            {
                const string modname = "modifier_invoker_sun_strike";
                foreach (var t in baseList.Where(x => !InSys.Contains(x) && x.HasModifier(modname)))
                {
                    InSys.Add(t);
                    ParticleEffect effect;
                    if (!ShowMeMoreEffect.TryGetValue(t, out effect))
                    {
                        effect = t.AddParticleEffect(@"particles\ui_mouseactions\range_display.vpcf");
                        effect.SetControlPoint(1, new Vector3(175, 0, 0));
                        ShowMeMoreEffect.Add(t, effect);
                    }
                }
            }
            if (Members.Menu.Item("lina.Enable").GetValue<bool>() && Members.Lina != null && Members.Lina.IsValid)
            {
                const string modname = "modifier_lina_light_strike_array";
                foreach (var t in baseList.Where(x => !InSys.Contains(x) && x.HasModifier(modname)))
                {
                    InSys.Add(t);
                    ParticleEffect effect;
                    if (!ShowMeMoreEffect.TryGetValue(t, out effect))
                    {
                        effect = t.AddParticleEffect(@"particles\ui_mouseactions\range_display.vpcf");
                        effect.SetControlPoint(1, new Vector3(225, 0, 0));
                        ShowMeMoreEffect.Add(t, effect);
                    }
                }
            }
            if (Members.Menu.Item("lesh.Enable").GetValue<bool>() && Members.Leshrac != null && Members.Leshrac.IsValid)
            {
                const string modname = "modifier_leshrac_split_earth_thinker";
                foreach (var t in baseList.Where(x => x.HasModifier(modname)))
                {
                    ParticleEffect effect;
                    if (!ShowMeMoreEffect.TryGetValue(t, out effect))
                    {
                        effect = t.AddParticleEffect(@"particles\ui_mouseactions\range_display.vpcf");
                        effect.SetControlPoint(1, new Vector3(225, 0, 0));
                        ShowMeMoreEffect.Add(t, effect);
                    }
                }
            }
            if (Members.Menu.Item("wr.Enable").GetValue<bool>() && Members.Windrunner != null && Members.Windrunner.IsValid)
            {
                DrawForWr(Members.Windrunner);
            }
            if (Members.Menu.Item("mirana.Enable").GetValue<bool>() && Members.Mirana != null && Members.Mirana.IsValid)
            {
                try
                {
                    DrawForMirana(Members.Mirana, baseList);
                }
                catch (Exception)
                {
                    Printer.Print("[ShowMeMore]: mirana");
                }
                
            }
        }

        private static void DrawForMirana(Hero mirana,List<Unit> Base)
        {
            if (_arrowUnit == null)
            {
                _arrowUnit =
                    Base.Find(x => x.DayVision == 650 && x.Team == Members.MyHero.GetEnemyTeam());
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
            if (Members.Menu.Item("apparition.Enable").GetValue<bool>() && AAunit != null && AAunit.IsValid)
            {
                try
                {
                    var aapos = Drawing.WorldToScreen(AAunit.Position);
                    if (aapos.X > 0 && aapos.Y > 0)
                    {
                        Drawing.DrawLine(Drawing.WorldToScreen(Members.MyHero.Position), aapos, Color.AliceBlue);
                        const string name = "materials/ensage_ui/spellicons/ancient_apparition_ice_blast.vmat";
                        Drawing.DrawRect(aapos, new Vector2(50, 50), Drawing.GetTexture(name));
                    }
                }
                catch (Exception)
                {
                    Printer.Print("[Draw]: Apparation");
                }
                
            }
            if (Members.Menu.Item("scan.Enable").GetValue<bool>())
            {
                if (Members.ScanEnemy != null && Members.ScanEnemy.IsValid)
                {
                    try
                    {
                        var position = Members.ScanEnemy.Position;
                        var w2S = Drawing.WorldToScreen(position);
                        if (!w2S.IsZero)
                            Drawing.DrawText(
                                "Scan Ability " +
                                Members.ScanEnemy.FindModifier("modifier_radar_thinker").RemainingTime.ToString("F1"),
                                w2S,
                                new Vector2(15, 15),
                                Color.White,
                                FontFlags.AntiAlias | FontFlags.StrikeOut);
                    }
                    catch (Exception)
                    {
                        Printer.Print("[Draw]: scan");
                    }
                }
            }
            if (Members.Menu.Item("charge.Enable").GetValue<bool>() && Members.BaraIsHere)
            {
                try
                {
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
                }
                catch (Exception)
                {
                    Printer.Print("[Draw]: charge");
                }
            }
            if (Members.Menu.Item("lifestealer.Enable").GetValue<bool>() && Members.LifeStealer != null && Members.LifeStealer.IsValid && !Members.LifeStealer.IsVisible)
            {
                try
                {
                    const string modname = "modifier_life_stealer_infest_effect";
                    foreach (var t in Manager.HeroManager.GetEnemyViableHeroes().Where(x => x.HasModifier(modname)))
                    {
                        var size3 = new Vector2(10, 20) + new Vector2(13, -6);
                        var w2SPos = HUDInfo.GetHPbarPosition(t);
                        if (w2SPos.IsZero)
                            continue;
                        var name = "materials/ensage_ui/miniheroes/" +
                                   Members.LifeStealer.StoredName().Replace("npc_dota_hero_", "") + ".vmat";
                        Drawing.DrawRect(w2SPos - new Vector2(size3.X/2, size3.Y/2), size3,
                            Drawing.GetTexture(name));
                    }
                    if (Members.Menu.Item("lifestealer.creeps.Enable").GetValue<bool>())
                        foreach (var t in Creeps.All.Where(x => x != null && x.IsAlive))
                        {
                            var size3 = new Vector2(10, 20) + new Vector2(13, -6);
                            var w2SPos = HUDInfo.GetHPbarPosition(t);
                            if (w2SPos.IsZero)
                                continue;
                            var name = "materials/ensage_ui/miniheroes/" +
                                       Members.LifeStealer.StoredName().Replace("npc_dota_hero_", "") + ".vmat";
                            Drawing.DrawRect(w2SPos - new Vector2(size3.X/2, size3.Y/2), size3,
                                Drawing.GetTexture(name));
                        }
                }
                catch (Exception)
                {
                    Printer.Print("[Draw]: lifestealer");
                }
            }
            if (Members.Menu.Item("blur.Enable").GetValue<bool>() && Members.PAisHere != null && Members.PAisHere.IsValid)
            {
                try
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
                catch (Exception)
                {
                    Printer.Print("[Draw]: phantom assasin");
                }
            }

        }
    }
}