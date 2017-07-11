using System.Reflection;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.SDK.Helpers;
using Ensage.SDK.Menu;
using log4net;
using PlaySharp.Toolkit.Logging;

namespace InvokerAnnihilationCrappa.Features
{
    public class Prepare
    {
        private readonly Config _main;
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public Prepare(Config main)
        {
            _main = main;
            var panel = main.Factory.Menu("Prepare");
            Enable = panel.Item("Combo key with CTRL (need to hold)", true);
            CustomKey = panel.Item("Cusom key (need to hold)", new KeyBind('0'));

            if (Enable)
            {
                //UpdateManager.BeginInvoke(Callback);
                UpdateManager.Subscribe(Tost, 100);
                CustomKey.Item.ValueChanged += ItemOnValueChanged;
            }

            Enable.Item.ValueChanged += (sender, args) =>
            {
                if (args.GetNewValue<bool>())
                    UpdateManager.Subscribe(Tost, 100);
                else
                    UpdateManager.Unsubscribe(Tost);
                //UpdateManager.BeginInvoke(Callback);
            };
        }

        private void Tost()
        {
            var inAction = _main.Invoker.Mode.CanExecute;
            if (inAction && Game.IsKeyDown(0x11))
            {
                Invoke2();
            }
        }
        private void CustomHotkeyLooper()
        {
            var inAction = _main.Invoker.Mode.CanExecute;
            if (!inAction)
            {
                Invoke2();
            }
        }

        private void ItemOnValueChanged(object sender, OnValueChangeEventArgs args)
        {
            if (args.GetNewValue<KeyBind>().Active)
            {
                UpdateManager.Subscribe(CustomHotkeyLooper, 100);
                /*
                if (!Enable)
                    UpdateManager.BeginInvoke(Callback);*/
            }
            else
            {
                UpdateManager.Unsubscribe(CustomHotkeyLooper);
            }
        }

        public MenuItem<KeyBind> CustomKey { get; set; }

        private void Invoke2()
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
                _main.Invoker.Invoke(one.Ability.Equals(empty2) ? one : two);
            }
            else if (ability2Invoked)
            {
                _main.Invoker.Invoke(two.Ability.Equals(empty2) ? two : one);
            }
            else
            {
                _main.Invoker.Invoke(one);
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
                    await _main.Invoker.InvokeAsync(one);
                else
                    await _main.Invoker.InvokeAsync(two);
            }
            else if (ability2Invoked)
            {
                if (two.Ability.Equals(empty2))
                    await _main.Invoker.InvokeAsync(two);
                else
                    await _main.Invoker.InvokeAsync(one);
            }
            else
            {
                await _main.Invoker.InvokeAsync(one);
            }
        }

        public void OnDeactivate()
        {

        }

        public MenuItem<bool> Enable { get; set; }
    }
}