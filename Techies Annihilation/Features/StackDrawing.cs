using System;
using System.Linq;
using Ensage;
using Ensage.Common;
using SharpDX;

namespace Techies_Annihilation.Features
{
    internal class StackDrawing
    {
        public static void OnDraw(EventArgs args)
        {
            if (!MenuManager.IsStackerEnabled) return;
            foreach (var bomb in Core.Bombs.Where(x=>x.IsRemoteMine && x.Active && x.Stacker.IsActive))
            {
                var topPos = HUDInfo.GetHPbarPosition(bomb.Bomb);
                var size = new Vector2((float)HUDInfo.GetHPBarSizeX(bomb.Bomb), (float)HUDInfo.GetHpBarSizeY(bomb.Bomb));
                var text = bomb.Stacker.Counter.ToString();
                var textSize = Drawing.MeasureText(text, "Arial",
                    new Vector2((float)(size.Y * 4.5f), (float)(size.Y * 4.5f)), FontFlags.AntiAlias);
                var textPos = topPos + new Vector2(size.X/2-textSize.X/2, size.Y*2);
                Drawing.DrawText(
                    text,
                    textPos + new Vector2(2, 2),
                    new Vector2(textSize.Y, 0),
                    Color.White,
                    FontFlags.AntiAlias | FontFlags.StrikeOut);
            }
        }
    }
}