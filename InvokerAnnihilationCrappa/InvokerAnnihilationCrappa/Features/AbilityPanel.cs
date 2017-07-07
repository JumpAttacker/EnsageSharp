using System;
using System.Globalization;
using System.Linq;
using Ensage;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using Ensage.SDK.Menu;
using InvokerAnnihilationCrappa.Features.behavior;
using SharpDX;

namespace InvokerAnnihilationCrappa.Features
{
    public class AbilityPanel : Movable
    {
        private readonly Config _main;

        public AbilityPanel(Config main)
        {
            _main = main;
            var panel = main.Factory.Menu("Ability Panel");
            Enable = panel.Item("Enable", true);
            Movable = panel.Item("Movable", false);
            Size = panel.Item("Size", new Slider(5, 0, 100));
            PosX = panel.Item("Position X", new Slider(500, 0, 2000));
            PosY = panel.Item("Position Y", new Slider(500, 0, 2000));
            
            if (Enable)
            {
                Drawing.OnDraw += DrawingOnOnDraw;
            }
            LoadMovable();
            Enable.Item.ValueChanged += (sender, args) =>
            {
                if (args.GetNewValue<bool>())
                    Drawing.OnDraw += DrawingOnOnDraw;
                else
                    Drawing.OnDraw -= DrawingOnOnDraw;
            };
        }

        public MenuItem<Slider> Size { get; set; }

        private void DrawingOnOnDraw(EventArgs args)
        {
            if (Game.GameState == GameState.PostGame)
                return;
            Vector2 startPos = new Vector2(PosX.Value.Value, PosY.Value.Value);
            var size = new Vector2(Size * 10 * _main.Invoker.AbilityInfos.Count, Size * 10);
            if (Movable)
            {
                var tempSize = size;//new Vector2(200, 200);
                if (CanMoveWindow(ref startPos, tempSize,true))
                {
                    PosX.Item.SetValue(new Slider((int) startPos.X, 0, 2000));
                    PosY.Item.SetValue(new Slider((int) startPos.Y, 0, 2000));
                }
            }
            var pos = startPos;
            var iconSize = new Vector2(Size * 10);
            foreach (var info in _main.Invoker.AbilityInfos.OrderBy(x=>x.Ability.Cooldown))
            {
                var ability = info.Ability;
                Drawing.DrawRect(pos, iconSize, Textures.GetSpellTexture(ability.StoredName()));
                var miniIconSize = iconSize / 3;
                var miniIconPos = pos + new Vector2(0, iconSize.Y);
                Drawing.DrawRect(miniIconPos, miniIconSize, Textures.GetSpellTexture(info.One.StoredName()));
                Drawing.DrawRect(miniIconPos + new Vector2(miniIconSize.X, 0), miniIconSize, Textures.GetSpellTexture(info.Two.StoredName()));
                Drawing.DrawRect(miniIconPos + new Vector2(miniIconSize.X*2, 0), miniIconSize, Textures.GetSpellTexture(info.Three.StoredName()));
                var cd = ability.Cooldown;
                if (cd > 0)
                {
                    var text = ((int) (cd + 1)).ToString(CultureInfo.InvariantCulture);
                    Drawing.DrawText(
                        text,
                        pos, size / 10,
                        Color.White,
                        FontFlags.AntiAlias | FontFlags.StrikeOut);
                }
                pos += new Vector2(iconSize.X, 0);
                
            }

            //Drawing.DrawRect(startPos, size, new Color(155, 155, 155, 50));
            Drawing.DrawRect(startPos, size, new Color(0, 0, 0, 255),true);
        }

        public void OnDeactivate()
        {
            UnloadMovable();
            if (Enable)
                Drawing.OnDraw -= DrawingOnOnDraw;
        }

        public MenuItem<Slider> PosX { get; set; }
        public MenuItem<Slider> PosY { get; set; }

        public MenuItem<bool> Movable { get; set; }

        public MenuItem<bool> Enable { get; set; }
    }
}