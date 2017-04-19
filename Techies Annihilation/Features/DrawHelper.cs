using System;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Objects;
using SharpDX;
using Techies_Annihilation.BombFolder;
using Techies_Annihilation.Utils;

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
    internal class DrawHelper
    {
        public static void OnDraw(EventArgs args)
        {
            foreach (var heroCont in BombDamageManager.DamageContainers)
            {
                DrawOnTopPanel(heroCont);
                DrawOnHeroes(heroCont);
            }
        }

        private static void DrawOnHeroes(BombDamageManager.HeroDamageContainer heroCont)
        {
            var hero = heroCont.Hero;
            if (!hero.IsVisible || !hero.IsAlive)
                return;
            var topPos = HUDInfo.GetHPbarPosition(hero);
            var size = new Vector2((float)HUDInfo.GetHPBarSizeX(hero), (float)HUDInfo.GetHpBarSizeY(hero));
            var text = heroCont.HealthAfterSuicide.ToString("####");
            var textSize = Drawing.MeasureText(text, "Arial",
                new Vector2((float)(size.Y*1.5), (float)(size.Y * 1.5)), FontFlags.AntiAlias);
            var textPos = topPos - new Vector2(textSize.X+5, 0);
            Drawing.DrawText(
                text,
                textPos + new Vector2(2, 2),
                new Vector2(textSize.Y, 0),
                Color.White,
                FontFlags.AntiAlias | FontFlags.StrikeOut);
            
        }

        private static void DrawOnTopPanel(BombDamageManager.HeroDamageContainer heroCont)
        {
            var hero = heroCont.Hero;
            var topPos = HUDInfo.GetTopPanelPosition(hero) + new Vector2(0, (float) HUDInfo.GetTopPanelSizeX(hero)) +
                         MenuManager.GetExtraPosForTopPanel;
            var size = new Vector2((float)HUDInfo.GetTopPanelSizeX(hero), (float)HUDInfo.GetTopPanelSizeY(hero));
            var iconSize = new Vector2(size.X/2, size.Y)/2 + MenuManager.GetTopPanelExtraSize;
            Drawing.DrawRect(topPos, iconSize,
                Textures.GetSpellTexture(AbilityId.techies_land_mines.ToString()));
            var text = heroCont.GetLandDamage;
            var textSize = Drawing.MeasureText(text, "Arial",
                new Vector2((float)(iconSize.Y * .80), (float)(iconSize.Y * .95)), FontFlags.AntiAlias);
            var textPos = topPos + new Vector2(iconSize.X + 2, 0);
            Drawing.DrawText(
                text,
                textPos + new Vector2(2, 2),
                new Vector2(textSize.Y, 0),
                Color.White,
                FontFlags.AntiAlias | FontFlags.StrikeOut);
            Drawing.DrawRect(topPos + new Vector2(0, iconSize.Y), iconSize,
                Textures.GetSpellTexture(AbilityId.techies_suicide.ToString()));
            text = heroCont.GetSuicideStatus;
            textSize = Drawing.MeasureText(text, "Arial",
                new Vector2((float)(iconSize.Y * .80), (float)(iconSize.Y * .95)), FontFlags.AntiAlias);
            textPos = topPos + new Vector2(iconSize.X + 2, iconSize.Y);
            Drawing.DrawText(
                text,
                textPos + new Vector2(2, 2),
                new Vector2(textSize.Y, 0),
                Color.White,
                FontFlags.AntiAlias | FontFlags.StrikeOut);
            Drawing.DrawRect(topPos + new Vector2(0, iconSize.Y * 2), iconSize,
                Textures.GetSpellTexture(AbilityId.techies_remote_mines.ToString()));
            text = heroCont.GetRemoteDamage;
            textSize = Drawing.MeasureText(text, "Arial",
                new Vector2((float)(iconSize.Y * .80), (float)(iconSize.Y * .95)), FontFlags.AntiAlias);
            textPos = topPos + new Vector2(iconSize.X + 2, iconSize.Y * 2);
            Drawing.DrawText(
                text,
                textPos + new Vector2(2, 2),
                new Vector2(textSize.Y, 0),
                Color.White,
                FontFlags.AntiAlias | FontFlags.StrikeOut);
        }
    }
}