using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using SharpDX;

namespace Invorker
{
    internal class Program
    {
        #region Members

        private static bool _loaded;
        private const string Ver = "0.1";
        private const int WmKeyup = 0x0101;
        private static bool _leftMouseIsPress;
        private static int _combo;
        private static bool _showMenu = true;
        //private static readonly SpellStruct[] Spells = new SpellStruct[12];
        private static readonly ComboStruct[] Combos = new ComboStruct[5];
        private static bool _inAction;
        private static bool _initNewCombo;
        private static byte _stage;
        private static readonly Dictionary<string, SpellStruct> SpellInfo = new Dictionary<string, SpellStruct>();
        private static byte _balstStage = 1;
        private static Ability _spellForCast;
        private static bool _startInitSpell;
        private static bool SmartSphere=true;
        private static Vector2 _sizer = new Vector2(265, 300);
        private static NetworkActivity LastAct=NetworkActivity.Idle;
        #endregion


        private static void Main()
        {
            Game.OnUpdate += Game_OnUpdate;
            _loaded = false;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnWndProc += Game_OnWndProc;
            Player.OnExecuteOrder += Player_OnExecuteAction;
            /*Drawing.OnPreReset += Drawing_OnPreReset;
            Drawing.OnPostReset += Drawing_OnPostReset;
            Drawing.OnEndScene += Drawing_OnEndScene;
            AppDomain.CurrentDomain.DomainUnload += CurrentDomainDomainUnload;*/
        }
        static void Player_OnExecuteAction(Player sender, ExecuteOrderEventArgs args)
        {
            if (args.Order != Order.AttackTarget && args.Order != Order.MoveLocation && _inAction && !SmartSphere) return;
            var me = sender.Hero;
            var quas = me.Spellbook.SpellQ;
            var wex = me.Spellbook.SpellW;
            var exort = me.Spellbook.SpellE;
            if (args.Order == Order.AttackTarget && me.Distance2D(args.Target)<=650)
            {
                exort.UseAbility();
                exort.UseAbility();
                exort.UseAbility();
                Utils.Sleep(200, "act");
                return;
            }
            /*wex.UseAbility();
            wex.UseAbility();
            wex.UseAbility();*/

        }
        struct ComboStruct
        {
            private readonly bool _isNeedEul;
            private readonly Ability _s1;
            private readonly Ability _s2;
            private readonly Ability _s3;
            private readonly Ability _s4;
            private readonly Ability _s5;

            public ComboStruct(bool useEul, Ability s1, Ability s2, Ability s3, Ability s4, Ability s5)
            {
                _isNeedEul = useEul;
                _s1 = s1;
                _s2 = s2;
                _s3 = s3;
                _s4 = s4;
                _s5 = s5;
            }

            public ComboStruct(Ability s1, Ability s2, Ability s3, Ability s4, Ability s5)
                : this()
            {
                _s1 = s1;
                _s2 = s2;
                _s3 = s3;
                _s4 = s4;
                _s5 = s5;
            }

            public Ability[] GetComboAbilities()
            {
                return new[] { _s1, _s2, _s3, _s4, _s5 };
            }

            public bool CheckEul()
            {
                return _isNeedEul;
            }

            public override string ToString()
            {
                return string.Format("Eul? {0} -> {1} -> {2} -> {3} -> {4}-> {5}", _isNeedEul, _s1.Name,
                    _s2.Name, _s3 != null ? _s3.Name : "", _s4 != null ? _s4.Name : "", _s5 != null ? _s5.Name : "");
            }
        }
        struct SpellStruct
        {
            private readonly Ability _oneAbility;
            private readonly Ability _twoAbility;
            private readonly Ability _threeAbility;

            public SpellStruct(Ability oneAbility, Ability twoAbility, Ability threeAbility)
            {
                _oneAbility = oneAbility;
                _twoAbility = twoAbility;
                _threeAbility = threeAbility;
            }

            public Ability[] GetNeededAbilities()
            {
                return new[] { _oneAbility, _twoAbility, _threeAbility };
            }
        }
        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (Game.IsChatOpen)
                return;
            if (args.WParam == 'G')
            {
                if (Game.IsKeyDown(0x11))
                {
                    _startInitSpell = true;
                }
                else
                {
                    _inAction = args.Msg != WmKeyup;
                    Game.ExecuteCommand(string.Format("dota_player_units_auto_attack_after_spell {0}", _inAction ? 0 : 1));
                    if (!_inAction) _initNewCombo = false;
                }
            }

            if (args.WParam != 1 || !Utils.SleepCheck("clicker"))
            {
                _leftMouseIsPress = false;
                return;
            }
            _leftMouseIsPress = true;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var player = ObjectMgr.LocalPlayer;
            if (player == null || player.Team == Team.Observer)
            {
                return;
            }
            var startPos = new Vector2(10, 200);
            var maxSize = new Vector2(265, 300);


            if (_showMenu)
            {
                _sizer.X += 1;
                _sizer.Y += 1;
                _sizer.X = Math.Min(_sizer.X, maxSize.X);
                _sizer.Y = Math.Min(_sizer.Y, maxSize.Y);

                Drawing.DrawRect(startPos, _sizer, new Color(0, 155, 255, 100));
                Drawing.DrawRect(startPos, _sizer, new Color(0, 0, 0, 255), true);
                Drawing.DrawRect(startPos + new Vector2(-5, -5), _sizer + new Vector2(10, 10),
                    new Color(0, 0, 0, 255), true);
                DrawButton(startPos + new Vector2(_sizer.X - 20, -20), 20, 20, ref _showMenu, true, Color.Gray,
                    Color.Gray);
                if (!Equals(_sizer, maxSize)) return;
                /*
                Tornado > EMP > Meteor > Blast (Requiers Aghanims)
                Tornado > Meteor > Blast
                Tornado > EMP > Blast
                Tornado > EMP > Ica Wall
                 */
                DrawButton(startPos + new Vector2(10, 10), 50, 50, 0, true, new Color(0, 200, 150),
                    new Color(200, 0, 0, 100), "Eul > SS > Meteor > Blast > cSnap > fg");
                DrawButton(startPos + new Vector2(10, 60), 50, 50, 1, true, new Color(0, 200, 150),
                    new Color(200, 0, 0, 100), "Tornado > EMP > Meteor > Blast");
                DrawButton(startPos + new Vector2(10, 110), 50, 50, 2, true, new Color(0, 200, 150),
                    new Color(200, 0, 0, 100), "Tornado > Meteor > Blast");
                DrawButton(startPos + new Vector2(10, 160), 50, 50, 3, true, new Color(0, 200, 150),
                    new Color(200, 0, 0, 100), "Tornado > EMP > Blast");
                DrawButton(startPos + new Vector2(10, 210), 50, 50, 4, true, new Color(0, 200, 150),
                    new Color(200, 0, 0, 100), "Tornado > EMP > Ica Wall");
                DrawButton(startPos + new Vector2(_sizer.X-22, _sizer.Y-22), 20, 20, ref SmartSphere, true, Color.Green,
                    Color.Red);
                var spellName = "empty";
                if (_inAction && _spellForCast != null)
                    spellName = _spellForCast.NetworkName.Substring(_spellForCast.NetworkName.LastIndexOf('_') + 1);
                Drawing.DrawText(
                    string.Format("Status: [{0}] Current Spell [{1}] ", _inAction ? "ON" : "OFF", spellName),
                    startPos + new Vector2(10, 280), Color.White,
                    FontFlags.AntiAlias | FontFlags.DropShadow);
                Drawing.DrawText(
                    "ComboKey [G] PrepareKey [ctrl+G] ",
                    startPos + new Vector2(10, 265), Color.White,
                    FontFlags.AntiAlias | FontFlags.DropShadow);

            }
            else
            {
                _sizer.X -= 1;
                _sizer.Y -= 1;
                _sizer.X = Math.Max(_sizer.X, 20);
                _sizer.Y = Math.Max(_sizer.Y, 0);
                Drawing.DrawRect(startPos, _sizer, new Color(0, 0, 0, 255), true);
                /*Drawing.DrawRect(startPos + new Vector2(-5, -5), Sizer + new Vector2(10, 10),
                    new Color(0, 0, 0, 255), true);*/
                DrawButton(startPos + new Vector2(_sizer.X - 20, -20), 20, 20, ref _showMenu, true, Color.Gray,
                    Color.Gray);
                /*
                DrawButton(startPos, 25, 25, ref _showMenu, true, new Color(0, 200, 150),
                    new Color(200, 0, 0, 100));*/
            }

        }

        private static void Game_OnUpdate(EventArgs args)
        {
            #region Init

            var me = ObjectMgr.LocalHero;
            if (!_loaded)
            {
                if (!Game.IsInGame || me == null || me.ClassID != ClassID.CDOTA_Unit_Hero_Invoker)
                {
                    return;
                }
                _loaded = true;
                _combo = 0;


                var q = me.Spellbook.SpellQ;
                var w = me.Spellbook.SpellW;
                var e = me.Spellbook.SpellE;

                var ss = me.FindSpell("invoker_sun_strike");
                var coldsnap = me.FindSpell("invoker_cold_snap");
                var ghostwalk = me.FindSpell("invoker_ghost_walk");
                var icewall = me.FindSpell("invoker_ice_wall");
                var tornado = me.FindSpell("invoker_tornado");
                var deafblast = me.FindSpell("invoker_deafening_blast");
                var forge = me.FindSpell("invoker_forge_spirit");
                var emp = me.FindSpell("invoker_emp");
                var alacrity = me.FindSpell("invoker_alacrity");
                var chaosmeteor = me.FindSpell("invoker_chaos_meteor");

                SpellInfo.Add(ss.Name, new SpellStruct(e, e, e));
                SpellInfo.Add(coldsnap.Name, new SpellStruct(q, q, q));
                SpellInfo.Add(ghostwalk.Name, new SpellStruct(q, q, w));
                SpellInfo.Add(icewall.Name, new SpellStruct(q, q, e));
                SpellInfo.Add(tornado.Name, new SpellStruct(w, w, q));
                SpellInfo.Add(deafblast.Name, new SpellStruct(q, w, e));
                SpellInfo.Add(forge.Name, new SpellStruct(e, e, q));
                SpellInfo.Add(emp.Name, new SpellStruct(w, w, w));
                SpellInfo.Add(alacrity.Name, new SpellStruct(w, w, e));
                SpellInfo.Add(chaosmeteor.Name, new SpellStruct(e, e, w));

                /*
                Spells[Convert.ToInt32(ss.Name)] = new SpellStruct(e, e, e);
                Spells[Convert.ToInt32(coldsnap.Name)] = new SpellStruct( q, q, q);
                Spells[Convert.ToInt32(ghostwalk.Name)] = new SpellStruct( );
                Spells[Convert.ToInt32(icewall.Name)] = new SpellStruct( );
                Spells[Convert.ToInt32(tornado.Name)] = new SpellStruct();
                Spells[Convert.ToInt32(deafblast.Name)] = new SpellStruct();
                Spells[Convert.ToInt32(forge.Name)] = new SpellStruct();
                Spells[Convert.ToInt32(emp.Name)] = new SpellStruct();
                Spells[Convert.ToInt32(alacrity.Name)] = new SpellStruct();
                Spells[Convert.ToInt32(chaosmeteor.Name)] = new SpellStruct(e, e, w);
                 * */
                //Eul->SS->Met->Bla->Cold
                /*
                Tornado > EMP > Meteor > Blast (Requiers Aghanims)
                Tornado > Meteor > Blast
                Tornado > EMP > Blast
                Tornado > EMP > Ica Wall
                 */
                //Combos[0] = new ComboStruct(true, ss, chaosmeteor, deafblast, null, null);
                Combos[0] = new ComboStruct(true, ss, chaosmeteor, deafblast, coldsnap, forge);
                Combos[1] = new ComboStruct(tornado, emp, chaosmeteor, deafblast, coldsnap);
                Combos[2] = new ComboStruct(tornado, chaosmeteor, deafblast, null, null);
                Combos[3] = new ComboStruct(tornado, emp, deafblast, null, null);
                Combos[4] = new ComboStruct(tornado, emp, icewall, null, null);
                PrintSuccess(string.Format("> Invorker Loaded v{0}", Ver));
                PrintInfo("===============Combo selection===============");
                PrintInfo(string.Format("Init new combo--> {0}", Combos[0]));
                PrintInfo(string.Format("Init new combo--> {0}", Combos[1]));
                PrintInfo(string.Format("Init new combo--> {0}", Combos[2]));
                PrintInfo(string.Format("Init new combo--> {0}", Combos[3]));
                PrintInfo(string.Format("Init new combo--> {0}", Combos[4]));
                PrintInfo("============================================");

            }

            if (!Game.IsInGame || me == null)
            {
                _loaded = false;
                PrintInfo("> Invorker unLoaded");
                return;
            }

            if (Game.IsPaused)
            {
                return;
            }
            //PrintInfo(me.NetworkActivity.ToString());
            if (Utils.SleepCheck("act") && !_inAction && SmartSphere)
            {
                var quas = me.Spellbook.SpellQ;
                var wex = me.Spellbook.SpellW;
                var exort = me.Spellbook.SpellE;
                if (wex!=null && wex.CanBeCasted())
                {
                    //if (me.NetworkActivity == NetworkActivity.Attack2 && me.NetworkActivity != LastAct)

                    if ((me.NetworkActivity == NetworkActivity.Attack || me.NetworkActivity == NetworkActivity.Attack2) &&
                        me.NetworkActivity != LastAct)
                    {
                        //exort.UseAbility();
                        //exort.UseAbility();
                        //exort.UseAbility();
                        //Utils.Sleep(me.AttackSpeedValue+2000, "act");
                    }
                    //else if ((me.NetworkActivity == NetworkActivity.Move && me.NetworkActivity != LastAct))
                    else if (me.NetworkActivity == NetworkActivity.Move && me.NetworkActivity != LastAct)
                    {
                        wex.UseAbility();
                        wex.UseAbility();
                        wex.UseAbility();
                    }
                    else if (me.NetworkActivity == NetworkActivity.Idle && me.NetworkActivity != LastAct)
                    {
                        /*quas.UseAbility();
                    quas.UseAbility();
                    quas.UseAbility();
                    Utils.Sleep(150, "act");*/
                    }
                    LastAct = me.NetworkActivity;
                }
            }

            #endregion

            #region Get needed spells

            if (_startInitSpell && Utils.SleepCheck("GettingNeededSpells"))
            {
                _startInitSpell = false;
                SpellStruct s;
                var spell1 = _spellForCast = Combos[_combo].GetComboAbilities()[0];
                var spell2 = _spellForCast = Combos[_combo].GetComboAbilities()[1];
                var active1 = me.Spellbook.Spell4;
                var active2 = me.Spellbook.Spell5;
                if (Equals(spell1, active1) || Equals(spell1, active2))
                {

                }
                else
                {
                    if (SpellInfo.TryGetValue(spell1.Name, out s))
                    {
                        var invoke = me.FindSpell("invoker_invoke");
                        if (invoke.CanBeCasted())
                        {
                            var spells = s.GetNeededAbilities();
                            if (spells[0] != null) spells[0].UseAbility();
                            if (spells[1] != null) spells[1].UseAbility();
                            if (spells[2] != null) spells[2].UseAbility();
                            invoke.UseAbility();
                            Utils.Sleep(Game.Ping + 25, "GettingNeededSpells");
                        }
                    }
                    else
                        try
                        {
                            PrintError("couldnt find data for spell: " + _spellForCast.Name);
                        }
                        catch (Exception)
                        {
                            PrintError("couldnt find data for spell: ERROR");
                        }
                }
                if (Equals(spell2, active1) || Equals(spell2, active2))
                {

                }
                else
                {
                    if (SpellInfo.TryGetValue(spell2.Name, out s))
                    {
                        var invoke = me.FindSpell("invoker_invoke");
                        if (invoke.CanBeCasted())
                        {
                            var spells = s.GetNeededAbilities();
                            if (spells[0] != null) spells[0].UseAbility();
                            if (spells[1] != null) spells[1].UseAbility();
                            if (spells[2] != null) spells[2].UseAbility();
                            invoke.UseAbility();
                            Utils.Sleep(Game.Ping + 25, "GettingNeededSpells");
                        }
                    }
                    else
                        try
                        {
                            PrintError("couldnt find data for spell: " + _spellForCast.Name);
                        }
                        catch (Exception)
                        {
                            PrintError("couldnt find data for spell: ERROR");
                        }
                }
            }

            #endregion

            #region Starting Combo

            var target = ClosestToMouse(me);
            if (_inAction && target != null && target.IsAlive)
                ComboInAction(me, target);

            #endregion

        }



        private static void ComboInAction(Hero me, Hero target)
        {
            #region Init

            var q = me.Spellbook.SpellQ;
            var w = me.Spellbook.SpellW;
            var e = me.Spellbook.SpellE;
            var active1 = me.Spellbook.Spell4;
            var active2 = me.Spellbook.Spell5;

            var invoke = me.FindSpell("invoker_invoke");

            var eul = me.FindItem("item_cyclone");
            var dagger = me.FindSpell("item_blink");

            var ss = me.FindSpell("invoker_sun_strike");
            var coldsnap = me.FindSpell("invoker_cold_snap");
            var ghostwalk = me.FindSpell("invoker_ghost_walk");
            var icewall = me.FindSpell("invoker_ice_wall");
            var tornado = me.FindSpell("invoker_tornado");
            var deafblast = me.FindSpell("invoker_deafening_blast");
            var forge = me.FindSpell("invoker_forge_spirit");
            var emp = me.FindSpell("invoker_emp");
            var alacrity = me.FindSpell("invoker_alacrity");
            var chaosmeteor = me.FindSpell("invoker_chaos_meteor");

            if (!_initNewCombo)
            {
                _initNewCombo = true;
                _stage = 1;
                PrintInfo("Starting new combo! " + string.Format("[{0}]", _combo + 1));
            }
            if (!Utils.SleepCheck("StageCheck")) return;
            #endregion

            /*var modif = target.Modifiers.Where(x=>x.IsDebuff);
            PrintInfo("===========================");
            foreach (var s in modif)
            {
                PrintInfo(s.Name);
            }*/
            switch (_stage)
            {
                case 1:
                    if (Combos[_combo].CheckEul())
                    {
                        if (eul == null || !eul.CanBeCasted(target)) return;
                        eul.UseAbility(target);
                        _stage++;
                        Utils.Sleep(250, "StageCheck");
                    }
                    else
                    {
                        _stage++;
                    }
                    break;
                default:
                    if (Combos[_combo].GetComboAbilities().Length < _stage - 1) return;
                    _spellForCast = Combos[_combo].GetComboAbilities()[_stage - 2];
                    if (_spellForCast != null)
                    {
                        if (!_spellForCast.CanBeCasted())
                        {
                            PrintInfo(string.Format("spell {0} cant be casted, go next [{1}]", _spellForCast.Name, _stage));
                            _stage++;
                            return;
                        }
                        if (Equals(active1, _spellForCast) || Equals(active2, _spellForCast))
                        {
                            if (Combos[_combo].CheckEul())
                            {
                                var eulmodif = target.Modifiers.FirstOrDefault(x => x.Name == "modifier_eul_cyclone" || x.Name == "modifier_invoker_tornado");
                                if (Equals(_spellForCast, deafblast))
                                {
                                    if (eulmodif != null && eulmodif.RemainingTime <= me.Distance2D(target) / 1100 - .1 - Game.Ping / 1000)
                                    {
                                        UseSpell(_spellForCast, target);
                                        _stage++;
                                        Utils.Sleep(250, "StageCheck");
                                    }
                                    else if (eulmodif == null)
                                    {
                                        UseSpell(_spellForCast, target);
                                        _stage++;
                                        Utils.Sleep(250, "StageCheck");
                                    }
                                    /*
                                    if (_balstStage == 1 && Utils.SleepCheck("blast"))
                                    {
                                        Utils.Sleep(me.Distance2D(target)/1100, "blast");
                                        _balstStage = 2;
                                    }
                                    else if (Utils.SleepCheck("blast"))
                                    {
                                        UseSpell(spellForCast, target);
                                        _stage++;
                                        Utils.Sleep(250, "StageCheck");
                                        _balstStage = 1;
                                    }
                                    */
                                }
                                else if (!Equals(_spellForCast, ss) && !Equals(_spellForCast, chaosmeteor))
                                {
                                    UseSpell(_spellForCast, target);
                                    _stage++;
                                    Utils.Sleep(250, "StageCheck");
                                }


                                var timing = (Equals(_spellForCast, ss))
                                    ? 1.7
                                    : (Equals(_spellForCast, chaosmeteor)) ? 1.3 : 0;
                                timing += Game.Ping / 1000 - 0.005;
                                //PrintInfo(timing.ToString(CultureInfo.InvariantCulture));
                                if (eulmodif != null && eulmodif.RemainingTime < timing)
                                {
                                    UseSpell(_spellForCast, target);
                                    _stage++;
                                    Utils.Sleep(250, "StageCheck");
                                }
                            }
                            else
                            {
                                var tornadoMod = target.Modifiers.FirstOrDefault(x => x.Name == "modifier_invoker_tornado");
                                if (Equals(_spellForCast, deafblast))
                                {
                                    if (tornadoMod != null &&
                                        tornadoMod.RemainingTime <= me.Distance2D(target) / 1100 - .1 - Game.Ping / 1000)
                                    {
                                        UseSpell(_spellForCast, target);
                                        _stage++;
                                        Utils.Sleep(250, "StageCheck");
                                    }
                                    else if (tornadoMod == null)
                                    {
                                        UseSpell(_spellForCast, target);
                                        _stage++;
                                        Utils.Sleep(250, "StageCheck");
                                    }
                                }
                                else if (!Equals(_spellForCast, ss) && !Equals(_spellForCast, chaosmeteor))
                                {
                                    UseSpell(_spellForCast, target);
                                    _stage++;
                                    Utils.Sleep(250, "StageCheck");
                                }


                                var timing = (Equals(_spellForCast, ss))
                                    ? 1.7
                                    : (Equals(_spellForCast, chaosmeteor)) ? 1.3 : 0;
                                timing += Game.Ping / 1000 - 0.005;
                                //PrintInfo(timing.ToString(CultureInfo.InvariantCulture));
                                if (tornadoMod != null && tornadoMod.RemainingTime < timing)
                                {
                                    UseSpell(_spellForCast, target);
                                    _stage++;
                                    Utils.Sleep(250, "StageCheck");
                                }
                                /*
                                if (Equals(spellForCast, deafblast))
                                {
                                    if (_balstStage == 1 && Utils.SleepCheck("blast"))
                                    {
                                        Utils.Sleep(me.Distance2D(target) / 1100, "blast");
                                        _balstStage = 2;
                                    }
                                    else if (Utils.SleepCheck("blast"))
                                    {
                                        UseSpell(spellForCast, target);
                                        _stage++;
                                        Utils.Sleep(250, "StageCheck");
                                        _balstStage = 1;
                                    }
                                }
                                else
                                {
                                    UseSpell(spellForCast, target);
                                    _stage++;
                                    Utils.Sleep(250, "StageCheck");
                                }*/
                            }

                        }
                        else
                        {
                            SpellStruct s;
                            if (SpellInfo.TryGetValue(_spellForCast.Name, out s))
                            {
                                if (invoke.CanBeCasted())
                                {
                                    var spells = s.GetNeededAbilities();
                                    if (spells[0] != null) spells[0].UseAbility();
                                    if (spells[1] != null) spells[1].UseAbility();
                                    if (spells[2] != null) spells[2].UseAbility();
                                    invoke.UseAbility();
                                    Utils.Sleep(Game.Ping + 25, "StageCheck");
                                }
                            }
                            else
                                try
                                {
                                    PrintError("couldnt find data for spell: " + _spellForCast.Name);
                                }
                                catch (Exception)
                                {
                                    PrintError("couldnt find data for spell: ERROR");
                                }
                        }
                    }
                    break;
            }
        }
        private static void UseSpell(Ability spellForCast, Hero target)
        {
            if (spellForCast.CanBeCasted(target))
            {
                spellForCast.UseAbility(target);
            }
            spellForCast.UseAbility();
            spellForCast.UseAbility(target.Position);
        }

        public static Hero ClosestToMouse(Hero source, float range = 400)
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
        private static void DrawButton(Vector2 a, float w, float h, ref bool clicked, bool isActive, Color @on, Color off)
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
