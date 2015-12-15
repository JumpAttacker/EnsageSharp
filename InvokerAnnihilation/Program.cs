using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using SharpDX;

namespace InvokerAnnihilation
{
    internal static class Program
    {
        #region Members
        private static readonly Menu Menu = new Menu("Invoker Annihilation", "InvokerAnnihilation", true, "npc_dota_hero_invoker", true);
        private static bool _loaded;
        private static readonly string Ver = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        private const int WmKeyup = 0x0101;
        private static int _combo;
        //private static readonly SpellStruct[] Spells = new SpellStruct[12];
        private static readonly ComboStruct[] Combos = new ComboStruct[11];
        private static int _maxCombo;
        private static bool _inAction;
        private static bool _initNewCombo;
        private static byte _stage;
        private static readonly Dictionary<string, SpellStruct> SpellInfo = new Dictionary<string, SpellStruct>();
        private static ParticleEffect _predictionEffect;
        private static Ability _spellForCast;
        private static bool _startInitSpell;
        private static NetworkActivity _lastAct=NetworkActivity.Idle;
        private static bool _lastAction;
        //============================================================
        //============================================================
        private static ulong _myKey='G';
        private static bool _timetochange;
        private static bool _sunstrikekill;
        //============================================================
        private static Hero _globalTarget;
        //============================================================
        private enum SmartSphereEnum
        {
           Quas=0,Wex=1,Exort=2 
        }
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
            var sunStrikeSettings=new Menu("Sun Strike Settings","ssSettings");
            /*sunStrikeSettings.AddItem(
                new MenuItem("hotkey", "Hotkey").SetValue(new KeyBind('T', KeyBindType.Press))
                    .SetTooltip("press hotkey for auto SunStrike"));
            sunStrikeSettings.AddItem(new MenuItem("ssShift", "Use Shift With Hotkey").SetValue(true));*/
            sunStrikeSettings.AddItem(new MenuItem("ssDamageontop", "Show Damage on Top Panel").SetValue(false));
            sunStrikeSettings.AddItem(new MenuItem("ssDamageonhero", "Show Damage on Hero").SetValue(false));
            sunStrikeSettings.AddItem(new MenuItem("ssPrediction", "Show Prediction").SetValue(false));
            


            var combo = new Menu("Combos", "combos");
            combo.AddItem(
                new MenuItem("hotkeyPrev", "Previous Combo").SetValue(new KeyBind(0x6B, KeyBindType.Press))
                    .SetTooltip("default hotkey is notepad [+]"));
            combo.AddItem(
                new MenuItem("hotkeyNext", "Next Combo").SetValue(new KeyBind(0x6D, KeyBindType.Press))
                    .SetTooltip("default hotkey is notepad [-]"));
            combo.AddItem(new MenuItem("ShowComboMenu", "Show Combo Menu").SetValue(true));
            //combo.AddItem(new MenuItem("ShowCurrentCombo", "Show Current Combo").SetValue(true));



            var showComboMenuPos = new Menu("Combo menu position", "ShowComboMenuPos");
            showComboMenuPos.AddItem(
                new MenuItem("MenuPosX", "Pos X").SetValue(new Slider(100, 0, 3000)));
            showComboMenuPos.AddItem(
                new MenuItem("MenuPosY", "Pos Y").SetValue(new Slider(100, 0, 3000)));



            /*var showCurrentCombo = new Menu("Current Combo position", "showCurrentCombo");
            showCurrentCombo.AddItem(
                new MenuItem("MenuPosX", "Extra Pos X").SetValue(new Slider(0, -2500, 2500)));
            showCurrentCombo.AddItem(
                new MenuItem("MenuPosY", "Extra Pos Y").SetValue(new Slider(0, -2500, 2500)));
            */
            var smart = new Menu("Smart Sphere", "Smart");

            /*var onmov = new Dictionary<string, bool>()
            {
                {"invoker_exort",false},
                {"invoker_wex",false},
                {"invoker_quas",true}
            };
            var onatt = new Dictionary<string, bool>()
            {
                {"invoker_exort",false},
                {"invoker_wex",false},
                {"invoker_quas",true}
            };*/
            smart.AddItem(new MenuItem("smartIsActive", "Is Active").SetValue(true));
            smart.AddItem(new MenuItem("OnMoving", "On moving").SetValue(new StringList(new[] {"quas", "wex"})));
            smart.AddItem(
                new MenuItem("OnAttacking", "On attacking").SetValue(new StringList(new[] {"quas", "wex", "exort"}, 2)));

            //om.ValueChanged += OnMoveChange;
            //ot.ValueChanged += OnAttackChange;

            var items = new Dictionary<string, bool>()
            {
                {"item_blink",true},
                {"item_refresher",true},
                {"item_orchid",true},
                {"item_sheepstick",true}
            };
            var settings = new Menu("Settings", "Settings");
            settings.AddItem(new MenuItem("items", "Items:").SetValue(new AbilityToggler(items)));
            settings.AddItem(new MenuItem("moving", "MoveToEnemy").SetValue(true).SetTooltip("while combing"));


            combo.AddSubMenu(showComboMenuPos);
            //combo.AddSubMenu(showCurrentCombo);
            Menu.AddSubMenu(settings);
            Menu.AddSubMenu(smart);
            Menu.AddSubMenu(sunStrikeSettings);
            Menu.AddSubMenu(combo);
            Menu.AddToMainMenu();
        }

/*
        private static void OnAttackChange(object sender, OnValueChangeEventArgs onValueChangeEventArgs)
        {
            var oldValue = onValueChangeEventArgs.GetOldValue<AbilityToggler>();
            var oldQ = oldValue.IsEnabled("invoker_quas");
            var oldW = oldValue.IsEnabled("invoker_wex");
            var oldE = oldValue.IsEnabled("invoker_exort");

            var newValue = onValueChangeEventArgs.GetNewValue<AbilityToggler>();
            var newQ = newValue.IsEnabled("invoker_quas");
            var newW = newValue.IsEnabled("invoker_wex");
            var newE = newValue.IsEnabled("invoker_exort");
            Game.PrintMessage(string.Format("old q {0} w {1} e {2}. new q {3} w {4} e {5}", oldQ,oldW,oldE,newQ,newW,newE), MessageType.ChatMessage);
            if (newQ != oldQ)
            {
                Game.PrintMessage("q",MessageType.ChatMessage);
                var onatt = new Dictionary<string, bool>()
                {
                    {"invoker_exort", false},
                    {"invoker_wex", false},
                    {"invoker_quas", true}
                };
                Menu.Item("OnAttacking").SetValue(new AbilityToggler(onatt));
            }
            if (newW != oldW)
            {
                Game.PrintMessage("w", MessageType.ChatMessage);
                var onatt = new Dictionary<string, bool>()
                {
                    {"invoker_exort", false},
                    {"invoker_wex", true},
                    {"invoker_quas", false}
                };
                Menu.Item("OnAttacking").SetValue(new AbilityToggler(onatt));
            }
            if (newE != oldE)
            {
                Game.PrintMessage("e", MessageType.ChatMessage);
                var onatt = new Dictionary<string, bool>()
                {
                    {"invoker_exort", true},
                    {"invoker_wex", false},
                    {"invoker_quas", false}
                };
                Menu.Item("OnAttacking").SetValue(new AbilityToggler(onatt));
            }
        }
*/

/*
        private static void OnMoveChange(object sender, OnValueChangeEventArgs onValueChangeEventArgs)
        {
        }
*/

        private static void Player_OnExecuteAction(Player sender, ExecuteOrderEventArgs args)
        {
            if (!Menu.Item("smartIsActive").GetValue<bool>()) return;
            if (args.Order != Order.AttackTarget && args.Order != Order.MoveLocation && _inAction) return;
            var me = sender.Hero;
            Ability spell = null;
            switch (Menu.Item("OnAttacking").GetValue<StringList>().SelectedIndex)
            {
                case (int)SmartSphereEnum.Quas:
                    spell = me.Spellbook.SpellQ;
                    break;
                case (int)SmartSphereEnum.Wex:
                    spell = me.Spellbook.SpellW;
                    break;
                case (int)SmartSphereEnum.Exort:
                    spell = me.Spellbook.SpellE;
                    break;
            }
            if (!me.IsAlive || args.Order != Order.AttackTarget || !(me.Distance2D(args.Target) <= 650)) return;
            if (spell == null || !spell.CanBeCasted()) return;
            spell.UseAbility();
            spell.UseAbility();
            spell.UseAbility();
            Utils.Sleep(200, "act");
        }

        private static int _maxComboSpells;
        private struct ComboStruct
        {
            private readonly bool _isNeedEul;
            private readonly Ability []_spells;
            private readonly int _refreshPos;
            /*private readonly Ability _s1;
            private readonly Ability _s2;
            private readonly Ability _s3;
            private readonly Ability _s4;
            private readonly Ability _s5;
            private readonly Ability _s6;*/
            /// <summary>
            /// add new combo to sys
            /// </summary>
            /// <param name="spells">array of spells</param>
            /// <param name="refreshPos">min spell pos for refresher (type -1 to disable refresher)</param>
            /// <param name="useEul">use uel in this combo</param>
            public ComboStruct(Ability[] spells, int refreshPos,bool useEul=false)
            {
                _isNeedEul = useEul;
                _spells = spells;
                _maxCombo++;
                _refreshPos = refreshPos;
                _maxComboSpells = Math.Max(_spells.Length, _maxComboSpells);
            }
            /*public ComboStruct(bool useEul, Ability s1, Ability s2, Ability s3 = null, Ability s4 = null, Ability s5 = null, Ability s6=null)
            {
                _isNeedEul = useEul;
                _s1 = s1;
                _s2 = s2;
                _s6 = s6;
                _s3 = s3;
                _s4 = s4;
                _s5 = s5;
                _s6 = s6;
                _maxCombo++;
            }

            public ComboStruct(Ability s1, Ability s2, Ability s3 = null, Ability s4 = null, Ability s5 = null, Ability s6 = null)
                : this()
            {
                _s1 = s1;
                _s2 = s2;
                _s6 = s6;
                _s3 = s3;
                _s4 = s4;
                _s5 = s5;
                _s6 = s6;
                _maxCombo++;
            }*/
            public int GetRefreshPos()
            {
                return _refreshPos;
            }
            public int GetSpellsInCombo()
            {
                return _spells.Length;
            }
            public Ability[] GetComboAbilities()
            {
                //return new[] { _s1, _s2, _s3, _s4, _s5, _s6 };
                return _spells;
            }

            public bool CheckEul()
            {
                return _isNeedEul;
            }

            public override string ToString()
            {
                var s=new StringBuilder();
                foreach (var ability in _spells)
                {
                    s.AppendLine(ability.Name);
                }
                return s.ToString();
            }
        }

        private struct SpellStruct
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
            if (Game.IsKeyDown(0x11) && args.WParam == 'T')
            {
                _sunstrikekill = args.Msg != WmKeyup;
            }
            if (_timetochange && args.Msg == WmKeyup && args.WParam >= 0x41 && args.WParam <= 0x5A)
            {
                _timetochange = false;
                _myKey = args.WParam;
            }
            if (args.WParam == _myKey)
            {
                if (Game.IsKeyDown(0x11))
                {
                    _startInitSpell = true;
                }
                else
                {
                    _inAction = args.Msg != WmKeyup;
                    if (_inAction != _lastAction)
                        Game.ExecuteCommand($"dota_player_units_auto_attack_after_spell {(_inAction ? 0 : 1)}");
                    if (!_inAction)
                    {
                        _stage = 0;
                        _initNewCombo = false;
                    }
                    _lastAction = _inAction;
                }
            }
            if (args.WParam == Menu.Item("hotkeyPrev").GetValue<KeyBind>().Key && args.Msg == WmKeyup)
            {
                _combo = _combo == _maxCombo - 1 ? _combo = 0 : _combo + 1;
                args.Process = false;
            }
            if (args.WParam == Menu.Item("hotkeyNext").GetValue<KeyBind>().Key && args.Msg == WmKeyup)
            {
                _combo = _combo == 0 ? _combo = _maxCombo - 1 : _combo - 1;
                args.Process = false;
            }
        }

        private static Vector2 _size = new Vector2(HUDInfo.GetHPBarSizeX() / 4, HUDInfo.GetHPBarSizeX() / 4);
        private static void Drawing_OnDraw(EventArgs args)
        {
            var player = ObjectMgr.LocalPlayer;
            if (player == null || player.Team == Team.Observer || !_loaded)
            {
                return;
            }
            var me = player.Hero;

            #region SS ACTION

            var exort = me.Spellbook.SpellE;
            var topDamage = Menu.Item("ssDamageontop").GetValue<bool>();
            var heroDamage = Menu.Item("ssDamageonhero").GetValue<bool>();
            var predDamage = Menu.Item("ssPrediction").GetValue<bool>();
            if (exort != null && exort.Level>0 && (topDamage || heroDamage || predDamage))
            {
                var damage = 100 + 62.5*(exort.Level - 1);
                if (topDamage)
                {
                    var enemy = ObjectMgr.GetEntities<Hero>()
                        .Where(x => x.IsAlive && x.Team != me.Team && !x.IsIllusion);
                    if (me.AghanimState())
                    {
                        damage += 62.5;
                    }
                    foreach (var hero in enemy)
                    {
                        Drawing.DrawText((hero.Health - damage).ToString(CultureInfo.InvariantCulture),
                            HUDInfo.GetTopPanelPosition(hero) + new Vector2(0, 90), Color.White,
                            FontFlags.AntiAlias | FontFlags.DropShadow);
                    }
                }
                var target = ClosestToMouse(me);
                if (target != null && target.IsValid)
                {
                    if (heroDamage)
                    {
                        try
                        {
                            Drawing.DrawText((target.Health - damage).ToString(CultureInfo.InvariantCulture),
                                HUDInfo.GetHPbarPosition(target) + new Vector2(0, -35), Color.White,
                                FontFlags.AntiAlias | FontFlags.DropShadow);
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                    var predVector3 = target.NetworkActivity == NetworkActivity.Move
                        ? Prediction.InFront(target, (float)(target.MovementSpeed * 1.7 + Game.Ping / 1000))
                        : target.Position;
                    if (predDamage)
                    {
                        if (_predictionEffect == null)
                        {
                            _predictionEffect = new ParticleEffect(@"particles\ui_mouseactions\range_display.vpcf",
                                predVector3);
                            _predictionEffect.SetControlPoint(1, new Vector3(175, 0, 0));
                        }
                        _predictionEffect.SetControlPoint(0, predVector3);
                    }
                    else
                    {
                        if (_predictionEffect != null)
                        {
                            _predictionEffect.Dispose();
                            _predictionEffect = null;
                        }
                    }
                    if (_sunstrikekill && Utils.SleepCheck("SunStrike"))
                    {
                        var sunstrike = me.FindSpell("invoker_sun_strike");
                        var active1 = me.Spellbook.Spell4;
                        var active2 = me.Spellbook.Spell5;
                        if (active2 != null && active1 != null && sunstrike != null && (Equals(sunstrike, active1) || Equals(sunstrike, active2)))
                        {
                            if (sunstrike.CanBeCasted())
                            {
                                sunstrike.UseAbility(predVector3);
                                Utils.Sleep(250, "SunStrike");
                            }
                        }
                    }
                }
                else
                {
                    if (_predictionEffect != null)
                    {
                        _predictionEffect.Dispose();
                        _predictionEffect = null;
                    }
                }
            }

            #endregion

            #region Menu
            if (!Menu.Item("ShowComboMenu").GetValue<bool>()) return;
            var pos = new Vector2(Menu.Item("MenuPosX").GetValue<Slider>().Value, Menu.Item("MenuPosY").GetValue<Slider>().Value);
            var size2 = new Vector2(_size.X * _maxComboSpells + 10/*150 * Percent*/, 10 + _maxCombo * (_size.Y + 2)/*250 * Percent*/);

            Drawing.DrawRect(pos, size2, new Color(0, 0, 0, 100));
            Drawing.DrawRect(pos, size2, new Color(0, 155, 255, 255), true);
            var i = 0;

            //var max = Combos.Length;
            foreach (var comboStruct in Combos)
            {
                var sizeY = 4 + i*(_size.Y + 2);
                var eul = comboStruct.CheckEul();
                var texturename = "materials/ensage_ui/items/cyclone.vmat";
                var selected = _combo == i;
                Vector2 itemStartPos;
                if (eul)
                {
                    itemStartPos = pos + new Vector2(10, sizeY);
                    
                    Drawing.DrawRect(
                        itemStartPos,
                        new Vector2((float)(_size.X + _size.X * 0.16), _size.Y),
                        Drawing.GetTexture(texturename));
                    if (_stage == 1 && selected)
                        Drawing.DrawRect(
                            itemStartPos + new Vector2(1, 1),
                            new Vector2(_size.X - 6, _size.Y-2),
                            Color.Red, true);

                }
                var j = 0;//eul ? 0 : 0;
                var legal = 1;
                //PrintInfo(i.ToString());
                //PrintInfo(i+": "+Combos[i]);
                for (; j <= Combos[i].GetSpellsInCombo(); j++)
                {
                    try
                    {
                        texturename = $"materials/ensage_ui/spellicons/{comboStruct.GetComboAbilities()[j].Name}.vmat";
                        var sizeX = _size.X * (j + (eul?1:0)) + 10;
                        itemStartPos = pos + new Vector2(sizeX, sizeY);
                        Drawing.DrawRect(
                            itemStartPos,
                            new Vector2(_size.X-6, _size.Y),
                            Drawing.GetTexture(texturename));
                        legal++;
                        if (Equals(comboStruct.GetComboAbilities()[j], Combos[_combo].GetComboAbilities()[_stage-2]) && selected)
                        {
                            Drawing.DrawRect(
                                itemStartPos,
                                new Vector2(_size.X - 3, _size.Y),
                                Color.Red, true);
                        }
                        
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
                if (selected)
                {
                    Drawing.DrawRect(
                        pos + new Vector2(10, sizeY),
                        new Vector2(_size.X * (legal - (eul ? 0 : 1)) - 6, _size.Y),
                        Color.YellowGreen, true);
                }
                i++;
            }
            #endregion

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
                var blast = me.FindSpell("invoker_deafening_blast");
                var forgeSpirit = me.FindSpell("invoker_forge_spirit");
                var emp = me.FindSpell("invoker_emp");
                var alacrity = me.FindSpell("invoker_alacrity");
                var meteor = me.FindSpell("invoker_chaos_meteor");

                SpellInfo.Add(ss.Name, new SpellStruct(e, e, e));
                SpellInfo.Add(coldsnap.Name, new SpellStruct(q, q, q));
                SpellInfo.Add(ghostwalk.Name, new SpellStruct(q, q, w));
                SpellInfo.Add(icewall.Name, new SpellStruct(q, q, e));
                SpellInfo.Add(tornado.Name, new SpellStruct(w, w, q));
                SpellInfo.Add(blast.Name, new SpellStruct(q, w, e));
                SpellInfo.Add(forgeSpirit.Name, new SpellStruct(e, e, q));
                SpellInfo.Add(emp.Name, new SpellStruct(w, w, w));
                SpellInfo.Add(alacrity.Name, new SpellStruct(w, w, e));
                SpellInfo.Add(meteor.Name, new SpellStruct(e, e, w));

                Combos[_maxCombo] = new ComboStruct(new[] {ss, meteor, blast, coldsnap, forgeSpirit}, 5, true);
                Combos[_maxCombo] =
                    new ComboStruct(new[] {tornado, ss, meteor, blast, tornado, coldsnap, alacrity, forgeSpirit},5);
                Combos[_maxCombo] = new ComboStruct(new[] {tornado, emp, ss, meteor, blast, tornado, coldsnap},5);
                Combos[_maxCombo] = new ComboStruct(new[] {tornado, emp, meteor, blast, coldsnap},5);
                Combos[_maxCombo] = new ComboStruct(new[] {tornado, meteor, blast},3);
                Combos[_maxCombo] = new ComboStruct(new[] {tornado, emp, blast},3);
                Combos[_maxCombo] = new ComboStruct(new[] {tornado, emp, icewall},3);
                Combos[_maxCombo] = new ComboStruct(new[] {tornado, ss, icewall},3);
                Combos[_maxCombo] = new ComboStruct(new[] {tornado, blast, coldsnap},3);
                Combos[_maxCombo] = new ComboStruct(new[] {tornado, emp, coldsnap},3);
                Combos[_maxCombo] = new ComboStruct(new[] {coldsnap, alacrity, forgeSpirit},3);

                Game.PrintMessage(
                    "<font face='Comic Sans MS, cursive'><font color='#00aaff'>" + Menu.DisplayName + " By Jumpering" +
                    " loaded!</font> <font color='#aa0000'>v"  + Ver, MessageType.LogMessage);
                PrintSuccess(string.Format("> {1} Loaded v{0}", Ver, Menu.DisplayName));
                /*PrintInfo("===============Combo selection===============");
                for (var i = 0; i < _maxCombo; i++)
                    PrintInfo(string.Format("Init new combo--> {0}", Combos[i]));
                PrintInfo("============================================");*/
            }

            if (!Game.IsInGame || me == null)
            {
                _loaded = false;
                PrintInfo("> Invorker Annihilation unLoaded");
                return;
            }

            if (Game.IsPaused)
            {
                return;
            }
            //PrintInfo(me.NetworkActivity.ToString());
            if (Utils.SleepCheck("act") && !_inAction && Menu.Item("smartIsActive").GetValue<bool>())
            {
                Ability spell = null;
                switch (Menu.Item("OnMoving").GetValue<StringList>().SelectedIndex)
                {
                    case (int) SmartSphereEnum.Quas:
                        spell = me.Spellbook.SpellQ;
                        break;
                    case (int) SmartSphereEnum.Wex:
                        spell = me.Spellbook.SpellW;
                        break;
                }
                if (me.NetworkActivity == NetworkActivity.Move && me.NetworkActivity != _lastAct && !me.IsInvisible())
                {
                    if (spell != null && spell.CanBeCasted())
                    {
                        spell.UseAbility();
                        spell.UseAbility();
                        spell.UseAbility();
                    }
                }
                _lastAct = me.NetworkActivity;
            }

            #endregion
            
            #region Get needed spells

            if (_startInitSpell && Utils.SleepCheck("GettingNeededSpells"))
            {
                _startInitSpell = false;
                InvokeNeededSpells(me);
            }

            #endregion

            #region Starting Combo

            if (!_inAction)
            {
                _globalTarget = null;
                return;
            }
            if (_globalTarget == null || !_globalTarget.IsValid)
            {
                _globalTarget = ClosestToMouse(me);
            }
            if (_globalTarget == null || !_globalTarget.IsValid || !_globalTarget.IsAlive || !me.CanCast()) return;

            ComboInAction(me, _globalTarget);
            /*
            var target = ClosestToMouse(me);
            if (_ssprediction || _showsSsDamage || _sunstrikekill)
            {
                _globalTarget = target;
            }
            else
            {
                if (_predictionEffect != null)
                {
                    _predictionEffect.Dispose();
                    _predictionEffect = null;
                }
            }
            if (_inAction && target != null && target.IsAlive)
                ComboInAction(me, target);
            */
            #endregion
        }

        private static void InvokeNeededSpells(Hero me)
        {
            var spell1 = _spellForCast = Combos[_combo].GetComboAbilities()[0];
            var spell2 = _spellForCast = Combos[_combo].GetComboAbilities()[1];
            var active1 = me.Spellbook.Spell4;
            var active2 = me.Spellbook.Spell5;
            if ((Equals(spell1, active1) || Equals(spell1, active2)) && (Equals(spell2, active1) || Equals(spell2, active2)))
            {
            }
            else
            {
                SpellStruct s;
                if (Equals(spell1, active2))
                {
                    if (!SpellInfo.TryGetValue(spell1.Name, out s)) return;
                }
                else if (Equals(spell2, active2))
                {
                    if (!SpellInfo.TryGetValue(spell2.Name, out s)) return;
                }
                else if (Equals(spell1, active1) || Equals(spell1, active2))
                {
                    if (!SpellInfo.TryGetValue(spell2.Name, out s)) return;
                }
                else
                {
                    if (!SpellInfo.TryGetValue(spell1.Name, out s)) return;
                }
                var invoke = me.FindSpell("invoker_invoke");
                if (!invoke.CanBeCasted()) return;
                var spells = s.GetNeededAbilities();
                spells[0]?.UseAbility();
                spells[1]?.UseAbility();
                spells[2]?.UseAbility();
                invoke.UseAbility();
                Utils.Sleep(Game.Ping + 50, "GettingNeededSpells");
            }
        }

        private static void ComboInAction(Hero me, Hero target)
        {
            #region Init
            /*
            var q = me.Spellbook.SpellQ;
            var w = me.Spellbook.SpellW;
            var e = me.Spellbook.SpellE;
            var active1 = me.Spellbook.Spell4;
            var active2 = me.Spellbook.Spell5;
            */
            var invoke = me.FindSpell("invoker_invoke");
            
            var eul = me.FindItem("item_cyclone");
            var dagger = me.FindItem("item_blink");
            var refresher = me.FindItem("item_refresher");
            var icewall = me.FindSpell("invoker_ice_wall");
            var deafblast = me.FindSpell("invoker_deafening_blast");
            var hex = me.FindItem("item_sheepstick");
            var orchid = me.FindItem("item_orchid");
            var meteor = me.FindSpell("invoker_chaos_meteor");
            //var emp = me.FindSpell("invoker_emp");
            /*
            var ss = me.FindSpell("invoker_sun_strike");
            var coldsnap = me.FindSpell("invoker_cold_snap");
            var ghostwalk = me.FindSpell("invoker_ghost_walk");
            
            var tornado = me.FindSpell("invoker_tornado");
            
            var forge = me.FindSpell("invoker_forge_spirit");
            
            var alacrity = me.FindSpell("invoker_alacrity");
            
            */
            if (!_initNewCombo)
            {
                _initNewCombo = true;
                _stage = 1;
                PrintInfo("Starting new combo! " + $"[{_combo + 1}] target: {target.Name}");
            }
            if (!Utils.SleepCheck("StageCheck")) return;
            #endregion

            /*var modif = target.Modifiers.Where(x=>x.IsDebuff);
            PrintInfo("===========================");
            foreach (var s in modif)
            {
                PrintInfo(s.Name);
            }*/
            var myBoys = ObjectMgr.GetEntities<Unit>().Where(x => x.Team == me.Team && x.IsControllable && x.IsAlive && Utils.SleepCheck(x.Handle.ToString()));
            foreach (var myBoy in myBoys)
            {
                myBoy.Attack(target);
                Utils.Sleep(300, myBoy.Handle.ToString());
            }
            if (_stage > 2 && !target.IsHexed() && !target.IsStunned())
            {
                if (hex != null && hex.CanBeCasted(target) &&
                    Menu.Item("items").GetValue<AbilityToggler>().IsEnabled(hex.Name) && Utils.SleepCheck("items"))
                {
                    hex.UseAbility(target);
                    Utils.Sleep(300,"items");
                }
                if (orchid != null && orchid.CanBeCasted(target) &&
                    Menu.Item("items").GetValue<AbilityToggler>().IsEnabled(orchid.Name) && Utils.SleepCheck("items"))
                {
                    orchid.UseAbility(target);
                    Utils.Sleep(300,"items");
                }
            }
            if (dagger != null && Menu.Item("items").GetValue<AbilityToggler>().IsEnabled(dagger.Name) &&
                dagger.CanBeCasted() && Utils.SleepCheck("blinker") && me.Distance2D(target)>= 700)
            {
                var dist = 300;
                var angle = me.FindAngleBetween(target.Position, true);
                var point =
                    new Vector3(
                        (float) (target.Position.X -
                                 dist *
                                 Math.Cos(angle)),
                        (float) (target.Position.Y -
                                 dist *
                                 Math.Sin(angle)), 0);
                if (me.Distance2D(target) <= 1150+700)
                    dagger.UseAbility(point);
                else
                    me.Move(target.Position);
                Utils.Sleep(250, "blinker");
            }
            switch (_stage)
            {
                case 1:
                    if (Combos[_combo].CheckEul())
                    {
                        if (eul == null || eul.AbilityState != AbilityState.Ready) return;
                        if (me.Distance2D(target) <= eul.CastRange+50)
                        {
                            eul.UseAbility(target);
                            _stage++;
                            Utils.Sleep(250, "StageCheck");
                        }
                        else if (Utils.SleepCheck("move"))
                        {
                            me.Move(!target.IsValid ? Game.MousePosition : target.Position);
                            Utils.Sleep(250, "move");
                        }
                    }
                    else
                    {
                        _stage++;
                    }
                    break;
                default:
                    if (Combos[_combo].GetComboAbilities().Length < _stage - 1)
                    {
                        me.Attack(target);
                        Utils.Sleep(1000, "StageCheck");
                        return;
                    }
                    _spellForCast = Combos[_combo].GetComboAbilities()[_stage - 2];
                    Ability nextSpell = null;
                    try
                    {
                        nextSpell = Combos[_combo].GetComboAbilities()[_stage - 1];
                    }
                    catch
                    {
                        // ignored
                    }
                    if (nextSpell != null && nextSpell.AbilityState == AbilityState.Ready)
                    {
                        if (Equals(nextSpell, icewall) || Menu.Item("moving").GetValue<bool>())
                        {
                            CastIceWall(me, target, false, icewall);
                        }
                    }
                    if (_spellForCast != null)
                    {
                        if (_spellForCast.AbilityState == AbilityState.Ready)
                        {
                            if (_spellForCast.CanBeCasted())
                            {
                                if (_spellForCast.Name == "invoker_cold_snap")
                                {
                                    if (_spellForCast.CanBeCasted(target))
                                        LetsTryCastSpell(me, target, _spellForCast);
                                }
                                else
                                {
                                    LetsTryCastSpell(me, target, _spellForCast, Equals(nextSpell, deafblast) || Equals(nextSpell, meteor) /*|| Equals(nextSpell, emp)*/);
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
                                        spells[0]?.UseAbility();
                                        spells[1]?.UseAbility();
                                        spells[2]?.UseAbility();
                                        invoke.UseAbility();
                                        Utils.Sleep(100, "StageCheck");
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
                        else
                        {
                            PrintInfo($"spell {_spellForCast.Name} cant be casted, go next [{_stage}]");
                            _stage++;
                            return;
                        }
                    }
                    else
                    {
                        me.Attack(target);
                        Utils.Sleep(1000, "StageCheck");
                        return;
                    }
                    break;
            }
            if (refresher == null || !Menu.Item("items").GetValue<AbilityToggler>().IsEnabled(refresher.Name) || refresher.AbilityState != AbilityState.Ready || _stage < Combos[_combo].GetRefreshPos()|| Combos[_combo].GetRefreshPos()==-1)
                return;
            //Game.PrintMessage($"refreshPos {Combos[_combo].GetRefreshPos()} Combo: {_combo} Stage: {_stage}",MessageType.ChatMessage);
            refresher.UseAbility();
            _stage = 3;
        }

        private static void LetsTryCastSpell(Hero me, Hero target, Ability spellForCast, bool nextSpell=false)
        {
            var ss = me.FindSpell("invoker_sun_strike");
            var icewall = me.FindSpell("invoker_ice_wall");
            var blast = me.FindSpell("invoker_deafening_blast");
            var tornado = me.FindSpell("invoker_tornado");
            var emp = me.FindSpell("invoker_emp");
            /*
            var coldsnap = me.FindSpell("invoker_cold_snap");
            var ghostwalk = me.FindSpell("invoker_ghost_walk");
            
            var forge = me.FindSpell("invoker_forge_spirit");
            var emp = me.FindSpell("invoker_emp");
            var alacrity = me.FindSpell("invoker_alacrity");
            */
            var meteor = me.FindSpell("invoker_chaos_meteor");
            var eulmodif = target.Modifiers.FirstOrDefault(x => x.Name == "modifier_eul_cyclone" || x.Name == "modifier_invoker_tornado");
            var timing = (Equals(spellForCast, ss))
                ? 1.7
                : (Equals(spellForCast, meteor))
                    ? 1.3
                    : Equals(spellForCast, blast)
                        ? me.Distance2D(target)/1100 + Game.Ping/1000
                        : (Equals(spellForCast, icewall))
                            ? 2.5
                            : Equals(spellForCast, emp) ? 2.9 : 0;
                        
            if (eulmodif!=null && eulmodif.RemainingTime<=timing)
            {
                if (icewall != null && Equals(spellForCast, icewall))
                {
                    CastIceWall(me, target, me.Distance2D(target) <= 250,icewall);
                }
                else
                {
                    UseSpell(spellForCast, target,me);
                    //Game.PrintMessage(spellForCast.Name+" (2)", MessageType.ChatMessage);
                    //PrintInfo("caster "+spellForCast.Name+" with timing "+timing);
                    Utils.Sleep(250, "StageCheck");
                    _stage++;
                }
            }
            else if (eulmodif == null && !Equals(spellForCast, ss))
            {
                if (icewall != null && Equals(spellForCast, icewall))
                {
                    CastIceWall(me, target, me.Distance2D(target) <= 300, icewall);
                }
                else
                {
                    var time = 250f;
                    if (Equals(spellForCast, tornado))
                    {
                        if (nextSpell) time += me.Distance2D(target)/spellForCast.GetProjectileSpeed()*1000 + Game.Ping;

                        spellForCast.CastSkillShot(target, me.Position,spellForCast.Name);
                        //Game.PrintMessage("CastSkillShot "+spellForCast.CastSkillShot(target, me.Position,spellForCast.Name),MessageType.ChatMessage);
                    }
                    else
                    {
                        UseSpell(spellForCast, target,me);
                    }
                    Utils.Sleep(time, "StageCheck");
                    _stage++;
                }
            }
        }

        private static void CastIceWall(Hero me, Hero target, bool b, Ability icewall)
        {
            if (!b)
            {
                if (!me.CanMove() || !Utils.SleepCheck("icewallmove")) return;
                var angle = me.FindAngleBetween(target.Position, true);
                var point =
                    new Vector3(
                        (float)(target.Position.X -
                                 200 *
                                 Math.Cos(angle)),
                        (float)(target.Position.Y -
                                 200 *
                                 Math.Sin(angle)), 0);
                me.Move(point);
                Utils.Sleep(300, "icewallmove");
            }
            else
            {
                icewall.UseAbility();
                _stage++;
                Utils.Sleep(250, "StageCheck");
            }
        }

        private static void UseSpell(Ability spellForCast, Hero target,Hero me)
        {
            if (spellForCast.CanBeCasted(target))
            {
                spellForCast.UseAbility(target);
            }
            spellForCast.UseAbility();
            spellForCast.UseAbility(target.Position);
            spellForCast.UseAbility(me);
        }

        private static Hero ClosestToMouse(Hero source, float range = 1000)
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
        private static void PrintInfo(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.White, arguments);
        }

        private static void PrintSuccess(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.Green, arguments);
        }

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
        #endregion

    }
}
