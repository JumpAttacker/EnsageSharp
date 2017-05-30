using Ensage;
using Ensage.Common.Objects.UtilityObjects;

namespace BadGuy.Features
{
    public static class Laugh
    {
        private static readonly Sleeper RateSleeper = new Sleeper();
        public static void Updater()
        {
            if (RateSleeper.Sleeping)
                return;
            RateSleeper.Sleep(BadGuy.Config.Laugh.Rate*1000);
            Game.ExecuteCommand("say \"/laugh \"");
        }
    }
}