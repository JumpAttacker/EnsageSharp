using System;
using System.Collections.Generic;
using Ensage;
using Ensage.SDK.Service;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Ensage.Common;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using Ensage.SDK.Extensions;
using Ensage.SDK.Helpers;
using Ensage.SDK.Input;
using Ensage.SDK.Inventory;
using Ensage.SDK.Menu;
using Ensage.SDK.Orbwalker;
using Ensage.SDK.Renderer.Particle;
using Ensage.SDK.Service.Metadata;
using Ensage.SDK.TargetSelector;
using SharpDX;

namespace Wisp_Annihilation
{
    public class Spirit
    {
        public Entity Unit { get; }
        public ParticleEffect Effect { get; }
        public Vector3 LastPosition;
        public bool IsValid => Unit != null && Unit.IsValid && Unit.Health > 0 && Effect != null && Effect.IsValid && !Effect.IsDestroyed;

        public Spirit(Entity unit, ParticleEffect effect)
        {
            Unit = unit;
            Effect = effect;
            LastPosition = unit.Position;
            StartTime = Game.RawGameTime;
            Console.WriteLine($"new spirit: {unit.Handle} {unit.Name} {unit.ClassId} Ownder: {unit.Owner.Name} {unit.Health}");
        }

        public float StartTime { get; set; }
    }
    public class SpiritCatcher
    {
        public List<Spirit> Spirits;
        public SpiritCatcher()
        {
            UpdateManager.Subscribe(Callback,100);
            Spirits = new List<Spirit>();
            Entity.OnParticleEffectAdded += EntityOnOnParticleEffectAdded;
        }

        private void EntityOnOnParticleEffectAdded(Entity sender, ParticleEffectAddedEventArgs args)
        {
            var partName = args.Name;
            if (partName == "particles/units/heroes/hero_wisp/wisp_guardian.vpcf")
            {
                Spirits.Add(new Spirit(sender, args.ParticleEffect));
            }
            /*else if (partName == "particles/units/heroes/hero_wisp/wisp_guardian_explosion.vpcf")
            {
                Console.WriteLine("old owner -> " + sender.Handle);
                var destroySpirit = Spirits.Find(x => x.Unit.Equals(sender));
                if (destroySpirit != null)
                {
                    Spirits.Remove(destroySpirit);
                }
                else
                {
                    Console.WriteLine("cant find this shit");
                }
            }*/
        }

        private void Callback()
        {
            var tempList = Spirits.ToList();
            foreach (var spirit in tempList)
            {
                if (Game.RawGameTime - spirit.StartTime < 1.5)
                    continue;
                if (Math.Abs(spirit.LastPosition.X - spirit.Unit.Position.X) < 0.1)
                {
                    Spirits.Remove(spirit);
                }
                else
                {
                    spirit.LastPosition = spirit.Unit.Position;
                }
            }
        }

        public void OnDeactivate()
        {
            Entity.OnParticleEffectAdded -= EntityOnOnParticleEffectAdded;
            UpdateManager.Unsubscribe(Callback);
        }
    }
    public class Config
    {
        public WispAnnihilation Wisp { get; }
        public MenuFactory Factory { get; }
        
        public Config(WispAnnihilation wisp)
        {
            Wisp = wisp;
            Factory = MenuFactory.Create("Wisp[Aim] annihilation");
            AimKey = Factory.Item("Aim Key", new KeyBind('Z',KeyBindType.Toggle));
        }

        public MenuItem<KeyBind> AimKey { get; set; }

        public void Dispose()
        {
            Factory?.Dispose();
        }
    }

    [ExportPlugin("Wisp[Aim] annihilation", author: "JumpAttacker", units: HeroId.npc_dota_hero_wisp)]
    public class WispAnnihilation : Plugin
    {
        public Lazy<IInventoryManager> InventoryManager { get; }
        public Lazy<IInputManager> Input { get; }
        public Lazy<IOrbwalkerManager> OrbwalkerManager { get; }
        public Lazy<ITargetSelectorManager> TargetManager { get; }
        public Lazy<IParticleManager> ParticleManager { get; }
        public SpiritCatcher SpiritCatcher;
        [ImportingConstructor]
        public WispAnnihilation(
            [Import] Lazy<IServiceContext> context,
            [Import] Lazy<IInventoryManager> inventoryManager,
            [Import] Lazy<IInputManager> input,
            [Import] Lazy<IOrbwalkerManager> orbwalkerManager,
            [Import] Lazy<ITargetSelectorManager> targetManager,
            [Import] Lazy<IParticleManager> particleManager)
        {
            InventoryManager = inventoryManager;
            Input = input;
            OrbwalkerManager = orbwalkerManager;
            TargetManager = targetManager;
            ParticleManager = particleManager;
            Owner = (Hero) context.Value.Owner;
        }

        public Config Config;
        public Hero Owner;
        public float CurrentRange = 150;
        protected override void OnActivate()
        {
            Config = new Config(this);
            SpiritCatcher = new SpiritCatcher();
            TargetManager.Value.Activate();

            Config.AimKey.Item.ValueChanged += ItemOnValueChanged;

            Spirits = Owner.GetAbilityById(AbilityId.wisp_spirits);
            SpiritsOut = Owner.GetAbilityById(AbilityId.wisp_spirits_out);
            SpiritsIn = Owner.GetAbilityById(AbilityId.wisp_spirits_in);

            Player.OnExecuteOrder += PlayerOnOnExecuteOrder;

            

        }

        private void DrawingOnOnDraw(EventArgs args)
        {
            var startPos = new Vector2(Drawing.Width - 250, 100);
            var size = new Vector2(180, 90);
            Drawing.DrawRect(startPos, size, new Color(0, 0, 0, 100));
            Drawing.DrawRect(startPos, size, new Color(0, 0, 0, 255), true);
            Drawing.DrawText(
                "Spirits Aim is Active" +
                $"[{Utils.KeyToText(Config.AimKey.Value.Key)}]",
                startPos + new Vector2(10, 10), new Vector2(20), new Color(0, 155, 255),
                FontFlags.AntiAlias | FontFlags.DropShadow | FontFlags.Additive | FontFlags.Custom |
                FontFlags.StrikeOut);
            if (Target != null && Target.IsAlive)
            {
                var pos = Drawing.WorldToScreen(Target.Position);
                Drawing.DrawText("Aim Target", pos, new Vector2(0, 50), Color.Red,
                    FontFlags.AntiAlias | FontFlags.DropShadow);
                var name = "materials/ensage_ui/heroes_horizontal/" +
                           Target.Name.Replace("npc_dota_hero_", "") + ".vmat";
                size = new Vector2(50, 50);
                Drawing.DrawRect(startPos + new Vector2(10, 35), size + new Vector2(13, -6),
                    Textures.GetTexture(name));
                Drawing.DrawRect(startPos + new Vector2(10, 35), size + new Vector2(14, -5),
                    new Color(0, 0, 0, 255), true);
            }
        }

        private void PlayerOnOnExecuteOrder(Player sender, ExecuteOrderEventArgs args)
        {
            var ability = args.Ability;
            if (ability != null && ability.Id == AbilityId.wisp_spirits)
            {
                CurrentRange = 150;
            }
        }

        public Ability SpiritsOut { get; set; }

        public Ability SpiritsIn { get; set; }

        public Ability Spirits { get; set; }
        public Hero Target;
        private void ItemOnValueChanged(object sender, OnValueChangeEventArgs args)
        {
            var nv = args.GetNewValue<KeyBind>().Active;
            var ov = args.GetOldValue<KeyBind>().Active;
            if (ov != nv)
            {
                if (nv)
                {
                    UpdateManager.BeginInvoke(Callback);
                    Drawing.OnDraw += DrawingOnOnDraw;
                    UpdateManager.Subscribe(ParticleUpdater);
                }
                else
                {
                    Drawing.OnDraw -= DrawingOnOnDraw;
                    UpdateManager.Unsubscribe(ParticleUpdater);
                    ParticleManager.Value.Remove("attackRange" + Owner.Handle);
                }
            }
        }

        private void ParticleUpdater()
        {
            ParticleManager.Value.AddOrUpdate(Owner, "attackRange" + Owner.Handle,
                        "materials/ensage_ui/particles/range_display_mod.vpcf", ParticleAttachment.AbsOriginFollow, RestartType.None, 1,
                        CurrentRange * 1.1f, 2, Color.LimeGreen);
        }

        private async void Callback()
        {
            var particle = ParticleManager.Value;
            while (Config.AimKey)
            {
                await GetTarget();
                if (!Target.IsVisible)
                {
                    await Task.Delay(1);
                    continue;
                }
                //particle.DrawCircle(Owner.Position,"kek",500,Color.Aqua);
                var anySpirit = SpiritCatcher.Spirits.FirstOrDefault();
                if (anySpirit != null)
                {
                    CurrentRange = anySpirit.Unit.Distance2D(Owner);
                    var targetDist = Target.Distance2D(Owner);
                    var targetToWisp = Math.Abs(CurrentRange - targetDist);
                    if (targetToWisp >= 20)
                    {
                        if (CurrentRange > targetDist)
                        {
                            //dec
                            if (!SpiritsIn.IsToggled)
                                SpiritsIn.ToggleAbility();
                        }
                        else
                        {
                            if (!SpiritsOut.IsToggled)
                                SpiritsOut.ToggleAbility();
                            //inc
                        }
                    }
                    else
                    {
                        if (SpiritsIn.IsToggled)
                            SpiritsIn.ToggleAbility();
                        if (SpiritsOut.IsToggled)
                            SpiritsOut.ToggleAbility();
                    }
                    await Task.Delay(125);
                }
                //particle.DrawRange(Owner, "attackRange" + Owner.Handle, CurrentRange, Color.LimeGreen);
                
                /*if (effect == null)
                    effect = Owner.AddParticleEffect("materials/ensage_ui/particles/range_display_mod.vpcf");
                effect.SetControlPoint(1, new Vector3(CurrentRange, 255, 0));
                effect.SetControlPoint(2, new Vector3(255, 255, 255));*/
                await Task.Delay(1);
            }
            /*effect.Dispose();
            effect = null;*/
            particle.Remove("attackRange" + Owner.Handle);
        }

        private async Task GetTarget()
        {
            while (Target == null || !Target.IsValid || !Target.IsAlive)
            {
                Target = (Hero) TargetManager.Value.Active.GetTargets().FirstOrDefault();
                await Task.Delay(100);
            }
        }

        protected override void OnDeactivate()
        {
            TargetManager.Value.Deactivate();
            SpiritCatcher.OnDeactivate();
            Player.OnExecuteOrder -= PlayerOnOnExecuteOrder;
            Config?.Dispose();
        }
    }
}
