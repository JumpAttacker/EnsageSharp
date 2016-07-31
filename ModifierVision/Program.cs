namespace ModifierVision
{
    internal class Program
    {
        public static Initialization ModifierMasterInitialization { get; private set; }

        private static void Main()
        {
            ModifierMasterInitialization = new Initialization();
        }
    }
}
