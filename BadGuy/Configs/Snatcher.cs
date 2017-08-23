using System;
using System.Linq;
using BadGuy.Features;
using Ensage;
using Ensage.Common.Menu;
using Ensage.Common.Objects.UtilityObjects;
using Ensage.SDK.Extensions;
using Ensage.SDK.Handlers;
using Ensage.SDK.Helpers;
using Ensage.SDK.Menu;

namespace BadGuy.Configs
{
    internal class Snatcher : IDisposable
    {
        private readonly Sleeper _sleeper;
        public Snatcher(MenuFactory main)
        {
            var pinger = main.Menu("Snatcher");
            Enable = pinger.Item("Enable", false);
            _sleeper = new Sleeper();
            _updateHandler = UpdateManager.Subscribe(Updater, 0, Enable.Value);
            Enable.Item.ValueChanged += ItemOnValueChanged;
        }

        private void Updater()
        {
            if (_sleeper.Sleeping)
                return;
            var me = ObjectManager.LocalHero;
            var rune =
                EntityManager<Rune>.Entities.FirstOrDefault(x => x.IsValid && x.Distance2D(me) <= 200);
            if (rune == null) return;
            me.PickUpRune(rune);
            _sleeper.Sleep(150);
        }

        private readonly IUpdateHandler _updateHandler;
        private void ItemOnValueChanged(object sender, OnValueChangeEventArgs args)
        {
            if (_updateHandler != null)
                _updateHandler.IsEnabled = args.GetNewValue<bool>();
        }

        public MenuItem<bool> Enable { get; set; }
        public void Dispose()
        {
            UpdateManager.Unsubscribe(Laugh.Updater);
            Enable.Item.ValueChanged -= ItemOnValueChanged;
        }
    }
}