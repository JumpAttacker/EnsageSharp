using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using SharpDX;

// ReSharper disable UnusedMember.Local

namespace ArcAnnihilation
{
    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    internal static class Program
    {
        #region Variables

        private static Hero _mainHero;
        private static bool _loaded;
        private static Vector3 _pushLaneTop = new Vector3(-5895, 5402, 384);
        private static Vector3 _pushLaneBot = new Vector3(5827, -5229, 384);
        private static Vector3 _pushLaneMid = new Vector3(1, 1, 384);
        private static float _myHull;
        private static readonly Dictionary<Vector3, string> LaneDictionary = new Dictionary<Vector3, string>()
        {
<<<<<<< HEAD
            {new Vector3(-6080, 5805, 384), "top"}, 
=======
            {new Vector3(-5895, 5402, 384), "top"}, 
>>>>>>> origin/master
            {new Vector3(-6600, -3000, 384), "top"},
            {new Vector3(2700, 5600, 384), "top"},


<<<<<<< HEAD
            {new Vector3(5807, -5785, 384), "bot"}, 
=======
            {new Vector3(5827, -5229, 384), "bot"}, 
>>>>>>> origin/master
            {new Vector3(-3200, -6200, 384), "bot"},
            {new Vector3(6200, 2200, 384), "bot"},


            {new Vector3(-600, -300, 384), "middle"},
            {new Vector3(3600, 3200, 384), "middle"},
            {new Vector3(-4400, -3900, 384), "middle"}

        };

        private static readonly Menu Menu = new Menu("Arc Annihilation", "arc", true, "npc_dota_hero_arc_warden", true);
        private static Hero _globalTarget;
        private static Hero _globalTarget2;
        private static int _tick;
        private static readonly Dictionary<uint, int> LastAttackStart = new Dictionary<uint, int>();
        private static readonly Dictionary<uint, NetworkActivity> LastActivity = new Dictionary<uint, NetworkActivity>();

        private static readonly Dictionary<string, byte> Items = new Dictionary<string, byte>
        {
            {"item_mask_of_madness", 1},
            {"item_dagon", 2},
            {"item_dagon_2", 2},
            {"item_dagon_3", 2},
            {"item_dagon_4", 2},
            {"item_dagon_5", 2},
            {"item_blink", 5},
            {"item_orchid",4},
            {"item_manta", 1},
            {"item_arcane_boots", 1},
            {"item_guardian_greaves", 1},
            {"item_shivas_guard", 1},
            {"item_ethereal_blade", 3},

            {"item_soul_ring", 4},
            {"item_blade_mail", 4},
            {"item_veil_of_discord", 4},
            {"item_heavens_halberd", 1},

            {"item_necronomicon", 2},
            {"item_necronomicon_2", 2},
            {"item_necronomicon_3", 2},
            {"item_mjollnir", 1},

            {"item_sheepstick", 4}
        };

        private static readonly List<string> AutoPushItems = new List<string>
        {
            "item_necronomicon",
            "item_necronomicon_2",
            "item_necronomicon_3",
            "item_manta",
            "item_mjollnir"
        };

        private static readonly List<string> CloneHealItems = new List<string>
        {
            "item_flask",
            "item_clarity",
            "item_enchanted_mango",
            "item_guardian_greaves",
            "item_arcane_boots",
            "item_sphere",
            "item_bottle"
        };

        private static readonly List<string> CloneOnlyItems = new List<string>
        {
            "item_diffusal_blade",
            "item_flask",
            "item_clarity",
            "item_enchanted_mango",
            "item_bottle",
            "item_diffusal_blade_2"
        };

        private static readonly List<string> CloneOnlyComboItems = new List<string>
        {
            "item_diffusal_blade",
            "item_diffusal_blade_2"
        };

        #endregion

        #region Enums

        private enum Orders
        {
            Monkey,
            Caster,
            Nothing
        }

        private enum PurgeSelection
        {
            MainTarget,
            AllEnemies,
            Noone
        }

        private enum DispelSelection
        {
            Me,
            Tempest,
            Both,
            Noone
        }

        private enum BkbUsage
        {
            Me,
            Clones,
            All,
            NoOne
        }

        #endregion

        private static void Main()
        {
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Player.OnExecuteOrder += Player_OnExecuteAction;
            //var dict = Items.ToDictionary(item => item, item => true);
            Menu.AddItem(new MenuItem("hotkey", "Hotkey").SetValue(new KeyBind('G', KeyBindType.Press)));
            Menu.AddItem(new MenuItem("spamHotkey", "Spark Spam").SetValue(new KeyBind('H', KeyBindType.Press)));
            Menu.AddItem(
                new MenuItem("hotkeyClone", "ComboKey with Clones").SetValue(new KeyBind('Z', KeyBindType.Toggle)));
            //Menu.AddItem(new MenuItem("Items", "Items:").SetValue(new AbilityToggler(dict)));
            Menu.AddItem(new MenuItem("LockTarget", "Lock Target").SetValue(true));
            Menu.AddItem(new MenuItem("AutoMidas", "Auto Midas").SetValue(true));
            Menu.AddItem(
                new MenuItem("FirstClone", "Ez Heal").SetValue(true)
                    .SetTooltip("when you use some heal-items, at the beginning of the clone will use this"));
            //Menu.AddItem(new MenuItem("AutoHeal", "Auto Heal/Bottle").SetValue(true).SetTooltip("clone use heal items on main hero if there are no enemies in 500(800) range"));
            var autoheal=new Menu("Auto Heal","aheal");
            autoheal.AddItem(
                new MenuItem("AutoHeal.Enable", "Auto Heal").SetValue(new KeyBind('X', KeyBindType.Toggle))
                    .SetTooltip(
                        "clone use heal items on main hero if there are no enemies in selected range. But ll still use insta heal items"));
            autoheal.AddItem(
                            new MenuItem("AutoHeal.Range", "Enemy checker").SetValue(new Slider(500, 0, 1000)).SetTooltip("check enemy in selected range"));
            var autoPush = new Menu("Auto Push", "AutoPush");
            autoPush.AddItem(new MenuItem("AutoPush.Enable", "Enable").SetValue(new KeyBind('V', KeyBindType.Toggle)));
            autoPush.AddItem(new MenuItem("AutoPush.DrawLine", "Draw line").SetValue(false));
            autoPush.AddItem(new MenuItem("AutoPush.Travels", "Use Travel Boots").SetValue(true));
            autoPush.AddItem(
                new MenuItem("AutoPush.UnAggro.Enable", "UnAggro under tower").SetValue(true)
                    .SetTooltip(
                        "Necronomicon will try to unaggro under tower and will stay away from tower if didnt see any ally creep under tower"));
            var antiFeed = new Menu("Anti Feed", "AntiFeed", false, "item_necronomicon_3", true);
            antiFeed.AddItem(new MenuItem("AntiFeed.Enable", "Ebable").SetValue(true).SetTooltip("if u have any enemy hero in range, ur necro will run on base"));
            antiFeed.AddItem(new MenuItem("AntiFeed.Range", "Range Checker").SetValue(new Slider(800,0,1500)));


            var orbwalnking = new Menu("OrbWalking", "ow");
            orbwalnking.AddItem(
                            new MenuItem("OrbWalking.Enable", "Enable OrbWalking").SetValue(true));
            orbwalnking.AddItem(
                new MenuItem("OrbWalking.bonusWindupMs", "Bonus Windup Time").SetValue(new Slider(100, 100, 1000))
                    .SetTooltip("Time between attacks"));

            var daggerSelection = new Menu("Dagger", "dagger");
            /*daggerSelection.AddItem(
                            new MenuItem("Dagger.Enable", "Enable Dagger").SetValue(true));*/
            daggerSelection.AddItem(
                new MenuItem("Dagger.CloseRange", "Min distance between target and blink position").SetValue(
                    new Slider(200, 100, 800)));
            daggerSelection.AddItem(
                new MenuItem("Dagger.MinDistance", "Min distance for blink").SetValue(new Slider(400, 100, 800)));

            var difblade = new Menu("Diffusal blade", "item_diffusal_blade", false, "item_diffusal_blade",true);
            difblade.AddItem(
                new MenuItem("Diffusal.Dispel", "Dispel Selection").SetValue(new StringList(new[]
                {
                    "Me",
                    "Tempest",
                    "All",
                    "Noone"
                }, 2))).SetTooltip("All include ally heroes too");
            difblade.AddItem(
                new MenuItem("Diffusal.PurgEnemy", "Purge Selection").SetValue(new StringList(new[]
                {
                    "Only on Main Target",
                    "For all Enemies in cast range",
                    "No one"
                }, 1)));

            Menu.AddItem(new MenuItem("usePrediction", "Use Prediction For Spark").SetValue(true));
            Menu.AddItem(
                new MenuItem("BkbUsage", "Bkb Selection").SetValue(
                    new StringList(new[] {"me", "clones", "all", "no one"}, 1)));
            //var il=new Menu("Illusion","il");
            //il.AddItem(new MenuItem("orderList", "Use order list").SetValue(false));
            Menu.AddItem(
                new MenuItem("order", "Clone Order Selection").SetValue(
                    new StringList(new[] {"monkey", "caster", "nothing"}, 1)));


            Menu.AddSubMenu(difblade);
            Menu.AddSubMenu(daggerSelection);
            Menu.AddSubMenu(autoheal);
            Menu.AddSubMenu(orbwalnking);
            Menu.AddSubMenu(autoPush);
            autoPush.AddSubMenu(antiFeed);
            Menu.AddToMainMenu();
        }

        /**
        * Whenever a player executes an action, do the following
        * 1) if monkey mode is active, make the clone copy the actions
        * 2) if ez heal is active and player executes a command with eligible items (given in CloneOnlyItems), cancel command and make clone use it instead
        **/
        private static void Player_OnExecuteAction(Player sender, ExecuteOrderEventArgs args)
        {
            #region code for monkey
            if (Menu.Item("order").GetValue<StringList>().SelectedIndex == (int)Orders.Monkey && !Menu.Item("AutoPush.Enable").GetValue<KeyBind>().Active)
            {
                //Game.PrintMessage(args.Order.ToString(), MessageType.ChatMessage);
                if (args.Order != Order.Stop && args.Order != Order.AttackLocation && args.Order != Order.AttackTarget &&
                    args.Order != Order.Ability && args.Order != Order.AbilityTarget &&
                    args.Order != Order.AbilityLocation &&
                    args.Order != Order.MoveLocation && args.Order != Order.MoveTarget && args.Order != Order.Hold)
                    return;

                // make each tempest clone copy main hero's moves
                foreach (var hero in Objects.Tempest.GetCloneList(sender.Hero))
                {
                    Ability spell;
                    Ability needed;
                    switch (args.Order)
                    {
                        case Order.Stop:
                            hero.Stop();
                            break;
                        case Order.AttackLocation:
                            hero.Attack(args.TargetPosition);
                            break;
                        case Order.AttackTarget:
                            var target = args.Target;
                            hero.Attack(target as Unit);
                            break;
                        case Order.Ability:
                            spell = args.Ability;
                            needed = hero.FindSpell(spell.Name) ?? hero.FindItem(spell.Name);
                            if (needed != null && needed.CanBeCasted())
                            {
                                needed.UseAbility();
                            }
                            break;
                        case Order.AbilityTarget:
                            spell = args.Ability;
                            needed = hero.FindSpell(spell.Name) ?? hero.FindItem(spell.Name);
                            if (needed != null && needed.CanBeCasted())
                            {
                                needed.UseAbility(args.Target as Unit);
                            }
                            break;
                        case Order.AbilityLocation:
                            spell = args.Ability;
                            needed = hero.FindSpell(spell.Name) ?? hero.FindItem(spell.Name);
                            if (needed != null && needed.CanBeCasted())
                            {
                                needed.UseAbility(args.TargetPosition);
                            }
                            break;
                        case Order.MoveLocation:
                            hero.Move(args.TargetPosition);
                            break;
                        case Order.MoveTarget:
                            hero.Move(args.TargetPosition);
                            break;
                        case Order.AbilityTargetTree:
                            break;
                        case Order.ToggleAbility:
                            break;
                        case Order.Hold:
                            hero.Stop();
                            break;
                    }
                }
            }
            #endregion
            #region code for ez heal
            else if (Menu.Item("FirstClone").GetValue<bool>())
            {
                if (args.Order != Order.Ability && args.Order != Order.AbilityTarget &&
                    args.Order != Order.AbilityLocation)
                    return;
                if (!CloneOnlyItems.Contains(args.Ability.Name)) return;
                
                // use clone items instead of main hero's items
                foreach (var hero in Objects.Tempest.GetCloneList(sender.Hero).Where(x => x.Distance2D(sender.Hero) <= 1000))
                {
                    Ability spell;
                    Ability needed;
                    switch (args.Order)
                    {
                        case Order.Ability:
                            spell = args.Ability;
                            needed = hero.FindSpell(spell.Name) ?? hero.FindItem(spell.Name);
                            if (needed != null && needed.CanBeCasted())
                            {
                                needed.UseAbility(sender.Hero);
                                args.Process = false;
                            }
                            break;
                        case Order.AbilityTarget:
                            spell = args.Ability;
                            needed = hero.FindSpell(spell.Name) ?? hero.FindItem(spell.Name);
                            if (needed != null && needed.CanBeCasted())
                            {
                                needed.UseAbility(args.Target as Unit);
                                args.Process = false;
                            }
                            break;
                        case Order.AbilityLocation:
                            spell = args.Ability;
                            needed = hero.FindSpell(spell.Name) ?? hero.FindItem(spell.Name);
                            if (needed != null && needed.CanBeCasted())
                            {
                                needed.UseAbility(args.TargetPosition);
                                args.Process = false;
                            }
                            break;
                    }
                }
            }
            #endregion
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            //Drawing.DrawText($"x:{Game.MousePosition.X} y:{Game.MousePosition.Y}",Game.MouseScreenPosition+new Vector2(150,150),Color.Red,FontFlags.Custom);
            if (!_loaded) return;
            if (Menu.Item("AutoPush.DrawLine").GetValue<bool>() &&
                Menu.Item("AutoPush.Enable").GetValue<KeyBind>().Active &&
                !Menu.Item("hotkeyClone").GetValue<KeyBind>().Active)
            {
                var me = ObjectManager.LocalHero;
                foreach (var hero in Objects.Tempest.GetCloneList(me))
                {
                    var nearestTower =
                        Objects.Towers.GetTowers()
                            .Where(x => x.Team == hero.GetEnemyTeam())
                            .OrderBy(y => y.Distance2D(hero))
                            .FirstOrDefault() ?? Objects.Fountains.GetEnemyFountain();
                    var fountain = Objects.Fountains.GetAllyFountain();
                    var curlane = GetCurrentLane(hero);
                    var clospoint = GetClosestPoint(curlane);
                    var useThisShit = clospoint.Distance2D(fountain) - 250 > hero.Distance2D(fountain);

                    if (nearestTower == null) continue;
                    var pos222 = curlane == "mid" || !useThisShit ? nearestTower.Position : clospoint;
                    var w2Shero = Drawing.WorldToScreen(hero.Position);
                    var w2SPos = Drawing.WorldToScreen(pos222);
                    if ((w2Shero.X == 0 && w2Shero.Y == 0) || (w2SPos.X == 0 && w2SPos.Y == 0)) continue;
                    //Print($"w2shero: {w2shero.X}/{w2shero.Y}");
                    //Print($"w2sPos: {w2sPos.X}/{w2sPos.Y}");
                    Drawing.DrawLine(w2Shero, w2SPos, Color.YellowGreen);
                }
            }
            /*var position = Game.MousePosition;
            Drawing.DrawText($"{position.X},{position.Y},{position.Z}", Game.MouseScreenPosition, new Vector2(0, 50), Color.Red,
                        FontFlags.AntiAlias | FontFlags.DropShadow);*/
            Vector2 pos;
            if (Menu.Item("hotkeyClone").GetValue<KeyBind>().Active)
            {
                var startPos = new Vector2(Drawing.Width - 250, 100);
                var size = new Vector2(180, 90);
                Drawing.DrawRect(startPos, size, new Color(0, 0, 0, 100));
                Drawing.DrawRect(startPos, size, new Color(0, 0, 0, 255), true);
                Drawing.DrawText("Clone Mode is Active" + $"[{Utils.KeyToText(Menu.Item("hotkeyClone").GetValue<KeyBind>().Key)}]", startPos + new Vector2(10, 10),new Vector2(20), new Color(0, 155, 255),
                    FontFlags.AntiAlias | FontFlags.DropShadow | FontFlags.Additive | FontFlags.Custom |
                    FontFlags.StrikeOut);
                if (_globalTarget2 != null && _globalTarget2.IsAlive)
                {
                    pos = Drawing.WorldToScreen(_globalTarget2.Position);
                    Drawing.DrawText("CloneTarget", pos, new Vector2(0, 50), Color.Red,
                        FontFlags.AntiAlias | FontFlags.DropShadow);
                    var name = "materials/ensage_ui/heroes_horizontal/" + _globalTarget2.Name.Replace("npc_dota_hero_", "") + ".vmat";
                    size=new Vector2(50,50);
                    Drawing.DrawRect(startPos + new Vector2(10, 35), size + new Vector2(13, -6),
                        Drawing.GetTexture(name));
                    Drawing.DrawRect(startPos + new Vector2(10, 35), size + new Vector2(14, -5),
                        new Color(0, 0, 0, 255), true);
                    foreach (var me in Objects.Tempest.GetCloneList(_mainHero).ToList())
                    {
                        DrawEffects(me,_globalTarget2);
                    }
                    foreach (var me in Objects.Necronomicon.GetNecronomicons(_mainHero).ToList())
                    {
                        DrawEffects(me,_globalTarget2);
                    }
                }
            }
            else if (Menu.Item("AutoPush.Enable").GetValue<KeyBind>().Active)
            {
                var startPos = new Vector2(Drawing.Width - 250, 100);
                var size = new Vector2(180, 40);
                Drawing.DrawRect(startPos, size, new Color(0, 0, 0, 100));
                Drawing.DrawRect(startPos, size, new Color(0, 0, 0, 255), true);
                Drawing.DrawText("AutoPush is Active " + $"[{Utils.KeyToText(Menu.Item("AutoPush.Enable").GetValue<KeyBind>().Key)}]", startPos + new Vector2(10, 10), new Vector2(20), new Color(0, 155, 255),
                    FontFlags.AntiAlias | FontFlags.DropShadow | FontFlags.Additive | FontFlags.Custom |
                    FontFlags.StrikeOut);
            }
            if (Menu.Item("AutoHeal.Enable").GetValue<KeyBind>().Active)
            {
                var startPos = new Vector2(Drawing.Width - 250, 190);
                var size = new Vector2(180, 30);
                Drawing.DrawRect(startPos, size, new Color(0, 0, 0, 100));
                Drawing.DrawRect(startPos, size, new Color(0, 0, 0, 255), true);
                Drawing.DrawText("AutoHeal is Active" + $"[{Utils.KeyToText(Menu.Item("AutoHeal.Enable").GetValue<KeyBind>().Key)}]", startPos + new Vector2(10, 10), new Vector2(20), new Color(0, 155, 255),
                    FontFlags.AntiAlias | FontFlags.DropShadow | FontFlags.Additive | FontFlags.Custom |
                    FontFlags.StrikeOut);
            }

            if (_globalTarget == null || !_globalTarget.IsAlive) return;
            pos = Drawing.WorldToScreen(_globalTarget.Position);
            Drawing.DrawText("Target", pos, new Vector2(0, 50), Color.Red, FontFlags.AntiAlias | FontFlags.DropShadow);
            foreach (var me in Objects.Tempest.GetCloneList(_mainHero).ToList())
            {
                DrawEffects(me, _globalTarget2);
            }
            foreach (var me in Objects.Necronomicon.GetNecronomicons(_mainHero).ToList())
            {
                DrawEffects(me, _globalTarget2);
            }
            DrawEffects(_mainHero, _globalTarget2);
        }

        /**
        * On each game tick, do the following
        * 1) make clone auto heal if enabled
        * 2) spam spark if enabled
        * 3) use clone combo attack if enabled
        * 4) use auto midas if enabled
        * 5) use generic hotkey for pushing (?) if enabled. 
        * 6) defaults to using combo1 (?)
        **/
        private static void Game_OnUpdate(EventArgs args)
        {
<<<<<<< HEAD
            if (_mainHero == null || !_mainHero.IsValid)
                _mainHero = ObjectManager.LocalHero;
=======
            _mainHero = ObjectManager.LocalHero;
>>>>>>> origin/master
            
            #region standard checks for loader and in-game status
            //Print($"_mainHero: {_mainHero.Position.X}/{_mainHero.Position.Y}/{_mainHero.Position.Z}");
            if (!_loaded)
            {
                if (!Game.IsInGame || _mainHero == null || _mainHero.ClassID != ClassID.CDOTA_Unit_Hero_ArcWarden)
                {
                    return;
                }
                _loaded = true;
                Game.PrintMessage(
                    "<font face='Comic Sans MS, cursive'><font color='#00aaff'>" + Menu.DisplayName + " By Jumpering" +
                    " loaded!</font> <font color='#aa0000'>v" + Assembly.GetExecutingAssembly().GetName().Version,
                    MessageType.LogMessage);
                LastAttackStart.Clear();
                LastActivity.Clear();
                _myHull = _mainHero.HullRadius;
            }

            if (!Game.IsInGame || _mainHero == null)
            {
                _loaded = false;
                return;
            }
            if (Game.IsPaused) return;
            #endregion

            _tick = Environment.TickCount;
            NetworkActivity act;
            var handle = _mainHero.Handle;
            if (!LastActivity.TryGetValue(handle, out act) || _mainHero.NetworkActivity != act)
            {
                LastActivity.Remove(handle);
                LastActivity.Add(handle, _mainHero.NetworkActivity);
                if (_mainHero.IsAttacking())
                {
                    LastAttackStart.Remove(handle);
                    LastAttackStart.Add(handle,_tick);
                }
            }
            foreach (var clone in Objects.Tempest.GetCloneList(_mainHero))
            {
                #region auto heal code
                if (Menu.Item("AutoHeal.Enable").GetValue<KeyBind>().Active)
                {
                    var enemy = ObjectManager.GetEntities<Unit>()
                            .Any(
                                x =>
                                    x.Team == _mainHero.GetEnemyTeam() && x.IsAlive && x.IsVisible &&
                                    x.Distance2D(_mainHero) < Menu.Item("AutoHeal.Range").GetValue<Slider>().Value);
                    if (!enemy) 
                    {
                        CloneUseHealItems(clone, _mainHero, clone.Distance2D(_mainHero));
                    }
                }
                #endregion

                // following code updates lastactivity and lastattack state
                handle = clone.Handle;
                if (LastActivity.TryGetValue(handle, out act) && clone.NetworkActivity == act) continue;
                LastActivity.Remove(handle);
                LastActivity.Add(handle, clone.NetworkActivity);
                if (!clone.IsAttacking()) continue;
                LastAttackStart.Remove(handle);
                LastAttackStart.Add(handle, _tick);
            }

            #region code for spark spam
            if (Menu.Item("spamHotkey").GetValue<KeyBind>().Active)
            {
                SparkSpam(_mainHero);
                return;
            }
            #endregion

<<<<<<< HEAD
            #region Flusher
            if (_globalTarget2 != null)
                FlushEffectForDyingUnits();
            #endregion

=======
>>>>>>> origin/master
            #region code for clone combo2 hotkey
            if (Menu.Item("hotkeyClone").GetValue<KeyBind>().Active)
            {
                // if target is not valid, set target to the hero closest to the mouse
                if (_globalTarget2 == null || !_globalTarget2.IsValid || !_globalTarget2.IsAlive)
                {
                    _globalTarget2 = ClosestToMouse(_mainHero, 500);
                }
                if (_globalTarget2 != null && _globalTarget2.IsValid && _globalTarget2.IsAlive)
                {
                    DoCombo2(_mainHero, _globalTarget2);
                }
            }
            else
            {
                _globalTarget2 = null;
                if (!Menu.Item("hotkey").GetValue<KeyBind>().Active) FlushEffect();
            }
            #endregion

            #region code for auto midas 
            var midas = _mainHero.FindItem("item_hand_of_midas");
            if (midas != null && Menu.Item("AutoMidas").GetValue<bool>())
            {
                if (midas.CanBeCasted() && Utils.SleepCheck(_mainHero.Handle + "midas") && _mainHero.IsAlive && !_mainHero.IsInvisible())
                {
                    var enemy = ObjectManager.GetEntities<Unit>()
                            .Where(
                                x =>
                                    !x.IsMagicImmune() && x.Team != _mainHero.Team &&
                                    (x.ClassID == ClassID.CDOTA_BaseNPC_Creep_Lane ||
                                     x.ClassID == ClassID.CDOTA_BaseNPC_Creep_Siege ||
                                     x.ClassID == ClassID.CDOTA_BaseNPC_Creep_Neutral) && x.IsSpawned && x.IsAlive &&
                                    x.Distance2D(_mainHero) <= 600).OrderByDescending(x => x.Health)
                            .DefaultIfEmpty(null)
                            .FirstOrDefault();
                    if (enemy != null)
                    {
                        Utils.Sleep(500, _mainHero.Handle + "midas");
                        midas.UseAbility(enemy);
                    }
                }
                foreach (var clone in Objects.Tempest.GetCloneList(_mainHero).Where(x => Utils.SleepCheck(x.Handle + "midas")))
                {
                    midas = clone.FindItem("item_hand_of_midas");
                    if (midas == null || !midas.CanBeCasted()) continue;
                    var enemy = ObjectManager.GetEntities<Unit>()
                            .Where(
                                x =>
                                    !x.IsMagicImmune() && x.Team != _mainHero.Team &&
                                    (x.ClassID == ClassID.CDOTA_BaseNPC_Creep_Lane ||
                                     x.ClassID == ClassID.CDOTA_BaseNPC_Creep_Siege ||
                                     x.ClassID == ClassID.CDOTA_BaseNPC_Creep_Neutral) && x.IsSpawned && x.IsAlive &&
                                    x.Distance2D(clone) <= 600).OrderByDescending(x => x.Health)
                            .DefaultIfEmpty(null)
                            .FirstOrDefault();
                    if (enemy == null) continue;
                    Utils.Sleep(500, clone.Handle + "midas");
                    midas.UseAbility(enemy);
                }
            }
            #endregion

            #region code for generic hotkey(?)
            if (!Menu.Item("hotkey").GetValue<KeyBind>().Active)
            {
                _globalTarget = null;
                if (Menu.Item("AutoPush.Enable").GetValue<KeyBind>().Active && _globalTarget2==null)
                    AutoPush(_mainHero);

                return;
            }
            #endregion

            // if globaltarget is not set and target is LockTarget is not set, set target to the unit closest to the mouse
            if (_globalTarget == null || !_globalTarget.IsValid || !Menu.Item("LockTarget").GetValue<bool>())
            {
                _globalTarget = ClosestToMouse(_mainHero, 300);
            }
            if (_globalTarget == null || !_globalTarget.IsValid || !_globalTarget.IsAlive) return;

            DoCombo(_mainHero, _globalTarget);
            
        }

        /**
        * AutoPush takes control of all available units and sends each unit to DoShit() to push
        * arg me: main hero (used to check units controlled by main hero)
        **/

        private static void AutoPush(Hero me)
        {
            /*var curlane2 = GetCurrentLane(me);
            var clospoint2 = GetClosestPoint(curlane2);
            Print($"{curlane2}: to: x:{clospoint2.X}/ y:{clospoint2.Y}");*/

            // make each clone doshit
            foreach (var hero in Objects.Tempest.GetCloneList(me))
            {
                DoShit(hero, true);
            }
            // make each illusion do shit
            foreach (
                var source in
                    ObjectManager.GetEntities<Hero>()
                        .Where(x => x.IsIllusion && Utils.SleepCheck("Tempest.Attack.Cd" + x.Handle) && !x.IsAttacking())
                )
            {
                //source.Attack(pos);
                DoShit(source);
                Utils.Sleep(350, "Tempest.Attack.Cd" + source.Handle);
            }
            // make each necronomicon unit doshit
            foreach (
                var necr in
                    Objects.Necronomicon.GetNecronomicons(me)
                        .Where(
                            x =>Utils.SleepCheck(x.Handle + "AutoPush.Attack") && !x.IsAttacking()))
            {
                if (Menu.Item("AntiFeed.Enable").GetValue<bool>() &&
                    Ensage.Common.Objects.Heroes.GetByTeam(necr.GetEnemyTeam())
                        .Any(
                            x =>
                                x.IsAlive && x.IsVisible &&
                                x.Distance2D(necr) <= Menu.Item("AntiFeed.Range").GetValue<Slider>().Value))
                {
                    necr.Move(Objects.Fountains.GetAllyFountain().Position);
                }
                else
                {
                    DoShit(necr);
                }

                Utils.Sleep(1000, necr.Handle + "AutoPush.Attack");
            }
        }
        /**
        * DoShit takes in a unit and makes the unit do pushing related actions:
        * 1) boots of travel to creep furthest away from unit
        * 2) push the current lane
        * 3) attack tower with shield if nearby
        * 4) attack creeps if nearby
        * 5) if enemy creeps are nearby, use mjollnir and necronomicon
        * args hero: unit to control
        * args isTempest: passed to easily distinguish between clone unit and other units
        **/
        private static void DoShit(Unit hero, bool isTempest=false)
        {
            // setting variables
            var handle = hero.Handle;
            var items = isTempest ? hero.Inventory.Items.ToList() : null;
            var travelBoots = isTempest?
                items.FirstOrDefault(
                    x =>
                        (x.Name == "item_travel_boots" ||
                        x.Name == "item_travel_boots_2") && x.CanBeCasted() &&
                        Utils.SleepCheck("Tempest.Travels.Cd" + handle)) : null;
            var autoPushItems =isTempest ? 
                items.Where(
                    x =>
                        AutoPushItems.Contains(x.Name) && x.CanBeCasted() &&
                        Utils.SleepCheck("Tempest.AutoPush.Cd" + handle + x.Name)) : null;
            var myCreeps = Objects.LaneCreeps.GetCreeps().Where(x => x.Team == hero.Team).ToList();
            var enemyCreeps = Objects.LaneCreeps.GetCreeps().Where(x => x.Team != hero.Team).ToList();
            var creepWithEnemy =
                myCreeps.FirstOrDefault(
                    x => x.MaximumHealth * 65 / 100 < x.Health && enemyCreeps.Any(y => y.Distance2D(x) <= 1000));
            var isChannel = isTempest && hero.IsChanneling();

            // code for using boots of travel
            // note: chooses creep furthest away from unit to TP to
            if (travelBoots != null && !enemyCreeps.Any(x => x.Distance2D(hero) <= 1000) && !isChannel && Menu.Item("AutoPush.Travels").GetValue<bool>())
            {
                if (creepWithEnemy == null)
                {
                    creepWithEnemy = myCreeps.OrderByDescending(x => x.Distance2D(hero)).FirstOrDefault(); 
                }
                if (creepWithEnemy != null)
                {
                    travelBoots.UseAbility(creepWithEnemy);
                    Utils.Sleep(500, "Tempest.Travels.Cd" + handle);
                    return;
                }
            }
            if (isChannel) return;
            
            var nearestTower =
                    Objects.Towers.GetTowers()
                        .Where(x => x.Team == hero.GetEnemyTeam())
                        .OrderBy(y => y.Distance2D(hero))
                        .FirstOrDefault() ?? Objects.Fountains.GetEnemyFountain();
            var fountain = Objects.Fountains.GetAllyFountain();
            var curlane = GetCurrentLane(hero);
            var clospoint = GetClosestPoint(curlane);
            var useThisShit = clospoint.Distance2D(fountain) - 250 > hero.Distance2D(fountain);
            // useThisShit will return true if unit is closer to the fountain than the clospoint

            if (nearestTower != null)
            {
                var pos = (curlane == "mid" || !useThisShit) ? nearestTower.Position : clospoint;
                // if unit is at mid or clospoint is closer than the unit to the fountain, push the nearest tower
                // otherwise, push the closest point

                // if unit is a tempest and is close to the tower, use shield and attack tower
                if (nearestTower.Distance2D(hero) <= 900 && isTempest)
                {
                    if (Utils.SleepCheck("Tempest.Attack.Tower.Cd" + handle))
                    {
                        var spell = hero.Spellbook.Spell2;
                        if (spell != null && spell.CanBeCasted() && Utils.SleepCheck("shield" + handle)) // handle used to uniquely identify the current hero's cooldowns
                        {
                            spell.UseAbility(Prediction.InFront(hero, 100));
                            Utils.Sleep(1500, "shield" + handle);
                        }
                        else if (!hero.IsAttacking())
                        {
                            hero.Attack(nearestTower);
                        }
                        Utils.Sleep(1000, "Tempest.Attack.Tower.Cd" + handle);
                    }
                }
                // make the unit issue an attack command at the position pos 
<<<<<<< HEAD
                else if (Utils.SleepCheck("Tempest.Attack.Cd" + handle) && !hero.IsAttacking() && isTempest)
=======
                else if (Utils.SleepCheck("Tempest.Attack.Cd" + handle) && !hero.IsAttacking())
>>>>>>> origin/master
                {
                    hero.Attack(pos);
                    Utils.Sleep(1000, "Tempest.Attack.Cd" + handle);
                }
<<<<<<< HEAD
                // smart attack for necrobook (unaggro under tower)
                if (!isTempest && Utils.SleepCheck(hero.StoredName() + "attack"))
                {
                    SmartAttack(hero, myCreeps, nearestTower, pos);
                }
=======

>>>>>>> origin/master
                // if there are creeps in the vicinity, make tempest use mjollnir and necronomicon
                if (enemyCreeps.Any(x => x.Distance2D(hero) <= 800) && isTempest)
                {
                    foreach (var item in autoPushItems)
                    {
                        if (item.Name != "item_mjollnir")
                        {
                            item.UseAbility();
                        }
                        else
                        {
                            var necros =
                                Objects.Necronomicon.GetNecronomicons(hero)
                                    .FirstOrDefault(x => x.Distance2D(hero) <= 500 && x.Name.Contains("warrior"));
                            if (necros != null) item.UseAbility(necros);
                        }
                        Utils.Sleep(350, "Tempest.AutoPush.Cd" + handle + item.Name);
                    }
                }
            }
        }

<<<<<<< HEAD
        private static void SmartAttack(Unit me, List<Unit> myCreeps, Unit nearestTower, Vector3 pos)
        {
            var name = me.StoredName();
            if (Menu.Item("AutoPush.UnAggro.Enable").GetValue<bool>() &&
                myCreeps.Any(x => x.Distance2D(nearestTower) <= 800) && me.Distance2D(nearestTower) <= 1000)
            {
                var hpwasChanged = CheckForChangedHealth(me);
                if (hpwasChanged)
                {
                    var allyCreep = myCreeps.OrderBy(x => x.Distance2D(me)).First();
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
                        var p = new Vector3((float) (me.Position.X - 1000*Math.Cos(ang)),
                            (float) (me.Position.Y - 1000*Math.Sin(ang)), 0);
                        me.Move(p);
                        Utils.Sleep(500, name + "attack");
                    }
                }
                else
                {
                    me.Attack(pos);
                    Utils.Sleep(500, name + "attack");
                }
            }
            else
            {
                me.Attack(pos);
                Utils.Sleep(500, name + "attack");
            }
        }

        private static readonly Dictionary<Unit,uint> LastCheckedHp = new Dictionary<Unit, uint>();
        private static bool CheckForChangedHealth(Unit me)
        {
            uint health;
            if (!LastCheckedHp.TryGetValue(me, out health))
            {
                LastCheckedHp.Add(me,me.Health);
            }
            var boolka = health > me.Health;
            LastCheckedHp[me] = me.Health;
            return boolka;
        }

=======
>>>>>>> origin/master
        /**
        * GetCurrentLane returns the lane in string that the unit me is currently in
        * uses LaneDictionary with 9 waypoints to determine closest lane (room for possible improvement)
        * args me: the unit in question
        * return string: the lane. either "bot", "top" or "middle"
        **/
        private static string GetCurrentLane(Unit me)
        {
            return LaneDictionary.OrderBy(x => x.Key.Distance2D(me)).First().Value;
        }

        /**
        * GetClosestPoint takes in a lane in string and returns a position given by the 0th, 3rd, or 6th key in LaneDictionary
        * (Require author documentation to explain what these positions mean)
        * args pos: the position, "top", "bot" or "middle"
        * return: vector3 position given in LaneDictionary
        **/
        private static Vector3 GetClosestPoint(string pos)
        {
            /*return LaneDictionary.Where(
                y => y.Value == pos && y.Key.Distance2D(Objects.Fountains.GetAllyFountain()) <= y.Key.Distance2D(me))
                .OrderBy(x => x.Key.Distance2D(me)).First().Key;*/
            
            var list=LaneDictionary.Keys.ToList();
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

        private static void CloneUseHealItems(Hero clone, Hero me, float distance)
        {
            var handle = clone.Handle;
            
            var items = clone.Inventory.Items.Where(x => CloneHealItems.Contains(x.Name) && x.CanBeCasted() && x.CastRange+200>distance && Utils.SleepCheck(handle+x.Name));
            foreach (var item in items)
            {
                switch (item.Name)
                {
                    case "item_flask":
                        item.UseAbility(me);
                        Utils.Sleep(500,(handle+item.Name).ToString(CultureInfo.InvariantCulture));
                        break;
                    case "item_clarity":
                        item.UseAbility(me);
                        Utils.Sleep(500, (handle + item.Name).ToString(CultureInfo.InvariantCulture));
                        break;
                    case "item_enchanted_mango":
                        item.UseAbility(me);
                        Utils.Sleep(500, (handle + item.Name).ToString(CultureInfo.InvariantCulture));
                        break;
                    case "item_sphere":
                        item.UseAbility(me);
                        Utils.Sleep(500, (handle + item.Name).ToString(CultureInfo.InvariantCulture));
                        break;
                    case "item_bottle":
                        var bottlemod = me.HasModifier("modifier_bottle_regeneration");
                        if (!bottlemod && item.CurrentCharges > 0)
                        {
                            item.UseAbility(me);
                            Utils.Sleep(500, (handle + item.Name).ToString(CultureInfo.InvariantCulture));
                        }
                        break;
                    default:
                        item.UseAbility();
                        Utils.Sleep(500, (handle + item.Name).ToString(CultureInfo.InvariantCulture));
                        break;
                }
            }
        }

        /**
        * DoCombo2 takes in a hero, me, and a hero, target, and does the following
        * 1) Use dagger towards the target if available
        * 2) Use all spells on target handled by SpellsUsage()
        * 3) Use all possible items handled by ItemUsage()
        * 4) Attacks enemy with orbwalking (if enabled) or with regular attack
        * 5) Make illusions attack if is illusion
        * 6) Make necronomicon units attack and use spell on target
        **/
        private static void DoCombo2(Hero me, Hero target)
        {
            double targetHull = target.HullRadius;
            foreach (var hero in Objects.Tempest.GetCloneList(me))
            {
                var d = hero.Distance2D(target) - _myHull - targetHull;
                var inv = hero.Inventory.Items;
                var enumerable = inv as Item[] ?? inv.ToArray();

                // use dagger if available
                var dagger = enumerable.Any(x => x.Name == "item_blink" && x.Cooldown == 0);
                SpellsUsage(hero, target, d, dagger);

                // uses all items available
                ItemUsage(hero, enumerable, target, d,
                    Menu.Item("BkbUsage").GetValue<StringList>().SelectedIndex == (int) BkbUsage.Clones ||
                    Menu.Item("BkbUsage").GetValue<StringList>().SelectedIndex == (int) BkbUsage.All, true);

                // do orbwalking if enabled
                // otherwise simply attack target
                if (Menu.Item("OrbWalking.Enable").GetValue<bool>())
                {
                    Orbwalk(hero, target, Menu.Item("OrbWalking.bonusWindupMs").GetValue<Slider>().Value);
                }
                else
                {
                    if (!Utils.SleepCheck("clone_attacking" + hero.Handle)) continue;
                    hero.Attack(target);
                    Utils.Sleep(350, "clone_attacking" + hero.Handle);
                }
            }
            var illusions = ObjectManager.GetEntities<Hero>().Where(x => x.IsAlive && x.IsControllable && x.Team == me.Team && x.IsIllusion && !x.HasModifier("modifier_kill")).ToList();
            foreach (var illusion in illusions.TakeWhile(illusion => Utils.SleepCheck("clone_attacking" + illusion.Handle) && illusion.Distance2D(target) <= 1500))
            {
                illusion.Attack(target);
                Utils.Sleep(350, "clone_attacking" + illusion.Handle);
            }
            var necr = Objects.Necronomicon.GetNecronomicons(me);
            foreach (var necronomicon in necr.TakeWhile(illusion => Utils.SleepCheck("clone_attacking" + illusion.Handle) && illusion.Distance2D(target) <= 1500 && !Equals(illusion, me)))
            {
                necronomicon.Attack(target);
                var spell = necronomicon.Spellbook.Spell1;
                if (spell != null && spell.CanBeCasted(target) &&
                    necronomicon.Distance2D(target) <= spell.CastRange + necronomicon.HullRadius + targetHull+50 &&
                    Utils.SleepCheck(spell.Name + "clone_attacking" + necronomicon.Handle))
                {
                    spell.UseAbility(target);
                    Utils.Sleep(300, spell.Name + "clone_attacking" + necronomicon.Handle);
                }
                Utils.Sleep(600, "clone_attacking" + necronomicon.Handle);
            }
        }

        private static void SparkSpam(Hero me)
        {
            foreach (var hero in Objects.Tempest.GetCloneList(me))
            {
                var spell = hero.Spellbook.Spell3;
                if (spell == null || !spell.CanBeCasted() || !Utils.SleepCheck("spam" + hero.Handle)) continue;
                spell.UseAbility(Game.MousePosition);
                Utils.Sleep(400, "spam" + hero.Handle);
            }
            if (!me.IsAlive || !Utils.SleepCheck("spam" + me.Handle)) return;
            {
                var spell = me.Spellbook.Spell3;
                if (spell == null || !spell.CanBeCasted()) return;
                spell.UseAbility(Game.MousePosition);
                Utils.Sleep(400, "spam" + me.Handle);
            }
        }

        private static void DoCombo(Hero me, Hero target)
        {
            var distance = me.Distance2D(target);
            IEnumerable<Item> inv;
            Item[] enumerable;
            bool dagger;
            double targetHull = target.HullRadius;
            if (Menu.Item("order").GetValue<StringList>().SelectedIndex == (int)Orders.Caster && !Menu.Item("hotkeyClone").GetValue<KeyBind>().Active)
            {
                foreach (var hero in Objects.Tempest.GetCloneList(me))
                {
                    var d = hero.Distance2D(target)-_myHull-targetHull;
                    inv = hero.Inventory.Items;
                    enumerable = inv as Item[] ?? inv.ToArray();
                    dagger = enumerable.Any(x => x.Name == "item_blink" && x.Cooldown == 0);
                    SpellsUsage(hero, target, d, dagger);
                    ItemUsage(hero, enumerable, target, d,
                        Menu.Item("BkbUsage").GetValue<StringList>().SelectedIndex == (int) BkbUsage.Clones ||
                        Menu.Item("BkbUsage").GetValue<StringList>().SelectedIndex == (int) BkbUsage.All, true);
                    if (Menu.Item("OrbWalking.Enable").GetValue<bool>())
                    {
                        Orbwalk(hero, target, Menu.Item("OrbWalking.bonusWindupMs").GetValue<Slider>().Value);
                    }
                    {
                        if (!Utils.SleepCheck("clone_attacking" + hero.Handle)) continue;
                        hero.Attack(target);
                        Utils.Sleep(350, "clone_attacking" + hero.Handle);
                    }
                }
            }
            var illusions = ObjectManager.GetEntities<Hero>().Where(x => x.IsAlive && x.IsControllable && x.Team == me.Team && x.IsIllusion && !x.HasModifier("modifier_kill")).ToList();
            foreach (var illusion in illusions.TakeWhile(illusion => Utils.SleepCheck("clone_attacking" + illusion.Handle)))
            {
                illusion.Attack(target);
                Utils.Sleep(350, "clone_attacking" + illusion.Handle);
            }
            var necr = Objects.Necronomicon.GetNecronomicons(me);
            foreach (var necronomicon in necr.TakeWhile(illusion => Utils.SleepCheck("clone_attacking" + illusion.Handle) && illusion.Distance2D(target) <= 1500 && !Equals(illusion, me)))
            {
                necronomicon.Attack(target);
                var spell = necronomicon.Spellbook.Spell1;

                if (spell != null && spell.CanBeCasted(target) &&
                    necronomicon.Distance2D(target) <= spell.CastRange + necronomicon.HullRadius + targetHull &&
                    Utils.SleepCheck(spell.Name + "clone_attacking" + necronomicon.Handle))
                {
                    spell.UseAbility(target);
                    Utils.Sleep(300, spell.Name + "clone_attacking" + necronomicon.Handle);
                }
                Utils.Sleep(600, "clone_attacking" + necronomicon.Handle);
            }
            if (!me.IsAlive) return;
            inv = me.Inventory.Items;
            enumerable = inv as Item[] ?? inv.ToArray();
            dagger = enumerable.Any(x=>x.Name=="item_blink" && x.Cooldown==0);
            SpellsUsage(me, target, distance, dagger);
            ItemUsage(me,enumerable, target, distance,
                Menu.Item("BkbUsage").GetValue<StringList>().SelectedIndex == (int) BkbUsage.Me ||
                Menu.Item("BkbUsage").GetValue<StringList>().SelectedIndex == (int) BkbUsage.All);
            Orbwalk(me,target);
        }

        /**
        * SpellsUsage does the following
        * 1) Use Q spell on target
        * 2) Use W if close enough to target and blink dagger not available
        * 3) Use E with prediction (based on enemy pathing) if blink dagger is not available
        * 4) Use R if available (?)
        **/
        private static void SpellsUsage(Hero me, Hero target, double distance,bool daggerIsReady)
        {
            var spellbook = me.Spellbook;
            var q = spellbook.SpellQ;
            var w = spellbook.SpellW;
            var e = spellbook.SpellE;
            if (q != null && q.CanBeCasted() && q.CastRange >= distance && Utils.SleepCheck(me.Handle+q.Name))
            {
                q.UseAbility(target);
                Utils.Sleep(500, me.Handle + q.Name);
            }
            if (w != null && w.CanBeCasted() && Utils.SleepCheck(w.Name) && !me.HasModifier("modifier_arc_warden_magnetic_field") && distance <= 600 && !daggerIsReady)
            {
                w.UseAbility(Prediction.InFront(me,200));
                Utils.Sleep(500, w.Name);
            }
            if (e != null && e.CanBeCasted() && Utils.SleepCheck(me.Handle + e.Name) && !daggerIsReady)
            {
                var predVector3 = target.NetworkActivity == NetworkActivity.Move && Menu.Item("usePrediction").GetValue<bool>()
                        ? Prediction.InFront(target, target.MovementSpeed * 3 + Game.Ping / 1000)
                        : target.Position;
                e.UseAbility(predVector3);
                Utils.Sleep(500, me.Handle + e.Name);
            }
            var r = me.Spellbook.SpellR;
            if (r == null || !r.CanBeCasted() || !Utils.SleepCheck(me.Handle + r.Name) || distance>900) return;
            r.UseAbility();
            Utils.Sleep(500, me.Handle + r.Name);
        }

        /**
        * ItemUsage takes in a Hero me, its inventory inv, a Hero target, the distance between me and target, a boolean usebkb and byillusion
        * and uses all items as necessary
        * 1) Uses blink dagger to a point given by Dagger.CloseRange menu option
        * 2) Uses all other items specified in the Items list (line 53)
        * 3) Uses diffusal blade (to purge or dispel) if enabled
        * 4) Uses bkb if enabled
<<<<<<< HEAD
        * 5) Uses ultimate if all items expect of refresher was casted
        **/
        private static void ItemUsage(Hero me, IEnumerable<Item> inv, Hero target, double distance, bool useBkb, bool byIllusion = false)
        {
            if (me.IsChanneling() || !Utils.SleepCheck("DaggerTime")) return;
=======
        * 5) Uses refresher (?) code looks weird. Require author review
        **/
        private static void ItemUsage(Hero me, IEnumerable<Item> inv, Hero target, double distance, bool useBkb, bool byIllusion = false)
        {
            if (me.IsChanneling()) return;
>>>>>>> origin/master
            // use all items given in Items list (line 53)
            var inventory =
                inv.Where(x => Utils.SleepCheck(x.Name + me.Handle) && x.CanBeCasted()
                    /* && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(x.Name)*/).ToList();
            var items =
                inventory.Where(
                    x =>
                        Items.Keys.Contains(x.Name) &&
                        ((x.CastRange == 0 &&
                          distance <=
                          (x.Name == "item_blink" ? 1150 + Menu.Item("Dagger.CloseRange").GetValue<Slider>().Value : 800)) ||
                         x.CastRange >= distance)).OrderByDescending(y => Items[y.StoredName()]);
            //var count = 0;
            foreach (var item in items)
            {
<<<<<<< HEAD
                //Print(++count+". "+item.Name+" ("+Items[item.Name]+")");
=======
>>>>>>> origin/master
                // code for using blink
                if (item.Name == "item_blink")
                {
                    // if target is more than blink range away
                    if (distance > 1150)
                    {
                        // sets blink point to position given by CloseRange menu slider
                        var point = new Vector3(
                            (float)
                                (target.Position.X -
                                 Menu.Item("Dagger.CloseRange").GetValue<Slider>().Value*
                                 Math.Cos(me.FindAngleBetween(target.Position, true))),
                            (float)
                                (target.Position.Y -
                                 Menu.Item("Dagger.CloseRange").GetValue<Slider>().Value*
                                 Math.Sin(me.FindAngleBetween(target.Position, true))),
                            target.Position.Z);
                        var dist = me.Distance2D(point);
                        if (dist >= Menu.Item("Dagger.MinDistance").GetValue<Slider>().Value && dist <= 1150)
                        {
                            item.UseAbility(point);
                            Utils.Sleep(500, item.Name + me.Handle);
                            Utils.Sleep(250, "DaggerTime");
                            return;
                        }
                    }
                    else if (distance>Menu.Item("Dagger.MinDistance").GetValue<Slider>().Value)
                    {
                        item.UseAbility(target.Position);
                        Utils.Sleep(500, item.Name + me.Handle);
                    }
                    continue;
                    //return;
                }
                item.UseAbility();
                item.UseAbility(target);
                item.UseAbility(target.Position);
                item.UseAbility(me);
                Utils.Sleep(500, item.Name + me.Handle);
            }
            
            // purge enemies if menu setting enabled
            var purgeAll = Menu.Item("Diffusal.PurgEnemy").GetValue<StringList>().SelectedIndex == (int)PurgeSelection.AllEnemies;
            
            // if ItemUsage is called by combo1
            if (byIllusion)
            {
                var targets = ObjectManager.GetEntities<Hero>()
                    .Where(
                        x =>
                            x.IsValid && x.IsAlive && x.IsVisible && x.Team!=me.Team && x.Distance2D(me) <= 750 &&
                            (purgeAll || Equals(target, x)) &&
                             !x.IsStunned() && !x.IsHexed())
                    .ToList();
                foreach (var hero in targets)
                {
                    var mod =
                        hero.HasModifier("modifier_item_diffusal_blade_slow");
                    var repel = hero.FindModifier("modifier_omniknight_repel")!=null;
                    var guard = hero.FindModifier("modifier_omninight_guardian_angel")!=null;
                    if (mod && !repel && !guard)
                    {
                        continue;
                    }
                    var items2 =
                    inventory.Where(
                        x =>
                            CloneOnlyComboItems.Contains(x.Name) &&
                            (me.Distance2D(hero) <= 650)).ToList();
                    foreach (var item in items2)
                    {
                        item.UseAbility(hero);
                        Utils.Sleep(500, item.Name + me.Handle);
                    }
                }
            }

            // code for diffusal blade dispelling 
            var both = Menu.Item("Diffusal.Dispel").GetValue<StringList>().SelectedIndex == (int)DispelSelection.Both;
            var main = Menu.Item("Diffusal.Dispel").GetValue<StringList>().SelectedIndex == (int)DispelSelection.Me;
            var tempest = Menu.Item("Diffusal.Dispel").GetValue<StringList>().SelectedIndex == (int)DispelSelection.Tempest;
            
            if (byIllusion && (both || main || tempest))
            {
                var dif = inventory.Where(
                    x =>
                        CloneOnlyComboItems.Contains(x.Name)).ToList();
                if (dif.Any())
                    TryToDispell(me, dif, both, main, tempest);
            }

            // code for using bkb
<<<<<<< HEAD
            if (useBkb && distance<900)
=======
            if (useBkb && distance<650)
>>>>>>> origin/master
            {
                var bkb = inventory.FirstOrDefault(x => x.Name == "item_black_king_bar");
                if (bkb != null && bkb.CanBeCasted() && Utils.SleepCheck(bkb.Name + me.Handle))
                {
                    bkb.UseAbility();
                    Utils.Sleep(500, bkb.Name + me.Handle);
                }
            }

<<<<<<< HEAD
            // Uses ultimate if all items expect of refresher was casted
=======
            // not sure what this chunk of code means. weird syntax.
>>>>>>> origin/master
            if (!items.Any()) return;
            {
                var r = me.Spellbook.SpellR;
                if (r == null || r.CanBeCasted()) return;
                var refresher = inventory.FirstOrDefault(x => x.Name == "item_refresher");
                refresher?.UseAbility();
                Utils.Sleep(500, refresher?.Name + me.Handle);
            }
        }

        private static void TryToDispell(Hero me, List<Item> toList, bool both, bool main, bool tempest)
        {
            var target = main ? _mainHero : tempest ? me : null;
            if (both)
            {
                var underShit = me.IsSilenced() || me.IsHexed() /*|| me.DebuffState*/;
                var isSilenced2 = _mainHero.IsSilenced();
                if (isSilenced2 && me.Distance2D(_mainHero) <= 600)
                {
                    foreach (var item in toList.Where(x => Utils.SleepCheck(x.Name + me.Handle)))
                    {
                        item.UseAbility(_mainHero);
                        Utils.Sleep(500, item.Name + me.Handle);
                    }
                }
                if (underShit)
                {
                    foreach (var item in toList)
                    {
                        item.UseAbility(me);
                        Utils.Sleep(500, item.Name + me.Handle);
                    }
                }
                foreach (var hero in Ensage.Common.Objects.Heroes.GetByTeam(me.Team).Where(x=>x!=null && x.IsValid && x.IsAlive && x.Distance2D(me)<=600 && (x.IsHexed() || x.IsSilenced())))
                {
                    foreach (var item in toList)
                    {
                        item.UseAbility(hero);
                        Utils.Sleep(500, item.Name + me.Handle);
                    }
                }
            }
            else
            {
                var isSilenced = target.IsSilenced();
                if (!isSilenced) return;
                foreach (var item in toList.Where(x => me.Distance2D(target) <= 600))
                {
                    item.UseAbility(target);
                    Utils.Sleep(500, item.Name + me.Handle);
                }
            }
        }

        private static void Orbwalk(Hero me,Unit target,float bonusWindupMs = 100,float bonusRange = 0)
        {
            if (me == null)
            {
                return;
            }
            var targetHull = 0f;
            if (target != null)
            {
                targetHull = target.HullRadius;
            }
            float distance = 0;
            if (target != null)
            {
                var pos = Prediction.InFront(
                    me,
                    (float)((Game.Ping / 1000 + me.GetTurnTime(target.Position)) * me.MovementSpeed));
                distance = pos.Distance2D(target) - me.Distance2D(target);
            }

            // target is valid if it is alive, visbile, not invulnerable, not in ethereal state, and is within attack range
            var isValid = target != null && target.IsValid && target.IsAlive && target.IsVisible && !target.IsInvul()
                          && !target.HasModifiers(new[] { "modifier_ghost_state", "modifier_item_ethereal_blade_slow" },
                              false) && target.Distance2D(me)
                          <= (me.GetAttackRange() + me.HullRadius + 50 + targetHull + bonusRange + Math.Max(distance, 0));

            // attack and set cooldown on attack timer if possible to attack
            if (isValid || (target != null && me.IsAttacking() && me.GetTurnTime(target.Position) < 0.1))
            {
                var canAttack = !AttackOnCooldown(me,target, bonusWindupMs)
                                && !target.IsAttackImmune() && !target.IsInvul() && me.CanAttack();
                if (canAttack && Utils.SleepCheck("!Orbwalk.Attack"))
                {
                    me.Attack(target);
                    Utils.Sleep(
                        UnitDatabase.GetAttackPoint(me) * 1000 + me.GetTurnTime(target) * 1000,
                        "!Orbwalk.Attack");
                    return;
                }
            }

            // do animation cancelling by walking to the target location if possible to cancel
            var canCancel = (CanCancelAnimation(me) && AttackOnCooldown(me,target, bonusWindupMs))
                            || (!isValid && !me.IsAttacking() && CanCancelAnimation(me));
            if (!canCancel || !Utils.SleepCheck("!Orbwalk.Move") || !Utils.SleepCheck("!Orbwalk.Attack"))
            {
                return;
            }
            if (target != null) me.Move(target.Position);
            Utils.Sleep(100, "!Orbwalk.Move");
        }

        private static bool AttackOnCooldown(Hero me, Entity target = null, float bonusWindupMs = 0)
        {
            if (me == null)
            {
                return false;
            }
            var turnTime = 0d;
            if (target != null)
            {
                //turnTime = me.GetTurnTime(target);
                turnTime = me.GetTurnTime(target)
                           + Math.Max(me.Distance2D(target) - me.GetAttackRange() - 100, 0) / me.MovementSpeed;
            }
            int lastAttackStart;
            LastAttackStart.TryGetValue(me.Handle,out lastAttackStart);
            return lastAttackStart + UnitDatabase.GetAttackRate(me)*1000 - Game.Ping - turnTime*1000 - 75
                   + bonusWindupMs >= _tick;
        }

        private static bool CanCancelAnimation(Hero me, float delay = 0f)
        {
            int lastAttackStart;
            LastAttackStart.TryGetValue(me.Handle, out lastAttackStart);
            var time = _tick - lastAttackStart;
            var cancelDur = UnitDatabase.GetAttackPoint(me) * 1000 - Game.Ping + 100 - delay;
            return time >= cancelDur;
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

<<<<<<< HEAD
        #region Effects

        private static readonly Dictionary<uint, ParticleEffect> Effects = new Dictionary<uint, ParticleEffect>();

        private static void DrawEffects(Entity me, Entity target)
        {
            ParticleEffect effect;
            var handle = me.Handle;
            if (!Effects.TryGetValue(handle, out effect))
            {
                Effects.Add(handle, new ParticleEffect(@"particles\ui_mouseactions\range_finder_tower_aoe.vpcf", target));
            }
            if (effect == null) return;
            effect.SetControlPoint(2, new Vector3(me.Position.X, me.Position.Y, me.Position.Z));
            effect.SetControlPoint(6, new Vector3(1, 0, 0));
            effect.SetControlPoint(7, new Vector3(target.Position.X, target.Position.Y, target.Position.Z));
        }

        private static void FlushEffect()
        {
            if (!Utils.SleepCheck("FlushCheck")) return;
            Utils.Sleep(500, "FlushCheck");

            ParticleEffect effect;
            uint handle;
            foreach (var me in Objects.Necronomicon.GetNecronomicons(_mainHero).ToList())
            {
                handle = me.Handle;
                if (!Effects.TryGetValue(handle, out effect)) continue;
                effect.Dispose();
                Effects.Remove(handle);
            }
            foreach (var me in Objects.Tempest.GetCloneList(_mainHero))
            {
                handle = me.Handle;
                if (!Effects.TryGetValue(handle, out effect)) continue;
                effect.Dispose();
                Effects.Remove(handle);
            }
            handle = _mainHero.Handle;
            if (!Effects.TryGetValue(handle, out effect)) return;
            effect.Dispose();
            Effects.Remove(handle);
        }

        private static void FlushEffectForDyingUnits()
        {
            if (!Utils.SleepCheck("FlushCheck.Fully")) return;
            Utils.Sleep(500, "FlushCheck.Fully");

            ParticleEffect effect;
            uint handle;
            foreach (var me in Objects.Necronomicon.GetFullyNecronomicons(_mainHero).Where(x => !x.IsAlive).ToList())
            {
                handle = me.Handle;
                if (!Effects.TryGetValue(handle, out effect)) continue;
                effect.Dispose();
                Effects.Remove(handle);
            }
            foreach (var me in Objects.Tempest.GetFullyCloneList(_mainHero).Where(x => !x.IsAlive).ToList())
            {
                handle = me.Handle;
                if (!Effects.TryGetValue(handle, out effect)) continue;
                effect.Dispose();
                Effects.Remove(handle);
            }
        }

        #endregion

=======
>>>>>>> origin/master
        #region functions to print to screen
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

        private static void Print(string toString, MessageType type = MessageType.ChatMessage)
        {
            Game.PrintMessage(toString, type);
        }
        #endregion
    }
}