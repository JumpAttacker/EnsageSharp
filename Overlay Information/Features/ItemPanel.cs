using System;
using Ensage;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using Ensage.Items;
using Ensage.SDK.Menu;
using OverlayInformation.Features.beh;
using SharpDX;

namespace OverlayInformation.Features
{
    public class ItemPanel : Movable
    {
        public Config Config { get; }

        public ItemPanel(Config config)
        {
            Config = config;
            var panel = Config.Factory.Menu("Item panel");

            Enable = panel.Item("Enable", true);
            Cooldown = panel.Item("Draw cooldown", true);
            SizeX = panel.Item("Size X", new Slider(28, 1));
            SizeY = panel.Item("Size Y", new Slider(14, 1));
            CanMove = panel.Item("Movable", false);
            PosX = panel.Item("Position -> X", new Slider(20, 1, 2500));
            PosY = panel.Item("Position -> Y", new Slider(500, 1, 2500));

            if (Enable)
                Drawing.OnDraw += DrawingOnOnDraw;

            Enable.Item.ValueChanged += (sender, args) =>
            {
                if (args.GetNewValue<bool>())
                    Drawing.OnDraw += DrawingOnOnDraw;
                else
                    Drawing.OnDraw -= DrawingOnOnDraw;
            };

            LoadMovable();
        }

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
                    PosX.Item.SetValue(new Slider((int)pos.X, 1, 2500));
                    PosY.Item.SetValue(new Slider((int)pos.Y, 1, 2500));
                }
            }
            
            var stageSize = new Vector2(size.X / 7f, size.Y / 5f);
            var itemSize = new Vector2(stageSize.X / .7f, stageSize.Y);
            var emptyTexture = Textures.GetTexture("materials/ensage_ui/items/emptyitembg.vmat");
            foreach (var heroC in Config.Main.Updater.EnemyHeroes)
            {
                if (heroC.DontDraw)
                    continue;
                var hero = heroC.Hero;
                var startPos = new Vector2(pos.X, pos.Y);
                Drawing.DrawRect(pos, stageSize, Textures.GetHeroTexture(hero.StoredName()));
                pos += new Vector2(stageSize.X, 0);
                for (var i = 0; i < 6; i++)
                {
                    Item item = null;
                    try
                    {
                        item = heroC.Items[i];
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                    if (item == null || !item.IsValid)
                    {
                        Drawing.DrawRect(pos, itemSize, emptyTexture);
                    }
                    else
                    {
                        var bottletype = item as Bottle;
                        if (bottletype != null && bottletype.StoredRune != RuneType.None)
                        {
                            var itemTexture =
                                Textures.GetTexture(
                                    $"materials/ensage_ui/items/{item.Name.Replace("item_", "") + "_" + bottletype.StoredRune}.vmat");
                            Drawing.DrawRect(pos, itemSize, itemTexture);
                        }
                        else
                        {
                            Drawing.DrawRect(pos, itemSize, Textures.GetItemTexture(item.Name));
                        }

                        if (item.AbilityState == AbilityState.OnCooldown)
                        {
                            var cooldown = item.Cooldown + 1;
                            var cdText = ((int)cooldown).ToString();
                            DrawItemCooldown(cdText, pos, stageSize);
                        }
                    }
                    
                    Drawing.DrawRect(pos, stageSize, Color.White, true);
                    pos += new Vector2(stageSize.X, 0);
                }
                pos = new Vector2(startPos.X, startPos.Y + itemSize.Y);
            }
            Drawing.DrawRect(startPosition, size, Color.White, true);
        }

        private void DrawItemCooldown(string text, Vector2 pos, Vector2 maxSize)
        {
            var textSize = Drawing.MeasureText(text, "Arial",
                new Vector2(maxSize.Y * 0.9f, 0), FontFlags.AntiAlias | FontFlags.StrikeOut);

            var textPos = pos + new Vector2(maxSize.X / 2 - textSize.X / 2, maxSize.Y / 2 - textSize.Y / 2);
            //var textPos = pos + new Vector2(0, 0);
            Drawing.DrawRect(pos, maxSize, new Color(0, 0, 0, 100));
            Drawing.DrawText(
                text, "Arial",
                textPos, new Vector2(textSize.Y, 0),
                Color.White,
                FontFlags.AntiAlias | FontFlags.StrikeOut);
        }

        public void OnDeactivate()
        {
            Drawing.OnDraw -= DrawingOnOnDraw;
        }
    }
}