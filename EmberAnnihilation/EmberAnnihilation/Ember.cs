using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.SDK.Helpers;
using Ensage.SDK.Service;
using Ensage.SDK.Service.Metadata;
using Ensage.SDK.TargetSelector;
using log4net;
using PlaySharp.Toolkit.Logging;

namespace EmberAnnihilation
{
    [ExportPlugin("Ember Annihilation", author:"JumpAttacker", units: HeroId.npc_dota_hero_ember_spirit)]
    public class Ember : Plugin
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private Ability Fist { get; }
        private Ability Chains { get; }
        private Ability Activator { get; }
        private Ability Remnant { get; }
        private Config Config { get; set; }
        public Hero Me { get; set; }
        public Unit Fountain { get; set; }
        private ITargetSelectorManager Selector { get; }

        [ImportingConstructor]
        public Ember([Import] IServiceContext context, [Import] ITargetSelectorManager selector/*, [Import] IPrediction prediction*/)
        {
            Me = context.Owner as Hero;
            Selector = selector;
            Remnant = Me.GetAbilityById(AbilityId.ember_spirit_fire_remnant);
            Fist = Me.GetAbilityById(AbilityId.ember_spirit_sleight_of_fist);
            Activator = Me.GetAbilityById(AbilityId.ember_spirit_activate_fire_remnant);
            Chains = Me.GetAbilityById(AbilityId.ember_spirit_searing_chains);
            Fountain =
                ObjectManager.GetEntities<Unit>()
                    .FirstOrDefault(
                        x => x != null && x.IsValid && x.ClassId == ClassId.CDOTA_Unit_Fountain && x.Team == Me.Team);
        }

        protected override void OnActivate()
        {
            Config = new Config();
            Config.FistAndComboKey.Item.ValueChanged += FistAndComboKeyChanged;
            Config.RemntantCombo.Item.ValueChanged += RemnantActivator;
            Config.PussyKey.Item.ValueChanged += PussyAction;
            Config.AutoChain.Item.ValueChanged += AutoChains;
            if (Config.AutoChain.Value)
                UpdateManager.BeginInvoke(AutoChainer);
        }

        private void PussyAction(object sender, OnValueChangeEventArgs e)
        {
            var newValue = e.GetNewValue<KeyBind>().Active;
            if (newValue)
            {
                if (Me.HasModifier("modifier_ember_spirit_fire_remnant_timer") && Activator.CanBeCasted() && Me.CanCast())
                {
                    if (Fountain == null)
                    {
                        Log.Error("cant find Fountain");
                        Fountain =
                            ObjectManager.GetEntities<Unit>()
                                .FirstOrDefault(
                                    x =>
                                        x != null && x.IsValid && x.ClassId == ClassId.CDOTA_Unit_Fountain &&
                                        x.Team == Me.Team);
                        return;
                    }
                    Activator.UseAbility(Fountain.Position);
                }
            }
        }

        protected override void OnDeactivate()
        {
            Config?.Dispose();
        }

        private void RemnantActivator(object sender, OnValueChangeEventArgs e)
        {
            var newValue = e.GetNewValue<KeyBind>().Active;
            if (newValue)
                UpdateManager.BeginInvoke(RemnantCombo);
        }
        private void FistAndComboKeyChanged(object sender, OnValueChangeEventArgs args)
        {
            var newValue = args.GetNewValue<KeyBind>().Active;
            if (newValue)
                UpdateManager.BeginInvoke(FistAndChain);
        }
        private void AutoChains(object sender, OnValueChangeEventArgs args)
        {
            var newValue = args.GetNewValue<KeyBind>().Active;
            if (newValue)
                UpdateManager.BeginInvoke(AutoChainer);
        }
        
        private async void RemnantCombo()
        {
            Log.Debug("start remnant combo");
            while (Config.RemntantCombo.Value.Active)
            {
                var target = Selector.Active.GetTargets().FirstOrDefault();
                if (target != null)
                {
                    var mod = Me.FindModifier("modifier_ember_spirit_fire_remnant_charge_counter");
                    var stacks = mod?.StackCount;
                    if (stacks > 0)
                    {
                        Remnant.UseAbility(target.Position);
                        Log.Debug("Remnant: "+stacks);
                        await Task.Delay(20);
                    }
                    else
                    {
                        if (
                            EntityManager<Entity>.Entities.Any(
                                x => x.Name == "npc_dota_ember_spirit_remnant" && x.Distance2D(target) <= 450))
                        {
                            await Task.Delay(150);
                            Activator.UseAbility(target.Position);
                            Log.Debug("Activator");
                            await Task.Delay(100);
                        }
                    }
                }
                await Task.Delay(1);
            }
        }
        private async void FistAndChain()
        {
            Log.Debug("starting combo");
            while (Config.FistAndComboKey.Value.Active)
            {
                var target = Selector.Active.GetTargets().FirstOrDefault();
                if (target != null)
                {
                    if (Fist.CanBeCasted() && Fist.CanHit(target))
                    {
                        Fist.UseAbility(target.Position);
                        Log.Debug("Fist usages");
                        await Task.Delay(25);
                    }
                    if (Chains.CanBeCasted())
                    {
                        if (Me.Distance2D(target) <= 400)
                        {
                            Chains.UseAbility();
                            Log.Debug("Chains usages");
                            await Task.Delay(100);
                        }
                    }
                }
                await Task.Delay(1);
            }
        }
        public async void AutoChainer()
        {
            while (Config.AutoChain.Value)
            {
                if (!Config.FistAndComboKey.Value.Active && !Config.RemntantCombo.Value.Active)
                {
                    var target = Selector.Active.GetTargets().FirstOrDefault();
                    if (target != null)
                    {
                        var mod = Me.FindModifier("modifier_ember_spirit_sleight_of_fist_caster");
                        if (mod != null)
                        {
                            if (Chains.CanBeCasted())
                            {
                                if (Me.Distance2D(target) <= 400)
                                {
                                    Chains.UseAbility();
                                    Log.Debug("Auto Chains usages");
                                    await Task.Delay(100);
                                }
                            }
                        }
                    }
                }
                await Task.Delay(1);
            }
        }
    }
}