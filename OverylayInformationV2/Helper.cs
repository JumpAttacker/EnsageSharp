using System;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Enums;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using SharpDX;

namespace OverlayInformation
{
    internal static class Helper
    {
        private const float MapLeft = -8000;
        private const float MapTop = 7350;
        private const float MapRight = 7500;
        private const float MapBottom = -7200;
        private static readonly float MapWidth = Math.Abs(MapLeft - MapRight);
        private static readonly float MapHeight = Math.Abs(MapBottom - MapTop);
        public static Vector2 WorldToMinimap(Vector3 pos)
        {
            return pos.WorldToMinimap();

            #region OldShit

            var x = pos.X - MapLeft;
            var y = pos.Y - MapBottom;

            float dx, dy, px, py;
            if (Math.Round((float) Drawing.Width/Drawing.Height, 1) >= 1.7)
            {
                dx = 272f/1920f*Drawing.Width;
                dy = 261f/1080f*Drawing.Height;
                px = 11f/1920f*Drawing.Width;
                py = 11f/1080f*Drawing.Height;
            }
            else if (Math.Round((float) Drawing.Width/Drawing.Height, 1) >= 1.5)
            {
                dx = 267f/1680f*Drawing.Width;
                dy = 252f/1050f*Drawing.Height;
                px = 10f/1680f*Drawing.Width;
                py = 11f/1050f*Drawing.Height;
            }
            else
            {
                dx = 255f/1280f*Drawing.Width;
                dy = 229f/1024f*Drawing.Height;
                px = 6f/1280f*Drawing.Width;
                py = 9f/1024f*Drawing.Height;
            }
            var minimapMapScaleX = dx/MapWidth;
            var minimapMapScaleY = dy/MapHeight;

            var scaledX = Math.Min(Math.Max(x*minimapMapScaleX, 0), dx);
            var scaledY = Math.Min(Math.Max(y*minimapMapScaleY, 0), dy);

            var screenX = px + scaledX;
            var screenY = Drawing.Height - scaledY - py;

            return new Vector2((float) Math.Floor(screenX), (float) Math.Floor(screenY));

            #endregion

        }

        private static int AutoItemsStickHealth => Members.Menu.Item("autoItems.Percent").GetValue<Slider>().Value;
        private static int AutoItemsStickMana => Members.Menu.Item("autoItems.Percent2").GetValue<Slider>().Value;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="checkForHealth">true -> health; false -> mana</param>
        /// <returns></returns>
        public static bool CheckForPercents(Hero x,bool checkForHealth=true)
        {
            var health = checkForHealth ? x.Health : x.Mana;
            var maxHealth = checkForHealth ? x.MaximumHealth : x.MaximumMana;
            var percent = (float)((float)health / (float)maxHealth * 100);
            //Printer.Print($"H[{health}]MH[{maxHealth}]Min%[{MinPercHealthForRelocate}]Final%[{percent}]");
            return percent <= (checkForHealth ? AutoItemsStickHealth : AutoItemsStickMana);
        }
        /// <summary>
        /// Returns an item by using its internal ID.
        /// </summary>
        /// <param name="owner">Owner unit.</param>
        /// <param name="itemId">The item ID of the wanted item.</param>
        /// <returns></returns>
        public static Ability GetItemById(this Unit owner, ItemId itemId)
        {
            return owner.Inventory.Items.FirstOrDefault(x => x!=null && x.IsValid && (uint)x.AbilityData.Id == (uint)itemId);
        }
        public static void GenerateSideMessage(string hero, string spellName)
        {
            try
            {
                var msg = new SideMessage(hero, new Vector2(226, 59),stayTime:2000);
                msg.AddElement(new Vector2(9, 9), new Vector2(73, 41), Textures.GetTexture("materials/ensage_ui/heroes_horizontal/" + hero + ".vmat"));
                msg.AddElement(new Vector2(97, 2), new Vector2(50, 50), Textures.GetTexture("materials/ensage_ui/other/arrow_usual.vmat"));
                msg.AddElement(new Vector2(163, 9), new Vector2(75, 41), Textures.GetTexture("materials/ensage_ui/spellicons/" + spellName + ".vmat"));
                msg.CreateMessage();
            }
            catch (Exception)
            {
                Printer.Print($"[SideMessage]: {hero} -> {spellName}");
            }

        }
        public static void GenerateTpCatcherSideMessage(string hero, string itemName, int d)
        {
            try
            {
                var msg = new SideMessage(hero, new Vector2(170, 59),stayTime:d);
                msg.AddElement(new Vector2(9, 9), new Vector2(73, 41), Textures.GetHeroTexture(hero));
                msg.AddElement(new Vector2(110, 9), new Vector2(73, 41), Textures.GetItemTexture(itemName));
                msg.CreateMessage();
            }
            catch (Exception)
            {
                Printer.Print($"[TpCatcher.SideMessage]: {hero} -> {itemName}");
            }

        }
        public static Vector3 FindVector(Vector3 first, double ret, float distance)
        {
            var retVector = new Vector3(first.X + (float)Math.Cos(Utils.DegreeToRadian(ret)) * distance, first.Y + (float)Math.Sin(Utils.DegreeToRadian(ret)) * distance, 100);

            return retVector;
        }

        public static double FindRet(Vector3 first, Vector3 second)
        {
            var xAngle = Utils.RadianToDegree(Math.Atan(Math.Abs(second.X - first.X) / Math.Abs(second.Y - first.Y)));
            if (first.X <= second.X && first.Y >= second.Y)
            {
                return 270 + xAngle;
            }
            if (first.X >= second.X & first.Y >= second.Y)
            {
                return 90 - xAngle + 180;
            }
            if (first.X >= second.X && first.Y <= second.Y)
            {
                return 90 + xAngle;
            }
            if (first.X <= second.X && first.Y <= second.Y)
            {
                return 90 - xAngle;
            }
            return 0;
        }

        private static int IllusionType => Members.Menu.Item("showillusion.Type").GetValue<StringList>().SelectedIndex;
        private static int IllusionAlpha => Members.Menu.Item("showillusion.Alpha").GetValue<Slider>().Value;
        private static int IllusionSize => Members.Menu.Item("showillusion.Size").GetValue<Slider>().Value;
        private static Vector3 IllusionColor
            =>
                new Vector3(Members.Menu.Item("showillusion.X").GetValue<Slider>().Value,
                    Members.Menu.Item("showillusion.Y").GetValue<Slider>().Value,
                    Members.Menu.Item("showillusion.Z").GetValue<Slider>().Value);
        public static void HandleEffect(Unit unit)
        {
            ParticleEffect effect;
            ParticleEffect effect2;
            if (unit.IsAlive /* && unit.IsVisibleToEnemies*/)
            {
                if (Members.Effects1.TryGetValue(unit, out effect)) return;
                var id = IllusionType;
                effect = unit.AddParticleEffect(Members.ShowIllusionList[id]);
                switch (id)
                {
                    case 0:
                        effect2 = unit.AddParticleEffect(Members.ShowIllusionList[Members.ShowIllusionList.Count-1]);
                        Members.Effects2.Add(unit, effect2);
                        break;
                    default:
                        effect.SetControlPoint(1, new Vector3(IllusionSize, IllusionAlpha, 0));
                        effect.SetControlPoint(2, IllusionColor);
                        break;
                }
                
                Members.Effects1.Add(unit, effect);
            }
            else
            {
                if (!Members.Effects1.TryGetValue(unit, out effect)) return;
                effect.Dispose();
                
                Members.Effects1.Remove(unit);
                if (!Members.Effects2.TryGetValue(unit, out effect2)) return;
                effect2.Dispose();
                Members.Effects2.Remove(unit);
            }
        }
        
        public static Vector2 GetTopPanelPosition(Hero v)
        {
            try
            {
                Vector2 pos;
                if (Members.TopPanelPostiion.TryGetValue(v.StoredName(), out pos))
                {
                    return pos;
                }
                Members.TopPanelPostiion.Add(v.StoredName(), HudInfoNew.GetTopPanelPosition(v));
                //Members.TopPanelPostiion.Add(v.StoredName(), HUDInfo.GetTopPanelPosition(v));
                return HudInfoNew.GetTopPanelPosition(v);
                //return HUDInfo.GetTopPanelPosition(v);
            }
            catch (Exception e)
            {
                Printer.Print("GetTopPanelPosition: "+e.Message);
                return new Vector2();
            }
            
        }

        public static DotaTexture GetHeroTextureMinimap(string heroName)
        {
            try
            {
                if (!heroName.Contains("npc_dota_hero_")) return Textures.GetHeroTexture(heroName);
                var name = "materials/ensage_ui/miniheroes/" + heroName.Substring("npc_dota_hero_".Length) + ".vmat";
                return Textures.GetTexture(name);
            }
            catch (Exception)
            {
                return Drawing.GetTexture("materials/ensage_ui/spellicons/doom_bringer_empty1");
            }
            
        }
    }
}