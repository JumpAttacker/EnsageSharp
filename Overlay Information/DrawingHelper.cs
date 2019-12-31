using Ensage;
using Ensage.SDK.Menu;
using SharpDX;

namespace OverlayInformation
{
    public static class DrawingHelper
    {
        public static Vector2 DrawBar(Vector2 pos, string text, Vector2 maxSize, DotaTexture texture, Color clr,
            Color color)
        {
            var textSize = Drawing.MeasureText(text, "Arial",
                new Vector2((float) (maxSize.Y * .7), 0), FontFlags.AntiAlias | FontFlags.StrikeOut);
            Drawing.DrawRect(pos, maxSize, texture);
            Drawing.DrawRect(pos, maxSize, color);
            var textPos = pos + new Vector2(maxSize.X / 2 - textSize.X / 2, maxSize.Y / 2 - textSize.Y / 2);
            Drawing.DrawText(
                text, "Arial",
                textPos, new Vector2(textSize.Y, 0),
                clr,
                FontFlags.AntiAlias | FontFlags.StrikeOut);

            Drawing.DrawRect(pos, maxSize, Color.Black, true);
            return pos + new Vector2(0, maxSize.Y);
        }


        public static Vector2 DrawBar(Vector2 pos, float value, Vector2 maxSize, Color clrOn, Color clrOff)
        {
            Drawing.DrawRect(pos, maxSize, clrOff);
            Drawing.DrawRect(pos, new Vector2(value, maxSize.Y), clrOn);
            Drawing.DrawRect(pos, maxSize, Color.Black, true);
            return pos + new Vector2(0, maxSize.Y);
        }

        public static Vector2 DrawBar(Vector2 pos, string text, Vector2 maxSize, Color clr)
        {
            /*var size = new Vector2(maxSize.X, maxSize.Y * 2f);
            Drawing.DrawRect(pos, size, new Color(155, 155, 155, 155));
            Drawing.DrawText(
                text, "Arial",
                pos + new Vector2(5, 0), maxSize / 4,
                clr,
                FontFlags.AntiAlias | FontFlags.StrikeOut);
            Drawing.DrawRect(pos, size, Color.Black, true);*/

            var textSize = Drawing.MeasureText(text, "Arial",
                new Vector2((float) (maxSize.Y * .95), 0), FontFlags.AntiAlias | FontFlags.StrikeOut);
            var textPos = pos + new Vector2(maxSize.X / 2 - textSize.X / 2, maxSize.Y / 2 - textSize.Y / 2);
            Drawing.DrawRect(pos, maxSize, new Color(155, 155, 155, 155));
            Drawing.DrawText(
                text, "Arial",
                textPos, new Vector2(textSize.Y, 0),
                clr,
                FontFlags.AntiAlias | FontFlags.StrikeOut);

            Drawing.DrawRect(pos, maxSize, Color.Black, true);
            return pos + new Vector2(0, textSize.Y);
        }

        public static Vector2 DrawBar(Vector2 pos, string text, Vector2 maxSize, DotaTexture texture, Color clr)
        {
            var textSize = Drawing.MeasureText(text, "Arial",
                new Vector2((float) (maxSize.Y * .7), 0), FontFlags.AntiAlias | FontFlags.StrikeOut);
            Drawing.DrawRect(pos, maxSize, texture);
            var textPos = pos + new Vector2(maxSize.X / 2 - textSize.X / 2, maxSize.Y / 2 - textSize.Y / 2);
            Drawing.DrawRect(textPos, textSize, new Color(0, 0, 0, 75));
            Drawing.DrawText(
                text, "Arial",
                textPos, new Vector2(textSize.Y, 0),
                clr,
                FontFlags.AntiAlias | FontFlags.StrikeOut);

            Drawing.DrawRect(pos, maxSize, Color.Black, true);
            return pos + new Vector2(0, maxSize.Y);
        }

        public static Vector2 DrawManaBar(Vector2 pos, float value, Vector2 maxSize, Color clrOn, Color clrOff,
            string text, MenuItem<bool> manaBarsNumbers, float size)
        {
            var textSize = Drawing.MeasureText(text, "Arial",
                new Vector2(maxSize.Y * size / 10f, 0), FontFlags.AntiAlias | FontFlags.StrikeOut);
            var textPos = pos +
                          new Vector2(maxSize.X / 2 - textSize.X / 2, maxSize.Y / 2 - textSize.Y / 2);


            Drawing.DrawRect(pos, maxSize, clrOff);
            Drawing.DrawRect(pos, new Vector2(value, maxSize.Y), clrOn);
            if (manaBarsNumbers)
                Drawing.DrawText(
                    text, "Arial",
                    textPos, new Vector2(textSize.Y, 0),
                    Color.White,
                    FontFlags.AntiAlias | FontFlags.StrikeOut);

            Drawing.DrawRect(pos, maxSize, Color.Black, true);
            return pos + new Vector2(0, maxSize.Y);
        }

        public static Vector2 DrawHealthBar(Vector2 pos, Vector2 maxSize, string text, float size)
        {
            var textSize = Drawing.MeasureText(text, "Arial",
                new Vector2(maxSize.Y * size / 10f, 0), FontFlags.AntiAlias | FontFlags.StrikeOut);
            var textPos = pos +
                          new Vector2(maxSize.X / 2 - textSize.X / 2, maxSize.Y / 2 - textSize.Y / 2);

            Drawing.DrawText(
                text, "Arial",
                textPos, new Vector2(textSize.Y, 0),
                Color.White,
                FontFlags.AntiAlias | FontFlags.StrikeOut);

            Drawing.DrawRect(pos, maxSize, Color.Black, true);
            return pos + new Vector2(0, maxSize.Y);
        }
    }
}