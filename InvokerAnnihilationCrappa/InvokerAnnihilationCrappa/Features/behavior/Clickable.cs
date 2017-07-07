using Ensage;
using Ensage.Common;
using SharpDX;

namespace InvokerAnnihilationCrappa.Features.behavior
{
    public abstract class Clickable
    {
        private Vector2 _position;
        private Vector2 _size;
        public bool LoadedClickable;
        public delegate void Ui();
        public event Ui OnClick;
        public void LoadClickable()
        {
            if (LoadedClickable)
                return;
            LoadedClickable = true;
            Game.OnWndProc += Game_OnWndProc;
        }
        public void UnloadClickable()
        {
            if (!LoadedClickable)
                return;
            LoadedClickable = false;
            Game.OnWndProc -= Game_OnWndProc;
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