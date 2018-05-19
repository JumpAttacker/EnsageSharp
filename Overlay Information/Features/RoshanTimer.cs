using System;
using System.Linq;
using System.Reflection;
using Ensage;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using Ensage.SDK.Helpers;
using Ensage.SDK.Menu;
using log4net;
using OverlayInformation.Features.beh;
using PlaySharp.Toolkit.Logging;
using SharpDX;

namespace OverlayInformation.Features
{
    public class RoshanTimer : Movable, IDisposable
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public Config Config { get; }

        public Unit Roshan { get; set; }

        public RoshanTimer(Config config)
        {
            Config = config;
            var panel = Config.Factory.Menu("Roshan timer");
            Enable = panel.Item("Enable", true);
            CanMove = panel.Item("Movable", false);
            PosX = panel.Item("Position -> X", new Slider(20, 1, 2500));
            PosY = panel.Item("Position -> Y", new Slider(500, 1, 2500));
            TextSize = panel.Item("Text Size", new Slider(17, 5, 30));
            LoadMovable(config.Main.Context.Value.Input);
            Game.OnFireEvent += Game_OnGameEvent;
            AegisEvent = false;

            Roshan = ObjectManager.GetEntities<Unit>().FirstOrDefault(x => x.Name == "npc_dota_roshan" && x.IsAlive) ??
                     ObjectManager.GetDormantEntities<Unit>().FirstOrDefault(x => x.Name == "npc_dota_roshan" && x.IsAlive);

            if (Enable)
            {
                Drawing.OnDraw += DrawingOnOnDraw;
                UpdateManager.Subscribe(AegisSearcher, 500);
            }

            Enable.Item.ValueChanged += (sender, args) =>
            {
                if (args.GetNewValue<bool>())
                {
                    UpdateManager.Subscribe(AegisSearcher, 1000);
                    Drawing.OnDraw += DrawingOnOnDraw;
                }
                else
                {
                    UpdateManager.Unsubscribe(AegisSearcher);
                    Drawing.OnDraw -= DrawingOnOnDraw;
                }
            };
        }

        public MenuItem<Slider> TextSize { get; set; }

        private void AegisSearcher()
        {
            var tickDelta = Game.GameTime - DeathTime;
            RoshanMinutes = Math.Floor(tickDelta / 60);
            RoshanSeconds = tickDelta % 60;
            RoshIsAlive = Roshan != null && Roshan.IsValid && Roshan.IsAlive;

            if (!RoshIsAlive)
            {
                Roshan =
                    EntityManager<Unit>.Entities
                        //ObjectManager.GetEntities<Unit>()
                        .FirstOrDefault(
                            unit =>
                                unit.Name == "npc_dota_roshan" /*unit.ClassId == ClassId.CDOTA_Unit_Roshan*/&&
                                unit.IsAlive);
            }
            if (AegisEvent)
            {
                tickDelta = Game.GameTime - AegisTime;
                AegisMinutes = Math.Floor(tickDelta / 60);
                AegisSeconds = tickDelta % 60;
                //Log.Debug($"Timer {AegisMinutes}:{AegisSeconds}");
                if (!AegisWasFound)
                    Aegis = EntityManager<Item>.Entities.FirstOrDefault(x => x.Name == "item_aegis");
                if (Aegis != null && !AegisWasFound)
                {
                    Log.Debug($"Aegis found! {Aegis?.Owner?.Name}");
                    AegisWasFound = true;
                }
                if (4 - AegisMinutes < 0 || (AegisWasFound && (Aegis == null || !Aegis.IsValid)))
                {
                    AegisEvent = false;
                    AegisWasFound = false;
                    Log.Debug("Flush Aegis Timer");
                }
            }
        }

        public float RoshanSeconds { get; set; }

        public double RoshanMinutes { get; set; }

        public float AegisSeconds { get; set; }

        public double AegisMinutes { get; set; }

        public bool AegisWasFound { get; set; }

        public Item Aegis { get; set; }

        private void DrawingOnOnDraw(EventArgs args)
        {
            string text="";
            if (!RoshIsAlive)
            {
                if (RoshanMinutes < 8)
                    text =
                        $"Roshan: {7 - RoshanMinutes}:{59 - RoshanSeconds:0.} - {10 - RoshanMinutes}:{59 - RoshanSeconds:0.}";
                else if (RoshanMinutes == 8)
                {
                    text =
                        $"Roshan: {8 - RoshanMinutes}:{59 - RoshanSeconds:0.} - {10 - RoshanMinutes}:{59 - RoshanSeconds:0.}";
                }
                else if (RoshanMinutes == 9)
                {
                    text =
                        $"Roshan: {9 - RoshanMinutes}:{59 - RoshanSeconds:0.} - {10 - RoshanMinutes}:{59 - RoshanSeconds:0.}";
                }
                else
                {
                    text = $"Roshan: {0}:{59 - RoshanSeconds:0.}";
                    if (59 - RoshanSeconds <= 1)
                    {
                        RoshIsAlive = true;
                    }
                }
            }
            var textClr = Color.White;
            var outLineClr = RoshIsAlive ? Color.YellowGreen : Color.Red;
            var endText = RoshIsAlive ? "Roshan alive" : Math.Abs(DeathTime) < 0.01f ? "Roshan death" : text;
            var textSize = new Vector2(TextSize);
            var textPos = new Vector2(PosX, PosY);
            DrawText(textPos, textSize, endText, textClr, outLineClr, true);
            if (AegisEvent)
            {
                try
                {
                    text = $"Aegis Timer: {4 - AegisMinutes}:{59 - AegisSeconds:0.}";
                    if (Aegis != null)
                        if (Aegis.Owner != null)
                            DrawTextWithIcon(textPos + new Vector2(1, TextSize.Value.Value), textSize, text, textClr,
                                Color.YellowGreen, Textures.GetHeroTexture(Aegis.Owner.Name));
                        else
                        {
                            DrawText(textPos + new Vector2(1, TextSize.Value.Value), textSize, text, textClr,
                                Color.YellowGreen);
                        }
                }
                catch (Exception e)
                {

                }
                
                //DrawText(textPos + new Vector2(0, TextSize.Value.Value), textSize, text, textClr, Color.YellowGreen);
                /*Drawing.DrawText(text, new Vector2(PosX, PosY + TextSize.Value.Value), new Vector2(TextSize), Color.YellowGreen,
                    FontFlags.DropShadow | FontFlags.AntiAlias);*/
            }
        }

        private void DrawText(Vector2 textPos, Vector2 textSize, string endText, Color textClr, Color outLineClr, bool checkForMovable = false)
        {
            var measureText = Drawing.MeasureText(endText, "arial", textSize,
                FontFlags.DropShadow | FontFlags.AntiAlias);
            
            Drawing.DrawRect(textPos, measureText, new Color(100, 100, 100, 100));
            Drawing.DrawText(endText, textPos, textSize, textClr,
                FontFlags.DropShadow | FontFlags.AntiAlias);
            Drawing.DrawRect(textPos, measureText, outLineClr, true);

            if (checkForMovable)
            {
                if (CanMove)
                {
                    if (CanMoveWindow(ref textPos, measureText))
                    {
                        PosX.Item.SetValue(new Slider((int)textPos.X, 1, 2500));
                        PosY.Item.SetValue(new Slider((int)textPos.Y, 1, 2500));
                    }
                }
            }
        }

        private void DrawTextWithIcon(Vector2 textPos, Vector2 textSize, string endText, Color textClr, Color outLineClr, DotaTexture texture)
        {
            var measureText = Drawing.MeasureText(endText, "arial", textSize,
                FontFlags.DropShadow | FontFlags.AntiAlias);
            
            var rectSize = measureText + new Vector2(measureText.Y + 2, 0);
            Drawing.DrawRect(textPos, rectSize, new Color(100, 100, 100, 100));

            Drawing.DrawText(endText, textPos + new Vector2(textSize.Y + 2, 0), textSize, textClr,
                FontFlags.DropShadow | FontFlags.AntiAlias);
            Drawing.DrawRect(textPos, new Vector2(measureText.Y), texture);

            Drawing.DrawRect(textPos, rectSize, outLineClr, true);

        }

        public void Game_OnGameEvent(FireEventEventArgs args)
        {
            if (args.GameEvent.Name == "dota_roshan_kill")
            {
                DeathTime = Game.GameTime;
                RoshIsAlive = false;
            }
            if (args.GameEvent.Name == "aegis_event")
            {
                AegisTime = Game.GameTime;
                AegisEvent = true;
                Log.Info($"Event: {args.GameEvent.Name}");
            }
        }

        public bool AegisEvent { get; set; }

        public float AegisTime { get; set; }

        public bool RoshIsAlive { get; set; }

        public float DeathTime { get; set; }

        public MenuItem<Slider> PosX { get; set; }
        public MenuItem<Slider> PosY { get; set; }

        public MenuItem<bool> CanMove { get; set; }

        public MenuItem<bool> Enable { get; set; }

        public void Dispose()
        {
            if (Enable)
            {
                Drawing.OnDraw -= DrawingOnOnDraw;
                UpdateManager.Unsubscribe(AegisSearcher);
            }
        }
    }
}