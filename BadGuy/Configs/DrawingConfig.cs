using System;
using BadGuy.Features;
using Ensage.Common.Menu;
using Ensage.SDK.Handlers;
using Ensage.SDK.Helpers;
using Ensage.SDK.Menu;

namespace BadGuy.Configs
{
    internal class DrawingConfig : IDisposable
    {
        public DrawingConfig(MenuFactory main)
        {
            var draw = main.Menu("Drawing");
            Enable = draw.Item("Enable", false);
            Enable.Item.SetTooltip("Working only with CTRL hotkey");
            Key = draw.Item("Key", new KeyBind(0x11));
            Key.Item.DontSave();
            Rate = draw.Item("Rate", new Slider(40, 1, 1000));
            Speed = draw.Item("Speed", new Slider(4, 1, 10));
            _updateHandler = UpdateManager.Subscribe(DrawingAction.Updater, 0, Enable.Value);
            Enable.Item.ValueChanged += ItemOnValueChanged;
            Key.Item.ValueChanged += DrawingAction.KeyOnPropertyChanged;
        }

        public MenuItem<KeyBind> Key { get; set; }

        private readonly IUpdateHandler _updateHandler;
        private void ItemOnValueChanged(object sender, OnValueChangeEventArgs args)
        {
            if (_updateHandler != null)
                _updateHandler.IsEnabled = args.GetNewValue<bool>();
        }

        public MenuItem<Slider> Rate { get; set; }

        public MenuItem<Slider> Speed { get; set; }

        public MenuItem<bool> Enable { get; set; }
        public void Dispose()
        {
            UpdateManager.Unsubscribe(DrawingAction.Updater);
            Enable.Item.ValueChanged -= ItemOnValueChanged;
            Key.Item.ValueChanged -= DrawingAction.KeyOnPropertyChanged;
        }
    }
}