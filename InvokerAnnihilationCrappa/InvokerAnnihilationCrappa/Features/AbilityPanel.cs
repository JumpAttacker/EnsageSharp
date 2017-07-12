using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using Ensage;
using Ensage.Common;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using Ensage.SDK.Menu;
using InvokerAnnihilationCrappa.Features.behavior;
using log4net;
using PlaySharp.Toolkit.Logging;
using SharpDX;

namespace InvokerAnnihilationCrappa.Features
{
    public class AbilityPanel : Movable
    {
        private readonly Config _main;
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public AbilityPanel(Config main)
        {
            _main = main;
            var panel = main.Factory.Menu("Ability Panel");
            Enable = panel.Item("Enable", true);
            Movable = panel.Item("Movable", false);
            OderBy = panel.Item("Sort by cooldown", false);
            Size = panel.Item("Size", new Slider(5, 0, 100));
            PosX = panel.Item("Position X", new Slider(500, 0, 2000));
            PosY = panel.Item("Position Y", new Slider(500, 0, 2000));
            ConfigSize = panel.Item("Text Size", new Slider(100, 1, 100));
            ColorR = panel.Item("text -> Red", new Slider(0, 0, 255));
            ColorG = panel.Item("text -> Gree", new Slider(0, 0, 255));
            ColorB = panel.Item("text -> Blue", new Slider(0, 0, 255));
            QCast = panel.Menu("Quick casts");
            foreach (var ability in _main.Invoker.AbilityInfos.Where(x => !(x.Ability is Item)))
            {
                CreateQuickCastForAbility(ability);
            }
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

        public MenuItem<Slider> ColorR { get; set; }
        public MenuItem<Slider> ColorG { get; set; }
        public MenuItem<Slider> ColorB { get; set; }

        public MenuItem<Slider> ConfigSize { get; set; }

        public MenuFactory QCast { get; set; }

        private void CreateQuickCastForAbility(AbilityInfo ability)
        {
            var menu = QCast.MenuWithTexture("", ability.Ability.Name, ability.Ability.Name);
            var enable = menu.Item("Enable qCast", true);
            var key = menu.Item("Hotkey", new KeyBind('0'));
            ability.UpdateKey(key.Value.Key);
            var key2 = KeyInterop.KeyFromVirtualKey((int)key.Value.Key);
            Log.Info($"{ability.Ability.Name} -> Key: {key.Value.Key} {key2}");

            key.Item.ValueChanged += (sender, args) =>
            {
                if (!enable)
                    return;
                var o = args.GetOldValue<KeyBind>().Active;
                var n = args.GetNewValue<KeyBind>().Active;
                if (o!=n)
                {
                    if (args.GetNewValue<KeyBind>().Active)
                        _main.Invoker.Invoke(ability);
                    else
                        ability.UpdateKey(key.Value.Key);
                }
            };
        }

        public MenuItem<bool> OderBy { get; set; }

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
            var list = OderBy
                ? _main.Invoker.AbilityInfos.OrderBy(x => x.Ability.Cooldown)
                : (IEnumerable<AbilityInfo>)_main.Invoker.AbilityInfos;
            foreach (var info in list)
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
                        pos, size * ConfigSize / 100 / 10,
                        new Color(ColorR, ColorG, ColorB),
                        FontFlags.AntiAlias | FontFlags.StrikeOut);
                }
                else
                {
                    var key = KeyInterop.KeyFromVirtualKey((int)info.Key);
                    if (key != Key.None && key != Key.D0)
                    {
                        var text = key.ToString();
                        Drawing.DrawText(
                            text,
                            pos, size * ConfigSize / 100 / 10,
                            new Color(ColorR, ColorG, ColorB),
                            FontFlags.AntiAlias | FontFlags.StrikeOut);
                    }
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