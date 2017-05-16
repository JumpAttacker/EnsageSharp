using ArcAnnihilation.OrderState;
using ArcAnnihilation.Utils;

namespace ArcAnnihilation
{
    public static class OrderManager
    {
        public static Order CurrentOrder;

        static OrderManager()
        {
            Orders.AutoPushing = new AutoPushing();
            Orders.DefaultCombo = new DefaultCombo();
            Orders.Idle = new Idle();
            Orders.SparkSpam = new SparkSpam();
            Orders.SparkSpamTempest = new SparkSpamTempest();
            Orders.TempestCombo = new TempestCombo();

            ChangeOrder(Orders.Idle);
        }

        public static bool CanBeExecuted => CurrentOrder.CanBeExecuted;

        public static void ChangeOrder(Order setOrder)
        {
            if (CurrentOrder == setOrder)
            {
                Printer.Both($"[Order][Error] {setOrder}");
                return;
            }
            Printer.Both(CurrentOrder != null
                ? $"[Order] changed from {CurrentOrder} to {setOrder}"
                : $"[Order][Init] {setOrder}");
            CurrentOrder = setOrder;
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