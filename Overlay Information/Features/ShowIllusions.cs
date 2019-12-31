using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Ensage;
using Ensage.Common.Menu;
using Ensage.SDK.Helpers;
using Ensage.SDK.Menu;
using Ensage.SDK.Renderer.Particle;
using log4net;
using PlaySharp.Toolkit.Logging;
using SharpDX;

namespace OverlayInformation.Features
{
    public class ShowIllusions : IDisposable
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static List<string> ShowIllusionList = new List<string>
        {
            "materials/ensage_ui/particles/smoke_illusions_mod.vpcf",
            "materials/ensage_ui/particles/illusions_mod.vpcf",
            "materials/ensage_ui/particles/illusions_mod_v2.vpcf",
            "materials/ensage_ui/particles/illusions_mod_balloons.vpcf"
            //"particles/items2_fx/shadow_amulet_active_ground_proj.vpcf"
        };

        public ShowIllusions(Config config)
        {
            Config = config;
            Particle = config.Main.Context.Value.Particle;
            var panel = Config.Factory.Menu("Show Illusions");
            Enable = panel.Item("Enable", true);
            Id = panel.Item("Id", new StringList(new[] {"1", "2", "3", "4"}, 2));
            IllusionSize = panel.Item("Effect Size", new Slider(120, 1, 250));
            ClrR = panel.Item("Red", new Slider(255, 0, 255));
            ClrG = panel.Item("Green", new Slider(255, 0, 255));
            ClrB = panel.Item("Blue", new Slider(255, 0, 255));
            ClrA = panel.Item("Alpha", new Slider(255, 0, 255));
            MyTeam = ObjectManager.LocalHero.Team;

            if (Enable) Load();

            Enable.Item.ValueChanged += (sender, args) =>
            {
                if (args.GetNewValue<bool>())
                    Load();
                else
                    UnLoad();
            };

            Id.PropertyChanged += OnChangeAnySettings;
            IllusionSize.PropertyChanged += OnChangeAnySettings;
            ClrR.PropertyChanged += OnChangeAnySettings;
            ClrG.PropertyChanged += OnChangeAnySettings;
            ClrB.PropertyChanged += OnChangeAnySettings;
            ClrA.PropertyChanged += OnChangeAnySettings;
        }

        public Config Config { get; }

        public MenuItem<Slider> ClrR { get; set; }
        public MenuItem<Slider> ClrG { get; set; }
        public MenuItem<Slider> ClrB { get; set; }
        public MenuItem<Slider> ClrA { get; set; }
        public Vector3 GetColor => new Vector3(ClrR, ClrG, ClrB);
        public MenuItem<Slider> IllusionSize { get; set; }

        public MenuItem<StringList> Id { get; set; }
        public int GetId => Id.Value.SelectedIndex;

        public Team MyTeam { get; set; }

        public IParticleManager Particle { get; set; }

        public MenuItem<bool> Enable { get; set; }

        public void Dispose()
        {
            if (Enable) UnLoad();
        }

        private void OnChangeAnySettings(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (!Enable)
                return;
            var illusions = EntityManager<Hero>.Entities.Where(x => x.IsAlive && x.Team != MyTeam && x.IsIllusion);
            foreach (var x in illusions) AddEffect(x);
        }

        private void EntityAdded(object sender, Hero x)
        {
            if (x.Team == MyTeam || !x.IsIllusion)
                return;
            AddEffect(x);
        }

        public void AddEffect(Hero x)
        {
            if (GetId == 0)
            {
                Particle.AddOrUpdate(x, "showIllusion" + x.Handle, ShowIllusionList[GetId],
                    ParticleAttachment.AbsOrigin);
            }
            else
            {
                var size = new Vector3(IllusionSize, ClrA, 0);
                Particle.AddOrUpdate(x, "showIllusion" + x.Handle, ShowIllusionList[GetId],
                    ParticleAttachment.AbsOrigin,
                    RestartType.FullRestart,
                    1, size, 2, GetColor);
            }
        }

        private void Load()
        {
            EntityManager<Hero>.EntityAdded += EntityAdded;
        }

        private void UnLoad()
        {
            EntityManager<Hero>.EntityAdded -= EntityAdded;
            var illusions = EntityManager<Hero>.Entities.Where(x => x.IsAlive && x.Team != MyTeam && x.IsIllusion);
            foreach (var x in illusions) Particle.Remove("showIllusion" + x.Handle);
        }
    }
}