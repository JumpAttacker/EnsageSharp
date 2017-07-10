using System;
using System.Linq;
using Ensage.Common.Menu;
using Ensage.SDK.Menu;
using InvokerAnnihilationCrappa.Features;

namespace InvokerAnnihilationCrappa
{
    public class Config : IDisposable
    {
        public readonly Invoker Invoker;

        public Config(Invoker invoker)
        {
            Invoker = invoker;
            Factory = MenuFactory.Create("Invoker Crappahilation");
            ComboKey = Factory.Item("Combo Key", new KeyBind('G'));
            InvokeTime = Factory.Item("Time between spheres in combo", new Slider(1, 1, 200));
            AfterInvokeDelay = Factory.Item("Delay after Invoke", new Slider(1, 1, 500));

            SmartInvoke = Factory.Item("Smart invoke", true);
            SmartInvoke.Item.SetTooltip("will check for spheres before invoke");

            AbilityPanel = new AbilityPanel(this);
            ComboPanel = new ComboPanel(this);
            SmartSphere = new SmartSphere(this);
            AutoSunStrike = new AutoSunStrike(this);
            AutoGhostWalk = new AutoGhostWalk(this);
            Prepare = new Prepare(this);

            var panel = Factory.Menu("Abilities");
            var dict = invoker.AbilityInfos.Select(x => x.Ability.Name).ToDictionary(result => result, result => true);
            AbilitiesInCombo = panel.Item("Abilities in combo", new AbilityToggler(dict));

            //Factory.Target.TextureName = "npc_dota_hero_invoker";
            //Factory.Target.ShowTextWithTexture = true;
        }

        public MenuItem<bool> SmartInvoke { get; set; }

        public MenuItem<AbilityToggler> AbilitiesInCombo { get; set; }

        public AutoGhostWalk AutoGhostWalk { get; set; }

        public MenuItem<Slider> AfterInvokeDelay { get; set; }

        public MenuItem<Slider> InvokeTime { get; set; }

        public Prepare Prepare { get; set; }

        public AutoSunStrike AutoSunStrike { get; set; }

        public ComboPanel ComboPanel { get; set; }

        public SmartSphere SmartSphere { get; set; }

        public AbilityPanel AbilityPanel { get; set; }

        public MenuItem<KeyBind> ComboKey { get; set; }

        public MenuFactory Factory { get; }

        public void Dispose()
        {
            AbilityPanel.OnDeactivate();
            SmartSphere.OnDeactivate();
            ComboPanel.OnDeactivate();
            AutoSunStrike.OnDeactivate();
            Factory?.Dispose();
        }
    }
}