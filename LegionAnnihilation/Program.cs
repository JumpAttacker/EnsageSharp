using Ensage;
using Ensage.Common;

namespace Legion_Annihilation
{
    internal static class Program
    {
        public static Core MyLittleCore { get; private set; }
        private static void Main()
        {
            Events.OnLoad += (sender, args) =>
            {
                if (ObjectManager.LocalHero.ClassId != Members.MyClassId)
                    return;
                MyLittleCore = new Core();
            };
        }
    }
}