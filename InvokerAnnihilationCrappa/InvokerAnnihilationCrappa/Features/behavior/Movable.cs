using System.Windows.Forms;
using Ensage;
using Ensage.Common;
using Ensage.Common.Objects.UtilityObjects;
using Ensage.SDK.Input;
using SharpDX;

namespace InvokerAnnihilationCrappa.Features.behavior
{
    public abstract class Movable
    {
        private Sleeper _sleeper;
        private Vector2 _globalDif;
        private bool _movableLoaded;
        private IInputManager _inputManager;
        public void LoadMovable(IInputManager valueInput)
        {
            if (_movableLoaded)
                return;
            _movableLoaded = true;
            _sleeper = new Sleeper();
            _inputManager = valueInput;
        }


        public bool CanMoveWindow(ref Vector2 startPos, Vector2 size, bool drawMovablePosition = false)
        {
            if (drawMovablePosition)
            {
                Drawing.DrawRect(startPos, size, new Color(155, 155, 155, 155));
                Drawing.DrawRect(startPos, size, new Color(0, 0, 0, 255), true);
            }
            var mPos = Game.MouseScreenPosition;
            if (Utils.IsUnderRectangle(mPos, startPos.X, startPos.Y, size.X, size.Y))
            {
                if ((_inputManager.ActiveButtons & MouseButtons.Left) != 0)
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