using System;
using Ensage.Common;
using Ensage.Common.Objects;
using SharpDX;

namespace OverlayInformation
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
                msg.AddElement(new Vector2(9, 9), new Vector2(73, 41), Textures.GetHeroTexture(hero));
                msg.AddElement(new Vector2(97, 2), new Vector2(50, 50), Textures.GetTexture("materials/ensage_ui/other/arrow_usual.vmat"));
                msg.AddElement(new Vector2(163, 9), new Vector2(75, 41), Textures.GetSpellTexture(spellName));
                msg.CreateMessage();
            }
            catch (Exception)
            {
            }

        }
        public static void GenerateTpCatcherSideMessage(string hero, string itemName, int d)
        {
            try
            {
                var msg = new SideMessage(hero, new Vector2(170, 59), stayTime: d);
                msg.AddElement(new Vector2(9, 9), new Vector2(73, 41), Textures.GetHeroTexture(hero));
                msg.AddElement(new Vector2(110, 9), new Vector2(73, 41), Textures.GetItemTexture(itemName));
                msg.CreateMessage();
            }
            catch (Exception)
            {
            }

        }
    }
}
