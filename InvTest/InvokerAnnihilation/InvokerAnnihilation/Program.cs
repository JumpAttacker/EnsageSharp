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
using Ensage.Common.Objects;
using SharpDX;

namespace InvokerAnnihilation
{
    internal static class Program
    {
        #region Members
        private static readonly Menu Menu = new Menu("Invoker Annihilation", "InvokerAnnihilation", true, "npc_dota_hero_invoker", true);
        private static readonly string Ver = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        private const int WmKeyup = 0x0101;
        private static int _combo;
        //private static readonly SpellStruct[] Spells = new SpellStruct[12];
        private static readonly ComboStruct[] Combos = new ComboStruct[13];
        private static int _maxCombo;
        private static bool _inAction;
        private static bool _initNewCombo;
        private static uint _stage;
        private static readonly Dictionary<string, SpellStruct> SpellInfo = new Dictionary<string, SpellStruct>();
        private static ParticleEffect _predictionEffect;
        private static Ability _spellForCast;
        private static bool _startInitSpell;
        private static NetworkActivity _lastAct=NetworkActivity.Idle;
        private static bool _lastAction;
        private static Hero _myHero;
        private static Player MyPlayer { get; set; }
        //============================================================
        //============================================================
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
            Events.OnLoad += (sender, args) =>
            {
                _myHero = ObjectManager.LocalHero;
                if (_myHero.ClassID != ClassID.CDOTA_Unit_Hero_Invoker)
                {
                    return;
                }
                MyPlayer = ObjectManager.LocalPlayer;
                _stage = 0;
                _combo = 0;
                var spells = _myHero.Spellbook;

                var q = spells.SpellQ;
                var w = spells.SpellW;
                var e = spells.SpellE;
                
                var ss = Abilities.FindAbility("invoker_sun_strike");
                var coldsnap = Abilities.FindAbility("invoker_cold_snap");
                var ghostwalk = Abilities.FindAbility("invoker_ghost_walk");
                var icewall = Abilities.FindAbility("invoker_ice_wall");
                var tornado = Abilities.FindAbility("invoker_tornado");
                var blast = Abilities.FindAbility("invoker_deafening_blast");

                var forgeSpirit = spells.Spells.FirstOrDefault(x=>x.Name=="invoker_forge_spirit");
                if (forgeSpirit == null)
                {
                    Print("oops, something went wrong. Please reload the script.");
                    return;
                }
                var emp = Abilities.FindAbility("invoker_emp");
                var alacrity = Abilities.FindAbility("invoker_alacrity");
                var meteor = Abilities.FindAbility("invoker_chaos_meteor");

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
                Combos[_maxCombo] = new ComboStruct(new[] {tornado, emp, meteor,blast,ss,icewall},5);
                Combos[_maxCombo] = new ComboStruct(new[] {ss, ss, ss, ss, ss, ss, ss, ss}, 3, false, true);

                Game.PrintMessage(
                    "<font face='Comic Sans MS, cursive'><font color='#00aaff'>" + Menu.DisplayName + " By Jumpering" +
                    " loaded!</font> <font color='#aa0000'>v"  + Ver, MessageType.LogMessage);
                PrintSuccess(string.Format("> {1} Loaded v{0}", Ver, Menu.DisplayName));

                Game.OnUpdate += Game_OnUpdate;
                Drawing.OnDraw += Drawing_OnDraw;
                Game.OnWndProc += Game_OnWndProc;
                Player.OnExecuteOrder += Player_OnExecuteAction;
            };
            Events.OnClose += (sender, args) =>
            {
                Game.OnUpdate -= Game_OnUpdate;
                Drawing.OnDraw -= Drawing_OnDraw;
                Game.OnWndProc -= Game_OnWndProc;
                Player.OnExecuteOrder -= Player_OnExecuteAction;
                SpellInfo.Clear();
                _maxCombo = 0;
                PrintSuccess(string.Format("> {1} Unloaded v{0}", Ver, Menu.DisplayName));
            };

            Menu.AddItem(new MenuItem("Hotkey", "Combo Key").SetValue(new KeyBind('G', KeyBindType.Press)));
            Menu.AddItem(
                new MenuItem("Hotkey.Ghost", " Quick cast ghost walk").SetValue(new KeyBind('H', KeyBindType.Press)));
            var sunStrikeSettings=new Menu("Sun Strike Settings","ssSettings");

            /*sunStrikeSettings.AddItem(
                new MenuItem("hotkey", "Hotkey").SetValue(new KeyBind('T', KeyBindType.Press))
                    .SetTooltip("press hotkey for auto SunStrike"));
            sunStrikeSettings.AddItem(new MenuItem("ssShift", "Use Shift With Hotkey").SetValue(true));*/
            sunStrikeSettings.AddItem(new MenuItem("ssDamageontop", "Show Damage on Top Panel").SetValue(false));
            sunStrikeSettings.AddItem(new MenuItem("ssDamageonhero", "Show Damage on Hero").SetValue(false));
            sunStrikeSettings.AddItem(new MenuItem("ssPrediction", "Show Prediction").SetValue(false));
            sunStrikeSettings.AddItem(new MenuItem("ssAutoInStunned", "Use sunstrike on stun").SetValue(true));
            


            var combo = new Menu("Combos", "combos");
            combo.AddItem(
                new MenuItem("hotkeyPrev", "Previous Combo").SetValue(new KeyBind(0x6B, KeyBindType.Press))
                    .SetTooltip("default hotkey is numpad [+]"));
            combo.AddItem(
                new MenuItem("hotkeyNext", "Next Combo").SetValue(new KeyBind(0x6D, KeyBindType.Press))
                    .SetTooltip("default hotkey is numpad [-]"));
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
                {"item_sheepstick",true},
                {"item_urn_of_shadows",true}
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
            private readonly bool _custom;
            public readonly Ability []Spells;
            private readonly int _refreshPos;

            /// <summary>
            /// add new combo to sys
            /// </summary>
            /// <param name="spells">array of spells</param>
            /// <param name="refreshPos">min spell pos for refresher (type -1 to disable refresher)</param>
            /// <param name="useEul">use uel in this combo</param>
            /// <param name="isItCustom">player can change this combo</param>
            public ComboStruct(Ability[] spells, int refreshPos,bool useEul=false,bool isItCustom=true)
            {
                _isNeedEul = useEul;
                Spells = spells;
                _maxCombo++;
                _refreshPos = refreshPos;
                _maxComboSpells = Math.Max(Spells.Length, _maxComboSpells);
                _custom = isItCustom;
            }
            
            public int GetRefreshPos()
            {
                return _refreshPos;
            }
            public int GetSpellsInCombo()
            {
                return Spells.Length;
            }
            public Ability[] GetComboAbilities()
            {
                return Spells;
            }
            public bool IsCustom()
            {
                return _custom;
            }

            public bool CheckEul()
            {
                return _isNeedEul;
            }

            public override string ToString()
            {
                var s=new StringBuilder();
                foreach (var ability in Spells)
                {
                    s.AppendLine(ability.StoredName());
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
            if (args.WParam == Menu.Item("Hotkey").GetValue<KeyBind>().Key)
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
            if (args.WParam == Menu.Item("Hotkey.Ghost").GetValue<KeyBind>().Key)
            {
                _ghostMode = args.Msg != WmKeyup;
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
            if (args.WParam != 1 || !Utils.SleepCheck("clicker"))
            {
                _leftMouseIsPress = false;
                return;
            }
            _leftMouseIsPress = true;
        }

        private static Vector2 _size = new Vector2(HUDInfo.GetHPBarSizeX() / 4, HUDInfo.GetHPBarSizeX() / 4);
        private static bool _ghostMode;
        private static bool _leftMouseIsPress;
        private static readonly bool[,] OpenStatus=new bool[15,15];

        private static void Drawing_OnDraw(EventArgs args)
        {
            var me = _myHero;

            #region SS ACTION

            //var exort = me.Spellbook.SpellE;
            var exort=Abilities.FindAbility("invoker_exort");
            var topDamage = Menu.Item("ssDamageontop").GetValue<bool>();
            var heroDamage = Menu.Item("ssDamageonhero").GetValue<bool>();
            var predDamage = Menu.Item("ssPrediction").GetValue<bool>();
            if (exort != null && exort.Level>0 && (topDamage || heroDamage || predDamage))
            {
                var damage = 100 + 62.5*(exort.Level - 1);
                if (topDamage)
                {
                    var enemy = ObjectManager.GetEntities<Hero>()
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
                            var w2SPos = HUDInfo.GetHPbarPosition(target);
                            if (w2SPos.X > 0 && w2SPos.Y > 0)
                            {
                                var sizeY = HUDInfo.GetHpBarSizeY();
                                var damagePerSs = (target.Health - damage).ToString(CultureInfo.InvariantCulture);
                                var textSize = Drawing.MeasureText(damagePerSs, "Arial",
                                    new Vector2((float) (sizeY*1.5), 500), FontFlags.AntiAlias);
                                var textPos = w2SPos - new Vector2(textSize.X + 5, (float) ((sizeY*1.5) - (textSize.Y)));
                                Drawing.DrawText(
                                    damagePerSs,
                                    textPos,
                                    new Vector2((float) (sizeY*1.5), 100),
                                    Color.White,
                                    FontFlags.AntiAlias | FontFlags.StrikeOut);
                                var ss=Abilities.FindAbility("invoker_sun_strike");
                                var texturename = $"materials/ensage_ui/spellicons/{ss.StoredName()}.vmat";
                                var iconPos = textPos - new Vector2(sizeY*2 + 5, 0);
                                Drawing.DrawRect(
                                    iconPos,
                                    new Vector2(sizeY*2, sizeY*2),
                                    Textures.GetTexture(texturename));
                            }
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
                        var sunstrike = Abilities.FindAbility("invoker_sun_strike");
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

            var max = Combos.Length;
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
                        Textures.GetTexture(texturename));
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
                for (; j+1 <= Combos[i].GetSpellsInCombo(); j++)
                {
                    try
                    {
                        texturename = $"materials/ensage_ui/spellicons/{comboStruct.GetComboAbilities()[j].StoredName()}.vmat";
                        var sizeX = _size.X * (j + (eul?1:0)) + 10;
                        itemStartPos = pos + new Vector2(sizeX, sizeY);
                        Drawing.DrawRect(
                            itemStartPos,
                            new Vector2(_size.X-6, _size.Y),
                            Textures.GetTexture(texturename));
                        legal++;
                        var tempStage = _stage<2?0:_stage-2;
                        if (Equals(comboStruct.GetComboAbilities()[j], Combos[_combo].GetComboAbilities()[tempStage]) && selected && _stage>0)
                        {
                            Drawing.DrawRect(
                                itemStartPos,
                                new Vector2(_size.X - 3, _size.Y),
                                Color.Red, true);
                        }
                        if (comboStruct.IsCustom())
                        {
                            var isIn=Utils.IsUnderRectangle(Game.MouseScreenPosition, itemStartPos.X, itemStartPos.Y, _size.X - 3,
                                _size.Y);
                            if (isIn)
                            {
                                Drawing.DrawRect(
                                    itemStartPos,
                                    new Vector2(_size.X - 3, _size.Y),
                                    Color.GreenYellow, true);
                            }
                            if (isIn && _leftMouseIsPress && Utils.SleepCheck("clicker"+j+"/"+i))
                            {
                                Utils.Sleep(200,"clicker"+j+"/"+i);
                                OpenStatus[j,i] = !OpenStatus[j,i];
                            }
                            if (OpenStatus[j,i])
                            {
                                var kek = 0;
                                var itemStartPos3 = pos + new Vector2(8, sizeY);
                                
                                Drawing.DrawRect(
                                    itemStartPos,
                                    new Vector2(_size.X - 3, _size.Y),
                                    new Color(0,0,0,155));

                                Drawing.DrawRect(
                                    itemStartPos,
                                    new Vector2(_size.X - 3, _size.Y),
                                    Color.Orange,true);

                                Drawing.DrawRect(
                                    itemStartPos3 + new Vector2((-_size.X - 2)*10 - 2, -2),
                                    new Vector2((_size.X - 2)*11, (_size.Y + 2) + 2),
                                    Color.DarkOrange, true);
                                foreach (var spell in SpellInfo)
                                {
                                    kek++;
                                    texturename = $"materials/ensage_ui/spellicons/{spell.Key}.vmat";
                                    var sizeX2 = _size.X * kek + 10;
                                    //var sizeY2 = 4 + (i + kek)*(_size.Y + 2);
                                    var itemStartPos2 = pos + new Vector2(-sizeX2, sizeY);
                                    Drawing.DrawRect(
                                        itemStartPos2,
                                        new Vector2(_size.X - 6, _size.Y),
                                        Textures.GetTexture(texturename));
                                    isIn = Utils.IsUnderRectangle(Game.MouseScreenPosition, itemStartPos2.X,
                                        itemStartPos2.Y, _size.X - 3,
                                        _size.Y);
                                    if (isIn)
                                    {
                                        Drawing.DrawRect(
                                            itemStartPos2,
                                            new Vector2(_size.X - 3, _size.Y),
                                            Color.GreenYellow, true);
                                        if (_leftMouseIsPress)
                                        {
                                            OpenStatus[j,i] = !OpenStatus[j,i];
                                            comboStruct.Spells[j] = Abilities.FindAbility(spell.Key);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        //Print("I:" + i + " J: " + j + " stage-2:" + (_stage - 2));
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
            //Print($"Max:{max}; I:{i}");
            #endregion
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            #region Init

            var me = _myHero;

            if (Game.IsPaused)
            {
                return;
            }

            if (Utils.SleepCheck("act") && !_inAction && Menu.Item("smartIsActive").GetValue<bool>())
            {
                Ability spell = null;
                switch (Menu.Item("OnMoving").GetValue<StringList>().SelectedIndex)
                {
                    case (int) SmartSphereEnum.Quas:
                        spell = Abilities.FindAbility("invoker_quas");;
                        break;
                    case (int) SmartSphereEnum.Wex:
                        spell = Abilities.FindAbility("invoker_wex");;
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

            #region Flee mode

            if (_ghostMode && Utils.SleepCheck("flee_mode") && !me.IsInvisible())
            {
                var q = Abilities.FindAbility("invoker_quas");;
                var w = Abilities.FindAbility("invoker_wex");;
                var active1 = me.Spellbook.Spell4;
                var active2 = me.Spellbook.Spell5;
                
                if (q?.Level > 0 && w?.Level > 0 )
                {
                    var ghostwalk = Abilities.FindAbility("invoker_ghost_walk");
                    if (ghostwalk==null || ghostwalk.Cooldown>0) return;
                    if (active1.Equals(ghostwalk) || active2.Equals(ghostwalk))
                    {
                        w.UseAbility();
                        w.UseAbility();
                        w.UseAbility();
                        ghostwalk.UseAbility();
                        Utils.Sleep(500,"flee_mode");
                    }
                    else
                    {
                        InvokeNeededSpells(me, ghostwalk);
                        w.UseAbility();
                        w.UseAbility();
                        w.UseAbility();
                        ghostwalk.UseAbility();
                        Utils.Sleep(500,"flee_mode");
                    }
                }
                
            }

            #endregion

            #region Auto ss on stunned enemy

            if (Menu.Item("ssAutoInStunned").GetValue<bool>() && !me.IsInvisible() && Utils.SleepCheck("auto_ss"))
            {
                var ss = Abilities.FindAbility("invoker_sun_strike");
                if (ss != null && ss.CanBeCasted())
                {
                    var enemy =
                        Heroes.GetByTeam(_myHero.GetEnemyTeam())
                            .Where(x => x.IsAlive && x.IsVisible);
                    foreach (var hero in enemy)
                    {
                        float time;
                        if (hero.IsStunned(out time))
                        {
                            //hero.Modifiers.ForEach(modifier => Print(modifier.Name));
                            var mod =
                                hero.HasModifiers(new[]
                                {
                                    "modifier_obsidian_destroyer_astral_imprisonment_prison", "modifier_eul_cyclone",
                                    "modifier_shadow_demon_disruption", "modifier_invoker_tornado"
                                }, false);
                            
                            if ((Math.Abs(time) >= 1.7 + Game.Ping/1000 && !mod) ||  (Math.Abs(time) <= 1.69 + Game.Ping/1000 && Math.Abs(time) >= 1.00 + Game.Ping/1000))
                            {
                                var spells = me.Spellbook;
                                var e = spells.SpellE;
                                var active1 = me.Spellbook.Spell4;
                                var active2 = me.Spellbook.Spell5;

                                if (e?.Level > 0)
                                {
                                    if (active1.Equals(ss) || active2.Equals(ss))
                                    {
                                        ss.UseAbility(hero.Position);
                                        Utils.Sleep(500, "auto_ss");
                                    }
                                    else
                                    {
                                        InvokeNeededSpells(me, ss);
                                        ss.UseAbility(hero.Position);
                                        Utils.Sleep(500, "auto_ss");
                                    }
                                }
                            }
                        }
                        else
                        {
                            //hero.Modifiers.ForEach(modifier => Print("2. "+modifier.Name));
                            var extramod = hero.FindModifier("modifier_ember_spirit_searing_chains");
                            if (extramod!=null && extramod.RemainingTime>=1.7+Game.Ping/1000)
                            {
                                var spells = me.Spellbook;
                                var e = spells.SpellE;
                                var active1 = me.Spellbook.Spell4;
                                var active2 = me.Spellbook.Spell5;

                                if (e?.Level > 0)
                                {
                                    if (active1.Equals(ss) || active2.Equals(ss))
                                    {
                                        ss.UseAbility(hero.Position);
                                        Utils.Sleep(500, "auto_ss");
                                    }
                                    else
                                    {
                                        InvokeNeededSpells(me, ss);
                                        ss.UseAbility(hero.Position);
                                        Utils.Sleep(500, "auto_ss");
                                    }
                                }
                            }
                        }
                    }
                }
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

        private static void InvokeNeededSpells(Hero me,Ability neededAbility=null)
        {
            var spell1 = _spellForCast = neededAbility ?? Combos[_combo].GetComboAbilities()[0];
            var spell2 = _spellForCast = neededAbility ?? Combos[_combo].GetComboAbilities()[1];
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
                    if (!SpellInfo.TryGetValue(spell1.StoredName(), out s)) return;
                }
                else if (Equals(spell2, active2))
                {
                    if (!SpellInfo.TryGetValue(spell2.StoredName(), out s)) return;
                }
                else if (Equals(spell1, active1) || Equals(spell1, active2))
                {
                    if (!SpellInfo.TryGetValue(spell2.StoredName(), out s)) return;
                }
                else
                {
                    if (!SpellInfo.TryGetValue(spell1.StoredName(), out s)) return;
                }
                var invoke = Abilities.FindAbility("invoker_invoke");
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
            var items = me.Inventory.Items.ToList();
            var invoke = Abilities.FindAbility("invoker_invoke");
            
            var eul = items.FirstOrDefault(x=>x.StoredName()=="item_cyclone");
            var dagger = items.FirstOrDefault(x=>x.StoredName()=="item_blink");
            var refresher = items.FirstOrDefault(x=>x.StoredName()=="item_refresher");
            var hex = items.FirstOrDefault(x=>x.StoredName()=="item_sheepstick");
            var urn = items.FirstOrDefault(x=>x.StoredName()=="item_urn_of_shadows");
            var orchid = items.FirstOrDefault(x=>x.StoredName()=="item_orchid");

            var meteor = Abilities.FindAbility("invoker_chaos_meteor");
            var ss = Abilities.FindAbility("invoker_sun_strike");
            var icewall = Abilities.FindAbility("invoker_ice_wall");
            var deafblast = Abilities.FindAbility("invoker_deafening_blast");
            //var emp = me.FindSpell("invoker_emp");
            /*
            
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
                //PrintInfo("Starting new combo! " + $"[{_combo + 1}] target: {target.StoredName()}");
            }
            if (!Utils.SleepCheck("StageCheck")) return;
            #endregion

            /*var modif = target.Modifiers.Where(x=>x.IsDebuff);
            PrintInfo("===========================");
            foreach (var s in modif)
            {
                PrintInfo(s.StoredName());
            }*/
            var myBoys = ObjectManager.GetEntities<Unit>().Where(x => x.Team == me.Team && x.IsControllable && x.IsAlive && Utils.SleepCheck(x.Handle.ToString()));
            foreach (var myBoy in myBoys)
            {
                myBoy.Attack(target);
                Utils.Sleep(300, myBoy.Handle.ToString());
            }
            if (me.CanUseItems())
            {
                if (urn != null && urn.CanBeCasted(target))
                {
                    var urnMod = target.HasModifier("modifier_item_urn_damage") &&
                                 Utils.SleepCheck(urn.StoredName());
                    if (!urnMod)
                    {
                        urn.UseAbility(target);
                        Utils.Sleep(300, urn.StoredName());
                    }
                }
                if (_stage > 2 && !target.IsHexed() && !target.IsStunned())
                {
                    if (hex != null && hex.CanBeCasted(target) &&
                        Menu.Item("items").GetValue<AbilityToggler>().IsEnabled(hex.StoredName()) &&
                        Utils.SleepCheck("items"))
                    {
                        hex.UseAbility(target);
                        Utils.Sleep(300, "items");
                    }
                    if (orchid != null && orchid.CanBeCasted(target) &&
                        Menu.Item("items").GetValue<AbilityToggler>().IsEnabled(orchid.StoredName()) &&
                        Utils.SleepCheck("items"))
                    {
                        orchid.UseAbility(target);
                        Utils.Sleep(300, "items");
                    }
                }
                if (dagger != null && Menu.Item("items").GetValue<AbilityToggler>().IsEnabled(dagger.StoredName()) &&
                    dagger.CanBeCasted() && Utils.SleepCheck("blinker") && me.Distance2D(target) >= 700)
                {
                    var dist = 300;
                    var angle = me.FindAngleBetween(target.Position, true);
                    var point =
                        new Vector3(
                            (float) (target.Position.X -
                                     dist*
                                     Math.Cos(angle)),
                            (float) (target.Position.Y -
                                     dist*
                                     Math.Sin(angle)), 0);
                    if (me.Distance2D(target) <= 1150 + 700)
                        dagger.UseAbility(point);
                    else
                        me.Move(target.Position);
                    Utils.Sleep(250, "blinker");
                }
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
                                if (_spellForCast.StoredName() == "invoker_cold_snap")
                                {
                                    if (_spellForCast.CanBeCasted(target))
                                        LetsTryCastSpell(me, target, _spellForCast);
                                }
                                else
                                {
                                    LetsTryCastSpell(me, target, _spellForCast, Equals(nextSpell, deafblast) || Equals(nextSpell, meteor)|| Equals(nextSpell, ss) /*|| Equals(nextSpell, emp)*/);
                                }
                            }
                            else
                            {
                                SpellStruct s;
                                if (SpellInfo.TryGetValue(_spellForCast.StoredName(), out s))
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
                                        PrintError("couldnt find data for spell: " + _spellForCast.StoredName());
                                    }
                                    catch (Exception)
                                    {
                                        PrintError("couldnt find data for spell: ERROR");
                                    }
                            }
                        }
                        else
                        {
                            PrintInfo($"spell {_spellForCast.StoredName()} cant be casted, go next [{_stage}]");
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
            if (refresher == null || !Menu.Item("items").GetValue<AbilityToggler>().IsEnabled(refresher.StoredName()) ||
                refresher.AbilityState != AbilityState.Ready || _stage < Combos[_combo].GetRefreshPos() ||
                Combos[_combo].GetRefreshPos() == -1)
                return;
            //Game.PrintMessage($"refreshPos {Combos[_combo].GetRefreshPos()} Combo: {_combo} Stage: {_stage}",MessageType.ChatMessage);
            refresher.UseAbility();
            _stage --;
        }

        private static void LetsTryCastSpell(Hero me, Hero target, Ability spellForCast, bool nextSpell=false)
        {
            var ss = Abilities.FindAbility("invoker_sun_strike");
            var icewall = Abilities.FindAbility("invoker_ice_wall");
            var blast = Abilities.FindAbility("invoker_deafening_blast");
            var tornado = Abilities.FindAbility("invoker_tornado");
            var emp = Abilities.FindAbility("invoker_emp");
            /*
            var coldsnap = me.FindSpell("invoker_cold_snap");
            var ghostwalk = me.FindSpell("invoker_ghost_walk");
            
            var forge = me.FindSpell("invoker_forge_spirit");
            var emp = me.FindSpell("invoker_emp");
            var alacrity = me.FindSpell("invoker_alacrity");
            */
            var meteor = Abilities.FindAbility("invoker_chaos_meteor");
            var eulmodif = target.FindModifier("modifier_eul_cyclone") ??
                           target.FindModifier("modifier_invoker_tornado");
            /*foreach (var source in target.Modifiers.ToList())
            {
                PrintInfo(source.StoredName()+": "+source.RemainingTime);
            }*/
            var timing = Equals(spellForCast, ss)
                ? 1.7
                : Equals(spellForCast, meteor)
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
                    //Game.PrintMessage(spellForCast.StoredName()+" (2)", MessageType.ChatMessage);
                    //PrintInfo("caster "+spellForCast.StoredName()+" with timing "+timing);
                    Utils.Sleep(250, "StageCheck");
                    _stage++;
                }
            }
            else if (eulmodif == null /*&& !Equals(spellForCast, ss)*/)
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

                        spellForCast.CastSkillShot(target, me.Position,spellForCast.StoredName());
                        //Game.PrintMessage("CastSkillShot "+spellForCast.CastSkillShot(target, me.Position,spellForCast.StoredName()),MessageType.ChatMessage);
                    }
                    else
                    {
                        //Game.PrintMessage("suka: " + spellForCast.StoredName(),MessageType.ChatMessage);
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
                ObjectManager.GetEntities<Hero>()
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

        private static void Print(string s)
        {
            Game.PrintMessage(s, MessageType.ChatMessage);
        }
        private static string GPrint(this string s)
        {
            Game.PrintMessage("[debug]: "+s, MessageType.ChatMessage);
            return s;
        }

        #endregion

    }
}
