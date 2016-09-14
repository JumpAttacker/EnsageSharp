using Ensage;

namespace Wisp_Annihilation
{
    public class Tracker
    {
        public Entity V { get; set; }
        public ParticleEffect Ef { get; set; }

        public Tracker(Entity v,ParticleEffect ef)
        {
            V = v;
            Ef = ef;
            /*Game.OnUpdate += args =>
            {
                Printer.Print($"{ef.Name}/{ef.IsDestroyed}/{ef.IsManaged}/{ef.IsValid}/{ef.RefCount}");
            };*/
        }
    }
}