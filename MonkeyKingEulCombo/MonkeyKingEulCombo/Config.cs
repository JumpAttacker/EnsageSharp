using System;
using Ensage.Common.Menu;
using Ensage.SDK.Menu;

namespace MonkeyKingEulCombo
{
    public class Config : IDisposable
    {
        public MenuItem<KeyBind> Key;

        public Config()
        {
            Factory = MenuFactory.Create("MonkeyKing Eul Combo");
            Key = Factory.Item("Combo Key", new KeyBind('0'));
            ExtraTime = Factory.Item("Timing corrector", new Slider(0, -50, 50));
        }

        public MenuItem<Slider> ExtraTime { get; set; }

        public MenuFactory Factory { get; }

        public void Dispose()
        {
            Factory?.Dispose();
        }
    }
}