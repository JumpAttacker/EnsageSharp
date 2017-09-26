using Ensage;
using Ensage.Common.Objects.UtilityObjects;

namespace BadGuy.Features
{
    public static class Taunt
    {
        private static readonly Sleeper RateSleeper = new Sleeper();
        public static void Updater()
        {
            if (RateSleeper.Sleeping)
                return;
            RateSleeper.Sleep(BadGuy.Config.TauntConfig.Rate*1000);
            Game.ExecuteCommand("use_item_client current_hero taunt");
        }
    }
}