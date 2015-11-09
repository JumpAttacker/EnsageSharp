using System;
using System.Drawing.Drawing2D;
using System.Globalization;
using Ensage;
using Ensage.Common;
using SharpDX;

namespace ItemPanel
{
    internal class Program
    {
        #region Members
        //============================================================
        private static bool _loaded;
        private const string Ver = "0.2";
        private static bool _leftMouseIsPress;
        private static bool _showMenu = true;
        private static Vector2 _sizer = new Vector2(265, 300);
        private static Vector2 _startPos = new Vector2(50, 500);
        private static readonly Hero[] Heroes=new Hero[10];
        private static readonly float Con = Math.Max(1, HUDInfo.ScreenSizeX() / 1600);
        //============================================================
        private static bool _moving;
        //============================================================
        #endregion

        private static void Main()
        {
            _loaded = false;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnWndProc += Game_OnWndProc;
        }
        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (Game.IsChatOpen)
                return;
            if (args.WParam != 1 || !Utils.SleepCheck("clicker"))
            {
                _leftMouseIsPress = false;
                return;
            }
            _leftMouseIsPress = true;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var me = ObjectMgr.LocalHero;
            if (!_loaded)
            {
                if (!Game.IsInGame || me == null)
                {
                    return;
                }
                Flush();
                _loaded = true;
                PrintSuccess(string.Format("> ItemPanel Loaded v{0}", Ver));
            }

            if (!Game.IsInGame || me == null)
            {
                _loaded = false;
                PrintInfo("> ItemPanel unLoaded");
                return;
            }
            if (_moving)
            {
                _startPos = Game.MouseScreenPosition+new Vector2(-30,15);
            }
            var maxSize = new Vector2(25 * Con*7, 18*Con*7);
            if (_showMenu)
            {
                _sizer.X += 4;
                _sizer.Y += 4;
                _sizer.X = Math.Min(_sizer.X, maxSize.X);
                _sizer.Y = Math.Min(_sizer.Y, maxSize.Y);

                Drawing.DrawRect(_startPos, _sizer, new Color(0, 155, 255, 100));
                Drawing.DrawRect(_startPos, _sizer, new Color(0, 0, 0, 255), true);
                Drawing.DrawRect(_startPos + new Vector2(-5, -5), _sizer + new Vector2(10, 10),
                    new Color(0, 0, 0, 255), true);
                DrawButton(_startPos + new Vector2(_sizer.X - 20, -20), 20, 20, ref _showMenu, true, Color.Gray,
                    Color.Gray);
                if (!Equals(_sizer, maxSize)) return;

                DrawButton(_startPos + new Vector2(0, -20), _sizer.X-20, 20, ref _moving, true,
                    new Color(0, 200, 150),
                    new Color(200, 0, 0, 100), "Click to move");

                Drawing.DrawText(
                    string.Format("Item Panel v {0}", Ver),
                    _startPos + new Vector2(5, 5), Color.White,
                    FontFlags.AntiAlias | FontFlags.DropShadow);
                uint i = 0;
                for (uint num = 0; num < 10; num++)
                {
                    if (Heroes[num] == null || !Heroes[num].IsValid)
                    {
                        try
                        {
                            Heroes[num] = ObjectMgr.GetPlayerById(num).Hero;
                        }
                        catch
                        {
                            continue;
                        }
                    }
                    if (Heroes[num].Team == me.Team) continue;

                    Drawing.DrawRect(_startPos + new Vector2(5, 18*Con*(i + 1)), new Vector2(18*Con, 18*Con),
                        Drawing.GetTexture(string.Format("materials/ensage_ui/miniheroes/{0}.vmat",
                            Heroes[num].Name.Replace("npc_dota_hero_", ""))));
                    for (var i2 = 1; i2 <= 6; i2++)
                    {
                        string texturename;
                        Item item;
                        try
                        {
                            item = Heroes[num].Inventory.GetItem((ItemSlot)i2 - 1);
                        }
                        catch (Exception)
                        {
                            item = null;
                        }
                        if (item == null)
                        {
                            texturename = "materials/ensage_ui/items/emptyitembg.vmat";
                        }
                        else if (item.IsRecipe)
                        {
                            texturename = "materials/ensage_ui/items/recipe.vmat";
                        }
                        else
                        {
                            texturename = string.Format("materials/ensage_ui/items/{0}.vmat",
                                item.Name.Replace("item_", ""));
                        }
                        Drawing.DrawRect(_startPos + new Vector2(25 * Con * i2, 18 * Con * (i + 1)), new Vector2(32 * Con, 18 * Con),
                            Drawing.GetTexture(texturename));
                        if (item == null) continue;
                        if (item.AbilityState == AbilityState.OnCooldown)
                        {
                            Drawing.DrawRect(_startPos + new Vector2(25 * Con * i2, 18 * Con * (i + 1)),
                                new Vector2((30 * Con)/2, (19 * Con)/2), new Color(255, 255, 255, 255));
                            Drawing.DrawText(((int) item.Cooldown).ToString(CultureInfo.InvariantCulture),
                                _startPos + new Vector2(25*Con*i2, 18*Con*(i + 1)), Color.Black,
                                FontFlags.AntiAlias | FontFlags.DropShadow);
                        }

                        if (item.AbilityState == AbilityState.NotEnoughMana)
                        {
                            Drawing.DrawRect(_startPos + new Vector2(25*Con*i2, 18*Con*(i + 1)),
                                new Vector2(32*Con, 18*Con), new Color(0, 0, 255, 50));
                        }
                    }
                    i++;
                }
            }
            else
            {
                _sizer.X -= 4;
                _sizer.Y -= 4;
                _sizer.X = Math.Max(_sizer.X, 20);
                _sizer.Y = Math.Max(_sizer.Y, 0);
                Drawing.DrawRect(_startPos, _sizer, new Color(0, 155, 255, 100));
                Drawing.DrawRect(_startPos, _sizer, new Color(0, 0, 0, 255), true);
                DrawButton(_startPos + new Vector2(_sizer.X - 20, -20), 20, 20, ref _showMenu, true, Color.Gray,
                    Color.Gray);
            }
        }

        private static void Flush()
        {
            for (uint i = 0; i < 10; i++)
                Heroes[i] = null;
        }

        #region Helpers
        private static void DrawButton(Vector2 a, float w, float h, ref bool clicked, bool isActive, Color @on, Color off, string drawOnButtonText = "")
        {
            var isIn = Utils.IsUnderRectangle(Game.MouseScreenPosition, a.X, a.Y, w, h);
            if (isActive)
            {
                if (_leftMouseIsPress && Utils.SleepCheck("ClickButtonCd") && isIn)
                {
                    clicked = !clicked;
                    Utils.Sleep(250, "ClickButtonCd");
                }
                var newColor = isIn
                    ? new Color((int)(clicked ? @on.R : off.R), clicked ? @on.G : off.G, clicked ? @on.B : off.B, 150)
                    : clicked ? @on : off;
                Drawing.DrawRect(a, new Vector2(w, h), newColor);
                Drawing.DrawRect(a, new Vector2(w, h), new Color(0, 0, 0, 255), true);
                if (drawOnButtonText != "")
                {
                    Drawing.DrawText(drawOnButtonText, a + new Vector2(10, 2), Color.White,
                    FontFlags.AntiAlias | FontFlags.DropShadow);
                }
            }
            else
            {
                Drawing.DrawRect(a, new Vector2(w, h), Color.Gray);
                Drawing.DrawRect(a, new Vector2(w, h), new Color(0, 0, 0, 255), true);
            }
        }
        public static void PrintInfo(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.White, arguments);
        }

        public static void PrintSuccess(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.Green, arguments);
        }

        public static void PrintError(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.Red, arguments);
        }

        public static void PrintEncolored(string text, ConsoleColor color, params object[] arguments)
        {
            var clr = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text, arguments);
            Console.ForegroundColor = clr;
        }
        #endregion

    }
}