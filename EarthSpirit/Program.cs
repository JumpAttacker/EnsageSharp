using System;
using System.Linq;
using System.Reflection;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using SharpDX;

namespace EarthSpirit
{
    internal class Program
    {
        #region Members
        //============================================================
        private static bool _loaded;
        private static readonly string Ver = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        private const int WmKeyup = 0x0101;
        private static bool _leftMouseIsPress;
        private static bool _showMenu = true;
        private static bool _lastStateAction;
        private static bool _inAction;
        private static bool _aghanimState;
        private static bool _supUlt;
        private static Vector2 _sizer = new Vector2(265, 300);
        //============================================================
        private static ulong _useComboKey = 'G';
        private static ulong _usePushKey = 'Z';
        private static ulong _useRollKey = 'X';
        private static ulong _usePullKey = 'C';
        private static bool _timetochange;
        private static bool _shouldUseDagger;
        private static bool _tryToStealWithPush=true;
        private static bool _useRoll;
        public static Ability Remnant;
        public static Ability Push;
        public static Ability Pull;
        public static Ability Roll;
        public static Ability Magnetize;
        private static int _stage;
        private static Hero _globalTarget;
        public static int Combo { get; set; }
        private static bool _timetochangePush;
        private static bool _timetochangeRoll;
        private static bool _timetochangePull;
        public static bool Debug = true;


        //============================================================
        #endregion

        private static void Main()
        {
            Game.OnUpdate += Game_OnUpdate;
            _loaded = false;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnWndProc += Game_OnWndProc;
        }

        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (Game.IsChatOpen || !_loaded)
                return;
            if (!_inAction)
            {
                if (_timetochange && args.Msg == WmKeyup && args.WParam >= 0x41 && args.WParam <= 0x5A)
                {
                    _timetochange = false;
                    _useComboKey = args.WParam;
                    return;
                }
                if (_timetochangePush && args.Msg == WmKeyup && args.WParam >= 0x41 && args.WParam <= 0x5A)
                {
                    _timetochangePush = false;
                    _usePushKey = args.WParam;
                    return;
                }
                if (_timetochangeRoll && args.Msg == WmKeyup && args.WParam >= 0x41 && args.WParam <= 0x5A)
                {
                    _timetochangeRoll = false;
                    _useRollKey = args.WParam;
                    return;
                }
                if (_timetochangePull && args.Msg == WmKeyup && args.WParam >= 0x41 && args.WParam <= 0x5A)
                {
                    _timetochangePull = false;
                    _usePullKey = args.WParam;
                    return;
                }
                if (args.Msg == WmKeyup)
                {
                    if (args.WParam == _usePushKey)
                        LetsPush();
                    if (args.WParam == _useRollKey)
                        LetsRoll();
                    if (args.WParam == _usePullKey)
                        LetsPull();
                }
            }
            if (args.WParam == _useComboKey)
            {
                _aghanimState = Game.IsKeyDown(0x11);
                _inAction = args.Msg != WmKeyup;
                if (_inAction != _lastStateAction)
                {
                    if (Debug) if (_inAction) PrintInfo("combo key is pressed");
                    _lastStateAction = _inAction;
                    Game.ExecuteCommand(string.Format("dota_player_units_auto_attack_after_spell {0}", _inAction ? 0 : 1));
                    if (_inAction)
                    {
                        ObjectMgr.LocalHero.Stop();
                    }
                }
                if (!_inAction)
                {
                    _globalTarget = null;
                }
            }
            if (args.WParam != 1 || !Utils.SleepCheck("clicker"))
            {
                _leftMouseIsPress = false;
                return;
            }
            _leftMouseIsPress = true;
        }

        private static void LetsPull()
        {
            var me = ObjectMgr.LocalHero;
            if (!Remnant.CanBeCasted() || !Pull.CanBeCasted() || !Utils.SleepCheck("preComboW8")) return;
            var pos = Game.MousePosition;
            if (AnyStoneNear(me, pos))
            {

            }
            else if (Remnant.CanBeCasted())
            {
                Remnant.UseAbility(pos);
            }
            else
            {
                return;
            }
            Pull.UseAbility(pos);
            Utils.Sleep(500,"preComboW8");
        }

        private static void LetsRoll()
        {
            if (!Roll.CanBeCasted() || !Utils.SleepCheck("preComboW8")) return;
            var me = ObjectMgr.LocalHero;
            if (AnyStoneNear(me))
            {

            }
            else if (Remnant.CanBeCasted())
            {
                var ang = me.FindAngleBetween(Game.MousePosition, true);
                var p = new Vector2((float)(me.Position.X + 200 * Math.Cos(ang)), (float)(me.Position.Y + 100 * Math.Sin(ang)));
                Remnant.UseAbility(p.ToVector3(true));
            }
            else
            {
                return;
            }
            Roll.UseAbility(Game.MousePosition);
            Utils.Sleep(500, "preComboW8");
        }

        private static void LetsPush()
        {
            var me = ObjectMgr.LocalHero;
            if (!Push.CanBeCasted() || !Utils.SleepCheck("preComboW8")) return;
            if (AnyStoneNear(me,new Vector3(),200F))
            {
                
            }
            else if (Remnant.CanBeCasted())
            {
                var ang = me.FindAngleBetween(Game.MousePosition, true);
                var p = new Vector2((float)(me.Position.X + 50 * Math.Cos(ang)), (float)(me.Position.Y + 50 * Math.Sin(ang)));
                Remnant.UseAbility(p.ToVector3(true));
            }
            else
            {
                return;
            }
            var pos = Game.MousePosition;
            if (AnyStoneNear(me, pos))
            {
                var ang = me.FindAngleBetween(pos, true);
                pos = new Vector2((float)(pos.X + 300 * Math.Cos(ang)), (float)(pos.Y + 300 * Math.Sin(ang))).ToVector3(true);
            }
            Push.UseAbility(pos);
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var player = ObjectMgr.LocalPlayer;
            if (player == null || player.Team == Team.Observer || !_loaded)
            {
                return;
            }
            if (ObjectMgr.LocalHero.ClassID != ClassID.CDOTA_Unit_Hero_EarthSpirit) return;
            var startPos = new Vector2(50, 200);
            var maxSize = new Vector2(120, 280);
            if (_showMenu)
            {
                _sizer.X += 4;
                _sizer.Y += 4;
                _sizer.X = Math.Min(_sizer.X, maxSize.X);
                _sizer.Y = Math.Min(_sizer.Y, maxSize.Y);

                Drawing.DrawRect(startPos, _sizer, new Color(0, 155, 255, 100));
                Drawing.DrawRect(startPos, _sizer, new Color(0, 0, 0, 255), true);
                Drawing.DrawRect(startPos + new Vector2(-5, -5), _sizer + new Vector2(10, 10),
                    new Color(0, 0, 0, 255), true);
                DrawButton(startPos + new Vector2(_sizer.X - 20, -20), 20, 20, ref _showMenu, true, Color.Gray,
                    Color.Gray);
                if (!Equals(_sizer, maxSize)) return;

                DrawButton(startPos + new Vector2(10, 10), 100, 20, ref _shouldUseDagger, true,
                    new Color(0, 200, 150),
                    new Color(200, 0, 0, 100), "Dagger On Start");
                DrawButton(startPos + new Vector2(10, 35), 100, 20, ref _tryToStealWithPush, true,
                    new Color(0, 200, 150),
                    new Color(200, 0, 0, 100), "Kill Steal Smash");
                DrawButton(startPos + new Vector2(10, 60), 100, 20, ref _useRoll, true,
                    new Color(0, 200, 150),
                    new Color(200, 0, 0, 100), "Rolling");
                DrawButton(startPos + new Vector2(10, 85), 100, 20, ref _supUlt, true,
                    new Color(0, 200, 150),
                    new Color(200, 0, 0, 100), "Sup Ult");
                DrawButton(startPos + new Vector2(10, 110), 100, 20, ref _timetochangePush, true,
                    new Color(0, 200, 150),
                    new Color(200, 0, 0, 100), "Push HK");
                DrawButton(startPos + new Vector2(10, 135), 100, 20, ref _timetochangeRoll, true,
                    new Color(0, 200, 150),
                    new Color(200, 0, 0, 100), "Roll HK");
                DrawButton(startPos + new Vector2(10, 160), 100, 20, ref _timetochangePull, true,
                    new Color(0, 200, 150),
                    new Color(200, 0, 0, 100), "Pull HK");

                DrawButton(startPos + new Vector2(10, _sizer.Y - 70), 100, 20, ref _timetochange, true,
                    new Color(0, 200, 150),
                    new Color(200, 0, 0, 100), "Change Hotkey");

                Drawing.DrawText(
                    string.Format("Status: [{0}]", _inAction ? _aghanimState ? "Agh ON" : "ON" : "OFF"),
                    startPos + new Vector2(10, _sizer.Y - 50), Color.White,
                    FontFlags.AntiAlias | FontFlags.DropShadow);
                Drawing.DrawText(string.Format("Single [{0}] [{1}] [{2}]", (char)_usePushKey, (char)_useRollKey, (char)_usePullKey),
                    startPos + new Vector2(10, _sizer.Y - 35), Color.White,
                    FontFlags.AntiAlias | FontFlags.DropShadow);
                Drawing.DrawText(string.Format("ComboKey {0}", (char)_useComboKey),
                    startPos + new Vector2(10, _sizer.Y - 20), Color.White,
                    FontFlags.AntiAlias | FontFlags.DropShadow);
            }
            else
            {
                _sizer.X -= 4;
                _sizer.Y -= 4;
                _sizer.X = Math.Max(_sizer.X, 20);
                _sizer.Y = Math.Max(_sizer.Y, 0);
                Drawing.DrawRect(startPos, _sizer, new Color(0, 155, 255, 100));
                Drawing.DrawRect(startPos, _sizer, new Color(0, 0, 0, 255), true);
                DrawButton(startPos + new Vector2(_sizer.X - 20, -20), 20, 20, ref _showMenu, true, Color.Gray,
                    Color.Gray);
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            #region Init

            var me = ObjectMgr.LocalHero;
            if (!_loaded)
            {
                if (!Game.IsInGame || me == null || me.ClassID != ClassID.CDOTA_Unit_Hero_EarthSpirit)
                {
                    return;
                }
                _loaded = true;

                PrintSuccess(string.Format("> EarthSpirit Annihilation Loaded v{0}", Ver));

                Remnant = me.FindSpell("earth_spirit_stone_caller");
                Push = me.FindSpell("earth_spirit_boulder_smash");
                Pull = me.FindSpell("earth_spirit_geomagnetic_grip");
                Roll = me.FindSpell("earth_spirit_rolling_boulder");
                Magnetize = me.FindSpell("earth_spirit_magnetize");
            }

            if (!Game.IsInGame || me == null)
            {
                _loaded = false;
                PrintInfo("> EarthSpirit unLoaded");
                return;
            }

            if (Game.IsPaused)
            {
                return;
            }
            #endregion

            #region Lets combo

            
            if (!_inAction) return;
            if (_globalTarget == null || !_globalTarget.IsValid)
            {
                _globalTarget = ClosestToMouse(me, 150);
                _stage = 0;
            }
            if (_globalTarget == null || !_globalTarget.IsValid || !_globalTarget.IsAlive || !me.CanCast()) return;
            if (_aghanimState)
            {
                LetAghanimCombo(me, _globalTarget);
            }
            else
            {
                ComboInAction(me, _globalTarget);
            }

            #endregion
        }

        private static void LetAghanimCombo(Hero me,Hero target)
        {
            if (!Utils.SleepCheck("nextAction")) return;
            var ability = me.FindSpell("earth_spirit_petrify");
            if (ability==null) return;
            if (ability.Level==0) return;

            var inStone = target.Modifiers.Any(x=>x.Name=="modifier_earthspirit_petrify");

            var dist = me.Distance2D(target);
            if (ability.CanBeCasted() && !inStone)
            {
                if (_shouldUseDagger)
                {
                    var blink = me.FindItem("item_blink");
                    if (dist >= ability.CastRange && blink!=null && blink.CanBeCasted())
                    {
                        var ang = me.FindAngleBetween(target.Position, true);
                        var p = new Vector2((float)(me.Position.X + 1100 * Math.Cos(ang)), (float)(me.Position.Y + 1100 * Math.Sin(ang)));
                        blink.UseAbility(p.ToVector3(true));
                    }
                }
                ability.UseAbility(target);
                Utils.Sleep(150 + ability.FindCastPoint(), "nextAction");
                return;
            }
            if (Pull != null && Pull.CanBeCasted() && inStone)
            {
                Pull.UseAbility(target.Position);
                //PrintInfo(Roll.CastSkillShot(target).ToString());
                Utils.Sleep(300 + Pull.FindCastPoint(), "nextAction");
                return;
            }
            if (Push != null && Push.CanBeCasted() && inStone && dist<=150)
            {
                Push.UseAbility(Game.MousePosition);
                if (Roll != null && Roll.CanBeCasted())
                {
                    if (Remnant != null && Remnant.CanBeCasted())
                    {
                        var ang = me.FindAngleBetween(Game.MousePosition, true);
                        var p = new Vector2((float) (me.Position.X + 100 * Math.Cos(ang)), (float) (me.Position.Y + 100 * Math.Sin(ang)));
                        Remnant.UseAbility(p.ToVector3(true));
                    }
                    Roll.UseAbility(Game.MousePosition,true);
                }
                Utils.Sleep(300 + Push.FindCastPoint(), "nextAction");
            }
        }

        private static void ComboInAction(Hero me, Hero target)
        {
            if (!Utils.SleepCheck("nextAction")) return;
            var dist = me.Distance2D(target);
            switch (_stage)
            {
                case 0:
                    if (target.Modifiers.Any(x => x.Name == "modifier_earth_spirit_magnetize") && !CanCastCombo()) { _stage = 5; }
                    if (_shouldUseDagger)
                    {
                        var blink = me.FindItem("item_blink");
                        if (dist >= Pull.CastRange && blink != null && blink.CanBeCasted())
                        {
                            if (dist >= Pull.CastRange + 1100)
                            {
                                me.Move(target.Position);
                                Utils.Sleep(200, "nextAction");
                                break;
                            }
                            var ang = me.FindAngleBetween(target.Position, true);
                            var p = new Vector2((float)(me.Position.X + 1100 * Math.Cos(ang)), (float)(me.Position.Y + 1100 * Math.Sin(ang)));
                            blink.UseAbility(p.ToVector3(true));
                            break;
                        }
                    }
                    if (AnyStoneNear(me) && dist <= 1900)
                    {
                        if (Debug) PrintInfo("stone near you finded");
                        _stage++;
                        break;
                    }
                    if (Remnant.CanBeCasted())
                    {
                        if (dist <= 1900)
                        {
                            if (me.NetworkActivity == NetworkActivity.Move)
                                me.Stop();
                            Remnant.UseAbility(Prediction.InFront(me, 100));
                            Utils.Sleep(50 + Remnant.FindCastPoint(), "nextAction");
                            _stage++;
                            if (Debug) PrintInfo("remnant create");
                        }
                        else
                        {
                            me.Move(target.Position);
                            Utils.Sleep(50 + Remnant.FindCastPoint(), "nextAction");
                        }

                    }
                    break;
                case 1:
                    if (Push.CanBeCasted())
                    {
                        var last = GetLastRemnant(me);

                        if (last != null)
                        {
                            if (Debug) PrintInfo("push casted");
                            Push.UseAbility(target.Position);
                            Utils.Sleep(100 + Push.FindCastPoint(), "nextAction");
                        }
                    }
                    else
                        _stage++;
                    break;
                case 2:
                    if (Pull.CanBeCasted())
                    {
                        var last = GetLastRemnant(me);
                        if (target.Distance2D(last) <= 200)
                        {
                            if (me.Distance2D(target) <= Pull.CastRange)
                            {
                                Pull.UseAbility(last.Position);
                                if (Debug) PrintInfo("pull casted");
                                Utils.Sleep(100 + Pull.FindCastPoint(), "nextAction");
                                
                            }
                            else /*if (_shouldUseDagger)*/
                            {
                                var blink = me.FindItem("item_blink");
                                if (dist >= Pull.CastRange && blink != null && blink.CanBeCasted())
                                {
                                    if (dist >= Pull.CastRange + 1100)
                                    {
                                        me.Move(target.Position);
                                        Utils.Sleep(200, "nextAction");
                                        break;
                                    }
                                    var ang = me.FindAngleBetween(target.Position, true);
                                    var p = new Vector2((float)(me.Position.X + 1100 * Math.Cos(ang)), (float)(me.Position.Y + 1100 * Math.Sin(ang)));
                                    blink.UseAbility(p.ToVector3(true));
                                    Utils.Sleep(100, "nextAction");
                                    if (Debug) PrintInfo("dagger is used");
                                }
                            }
                        }
                    }
                    else
                        _stage++;
                    break;
                case 3:
                    if (Roll.CanBeCasted() && !Pull.CanBeCasted())
                    {
                        Roll.UseAbility(target.Position);
                        Utils.Sleep(100 + Roll.FindCastPoint(), "nextAction");
                        if (Debug) PrintInfo("roll casted");
                    }
                    else
                        _stage++;
                    break;
                case 4:
                    if (Magnetize.CanBeCasted())
                    {
                        if (me.Distance2D(target) <= 300)
                        {
                            Magnetize.UseAbility();
                            Utils.Sleep(100 + Magnetize.FindCastPoint(), "nextAction");
                            _stage++;
                            if (Debug) PrintInfo("Magnetize casted");
                        }
                    }
                    break;
                case 5:
                    if (Remnant.CanBeCasted() && _supUlt)
                    {
                        var mod = target.Modifiers.FirstOrDefault(x => x.Name == "modifier_earth_spirit_magnetize");
                        if (mod != null && mod.RemainingTime <= 0.5+Game.Ping && me.Distance2D(target)<=Remnant.CastRange)
                        {
                            Remnant.UseAbility(target.Position);
                            Utils.Sleep(1000, "nextAction");
                            me.Attack(target,true);
                            break;
                        }
                        if (Utils.SleepCheck("attackcd"))
                        {
                            me.Attack(target);
                            Utils.Sleep(150, "attackcd");
                        }
                    }
                    else if (Utils.SleepCheck("attackcd"))
                    {
                        me.Attack(target);
                        Utils.Sleep(200, "attackcd");
                    }
                    if (_tryToStealWithPush&&Push.CanBeCasted() && target.DamageTaken(50 * Push.Level, DamageType.Magical, me) > target.Health)
                    {
                        Push.UseAbility(target);
                        Utils.Sleep(500, "nextAction");
                        me.Attack(target, true);
                        break;
                    }
                    if (_useRoll && Roll.CanBeCasted())
                    {
                        Roll.UseAbility(target.Position);
                        Utils.Sleep(500, "nextAction");
                    }
                    break;
                    
            }
        }

        

        private static bool CanCastCombo()
        {
            return Push.CanBeCasted() && Pull.CanBeCasted() && Roll.CanBeCasted();
        }

        private static bool AnyStoneNear(Hero me,Vector3 pos=new Vector3(),float range=150)
        {
            if (pos.IsZero)
                return ObjectMgr.GetEntities<Unit>()
                    .Any(
                        x =>
                            x.ClassID == ClassID.CDOTA_Unit_Earth_Spirit_Stone && x.Team == me.Team &&
                            x.Distance2D(me) <= range && x.IsAlive && x.IsValid);
            return ObjectMgr.GetEntities<Unit>()
                .Any(
                    x =>
                        x.ClassID == ClassID.CDOTA_Unit_Earth_Spirit_Stone && x.Team == me.Team &&
                        x.Distance2D(pos) <= range);

        }

        private static Unit GetLastRemnant(Hero me)
        {
            try
            {
                var stones = ObjectMgr.GetEntities<Unit>()
                .Where(
                    x =>
                        x.ClassID == ClassID.CDOTA_Unit_Earth_Spirit_Stone && x.Team == me.Team);
                Unit[] last = { null };
                foreach (var enemyHero in stones.Where(enemyHero => last[0] == null || last[0].Modifiers.First().RemainingTime <= enemyHero.Modifiers.First().RemainingTime))
                {
                    last[0] = enemyHero;
                }
                return last[0];
            }
            catch (Exception)
            {

                return null;
            }
            
        }

        public static Hero ClosestToMouse(Hero source, float range = 600)
        {
            var mousePosition = Game.MousePosition;
            var enemyHeroes =
                ObjectMgr.GetEntities<Hero>()
                    .Where(
                        x =>
                            x.Team == source.GetEnemyTeam() && !x.IsIllusion && x.IsAlive && x.IsVisible
                            && x.Distance2D(mousePosition) <= range && !x.IsMagicImmune());
            Hero[] closestHero = { null };
            foreach (var enemyHero in enemyHeroes.Where(enemyHero => closestHero[0] == null || closestHero[0].Distance2D(mousePosition) > enemyHero.Distance2D(mousePosition)))
            {
                closestHero[0] = enemyHero;
            }
            return closestHero[0];
        }

        #region Helpers
/*
        private static void DrawButton(Vector2 a, float w, float h, int numberOfCombo, bool isActive, Color @on, Color off, string des)
        {
            var isIn = Utils.IsUnderRectangle(Game.MouseScreenPosition, a.X, a.Y, w, h);
            if (isActive)
            {
                if (_leftMouseIsPress && Utils.SleepCheck("ClickButtonCd") && isIn)
                {
                    _combo = numberOfCombo;
                    Utils.Sleep(250, "ClickButtonCd");
                }
                var newColor = isIn
                    ? new Color((int)(_combo == numberOfCombo ? @on.R : off.R), _combo == numberOfCombo ? @on.G : off.G, _combo == numberOfCombo ? @on.B : off.B, 150)
                    : _combo == numberOfCombo ? @on : off;
                Drawing.DrawRect(a, new Vector2(w, h), newColor);
                Drawing.DrawRect(a, new Vector2(w, h), new Color(0, 0, 0, 255), true);
                Drawing.DrawText(des, a + new Vector2(w + 10, 0), Color.White, FontFlags.AntiAlias | FontFlags.DropShadow);
            }
            else
            {
                Drawing.DrawRect(a, new Vector2(w, h), Color.Gray);
                Drawing.DrawRect(a, new Vector2(w, h), new Color(0, 0, 0, 255), true);
            }
        }
*/
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