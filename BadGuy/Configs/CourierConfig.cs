using System;
using System.Collections.Generic;
using BadGuy.Features;
using Ensage.Common.Menu;
using Ensage.Common.Menu.MenuItems;
using Ensage.SDK.Handlers;
using Ensage.SDK.Helpers;
using Ensage.SDK.Menu;

namespace BadGuy.Configs
{
    internal class CourierConfig : IDisposable
    {
        public enum OrderType
        {
            BlockingOnBase, GoToEnemyBase, MoveItemsToStash, GiveItemsToMainHero, BlockForSelectedHero
        }
        public CourierConfig(MenuFactory main)
        {
            var courier = main.Menu("Courier");
            Enable = courier.Item("Enable", false);
            Type = courier.Item("Order Type",
                new StringList("blocking on base", "go to the enemy base", "move items to stash",
                    "give items to main hero", "block for selected hero"));
            Rate = courier.Item("Rate", new Slider(50, 5, 500));
            MutedHeroes = courier.Item("Muted:",
                new HeroToggler(new Dictionary<string, bool>(), useAllyHeroes: true, defaultValues: false));
            MutedHeroes.Item.DontSave();
            ExtraSettingsForMute = courier.Item("Transfer items to main hero or just return to base", true);
            _updateHandler = UpdateManager.Subscribe(CourierAction.Updater, 0, Enable.Value);
            Enable.Item.ValueChanged += ItemOnValueChanged;
        }

        public MenuItem<bool> ExtraSettingsForMute { get; set; }

        public MenuItem<HeroToggler> MutedHeroes { get; set; }


        private readonly IUpdateHandler _updateHandler;
        private void ItemOnValueChanged(object sender, OnValueChangeEventArgs args)
        {
            if (_updateHandler != null)
                _updateHandler.IsEnabled = args.GetNewValue<bool>();
        }
        public MenuItem<Slider> Rate { get; set; }

        public MenuItem<StringList> Type { get; set; }

        public MenuItem<bool> Enable { get; set; }
        public void Dispose()
        {
            UpdateManager.Unsubscribe(CourierAction.Updater);
            Enable.Item.ValueChanged -= ItemOnValueChanged;
        }
    }
}