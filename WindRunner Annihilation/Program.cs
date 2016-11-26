using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common;

namespace WindRunner_Annihilation
{
    internal class Program
    {
        public static Core MyLittleCore { get; private set; }
        private static void Main()
        {
            Events.OnLoad += (sender, args) =>
            {
                if (ObjectManager.LocalHero.ClassID != Members.MyClassId)
                    return;
                MyLittleCore = new Core();
            };
        }
    }
}
