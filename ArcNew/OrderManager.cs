using System.Collections.Generic;
using System.Threading.Tasks;
using ArcAnnihilation.OrderState;
using ArcAnnihilation.Utils;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;

namespace ArcAnnihilation
{
    public static class OrderManager
    {
        public static Order CurrentOrder;
        public static List<Order> OrderList;
        static OrderManager()
        {
            Orders.AutoPushing = new AutoPushing();
            Orders.DefaultCombo = new DefaultCombo();
            Orders.Idle = new Idle();
            Orders.SparkSpam = new SparkSpam();
            Orders.SparkSpamTempest = new SparkSpamTempest();
            Orders.TempestCombo = new TempestCombo();
            OrderList = new List<Order>
            {
                Orders.AutoPushing,
                Orders.DefaultCombo,
                Orders.SparkSpam,
                Orders.SparkSpamTempest,
                Orders.TempestCombo
            };
            ChangeOrder(Orders.Idle);
        }

        public static bool CanBeExecuted => CurrentOrder.CanBeExecuted;
        private static bool _changed;

        public static async void ChangeOrder(Order setOrder)
        {
            if (CurrentOrder == setOrder)
            {
                Printer.Both($"[Order][Error] {setOrder}");
                return;
            }
            await Task.Delay(5);
            if (setOrder is TempestCombo &&
                (MenuManager.AutoSummonOnTempestCombog || MenuManager.IsSummmoningAndCombing) ||
                setOrder is AutoPushing && (MenuManager.AutoSummonOnPusing || MenuManager.IsSummmoningAndPushing))
            {
                if (Core.MainHero.TempestDouble.CanBeCasted())
                {
                    Core.MainHero.TempestDouble.UseAbility();
                }
            }
            Printer.Both(CurrentOrder != null
                ? $"[Order] changed from {CurrentOrder} to {setOrder}"
                : $"[Order][Init] {setOrder}");
            CurrentOrder = setOrder;
            //dota_player_teleport_requires_halt
            /*var reqh = Game.GetConsoleVar("dota_player_teleport_requires_halt");
            if (reqh.GetInt() == 0)
                reqh.SetValue(1);*/
        }

        public static class Orders
        {
            public static AutoPushing AutoPushing;
            public static DefaultCombo DefaultCombo;
            public static Idle Idle;
            public static SparkSpam SparkSpam;
            public static SparkSpamTempest SparkSpamTempest;
            public static TempestCombo TempestCombo;
        }
    }
}