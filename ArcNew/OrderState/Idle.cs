namespace ArcAnnihilation.OrderState
{
    public class Idle : Order
    {
        public override bool CanBeExecuted => false;
        public override void Execute()
        {
            
        }
    }
}