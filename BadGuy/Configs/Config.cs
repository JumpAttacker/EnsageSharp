using System;
using Ensage.SDK.Menu;

namespace BadGuy.Configs
{
    internal class Config : IDisposable
    {
        public readonly MenuFactory Factory;
        public HeroFeederConfig HeroFeeder;
        public CourierConfig Courier;
        public DrawingConfig Drawing;
        public LaughConfig Laugh;
        public PingerConfig Pinger;

        public Config()
        {
            Factory = MenuFactory.Create("Bad Guy", "BadGuy");
            HeroFeeder = new HeroFeederConfig(Factory);
            Courier = new CourierConfig(Factory);
            Drawing = new DrawingConfig(Factory);
            Laugh = new LaughConfig(Factory);
            Pinger = new PingerConfig(Factory);
        }

        public void Dispose()
        {
            HeroFeeder?.Dispose();
            Courier?.Dispose();
            Drawing?.Dispose();
            Pinger?.Dispose();
            Laugh?.Dispose();
            Factory?.Dispose();
        }
    }
}