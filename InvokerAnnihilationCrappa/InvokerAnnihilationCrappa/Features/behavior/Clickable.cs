using System;
using System.Windows.Forms;
using Ensage;
using Ensage.Common;
using Ensage.SDK.Input;
using SharpDX;
using MouseEventArgs = Ensage.SDK.Input.MouseEventArgs;

namespace InvokerAnnihilationCrappa.Features.behavior
{
    public abstract class Clickable
    {
        private Vector2 _position;
        private Vector2 _size;
        public bool LoadedClickable;
        public delegate void Ui();
        public event Ui OnClick;
        private IInputManager _inputManager;
        public void LoadClickable(IInputManager input)
        {
            if (LoadedClickable)
                return;
            LoadedClickable = true;
            _inputManager = input;
            _inputManager.MouseClick += InputManagerOnMouseClick;
            //Game.OnWndProc += Game_OnWndProc;
        }

        private void InputManagerOnMouseClick(object sender, MouseEventArgs mouseEventArgs)
        {
            if ((mouseEventArgs.Buttons & MouseButtons.Left) != 0)
            {
                if (Utils.IsUnderRectangle(Game.MouseScreenPosition, _position.X, _position.Y, _size.X, _size.Y))
                {
                    OnClick?.Invoke();
                }
            }
        }

        public void UnloadClickable()
        {
            if (!LoadedClickable)
                return;
            LoadedClickable = false;
            _inputManager.MouseClick -= InputManagerOnMouseClick;
            //Game.OnWndProc -= Game_OnWndProc;
        }

        public void Update(Vector2 pos, Vector2 size)
        {
            _position = pos;
            _size = size;
        }

        private void Game_OnWndProc(WndEventArgs args)
        {
            if (Game.IsChatOpen)
            {
                return;
            }
            switch (args.Msg)
            {
                case (uint)Utils.WindowsMessages.WM_LBUTTONUP:
                    if (Utils.IsUnderRectangle(Game.MouseScreenPosition, _position.X, _position.Y, _size.X, _size.Y))
                    {
                        OnClick?.Invoke();
                    }
                    break;
            }
        }
    }
}