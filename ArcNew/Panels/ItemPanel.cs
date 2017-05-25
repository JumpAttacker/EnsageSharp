using System;
using System.Linq;
using ArcAnnihilation.Manager;
using ArcAnnihilation.Utils;
using Ensage;
using Ensage.Common;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using SharpDX;

namespace ArcAnnihilation.Panels
{
    public class ItemPanel : Movable
    {
        private bool _loaded;
        private static ItemPanel _panel;

        public void OnDrawing(EventArgs args)
        {
            if (!MenuManager.IsEnable)
                return;

            var count = 0;
            var startPos = MenuManager.GetItemPanelPosition;
            if (MenuManager.ItemPanelCanBeMovedByMouse && CanMoveWindow(ref startPos,new Vector2(85),true))
            {
                MenuManager.SetItemPanelPosition((int)startPos.X, (int)startPos.Y);
            }
            foreach (var item in TempestManager.Tempest.Inventory.Items.Where(x=>x.AbilityState == AbilityState.OnCooldown))
            {
                count++;
                
                var size = MenuManager.GetItemPanelSize;
                Drawing.DrawRect(startPos, new Vector2(size, size / 2f), Textures.GetItemTexture(item.StoredName()));
                Drawing.DrawRect(startPos, new Vector2(size * 70 / 100f, size / 2f), Color.White, true);

                var text = ((int) item.Cooldown+1).ToString();
                var textSize = Drawing.MeasureText(text, "Arial",
                    new Vector2((float)(size/2 * .9), (float)(size/2 * .9)), FontFlags.AntiAlias);
                Drawing.DrawRect(startPos + new Vector2(0, 2), textSize + new Vector2(0, -2), new Color(0, 0, 0, 155));
                var textPos = startPos;
                Drawing.DrawText(
                    text,
                    textPos + new Vector2(2, 2),
                    new Vector2(textSize.Y, 0),
                    Color.White,
                    FontFlags.AntiAlias | FontFlags.StrikeOut);

                if (count == 3)
                    startPos = new Vector2(112, 215 + size / 2f);
                else
                    startPos += new Vector2(size * 70 / 100f, 0);

            }
        }

        public void Load()
        {
            if (_loaded) return;
            Drawing.OnDraw += OnDrawing;
            _loaded = true;
            LoadMovable();
            Events.OnClose += EventsOnOnClose;
            Printer.Both("[InfoPanel] loaded");
        }

        private void EventsOnOnClose(object sender, EventArgs eventArgs)
        {
            UnLoad();
        }

        public void UnLoad()
        {
            if (!_loaded) return;
            _loaded = false;
            Drawing.OnDraw -= OnDrawing;
            Events.OnClose -= EventsOnOnClose;
            UnloadMovable();
            Printer.Both("[InfoPanel] unloaded");
        }

        public static ItemPanel GetItemPanel()
        {
            return _panel ?? (_panel = new ItemPanel());
        }

        public static void OnChange(object sender, OnValueChangeEventArgs e)
        {
            if (e.GetNewValue<bool>())
                GetItemPanel().Load();
            else
                GetItemPanel().UnLoad();
        }
    }
}