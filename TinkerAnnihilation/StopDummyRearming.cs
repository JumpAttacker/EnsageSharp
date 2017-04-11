using Ensage;
using Ensage.Common.Extensions;

namespace TinkerAnnihilation
{
    public static class StopDummyRearming
    {
        private static bool Block => Members.Menu.Item("Block.rearm").GetValue<bool>();
        public static void OnExecuteOrder(Player sender, ExecuteOrderEventArgs args)
        {
            if (!Block) return;
            if (args.Ability?.Name == "tinker_rearm" && args.OrderId == OrderId.Ability &&
                (Members.MyHero.IsChanneling() || args.Ability.IsInAbilityPhase))
            {
                args.Process = false;
            }
        }
    }
}