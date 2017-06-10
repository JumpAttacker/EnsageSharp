using System;
using BadGuy.Features;
using Ensage.Common.Menu;
using Ensage.SDK.Handlers;
using Ensage.SDK.Helpers;
using Ensage.SDK.Menu;

namespace BadGuy.Configs
{
    internal class HeroFeederConfig : IDisposable
    {
        public HeroFeederConfig(MenuFactory main)
        {
            var hero = main.Menu("Hero Feeder");
            Enable = hero.Item("Enable", false);
            Type = hero.Item("Order Type", new StringList("Move to", "Attack to"));
            Rate = hero.Item("Rate", new Slider(50, 50, 500));

            _updateHandler = UpdateManager.Subscribe(HeroFeeder.Updater, 0, Enable.Value);
            Enable.Item.ValueChanged += ItemOnValueChanged;
        }
        private readonly IUpdateHandler _updateHandler;
        private void ItemOnValueChanged(object sender, OnValueChangeEventArgs args)
        {
            if (_updateHandler != null)
                _updateHandler.IsEnabled = args.GetNewValue<bool>();
        }

        public MenuItem<StringList> Type { get; }

        public MenuItem<bool> Enable { get; }

        public MenuItem<Slider> Rate { get; }
        public void Dispose()
        {
            UpdateManager.Unsubscribe(HeroFeeder.Updater);
            Enable.Item.ValueChanged -= ItemOnValueChanged;
        }
    }
}