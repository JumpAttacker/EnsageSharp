using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
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
            SsExtraDelay = Factory.Item("Sun Strike Extra Delay", new Slider(15, 0, 25));
            SsExtraDelay.Item.SetTooltip("dec this value if you cant hit target by ss");
            //Ensage.SDK.Orbwalker.Modes.Farm
            SmartInvoke = Factory.Item("Smart invoke", true);
            SmartInvoke.Item.SetTooltip("will check for spheres before invoke");

            ExpInvoke = Factory.Item("Experemental invoke", false);
            ExpInvoke.Item.SetTooltip("Enable this if your hero cant use invoke properly and disable [SmartInvoke]");

            ExtraDelayAfterSpells = Factory.Item("Extra delay after each ability in combo", false);
            ExtraDelayAfterSpells.Item.SetTooltip("Enable this if your hero sometimes not use abilities");

            AutoIceWall = Factory.Item("Dummy IceWall", true);
            AutoIceWall.Item.SetTooltip("Hero will run to the enemy");

            SmartMove = Factory.Item("Move to target if ability out of range", true);

            AbilityPanel = new AbilityPanel(this);
            ComboPanel = new ComboPanel(this);
            SmartSphere = new SmartSphere(this);
            AutoSunStrike = new AutoSunStrike(this);
            AutoGhostWalk = new AutoGhostWalk(this);
            Prepare = new Prepare(this);
            ExortForFarmMode = new ExortForFarmMode(this);
            CustomCombos = new CustomCombos(this);

            var panel = Factory.Menu("Abilities");
            var dict = invoker.AbilityInfos.Select(x => x.Ability.Name).ToDictionary(result => result, result => true);
            AbilitiesInCombo = panel.Item("Abilities in combo", new AbilityToggler(dict));
            var dict2 = new Dictionary<string, bool>
            {
                {AbilityId.item_blink.ToString(),true},
                {AbilityId.item_sheepstick.ToString(),true},
                {AbilityId.item_shivas_guard.ToString(),true},
                {AbilityId.item_orchid.ToString(),true},
                {AbilityId.item_bloodthorn.ToString(),true},
            };
            ItemsInCombo = panel.Item("Items in combo", new AbilityToggler(dict2));

            //Factory.Target.TextureName = "npc_dota_hero_invoker";
            //Factory.Target.ShowTextWithTexture = true;
        }

        public CustomCombos CustomCombos { get; set; }

        public MenuItem<bool> SmartMove { get; set; }

        public MenuItem<bool> ExtraDelayAfterSpells { get; set; }

        public MenuItem<bool> ExpInvoke { get; set; }

        public MenuItem<bool> AutoIceWall { get; set; }

        public ExortForFarmMode ExortForFarmMode { get; set; }

        public MenuItem<Slider> SsExtraDelay { get; set; }

        public MenuItem<AbilityToggler> ItemsInCombo { get; set; }

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
            ExortForFarmMode.OnDeactivate();
            Factory?.Dispose();
        }
    }
}