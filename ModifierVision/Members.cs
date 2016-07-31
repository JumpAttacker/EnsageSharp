using System.Collections.Generic;
using Ensage;
using Ensage.Common.Menu;

namespace ModifierVision
{
    internal class Members
    {
        public static readonly Menu Menu = new Menu("Modifier Vision", "MV", true);
        public static Hero MyHero;
        public static List<HeroModifier> System;
    }
}