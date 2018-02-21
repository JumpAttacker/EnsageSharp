using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using Ensage.SDK.Menu;
using OverlayInformation.Features.beh;
using SharpDX;

namespace OverlayInformation.Features
{
    public class NetworthPanel : Movable
    {
        public Config Config { get; }

        private enum Orders
        {
            Standard,Descending,Ascending, Team1, Team2
        }
        public NetworthPanel(Config config)
        {
            Config = config;
            var panel = Config.Factory.Menu("Networth panel");


            var globalPanel = panel.Menu("Global Networth panel");
            EnableGlobal = globalPanel.Item("Enable", true);
            DrawTeamValues = globalPanel.Item("Draw team values", true);
            DrawPercent = globalPanel.Item("Draw percents", true);
            GlobalSizeY = globalPanel.Item("Size", new Slider(20, 1, 50));
            GetCoef = globalPanel.Item("Text size", new Slider(10, 1, 15));


            Enable = panel.Item("Enable", true);
            OrderBy = panel.Item("Sort by",
                new StringList("standart", "by descending", "by ascending", "by team 1", "by team 2"));
            SizeX = panel.Item("Size X", new Slider(28, 1));
            SizeY = panel.Item("Size Y", new Slider(14, 1));
            
            CanMove = panel.Item("Movable", false);
            PosX = panel.Item("Position -> X", new Slider(500, 1, 2500));
            PosY = panel.Item("Position -> Y", new Slider(500, 1, 2500));
            if (Enable)
                Drawing.OnDraw += DrawingOnOnDraw;
            if (EnableGlobal)
                Drawing.OnDraw += DrawGlobalPanel;

            Enable.Item.ValueChanged += (sender, args) =>
            {
                if (args.GetNewValue<bool>())
                    Drawing.OnDraw += DrawingOnOnDraw;
                else
                    Drawing.OnDraw -= DrawingOnOnDraw;
            };

            EnableGlobal.Item.ValueChanged += (sender, args) =>
            {
                if (args.GetNewValue<bool>())
                    Drawing.OnDraw += DrawGlobalPanel;
                else
                    Drawing.OnDraw -= DrawGlobalPanel;
            };

            LoadMovable(config.Main.Context.Value.Input);
        }

        public MenuItem<bool> DrawPercent { get; set; }

        public MenuItem<bool> DrawTeamValues { get; set; }

        public MenuItem<Slider> GetCoef { get; set; }

        public MenuItem<Slider> GlobalSizeY { get; set; }

        private void DrawGlobalPanel(EventArgs args)
        {
            var startPos = HudInfo.GetFakeTopPanelPosition(5, Team.Radiant) +
                           new Vector2(Config.TopPanel.ExtraPosX,
                               (float) HudInfo.GetTopPanelSizeY() + Config.TopPanel.ExtraPosY);
            var endPos = HudInfo.GetFakeTopPanelPosition(5, Team.Dire) +
                         new Vector2(Config.TopPanel.ExtraPosX, Config.TopPanel.ExtraPosY);
            var size = new Vector2(endPos.X - startPos.X, GlobalSizeY);
            uint direNetwoth = 0;
            uint radiantNetworh = 0;
            foreach (var heroC in Config.Main.Updater.Heroes)
            {
                var hero = heroC.Hero;
                if (hero.Team == Team.Radiant)
                {
                    radiantNetworh += heroC.Networth;
                }
                else
                {
                    direNetwoth += heroC.Networth;
                }
            }
            Color rightClr;
            Color leftClr;
            
            var percent = 100 * radiantNetworh / Math.Max(1, radiantNetworh + direNetwoth);
            var currentSize = size.X / 100 * percent;
            var lineSize = new Vector2(currentSize, size.Y);
            var endOfGreen = startPos + new Vector2(lineSize.X, 0);
            if (Config.Main.Context.Value.Owner.Team != Team.Radiant)
            {
                leftClr = new Color(155, 0, 0, 155);
                rightClr = new Color(0, 155, 0, 155);
                percent = 100 - percent;
            }
            else
            {
                rightClr = new Color(155, 0, 0, 155);
                leftClr = new Color(0, 155, 0, 155);
            }

            Drawing.DrawRect(startPos, lineSize, leftClr);
            Drawing.DrawRect(endOfGreen, new Vector2(size.X - lineSize.X, lineSize.Y), rightClr);
            Drawing.DrawRect(startPos, size, new Color(0, 0, 0, 255), true);


            var text = $"{percent}%";
            var textSize = Drawing.MeasureText(text, "Arial",
                new Vector2((float)(size.Y * .95), size.Y / 2), FontFlags.AntiAlias);
            var textPos = endOfGreen - new Vector2(textSize.X / 2, /*lineSize.Y / 2 - textSize.Y / 2*/0);
            var coef = GetCoef/10f;
            if (DrawPercent)
                Drawing.DrawText(
                    text,
                    textPos,
                    new Vector2(textSize.Y, 0),
                    Color.White,
                    FontFlags.AntiAlias | FontFlags.StrikeOut);
            if (DrawTeamValues)
            {
                text = $"{radiantNetworh}";
                Drawing.DrawText(
                    text,
                    startPos + new Vector2(0, size.Y),
                    new Vector2(textSize.Y * coef, 0),
                    Color.White,
                    FontFlags.AntiAlias | FontFlags.StrikeOut);
                text = $"{direNetwoth}";
                Drawing.DrawText(
                    text,
                    startPos + new Vector2(size.X - textSize.X * coef, size.Y),
                    new Vector2(textSize.Y * coef, 0),
                    Color.White,
                    FontFlags.AntiAlias | FontFlags.StrikeOut);
            }
        }

        public MenuItem<bool> EnableGlobal { get; set; }

        public MenuItem<StringList> OrderBy { get; set; }

        public MenuItem<Slider> SizeX { get; set; }
        public MenuItem<Slider> SizeY { get; set; }
        public MenuItem<bool> CanMove { get; set; }
        public MenuItem<bool> Cooldown { get; set; }
        public MenuItem<Slider> PosX { get; set; }
        public MenuItem<Slider> PosY { get; set; }
        public MenuItem<bool> Enable { get; set; }

        private void DrawingOnOnDraw(EventArgs args)
        {
            var pos = new Vector2(PosX.Value.Value, PosY.Value.Value);
            var startPosition = pos;
            var size = new Vector2(SizeX * 10, SizeY * 10);
            if (CanMove)
            {
                if (CanMoveWindow(ref pos, size, true))
                {
                    PosX.Item.SetValue(new Slider((int) pos.X, 1, 2500));
                    PosY.Item.SetValue(new Slider((int) pos.Y, 1, 2500));
                }
            }

            var stageSize = new Vector2(size.X / 7f, size.Y / 10f);
            var itemSize = new Vector2(stageSize.X / .7f, stageSize.Y);
            var orderIndex = (Orders)OrderBy.Value.SelectedIndex;
            var heroes = GetHeroes(orderIndex);
            if (heroes==null || heroes.Count==0)
                return;
            var maxNetworth = heroes.Max(x => x.Networth);
            foreach (var heroC in heroes)
            {
                if (heroC.DontDraw)
                    continue;
                var hero = heroC.Hero;
                var startPos = pos;
                Drawing.DrawRect(pos, stageSize, Textures.GetHeroTexture(hero.StoredName()));
                pos += new Vector2(stageSize.X, 0);
                var newSize = new Vector2(size.X - stageSize.X, stageSize.Y);
                var lineSize = new Vector2(heroC.Networth * newSize.X / maxNetworth, newSize.Y);
                Drawing.DrawRect(pos, lineSize,
                    heroC.IsAlly ? new Color(0, 155, 0, 155) : new Color(155, 0, 0, 155));
                DrawText(heroC.Networth.ToString(), pos, lineSize);
                Drawing.DrawRect(pos, newSize, Color.White,true);
                pos = new Vector2(startPos.X, startPos.Y + itemSize.Y);
            }
            Drawing.DrawRect(startPosition, size, Color.White, true);
        }

        private List<HeroContainer> GetHeroes(Orders orderBy)
        {
            switch (orderBy)
            {
                case Orders.Standard:
                    return Config.Main.Updater.Heroes;
                case Orders.Descending:
                    return Config.Main.Updater.Heroes.OrderByDescending(x=>x.Networth).ToList();
                case Orders.Ascending:
                    return Config.Main.Updater.Heroes.OrderBy(x => x.Networth).ToList();
                case Orders.Team1:
                    return Config.Main.Updater.Heroes.OrderByDescending(x => x.IsAlly).ToList();
                case Orders.Team2:
                    return Config.Main.Updater.Heroes.OrderBy(x => x.IsAlly).ToList();
                default:
                    return Config.Main.Updater.Heroes;
            }
        }

        private void DrawText(string text, Vector2 pos, Vector2 maxSize)
        {
            var textSize = Drawing.MeasureText(text, "Arial",
                new Vector2(maxSize.Y * 0.9f, 0), FontFlags.AntiAlias | FontFlags.StrikeOut);

            var textPos = pos + new Vector2(2, maxSize.Y / 2 - textSize.Y / 2);
            //var textPos = pos + new Vector2(0, 0);
            Drawing.DrawText(
                text, "Arial",
                textPos, new Vector2(textSize.Y, 0),
                Color.White,
                FontFlags.AntiAlias | FontFlags.StrikeOut);
        }

        public void OnDeactivate()
        {
            Drawing.OnDraw -= DrawingOnOnDraw;
            Drawing.OnDraw -= DrawGlobalPanel;
        }
    }
}