using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Extensions.Damage;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using Ensage.Common.Objects.UtilityObjects;
using Ensage.Heroes;
using log4net;
using PlaySharp.Toolkit.Logging;
using SharpDX;

namespace MeepoAnnihilation
{
    internal static class Program
    {
        #region Members

        private static readonly Menu Menu = new Menu("Meepo Annihilation", "meeporez", true, "npc_dota_hero_meepo", true);
            /*new Menu("Aniihilation", "meepo", true,"npc_dota_hero_meepo");*/

        private static readonly Dictionary<Vector3, string> LaneDictionary = new Dictionary<Vector3, string>()
        {
            {new Vector3(-6080, 5805, 384), "top"},
            {new Vector3(-6600, -3000, 384), "top"},
            {new Vector3(2700, 5600, 384), "top"},


            {new Vector3(5807, -5785, 384), "bot"},
            {new Vector3(-3200, -6200, 384), "bot"},
            {new Vector3(6200, 2200, 384), "bot"},


            {new Vector3(-600, -300, 384), "middle"},
            {new Vector3(3600, 3200, 384), "middle"},
            {new Vector3(-4400, -3900, 384), "middle"}

        };
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static Hero MyHero { get; set; }
        private static Player MyPlayer { get; set; }
        private static Hero _globalTarget;
        private static Item _blink, _meka, _aghainim, _hex, _orchid, _eb;
        private static Ability _ultimate;
        private static List<Meepo> _meepoList = new List<Meepo>();
        public static readonly Dictionary<uint, OrderState> OrderStates = new Dictionary<uint, OrderState>();
        public static readonly Dictionary<uint, OrderState> LastOrderStates = new Dictionary<uint, OrderState>();
        private static readonly Dictionary<uint, ParticleEffect> Effects = new Dictionary<uint, ParticleEffect>();

        public enum OrderState
        {
            Idle,
            Jungle,
            Stacking,
            Laning,
            Escape,
            InCombo
        }



        private static Vector2 IconPos
            => new Vector2(Menu.Item("Drawing.posX").GetValue<Slider>().Value, Menu.Item("Drawing.posY").GetValue<Slider>().Value);

        private static int IconSize => Menu.Item("Drawing.Size").GetValue<Slider>().Value;
        private static readonly bool[] MenuIsOpen = new bool[10];
        private static int _selectedId;
        private static List<Entity> _selectedMeepo = new List<Entity>();
        public static readonly Dictionary<uint, Ability> SpellQ = new Dictionary<uint, Ability>();
        public static readonly Dictionary<uint, Ability> SpellW = new Dictionary<uint, Ability>();
        private static bool ThrowingNet => Menu.Item("hotkey.ThrowNet").GetValue<KeyBind>().Active;
        private static bool isNetEnable = false;
        #endregion

        #region Main

        private static bool _firstTime = true;
        private static void Main()
        {
            Events.OnLoad += (sender, args) =>
            {
                InitMenu();
                MyHero = ObjectManager.LocalHero;
                if (MyHero.ClassId!=ClassId.CDOTA_Unit_Hero_Meepo) return;
                Game.PrintMessage(
                    "<font face='Comic Sans MS, cursive'><font color='#00aaff'>" + Menu.DisplayName + " By Jumpering" +
                    " loaded!</font> <font color='#aa0000'>v" + Assembly.GetExecutingAssembly().GetName().Version);
                Game.OnUpdate += Game_OnUpdate;
                Game.OnUpdate += Camp_update;
                Drawing.OnDraw += Drawing_OnDraw;
                Game.OnWndProc += Game_OnWndProc;
                Player.OnExecuteOrder += Player_OnExecuteAction;
                Orbwalking.Load();
                _meepoList.Clear();
                _aghainim = null;
                _blink = null;
                _meka = null;
                _ultimate = null;
                JungleCamps.Init();
                foreach (var camp in JungleCamps.GetCamps)
                {
                    camp.CanBeHere = true;
                }
                _selectedMeepo.Clear();
                _meepoList.Clear();
                MeepoSet.Clear();
                ScaleX = Drawing.Width / 1920f;
                ScaleY = Drawing.Height / 1080f;
            };
            Events.OnClose += (sender, args) =>
            {
                Game.OnUpdate -= Game_OnUpdate;
                Game.OnUpdate -= Camp_update;
                Drawing.OnDraw -= Drawing_OnDraw;
                Game.OnWndProc -= Game_OnWndProc;
                Player.OnExecuteOrder -= Player_OnExecuteAction;
                MyHero = null;
            };
        }

        public static float ScaleY { get; set; }

        public static float ScaleX { get; set; }

        public static void InitMenu()
        {
            if (!_firstTime)
                return;
            _firstTime = false;
            Menu.AddItem(new MenuItem("Enable", "Enable").SetValue(true));
            Menu.AddItem(new MenuItem("hotkey", "Hotkey").SetValue(new KeyBind('G', KeyBindType.Press)));
            Menu.AddItem(
                new MenuItem("hotkey.PoofAll", "Poof all to selected meepo").SetValue(new KeyBind('Q', KeyBindType.Press)));
            Menu.AddItem(new MenuItem("LockTarget", "LockTarget").SetValue(true));
            Menu.AddItem(
                new MenuItem("hotkey.Escape", "Escape for selected Meepo(s)").SetValue(new KeyBind('V',
                    KeyBindType.Press)));
            Menu.AddItem(
                new MenuItem("hotkey.ThrowNet", "Throw net").SetValue(new KeyBind('D',
                    KeyBindType.Press))).ValueChanged += (sender, args) =>
                    {
                        var newValue = args.GetNewValue<KeyBind>().Active;
                        var oldValue = args.GetOldValue<KeyBind>().Active;
                        if (oldValue != newValue)
                            isNetEnable = newValue;
                    };
            Menu.AddItem(new MenuItem("Escape.MinRange", "Min health for autoheal").SetValue(new Slider(300, 0, 4000)));
            Menu.AddItem(
                new MenuItem("Escape.MinRangePercent", "Min health for autoheal (%)").SetValue(new Slider(15, 0, 100)));
            Menu.AddItem(
                new MenuItem("Drawing.PoffSystem", "Poofer").SetValue(true)
                    .SetTooltip("All selected meepos will use W spell on target position when first meepo use W spell"));
            var drawingMenu = new Menu("Drawing", "Drawing");

            drawingMenu.AddItem(new MenuItem("Drawing.DamageFromPoof", "Draw Poof count on enemy").SetValue(true));
            drawingMenu.AddItem(new MenuItem("Drawing.NumOfMeepo", "Draw Number for each meepo").SetValue(true));
            drawingMenu.AddItem(
                new MenuItem("Drawing.NumOfMeepoOnMinimap", "Draw Number for each meepo on minimap").SetValue(true));
            drawingMenu.AddItem(
                new MenuItem("Drawing.NumOfMeepoInMenu", "Draw Number for each meepo in OverlayMenu").SetValue(true));
            drawingMenu.AddItem(
                new MenuItem("Drawing.posX", "OverlayMenu pos X").SetValue(new Slider(120, 0,
                    500)));
            drawingMenu.AddItem(
                new MenuItem("Drawing.posY", "OverlayMenu pos Y").SetValue(new Slider(95, 0,
                    500)));
            drawingMenu.AddItem(
                new MenuItem("Drawing.Size", "OverlayMenu size").SetValue(new Slider(75, 0,
                    500)));
            drawingMenu.AddItem(new MenuItem("Drawing.PoofState", "Draw poof state in OverlayMenu").SetValue(true));

            var autoPush = new Menu("Auto Push", "AutoPush");
            autoPush.AddItem(
                new MenuItem("AutoPush.Enable", "Push Lane By Selected Meepo(s)").SetValue(new KeyBind('Z',
                    KeyBindType.Press)));
            autoPush.AddItem(
                new MenuItem("AutoPush.EscapeFromAnyEnemyHero", "Escape From Enemy Hero in selected range").SetValue(
                    true));
            autoPush.AddItem(new MenuItem("AutoPush.EscapeRange", "Range For Escape").SetValue(new Slider(1500, 0, 5000)));
            autoPush.AddItem(new MenuItem("AutoPush.AutoW", "use w for lasthitting").SetValue(true));
            autoPush.AddItem(new MenuItem("AutoPush.TravelBoots", "use TravelBoots").SetValue(false));
            autoPush.AddItem(new MenuItem("AutoPush.LastHitMode", "Try to last hit creeps").SetValue(true));

            var jumgleFarm = new Menu("Jungle Farm", "JungleFarm");
            jumgleFarm.AddItem(
                new MenuItem("JungleFarm.Enable", "Jungle Farm By Selected Meepo(s)").SetValue(new KeyBind('X',
                    KeyBindType.Press)));
            jumgleFarm.AddItem(
                new MenuItem("JungleFarm.EscapeFromAnyEnemyHero", "Escape From Enemy Hero in selected range").SetValue(
                    true));
            jumgleFarm.AddItem(
                new MenuItem("JungleFarm.EscapeRange", "Range For Escape").SetValue(new Slider(1500, 0, 5000)));
            jumgleFarm.AddItem(new MenuItem("JungleFarm.AutoW", "use w for farming").SetValue(true));
            jumgleFarm.AddItem(new MenuItem("JungleFarm.TeamCheck", "Farm enemy jungle too").SetValue(false));
            jumgleFarm.AddItem(new MenuItem("JungleFarm.Ancient", "Farm ancients too").SetValue(false));

            var jungleStack = new Menu("Jungle Stack", "JungleStack");
            jungleStack.AddItem(
                new MenuItem("JungleStack.Enable", "Jungle Stacking By Selected Meepo(s)").SetValue(new KeyBind('C',
                    KeyBindType.Press)));
            jungleStack.AddItem(
                new MenuItem("JungleStack.EscapeFromAnyEnemyHero", "Escape From Enemy Hero in selected range").SetValue(
                    true));
            jungleStack.AddItem(
                new MenuItem("JungleStack.EscapeRange", "Range For Escape").SetValue(new Slider(1500, 0, 5000)));
            jungleStack.AddItem(new MenuItem("JungleStack.TeamCheck", "Stack enemy jungle too").SetValue(false));
            jungleStack.AddItem(new MenuItem("JungleStack.Ancient", "Stack ancients too").SetValue(false));

            Menu.AddSubMenu(autoPush);
            Menu.AddSubMenu(jumgleFarm);
            Menu.AddSubMenu(jungleStack);
            Menu.AddSubMenu(drawingMenu);
            Menu.AddToMainMenu();
        }

        #endregion

        #region ActionsWithLocalPlayer

        private static void Player_OnExecuteAction(Player sender, ExecuteOrderEventArgs args)
        {
            
            if (!Menu.Item("Enable").GetValue<bool>()) return;
            if (MyHero == null || !MyHero.IsValid || !MyHero.IsAlive) return;
            var me = sender.Selection.First();
            var order = args.OrderId;
            if (order == OrderId.Hold || order == OrderId.MoveLocation)
            {
                foreach (
                    var me2 in
                        args.Entities.Select(entity => MeepoSet.Find(x => x.Handle == entity.Handle))
                            .Where(me2 => me2 != null)
                            .Where(me2 => me2.CurrentOrderState != OrderState.Escape))
                {
                    OrderStates[me2.Handle] = OrderState.Idle;
                }
            }
            else if (Menu.Item("Drawing.PoffSystem").GetValue<bool>() &&
                (args.OrderId == OrderId.AbilityLocation || args.OrderId == OrderId.AbilityTarget) &&
                args.Ability.StoredName() == SpellW[MyHero.Handle].Name)
            {
                var pos = args.TargetPosition; 
                foreach (var meepo in _selectedMeepo.Where(x => !Equals(x, me)))
                {
                    var handle = meepo.Handle;
                    var spell = SpellW[handle];
                    if (spell.CanBeCasted())
                    {
                        //spell.UseAbility(meepo.Position);
                        spell.UseAbility(pos);
                    }
                }
            }
        }

        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (!Menu.Item("Enable").GetValue<bool>()) return;
            if (MyHero == null || !MyHero.IsValid || !MyHero.IsAlive) return;
            if (args.WParam != 1 || Game.IsChatOpen)
            {
                _leftMouseIsPress = false;
                return;
            }
            _leftMouseIsPress = true;
        }

        #endregion

        #region Update & Draw

        private static void Drawing_OnDraw(EventArgs args)
        {
            /*var ss = SpellW[MyHero.Handle];
            var s = string.Format("materials/ensage_ui/spellicons/{0}.vmat", ss.StoredName());
            var pos = WorldToMinimap(MyHero.Position);
            Drawing.DrawRect(
                pos,
                new Vector2(20, 20),
                Textures.GetTexture(s));*/
            if (!Menu.Item("Enable").GetValue<bool>()) return;

            try
            {
                if (Menu.Item("Drawing.DamageFromPoof").GetValue<bool>() && SpellW[MyHero.Handle] != null)
                {
                    var poof = SpellW[MyHero.Handle];
                    foreach (
                        var hero in
                            Heroes.GetByTeam(MyHero.GetEnemyTeam()).Where(x => x.IsValid && x.IsAlive && x.IsVisible))
                    {
                        var w2SPos = HUDInfo.GetHPbarPosition(hero);
                        if (w2SPos.X > 0 && w2SPos.Y > 0)
                        {
                            var sizeX = HUDInfo.GetHPBarSizeX();
                            var sizeY = HUDInfo.GetHpBarSizeY();
                            var handle = hero.Handle;
                            var damagePerPoof = Calculations.DamageTaken(hero,
                                poof.GetDamage(poof.Level), DamageType.Magical, MyHero);
                            var minCounter = (int) (hero.Health/damagePerPoof);
                            var count = ((minCounter == int.MinValue) ? "Invul" : minCounter.ToString());
                            var textSize = Drawing.MeasureText(count, "Arial",
                                new Vector2((float) (sizeY*1.5), 500), FontFlags.AntiAlias);
                            var textPos = w2SPos - new Vector2(textSize.X + 5, (float) ((sizeY*1.5) - (textSize.Y)));
                            Drawing.DrawText(
                                count,
                                textPos,
                                new Vector2((float) (sizeY*1.5), 100),
                                Color.White,
                                FontFlags.AntiAlias | FontFlags.StrikeOut);
                            var texturename = $"materials/ensage_ui/spellicons/{poof.StoredName()}.vmat";
                            var iconPos = textPos - new Vector2(sizeY*2 + 5, 0);
                            Drawing.DrawRect(
                                iconPos,
                                new Vector2(sizeY*2, sizeY*2),
                                Textures.GetTexture(texturename));
                        }
                    }
                }
            }
            catch
            {
                // ignored
            }

            if (true)
            {
                foreach (var meepo in MeepoSet/*.OrderBy(x=>x.Id)*/)
                {
                    var handle = meepo.Handle;
                    /*Drawing.DrawRect(IconPos + new Vector2(0, IconSize)*count++, new Vector2(20, 50),
                        new Color(0, 155, 255, 155));
                    Drawing.DrawText(OrderStates[handle].ToString(), IconPos + new Vector2(5, IconSize) * (count-1), 
                    new Vector2(20, 5), Color.Red, FontFlags.AntiAlias | FontFlags.Additive | FontFlags.Custom);*/
                    
                    if (Menu.Item("Drawing.NumOfMeepoInMenu").GetValue<bool>())
                    {
                        var sizeY = HUDInfo.GetHpBarSizeY();
                        var pos = IconPos + new Vector2(0, IconSize)* meepo.Id;
                        var textSize = Drawing.MeasureText((meepo.Id+1).ToString(CultureInfo.InvariantCulture), "Arial",
                            new Vector2((float) (sizeY*3), 100), FontFlags.AntiAlias);
                        var textPos = pos - new Vector2(sizeY / 2 + textSize.Y, 0);
                        Drawing.DrawText(
                            (meepo.Id+1).ToString(CultureInfo.InvariantCulture),
                            textPos - new Vector2(72, 0),
                            new Vector2((float) (sizeY*3), 100),
                            Color.White,
                            FontFlags.AntiAlias | FontFlags.StrikeOut);
                    }
                    DrawButton(IconPos + new Vector2(0, IconSize)* meepo.Id, 70, 30, ref MenuIsOpen[meepo.Id],
                        new Color(0, 155, 255, 150), new Color(0, 0, 0, 100), OrderStates[handle].ToString(),
                        _selectedMeepo.Contains(meepo.Hero));
                    if (Menu.Item("Drawing.PoofState").GetValue<bool>())
                    {
                        var w = meepo.SpellW;
                        if (w.IsInAbilityPhase)
                        {
                            var delta = (float) ((Game.RawGameTime - meepo.PoofStartTime)*70/1.5);
                            Drawing.DrawRect(IconPos + new Vector2(0, 32 + (IconSize) * meepo.Id), new Vector2(delta, 10),
                                Color.White);
                            Drawing.DrawRect(IconPos + new Vector2(0, 32 + (IconSize) * meepo.Id), new Vector2(70, 10),
                                new Color(0,0,0,100));
                            Drawing.DrawRect(IconPos + new Vector2(0, 32 + (IconSize) * meepo.Id), new Vector2(70, 10),
                                Color.Black, true);
                        }
                        else
                        {
                            var state = w.AbilityState;
                            var clr = state == AbilityState.NotEnoughMana
                                ? new Color(0, 155, 255, 120)
                                : state == AbilityState.Ready
                                    ? new Color(0, 0, 0, 0)
                                    : new Color(255, 50, 50, 120);
                            Drawing.DrawRect(IconPos + new Vector2(0, 32 + (IconSize)*meepo.Id), new Vector2(20, 20),
                                Textures.GetSpellTexture(w.StoredName()));
                            Drawing.DrawRect(IconPos + new Vector2(0, 32 + (IconSize)*meepo.Id), new Vector2(20, 20),
                                clr);
                            Drawing.DrawRect(IconPos + new Vector2(0, 32 + (IconSize)*meepo.Id), new Vector2(20, 20),
                                Color.Black, true);

                        }
                    }
                    if (MenuIsOpen[meepo.Id])
                    {
                        _selectedId = 0;
                        DrawButton(IconPos + new Vector2(0, IconSize)* meepo.Id + new Vector2(70, 0), 70, 30, ref _selectedId,
                            1,
                            new Color(0, 0, 0, 100),
                            OrderState.Idle.ToString());
                        DrawButton(IconPos + new Vector2(0, IconSize)* meepo.Id + new Vector2(70, 30), 70, 30,
                            ref _selectedId, 2,
                            new Color(0, 0, 0, 100),
                            OrderState.Jungle.ToString());
                        DrawButton(IconPos + new Vector2(0, IconSize)* meepo.Id + new Vector2(140, 0), 70, 30,
                            ref _selectedId, 3,
                            new Color(0, 0, 0, 100),
                            OrderState.Stacking.ToString());
                        DrawButton(IconPos + new Vector2(0, IconSize)* meepo.Id + new Vector2(140, 30), 70, 30,
                            ref _selectedId, 4,
                            new Color(0, 0, 0, 100),
                            OrderState.Laning.ToString());
                        DrawButton(IconPos + new Vector2(0, IconSize)* meepo.Id + new Vector2(71 + 70/2, 60), 70, 30,
                            ref _selectedId, 5,
                            new Color(0, 0, 0, 100),
                            OrderState.Escape.ToString());
                        if (_selectedId != 0)
                        {
                            OrderStates[handle] = (OrderState) _selectedId - 1;
                            MenuIsOpen[meepo.Id] = false;
                            if (OrderStates[handle] == OrderState.Escape)
                            {
                                NeedHeal[handle] = true;
                            }
                        }
                    }
                    if (Menu.Item("Drawing.NumOfMeepo").GetValue<bool>())
                    {
                        var w2SPos = HUDInfo.GetHPbarPosition(meepo.Hero);
                        if (w2SPos.X > 0 && w2SPos.Y > 0)
                        {
                            var sizeX = HUDInfo.GetHPBarSizeX();
                            var sizeY = HUDInfo.GetHpBarSizeY();
                            var text = meepo.Id+1;
                            var textSize = Drawing.MeasureText(text.ToString(CultureInfo.InvariantCulture), "Arial",
                                new Vector2((float)(sizeY * 3), 100), FontFlags.AntiAlias);
                            var textPos = w2SPos + new Vector2(sizeY / 2 + textSize.Y, 0);
                            Drawing.DrawText(
                                text.ToString(CultureInfo.InvariantCulture),
                                textPos + new Vector2(0, -50),
                                new Vector2((float)(sizeY * 3), 100),
                                Color.White,
                                FontFlags.AntiAlias | FontFlags.StrikeOut);
                        }
                    }
                    if (Menu.Item("Drawing.NumOfMeepoOnMinimap").GetValue<bool>())
                    {
                        var w2SPos = meepo.Hero.Position.WorldToMinimap();//WorldToMinimap(meepo.NetworkPosition);
                        var sizeY = HUDInfo.GetHpBarSizeY();
                        var text = meepo.Id+1;
                        Drawing.DrawText(
                            text.ToString(CultureInfo.InvariantCulture),
                            w2SPos + new Vector2(-5, -33),
                            new Vector2((float)(sizeY * 3), 100),
                            Color.White,
                            FontFlags.AntiAlias | FontFlags.StrikeOut);
                    }
                }
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (!Menu.Item("Enable").GetValue<bool>()) return;
            if (MyHero == null || !MyHero.IsValid)
            {
                MyHero = ObjectManager.LocalHero;
                if (MyHero != null)
                    Print("[Informer]: Main Hero was found");
                return;
            }

            if (MyPlayer == null || !MyPlayer.IsValid)
            {
                MyPlayer = ObjectManager.LocalPlayer;
                if (MyPlayer != null)
                    Print("[Informer]: Player was found");
                return;
            }

            if (_aghainim == null || !_aghainim.IsValid)
            {
                _aghainim = MyHero.FindItem("item_ultimate_scepter");
                if (_aghainim != null) Print("[Informer]: aghainim was found");
            }
            if (_blink == null || !_blink.IsValid)
            {
                _blink = MyHero.FindItem("item_blink");
                if (_blink != null) Print("[Informer]: blink was found");
            }
            if (_meka == null || !_meka.IsValid)
            {
                _meka = MyHero.FindItem("item_mekansm");
                if (_meka != null) Print("[Informer]: mekansm was found");
            }
            if (_eb == null || !_eb.IsValid)
            {
                _eb = MyHero.FindItem("item_ethereal_blade");
                if (_eb != null) Print("[Informer]: ethereal_blade was found");
            }
            if (_hex == null || !_hex.IsValid)
            {
                _hex = MyHero.FindItem("item_sheepstick");
                if (_hex != null) Print("[Informer]: hex was found");
            }
            if (_orchid == null || !_orchid.IsValid)
            {
                _orchid = MyHero.FindItem("item_orchid");
                if (_orchid != null) Print("[Informer]: orchid was found");
            }
            if (_ultimate == null || !_ultimate.IsValid)
            {
                _ultimate = MyHero.Spellbook.Spell4;
                if (_ultimate != null) Print("[Informer]: ultimate was found");
            }
            /*if (travelBoots == null || !travelBoots.IsValid)
            {
                travelBoots = MyHero.FindItem("item_travel_boots") ?? MyHero.FindItem("item_travel_boots_2");

                if (travelBoots != null) Print("[Informer]: TravelBoots was found");
            }*/
            if (MyHero == null || !MyHero.IsValid || !MyHero.IsAlive) return;
            if (Menu.Item("AutoPush.Enable").GetValue<KeyBind>().Active && Utils.SleepCheck("button_cd"))
            {
                foreach (var handle in _selectedMeepo.Select(x => x.Handle))
                {
                    if (OrderStates[handle] == OrderState.Laning)
                        OrderStates[handle] = OrderState.Idle;
                    else
                        OrderStates[handle] = OrderState.Laning;
                }
                Utils.Sleep(250, "button_cd");
            }
            if (Menu.Item("JungleFarm.Enable").GetValue<KeyBind>().Active && Utils.SleepCheck("button_cd"))
            {
                foreach (var handle in _selectedMeepo.Select(x => x.Handle))
                {
                    if (OrderStates[handle] == OrderState.Jungle)
                        OrderStates[handle] = OrderState.Idle;
                    else
                        OrderStates[handle] = OrderState.Jungle;
                }
                Utils.Sleep(250, "button_cd");
            }
            if (Menu.Item("JungleStack.Enable").GetValue<KeyBind>().Active && Utils.SleepCheck("button_cd"))
            {
                foreach (var me in _selectedMeepo)
                {
                    var handle = me.Handle;
                    if (OrderStates[handle] == OrderState.Stacking)
                    {
                        var s = JungleCamps.GetCamps.Find(x => Equals(x.Stacking, me));
                        if (s != null)
                        {
                            s.Stacking = null;
                        }
                        OrderStates[handle] = OrderState.Idle;
                    }
                    else
                        OrderStates[handle] = OrderState.Stacking;
                }
                Utils.Sleep(250, "button_cd");
            }
            RefreshMeepoList();
            foreach (var meepo in MeepoSet)
            {
                var w = meepo.SpellW;
                if (w.IsInAbilityPhase)
                {
                    if (meepo.PoofStartTime == float.MaxValue)
                    {
                        meepo.PoofStartTime = Game.RawGameTime;
                    }
                }
                else if (meepo.PoofStartTime != float.MaxValue)
                {
                    meepo.PoofStartTime = float.MaxValue;
                }
                SafeTp(meepo.Hero, meepo.SpellW);
            }

            if (Menu.Item("hotkey.Escape").GetValue<KeyBind>().Active && Utils.SleepCheck("button_cd"))
            {
                foreach (var handle in _selectedMeepo.Select(me => me.Handle))
                {
                    if (OrderStates[handle] == OrderState.Escape)
                        OrderStates[handle] = OrderState.Idle;
                    else
                    {
                        NeedHeal[handle] = true;
                        OrderStates[handle] = OrderState.Escape;
                    }
                }
                Utils.Sleep(250, "button_cd");
            }

            if (Menu.Item("hotkey.PoofAll").GetValue<KeyBind>().Active)
            {
                foreach (
                    var me in
                        MeepoSet.Where(
                            x =>
                                x.Hero.IsAlive && x.CurrentOrderState != OrderState.Escape &&
                                !Equals(_selectedMeepo.FirstOrDefault(), x.Hero)))
                {
                    var handle = me.Handle;
                    var spell = me.SpellW;
                    if (spell != null && spell.CanBeCasted() && Utils.SleepCheck("all_poof" + handle))
                    {
                        spell.UseAbility((Unit) _selectedMeepo.First());
                        Utils.Sleep(250, "all_poof" + handle);
                    }

                }
            }

            if (ThrowingNet && isNetEnable)
            {
                //var target = TargetSelector.ClosestToMouse(MyHero);
                var target = Game.MousePosition;
                if (true)
                {
                    foreach (
                        var me in
                            MeepoSet.Where(
                                x =>
                                    x.Hero.IsAlive && x.CurrentOrderState != OrderState.Escape && x.Hero.Distance2D(target)<=x.SpellQ.GetCastRange())
                                .OrderBy(y => y.Hero.Distance2D(target)))
                    {
                        var spell = me.SpellQ;
                        var handle = me.Handle;
                        if (spell != null && spell.CanBeCasted() && Utils.SleepCheck("throwNet"+handle))
                        {
                            spell.UseAbility(target);
                            isNetEnable = false;
                            Utils.Sleep(1000, "throwNet"+handle);
                            return;
                        }
                    }
                }
            }
            if (!Menu.Item("hotkey").GetValue<KeyBind>().Active || (_globalTarget != null && !_globalTarget.IsAlive))
            {
                _globalTarget = null;
                FlushEffect();
                return;
            }

            if (_globalTarget == null || !_globalTarget.IsValid || !Menu.Item("LockTarget").GetValue<bool>())
            {
                _globalTarget = ClosestToMouse(MyHero, 300);
            }
            if (!MyHero.IsAlive) return;
            if (_globalTarget == null || !_globalTarget.IsValid || !_globalTarget.IsAlive) return;

            DoCombo(_globalTarget);
        }

        #endregion

        #region MainShit

        private static void DoCombo(Hero target)
        {
            var theClosestMeepo = _meepoList.OrderBy(target.Distance2D).First();
            var dist = theClosestMeepo.Distance2D(target)+MyHero.HullRadius+target.HullRadius;
            var targetPos = target.Position;

            #region GetItems&Spells

            if (OrderStates[MyHero.Handle] != OrderState.Escape)
            {
                if (_blink != null && _blink.CanBeCasted() && dist <= 1150 && dist >= 250 && Utils.SleepCheck("Blink"))
                {
                    _blink.UseAbility(targetPos);
                    Utils.Sleep(250, "Blink");
                }
                var bkb = target.FindItem("item_black_king_bar");
                if (bkb != null && bkb.CanBeCasted() && _hex != null && _hex.CanBeCasted(target) &&
                    Utils.SleepCheck("hex"))
                {
                    _hex.UseAbility(target);
                    Utils.Sleep(250, "hex");
                }
                if (_orchid != null && _orchid.CanBeCasted(target) && !target.IsHexed() && Utils.SleepCheck("orchid") &&
                    Utils.SleepCheck("hex"))
                {
                    _orchid.UseAbility(target);
                    Utils.Sleep(250, "orchid");
                }
                if (_hex != null && _hex.CanBeCasted(target) && !target.IsSilenced() && Utils.SleepCheck("hex") &&
                    Utils.SleepCheck("orchid"))
                {
                    _hex.UseAbility(target);
                    Utils.Sleep(250, "hex");
                }
                if (_eb != null && _eb.CanBeCasted(target) && Utils.SleepCheck("eb"))
                {
                    _eb.UseAbility(target);
                    Utils.Sleep(250, "eb");
                }
            }

            #endregion

            foreach (
                var handle in
                    _meepoList.Where(x => x.IsAlive && OrderStates[x.Handle] != OrderState.Escape)
                        .Select(meepo => meepo.Handle))
            {
                OrderStates[handle] = OrderState.InCombo;
            }
            foreach (var meepo in _meepoList.Where(x => x.IsAlive && OrderStates[x.Handle] == OrderState.InCombo).OrderBy(y=>y.Distance2D(target)))
            {
                #region gettings spells and drawing effects

                var handle = meepo.Handle;
                DrawEffects(meepo, target);

                Ability q, w;
                if (!SpellQ.TryGetValue(handle, out q))
                    SpellQ[handle] = meepo.Spellbook.Spell1;
                if (!SpellW.TryGetValue(handle, out w))
                    SpellW[handle] = meepo.Spellbook.Spell2;
                #region Change Orders

                if (SafeTp(meepo, w))
                    continue;

                #endregion
                #endregion

                #region CastQ

                var mod = target.FindModifier("modifier_meepo_earthbind");
                var remTime = mod?.RemainingTime ?? 0;

                if ((_blink==null || !_blink.CanBeCasted()) && q != null && q.CanBeCasted() && dist <= q.CastRange &&
                    (mod == null || remTime <= .7) &&
                    Utils.SleepCheck("Period_q"))
                {
                    if (q.CastSkillShot(target))
                    //if (q.CastStun(target))
                    {
                        Utils.Sleep(q.GetHitDelay(target)*1000+100, "Period_q");
                    }
                }

                #endregion

                #region CastW

                if (w != null)
                {
                    var castRange = w.GetRealCastRange();
                    if ((!Equals(theClosestMeepo, meepo) || target.IsHexed() || target.IsStunned() ||
                         target.MovementSpeed <= 200 || (remTime > 1.3)) && w.CanBeCasted() &&
                        dist <= castRange &&
                        Utils.SleepCheck("Period_w" + handle))
                    {
                        w.UseAbility(theClosestMeepo);
                        Utils.Sleep(1500, "Period_w" + handle);
                    }
                    if (!Utils.SleepCheck("Period_w" + handle))
                    {
                        if (dist >= castRange)
                        {
                            Utils.Sleeps.Remove("Period_w" + handle);
                            meepo.Stop();
                        }
                    }
                }

                #endregion

                #region AutoAttack

                if (!target.IsVisible)
                {
                    if (Utils.SleepCheck("attack_rate" + handle))
                    {
                        Utils.Sleep(250, "attack_rate" + handle);
                        meepo.Move(Prediction.InFront(target, 250));
                    }
                }
                else
                {
                    var orb = OrbWalkManager(meepo);
                    orb?.OrbwalkOn(target, followTarget: true);
                }

                #endregion

            }

            #region Auto Meka

            if (NeedUseMeka() && _meka != null && _meka.CanBeCasted() && Utils.SleepCheck("meka"))
            {
                _meka.UseAbility();
                Utils.Sleep(250, "meka");
            }

            #endregion

        }

        private static void JungleFarm(Hero me)
        {
            var s = JungleCamps.FindClosestCamp(me, Menu.Item("JungleFarm.TeamCheck").GetValue<bool>(),
                Menu.Item("JungleFarm.Ancient").GetValue<bool>());
            string name;

            var enemyHeroes = Heroes.GetByTeam(me.GetEnemyTeam()).Where(x => x.IsAlive).ToList();
            var dist = Menu.Item("JungleFarm.EscapeRange").GetValue<Slider>().Value;
            if (Menu.Item("JungleFarm.EscapeFromAnyEnemyHero").GetValue<bool>() &&
                    enemyHeroes.Any(x => x.Distance2D(me) <= dist)) //escape from hero
            {
                var handle = me.Handle;
                OrderStates[handle] = OrderState.Escape;
                NeedHeal[handle] = true;
            }

            if (s == null)
            {
                s = JungleCamps.GetCamps.OrderBy(x => x.StackPosition.Distance2D(me)).FirstOrDefault();
                if (s != null)
                {
                    name = MeepoSet.Find(x => Equals(x.Hero, me)).Handle.ToString();
                    if (Utils.SleepCheck("MOVIER_jungle" + name))
                    {
                        me.Move(s.StackPosition);
                        Utils.Sleep(500, "MOVIER_jungle" + name);
                    }
                }
                return;
            }
            name = MeepoSet.Find(x => Equals(x.Hero, me)).Handle.ToString();
            var anyMeepo =
                MeepoSet.Where(
                    x =>
                        x.CurrentOrderState == OrderState.Jungle && x.IsAlive && x.Handle != me.Handle &&
                        x.Hero.Health >= Menu.Item("Escape.MinRange").GetValue<Slider>().Value)
                    .OrderBy(y => y.Hero.Distance2D(me))
                    .FirstOrDefault();
            if (anyMeepo != null && me.Health <= 500 && anyMeepo.Hero.Distance2D(MyHero) <= 400 &&
                CheckForChangedHealth(me))
            {
                if (!Utils.SleepCheck(name + "attack_test")) return;
                Utils.Sleep(200, name + "attack_test");
                var enemy =
                    ObjectManager.GetEntities<Unit>()
                        .FirstOrDefault(
                            x =>
                                x.IsAlive && x.IsVisible && x.Team != MyHero.Team && x.Distance2D(MyHero) <= 500 &&
                                !x.IsWaitingToSpawn);
                if (enemy != null)
                {
                    var creep = enemy.Position;
                    var ang = me.FindAngleBetween(creep, true);
                    var p = new Vector3((float) (me.Position.X - 250*Math.Cos(ang)),
                        (float) (me.Position.Y - 250*Math.Sin(ang)), 0);
                    me.Move(p);
                }
                me.Attack(anyMeepo.Hero, true);
                return;
            }

            var mySet = MeepoSet.Find(x => Equals(x.Hero, me));
            var w = mySet.SpellW;
            if (w != null && Menu.Item("JungleFarm.AutoW").GetValue<bool>() && w.CanBeCasted())
            {
                var enemy =
                    ObjectManager
                        .GetEntities<Unit>(
                        )
                        .FirstOrDefault(
                            x =>
                                x.IsAlive && x.Health > 80 && x.IsVisible && x.Team != MyHero.Team &&
                                x.Distance2D(me) <= 375 &&
                                !x.IsWaitingToSpawn);
                if (enemy != null && Utils.SleepCheck("jungle_farm_w" + name))
                {
                    w.UseAbility(enemy.Position);
                    Utils.Sleep(1500, "jungle_farm_w_inCasting" + name);
                    Utils.Sleep(250, "jungle_farm_w" + name);
                }
                else if (enemy == null && !Utils.SleepCheck("jungle_farm_w_inCasting" + name) &&
                         Utils.SleepCheck("jungle_farm_w_stop" + name))
                {
                    me.Stop();
                    Utils.Sleeps.Remove("jungle_farm_w_inCasting" + name);
                    Utils.Sleep(500, "jungle_farm_w_stop" + name);
                }
            }
            if (!Utils.SleepCheck(name + "attack") || me.IsAttacking()) return;
            Utils.Sleep(500, name + "attack");
            me.Attack(s.CampPosition);
        }

        private static bool SafeTp(Unit me, Ability w)
        {
            if (true) //(Menu.Item("AutoHeal.Hero.Enable").GetValue<bool>())
            {
                var handle = me.Handle;
                bool nh;
                if (!NeedHeal.TryGetValue(handle, out nh))
                    NeedHeal.Add(handle, false);
                var perc = me.Health/(float) me.MaximumHealth*100;
                //Print(String.Format("Health: {0}, Max Health: {1}, Percent: {2}", Me.Health, Me.MaximumHealth, perc));
                if (NeedHeal[handle])
                {
                    if ((perc > 95 &&
                         me.HasModifiers(new[] {"modifier_fountain_aura", "modifier_fountain_aura_buff"}, false)) ||
                        OrderStates[handle] != OrderState.Escape)
                    {
                        NeedHeal[handle] = false;
                        var newOrder = LastOrderStates[handle] != OrderState.Escape
                            ? LastOrderStates[handle]
                            : OrderState.Idle;
                        OrderStates[handle] = _globalTarget == null ? newOrder/*OrderState.Idle*/ : OrderState.InCombo;
                        //Print(newOrder.ToString());
                        //Print("im full now ["+handle+"]");
                    }
                    else if (!me.HasModifiers(new[] {"modifier_fountain_aura", "modifier_fountain_aura_buff"}, false))
                    {
                        if (Utils.SleepCheck("move check" + handle))
                        {
                            var anyEnemyHero =
                                Heroes.GetByTeam(me.GetEnemyTeam())
                                    .FirstOrDefault(x => x.IsAlive && x.IsVisible && x.Distance2D(me) <= 800);
                            if (anyEnemyHero != null)
                            {
                                var spell = SpellQ[handle];
                                if (spell != null && spell.CanBeCasted() && !anyEnemyHero.HasModifier("modifier_meepo_earthbind"))
                                {
                                    spell.CastSkillShot(anyEnemyHero);
                                }
                            }
                            var anyAllyMeepoNearBase =
                                _meepoList.Where(
                                    x =>
                                        !Equals(x, me) && x.Distance2D(Fountains.GetAllyFountain()) <= 5000 && x != me &&
                                        !Heroes.GetByTeam(me.GetEnemyTeam()).Any(y => y.IsAlive && y.IsVisible && y.Distance2D(x) <= 1500))
                                    .OrderBy(z => z.Distance2D(Fountains.GetAllyFountain())).FirstOrDefault();
                            var underTower = Towers.All.Where(x => x.Team == me.GetEnemyTeam())
                                .Any(x => x.Distance2D(me) <= 800);
                            if (anyAllyMeepoNearBase != null && w.CanBeCasted() && !underTower)
                            {
                                var crossCheck = (me.Distance2D(Fountains.GetAllyFountain()) >=
                                                   anyAllyMeepoNearBase.Distance2D(Fountains.GetAllyFountain()))
                                                  && (me.Distance2D(anyAllyMeepoNearBase) >= 500);

                                if (crossCheck)
                                {
                                    if (Utils.SleepCheck("poofTimeToBase" + handle))
                                    {
                                        w.UseAbility(anyAllyMeepoNearBase);
                                        Utils.Sleep(2000, "poofTimeToBase" + handle);
                                    }
                                }
                            }
                            var channeledAbility = me.GetChanneledAbility();
                            var travelBoots = me.FindItem("item_travel_boots", true) ??
                                              me.FindItem("item_travel_boots_2", true) ??
                                              me.FindItem("item_tpscroll", true);
                            if (me.IsChanneling() && channeledAbility.Name != "item_travel_boots"
                                && channeledAbility.Name != "item_travel_boots_2" &&
                                channeledAbility.Name != "item_tpscroll")
                            {
                                //do nothing while in tp
                            }
                            else if (!underTower && travelBoots != null && travelBoots.CanBeCasted() &&
                                     me.Distance2D(Fountains.GetAllyFountain()) >= 5000 && Utils.SleepCheck("tp_cd" + handle) && Utils.SleepCheck("poofTimeToBase" + handle))
                            {
                                Utils.Sleep(250, "tp_cd" + handle);
                                travelBoots.UseAbility(Fountains.GetAllyFountain().Position);
                            }
                            else
                            {
                                me.Move(Fountains.GetAllyFountain().Position);
                            }
                            Utils.Sleep(500, "move check"+handle);
                        }
                    }
                }
                else
                {
                    if (perc < Menu.Item("Escape.MinRangePercent").GetValue<Slider>().Value || me.Health <= Menu.Item("Escape.MinRange").GetValue<Slider>().Value)
                    {
                        //Print("hp too low, go to fountain. Perc: "+perc);
                        NeedHeal[handle] = true;
                        LastOrderStates[handle] = OrderStates[handle] == OrderState.Escape
                            ? OrderState.Idle
                            : OrderStates[handle];
                        
                        OrderStates[handle] = OrderState.Escape;
                    }
                    else
                    {
                        //Print("checking for hp");
                    }
                }
                return NeedHeal[handle];
            }
        }

        private static void AutoPush(this Hero me)
        {
            var handle = me.Handle;
            var creeps = Creeps.All.Where(x => x != null && x.IsValid && x.IsAlive && x.IsVisible).ToList();
            var creepsEnemy = creeps.Where(x => x.Team != MyHero.Team).ToList();
            var creepsAlly = creeps.Where(x => x.Team == MyHero.Team).ToList();
            var enemyHeroes = Heroes.GetByTeam(MyHero.GetEnemyTeam()).Where(x=>x.IsAlive).ToList();
            var towers = Towers.All.Where(x => x.Team != MyHero.Team).Where(x => x.IsAlive).ToList();
            var creepWithEnemy =
                creepsAlly.FirstOrDefault(
                    x => x.MaximumHealth*65/100 < x.Health && creepsEnemy.Any(y => y.Distance2D(x) <= 1000));
            var travelBoots = me.FindItem("item_travel_boots") ?? me.FindItem("item_travel_boots_2");
            if (travelBoots != null && creepWithEnemy != null && Menu.Item("AutoPush.TravelBoots").GetValue<bool>() && Utils.SleepCheck("TravelBoots."+handle))
            {
                if (travelBoots.CanBeCasted() && !creepsEnemy.Any(x => x.Distance2D(me) <= 1000))
                {
                    travelBoots.UseAbility(creepWithEnemy);
                    Utils.Sleep(500, "TravelBoots."+handle);
                    return;
                }
            }

            var nearestTower =
                towers.OrderBy(y => y.Distance2D(me))
                    .FirstOrDefault() ?? Fountains.GetEnemyFountain();
            var fountain = Fountains.GetAllyFountain();
            var curlane = GetCurrentLane(me);
            var clospoint = GetClosestPoint(curlane);
            var useThisShit = clospoint.Distance2D(fountain) - 250 > me.Distance2D(fountain);
            var name = MeepoSet.Find(x => x.Handle == me.Handle).Handle.ToString();
            if (nearestTower != null && Utils.SleepCheck(name + "attack"))
            {
                var pos = curlane == "mid" || !useThisShit ? nearestTower.Position : clospoint;
                var dist = Menu.Item("AutoPush.EscapeRange").GetValue<Slider>().Value;
                if (Menu.Item("AutoPush.EscapeFromAnyEnemyHero").GetValue<bool>() &&
                    enemyHeroes.Any(x => x.Distance2D(me) <= dist)) //escape from hero
                {
                    OrderStates[handle] = OrderState.Escape;
                    NeedHeal[handle] = true;
                }
                else if (creepsAlly.Any(x => x.Distance2D(nearestTower) <= 800) ||
                         me.Distance2D(nearestTower) > 800)
                {
                    //under tower
                    var hpwasChanged = CheckForChangedHealth(me);
                    if (hpwasChanged)
                    {
                        var allyCreep = creepsAlly.OrderBy(x => x.Distance2D(me)).First();
                        if (allyCreep != null)
                        {
                            var towerPos = nearestTower.Position;
                            var ang = allyCreep.FindAngleBetween(towerPos, true);
                            var p = new Vector3((float) (allyCreep.Position.X - 250*Math.Cos(ang)),
                                (float) (allyCreep.Position.Y - 250*Math.Sin(ang)), 0);
                            me.Move(p);
                            me.Attack(allyCreep, true);
                            Utils.Sleep(1200, name + "attack");
                        }
                        else
                        {
                            var towerPos = nearestTower.Position;
                            var ang = me.FindAngleBetween(towerPos, true);
                            var p = new Vector3((float) (towerPos.X - 1250*Math.Cos(ang)),
                                (float) (towerPos.Y - 1250*Math.Sin(ang)), 0);
                            me.Move(p);
                            Utils.Sleep(500, name + "attack");
                        }
                    }
                    else
                    {
                        var act = me.NetworkActivity;
                        if (!Utils.SleepCheck("attack_time" + name))
                            return;

                        if (Menu.Item("AutoPush.LastHitMode").GetValue<bool>())
                        {
                            var bestEnemyCreep =
                                creepsEnemy.Where(x => x.Health < me.DamageAverage && x.Distance2D(me) <= 800)
                                    .OrderBy(x => x.Distance2D(me))
                                    .FirstOrDefault();
                            if (bestEnemyCreep != null)
                            {
                                me.Attack(bestEnemyCreep);
                                Utils.Sleep(UnitDatabase.GetAttackPoint(me)*1000, "attack_time" + name);
                            }
                            else
                            {
                                /*if (act == NetworkActivity.Attack || act == NetworkActivity.Attack2)
                                {
                                    me.Stop();
                                }*/
                                if (act == NetworkActivity.Idle)
                                {
                                    me.Attack(pos);
                                }
                            }
                        }
                        else
                        {
                            if (act == NetworkActivity.Idle) me.Attack(pos);
                        }

                        if (Menu.Item("AutoPush.AutoW").GetValue<bool>() && SpellW[handle] != null)
                        {
                            var w = SpellW[handle];
                            var castRange = w.GetRealCastRange();
                            if (w.CanBeCasted() &&
                                creepsEnemy.Any(x => x.Distance2D(me) <= castRange && x.Health <= 60 + 20*w.Level) &&
                                Utils.SleepCheck("w_push" + name))
                            {

                                w.UseAbility(me);
                                Utils.Sleep(250, "w_push" + name);
                            }
                        }
                    }
                    Utils.Sleep(100, name + "attack");
                }
                else
                {
                    var towerPos = nearestTower.Position;
                    var ang = me.FindAngleBetween(towerPos, true);
                    var p = new Vector3((float) (me.Position.X - 1000*Math.Cos(ang)),
                        (float) (me.Position.Y - 1000*Math.Sin(ang)), 0);
                    me.Move(p);
                    Utils.Sleep(200, name + "attack");
                }
            }
        }

        private static void Stacking(this Hero me)
        {
            var s = JungleCamps.FindClosestCampForStacking(me, Menu.Item("JungleStack.TeamCheck").GetValue<bool>(),
                Menu.Item("JungleStack.Ancient").GetValue<bool>());
            var enemyHeroes = Heroes.GetByTeam(me.GetEnemyTeam()).Where(x => x.IsAlive).ToList();
            var dist = Menu.Item("JungleStack.EscapeRange").GetValue<Slider>().Value;
            if (Menu.Item("JungleStack.EscapeFromAnyEnemyHero").GetValue<bool>() &&
                    enemyHeroes.Any(x => x.Distance2D(me) <= dist)) //escape from hero
            {
                var handle = me.Handle;
                OrderStates[handle] = OrderState.Escape;
                NeedHeal[handle] = true;
            }
            if (s == null) return;
            s.Stacking = me;
            var set = MeepoSet.Find(x => Equals(x.Hero, me));
            var name = set.Handle.ToString();
            var sec = Game.GameTime%60;
            var timeForStart = s.WaitPosition.Distance2D(s.CampPosition)/me.MovementSpeed;
            var time = s.StackTime - timeForStart - sec;
            //Print("Current Time: [" + sec + "] Time For Travel: [" + timeForStart + "] TimeForStartMoving: [" + (time - sec) + "]");
            //Print(time.ToString());
            var min = Math.Floor(Game.GameTime/60)%2 == 0;
            if (time >= 0.5)
            {
                if (Utils.SleepCheck("move_cd2" + name))
                {
                    me.Move(s.WaitPosition);
                    Utils.Sleep(250, "move_cd2" + name);
                }
            }
            else if (Utils.SleepCheck("move_cd" + name) && min)
            {
                var pos = s.CampPosition;
                var ang = me.FindAngleBetween(pos, true);
                var p = new Vector3((float) (pos.X - 80*Math.Cos(ang)),
                    (float) (pos.Y - 80*Math.Sin(ang)), 0);
                me.Move(p);
                me.Move(s.StackPosition, true);
                Utils.Sleep((60 - s.StackTime)*1000 + 8000, "move_cd" + name);
                Log.Debug($"Meepo #[{MeepoSet.Find(settings => settings.Hero.Equals(me))?.Id}] trying to stack camp #[{s.Id}] ({s.Name})");
                // TODO: camp 4 & 3 
            }
        }

        #endregion

        #region OtherShit

        private static void DrawEffects(Meepo meepo, Hero target)
        {
            ParticleEffect effect;
            var handle = meepo.Handle;
            if (!Effects.TryGetValue(handle, out effect))
            {
                Effects.Add(handle, new ParticleEffect(@"particles\ui_mouseactions\range_finder_tower_aoe.vpcf", target));
            }
            if (effect == null) return;
            effect.SetControlPoint(2, new Vector3(meepo.Position.X, meepo.Position.Y, meepo.Position.Z));
            effect.SetControlPoint(6, new Vector3(1, 0, 0));
            effect.SetControlPoint(7, new Vector3(target.Position.X, target.Position.Y, target.Position.Z));
        }

        private static void FlushEffect()
        {
            foreach (var meepo in _meepoList)
            {
                ParticleEffect effect;
                var handle = meepo.Handle;
                Ability w;
                if (!SpellW.TryGetValue(handle, out w))
                    SpellW[handle] = meepo.Spellbook.Spell2;
                switch (OrderStates[handle])
                {
                    case OrderState.Laning:
                        AutoPush(meepo);
                        break;
                    case OrderState.Jungle:
                        JungleFarm(meepo);
                        break;
                    case OrderState.Stacking:
                        Stacking(meepo);
                        break;
                }

                
                if (OrderStates[handle] == OrderState.InCombo) OrderStates[handle] = OrderState.Idle;
                if (!Effects.TryGetValue(handle, out effect)) continue;
                effect.Dispose();
                Effects.Remove(handle);
            }
        }

        private static bool NeedUseMeka()
        {
            return _meepoList.Where(x => x.IsAlive && x.Distance2D(MyHero) <= 900).Any(meepo => meepo.Health <= 500);
        }

        private static readonly Dictionary<uint, bool> NeedHeal = new Dictionary<uint, bool>();
        private static bool _leftMouseIsPress;

        private static readonly Dictionary<Unit, uint> LastCheckedHp = new Dictionary<Unit, uint>();

        private static bool CheckForChangedHealth(Unit me)
        {
            uint health;
            if (!LastCheckedHp.TryGetValue(me, out health))
            {
                LastCheckedHp.Add(me, me.Health);
            }
            var boolka = health > me.Health;
            LastCheckedHp[me] = me.Health;
            return boolka;
        }

        private static string GetCurrentLane(Unit me)
        {
            return LaneDictionary.OrderBy(x => x.Key.Distance2D(me)).First().Value;
        }

        private static Vector3 GetClosestPoint(string pos)
        {
            var list = LaneDictionary.Keys.ToList();
            switch (pos)
            {
                case "top":
                    return list[0];
                case "bot":
                    return list[3];
                default:
                    return list[6];
            }
        }

        public static readonly List<MeepoSettings> MeepoSet = new List<MeepoSettings>();

        private static void RefreshMeepoList()
        {
            if (Utils.SleepCheck("SelectChecker"))
            {
                _selectedMeepo = MyPlayer.Selection.Where(x => x.ClassId == ClassId.CDOTA_Unit_Hero_Meepo).ToList();
                //Print("selected count: " + SelectedMeepo.Count);
                Utils.Sleep(150, "SelectChecker");
            }
            if (!Utils.SleepCheck("MeepoRefresh")) return;
            Utils.Sleep(500, "MeepoRefresh");
            if (_meepoList.Count >= 1 + _ultimate.Level + (MyHero.AghanimState() ? 1 : 0)) return;
            _meepoList =
                ObjectManager.GetEntities<Meepo>()
                    .Where(x => x.IsValid && !x.IsIllusion() && x.Team == MyHero.Team) /*.OrderBy(x => x.Handle)*/
                    .ToList();
            Print("meepo count is " + _meepoList.Count + " AGHANIM STATE: " + MyHero.AghanimState());

            foreach (var meepo in _meepoList)
            {
                var handle = meepo.Handle;
                OrderState state;
                OrderState laststate;
                if (!OrderStates.TryGetValue(handle, out state))
                {
                    OrderStates.Add(handle, OrderState.Idle);
                }
                if (!LastOrderStates.TryGetValue(handle, out laststate))
                {
                    LastOrderStates.Add(handle, OrderState.Idle);
                }
                Ability q, w;
                if (!SpellQ.TryGetValue(handle, out q))
                    SpellQ[handle] = meepo.Spellbook.Spell1;
                if (!SpellW.TryGetValue(handle, out w))
                    SpellW[handle] = meepo.Spellbook.Spell2;
            }
            foreach (var meepo in _meepoList.Where(meepo => !MeepoSet.Any(x => Equals(x.Hero, meepo))))
            {
                MeepoSet.Add(new MeepoSettings(meepo));
            }
        }

        private static void Camp_update(EventArgs args)
        {
            if (!Utils.SleepCheck("Camp.Update"))
                return;
            Utils.Sleep(150, "Camp.Update");
            //var refreshTime = (Game.GameTime + 60)%120 == 0;
            foreach (var camp in JungleCamps.GetCamps)
            {
                foreach (
                    var enemy in
                        from meepo in
                            MeepoSet.Where(x => x.CurrentOrderState == OrderState.Jungle && x.IsAlive)
                                .Select(y => y.Hero)
                        let heroDist = meepo.Distance2D(camp.CampPosition)
                        where heroDist < 100
                        select ObjectManager.GetEntities<Unit>()
                            .Any(
                                x =>
                                    x.IsAlive && x.IsVisible && x.Team != MyHero.Team && x.Distance2D(meepo) <= 500 &&
                                    !x.IsWaitingToSpawn))
                {
                    camp.CanBeHere = enemy;
                    
                    if (enemy || camp.Delayed) continue;
                    camp.Delayed = true;
                    //Print("[CampStatus]: add delay" + (120 - (Game.GameTime + 60) % 120),true);
                    DelayAction.Add((120 - (Game.GameTime + 60) % 120)*1000, () =>
                    {
                        camp.Delayed = false;
                        camp.CanBeHere = true;
                        //Print("[CampStatus]: delayAction" + camp.CanBeHere,true);
                    });
                }
            }
        }

        private static Hero ClosestToMouse(Hero source, float range = 600)
        {
            var mousePosition = Game.MousePosition;
            var enemyHeroes = ObjectManager.GetEntities<Hero>()
                .Where(
                    x =>
                        x.Team == source.GetEnemyTeam() && !x.IsIllusion && x.IsAlive && x.IsVisible &&
                        x.Distance2D(mousePosition) <= range /*&& !x.IsMagicImmune()*/)
                .OrderBy(x => x.Distance2D(mousePosition));
            return enemyHeroes.FirstOrDefault();
        }

        private static void Print(string s,bool print=false)
        {
            if (print)
                Game.PrintMessage(s);
        }

        private static float GetRealCastRange(this Ability ability)
        {
            var range = ability.CastRange;
            if (range >= 1) return range;
            var data =
                ability.AbilitySpecialData.FirstOrDefault(
                    x => x.Name.Contains("radius") || (x.Name.Contains("range") && !x.Name.Contains("ranged")));
            if (data == null) return range;
            var level = ability.Level == 0 ? 0 : ability.Level - 1;
            range = (uint) (data.Count > 1 ? data.GetValue(level) : data.Value);
            return range;
        }

        private static void DrawText(Vector2 a, float w, float h, Color @on, string drawOnButtonText = "")
        {
            var textSize = Drawing.MeasureText(
                drawOnButtonText,
                "Arial",
                new Vector2((float) (h*.50), 100),
                FontFlags.AntiAlias);
            var textPos = a + new Vector2(5, (float) ((h*0.5) - (textSize.Y*0.5)));
            Drawing.DrawRect(a, new Vector2(w, h), @on);
            Drawing.DrawText(
                drawOnButtonText,
                textPos,
                new Vector2((float) (h*.50), 100),
                Color.White,
                FontFlags.AntiAlias | FontFlags.Additive | FontFlags.Custom);
        }

        private static void DrawButton(Vector2 a, float w, float h, ref bool clicked, Color @on, Color off,
            string drawOnButtonText = "", bool isSelected = false)
        {
            var isIn = Utils.IsUnderRectangle(Game.MouseScreenPosition, a.X, a.Y, w, h);
            if (_leftMouseIsPress && Utils.SleepCheck("ClickButtonCd") && isIn)
            {
                clicked = !clicked;
                Utils.Sleep(250, "ClickButtonCd");
            }
            var newColor = isIn
                ? new Color((int) (clicked ? @on.R : off.R), clicked ? @on.G : off.G, clicked ? @on.B : off.B, 150)
                : clicked ? @on : off;
            var textSize = Drawing.MeasureText(
                drawOnButtonText,
                "Arial",
                new Vector2((float) (h*.50), 100),
                FontFlags.AntiAlias);
            var textPos = a + new Vector2(5, (float) ((h*0.5) - (textSize.Y*0.5)));
            Drawing.DrawRect(a, new Vector2(w, h), newColor);
            Drawing.DrawText(
                drawOnButtonText,
                textPos,
                new Vector2((float) (h*.50), 100),
                Color.White,
                FontFlags.AntiAlias | FontFlags.Additive | FontFlags.Custom);
            if (isSelected)
            {
                Drawing.DrawRect(a, new Vector2(w, h), Color.YellowGreen, true);
            }
        }

        private static void DrawButton(Vector2 a, float w, float h, ref int id, int needed, Color @on,
            string drawOnButtonText = "")
        {
            var isIn = Utils.IsUnderRectangle(Game.MouseScreenPosition, a.X, a.Y, w, h);
            if (_leftMouseIsPress && Utils.SleepCheck("ClickButtonCd") && isIn)
            {
                id = needed;
                Utils.Sleep(250, "ClickButtonCd");
            }
            var newColor = @on;
            var textSize = Drawing.MeasureText(
                drawOnButtonText,
                "Arial",
                new Vector2((float) (h*.50), 100),
                FontFlags.AntiAlias);
            var textPos = a + new Vector2(5, (float) ((h*0.5) - (textSize.Y*0.5)));
            Drawing.DrawRect(a, new Vector2(w, h), newColor);
            Drawing.DrawRect(a, new Vector2(w, h), Color.Black, true);
            Drawing.DrawText(
                drawOnButtonText,
                textPos,
                new Vector2((float) (h*.50), 100),
                Color.White,
                FontFlags.AntiAlias | FontFlags.Additive | FontFlags.Custom);
        }

        private static readonly Dictionary<uint, Orbwalker> Orbwalkers = new Dictionary<uint, Orbwalker>();
        private static Orbwalker OrbWalkManager(Hero me)
        {
            Orbwalker orb;
            var handle = me.Handle;
            if (Orbwalkers.TryGetValue(handle, out orb)) return orb;
            orb = new Orbwalker(me);
            Orbwalkers.Add(handle, orb);
            return orb;
        }

        #endregion
    }
}
