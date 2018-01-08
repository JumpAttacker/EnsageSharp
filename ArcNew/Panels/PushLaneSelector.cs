using System;
using System.Linq;
using System.Windows.Forms;
using ArcAnnihilation.OrderState;
using ArcAnnihilation.Utils;
using Ensage;
using Ensage.Common.Menu;
using Ensage.SDK.Input;
using SharpDX;
using MouseButtons = Ensage.SDK.Input.MouseButtons;
using MouseEventArgs = Ensage.SDK.Input.MouseEventArgs;

namespace ArcAnnihilation.Panels
{
    public class PushLaneSelector : Movable
    {
        public class Button
        {
            public string Text;
            public Vector2 Position;
            public Vector2 Size;
            public bool UnderMouse;
            public bool Active;

            public Button(string text, Vector2 position, Vector2 size)
            {
                Text = text;
                Position = position;
                Size = size;
            }
            public Button(string text, bool active=false)
            {
                Text = text;
                Active = active;
            }
        }
        private static PushLaneSelector _panel;
        public bool Loaded;
        public string GetSelectedLane => _buttons.FirstOrDefault(x => x.Active)?.Text;
        public enum LanesType
        {
            AutoPushing, OnlyTop, OnlyMid, OnlyBot
        }
        public void SetSelectedLane(LanesType type)
        {
            foreach (var button in _buttons)
            {
                button.Active = false;
            }

            switch (type)
            {
                case LanesType.AutoPushing:
                    _buttons[0].Active = true;
                    break;
                case LanesType.OnlyTop:
                    _buttons[1].Active = true;
                    break;
                case LanesType.OnlyMid:
                    _buttons[2].Active = true;
                    break;
                case LanesType.OnlyBot:
                    _buttons[3].Active = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
        private readonly Button[] _buttons;
        
        public PushLaneSelector()
        {
            Input = new InputManager();
            _buttons = new[]
            {
                new Button("Auto Pushing",true),
                new Button("Only Top"),
                new Button("Only Mid"),
                new Button("Only Bot")
            };
        }

        private IInputManager Input { get; }
        private void OnDrawing(EventArgs args)
        {
            if (!MenuManager.IsEnable)
                return;
            var canGo = OrderManager.CurrentOrder is AutoPushing || OrderManager.CurrentOrder is Idle;
            if (!canGo && MenuManager.PushLanePanelHide)
                return;
            var startPos = MenuManager.GetPushLanePanelPosition;//new Vector2(10, 350);
            var size = MenuManager.GetPushLaneSelectorSize;
            var text = _buttons[0].Text;
            var order = OrderManager.CurrentOrder as AutoPushing;
            if (_buttons[0].Active && order?.CurrentLane != null)
            {
                text += $" {order.CurrentLane}";
            }
            var textSize = Drawing.MeasureText($"{text}", "Arial",
                new Vector2(size), FontFlags.None);
            var rectSize = new Vector2(textSize.X, textSize.Y * 4);
            if (MenuManager.PushLanePanelCanBeMovedByMouse)
            {
                if (CanMoveWindow(ref startPos, rectSize))
                {
                    MenuManager.SetPushLanePanelPosition((int) startPos.X, (int) startPos.Y);
                }
            }
            Drawing.DrawRect(startPos, rectSize, new Color(0, 0, 0, 155));
            if (OrderManager.CurrentOrder is AutoPushing)
                Drawing.DrawRect(startPos - new Vector2(1, 1), rectSize + new Vector2(2, 2), new Color(0, 255, 0, 255),
                    true);
            else
                Drawing.DrawRect(startPos - new Vector2(1, 1), rectSize + new Vector2(2, 2), new Color(155, 0, 0, 255),
                    true);
            var count = 0;
            foreach (var button in _buttons)
            {
                button.Position = startPos + new Vector2(0, textSize.Y * count++);
                button.Size = DrawText(count == 1 ? text : button.Text, new Vector2(size), button);
            }
        }
        public void Load()
        {
            if (Loaded) return;
            LoadMovable();
            Drawing.OnDraw += OnDrawing;
            Loaded = true;
            Input.MouseClick += OnMouseClick;
            Printer.Both($"[{this}] loaded");
        }
        public void UnLoad()
        {
            if (!Loaded) return;
            Loaded = false;
            UnloadMovable();
            Drawing.OnDraw -= OnDrawing;
            Input.MouseClick -= OnMouseClick;
            Printer.Both($"[{this}] unloaded");
        }

        private static Vector2 DrawText(string text, Vector2 tSize, Button button)
        {
            Vector2 startPos = button.Position;
            var textSize = Drawing.MeasureText(text, "Arial",
                tSize, FontFlags.None);
            if (Ensage.Common.Utils.IsUnderRectangle(Game.MouseScreenPosition, startPos.X, startPos.Y, textSize.X,
                textSize.Y))
            {
                Drawing.DrawRect(startPos, textSize, new Color(0, 0, 0, 155));
                button.UnderMouse = true;
            }
            else
            {
                button.UnderMouse = false;
            }
            var textPos = startPos;
            Drawing.DrawText(
                text,
                textPos, tSize,
                button.Active ? Color.GreenYellow : Color.White,
                FontFlags.AntiAlias | FontFlags.StrikeOut);
            return textSize;
        }

        public static PushLaneSelector GetInstance()
        {
            return _panel ?? (_panel = new PushLaneSelector());
        }

        public static void OnChange(object sender, OnValueChangeEventArgs e)
        {
            if (e.GetNewValue<bool>())
                GetInstance().Load();
            else
                GetInstance().UnLoad();
        }

        private void OnMouseClick(object sender, MouseEventArgs e)
        {
            if (e.Buttons != MouseButtons.LeftUp)
                return;
            var button = _buttons.FirstOrDefault(x => x.UnderMouse);
            if (button == null) return;
            button.Active = true;
            foreach (var b in _buttons.Where(x=>!x.UnderMouse))
                b.Active = false;
        }
    }
}