using System.Collections.Generic;
using ArcAnnihilation.OrderState;
using ArcAnnihilation.Utils;

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
        public static void ChangeOrder(Order setOrder)
        {
            if (CurrentOrder == setOrder)
            {
                Printer.Both($"[Order][Error] {setOrder}");
                return;
            }
            if (CurrentOrder is AutoPushing)
            {
                /*if (_changed)
                {
                    _changed = false;
                    var reqh = Game.GetConsoleVar("dota_player_teleport_requires_halt");
                    reqh?.SetValue(0);
                }*/
            }
            else if (setOrder is AutoPushing)
            {
                /*var reqh = Game.GetConsoleVar("dota_player_teleport_requires_halt");
                Game.PrintMessage((reqh!=null).ToString());
                if (Game.GetConsoleVar("dota_player_teleport_requires_halt").GetInt() == 0)
                {
                    
                    reqh.SetValue(1);
                    _changed = true;
                }*/
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