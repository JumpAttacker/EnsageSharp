using System;
using BadGuy.Features;
using Ensage.Common.Menu;
using Ensage.SDK.Handlers;
using Ensage.SDK.Helpers;
using Ensage.SDK.Menu;

namespace BadGuy.Configs
{
    internal class LaughConfig : IDisposable
    {
        public LaughConfig(MenuFactory main)
        {
            var laugh = main.Menu("Laugh");
            Enable = laugh.Item("Enable", false);
            Rate = laugh.Item("Rate", new Slider(20, 20));
            _updateHandler = UpdateManager.Subscribe(Laugh.Updater, 0, Enable.Value);
            Enable.Item.ValueChanged += ItemOnValueChanged;
        }
        private readonly IUpdateHandler _updateHandler;
        private void ItemOnValueChanged(object sender, OnValueChangeEventArgs args)
        {
            if (_updateHandler != null)
                _updateHandler.IsEnabled = args.GetNewValue<bool>();
        }

        public MenuItem<Slider> Rate { get; set; }

        public MenuItem<bool> Enable { get; set; }
        public void Dispose()
        {
            UpdateManager.Unsubscribe(Laugh.Updater);
            Enable.Item.ValueChanged -= ItemOnValueChanged;
        }
    }
}