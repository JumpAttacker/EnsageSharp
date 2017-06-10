using System.Runtime.InteropServices;
using System.Security.Permissions;
using Ensage.Common.Menu;
using Ensage.Common.Objects.UtilityObjects;
using SharpDX;

namespace BadGuy.Features
{
    public static class DrawingAction
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
        private enum MouseEvent
        {
            MouseeventfLeftdown = 0x02,
            MouseeventfLeftup = 0x04,
        }
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetCursorPos(int x, int y);
        private static readonly Sleeper RateSleeper = new Sleeper();
        private static float _stageX;
        private static float _stageY;
        private static Vector2 _drawingStartPos = new Vector2(10, 809);
        private static Vector2 _drawingEndPos = new Vector2(270, 1061);//270
        private static bool _forward = true;

        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        public static void Updater()
        {
            if (RateSleeper.Sleeping || !BadGuy.Config.Drawing.Key.Value.Active)
                return;
            RateSleeper.Sleep(BadGuy.Config.Drawing.Rate);
            if (_forward)
            {
                SetCursorPos((int)_stageX, (int)_stageY);
                _stageY += BadGuy.Config.Drawing.Speed;
            }
            else
            {
                SetCursorPos((int)_drawingEndPos.X, (int)_stageY);
            }
            if (_stageY > _drawingEndPos.Y)
                _stageY = _drawingStartPos.Y;
            _forward = !_forward;
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        public static void KeyOnPropertyChanged(object sender, OnValueChangeEventArgs args)
        {
            if (!BadGuy.Config.Drawing.Enable)
                return;
            if (args.GetNewValue<KeyBind>().Active)
            {
                //Enable = true;
                _stageX = _drawingStartPos.X;
                _stageY = _drawingStartPos.Y;
                SetCursorPos((int)_drawingStartPos.X, (int)_drawingStartPos.Y);
            }
            else
            {
                //Enable = false;
            }
            mouse_event(
                args.GetNewValue<KeyBind>().Active
                    ? (int)MouseEvent.MouseeventfLeftdown
                    : (int)MouseEvent.MouseeventfLeftup, 0, 0, 0, 0);
        }
    }
}