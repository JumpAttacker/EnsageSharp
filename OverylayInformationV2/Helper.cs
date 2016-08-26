using System;
using Ensage;
using Ensage.Common;
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
            var x = pos.X - MapLeft;
            var y = pos.Y - MapBottom;

            float dx, dy, px, py;
            if (Math.Round((float)Drawing.Width / Drawing.Height, 1) >= 1.7)
            {
                dx = 272f / 1920f * Drawing.Width;
                dy = 261f / 1080f * Drawing.Height;
                px = 11f / 1920f * Drawing.Width;
                py = 11f / 1080f * Drawing.Height;
            }
            else if (Math.Round((float)Drawing.Width / Drawing.Height, 1) >= 1.5)
            {
                dx = 267f / 1680f * Drawing.Width;
                dy = 252f / 1050f * Drawing.Height;
                px = 10f / 1680f * Drawing.Width;
                py = 11f / 1050f * Drawing.Height;
            }
            else
            {
                dx = 255f / 1280f * Drawing.Width;
                dy = 229f / 1024f * Drawing.Height;
                px = 6f / 1280f * Drawing.Width;
                py = 9f / 1024f * Drawing.Height;
            }
            var minimapMapScaleX = dx / MapWidth;
            var minimapMapScaleY = dy / MapHeight;

            var scaledX = Math.Min(Math.Max(x * minimapMapScaleX, 0), dx);
            var scaledY = Math.Min(Math.Max(y * minimapMapScaleY, 0), dy);

            var screenX = px + scaledX;
            var screenY = Drawing.Height - scaledY - py;

            return new Vector2((float)Math.Floor(screenX), (float)Math.Floor(screenY));
        }

        public static void GenerateSideMessage(string hero, string spellName)
        {
            try
            {
                var msg = new SideMessage(hero, new Vector2(226, 59));
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

        public static void HandleEffect(Unit unit)
        {
            ParticleEffect effect;
            ParticleEffect effect2;
            if (unit.IsAlive /* && unit.IsVisibleToEnemies*/)
            {
                if (Members.Effects1.TryGetValue(unit, out effect)) return;
                effect = unit.AddParticleEffect("particles/items2_fx/smoke_of_deceit_buff.vpcf");
                //particles/items_fx/diffusal_slow.vpcf
                effect2 = unit.AddParticleEffect("particles/items2_fx/shadow_amulet_active_ground_proj.vpcf");
                Members.Effects1.Add(unit, effect);
                Members.Effects2.Add(unit, effect2);
            }
            else
            {
                if (!Members.Effects1.TryGetValue(unit, out effect)) return;
                if (!Members.Effects2.TryGetValue(unit, out effect2)) return;
                effect.Dispose();
                effect2.Dispose();
                Members.Effects1.Remove(unit);
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
                Members.TopPanelPostiion.Add(v.StoredName(), HUDInfo.GetTopPanelPosition(v));
                return HUDInfo.GetTopPanelPosition(v);
            }
            catch (Exception e)
            {
                Printer.Print("GetTopPanelPosition: "+e.Message);
                return new Vector2();
            }
            
        }

        public static DotaTexture GetHeroTextureMinimap(string heroName)
        {
            var name = "materials/ensage_ui/miniheroes/" + heroName.Substring("npc_dota_hero_".Length) + ".vmat";

            return Textures.GetTexture(name);
        }
    }
}