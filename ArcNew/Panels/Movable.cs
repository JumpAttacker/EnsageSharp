using Ensage;
using Ensage.Common.Objects.UtilityObjects;
using SharpDX;

namespace ArcAnnihilation.Panels
{
    public abstract class Movable
    {
        private Sleeper _sleeper;
        private Vector2 _globalDif;
        private bool _isClicked = false;
        private bool _movableLoaded = false;
        public void LoadMovable()
        {
            if (_movableLoaded)
                return;
            _movableLoaded = true;
            _sleeper = new Sleeper();
            Game.OnWndProc += Game_OnWndProc;
        }
        public void UnloadMovable()
        {
            if (!_movableLoaded)
                return;
            _movableLoaded = false;
            Game.OnWndProc -= Game_OnWndProc;
        }

        private void Game_OnWndProc(WndEventArgs args)
        {
            if (Game.IsChatOpen)
            {
                return;
            }
            switch (args.Msg)
            {
                case (uint)Ensage.Common.Utils.WindowsMessages.WM_LBUTTONUP:
                    _isClicked = false;
                    break;
                case (uint)Ensage.Common.Utils.WindowsMessages.WM_LBUTTONDOWN:
                    _isClicked = true;
                    break;
            }
        }

        public bool CanMoveWindow(ref Vector2 startPos, Vector2 size, bool drawMovablePosition = false)
        {
            if (!_movableLoaded)
                return false;
            if (drawMovablePosition)
            {
                Drawing.DrawRect(startPos, size, new Color(0, 0, 0, 155));
                Drawing.DrawRect(startPos, size, new Color(155, 155, 155, 255), true);
            }
            var mPos = Game.MouseScreenPosition;
            if (Ensage.Common.Utils.IsUnderRectangle(mPos, startPos.X, startPos.Y, size.X, size.Y))
            {
                if (_isClicked)
                {
                    if (!_sleeper.Sleeping)
                    {
                        _globalDif = mPos - startPos;
                        _sleeper.Sleep(500);
                    }
                    startPos = mPos - _globalDif;
                    return true;
                }
            }
            return false;
        }
    }
}