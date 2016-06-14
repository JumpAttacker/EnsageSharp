using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using SharpDX;
using Color = SharpDX.Color;
using Menu = Ensage.Common.Menu.Menu;
using MenuItem = Ensage.Common.Menu.MenuItem;

namespace BadGuy
{
    internal static class Program
    {
        private static readonly Menu Menu = new Menu("Bad Guy","BG",true);
        //private static bool Enable = false;
        private static Hero MyHero;
        private static Unit _fountain;
        private static Unit _fountain2;
        private static float _stageX;
        private static float _stageY;
        private static Vector2 _drawingStartPos = new Vector2(42, 809);
        private static Vector2 _drawingEndPos = new Vector2(278, 1061);//270
        

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetCursorPos(int x, int y);

        private enum MouseEvent
        {
            MouseeventfLeftdown = 0x02,
            MouseeventfLeftup = 0x04,
        }
        private enum Orders
        {
            BlockOnBace = 0, GoToEnemyFountain = 1, Stashing = 2, MoveToHero = 3
        }
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        private static void Main()
        {
            var herofeeder = new Menu("Hero Feeder", "Hero");
            herofeeder.AddItem(new MenuItem("Hero.Enable", "Enable Hero feeder").SetValue(false)).DontSave();
            herofeeder.AddItem(new MenuItem("Hero.Type", "Feed Type:").SetValue(new StringList(new[] { "attack every creep on midlane", "go to the enemy base" })));
            herofeeder.AddItem(new MenuItem("Hero.Cd", "Order rate").SetValue(new Slider(500, 100, 2000)));

            var courSelection=new Menu("Courier Selection","cour");
            courSelection.AddItem(new MenuItem("Courier.Blocking.Enable", "Enable Selected Order").SetValue(false)).DontSave();
            courSelection.AddItem(new MenuItem("Courier.Cd", "Rate").SetValue(new Slider(50, 5, 200)));
            courSelection.AddItem(new MenuItem("Courier.MaxRange", "Max Range").SetValue(new Slider(500, 0, 2000)));
            courSelection.AddItem(
                new MenuItem("Courier.Order", "Courier Order:").SetValue(
                    new StringList(new[]
                    {"blocking on base", "go to the enemy base", "move items to stash", "give items to main hero"})));

            var laugh = new Menu("laugh Selection", "laugh");
            laugh.AddItem(new MenuItem("laugh.Enable", "Enable laugh").SetValue(false));
            laugh.AddItem(new MenuItem("laugh.Cd", "Rate").SetValue(new Slider(20, 20))).SetTooltip("in secs");
            laugh.AddItem(new MenuItem("laugh.Message", "Print Message on laught").SetValue(false).SetTooltip("only for you"));

            var drawing = new Menu("Spam Drawing", "spamDrawing");
            drawing.AddItem(new MenuItem("Drawing.Fully.Enable", "Fully Enable").SetValue(false));
            drawing.AddItem(new MenuItem("Drawing.info", "Working only with ctrl hotkey").SetFontColor(Color.Red));
            drawing.AddItem(new MenuItem("Drawing.Enable", "Enable Spam Drawing").SetValue(new KeyBind(0x11, KeyBindType.Press)).SetTooltip("on minimap").DontSave()).ValueChanged += Program_ValueChanged;
                
                
            drawing.AddItem(new MenuItem("Drawing.Speed", "Speed").SetValue(new Slider(1, 1, 10)));
            drawing.AddItem(new MenuItem("Drawing.Cd", "Rate").SetValue(new Slider(1, 1, 1000)));


            Menu.AddSubMenu(herofeeder);
            Menu.AddSubMenu(courSelection);
            Menu.AddSubMenu(laugh);
            Menu.AddSubMenu(drawing);
            Menu.AddToMainMenu();

            Events.OnLoad += (sender, args) =>
            {
                MyHero = ObjectManager.LocalHero;
                Game.OnUpdate+=Game_OnUpdate;
                Game.PrintMessage(
                    "<font face='Comic Sans MS, cursive'><font color='#00aaff'>" + Menu.DisplayName +
                    " By Jumpering" +
                    " loaded!</font> <font color='#aa0000'>v" + Assembly.GetExecutingAssembly().GetName().Version,
                    MessageType.LogMessage);
                _fountain = null;
                _fountain2 = null;
            };
            Events.OnClose += (sender, args) =>
            {
                Game.OnUpdate -= Game_OnUpdate;
            };
        }
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        private static void Program_ValueChanged(object sender, OnValueChangeEventArgs args)
        {

            if (!Menu.Item("Drawing.Fully.Enable").GetValue<bool>()) return;
            if (args.GetNewValue<KeyBind>().Active)
            {
                //Enable = true;
                _stageX = _drawingStartPos.X;
                _stageY = _drawingStartPos.Y;
                SetCursorPos((int) _drawingStartPos.X, (int) _drawingStartPos.Y);
            }
            else
            {
                //Enable = false;
            }
            mouse_event(
                args.GetNewValue<KeyBind>().Active
                    ? (int) MouseEvent.MouseeventfLeftdown
                    : (int) MouseEvent.MouseeventfLeftup, 0, 0, 0, 0);
        }

        private static bool _forward = true;

        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        private static void Game_OnUpdate(EventArgs args)
        {
            if (Utils.SleepCheck("Hero.Cd") && Menu.Item("Hero.Enable").GetValue<bool>())
            {
                if (_fountain2 == null || !_fountain2.IsValid)
                {
                    _fountain2 = ObjectManager.GetEntities<Unit>()
                        .FirstOrDefault(x => x.Team != MyHero.Team && x.ClassID == ClassID.CDOTA_Unit_Fountain);
                }
                var angle = (float)Math.Max(
                        Math.Abs(MyHero.RotationRad -
                                 Utils.DegreeToRadian(MyHero.FindAngleBetween(_fountain2.Position))) - 0.20, 0);
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (angle > 0)
                {
                    if (Menu.Item("Hero.Type").GetValue<StringList>().SelectedIndex == 0)
                    {
                        MyHero.Attack(_fountain2.Position);
                    }
                    else
                    {
                        MyHero.Move(_fountain2.Position);
                    }
                    
                }
                Utils.Sleep(Menu.Item("Hero.Cd").GetValue<Slider>().Value, "Hero.Cd");
            }

            if (Utils.SleepCheck("Courier.Cd") && Menu.Item("Courier.Blocking.Enable").GetValue<bool>())
            {
                var couriers = ObjectManager.GetEntities<Courier>().Where(x => x.IsAlive && x.Team == MyHero.Team);
                if (_fountain == null || !_fountain.IsValid)
                {
                    _fountain = ObjectManager.GetEntities<Unit>()
                        .FirstOrDefault(x => x.Team == MyHero.Team && x.ClassID == ClassID.CDOTA_Unit_Fountain);
                }
                if (_fountain2 == null || !_fountain2.IsValid)
                {
                    _fountain2 = ObjectManager.GetEntities<Unit>()
                        .FirstOrDefault(x => x.Team != MyHero.Team && x.ClassID == ClassID.CDOTA_Unit_Fountain);
                }
                var index = Menu.Item("Courier.Order").GetValue<StringList>().SelectedIndex;
                var needFountain = index == (int) Orders.BlockOnBace
                    ? _fountain
                    : _fountain2;
                if ((index == 0 || index==1) && needFountain!=null)
                    foreach (var courier in from courier in couriers.Where(
                        courier =>
                            courier.Distance2D(needFountain) >
                            Menu.Item("Courier.MaxRange").GetValue<Slider>().Value) let angle = (float) Math.Max(
                                Math.Abs(courier.RotationRad -
                                         Utils.DegreeToRadian(courier.FindAngleBetween(needFountain.Position))) - 0.20, 0) where angle != 0 select courier)
                    {
                        courier.Move(needFountain.Position);
                        Utils.Sleep(Menu.Item("Courier.Cd").GetValue<Slider>().Value, "Courier.Cd");
                    }
                else if (index >= 2)
                {
                    foreach (var courier in couriers)
                    {
                        if (index == (int) Orders.Stashing)
                        {
                            //Game.PrintMessage("Any items: " + courier.Inventory.Items.Any(x => Equals(x.Owner, MyHero)),MessageType.ChatMessage);
                            //if (courier.Inventory.Items.Any(x => Equals(x.Owner, MyHero)))
                            courier.Spellbook.SpellE.UseAbility();
                        }
                        else
                        {
                            /*foreach (var item in courier.Inventory.Items)
                            {
                                Game.PrintMessage("Item: " + item.Name+" .Owner: "+item.,MessageType.ChatMessage);
                            }*/
                            //Game.PrintMessage("Any items: " + courier.Inventory.Items.Any(x => Equals(x.Owner, MyHero)), MessageType.ChatMessage);
                            if (courier.Inventory.Items.Any(/*x => Equals(x.Owner, MyHero)*/))
                                courier.Spellbook.SpellF.UseAbility();
                        }
                        Utils.Sleep(Menu.Item("Courier.Cd").GetValue<Slider>().Value, "Courier.Cd");
                    }
                }
            }
            if (Utils.SleepCheck("laugh.Cd") && Menu.Item("laugh.Enable").GetValue<bool>())
            {
                Game.ExecuteCommand("say \"/laugh \"");
                Utils.Sleep(Menu.Item("laugh.Cd").GetValue<Slider>().Value*1000, "laugh.Cd");
                if (Menu.Item("laugh.Message").GetValue<bool>())
                {
                    Game.PrintMessage("Laugh!", MessageType.ChatMessage,MyHero.Player);
                }
            }
            if (!Menu.Item("Drawing.Fully.Enable").GetValue<bool>()) return;
            //Print(Menu.Item("Drawing.Enable").GetValue<KeyBind>().Active.ToString());
            if (!Utils.SleepCheck("Drawing.Cd") || !Menu.Item("Drawing.Enable").GetValue<KeyBind>().Active) return;
            if (_forward)
            {
                SetCursorPos((int) _stageX, (int) _stageY);
                //mouse_event((int) MouseEvent.MouseeventfLeftdown, 0, 0, 0, 0);
                _stageY += Menu.Item("Drawing.Speed").GetValue<Slider>().Value;
            }
            else
            {
                SetCursorPos((int) _drawingEndPos.X, (int) _stageY);
            }
            if (_stageY > _drawingEndPos.Y)
                _stageY = _drawingStartPos.Y;
            _forward = !_forward;
            Utils.Sleep(Menu.Item("Drawing.Cd").GetValue<Slider>().Value, "Drawing.Cd");
        }

        private static void Print(string s)
        {
            Game.PrintMessage(s,MessageType.ChatMessage);
        }
    }
}
