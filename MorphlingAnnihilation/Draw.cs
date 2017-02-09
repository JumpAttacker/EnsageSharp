using System;
using System.Collections.Generic;
using Ensage;
using Ensage.Common.Menu;
using SharpDX;

namespace MorphlingAnnihilation
{
    public static class Draw
    {
        private static List<DrawStates> _states; 
        public static void Init()
        {
            _states = new List<DrawStates>
            {
                new DrawStates("all", "all.hotkey"),
                new DrawStates("hero", "hero.hotkey"),
                new DrawStates("replicate", "replicate.hotkey"),
                new DrawStates("hybrid", "hybrid.hotkey")
            };
        }
        
        public static void OnDrawing(EventArgs args)
        {
            var startPos = MenuManager.GetPosition;
            var size = new Vector2(MenuManager.GetSize);
            foreach (var state in _states)
            {
                var getState = state.GetState;
                var getText = getState ? "+" : "-";
                var text = $"{state.DisplayName}: [{getText}]";
                Drawing.DrawText(text, startPos, size, getState?Color.GreenYellow:Color.DarkRed,
                    FontFlags.AntiAlias | FontFlags.StrikeOut);
                startPos += new Vector2(0, size.X);
            }
        }
        private class DrawStates
        {
            public string DisplayName { get; }
            private readonly string _name;
            public DrawStates(string name, string hiddenName)
            {
                DisplayName = name;
                _name = hiddenName;
            }

            public bool GetState => MenuManager.Menu.Item(_name).GetValue<KeyBind>().Active;
        }
    }
}