using System.Linq;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.SDK.Extensions;
using Ensage.SDK.Helpers;
using Ensage.SDK.Menu;

namespace InvokerAnnihilationCrappa.Features
{
    public class AutoGhostWalk
    {
        private readonly Config _main;

        public AutoGhostWalk(Config main)
        {
            _main = main;
            var panel = main.Factory.Menu("Ghost Walk Key");
            Enable = panel.Item("Enable", false);
            MinHealth = panel.Item("Min Health for auto ghost walk", new Slider(15, 1, 100));
            MinUnits = panel.Item("Min Units for auto ghost walk", new Slider(3, 1, 5));
            Range = panel.Item("Range", new Slider(1100, 500, 2000));
            CustomKey = panel.Item("Ghost walk key", new KeyBind('0'));

            CustomKey.Item.ValueChanged += ItemOnValueChanged;
            if (Enable)
            {
                UpdateManager.BeginInvoke(Callback);
            }

            Enable.Item.ValueChanged += (sender, args) =>
            {
                if (args.GetNewValue<bool>())
                    UpdateManager.BeginInvoke(Callback);
            };
        }

        public MenuItem<Slider> Range { get; set; }

        public MenuItem<Slider> MinUnits { get; set; }

        public MenuItem<Slider> MinHealth { get; set; }

        private void ItemOnValueChanged(object sender, OnValueChangeEventArgs e)
        {
            UpdateManager.BeginInvoke(CustomGhostWalk);
        }

        private async Task TryToInvis()
        {
            var invis = _main.Invoker.GhostWalk;
            if (invis.Ability.CanBeCasted())
            {
                invis.Ability.UseAbility();
                await Task.Delay(1000);
            }
            else if (invis.Ability.AbilityState == AbilityState.Ready)
            {
                await _main.Invoker.Invoke(_main.Invoker.GhostWalk);
            }
        }

        private async void CustomGhostWalk()
        {
            await TryToInvis();
        }

        private async void Callback()
        {
            while (Enable)
            {
                var me = _main.Invoker.Owner;
                if (me.IsAlive)
                {
                    var health = me.HealthPercent();
                    if (health * 100 <= MinHealth)
                    {
                        var enemyHeroes =
                            EntityManager<Hero>.Entities.Count(
                                x =>
                                    x.IsAlive && !x.IsIllusion && !x.IsAlly(me) &&
                                    x.IsInRange(me, Range));
                        if (enemyHeroes >= MinUnits)
                        {
                            await TryToInvis();
                        }
                    }
                }
                await Task.Delay(200);
            }
        }

        public MenuItem<KeyBind> CustomKey { get; set; }

        public MenuItem<bool> Enable { get; set; }
    }
}