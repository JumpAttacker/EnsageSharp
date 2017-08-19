using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Linq;

using Ensage.SDK.Service;
using Ensage.SDK.Service.Metadata;
using Ensage.SDK.Helpers;
using log4net;

using PlaySharp.Toolkit.Logging;

using SharpDX;

using Ensage;

namespace Disruptor
{
    [ExportPlugin(
        mode: StartupMode.Auto,
        name: "Disruptor",
        version: "1.0.0.0",
        author: "JumpAttacker",
        description: "",
        units: new[] {  HeroId.npc_dota_hero_disruptor })]
    public sealed class Disruptor : Plugin
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [ImportingConstructor]
        public Disruptor([Import] IServiceContext context)
        {
            Context = context;
        }

        public IServiceContext Context { get; }
        public Hero Owner { get; set; }
        public Config Config { get; set; }

        protected override void OnActivate()
        {
            Owner = (Hero) Context.Owner;
            Context.TargetSelector.Activate();
            Timer = new List<TimeContainer>();
            Config = new Config(this);

            UpdateManager.Subscribe(GlimpseCalculation);

            if (Config.DrawFullPath)
            {
                Drawing.OnDraw += FullPathDrawing;
            }

            Config.DrawFullPath.Item.ValueChanged += (sender, args) =>
            {
                if (args.GetNewValue<bool>())
                {
                    Drawing.OnDraw += FullPathDrawing;
                }
                else
                {
                    Drawing.OnDraw -= FullPathDrawing;
                }
            };

            if (Config.DrawFinalPosition)
            {
                UpdateManager.Subscribe(EffectManager);
            }

            Config.DrawFinalPosition.Item.ValueChanged += (sender, args) =>
            {
                if (args.GetNewValue<bool>())
                {
                    UpdateManager.Subscribe(EffectManager);
                }
                else
                {
                    UpdateManager.Unsubscribe(EffectManager);
                }
            };
        }

        private void FullPathDrawing(EventArgs args)
        {
            var target = Context.TargetSelector.Active.GetTargets().FirstOrDefault();
            if (target == null) return;
            var container = Timer.Find(x => x.Owner.Equals(target));
            if (container == null) return;
            foreach (var pos in container.Positions)
            {
                var targetPos = pos.Value;
                var b = Drawing.WorldToScreen(targetPos);
                Drawing.DrawCircle(b, 5, 10, Color.Red);
            }
        }

        private void EffectManager()
        {
            var target = Context.TargetSelector.Active.GetTargets().FirstOrDefault();
            if (target == null)
            {
                Context.Particle.Remove("dis_line");
                return;
            }
            var container = Timer.Find(x => x.Owner.Equals(target));
            if (container == null) return;
            Vector3 abilityPosition;
            if (container.Positions.TryGetValue(Math.Round(Game.RawGameTime, 2) - 4, out abilityPosition))
            {
                Context.Particle.DrawTargetLine(target, "dis_line", abilityPosition, new Color(0, 155, 255));
            }
            else
            {
                var pos = GetClosestPosition(container);
                if (pos.IsZero)
                    return;
                Context.Particle.DrawDangerLine(target, "dis_line", pos);
            }
        }

        private Vector3 GetClosestPosition(TimeContainer container)
        {
            double dif = 0;
            var currentTime = Game.RawGameTime;
            var exitPosition = new Vector3();
            foreach (var cont in container.Positions)
            {
                var time = cont.Key;
                var localDif = Math.Abs(currentTime - time);
                if (localDif <= dif)
                {
                    exitPosition = container.Positions[time];
                    dif = localDif;
                }
            }
            return exitPosition;
        }

        public List<TimeContainer> Timer;
        private void GlimpseCalculation()
        {
            var enemyHeroes = EntityManager<Hero>.Entities.Where(x => x.IsValid && !x.IsIllusion && x.Team != Owner.Team);
            var time = Math.Round(Game.RawGameTime, 2);
            foreach (var hero in enemyHeroes)
            {
                var container = Timer.Find(x => x.Owner.Equals(hero));
                if (container==null)
                {
                    container = new TimeContainer(hero,Config.SuperFastMode);
                    Timer.Add(container);
                }
                container.Update(time);
            }
        }

        protected override void OnDeactivate()
        {
            UpdateManager.Unsubscribe(GlimpseCalculation);
            UpdateManager.Unsubscribe(EffectManager);
            Drawing.OnDraw -= FullPathDrawing;
            foreach (var container in Timer)
            {
                container.Dispose();
            }
            Timer.Clear();
            Context.TargetSelector.Deactivate();
            Context.Particle.Remove("dis_line");
        }
    }
}