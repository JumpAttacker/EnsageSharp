using System;
using System.Reflection;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common;
using Ensage.Common.Menu;
using Ensage.SDK.Extensions;
using Ensage.SDK.Helpers;
using Ensage.SDK.Menu;
using Ensage.SDK.Renderer.Particle;
using log4net;
using PlaySharp.Toolkit.Logging;
using SharpDX;

namespace OverlayInformation.Features
{
    public class ShowMeMore : IDisposable
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public Config Config { get; }
        public ShowMeMore(Config config)
        {
            Config = config;
            Particle = config.Main.Context.Value.Particle;
            var panel = Config.Factory.Menu("Show Me More");
            var baraPanel = panel.Menu("Spirit Breaker's charge");
            Enable = panel.Item("Enable", true);
            SpiritBreaker = baraPanel.Item("Fill background", true);
            SpiritBreaker.Item.SetTooltip("only for ur main hero");
            SpiritBreakerClrR = baraPanel.Item("Red", new Slider(255, 0, 255));
            SpiritBreakerClrG = baraPanel.Item("Green", new Slider(0, 0, 255));
            SpiritBreakerClrB = baraPanel.Item("Blue", new Slider(0, 0, 255));
            SpiritBreakerClrA = baraPanel.Item("Alpha", new Slider(40, 0, 255));

            if (Enable)
            {
                Load();
            }

            Enable.Item.ValueChanged += (sender, args) =>
            {
                if (args.GetNewValue<bool>())
                {
                    Load();
                }
                else
                {
                    UnLoad();
                }
            };
        }

        public MenuItem<Slider> SpiritBreakerClrR { get; set; }
        public MenuItem<Slider> SpiritBreakerClrG { get; set; }
        public MenuItem<Slider> SpiritBreakerClrB { get; set; }
        public MenuItem<Slider> SpiritBreakerClrA { get; set; }

        public MenuItem<bool> SpiritBreaker { get; set; }
        public bool ChargeOnMainHero;
        private void DrawingOnOnDraw(EventArgs args)
        {
            if (SpiritBreaker)
            {
                Drawing.DrawRect(Vector2.Zero, new Vector2(Drawing.Width, Drawing.Height),
                    new Color(
                        SpiritBreakerClrR.Value.Value, SpiritBreakerClrG.Value.Value,
                        SpiritBreakerClrB.Value.Value, SpiritBreakerClrA.Value.Value));
            }
        }


        private async void OnNewModifier(Unit sender, ModifierChangedEventArgs args)
        {
            var mod = args.Modifier;
            var name = mod.Name;
            if (name == "modifier_spirit_breaker_charge_of_darkness_vision")
            {
                if (sender.Team == Config.Main.Owner.Team)
                {
                    var effectName = "materials/ensage_ui/particles/spirit_breaker_charge_target.vpcf";
                    if (!(sender is Hero))
                    {
                        effectName = "particles/units/heroes/hero_spirit_breaker/spirit_breaker_charge_target.vpcf";
                    }
                    else
                    {
                        Program.GenerateSideMessage(sender.Name, AbilityId.spirit_breaker_charge_of_darkness.ToString());
                    }
                    var effect = new ParticleEffect(effectName, sender, ParticleAttachment.OverheadFollow);
                    var wasLocal = false;
                    if (sender.Equals(ObjectManager.LocalHero))
                    {
                        wasLocal = true;
                        Drawing.OnDraw += DrawingOnOnDraw;
                    }
                    while (mod.IsValid)
                    {
                        await Task.Delay(100);
                    }
                    if (wasLocal)
                        Drawing.OnDraw -= DrawingOnOnDraw;
                    effect.Dispose();
                }
            }
            else if (name == "modifier_life_stealer_infest_effect")
            {
                var effectName = "materials/ensage_ui/particles/life_stealer_infested_unit.vpcf";
                if (!(sender is Hero))
                {
                    effectName = "particles/units/heroes/hero_life_stealer/life_stealer_infested_unit.vpcf";
                }
                else
                {
                    Program.GenerateSideMessage(sender.Name, AbilityId.life_stealer_infest.ToString());
                }

                var effect = new ParticleEffect(effectName, sender, ParticleAttachment.OverheadFollow);
                while (mod.IsValid)
                {
                    await Task.Delay(100);
                }
                effect.Dispose();
            }
        }

        private void OnNewParticle(Entity sender, ParticleEffectAddedEventArgs args)
        {
            var name = args.Name;
            if (name.Contains("particles/units/heroes/hero_invoker/invoker_emp.vpcf"))
            {
                DelayAction.Add(10, async () =>
                {
                    var effect = args.ParticleEffect;
                    var a = effect.GetControlPoint(0);
                    var rangeEffect = new ParticleEffect("materials/ensage_ui/particles/range_display_mod.vpcf", a);
                    var range = 675;
                    rangeEffect.SetControlPoint(1, new Vector3(range, 255, 0));
                    rangeEffect.SetControlPoint(2, new Vector3(139, 0, 255));
                    //EmpRanger.Add(effect, rangeEffect);
                    await Task.Delay(2900);
                    rangeEffect.Dispose();
                });
            }
        }

        public IParticleManager Particle { get; set; }

        private void EntityAdded(object sender, Unit x)
        {
            var classId = x.ClassId;
            if (classId == ClassId.CDOTA_BaseNPC)
            {
                foreach (var mod in x.Modifiers)
                {
                    var name = mod.Name;
                    if (name == "modifier_invoker_sun_strike")
                    {
                        DrawRange(x, 175f, 1.7f);
                    }
                    else if (name == "modifier_lina_light_strike_array")
                    {
                        DrawRange(x, 225f, 0.5f);
                    }
                    else if (name == "modifier_leshrac_split_earth_thinker")
                    {
                        DrawRange(x, 225, 0.35f);
                    }
                    else if (name == "modifier_kunkka_torrent_thinker")
                    {
                        var kunkka =
                            Config.Main.Updater.Heroes.Find(y => y.Hero.ClassId == ClassId.CDOTA_Unit_Hero_Kunkka);
                        var range = 225;
                        if (
                                kunkka?.Hero.GetAbilityById(AbilityId.special_bonus_unique_kunkka).Level > 0)
                            //(kunkka.Hero.GetAbilityById(AbilityId.special_bonus_unique_kunkka).Level > 0)
                        {
                            range += 200;
                        }
                        DrawRange(x, range, 1.6f);
                    }
                    /*else if (name == "modifier_projectile_vision" && x.DayVision == 500)
                    {
                        
                    }*/
                }
                if (x.DayVision == 500)
                {
                    DrawMiranaLine(x);
                }
            }
        }

        private async void DrawMiranaLine(Unit unit)
        {
            var startPos = unit.Position;
            await Task.Delay(100);
            var newPos = unit.Position;
            var pos = (startPos - newPos).Normalized();
            pos *= 3000;
            pos = startPos - pos;
            Particle.DrawLine(unit, "mirana_arrow", pos,false);
            while (unit.IsValid)
            {
                await Task.Delay(50);
            }
            Particle.Remove("mirana_arrow");
        }

        private async void DrawRange(Unit unit, float range, float time, Color clr)
        {
            var handle = unit.Handle.ToString();
            Particle.DrawRange(unit, handle, range, clr);
            await Task.Delay((int) (time*1000));
            Particle.Remove(handle);
        }
        private void DrawRange(Unit unit, float range, float time)
        {
            DrawRange(unit, range, time, Color.Red);
        }

        private void Load()
        {
            EntityManager<Unit>.EntityAdded += EntityAdded;
            Entity.OnParticleEffectAdded += OnNewParticle;
            Unit.OnModifierAdded += OnNewModifier;
            //Drawing.OnDraw += DrawingOnOnDraw;
        }
        private void UnLoad()
        {
            EntityManager<Unit>.EntityAdded -= EntityAdded;
            Entity.OnParticleEffectAdded -= OnNewParticle;
            Unit.OnModifierAdded -= OnNewModifier;
            //Drawing.OnDraw -= DrawingOnOnDraw;
        }

        public MenuItem<bool> Enable { get; set; }

        public void Dispose()
        {
            if (Enable)
            {
                UnLoad();
            }
        }
    }
}