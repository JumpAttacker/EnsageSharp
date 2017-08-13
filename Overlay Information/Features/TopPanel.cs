using System;
using System.Collections.Generic;
using System.Globalization;
using Ensage;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using Ensage.SDK.Menu;
using SharpDX;

namespace OverlayInformation.Features
{
    public class TopPanel
    {
        public Config Config { get; }
        private readonly Dictionary<Hero, Vector2> _topPanelPosition;
        private Vector2 _size;
        public TopPanel(Config config)
        {
            Config = config;
            var panel = Config.Factory.Menu("Top Panel");
            HealthAndManaBars = panel.Menu("Health and Mana bars");
            HealthBar = HealthAndManaBars.Item("Health bar", true);
            ManaBar = HealthAndManaBars.Item("Mana bar", true);
            SizeY = HealthAndManaBars.Item("Size", new Slider(7, 1, 20));


            UltimateBar = panel.Item("Ultimate bar", true);
            UltimateIcon = panel.Item("Ultimate icon", true);
            VisibleBar = panel.Item("Visible status", true);
            AllyVisibleBarType = panel.Item("Ally Visible status type", new StringList("text", "rectangle"));
            RectangleA = panel.Item("rectangle's color -> A", new Slider(100, 0, 255));
            RectangleR = panel.Item("rectangle's color -> R", new Slider(255, 0, 255));
            RectangleG = panel.Item("rectangle's color -> G", new Slider(0, 0, 255));
            RectangleB = panel.Item("rectangle's color -> B", new Slider(255, 0, 255));
            


            _topPanelPosition = new Dictionary<Hero, Vector2>();
            Drawing.OnDraw += DrawingOnOnDraw;
            var size = HudInfo.GetTopPanelSize(Config.Main.Context.Value.Owner as Hero);
            _size = new Vector2(size[0], size[1]);

            config.Main.Renderer.Draw += RendererOnDraw;
        }

        public MenuItem<Slider> RectangleA { get; set; }
        public MenuItem<Slider> RectangleR { get; set; }
        public MenuItem<Slider> RectangleG { get; set; }
        public MenuItem<Slider> RectangleB { get; set; }


        public MenuItem<StringList> AllyVisibleBarType { get; set; }

        public MenuItem<bool> UltimateIcon { get; set; }

        public MenuFactory HealthAndManaBars { get; set; }

        public MenuItem<bool> VisibleBar { get; set; }

        public MenuItem<bool> UltimateBar { get; set; }

        public MenuItem<bool> ManaBar { get; set; }

        public MenuItem<bool> HealthBar { get; set; }

        public MenuItem<Slider> SizeY { get; set; }

        private void RendererOnDraw(object sender, EventArgs eventArgs)
        {
            var heroes = Config.Main.Updater.AllyHeroes;
            var size = HudInfo.GetTopPanelSize();
            var clr =
                Config.Main.BrushCache.GetOrCreate(System.Drawing.Color.FromArgb(RectangleA, RectangleR, RectangleG,
                    RectangleB));
            foreach (var heroCont in heroes)
            {
                var hero = heroCont.Hero;
                var pos = GetTopPanelPosition(hero);
                if (pos.IsZero)
                    continue;
                if (VisibleBar)
                {
                    var isVisible = hero.IsVisibleToEnemies;
                    if (isVisible)
                    {
                        if (AllyVisibleBarType.Value.SelectedIndex == 1)
                        {
                            //pos = Game.MouseScreenPosition;
                                
                            var rectangle = new RectangleF(pos.X, pos.Y - size.Y, size.X, size.Y);
                            Config.Main.D11Context.RenderTarget.FillRectangle(rectangle, clr);
                            /*Config.Main.Renderer.DrawRectangle(rectangle,
                                System.Drawing.Color.FromArgb(255, 255, 0, 255),10);*/
                        }
                    }
                }
            }
        }

        private void DrawingOnOnDraw(EventArgs args)
        {
            var heroes = Config.Main.Updater.Heroes;
            var size = new Vector2(_size.X, SizeY);
            foreach (var heroCont in heroes)
            {
                var hero = heroCont.Hero;
                var pos = GetTopPanelPosition(hero);
                if (pos.IsZero)
                    continue;
                if (HealthBar)
                    pos = DrawingHelper.DrawBar(pos, heroCont.Health * size.X / heroCont.MaxHealth, size,
                        Color.GreenYellow,
                        Color.Red);
                if (ManaBar)
                    pos = DrawingHelper.DrawBar(pos, heroCont.Mana * size.X / heroCont.MaxMana, size, Color.Blue,
                        new Color(155, 155, 155, 155));
                if (VisibleBar)
                    if (heroCont.IsAlly)
                    {
                        var isVisible = hero.IsVisibleToEnemies;
                        if (isVisible)
                        {
                            if (AllyVisibleBarType.Value.SelectedIndex == 0)
                            {
                                pos = DrawingHelper.DrawBar(pos, "visible", new Vector2(size.X, size.Y * 2),
                                    Color.White);
                            }
                        }
                    }
                    else
                    {
                        var isVisible = hero.IsVisible;
                        if (isVisible)
                        {
                            heroCont.LastTimeUnderVision = Game.RawGameTime;
                        }
                        else
                        {
                            var time = (int) (Game.RawGameTime - heroCont.LastTimeUnderVision) + 1;
                            pos = DrawingHelper.DrawBar(pos, $"in fog {time}", new Vector2(size.X, size.Y * 2),
                                Color.White);
                        }
                    }
                if (UltimateBar || UltimateIcon)
                {
                    var ultimate = heroCont.Ultimate;
                    if (UltimateBar)
                    {
                        switch (heroCont.AbilityState)
                        {
                            case AbilityState.OnCooldown:
                                var cdCalc = ultimate.Cooldown - (hero.IsVisible ? 0 : heroCont.TimeInFog);
                                if (cdCalc < 0)
                                    break;
                                var cd = Math.Min(99, (int) (cdCalc + 1));
                                pos = DrawingHelper.DrawBar(pos, cd.ToString(CultureInfo.InvariantCulture),
                                    new Vector2(size.X, size.X), ultimate.Texture, Color.White);
                                break;
                            case AbilityState.NotEnoughMana:
                                var mana = Math.Min(99, (int) (ultimate.Ability.ManaCost - heroCont.Mana));
                                pos = DrawingHelper.DrawBar(pos, mana.ToString(CultureInfo.InvariantCulture),
                                    new Vector2(size.X, size.X), ultimate.Texture, Color.White,
                                    new Color(100, 100, 255, 100));
                                break;
                        }
                    }

                    if (UltimateIcon && !heroCont.IsAlly)
                    {
                        string path;
                        switch (heroCont.AbilityState)
                        {
                            case AbilityState.OnCooldown:
                                path = "materials/ensage_ui/other/ulti_cooldown.vmat";
                                DrawUltimateIcon(hero, path, size);
                                break;
                            case AbilityState.NotEnoughMana:
                                path = "materials/ensage_ui/other/ulti_nomana.vmat";
                                DrawUltimateIcon(hero, path, size);
                                break;
                            case AbilityState.Ready:
                                path = "materials/ensage_ui/other/ulti_ready.vmat";
                                DrawUltimateIcon(hero, path, size);
                                break;
                        }
                    }
                }


                /*Drawing.DrawRect(pos, size, Color.White);
                pos += new Vector2(0, size.Y);
                Drawing.DrawRect(pos, size, Color.Blue);*/
            }
        }

        private void DrawUltimateIcon(Hero hero, string path, Vector2 size)
        {
            var newPos = GetTopPanelPosition(hero);
            var iconSize = new Vector2(15);
            Drawing.DrawRect(newPos + new Vector2(size.X / 2f - iconSize.X / 2f, -3), new Vector2(14),
                Textures.GetTexture(path));
        }

        private Vector2 GetTopPanelPosition(Hero hero)
        {
            Vector2 pos;
            if (!_topPanelPosition.TryGetValue(hero, out pos))
            {
                pos = HudInfo.GetTopPanelPosition(hero);
                _topPanelPosition[hero] = pos;
            }
            else if (pos.IsZero)
                pos = HudInfo.GetTopPanelPosition(hero);
            return pos + new Vector2(0, _size.Y);
        }

        public void OnDeactivate()
        {
            _topPanelPosition.Clear();
            Drawing.OnDraw -= DrawingOnOnDraw;
        }
    }
}