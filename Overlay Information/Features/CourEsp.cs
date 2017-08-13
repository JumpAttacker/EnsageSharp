using System;
using System.Collections.Generic;
using Ensage;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using Ensage.SDK.Menu;
using SharpDX;

namespace OverlayInformation.Features
{
    public class CourEsp
    {
        public Config Config { get; }

        public CourEsp(Config config)
        {
            Config = config;
            var panel = Config.Factory.Menu("Cour overlay");
            EnableForAlly = panel.Item("Enable for ally", true);
            EnableForEnemy = panel.Item("Enable for enemy", true);
            ItemSize = panel.Item("Size", new Slider(7, 1, 20));
            if (EnableForAlly)
                Drawing.OnDraw += DrawOnAlly;
            if (EnableForEnemy)
                Drawing.OnDraw += DrawOnEnemy;

            EnableForAlly.Item.ValueChanged += (sender, args) =>
            {
                if (args.GetNewValue<bool>())
                {
                    Drawing.OnDraw += DrawOnAlly;
                }
                else
                {
                    Drawing.OnDraw -= DrawOnAlly;
                }
            };

            EnableForEnemy.Item.ValueChanged += (sender, args) =>
            {
                if (args.GetNewValue<bool>())
                {
                    Drawing.OnDraw += DrawOnEnemy;
                }
                else
                {
                    Drawing.OnDraw -= DrawOnEnemy;
                }
            };
        }

        public MenuItem<Slider> ItemSize { get; set; }

        private void DrawOnAlly(EventArgs args)
        {
            DrawAction(Config.Main.Updater.AllyCouriers);
        }

        private void DrawOnEnemy(EventArgs args)
        {
            DrawAction(Config.Main.Updater.EnemyCouriers);
        }

        private void DrawAction(List<CourContainer> cours)
        {
            foreach (var cour in cours)
            {
                var hero = cour.Cour;
                if (!hero.IsAlive)
                    continue;
                if (!hero.IsVisible)
                    continue;
                var pos = HudInfo.GetHPbarPosition(hero);
                if (pos.IsZero)
                    continue;
                var size = new Vector2(HudInfo.GetHPBarSizeX(hero), HudInfo.GetHpBarSizeY(hero));
                var tempSize = size.X * ItemSize / 30f;
                var abilitySize = new Vector2(tempSize);
                var abilities = cour.Items;
                var abilityCount = abilities.Count;
                pos += new Vector2(0, size.Y - 2);
                pos += new Vector2((size.X - tempSize * abilityCount) / 2f, -abilitySize.Y);

                foreach (var ability in abilities)
                {
                    if (ability == null || !ability.IsValid)
                        continue;
                    pos = DrawItemState(pos, ability, abilitySize, Color.White);
                }
            }
        }
        public Vector2 DrawItemState(Vector2 pos, Item ability, Vector2 maxSize, Color color)
        {
            var itemSize = new Vector2(maxSize.X * 1.5f, maxSize.Y);
            Drawing.DrawRect(pos, itemSize, Textures.GetItemTexture(ability.StoredName()));
            Drawing.DrawRect(pos, new Vector2(maxSize.X, maxSize.Y), color, true);
            return pos + new Vector2(maxSize.X - 1, 0);
        }
        public MenuItem<bool> EnableForAlly { get; set; }
        public MenuItem<bool> EnableForEnemy { get; set; }
    }
}