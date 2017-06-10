using System;
using System.Collections.Generic;
using BadGuy.Features;
using Ensage.Common.Menu;
using Ensage.SDK.Helpers;
using Ensage.SDK.Menu;

namespace BadGuy.Configs
{
    internal class PingerConfig : IDisposable
    {
        public PingerConfig(MenuFactory main)
        {
            var pinger = main.Menu("Pinger");
            Enable = pinger.Item("Enable", false);
            Rate = pinger.Item("Rate", new Slider(100, 10, 1000));
            Type = pinger.Item("Type", new StringList("Danger", "Normal"));
            Toggler = pinger.Item("HeroToggler",
                new HeroToggler(new Dictionary<string, bool>(), useAllyHeroes: true, defaultValues: false));
            
            Enable.Item.ValueChanged += ItemOnValueChanged;
        }

        private void ItemOnValueChanged(object sender, OnValueChangeEventArgs args)
        {
            if (args.GetNewValue<bool>())
                UpdateManager.BeginInvoke(Pinger.Updater);
        }
        public MenuItem<StringList> Type { get; set; }

        public MenuItem<bool> Enable { get; set; }

        public MenuItem<HeroToggler> Toggler { get; set; }

        public MenuItem<Slider> Rate { get; set; }
        public void Dispose()
        {
            UpdateManager.Unsubscribe(Pinger.Updater);
            Enable.Item.ValueChanged -= ItemOnValueChanged;
        }
    }
}