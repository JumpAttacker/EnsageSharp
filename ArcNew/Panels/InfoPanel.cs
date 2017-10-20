using System;
using ArcAnnihilation.OrderState;
using ArcAnnihilation.Utils;
using Ensage;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using SharpDX;

namespace ArcAnnihilation.Panels
{
    public class InfoPanel : Movable
    {
        private static InfoPanel _panel;
        private bool _loaded;
        
        private void OnDrawing(EventArgs args)
        {
            if (!MenuManager.IsEnable)
                return;
            var isIdle = !OrderManager.CurrentOrder.CanBeExecuted;
            if (isIdle && !MenuManager.InfoPanelCanBeMovedByMouse)
                return;
            var isAutoPushing = OrderManager.CurrentOrder as AutoPushing;
            var isDefaultCombo = OrderManager.CurrentOrder as DefaultCombo;
            var isSparkSpam = OrderManager.CurrentOrder as SparkSpam;
            var isSparkSpamTempest = OrderManager.CurrentOrder as SparkSpamTempest;
            var isTempestCombo = OrderManager.CurrentOrder as TempestCombo;
            var startPos = MenuManager.GetInfoPanelPosition;//new Vector2(10, 350);
            var tSize = new Vector2(MenuManager.GetInfoPanelSize);
            if (MenuManager.InfoPanelCanBeMovedByMouse)
            {
                var tempSize = new Vector2(200, 200);
                if (CanMoveWindow(ref startPos, tempSize, true))
                {
                    MenuManager.SetInfoPanelPosition((int) startPos.X, (int) startPos.Y);
                    return;
                }
            }
            if (isAutoPushing != null && !PushLaneSelector.GetInstance().Loaded)
            {
                var lane = isAutoPushing.CurrentLane;

                var size = DrawText($"Pushing: [{lane}]", tSize, startPos);
                startPos += new Vector2(0, size.Y);
                DrawText($"Status: [{isAutoPushing.GetStatus()}]", tSize, startPos);
            }
            else if (isDefaultCombo != null)
            {
                var target = Core.MainHero.Orbwalker.GetTarget();
                var size = DrawText($"Default Combo", tSize, startPos);

                if (target != null)
                {
                    startPos += new Vector2(0, size.Y);
                    DrawHeroIcon(target, new Vector2(size.X / 3), startPos);
                }
            }
            else if (isTempestCombo != null)
            {
                var size = DrawText($"Tempest Combo", tSize, startPos);
                if (Core.TempestHero==null)
                    return;
                var target = Core.Target;
                if (target != null)
                {
                    startPos += new Vector2(0, size.Y);
                    DrawHeroIcon(target, new Vector2(size.X / 3), startPos);
                }
            }
            else if (isSparkSpam != null)
            {
                DrawText($"Spark Spam", tSize, startPos);
            }
            else if (isSparkSpamTempest != null)
            {
                DrawText($"[Tempest] Spark Spam", tSize, startPos);
            }
        }
        public void Load()
        {
            if (_loaded) return;
            LoadMovable();
            Drawing.OnDraw += OnDrawing;
            _loaded = true;
            Printer.Both("[InfoPanel] loaded");
        }

        public void UnLoad()
        {
            if (!_loaded) return;
            _loaded = false;
            UnloadMovable();
            Drawing.OnDraw -= OnDrawing;
            Printer.Both("[InfoPanel] unloaded");
        }

        private static Vector2 DrawText(string text, Vector2 tSize, Vector2 startPos)
        {
            var textSize = Drawing.MeasureText(text, "Arial",
                tSize, FontFlags.None);
            Drawing.DrawRect(startPos, textSize, new Color(0, 0, 0, 155));
            var textPos = startPos;
            Drawing.DrawText(
                text,
                textPos, tSize,
                Color.White,
                FontFlags.AntiAlias | FontFlags.StrikeOut);
            return textSize;
        }
        private static Vector2 DrawHeroIcon(Unit target, Vector2 size, Vector2 startPos)
        {
            var extra = new Vector2(size.X / 3, 0);
            var finalSize = size + extra;
            if (target is Hero)
            {
                Drawing.DrawRect(startPos, finalSize, Textures.GetHeroTexture(target.StoredName()));
            }
            else
            {
                Drawing.DrawRect(startPos, finalSize, new Color(0, 155, 255));
            }
            Drawing.DrawRect(startPos, finalSize, new Color(0, 0, 0, 255), true);
            return finalSize;
        }

        public static InfoPanel GetInfoPanel()
        {
            return _panel ?? (_panel = new InfoPanel());
        }

        public static void OnChange(object sender, OnValueChangeEventArgs e)
        {
            if (e.GetNewValue<bool>())
                GetInfoPanel().Load();
            else
                GetInfoPanel().UnLoad();
        }
    }
}