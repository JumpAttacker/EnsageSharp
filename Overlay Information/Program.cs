using System;
using System.Linq;
using System.Net.Mime;
using Ensage;
using Ensage.Common;
using SharpDX;
using SharpDX.Direct3D9;

// ReSharper disable EmptyGeneralCatchClause
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace Overlay_information
{


    internal class Program
    {
        #region Members

        private static bool _loaded;
        private static Hero _me;
        private static Player _player;
        private const float Ver =  0.4f;
        private static Vector2 _screenSizeVector2;
        private static ScreenSizer _drawHelper;
        private static bool IsOpen = false;
        private static bool LeftMouseIsPress;
        //======================================
        public static bool ShowCooldownOnTopPanel;
        public static bool ShowCooldownOnTopPanelLikeText; //working only with ShowCooldownOnTopPanel
        public static bool ShowHealthOnTopPanel;
        public static bool ShowManaOnTopPanel;
        public static bool OverlayOnlyOnEnemy;
        public static bool ShowGlyph;
        public static bool ShowIllusions;
        public static bool ShowLastHit;
        public static bool ShowManabars;
        //=====================================
        static readonly InitHelper _saveLoadSysHelper = new InitHelper("C:" + "\\jOverlay.ini");
        //=====================================
        private static readonly Font[] FontArray=new Font[21];
        private static Line _line;
        #endregion
        #region Methods

        #region Init

        private static void Main()
        {
            Game.OnUpdate += Game_OnUpdate;
            _loaded = false;
            Drawing.OnDraw += Drawing_OnDraw;
            ShowCooldownOnTopPanel = true;
            ShowHealthOnTopPanel = true;
            ShowManaOnTopPanel = true;
            ShowCooldownOnTopPanelLikeText = true;
            #region Init font & line

            for (var i = 0; i <= 20; i++)
            {
                FontArray[i] = new Font(
                Drawing.Direct3DDevice9,
                new FontDescription
                {
                    FaceName = "Tahoma",
                    Height = 10+i,
                    OutputPrecision = FontPrecision.Default,
                    Quality = FontQuality.Default
                });
            }
            _line = new Line(Drawing.Direct3DDevice9);
            #endregion

            Drawing.OnPreReset += Drawing_OnPreReset;
            Drawing.OnPostReset += Drawing_OnPostReset;
            Drawing.OnEndScene += Drawing_OnEndScene;
            AppDomain.CurrentDomain.DomainUnload += CurrentDomainDomainUnload;
            Game.OnWndProc += Game_OnWndProc;
            
            try
            {
                ShowHealthOnTopPanel = Convert.ToBoolean(_saveLoadSysHelper.IniReadValue("Booleans", "Show Health on top panel"));//
                ShowManaOnTopPanel = Convert.ToBoolean(_saveLoadSysHelper.IniReadValue("Booleans", "Show mana on top panel"));
                ShowCooldownOnTopPanel = Convert.ToBoolean(_saveLoadSysHelper.IniReadValue("Booleans", "Show Cooldown on top panel"));//
                ShowCooldownOnTopPanelLikeText = Convert.ToBoolean(_saveLoadSysHelper.IniReadValue("Booleans", "Show Cooldown on top panel (numbers)"));//
                OverlayOnlyOnEnemy = Convert.ToBoolean(_saveLoadSysHelper.IniReadValue("Booleans", "Overlay only on enemy"));//
                
                ShowGlyph = Convert.ToBoolean(_saveLoadSysHelper.IniReadValue("Booleans", "Show glyph cd"));//
                ShowIllusions = Convert.ToBoolean(_saveLoadSysHelper.IniReadValue("Booleans", "Show Illusions"));
                ShowLastHit = Convert.ToBoolean(_saveLoadSysHelper.IniReadValue("Booleans", "Show LastHit/Deny"));
                ShowManabars = Convert.ToBoolean(_saveLoadSysHelper.IniReadValue("Booleans", "Show manabars"));
            }
            catch
            {
                _saveLoadSysHelper.IniWriteValue("Booleans", "Show health on top panel", true.ToString());
                _saveLoadSysHelper.IniWriteValue("Booleans", "Show mana on top panel", true.ToString());
                _saveLoadSysHelper.IniWriteValue("Booleans", "Show cooldown on top panel", true.ToString());
                _saveLoadSysHelper.IniWriteValue("Booleans", "Show cooldown on top panel (numbers)", true.ToString());
                _saveLoadSysHelper.IniWriteValue("Booleans", "Overlay only on enemy", false.ToString());
                _saveLoadSysHelper.IniWriteValue("Booleans", "Show glyph cd", true.ToString());
                _saveLoadSysHelper.IniWriteValue("Booleans", "Show Illusions", true.ToString());
                _saveLoadSysHelper.IniWriteValue("Booleans", "Show LastHit/Deny", true.ToString());
                _saveLoadSysHelper.IniWriteValue("Booleans", "Show manabars", true.ToString());

                Console.Beep(1000, 100);
                Console.Beep(1000, 100);
                Console.Beep(1000, 100);
            }
            
        }

        #endregion
        #region !

        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (args.WParam != 1 || Game.IsChatOpen || !Utils.SleepCheck("clicker"))
            {
                LeftMouseIsPress = false;
                return;
            }
            LeftMouseIsPress = true;
        }
        private static void CurrentDomainDomainUnload(object sender, EventArgs e)
        {
            for (var i = 0; i <= 20; i++) 
                FontArray[i].Dispose();
            _line.Dispose();
        }

        private static void Drawing_OnPostReset(EventArgs args)
        {
            for (var i = 0; i <= 20; i++)
                FontArray[i].OnResetDevice();
            _line.OnResetDevice();
        }

        private static void Drawing_OnPreReset(EventArgs args)
        {
            for (var i = 0; i <= 20; i++)
                FontArray[i].OnLostDevice();
            _line.OnLostDevice();
        }


        #endregion
        private static void Drawing_OnEndScene(EventArgs args)
        {
            if (Drawing.Direct3DDevice9 == null || Drawing.Direct3DDevice9.IsDisposed || !Game.IsInGame || !_loaded)
            {
                return;
            }
            var player = ObjectMgr.LocalPlayer;
            if (player == null || player.Team == Team.Observer || !_loaded || _drawHelper == null)
            {
                return;
            }
            DrawButton(_drawHelper.MenuPos.X - 2, _drawHelper.MenuPos.Y, 60, 20, 1, ref IsOpen, true,
                new Color(0, 0, 0, 50), new Color(0, 0, 0, 50), new Color(0, 0, 0, 125));
            DrawShadowText("J-Overlay", (int)_drawHelper.MenuPos.X, (int)_drawHelper.MenuPos.Y, Color.White,
                    FontArray[5]);
            if (IsOpen)
            {
                DrawFilledBox(_drawHelper.MenuPos.X - 2, _drawHelper.MenuPos.Y, 60, 500, new ColorBGRA(0, 0, 0, 100));
                //-----------------------------------------------------------------------------------------------------------------
                DrawButton(_drawHelper.MenuPos.X + 5, _drawHelper.MenuPos.Y + 25, 50, 20, 1, ref ShowCooldownOnTopPanel,
                    true,
                    new Color(100, 255, 0, 50), new Color(100, 0, 0, 50), "Show Cooldown on top panel");
                //-----------------------------------------------------------------------------------------------------------------
                DrawButton(_drawHelper.MenuPos.X + 5, _drawHelper.MenuPos.Y + 50, 50, 20, 1, ref ShowCooldownOnTopPanelLikeText,
                    true,
                    new Color(100, 255, 0, 50), new Color(100, 0, 0, 50), "Show Cooldown on top panel (numbers)");
                //-----------------------------------------------------------------------------------------------------------------
                DrawButton(_drawHelper.MenuPos.X + 5, _drawHelper.MenuPos.Y + 75, 50, 20, 1, ref ShowHealthOnTopPanel,
                    true,
                    new Color(100, 255, 0, 50), new Color(100, 0, 0, 50), "Show Health on top panel");
                //-----------------------------------------------------------------------------------------------------------------
                DrawButton(_drawHelper.MenuPos.X + 5, _drawHelper.MenuPos.Y + 100, 50, 20, 1, ref ShowManaOnTopPanel,
                    true,
                    new Color(100, 255, 0, 50), new Color(100, 0, 0, 50), "Show Mana on top panel");
                //-----------------------------------------------------------------------------------------------------------------
                DrawButton(_drawHelper.MenuPos.X + 5, _drawHelper.MenuPos.Y + 125, 50, 20, 1, ref OverlayOnlyOnEnemy,
                    true,
                    new Color(100, 255, 0, 50), new Color(100, 0, 0, 50), "Overlay only on enemy");
                //-----------------------------------------------------------------------------------------------------------------
                DrawButton(_drawHelper.MenuPos.X + 5, _drawHelper.MenuPos.Y + 150, 50, 20, 1, ref ShowGlyph,
                    false,
                    new Color(100, 255, 0, 50), new Color(100, 0, 0, 50), "Show glyph cd");
                //-----------------------------------------------------------------------------------------------------------------
                DrawButton(_drawHelper.MenuPos.X + 5, _drawHelper.MenuPos.Y + 175, 50, 20, 1, ref ShowLastHit,
                    false,
                    new Color(100, 255, 0, 50), new Color(100, 0, 0, 50), "Show LastHit/Deny");
                //-----------------------------------------------------------------------------------------------------------------
                DrawButton(_drawHelper.MenuPos.X + 5, _drawHelper.MenuPos.Y + 200, 50, 20, 1, ref ShowIllusions,
                    false,
                    new Color(100, 255, 0, 50), new Color(100, 0, 0, 50), "Show Illusions");
                //-----------------------------------------------------------------------------------------------------------------
                DrawButton(_drawHelper.MenuPos.X + 5, _drawHelper.MenuPos.Y + 225, 50, 20, 1, ref ShowManabars,
                    false,
                    new Color(100, 255, 0, 50), new Color(100, 0, 0, 50), "Show manabars");
                //-----------------------------------------------------------------------------------------------------------------
            }
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Game.IsInGame || _me == null || !_loaded) return;
            if (_screenSizeVector2.X==0)
            {
                _screenSizeVector2=new Vector2(Drawing.Width,Drawing.Height);
                PrintSuccess(string.Format(">> OI: init screen size: {0}x{1} -> {2}", _screenSizeVector2.X,
                    _screenSizeVector2.Y, (int) Math.Floor((decimal) (_screenSizeVector2.X/_screenSizeVector2.Y*100))));
                switch ((int) Math.Floor((decimal) (_screenSizeVector2.X/_screenSizeVector2.Y*100)))
                {
                    case 177:
                        _drawHelper = new ScreenSizer(66, 1063 - 855, 7, 43, 528, new Vector2(1860, 49));
                        break;
                    case 166:
                        _drawHelper = new ScreenSizer(66, 1063 - 855, 7, 43, 528, new Vector2(1860, 49));
                        break;
                    case 160:
                        _drawHelper = new ScreenSizer(66, 1063 - 855, 7, 43, 528, new Vector2(1860, 49));
                        break;
                    case 133:
                        _drawHelper = new ScreenSizer(66, 1063 - 855, 7, 43, 528, new Vector2(1860, 49));
                        break;
                    case 125:
                        _drawHelper = new ScreenSizer(66, 1063 - 855, 7, 43, 528, new Vector2(1860, 49));
                        break;
                    default:
                        _drawHelper = new ScreenSizer(66, 1063 - 855, 7, 43, 528, new Vector2(1860, 49));
                        break;
                }
            }
            uint i;
            for (i = 0; i < 10; i++)
                {
                    try
                    {
                        var v = ObjectMgr.GetPlayerById(i).Hero;
                        if (OverlayOnlyOnEnemy && v.Team==_me.Team) continue;
                        if (v == null || !v.IsAlive) continue;
                        var initPos = i >= 5
                            ? (_drawHelper.RangeBetween + _drawHelper.FloatRange*i) + _drawHelper.Space
                            : (_drawHelper.RangeBetween + _drawHelper.FloatRange*i);
                        var healthDelta = new Vector2((float)v.Health * _drawHelper.FloatRange / v.MaximumHealth, 0);
                        var manaDelta = new Vector2(v.Mana * _drawHelper.FloatRange / v.MaximumMana, 0);
                        if (ShowHealthOnTopPanel)
                        {
                            Drawing.DrawRect(new Vector2(initPos, _drawHelper.BotRange + 1), new Vector2(_drawHelper.FloatRange, _drawHelper.Height),
                                Color.Red);
                            Drawing.DrawRect(new Vector2(initPos, _drawHelper.BotRange + 1), healthDelta + new Vector2(0, _drawHelper.Height),
                                Color.Green);
                            Drawing.DrawRect(new Vector2(initPos, _drawHelper.BotRange + 1), new Vector2(_drawHelper.FloatRange, _drawHelper.Height),
                                Color.Black, true);
                        }
                        if (ShowManaOnTopPanel)
                        {
                            Drawing.DrawRect(new Vector2(initPos, _drawHelper.BotRange + 1 + _drawHelper.Height),
                                new Vector2(_drawHelper.FloatRange, _drawHelper.Height), Color.Gray);
                            Drawing.DrawRect(new Vector2(initPos, _drawHelper.BotRange + 1 + _drawHelper.Height),
                                manaDelta + new Vector2(0, _drawHelper.Height), Color.Blue);
                            Drawing.DrawRect(new Vector2(initPos, _drawHelper.BotRange + 1 + _drawHelper.Height),
                                new Vector2(_drawHelper.FloatRange, _drawHelper.Height),
                                Color.Black, true);
                        }

                        if ( !v.IsVisible|| Equals(v, _me)) continue;
                        Vector2 screenPos;

                        if (!Drawing.WorldToScreen(v.Position, out screenPos))
                            continue;
                        
                        var start = screenPos + new Vector2(-75, 20);
                        var spells = new Ability[7];
                        try { spells[1] = v.Spellbook.Spell1; }
                        catch { }
                        try { spells[2] = v.Spellbook.Spell2; }
                        catch { }
                        try { spells[3] = v.Spellbook.Spell3; }
                        catch { }
                        try { spells[4] = v.Spellbook.Spell4; }
                        catch { }
                        try { spells[5] = v.Spellbook.Spell5; }
                        catch { }
                        try { spells[6] = v.Spellbook.Spell6; }
                        catch { }
                        var counter = 0;
                        for (var g = 6; g >= 1; g--)
                        {
                            if (spells[g]==null) continue;
                            counter++;
                            var cd = spells[g].Cooldown;
                            Drawing.DrawRect(start + new Vector2(g * 20 - 5, 0), new Vector2(20, cd==0?6:20),
                                new ColorBGRA(0, 0, 0, 100), true);
                            if (cd > 0)
                            {
                                var text = string.Format("{0:0.#}", cd);
                                var textSize = Drawing.MeasureText(text, "Arial", new Vector2(10, 200), FontFlags.None);
                                var textPos = (start + new Vector2(g * 20 - 5, 0) + new Vector2(10 - textSize.X / 2, -textSize.Y / 2 + 12));
                                Drawing.DrawText(text, textPos, new Vector2(10, 150), Color.White,
                                    FontFlags.AntiAlias | FontFlags.DropShadow);
                            }
                            if (spells[g].Level==0) continue;
                            for (var lvl = 1; lvl <= spells[g].Level; lvl++)
                            {
                                Drawing.DrawRect(start + new Vector2(g * 20 - 5 + 3 * lvl, 2), new Vector2(2, 2),
                                    new ColorBGRA(255, 255, 0, 255),true);
                            }
                        }
                        if (counter >= 4 && ShowCooldownOnTopPanel && spells[counter].Level>0)
                        {
                            var spellDelta =
                                new Vector2(spells[counter].Cooldown * _drawHelper.FloatRange / spells[counter].CooldownLength, 0);
                            Drawing.DrawRect(new Vector2(initPos, _drawHelper.BotRange + 1 + _drawHelper.Height * 2),
                                new Vector2(_drawHelper.FloatRange, _drawHelper.Height), Color.Gray);
                            Drawing.DrawRect(new Vector2(initPos, _drawHelper.BotRange + 1 + _drawHelper.Height * 2),
                                spellDelta + new Vector2(0, _drawHelper.Height), Color.Yellow);
                            Drawing.DrawRect(new Vector2(initPos, _drawHelper.BotRange + 1 + _drawHelper.Height * 2),
                                new Vector2(_drawHelper.FloatRange, _drawHelper.Height), Color.Black, true);
                            var text = string.Format("{0:0.}", spells[counter].Cooldown);
                            var textSize = Drawing.MeasureText(text, "Arial", new Vector2(10, 200), FontFlags.None);
                            var textPos = (new Vector2(initPos, _drawHelper.BotRange + 1 + _drawHelper.Height*2) +
                                           new Vector2(textSize.X/2 + 5, -textSize.Y/2 + 10));
                            if (ShowCooldownOnTopPanelLikeText)
                            {
                                Drawing.DrawText(text=="0"?"Ready":text, textPos, new Vector2(10, 150), Color.White,
                                    FontFlags.AntiAlias | FontFlags.DropShadow);
                            }
                        }

                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
        }
        #endregion
        #region Init
        private static void Game_OnUpdate(EventArgs args)
        {
            if (!_loaded)
            {
                _me = ObjectMgr.LocalHero;
                _player = ObjectMgr.LocalPlayer;
                if (!Game.IsInGame || _me == null)
                {
                    return;
                }
                _loaded = true;
                PrintSuccess("> OverlayInformation loaded! v"+Ver);
            }
            if (!Game.IsInGame || _me == null)
            {
                _loaded = false;
                PrintInfo("> OverlayInformation unLoaded");
                return;
            }
        }
        #endregion
        #region Helpers

        private static void PrintInfo(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.White, arguments);
        }

        private static void PrintSuccess(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.Green, arguments);
        }

        // ReSharper disable once UnusedMember.Local
        private static void PrintError(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.Red, arguments);
        }

        private static void PrintEncolored(string text, ConsoleColor color, params object[] arguments)
        {
            var clr = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text, arguments);
            Console.ForegroundColor = clr;
        }

        private static bool CheckMouse(float x, float y, float sizeX, float sizeY)
        {
            var mousePos = Game.MouseScreenPosition;
            return mousePos.X >= x && mousePos.X <= x + sizeX && mousePos.Y >= y && mousePos.Y <= y + sizeY;
        }

        #endregion
        #region Drawing Methods
        public static void DrawCircle(int x, int y, int radius, int numSides, int thickness, Color color)
        {
            var vector2S = new Vector2[128];
            var step = (float)Math.PI * 2.0f / numSides;
            var count = 0;
            for (float a = 0; a < (float)Math.PI * 2.0; a += step)
            {
                var x1 = radius * (float)Math.Cos(a) + x;
                var y1 = radius * (float)Math.Sin(a) + y;
                var x2 = radius * (float)Math.Cos(a + step) + x;
                var y2 = radius * (float)Math.Sin(a + step) + y;
                vector2S[count].X = x1;
                vector2S[count].Y = y1;
                vector2S[count + 1].X = x2;
                vector2S[count + 1].Y = y2;

                DrawLine(x1, y1, x2, y2, thickness, color);
                count += 2;
            }
        }
        public static void DrawFilledBox(float x, float y, float w, float h, Color color)
        {
            var vLine = new Vector2[2];

            _line.GLLines = true;
            _line.Antialias = false;
            _line.Width = w;

            vLine[0].X = x + w / 2;
            vLine[0].Y = y;
            vLine[1].X = x + w / 2;
            vLine[1].Y = y + h;

            _line.Begin();
            _line.Draw(vLine, color);
            _line.End();
        }
        public static void DrawLine(float x1, float y1, float x2, float y2, float w, Color color)
        {
            var vLine = new[] { new Vector2(x1, y1), new Vector2(x2, y2) };

            _line.GLLines = true;
            _line.Antialias = false;
            _line.Width = w;

            _line.Begin();
            _line.Draw(vLine, color);
            _line.End();

        }
        public static void DrawShadowText(string stext, int x, int y, Color color, Font f)
        {
            f.DrawText(null, stext, x + 1, y + 1, Color.Black);
            f.DrawText(null, stext, x, y, color);
        }
        public static void DrawBox(float x, float y, float w, float h, float px, Color color)
        {
            DrawFilledBox(x, y + h, w, px, color);
            DrawFilledBox(x - px, y, px, h, color);
            DrawFilledBox(x, y - px, w, px, color);
            DrawFilledBox(x + w, y, px, h, color);
        }
        private static void DrawButton(float x, float y, float w, float h, float px, ref bool clicked, bool isActive, Color @on, Color off, Color @select)
        {
            if (isActive)
            {
                var isIn = CheckMouse(x, y, w, h);
                if (LeftMouseIsPress && Utils.SleepCheck("ClickButtonCd") && isIn)
                {
                    clicked = !clicked;
                    _saveLoadSysHelper.IniWriteValue("Booleans", "Show health on top panel", true.ToString());
                    Utils.Sleep(250, "ClickButtonCd");
                }
                var newColor = isIn
                    ? @select
                    : clicked ? @on : off;
                DrawFilledBox(x, y, w, h, newColor);
                DrawBox(x, y, w, h, px, Color.Black);
            }
            else
            {
                DrawFilledBox(x, y, w, h, Color.Gray);
                DrawBox(x, y, w, h, px, Color.Black);
            }
        }
        /// <summary>
        /// Рисуем кнопку
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="px">ширина</param>
        /// <param name="clicked">булку для тогла</param>
        /// <param name="isActive">активна ли кнопка</param>
        /// <param name="on">цвет при активе</param>
        /// <param name="off">цвет при оффе</param>
        /// <param name="description">описание</param>
        private static void DrawButton(float x, float y, float w, float h, float px, ref bool clicked, bool isActive, Color @on, Color off, string description)
        {
            var isIn = CheckMouse(x, y, w, h);
            if (isActive)
            {
                if (LeftMouseIsPress && Utils.SleepCheck("ClickButtonCd") && isIn)
                {
                    clicked = !clicked;
                    _saveLoadSysHelper.IniWriteValue("Booleans",description,clicked.ToString());
                    Utils.Sleep(250, "ClickButtonCd");
                }
                var newColor = isIn
                    ? new Color((int) (clicked ? @on.R:off.R),clicked ? @on.G:off.G,clicked ? @on.B:off.B,150)
                    : clicked ? @on : off;
                DrawFilledBox(x, y, w, h, newColor);
                DrawBox(x, y, w, h, px, Color.Black);
            }
            else
            {
                DrawFilledBox(x, y, w, h, Color.Gray);
                DrawBox(x, y, w, h, px, Color.Black);
            }
            if (isIn)
            {
                DrawFilledBox(x - description.Length * 6 - 40, y, description.Length * 6 + 35, 20, new Color(0, 0, 0, 100));
                DrawShadowText(description, (int)x - description.Length * 6 - 40, (int)y, Color.White, FontArray[5]);
            }
        }
        #endregion
    }
}
