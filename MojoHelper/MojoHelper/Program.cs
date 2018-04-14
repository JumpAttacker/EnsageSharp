using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Ensage;
using Ensage.SDK.Menu;
using Ensage.SDK.Service;
using Ensage.SDK.Service.Metadata;
using log4net;
using PlaySharp.Toolkit.Logging;
using System.ComponentModel.Composition;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.SDK.Helpers;
using HeroExtensions = Ensage.SDK.Extensions.HeroExtensions;

namespace MojoHelper
{
    class Program
    {
        static void Main()
        {
        }
    }

    public class LittleEffect
    {
        public ParticleEffect Effect;
        public float StartTIme;
        public int ClickCount;

        public LittleEffect(ParticleEffect effect)
        {
            Effect = effect;
            StartTIme = Game.RawGameTime;
            ClickCount = 5;
        }
    }

    public class Config : IDisposable
    {
        public List<Player> SteamIds;
        public Config()
        {
            Factory = MenuFactory.Create("Mojo Helper");
            Rate = Factory.Item("Rate", new Slider(107, 10, 1000));
            TpRate = Factory.Item("Tp Rate", new Slider(500, 10, 1000));
            SteamIds = new List<Player>();
            foreach (var hero in EntityManager<Hero>.Entities)
            {
                TryToInitNewHero(hero);
            }

            EntityManager<Hero>.EntityAdded += EntityManagerOnEntityAdded;
        }

        private void EntityManagerOnEntityAdded(object sender, Hero hero)
        {
            TryToInitNewHero(hero);
        }

        private void TryToInitNewHero(Hero hero)
        {
            var player = hero.Player;
            if (player == null || !player.IsValid || hero.Team != ObjectManager.LocalHero.Team)
                return;
            var playerSteamId = player.PlayerSteamId;
            if (playerSteamId == 0 || SteamIds.Contains(player))
                return;
            SteamIds.Add(player);
            Factory.Item($"{player.Name}", $"MojoId {playerSteamId}", false);
            Console.WriteLine($"added new Player to Mojo -> {player.Name} {HeroExtensions.GetDisplayName(hero)}");
        }

        public MenuItem<Slider> TpRate { get; set; }

        public MenuItem<Slider> Rate { get; set; }

        public MenuFactory Factory { get; }

        public void Dispose()
        {
            EntityManager<Hero>.EntityAdded -= EntityManagerOnEntityAdded;
            Factory?.Dispose();
        }
    }

    [ExportPlugin("Mojo Helper", StartupMode.Auto, "JumpAttacker")]
    public class MojoHelper : Plugin
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private Config Config { get; set; }
        public Hero Me { get; set; }
        public List<Unit> PingList;
        public List<LittleEffect> EffectList;
        //public Hero Golden;
        public List<string> ModifierList = new List<string>
        {
            "modifier_invoker_sun_strike",
            "modifier_kunkka_torrent_thinker"
        };
        [ImportingConstructor]
        public MojoHelper([Import] IServiceContext context)
        {
            Me = context.Owner as Hero;
        }
        
        protected override void OnActivate()
        {
            Config = new Config();
            PingList = new List<Unit>();
            EffectList = new List<LittleEffect>();
            Unit.OnModifierAdded += UnitOnOnModifierAdded;
            Unit.OnModifierRemoved += UnitOnOnModifierRemoved;

            Entity.OnParticleEffectAdded += EntityOnOnParticleEffectAdded;
            UpdateManager.BeginInvoke(Updater);
        }

        private void EntityOnOnParticleEffectAdded(Entity sender, ParticleEffectAddedEventArgs args)
        {
            if (!args.Name.Contains("teleport_start"))
                return;
            DelayAction.Add(20, () =>
            {
                var pos = args.ParticleEffect.GetControlPoint(0);
                var pass = false;
                foreach (var hero in EntityManager<Hero>.Entities)
                {
                    var player = hero.Player;
                    if (player != null && player.PlayerSteamId>0)
                    {
                        var item =
                            Config.Factory.Target.Item($"{Config.Factory.Target.Name}.MojoId {player.PlayerSteamId}");
                        if (item != null)
                        {
                            if (item.GetValue<bool>() && hero.Distance2D(pos) <= 2750)
                            {
                                pass = true;
                                break;
                            }
                        }
                    }
                }
                //if (!HeroList.Any(x => x.Distance2D(pos) <= 3000))
                //if (!Config.SteamIds.Select(y => y.Hero).Any(x => x.Distance2D(pos) <= 3000))
                if (!pass)
                    return;
                var anyAllyHero =
                    EntityManager<Hero>.Entities.Any(
                        x => x.IsValid && x.Team == Me.Team && pos.Distance2D(x.Position) <= 50);
                if (!anyAllyHero)
                    EffectList.Add(new LittleEffect(args.ParticleEffect));
            });
        }

        private void UnitOnOnModifierRemoved(Unit sender, ModifierChangedEventArgs args)
        {
            var mod = args.Modifier;
            if (PingList.Contains(mod.Owner))
                PingList.Remove(mod.Owner);
        }

        private void UnitOnOnModifierAdded(Unit sender, ModifierChangedEventArgs args)
        {
            if (!ModifierList.Contains(args.Modifier.Name))
                return;
            var owner = args.Modifier.Owner;
            if (owner.Team == Me.Team)
                return;
            var pos = owner.Position;
            var pass = false;
            foreach (var hero in EntityManager<Hero>.Entities)
            {
                var player = hero.Player;
                if (player != null && player.PlayerSteamId > 0)
                {
                    var item =
                        Config.Factory.Target.Item($"{Config.Factory.Target.Name}.MojoId {player.PlayerSteamId}");
                    if (item != null)
                    {
                        if (item.GetValue<bool>() && hero.Distance2D(pos) <= 2750)
                        {
                            pass = true;
                            break;
                        }
                    }
                }
            }
            //if (!HeroList.Any(x => x.Distance2D(pos) <= 3000))
            //if (!Config.SteamIds.Select(y => y.Hero).Any(x => x.Distance2D(pos) <= 3000))
            if (!pass)
                return;
            PingList.Add(args.Modifier.Owner);
        }

        public async void Updater()
        {
            while (IsActive)
            {
                foreach (var unit in PingList.ToList())
                {
                    if (!unit.IsValid)
                        continue;
                    Network.MapPing(unit.Position.ToVector2(), PingType.Normal);
                    await Task.Delay(Config.Rate);
                }
                foreach (var manager in EffectList.ToList())
                {
                    var unit = manager.Effect;
                    if (unit == null || !unit.IsValid || unit.IsDestroyed || Game.RawGameTime - manager.StartTIme >= 2.8 ||
                        manager.ClickCount-- <= 0)
                    {
                        EffectList.Remove(manager);
                        continue;
                    }
                    Network.MapPing(unit.GetControlPoint(0).ToVector2(), PingType.Normal);
                    await Task.Delay(Config.TpRate);
                }

                if (!PingList.Any() && !EffectList.Any())
                    await Task.Delay(50);
            }
        }

        protected override void OnDeactivate()
        {
            //UpdateManager.Unsubscribe(Callback);
            Unit.OnModifierAdded -= UnitOnOnModifierAdded;
            Unit.OnModifierRemoved -= UnitOnOnModifierRemoved;
            PingList.Clear();
            EffectList.Clear();
            Entity.OnParticleEffectAdded -= EntityOnOnParticleEffectAdded;
            Config?.Dispose();
        }
    }
}
