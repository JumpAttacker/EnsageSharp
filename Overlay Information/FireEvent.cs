using Ensage;

namespace OverylayInformationV2
{
    internal static class FireEvent
    {
        public static void Game_OnGameEvent(FireEventEventArgs args)
        {
            if (!Checker.IsActive()) return;
            if (args.GameEvent.Name == "dota_roshan_kill")
            {
                Members.DeathTime = Game.GameTime;
                Members.RoshIsAlive = false;
            }

        }
    }
}