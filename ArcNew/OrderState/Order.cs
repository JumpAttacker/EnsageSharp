namespace ArcAnnihilation.OrderState
{
    public abstract class Order
    {
        public abstract void Execute();
        public virtual bool CanBeExecuted => true;
        public virtual bool NeedTarget => false;
    }
}