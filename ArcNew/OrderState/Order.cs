namespace ArcAnnihilation.OrderState
{
    public abstract class Order
    {
        public abstract void Execute();
        public virtual bool CanBeExecuted => false;
        public virtual bool NeedTarget => false;
    }
}