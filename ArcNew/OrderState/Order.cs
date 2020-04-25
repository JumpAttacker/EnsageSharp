namespace ArcAnnihilation.OrderState
{
    public abstract class Order
    {
        public virtual bool CanBeExecuted => false;
        public virtual bool NeedTarget => false;
        public abstract void Execute();
    }
}