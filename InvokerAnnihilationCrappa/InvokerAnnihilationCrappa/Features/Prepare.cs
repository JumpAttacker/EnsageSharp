using System;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.SDK.Helpers;
using Ensage.SDK.Menu;

namespace InvokerAnnihilationCrappa.Features
{
    public class Prepare
    {
        private readonly Config _main;

        public Prepare(Config main)
        {
            _main = main;
            var panel = main.Factory.Menu("Prepare");
            Enable = panel.Item("Combo key with CTRL", true);
            CustomKey = panel.Item("Cusom key", new KeyBind('0'));
            
            if (Enable)
            {
                UpdateManager.BeginInvoke(Callback);
                CustomKey.Item.ValueChanged += ItemOnValueChanged;
            }
        }

        private void ItemOnValueChanged(object sender, OnValueChangeEventArgs args)
        {
            if (args.GetNewValue<KeyBind>().Active)
            {
                if (!Enable)
                    UpdateManager.BeginInvoke(Callback);
            }
        }

        public MenuItem<KeyBind> CustomKey { get; set; }

        private async void Callback()
        {
            while (Enable || CustomKey.Value.Active)
            {
                var inAction = _main.Invoker._mode.CanExecute;
                if ((inAction && Game.IsKeyDown(0x11)) || CustomKey.Value.Active)
                {
                    await Invoke();
                }
                await Task.Delay(100);
            }
        }

        private async Task Invoke()
        {
            var me = _main.Invoker.Owner;
            if (!me.CanCast())
                return;
            var selectedComboId = _main.Invoker.SelectedCombo;
            var combo = _main.ComboPanel.Combos[selectedComboId];
            var abilities = combo.AbilityInfos;
            var one = abilities[0].Ability is Item ? abilities[1] : abilities[0];
            var two = abilities[0].Ability is Item ? abilities[2] : abilities[1];
            var empty1 = _main.Invoker.Owner.Spellbook.Spell4;
            var empty2 = _main.Invoker.Owner.Spellbook.Spell5;
            var ability1Invoked = one.Ability.Equals(empty1) || one.Ability.Equals(empty2);
            var ability2Invoked = two.Ability.Equals(empty1) || two.Ability.Equals(empty2);
            if (ability1Invoked && ability2Invoked)
                return;
            if (ability1Invoked)
            {
                if (one.Ability.Equals(empty2))
                    await _main.Invoker.Invoke(one);
                else
                    await _main.Invoker.Invoke(two);
            }
            else if (ability2Invoked)
            {
                if (two.Ability.Equals(empty2))
                    await _main.Invoker.Invoke(two);
                else
                    await _main.Invoker.Invoke(one);
            }
            else
            {
                await _main.Invoker.Invoke(one);
            }
        }

        public void OnDeactivate()
        {
            
        }

        public MenuItem<bool> Enable { get; set; }
    }
}