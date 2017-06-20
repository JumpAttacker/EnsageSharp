using System;
using Ensage.Common.Menu;
using Ensage.SDK.Menu;

namespace EmberAnnihilation
{
    public class Config : IDisposable
    {
        public Config()
        {
            Factory = MenuFactory.Create("Ember Annihilation");
            FistAndComboKey = Factory.Item("Fist + Chain Key", new KeyBind('F'));
            RemntantCombo = Factory.Item("3x Remntant Combo", new KeyBind('D'));
            PussyKey = Factory.Item("Pussy key", new KeyBind('G'));
            AutoChain = Factory.Item("Auto chain in fist", true);
        }

        public MenuItem<KeyBind> PussyKey { get; set; }

        public MenuItem<bool> AutoChain { get; set; }

        public MenuItem<KeyBind> RemntantCombo { get; set; }

        public MenuFactory Factory { get; }

        public MenuItem<KeyBind> FistAndComboKey { get; }

        public void Dispose()
        {
            Factory?.Dispose();
        }
    }
}