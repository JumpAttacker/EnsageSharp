using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using SharpDX;

namespace WindRunnerAnnihilation
{
    internal class ParticleMasterOnTimer
    {
        public Unit MaintTarget;
        public ParticleEffect Effect;

        public ParticleMasterOnTimer(Unit target,  ParticleEffect effect)
        {
            MaintTarget = target;
            Effect = effect;
        }
    }
    internal class Program
    {
        private static bool _loaded;
        private static readonly string Ver = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        private const int WmKeyup = 0x0101;
        private static bool _leftMouseIsPress;
        private static bool _enabled;
        private static bool _lastStateAction;
        private static List<Unit> _creeps = new List<Unit>();
        //private static readonly Dictionary<string, ParticleEffect> Effects = new Dictionary<string, ParticleEffect>();
        private static ParticleEffect _bestPosEff;
        private static Hero _globalTarget;
        private static Vector2 _sizer = new Vector2(265, 300);
        //============================================================
        private static ulong _myKey = 'G';
        private static bool _timetochange;
        private static bool _showMenu = true;
        private static bool _useultimate=true;
        private static bool _nearestPoint=true;
        private static readonly Dictionary<Unit, ParticleMasterOnTimer> EffectMaster = new Dictionary<Unit, ParticleMasterOnTimer>();
        private static bool _shackleshotHelper;
        private static bool _shackleshotHelperWithEffects = true;
        public static string WrEffect = "particles/items_fx/electrical_arc_01_cp0.vpcf";
        //private static Vector3 ASD;

        private static void Main()
        {
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
            Game.OnWndProc += Game_OnWndProc;
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            var player = ObjectMgr.LocalPlayer;
            if (player == null || player.Team == Team.Observer || !_loaded)
            {
                return;
            }
            if (ObjectMgr.LocalHero.ClassID != ClassID.CDOTA_Unit_Hero_Windrunner) return;
            var startPos = new Vector2(50, 200);
            var maxSize = new Vector2(120, 200);
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

                DrawButton(startPos + new Vector2(10, 10), 100, 20, ref _useultimate, true,
                    new Color(0, 200, 150),
                    new Color(200, 0, 0, 100), "Use ultimate");
                DrawButton(startPos + new Vector2(10, 35), 100, 20, ref _nearestPoint, true,
                    new Color(0, 200, 150),
                    new Color(200, 0, 0, 100), "Nearest");
                DrawButton(startPos + new Vector2(10, 60), 100, 20, ref _shackleshotHelperWithEffects, true,
                    new Color(0, 200, 150),
                    new Color(200, 0, 0, 100), "ShacleHelper");
                DrawButton(startPos + new Vector2(10, 85), 100, 20, ref _shackleshotHelper, true,
                    new Color(0, 200, 150),
                    new Color(200, 0, 0, 100), "AutoShacle");
                
                DrawButton(startPos + new Vector2(10, _sizer.Y - 70), 100, 20, ref _timetochange, true,
                    new Color(0, 200, 150),
                    new Color(200, 0, 0, 100), "Change Hotkey");
                Drawing.DrawText(
                    string.Format("Status: [{0}]", _enabled ? "ON" : "OFF"),
                    startPos + new Vector2(10, _sizer.Y - 35), Color.White,
                    FontFlags.AntiAlias | FontFlags.DropShadow);
                Drawing.DrawText(string.Format("ComboKey {0}", (char)_myKey),
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
        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (Game.IsChatOpen)
                return;
            if (_timetochange && args.Msg == WmKeyup && args.WParam >= 0x41 && args.WParam <= 0x5A)
            {
                _timetochange = false;
                _myKey = args.WParam;
                return;
            }
            if (args.WParam == _myKey)
            {
                _enabled = args.Msg != WmKeyup;
                if (_enabled != _lastStateAction)
                {
                    _lastStateAction = _enabled;
                    Game.ExecuteCommand(string.Format("dota_player_units_auto_attack_after_spell {0}", _enabled ? 0 : 1));
                    if (_enabled)
                    {
                        ObjectMgr.LocalHero.Stop();
                    }
                }
                if (!_enabled)
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

        

        private static void Game_OnUpdate(EventArgs args)
        {
            var me = ObjectMgr.LocalHero;
            if (!_loaded)
            {
                if (!Game.IsInGame || me == null || me.ClassID != ClassID.CDOTA_Unit_Hero_Windrunner)
                {
                    return;
                }
                _loaded = true;

                PrintSuccess(string.Format("> WindRunner Annihilation Loaded v{0}", Ver));
            }
            if (!Game.IsInGame || me == null)
            {
                _loaded = false;
                PrintInfo("> WindRunner unLoaded");
                return;
            }
            try
            {
                foreach (var f in EffectMaster.ToList())
                {
                    var hero = f.Key;
                    var dick = f.Value;
                    var mainTarget = dick.MaintTarget;
                    if (!hero.IsValid || !mainTarget.IsValid) EffectMaster.Remove(f.Key);
                    var angle = (float)(Math.Max(
                            Math.Abs(me.FindAngleBetween(hero.Position, true) - (me.FindAngleBetween(mainTarget.Position, true))) - .19, 0));
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (angle != 0 || me.Distance2D(mainTarget) <= me.Distance2D(hero) || !hero.IsAlive || !mainTarget.IsAlive)
                    {
                        if (dick.Effect != null)
                            dick.Effect.Dispose();
                        EffectMaster.Remove(f.Key);
                    }
                    else if (Utils.SleepCheck("cd " + dick.Effect.GetHashCode()))
                    {

                        EffectMaster.Remove(f.Key);
                        var eff = new ParticleEffect(WrEffect, hero, ParticleAttachment.WorldOrigin);
                        Utils.Sleep(500, "cd " + eff.GetHashCode());
                        //dick.Effect.Restart();
                        EffectMaster.Add(hero, new ParticleMasterOnTimer(mainTarget, eff));
                    }
                }
            }
            catch (Exception)
            {
                PrintError("error #2");
            }
            
            var shackleshot = me.Spellbook.Spell1;
            if (_shackleshotHelperWithEffects)
            {
                try
                {
                    var effectTarget = ClosestToMouse(me, 500);
                    if (effectTarget != null && effectTarget.IsValidTarget())
                        FindBestPosition(me, effectTarget, shackleshot, true);
                }
                catch (Exception)
                {
                    
                    PrintError("error #1");
                }
                
            }
            if (!_enabled)
            {
                if (_bestPosEff != null)
                    _bestPosEff.Dispose();
                return;
            }
            if (!Utils.SleepCheck("Dadzger") || !me.IsAlive)
                return;
            if (_globalTarget == null)
            {
                _globalTarget = ClosestToMouse(me, 500);
            }
            if (_globalTarget == null || !_globalTarget.IsValid || !_globalTarget.IsAlive || !me.CanCast()) return;

            var dagger = me.FindItem("item_blink");
            //var forsestaff = me.FindItem("item_force_staff");
            
            var ultimate = me.Spellbook.Spell4;
            if (_globalTarget == null) return;
            //PrintInfo("target: "+target.Name);
            var bestposition = FindBestPosition(me, _globalTarget,shackleshot);
            //ASD = bestposition;
            if (!bestposition.IsZero)
            {
                if (_bestPosEff == null || _bestPosEff.IsDestroyed)
                    _bestPosEff = new ParticleEffect(@"particles\ui_mouseactions\range_display.vpcf", bestposition);
                _bestPosEff.SetControlPoint(1, new Vector3(50, 0, 0));
                _bestPosEff.SetControlPoint(0, bestposition);
            }
            else
            {
                if (_bestPosEff != null)
                    _bestPosEff.Dispose();
            }
            if (!shackleshot.CanBeCasted() || bestposition.IsZero) return;
            var dist = me.Distance2D(bestposition);
            if (dist <= 1100)
            {
                if (dagger != null && dagger.CanBeCasted())
                    dagger.UseAbility(bestposition);
                //else if (dist <= 50)
                //{
                shackleshot.UseAbility(_globalTarget,true);
                if (_useultimate && ultimate != null && ultimate.CanBeCasted())
                {
                    ultimate.UseAbility(_globalTarget,true);
                }
                else
                {
                    me.Attack(_globalTarget,true);
                }
                Utils.Sleep(250, "Dadzger");
                /*}
                else
                {
                    me.Move(bestposition);
                    Utils.Sleep(250, "Dadzger");
                }*/

            }
            else if (me.CanMove() && Utils.SleepCheck("Move"))
            {
                me.Move(bestposition);
                Utils.Sleep(250, "Move");
            }
        }

        

        private static Vector3 FindBestPosition(Hero me, Hero target, Ability shackleshot, bool helper=false)
        {

            var tartgetPos = target.Position;
            var returnPointUnit = new Vector3();
            var returnPointTree = new Vector3();
            if (target.NetworkActivity == NetworkActivity.Move)
            {
                tartgetPos = Prediction.InFront(target,
                    target.MovementSpeed*(me.Distance2D(target)/1515));
            }
            _creeps =
                ObjectMgr.GetEntities<Unit>()
                    .Where(
                        x =>
                            x.Distance2D(target) <= 525 && x.Team != me.Team && !x.IsIllusion && x.IsAlive &&
                            x.IsVisible && !x.IsMagicImmune() && !Equals(x, target))
                    .ToList();
            List<Entity> trees = null;
            if (!helper)
            {
                trees = ObjectMgr.GetEntities<Entity>()
                    .Where(x => x.Name == "ent_dota_tree" && x.Distance2D(target.Position) < 500 && x.IsAlive)
                    .ToList();
            }
            foreach (var t in _creeps)
            {
                if (!helper)
                {
                    var tpos = t.Position;
                    var a = tpos.ToVector2().FindAngleBetween(tartgetPos.ToVector2(), true);
                    var points = new Dictionary<int, Vector3>();

                    for (var i = 0; i <= 7; i++)
                    {
                        var p = new Vector3(
                            target.Position.X + (150 + 100*i)*(float) Math.Cos(a),
                            target.Position.Y + (150 + 100*i)*(float) Math.Sin(a),
                            target.Position.Z);
                        points.Add(i, p);
                    }

                    GetClosest(ref returnPointUnit, me, points);
                }
                else
                {
                    LetsAddEffects(me,target,t,shackleshot);
                }
                
            }
            if (helper) return new Vector3();
            foreach (var t in trees)
            {
                var tpos = t.Position;
                var a = tpos.ToVector2().FindAngleBetween(tartgetPos.ToVector2(), true);
                var points = new Dictionary<int, Vector3>();
                for (var i = 0; i <= 7; i++)
                {
                    var p = new Vector3(
                        target.Position.X + (150 + 100 * i) * (float)Math.Cos(a),
                        target.Position.Y + (150 + 100 * i) * (float)Math.Sin(a),
                        target.Position.Z);
                    points.Add(i, p);
                }
                GetClosest(ref returnPointTree, me, points);
            }
            Vector3 onExit;
            if (returnPointTree.IsZero)
                onExit = returnPointUnit;
            else if (returnPointUnit.IsZero)
                onExit = returnPointTree;
            else
            {
                if (_nearestPoint)
                    onExit = me.Distance2D(returnPointTree) > me.Distance2D(returnPointUnit)
                        ? returnPointUnit
                        : returnPointTree;
                else
                {
                    onExit = target.Distance2D(returnPointTree) > target.Distance2D(returnPointUnit)
                        ? returnPointUnit
                        : returnPointTree;
                }
            }
            return onExit;
        }

        private static void LetsAddEffects(Hero me, Unit target, Unit t, Ability shackleshot)
        {
            var dist = me.Distance2D(target) >= me.Distance2D(t);
            var angle = (float)(Math.Max(
                    Math.Abs(me.FindAngleBetween(t.Position, true) - (me.FindAngleBetween(target.Position, true))) - .19, 0));
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (angle == 0 && dist)
            {
                if (!EffectMaster.ContainsKey(t))
                    EffectMaster.Add(t, new ParticleMasterOnTimer(target, new ParticleEffect(WrEffect, t, ParticleAttachment.WorldOrigin)));
                if (shackleshot != null && shackleshot.CanBeCasted() && Utils.SleepCheck("shshot") && (_shackleshotHelper))
                {
                    shackleshot.UseAbility(t);
                    Utils.Sleep(250, "shshot");
                }
            }
        }

        private static void GetClosest(ref Vector3 returnPoint, Hero me,Dictionary<int,Vector3> points)
        {
            for (var i = 0; i <= 8; i++)
            {
                Vector3 vec;
                if (!points.TryGetValue(i, out vec)) continue;
                if (!returnPoint.IsZero)
                {
                    if (_nearestPoint)
                    {
                        if (vec.Distance2D(me) <= returnPoint.Distance2D(me))
                            returnPoint = vec;
                    }
                    else
                    {
                        if (vec.Distance2D(_globalTarget) <= returnPoint.Distance2D(_globalTarget))
                            returnPoint = vec;
                    }
                }
                else
                {
                    returnPoint = vec;
                }
            }
        }

        public static Hero ClosestToMouse(Hero source, float range = 1000)
        {
            var mousePosition = Game.MousePosition;
            var enemyHeroes =
                ObjectMgr.GetEntities<Hero>()
                    .Where(
                        x =>
                            x.Team == source.GetEnemyTeam() && !x.IsIllusion && x.IsAlive && x.IsVisible
                            && x.Distance2D(mousePosition) <= range && !x.IsMagicImmune());
            Hero[] closestHero = {null};
            foreach (
                var enemyHero in
                    enemyHeroes.Where(
                        enemyHero =>
                            closestHero[0] == null ||
                            closestHero[0].Distance2D(mousePosition) > enemyHero.Distance2D(mousePosition)))
            {
                closestHero[0] = enemyHero;
            }
            return closestHero[0];
        }
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
    }
}