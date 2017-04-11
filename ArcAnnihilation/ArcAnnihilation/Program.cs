using System;
using Ensage;
using Ensage.Common;

// ReSharper disable UnusedMember.Local

namespace ArcAnnihilation
{
    internal class Program
    {
        public static Core MyLittleCore { get; private set; }

        private static void Main()
        {
            Events.OnLoad += (sender, args) =>
            {
                if (ObjectManager.LocalHero.ClassId != ClassId.CDOTA_Unit_Hero_ArcWarden)
                    return;
                MyLittleCore = new Core();
            };
            Events.OnClose += (sender, args) =>
            {
                try
                {
                    Core.Menu.RemoveFromMainMenu();
                    MyLittleCore.UnLink();
                }
                catch (Exception)
                {
                    // ignored
                }
            };
        }
    }
}