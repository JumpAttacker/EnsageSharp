using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.SDK.Extensions;
using Ensage.SDK.Menu;

namespace EarthSpiritCrappa
{
    public class Config : IDisposable
    {
        private readonly EarthSpiritCrappa _main;
        private readonly MenuFactory _factory;

        public Config(EarthSpiritCrappa earthSpiritCrappa)
        {
            _main = earthSpiritCrappa;
            _factory = MenuFactory.Create("Earth Spirit Crappahilation");
            ComboKey = _factory.Item("Combo key", new KeyBind('0'));
            PushKey = _factory.Item("Smash key", new KeyBind('0'));
            RollingKey = _factory.Item("Rolling key", new KeyBind('0'));
            PullKey = _factory.Item("Grip key", new KeyBind('0'));
            EnablePrediction = _factory.Item("Use prediction", false);

            var abilities =
                _main.Owner.Spellbook.Spells.Where(
                        x =>
                            !x.IsHidden && (x.AbilityType == AbilityType.Basic || x.AbilityType == AbilityType.Ultimate))
                    .OrderByDescending(x => x.AbilitySlot)
                    .Select(x => x.Id)
                    .ToList();
            var dict = abilities.ToDictionary(id => id.ToString(), x => true);
            AbilitiesInCombo = _factory.Item("Abilities:", new AbilityToggler(dict));
            Dictionary<string, bool> dict2 = new Dictionary<string, bool>()
            {
                {AbilityId.item_blink.ToString(), true},
                {AbilityId.item_sheepstick.ToString(), true},
                {AbilityId.item_cyclone.ToString(), true},
                {AbilityId.item_veil_of_discord.ToString(), true},
                {AbilityId.item_shivas_guard.ToString(), true},
                {AbilityId.item_heavens_halberd.ToString(), true},
                {AbilityId.item_lotus_orb.ToString(), true},
            };
            ItemsInCombo = _factory.Item("Items:", new AbilityToggler(dict2));
        }

        public MenuItem<AbilityToggler> AbilitiesInCombo { get; set; }
        public MenuItem<AbilityToggler> ItemsInCombo { get; set; }

        public MenuItem<bool> EnablePrediction { get; set; }

        public MenuItem<KeyBind> PushKey { get; set; }
        public MenuItem<KeyBind> RollingKey { get; set; }
        public MenuItem<KeyBind> PullKey { get; set; }

        public MenuItem<KeyBind> ComboKey { get; set; }

        public void Dispose()
        {
            _factory?.Dispose();
        }
    }
}