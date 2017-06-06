using Ensage;

namespace ArcAnnihilation
{
    public class InputBlocker
    {
        private static InputBlocker _inputBlocker;
        public void Load()
        {
            Game.OnWndProc += Game_OnWndProc;
        }
        public void UnLoad()
        {
            Game.OnWndProc -= Game_OnWndProc;
        }
        private void Game_OnWndProc(WndEventArgs args)
        {
            if (Game.IsChatOpen)
                return;
            if (args.Msg == (ulong)Ensage.Common.Utils.WindowsMessages.WM_KEYDOWN && MenuManager.InAnyCombo(args.WParam))
            {
                args.Process = false;
            }
        }
        public static InputBlocker GetInputBlocker()
        {
            return _inputBlocker ?? (_inputBlocker = new InputBlocker());
        }
    }
}