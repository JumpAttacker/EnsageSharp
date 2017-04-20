using System;
using System.Linq;
using Ensage;
using Ensage.Common;
using SharpDX;
using Techies_Annihilation.BombFolder;

namespace Techies_Annihilation.Features
{
    internal class BombStatus
    {
        private const float BombMaxTimer = 1.6f;
        public static void OnDraw(EventArgs args)
        {
            if (!Enable) return;
            foreach (
                var bomb in
                    Core.Bombs.Where(x => x.Active && !x.IsRemoteMine && x.Status == Enums.BombStatus.WillDetonate))
            {
                var pos = HUDInfo.GetHPbarPosition(bomb.Bomb);//Drawing.WorldToScreen(bomb.Bomb.Position);
                var bombTimer = Game.RawGameTime - bomb.StatusStartTIme;
                var hpBarSize = HUDInfo.GetHPBarSizeX();
                var size = new Vector2(hpBarSize * 1f, BarSize);
                var cdDelta = bombTimer * size.X / BombMaxTimer;
                pos += new Vector2(0, hpBarSize * 0.5f);
                Drawing.DrawRect(pos, new Vector2(size.X, size.Y), Color.Black);
                Drawing.DrawRect(pos, new Vector2(size.X - cdDelta, size.Y),Color.YellowGreen);
                Drawing.DrawRect(pos, new Vector2(size.X, size.Y), Color.Black, true);

                if (DrawDigs)
                {
                    var text = $"{(int) (100 - bombTimer/BombMaxTimer*100)}%";
                    var textSize = Drawing.MeasureText(text, "Arial",
                        new Vector2((float) (size.Y*DigSize), size.Y/2), FontFlags.AntiAlias);
                    var textPos = pos + new Vector2(size.X/2 - textSize.X/2, size.Y - textSize.Y);
                    Drawing.DrawText(
                        text,
                        textPos,
                        new Vector2(textSize.Y, 0),
                        Color.White,
                        FontFlags.AntiAlias | FontFlags.StrikeOut);
                }
            }
        }

        public static double DigSize = MenuManager.GetLandMineIndicatorDigSize; //1

        public static float BarSize = MenuManager.GetLandMineBarSize; //10;
        public static bool DrawDigs = MenuManager.LandMinesDrawDigs;
        public static bool Enable = MenuManager.LandMineIndicatorEnable;
    }
}