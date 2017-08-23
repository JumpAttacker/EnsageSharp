using System;
using Ensage.SDK.Helpers;
using Ensage.SDK.Renderer.Particle;
using SharpDX;

namespace EarthSpiritCrappa
{
    public class AbilityRanger : IDisposable
    {
        private readonly EarthSpiritCrappa _main;

        public AbilityRanger(EarthSpiritCrappa earthSpiritCrappa)
        {
            _main = earthSpiritCrappa;
            Config = _main.Config;
            ParticleManager = _main.Context.Particle;
            UpdateManager.Subscribe(EffectUpdater, 500);
        }

        public IParticleManager ParticleManager { get; set; }

        public Config Config { get; set; }

        private void EffectUpdater()
        {
            if (Config.SmashRange)
            {
                ParticleManager.DrawRange(_main.Owner,"smash_range", _main.Smash.CastRange, Config.GetColor(Config.SmashRange));
            }
            else
            {
                ParticleManager.Remove("smash_range");
            }

            if (Config.GripRange)
            {
                ParticleManager.DrawRange(_main.Owner, "grip_range", _main.Grip.CastRange, Config.GetColor(Config.GripRange));
            }
            else
            {
                ParticleManager.Remove("grip_range");
            }

            if (Config.BlinkRange && _main.Blink!=null)
            {
                ParticleManager.DrawRange(_main.Owner, "blink_range", _main.Blink.CastRange, Config.GetColor(Config.BlinkRange));
            }
            else
            {
                ParticleManager.Remove("blink_range");
            }
        }

        public void Dispose()
        {
            UpdateManager.Unsubscribe(EffectUpdater);
        }
    }
}