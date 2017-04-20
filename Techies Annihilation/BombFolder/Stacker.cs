namespace Techies_Annihilation.BombFolder
{
    public class Stacker
    {
        public bool IsActive => Counter > 0;
        public int Counter;
        public Stacker()
        {
            Counter = 0;
        }
    }
}