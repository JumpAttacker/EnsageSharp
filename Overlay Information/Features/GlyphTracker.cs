using System;
using System.Reflection;
using Ensage;
using Ensage.Common.Menu;
using Ensage.SDK.Menu;
using Ensage.SDK.Renderer;
using log4net;
using PlaySharp.Toolkit.Logging;
using SharpDX;

namespace OverlayInformation.Features
{
    public class GlyphTracker
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public GlyphTracker(Config config)
        {
            Config = config;
            Render = config.Main.Context.Value.Renderer;
            var panel = Config.Factory.Menu("Glyph tracker");
            Enable = panel.Item("Enable", true);
            PosX = panel.Item("Position -> X", new Slider(426, 1, 2500));
            PosY = panel.Item("Position -> Y", new Slider(18, 1, 2500));
            TextSize = panel.Item("Text Size", new Slider(17, 5, 30));
            if (Enable)
            {
                Load();
            }

            Enable.PropertyChanged += (sender, args) =>
            {
                if (Enable)
                {
                    Load();
                }
                else
                {
                    UnLoad();
                }
            };
        }

        public float MaxCooldown => 300f;
        public float Time { get; set; }

        private void TowerOnOnFloatPropertyChange(Entity entity, Int32PropertyChangeEventArgs args)
        {
            if (args.NewValue == 0 && args.PropertyName.Equals("m_iHealth") && entity is Tower &&
                entity.Team != Config.Main.Context.Value.Owner.Team && entity.Name.Contains("tower1"))
            {
                Time = 0;
                Log.Debug($"{entity.NetworkName} -> {args.PropertyName} from {args.OldValue} to {args.NewValue}");
            }
        }

        private void RenderOnDraw(object o, EventArgs eventArgs)
        {
            var time = Time - Game.RawGameTime;
            if (time > 0)
            {
                Render.DrawText(new Vector2(PosX, PosY), $"Glyph: {(int)time}", System.Drawing.Color.White, TextSize);
            }
            else
            {
                Render.DrawText(new Vector2(PosX, PosY), $"Glyph: ready", System.Drawing.Color.White, TextSize);
            }
        }

        private void TowerOnOnModifierAdded(Unit unit, ModifierChangedEventArgs modifierChangedEventArgs)
        {
            if (unit.Team != Config.Main.Context.Value.Owner.Team &&
                modifierChangedEventArgs.Modifier.Name == "modifier_fountain_glyph")
            {
                Time = Game.RawGameTime + MaxCooldown;
            }
        }

        public void Load()
        {
            Render.Draw += RenderOnDraw;
            Entity.OnInt32PropertyChange += TowerOnOnFloatPropertyChange;
            Unit.OnModifierAdded += TowerOnOnModifierAdded;
        }

        public void UnLoad()
        {
            Render.Draw -= RenderOnDraw;
            Entity.OnInt32PropertyChange -= TowerOnOnFloatPropertyChange;
            Unit.OnModifierAdded -= TowerOnOnModifierAdded;
        }

        public MenuItem<Slider> TextSize { get; set; }

        public MenuItem<Slider> PosY { get; set; }

        public MenuItem<Slider> PosX { get; set; }

        public IRendererManager Render { get; set; }

        public MenuItem<bool> Enable { get; set; }

        public Config Config { get; }


    }
}