using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Ensage;
using Ensage.Common;
using Ensage.Common.Menu;
using Ensage.Items;
using SharpDX;

namespace ItemPanel
{
    internal class Program
    {
        #region Members
        //============================================================
        private static bool _loaded;
        private static readonly string Ver = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        private static readonly Hero[] Heroes=new Hero[10];
        private static bool _isMoving;
        private static readonly float Percent = HUDInfo.RatioPercentage();
        //============================================================
        private static readonly Menu Menu = new Menu("Item Panel", "itempanel", true);
        //============================================================
        #endregion

        private static void Main()
        {
            _loaded = false;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnWndProc += Game_OnWndProc;
            Menu.AddItem(new MenuItem("show", "Show Item Panel").SetValue(new KeyBind('I', KeyBindType.Toggle, true)));
            Menu.AddItem(new MenuItem("showNumber", "Show cooldown (number)").SetValue(true));
            Menu.AddItem(new MenuItem("showRect", "Show cooldown (rect)").SetValue(true));
            var settings = new Menu("settings", "Settings");
            settings.AddItem(new MenuItem("DisableMoving", "Disable moving").SetValue(false));
            settings.AddItem(new MenuItem("posX", "Panel Position X").SetValue(new Slider(100,-2000,2000)));
            settings.AddItem(new MenuItem("posY", "Panel Position Y").SetValue(new Slider(200, -2000, 2000)));
            settings.AddItem(new MenuItem("trans", "Cooldown Transparency").SetValue(new Slider(100, 1, 255)));
            settings.AddItem(new MenuItem("fontSize", "Cooldown Font Size").SetValue(new Slider(15, 1, 20)));
            Menu.AddSubMenu(settings);


            Menu.AddToMainMenu();
        }
        private static void Game_OnWndProc(WndEventArgs args)
        {

            if (args.WParam != 1 || Game.IsChatOpen || Menu.Item("DisableMoving").GetValue<bool>())
            {
                return;
            }
            var size = new Vector2(200 * Percent, 150 * Percent);
            if (Utils.IsUnderRectangle(Game.MouseScreenPosition, Menu.Item("posX").GetValue<Slider>().Value,
                Menu.Item("posY").GetValue<Slider>().Value, size.X, size.Y/6))
                _isMoving = !_isMoving;
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
                _loaded = true;
                PrintSuccess(string.Format("> {1} Loaded v{0}", Ver, Menu.DisplayName));
                Game.PrintMessage(
                    "<font face='Comic Sans MS, cursive'><font color='#00aaff'>" + Menu.DisplayName + " By Jumpering" +
                    " loaded!</font> <font color='#aa0000'>v" + Ver, MessageType.LogMessage);
                Flush();
            }

            if (!Game.IsInGame || me == null)
            {
                _loaded = false;
                PrintInfo(string.Format("> {0} unLoaded", Menu.DisplayName));
                return;
            }
            if (!Menu.Item("show").GetValue<KeyBind>().Active) return;
            Utils.Sleep(100, "perf");
            var pos = new Vector2(Menu.Item("posX").GetValue<Slider>().Value, Menu.Item("posY").GetValue<Slider>().Value);//new Vector2(Menu.Item("posX").GetValue<Slider>().Value, Menu.Item("posY").GetValue<Slider>().Value);
            var size = new Vector2(200 * Percent, 150 * Percent);
                
            if (_isMoving)
            {
                Menu.Item("posX").SetValue(new Slider((int)Game.MouseScreenPosition.X-10, -2000, 2000));
                Menu.Item("posY").SetValue(new Slider((int)Game.MouseScreenPosition.Y-10, -2000, 2000));
                Drawing.DrawText("MOVING", pos + new Vector2(2, 2), new Vector2(20 * Percent, 0),
                    Color.LightBlue, FontFlags.AntiAlias | FontFlags.DropShadow);
            }
            else
            {
                Drawing.DrawText(Menu.DisplayName + " v " + Ver, pos + new Vector2(2, 2), new Vector2(20*Percent, 0),
                    Color.LightBlue, FontFlags.AntiAlias | FontFlags.DropShadow);
            }
            Drawing.DrawRect(pos, size, new Color(0, 0, 0, 100));
            Drawing.DrawRect(pos, size, new Color(0, 155, 255, 255), true);
            Drawing.DrawLine(pos + new Vector2(0, size.Y / 6), pos + new Vector2(size.X, size.Y / 6),
                new Color(0, 155, 255, 255));
            
            DrawItems(me, pos, size, Percent);
            
            
        }

        private static void DrawItems(Hero me, Vector2 pos, Vector2 size, float percent)
        {
            uint i = 0;
            for (uint num = 0; num < 10; num++)
            {
                try
                {
                    if (Heroes[num] == null || !Heroes[num].IsValid)
                        Heroes[num] = ObjectMgr.GetPlayerById(num).Hero;
                }
                catch
                {
                    continue;
                }
                if (Heroes[num]==null || !Heroes[num].IsValid || Heroes[num].Team == me.Team) continue;
                Drawing.DrawRect(pos + new Vector2(10, size.Y/7 + 10 + i*(size.Y/10 + 5)*percent),
                    new Vector2(size.X/7, size.Y/8),
                    Drawing.GetTexture("materials/ensage_ui/heroes_horizontal/" +
                                       Heroes[num].Name.Substring("npc_dota_hero_".Length) + ".vmat"));

                Drawing.DrawRect(pos + new Vector2(10, size.Y/7 + 10 + i*(size.Y/10 + 5)*percent),
                    new Vector2(size.X/7, size.Y/8), new Color(0, 0, 0, 255), true);
                for (var i2 = 1; i2 <= 6; i2++)
                {
                    try
                    {
                        string texturename;
                        var item = Heroes[num].Inventory.GetItem((ItemSlot)i2 - 1);
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
                        if (item is Bottle)
                        {
                            var bottletype = item as Bottle;
                            if (bottletype.StoredRune != RuneType.None)
                            {
                                texturename = string.Format("materials/ensage_ui/items/{0}.vmat",
                                    item.Name.Replace("item_", "") + "_" + bottletype.StoredRune);
                            }
                        }
                        if (item == null) continue;
                        var itemStartPos = pos +
                                           new Vector2((size.X / 7) + (size.X / 9) * i2, size.Y / 7 + 10 + i * (size.Y / 10 + 5) * percent);
                        Drawing.DrawRect(
                            itemStartPos,
                            new Vector2(size.X/7, size.Y/8),
                            Drawing.GetTexture(texturename));
                        
                        if (item.AbilityState == AbilityState.NotEnoughMana)
                        {
                            Drawing.DrawRect(itemStartPos,
                            new Vector2(size.X / 9, size.Y / 8), new Color(0, 0, 255, 75));
                        }

                        if (item.AbilityState == AbilityState.OnCooldown)
                        {
                            if (Menu.Item("showRect").GetValue<bool>())
                                Drawing.DrawRect(itemStartPos,
                                    new Vector2(size.X/9, item.Cooldown/item.CooldownLength*(size.Y/8)),
                                    new Color(255, 255, 255, Menu.Item("trans").GetValue<Slider>().Value));
                            if (Menu.Item("showNumber").GetValue<bool>())
                                Drawing.DrawText(((int) item.Cooldown).ToString(CultureInfo.InvariantCulture),
                                    itemStartPos, new Vector2(Menu.Item("fontSize").GetValue<Slider>().Value*percent),
                                    Color.Gold,
                                    FontFlags.AntiAlias | FontFlags.DropShadow);
                        }
                    }
                    catch (Exception)
                    {
                        /*Drawing.DrawRect(_startPos + new Vector2(25 * Con * i2, 18 * Con * (i + 1)), new Vector2(32 * Con, 18 * Con),
                        Drawing.GetTexture("materials/ensage_ui/items/emptyitembg.vmat"));*/
                    }
                }
                i++;
            }
        }

        private static void Flush()
        {
            for (uint i = 0; i < 10; i++)
                Heroes[i] = null;
        }
        

        #region Helpers
        
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