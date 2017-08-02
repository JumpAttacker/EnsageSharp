using System;
using Ensage.Common;
using Ensage.Common.Objects;
using SharpDX;

namespace InvokerAnnihilationCrappa
{
    internal class Program
    {
        private static void Main()
        {

        }

        public static void GenerateSideMessage(string hero, string spellName)
        {
            try
            {
                var msg = new SideMessage(hero, new Vector2(226, 59), stayTime: 2000);
                msg.AddElement(new Vector2(9, 9), new Vector2(73, 41), Textures.GetTexture("materials/ensage_ui/heroes_horizontal/" + hero + ".vmat"));
                msg.AddElement(new Vector2(97, 2), new Vector2(50, 50), Textures.GetTexture("materials/ensage_ui/other/arrow_usual.vmat"));
                msg.AddElement(new Vector2(163, 9), new Vector2(75, 41), Textures.GetTexture("materials/ensage_ui/spellicons/" + spellName + ".vmat"));
                msg.CreateMessage();
            }
            catch (Exception)
            {

            }


        }
    }
}
