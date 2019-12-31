using System;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using Ensage.SDK.Extensions;
using Ensage.SDK.Menu;
using Ensage.SDK.Renderer;
using SharpDX;
using Color = System.Drawing.Color;
using RectangleF = SharpDX.RectangleF;

namespace OverlayInformation.Features
{
    public class LastPositionTracker
    {
        public LastPositionTracker(Config config)
        {
            if (Game.GameMode == GameMode.Turbo) return;
            Config = config;
            var panel = Config.Factory.Menu("Last Position Tracker");
            Render = config.Main.Renderer;
            Enable = panel.Item("Enable", true);
            Prediction = panel.Item("Prediction", true);
            DrawOnMinimap = panel.Item("Draw on minimap", true);
            MinimapType = panel.Item("Minimap drawing type", new StringList("circle", "name", "icon"));
            MinimapSize = panel.Item("Minimap size", new Slider(15, 1, 30));
            DrawOnMap = panel.Item("Draw on map", true);
            MapSize = panel.Item("Map size", new Slider(50, 1));

            if (Enable)
            {
                Drawing.OnDraw += DrawingOnOnDraw;
                Render.Draw += RenderOnDraw;
            }

            Enable.Item.ValueChanged += (sender, args) =>
            {
                if (args.GetNewValue<bool>())
                {
                    Drawing.OnDraw += DrawingOnOnDraw;
                    Render.Draw += RenderOnDraw;
                }
                else
                {
                    Drawing.OnDraw -= DrawingOnOnDraw;
                    Render.Draw -= RenderOnDraw;
                }
            };
        }

        public Config Config { get; }

        public MenuItem<bool> DrawOnMap { get; set; }
        public MenuItem<Slider> MapSize { get; set; }
        public MenuItem<Slider> MinimapSize { get; set; }
        public MenuItem<StringList> MinimapType { get; set; }
        public MenuItem<bool> Enable { get; set; }
        public MenuItem<bool> Prediction { get; set; }
        public MenuItem<bool> DrawOnMinimap { get; set; }
        public IRenderManager Render { get; set; }

        private void RenderOnDraw(IRenderer renderer)
        {
            if (!DrawOnMinimap)
                return;
            foreach (var container in Config.Main.Updater.EnemyHeroes)
            {
                var hero = container.Hero;
                if (container.LastTimeUnderVision <= 0 && !hero.IsVisible)
                    continue;
                if (!hero.IsAlive)
                {
                    container.LastTimeUnderVision = 0;
                    continue;
                }

                if (container.IsVisible)
                {
                    /*if (!DrawOnMap)
                        container.LastTimeUnderVision = Game.RawGameTime;*/
                }
                else
                {
                    var delay = Game.RawGameTime - container.LastTimeUnderVision;
                    var pos = Prediction ? hero.Predict(delay * 1000).WorldToMinimap() : hero.Position.WorldToMinimap();
                    var vecClr = Config.TpCatcher.ColorList[container.Id] * 255;
                    //var vecClr = Config.TpCatcher.ColorList[hero.Player.Id] * 255;
                    var index = MinimapType.Value.SelectedIndex;
                    if (index == 0)
                    {
                        var color = Color.FromArgb(255, (int) vecClr.X, (int) vecClr.Y, (int) vecClr.Z);
                        renderer.DrawCircle(pos, MinimapSize, color);
                    }
                    else if (index == 1)
                    {
                        renderer.DrawText(pos - MinimapSize / 2f, hero.GetDisplayName(), Color.White, MinimapSize);
                    }
                    else
                    {
                        renderer.DrawTexture(container.HeroId.ToString(),
                            new RectangleF(pos.X - MinimapSize / 2f, pos.Y - MinimapSize / 2f, MinimapSize,
                                MinimapSize));
                    }
                }
            }
        }

        private void DrawingOnOnDraw(EventArgs args)
        {
            if (!DrawOnMap)
                return;
            foreach (var container in Config.Main.Updater.EnemyHeroes)
            {
                var hero = container.Hero;
                if (hero == null || !hero.IsValid)
                    continue;
                if (container.LastTimeUnderVision <= 0 && !hero.IsVisible)
                    continue;
                if (!hero.IsAlive)
                {
                    container.LastTimeUnderVision = 0;
                    continue;
                }

                if (hero.IsVisible)
                {
                    //container.LastTimeUnderVision = Game.RawGameTime;
                }
                else
                {
                    try
                    {
                        var delay = Game.RawGameTime - container.LastTimeUnderVision;
                        var pos = Drawing.WorldToScreen(Prediction ? hero.Predict(delay * 1000) : hero.Position);
                        if (pos.IsZero)
                            continue;
                        var size = new Vector2(MapSize);
                        Drawing.DrawRect(pos - size, size, Textures.GetHeroRoundTexture(hero.Name));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
        }

        public void OnDeactivate()
        {
            Drawing.OnDraw -= DrawingOnOnDraw;
            Render.Draw -= RenderOnDraw;
        }
    }
}