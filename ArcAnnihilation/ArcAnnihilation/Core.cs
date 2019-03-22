using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Ensage;
using Ensage.Common;
using Ensage.Common.Enums;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using Ensage.Common.Objects.UtilityObjects;
using SharpDX;

namespace ArcAnnihilation
{
    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    public class Core
    {
        #region Variables
        //private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static Hero _mainHero;
        private static Vector3 _pushLaneTop = new Vector3(-5895, 5402, 384);
        private static Vector3 _pushLaneBot = new Vector3(5827, -5229, 384);
        private static Vector3 _pushLaneMid = new Vector3(1, 1, 384);
        private static float _myHull;
        private static bool _drawType;
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

        public static Menu Menu = new Menu("Arc Annihilation", "arc", true, "npc_dota_hero_arc_warden", true);
        private static Hero _globalTarget;
        private static Hero _globalTarget2;
        private static readonly List<Spell> SpellBaseList=new List<Spell>(); 
        private static readonly Dictionary<string, byte> Items = new Dictionary<string, byte>
        {
            {"item_hurricane_pike",1 },
            {"item_mask_of_madness", 7},
            {"item_ancient_janggo", 1},
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
            {"item_bloodthorn", 4},

            {"item_soul_ring", 4},
            {"item_blade_mail", 4},
            {"item_veil_of_discord", 4},
            {"item_heavens_halberd", 1},

            {"item_necronomicon", 2},
            {"item_necronomicon_2", 2},
            {"item_necronomicon_3", 2},
            {"item_mjollnir", 1},
            {Ensage.AbilityId.item_diffusal_blade.ToString(), 5},
            {Ensage.AbilityId.item_nullifier.ToString(), 5},
            //{ "item_hurricane_pike",1},

            {"item_sheepstick", 5},
            {"item_urn_of_shadows", 5}

            /*{"item_dust", 4}*/
        };
        private static readonly List<string> HideItemList=new List<string>
        {
            "item_phase_boots"
            ,"item_invis_sword"
            ,"item_silver_edge"
        }; 
        private static int CloseRange => Menu.Item("Dagger.CloseRange").GetValue<Slider>().Value;
        private static int OrbwalkerType => Menu.Item("OrbWalking.Type").GetValue<StringList>().SelectedIndex;
        private static int MinDistance => Menu.Item("Dagger.MinDistance").GetValue<Slider>().Value;
        private static int ExtraDistance => Menu.Item("Dagger.ExtraDistance").GetValue<Slider>().Value;
        private static int OrbMinDist => Menu.Item("OrbWalking.minDistance").GetValue<Slider>().Value;
        private static Sleeper _ethereal;
        private static MultiSleeper blinkSleeper;

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
            "item_dust",
            "item_diffusal_blade_2"
        };

        private static readonly List<string> CloneOnlyComboItems = new List<string>
        {
            "item_diffusal_blade",
            "item_diffusal_blade_2"
        };

        private static readonly List<string> AbilityList = new List<string>
        {
            "arc_warden_flux",
            "arc_warden_magnetic_field",
            "arc_warden_spark_wraith",
            "arc_warden_tempest_double"
        };
        private static readonly List<string> PushList = new List<string>
        {
            "arc_warden_magnetic_field",
            "arc_warden_spark_wraith"
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

        private enum TargetSelectedEnum
        {
            ClosestToMouse,
            FastestKilable,
            ClosestToHero
        }

        #endregion

        private static TargetSelectedEnum TargetSelection => (TargetSelectedEnum)Menu.Item("TargetSelection").GetValue<StringList>().SelectedIndex;

        private static bool IsAbilityEnable(string name, bool tempest = false, bool calcForPushing = false)
        {
            return !calcForPushing
                ? Menu.Item(tempest ? "spellTempest" : "spellHero").GetValue<AbilityToggler>().IsEnabled(name)
                : Menu.Item("AutoPush.Abilites").GetValue<AbilityToggler>().IsEnabled(name);
        }
        private static bool IsItemEnable(string name, bool tempest = false)
        {
            return Menu.Item(tempest ? "itemTempestEnable" : "itemHeroEnable").GetValue<AbilityToggler>().IsEnabled(name);
        }

        private static uint GetComboOrder(Item y, bool byIllusion)
        {
            //Print(Menu.Item("itemTempest").Name);
            if (Menu.Item(byIllusion ? "customOrderTempest" : "customOrderHero").GetValue<bool>())
                return Menu.Item(byIllusion ? "itemTempest" : "itemHero")
                    .GetValue<PriorityChanger>()
                    .GetPriority(y.StoredName());
            return Items[y.StoredName()];
        }


        private static void InitMenu()
        {
            if (_firstTime)
            {
                var dict = AbilityList.ToDictionary(item => item, item => true);
                var dict2 = AbilityList.ToDictionary(item => item, item => true);
                var pushAbilities = PushList.ToDictionary(item => item, item => true);
                var itemListHero = Items.Keys.ToList().ToDictionary(item => item, item => true);
                var itemListTempest = Items.Keys.ToList().ToDictionary(item => item, item => true);
                Menu.AddItem(new MenuItem("hotkey", "Hotkey").SetValue(new KeyBind('G', KeyBindType.Press)));
                Menu.AddItem(new MenuItem("spamHotkey", "Spark Spam").SetValue(new KeyBind('H', KeyBindType.Press)));
                Menu.AddItem(
                    new MenuItem("hotkeyClone", "ComboKey with Clones").SetValue(new KeyBind('Z', KeyBindType.Toggle)));
                //Menu.AddItem(new MenuItem("Items", "Items:").SetValue(new AbilityToggler(dict)));
                Menu.AddItem(new MenuItem("LockTarget", "Lock target").SetValue(true));
                Menu.AddItem(new MenuItem("MagneticField", "Use Magnetic Field for Faster Kill").SetValue(true))
                    .SetTooltip(
                        "if enable, try to cast in font on hero, disable-> try to cast for defence from melee heroes");
                Menu.AddItem(new MenuItem("Dust.Check", "Dust Usage: Check if target can go invis").SetValue(true))
                    .SetTooltip("by ur tempest in combo");

                var usages = new Menu("Using in combo", "usages");

                var mainHero = new Menu("For Main Hero", "mainHero");
                var spellHero = new Menu("Spells:", "HeroSpells");
                var itemHero = new Menu("Items:", "HeroItems");

                var tempest = new Menu("Tempest", "tempest");
                var spellTempest = new Menu("Spells:", "TempestSpells");
                var itemTempest = new Menu("Items:", "TempestItems");

                itemHero.AddItem(
                    new MenuItem("itemHeroEnable", "Toggle Items:").SetValue(new AbilityToggler(itemListHero)));
                itemHero.AddItem(new MenuItem("customOrderHero", "Use Custom Order").SetValue(false));
                itemHero.AddItem(new MenuItem("itemHero", "Items:").SetValue(new PriorityChanger(Items.Keys.ToList())));

                itemTempest.AddItem(
                    new MenuItem("itemTempestEnable", "Toggle Items:").SetValue(new AbilityToggler(itemListTempest)));
                itemTempest.AddItem(new MenuItem("customOrderTempest", "Use Custom Order").SetValue(false));
                itemTempest.AddItem(
                    new MenuItem("itemTempest", "Items:").SetValue(new PriorityChanger(Items.Keys.ToList())));

                spellHero.AddItem(new MenuItem("spellHero", "Ability:").SetValue(new AbilityToggler(dict)));
                spellTempest.AddItem(new MenuItem("spellTempest", "Ability:").SetValue(new AbilityToggler(dict2)));

                Menu.AddSubMenu(usages);
                usages.AddSubMenu(mainHero);
                usages.AddSubMenu(tempest);
                mainHero.AddSubMenu(spellHero);
                mainHero.AddSubMenu(itemHero);
                tempest.AddSubMenu(spellTempest);
                tempest.AddSubMenu(itemTempest);


                var drawItems = new Menu("Items Drawing", "ItemsDrawing");
                drawItems.AddItem(new MenuItem("DrawItems", "Draw Items on cooldown").SetValue(true));
                drawItems.AddItem(new MenuItem("Draw.OrderId.Type", "Draw Type: New").SetValue(true))
                    .SetTooltip("use new way for drawing current orders").ValueChanged += (sender, args) =>
                    {
                        _drawType = args.GetNewValue<bool>();
                    };
                drawItems.AddItem(new MenuItem("DrawItems.pos.x", "Position X").SetValue(new Slider(500, 0, 2500)));
                drawItems.AddItem(new MenuItem("DrawItems.pos.y", "Position Y").SetValue(new Slider(500, 0, 2500)));

                Menu.AddItem(new MenuItem("AutoMidas", "Auto Midas").SetValue(true));
                Menu.AddItem(
                    new MenuItem("FirstClone", "Ez Heal").SetValue(true)
                        .SetTooltip("when you use some heal-items, at the beginning of the clone will use this"));
                //Menu.AddItem(new MenuItem("AutoHeal", "Auto Heal/Bottle").SetValue(true).SetTooltip("clone use heal items on main hero if there are no enemies in 500(800) range"));
                var autoheal = new Menu("Auto Heal", "aheal");
                autoheal.AddItem(
                    new MenuItem("AutoHeal.Enable", "Auto Heal").SetValue(new KeyBind('X', KeyBindType.Toggle))
                        .SetTooltip(
                            "clone use heal items on main hero if there are no enemies in selected range. But ll still use insta heal items"));
                autoheal.AddItem(
                    new MenuItem("AutoHeal.Range", "Enemy checker").SetValue(new Slider(500, 0, 1000))
                        .SetTooltip("check enemy in selected range"));
                var autoPush = new Menu("Auto Push", "AutoPush");
                autoPush.AddItem(new MenuItem("AutoPush.Enable", "Enable").SetValue(new KeyBind('V', KeyBindType.Toggle)));
                autoPush.AddItem(new MenuItem("AutoPush.DrawLine", "Draw line").SetValue(false));
                autoPush.AddItem(new MenuItem("AutoPush.Travels", "Use Travel Boots").SetValue(true));
                autoPush.AddItem(
                    new MenuItem("AutoPush.Abilites", "Abilities for pushing").SetValue(new AbilityToggler(pushAbilities)));
                autoPush.AddItem(
                    new MenuItem("AutoPush.UnAggro.Enable", "UnAggro under tower").SetValue(true)
                        .SetTooltip(
                            "Necronomicon will try to unaggro under tower and will stay away from tower if didnt see any ally creep under tower"));
                var antiFeed = new Menu("Anti Feed", "AntiFeed", false, "item_necronomicon_3", true);
                antiFeed.AddItem(
                    new MenuItem("AntiFeed.Enable", "Ebable").SetValue(true)
                        .SetTooltip("if u have any enemy hero in range, ur necro will run on base"));
                antiFeed.AddItem(new MenuItem("AntiFeed.Range", "Range Checker").SetValue(new Slider(800, 0, 1500)));


                var orbwalnking = new Menu("OrbWalking", "ow");
                orbwalnking.AddItem(
                    new MenuItem("OrbWalking.Enable", "Enable OrbWalking").SetValue(true));
                orbwalnking.AddItem(
                    new MenuItem("OrbWalking.Type", "Type").SetValue(
                        new StringList(new[] {"follow mouse", "follow target"}, 1)));
                orbwalnking.AddItem(
                    new MenuItem("OrbWalking.minDistance", "Min distance").SetValue(new Slider(100, 0, 1000)));

                /*orbwalnking.AddItem(
                new MenuItem("OrbWalking.bonusWindupMs", "Bonus Windup Time").SetValue(new Slider(100, 100, 1000))
                    .SetTooltip("Time between attacks"));*/

                var daggerSelection = new Menu("Dagger", "dagger");
                /*daggerSelection.AddItem(
                            new MenuItem("Dagger.Enable", "Enable Dagger").SetValue(true));*/
                daggerSelection.AddItem(
                    new MenuItem("Dagger.CloseRange", "Extra Distance for blink").SetValue(
                        new Slider(200, 100, 800))).SetTooltip("1200 (dagger's default range) + your value");
                daggerSelection.AddItem(
                    new MenuItem("Dagger.MinDistance", "Min distance for blink").SetValue(new Slider(400, 100, 800)))
                    .SetTooltip("dont use blink if you are in this range");
                daggerSelection.AddItem(
                    new MenuItem("Dagger.ExtraDistance", "Min distance between target & blink pos").SetValue(
                        new Slider(50, 50, 800)));

                var difblade = new Menu("Diffusal blade", "item_diffusal_blade", false, "item_diffusal_blade", true);
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
                        "Only on Main target",
                        "For all Enemies in cast range",
                        "No one"
                    }, 1)));

                Menu.AddItem(new MenuItem("usePrediction", "Use Prediction For Spark").SetValue(true));
                Menu.AddItem(
                    new MenuItem("SparkUsage", "Do not use spark if u re not under enemy's vision").SetValue(false));
                Menu.AddItem(
                    new MenuItem("BkbUsage", "Bkb Selection").SetValue(
                        new StringList(new[] {"me", "clones", "all", "no one"}, 1)));
                //var il=new Menu("Illusion","il");
                //il.AddItem(new MenuItem("orderList", "Use order list").SetValue(false));
                Menu.AddItem(
                    new MenuItem("order", "Clone Order Selection").SetValue(
                        new StringList(new[] {"monkey", "caster", "nothing"}, 1)));
                Menu.AddItem(
                    new MenuItem("TargetSelection", "Target Selection").SetValue(
                        new StringList(new[] {"ClosestToMouse", "FastestKilable", "ClosestToHero"})));

                Menu.AddSubMenu(drawItems);
                Menu.AddSubMenu(difblade);
                Menu.AddSubMenu(daggerSelection);
                Menu.AddSubMenu(autoheal);
                Menu.AddSubMenu(orbwalnking);
                Menu.AddSubMenu(autoPush);
                autoPush.AddSubMenu(antiFeed);
                _firstTime = false;
            }
            try
            {
                Menu.AddToMainMenu();
            }
            catch (Exception)
            {
                
            }
            
        }

        private static bool _firstTime = true;
        public Core()
        {
            _mainHero = ObjectManager.LocalHero;
            InitMenu();
            InitOtherStuff();
            Link();
        }

        private static void InitOtherStuff()
        {
            //Log.Info($"[{Menu.DisplayName}] Loaded - > ClassId ({_mainHero.ClassId})");
            Game.PrintMessage(
                "<font face='Comic Sans MS, cursive'><font color='#00aaff'>" + Menu.DisplayName + " By Jumpering" +
                " loaded!</font> <font color='#aa0000'>v" + Assembly.GetExecutingAssembly().GetName().Version);
            _ethereal = new Sleeper();
            blinkSleeper = new MultiSleeper();
            _myHull = _mainHero.HullRadius;
            _drawType = Menu.Item("Draw.OrderId.Type").GetValue<bool>();
        }

        public void UnLink()
        {
            Game.OnUpdate -= Game_OnUpdate;
            Drawing.OnDraw -= Drawing_OnDraw;
            Player.OnExecuteOrder -= Player_OnExecuteAction;
            Game.OnWndProc -= Game_OnWndProc;
        }

        private static void Link()
        {
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Player.OnExecuteOrder += Player_OnExecuteAction;
            Game.OnWndProc += Game_OnWndProc;
        }

        private static bool _keyState;
        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (Game.IsChatOpen)
                return;
            if (args.Msg == (ulong) Utils.WindowsMessages.WM_KEYDOWN &&
                (args.WParam == Menu.Item("AutoPush.Enable").GetValue<KeyBind>().Key ||
                 args.WParam == Menu.Item("AutoHeal.Enable").GetValue<KeyBind>().Key ||
                 args.WParam == Menu.Item("hotkeyClone").GetValue<KeyBind>().Key))
            {
                args.Process = false;
            }
            var startPos = new Vector2(Menu.Item("DrawItems.pos.x").GetValue<Slider>().Value,
                Menu.Item("DrawItems.pos.y").GetValue<Slider>().Value);
            var size = new Vector2(40, 25);
            var extraButtonPos = startPos - new Vector2(0, 20);
            var extraButtonSize = new Vector2(size.X*6*0.7f, 19);
            if (!Utils.IsUnderRectangle(Game.MouseScreenPosition, extraButtonPos.X, extraButtonPos.Y,
                extraButtonSize.X, extraButtonSize.Y)) return;
            if (args.Msg == (ulong) Utils.WindowsMessages.WM_LBUTTONUP)
            {
                _keyState = false;
                args.Process = false;
            }
            if (args.Msg == (ulong) Utils.WindowsMessages.WM_LBUTTONDOWN)
            {
                _keyState = true;
                args.Process = false;
            }
        }

        /**
        * Whenever a player executes an action, do the following
        * 1) if monkey mode is active, make the clone copy the actions
        * 2) if ez heal is active and player executes a command with eligible items (given in CloneOnlyItems), cancel command and make clone use it instead
        **/
        private static void Player_OnExecuteAction(Player sender, ExecuteOrderEventArgs args)
        {
            if (!args.IsPlayerInput)
                return;
            #region code for monkey
            if (Menu.Item("order").GetValue<StringList>().SelectedIndex == (int)Orders.Monkey && !Menu.Item("AutoPush.Enable").GetValue<KeyBind>().Active)
            {
                //Game.PrintMessage(args.OrderId.ToString(), MessageType.ChatMessage);
                if (args.OrderId != OrderId.Stop && args.OrderId != OrderId.AttackLocation && args.OrderId != OrderId.AttackTarget &&
                    args.OrderId != OrderId.Ability && args.OrderId != OrderId.AbilityTarget &&
                    args.OrderId != OrderId.AbilityLocation &&
                    args.OrderId != OrderId.MoveLocation && args.OrderId != OrderId.MoveTarget && args.OrderId != OrderId.Hold)
                    return;

                // make each tempest clone copy main hero's moves
                foreach (var hero in Objects.Tempest.GetCloneList(sender.Hero))
                {
                    Ability spell;
                    Ability needed;
                    switch (args.OrderId)
                    {
                        case OrderId.Stop:
                            hero.Stop();
                            break;
                        case OrderId.AttackLocation:
                            hero.Attack(args.TargetPosition);
                            break;
                        case OrderId.AttackTarget:
                            var target = args.Target;
                            hero.Attack(target as Unit);
                            break;
                        case OrderId.Ability:
                            spell = args.Ability;
                            needed = hero.FindSpell(spell.Name) ?? hero.FindItem(spell.Name);
                            if (needed != null && needed.CanBeCasted())
                            {
                                needed.UseAbility();
                            }
                            break;
                        case OrderId.AbilityTarget:
                            spell = args.Ability;
                            needed = hero.FindSpell(spell.Name) ?? hero.FindItem(spell.Name);
                            if (needed != null && needed.CanBeCasted())
                            {
                                needed.UseAbility(args.Target as Unit);
                            }
                            break;
                        case OrderId.AbilityLocation:
                            spell = args.Ability;
                            needed = hero.FindSpell(spell.Name) ?? hero.FindItem(spell.Name);
                            if (needed != null && needed.CanBeCasted())
                            {
                                needed.UseAbility(args.TargetPosition);
                            }
                            break;
                        case OrderId.MoveLocation:
                            hero.Move(args.TargetPosition);
                            break;
                        case OrderId.MoveTarget:
                            hero.Move(args.TargetPosition);
                            break;
                        case OrderId.AbilityTargetTree:
                            break;
                        case OrderId.ToggleAbility:
                            break;
                        case OrderId.Hold:
                            hero.Stop();
                            break;
                    }
                }
            }
                #endregion
            #region code for ez heal
            else if (Menu.Item("FirstClone").GetValue<bool>())
            {
                if (args.OrderId != OrderId.Ability && args.OrderId != OrderId.AbilityTarget &&
                    args.OrderId != OrderId.AbilityLocation)
                    return;
                if (!CloneOnlyItems.Contains(args.Ability.Name)) return;
                
                // use clone items instead of main hero's items
                foreach (var hero in Objects.Tempest.GetCloneList(sender.Hero).Where(x => x.Distance2D(sender.Hero) <= 1000))
                {
                    Ability spell;
                    Ability needed;
                    switch (args.OrderId)
                    {
                        case OrderId.Ability:
                            spell = args.Ability;
                            needed = hero.FindSpell(spell.Name) ?? hero.FindItem(spell.Name);
                            if (needed != null && needed.CanBeCasted())
                            {
                                if (needed.StoredName() == "item_dust")
                                    needed.UseAbility();
                                else
                                    needed.UseAbility(sender.Hero);
                                args.Process = false;
                            }
                            break;
                        case OrderId.AbilityTarget:
                            spell = args.Ability;
                            needed = hero.FindSpell(spell.Name) ?? hero.FindItem(spell.Name);
                            if (needed != null && needed.CanBeCasted())
                            {
                                needed.UseAbility(args.Target as Unit);
                                args.Process = false;
                            }
                            break;
                        case OrderId.AbilityLocation:
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
            if (Menu.Item("DrawItems").GetValue<bool>())
            {
                var startPos = new Vector2(Menu.Item("DrawItems.pos.x").GetValue<Slider>().Value,
                    Menu.Item("DrawItems.pos.y").GetValue<Slider>().Value);
                var itemCount = 0;
                var size = new Vector2(40, 25);
                var extraButtonPos = startPos - new Vector2(0, 20);
                var extraButtonSize = new Vector2(size.X*6*0.7f, 19);
                var isIn = Utils.IsUnderRectangle(Game.MouseScreenPosition, extraButtonPos.X, extraButtonPos.Y,
                    extraButtonSize.X, extraButtonSize.Y);
                if (isIn)
                {
                    Drawing.DrawRect(extraButtonPos, extraButtonSize,
                        new Color(0, 155, 255, 255), true);
                    Drawing.DrawRect(extraButtonPos, extraButtonSize,
                        new Color(0, 0, 0, 140));
                    var textSize = Drawing.MeasureText("Move me", "Arial",
                        new Vector2((float) (extraButtonSize.Y*.90), extraButtonSize.Y/2), FontFlags.AntiAlias);
                    var textPos = extraButtonPos +
                                  new Vector2((extraButtonSize.X - textSize.X)/2, (extraButtonSize.Y - textSize.Y)/2);
                    Drawing.DrawText(
                        "Move me",
                        textPos,
                        new Vector2(textSize.Y, 0),
                        Color.White,
                        FontFlags.AntiAlias | FontFlags.StrikeOut);
                }
                if (_keyState)
                {
                    Menu.Item("DrawItems.pos.x").SetValue(new Slider((int) Game.MouseScreenPosition.X - 50, 0, 2500));
                    Menu.Item("DrawItems.pos.y")
                        .SetValue(new Slider((int) ((int) Game.MouseScreenPosition.Y + extraButtonSize.Y/2), 0, 2500));
                }
                Drawing.DrawRect(startPos, new Vector2(size.X*6*0.7f, size.Y*1.15f),
                    new Color(0, 155, 255, 255), true);
                if (_drawType)
                {
                    if (Menu.Item("AutoHeal.Enable").GetValue<KeyBind>().Active && !isIn)
                    {
                        Drawing.DrawRect(extraButtonPos, extraButtonSize,
                            new Color(0, 155, 255, 255), true);
                        var text = "Auto Heal " +
                                   $"[{Utils.KeyToText(Menu.Item("AutoHeal.Enable").GetValue<KeyBind>().Key)}]";
                        var textSize = Drawing.MeasureText(text, "Arial",
                            new Vector2((float) (extraButtonSize.Y*.90), extraButtonSize.Y/2), FontFlags.AntiAlias);
                        var textPos = extraButtonPos +
                                      new Vector2((extraButtonSize.X - textSize.X)/2, (extraButtonSize.Y - textSize.Y)/2);
                        Drawing.DrawText(
                            text,
                            textPos,
                            new Vector2(textSize.Y, 0),
                            Color.White,
                            FontFlags.AntiAlias | FontFlags.StrikeOut);
                    }
                    if (Menu.Item("hotkeyClone").GetValue<KeyBind>().Active)
                    {
                        extraButtonPos = startPos + new Vector2(0, size.Y);
                        var boxSize = new Vector2(size.X*6*0.7f, size.Y*1.15f);
                        Drawing.DrawRect(extraButtonPos, boxSize,
                            new Color(0, 155, 255, 255), true);
                        var text = "Clone Mode " +
                                   $"[{Utils.KeyToText(Menu.Item("hotkeyClone").GetValue<KeyBind>().Key)}]";
                        var textSize = Drawing.MeasureText(text, "Arial",
                            new Vector2((float) (boxSize.Y*.6), boxSize.Y/2), FontFlags.AntiAlias);
                        var imageSize = new Vector2((float) (boxSize.Y*0.60 + 10), (float) (boxSize.Y*0.60));
                        textSize += new Vector2(imageSize.X, 0);
                        var textPos = extraButtonPos +
                                      new Vector2((boxSize.X - textSize.X)/2, boxSize.Y/2 - textSize.Y/2);
                        //+ new Vector2(2, boxSize.Y / 2 - textSize.Y / 2);
                        Drawing.DrawText(
                            text,
                            textPos,
                            new Vector2(textSize.Y, 0),
                            Color.White,
                            FontFlags.AntiAlias | FontFlags.StrikeOut);
                        var itemPos = textPos + new Vector2(10 + textSize.X - imageSize.X, 0);
                        if (_globalTarget2 != null)
                        {
                            Drawing.DrawRect(itemPos, imageSize,
                                Textures.GetHeroTexture(_globalTarget2.StoredName()));
                            Drawing.DrawRect(itemPos, imageSize, Color.Black, true);
                        }
                    }
                    else if (Menu.Item("AutoPush.Enable").GetValue<KeyBind>().Active)
                    {
                        extraButtonPos = startPos + new Vector2(0, size.Y);
                        var boxSize = new Vector2(size.X*6*0.7f, size.Y*1.15f);
                        Drawing.DrawRect(extraButtonPos, boxSize,
                            new Color(0, 155, 255, 255), true);
                        var text = "Auto Push " +
                                   $"[{Utils.KeyToText(Menu.Item("AutoPush.Enable").GetValue<KeyBind>().Key)}]";
                        var textSize = Drawing.MeasureText(text, "Arial",
                            new Vector2((float) (boxSize.Y*.6), boxSize.Y/2), FontFlags.AntiAlias);
                        var textPos = extraButtonPos +
                                      new Vector2((boxSize.X - textSize.X)/2, boxSize.Y/2 - textSize.Y/2);
                        Drawing.DrawText(
                            text,
                            textPos,
                            new Vector2(textSize.Y, 0),
                            Color.White,
                            FontFlags.AntiAlias | FontFlags.StrikeOut);
                    }
                }
                foreach (var f in SpellBaseList)
                {
                    if (itemCount>6)
                        continue;
                    var cd = f.GetLastCd() - Game.RawGameTime + f.GetLastTime() + 1;//f.GetCooldown();
                    if (cd > 0)
                    {
                        /*if (Utils.SleepCheck("draw_items_cooldown" + f.Name))
                        {
                            Utils.Sleep(1000, "draw_items_cooldown" + f.Name);
                            f.SetCooldown(f.GetCooldown()-1);
                            //var time = (f.GetLastTime()-Game.GameTime)%60;
                            //Print($"Time: {time}");
                            //f.SetCooldown(f.GetLastCd() + time);
                        }*/
                        var itemPos = startPos + new Vector2(2+size.X*itemCount*0.7f, 2);
                        
                        Drawing.DrawRect(itemPos, size,
                            f.GetTexture());
                        var cooldown =
                            ((int)Math.Min(cd, 999)).ToString(CultureInfo.InvariantCulture);
                            //((int)Math.Min(cd+1, 999)).ToString(CultureInfo.InvariantCulture);
                        var textSize = Drawing.MeasureText(cooldown, "Arial",
                            new Vector2((float) (size.Y*.75), size.Y/2), FontFlags.AntiAlias);
                        var textPos = itemPos + new Vector2(0, size.Y - textSize.Y);
                        Drawing.DrawRect(textPos - new Vector2(0, 0),
                            new Vector2(textSize.X, textSize.Y),
                            new Color(0, 0, 0, 200));
                        Drawing.DrawText(
                            cooldown,
                            textPos,
                            new Vector2(textSize.Y, 0),
                            Color.White,
                            FontFlags.AntiAlias | FontFlags.StrikeOut);
                        itemCount++;
                    }
                }
                var kek = SpellBaseList.Where(x => x.GetCooldown() <= 0 && x.GetCooldown()>=-5).ToList();
                foreach (var item in kek)
                {
                    SpellBaseList.Remove(item);
                    //Print("Kick from ItemCooldownBase: " + item.Name);
                }
            }

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
                if (!_drawType)
                {
                    Drawing.DrawRect(startPos, size, new Color(0, 0, 0, 100));
                    Drawing.DrawRect(startPos, size, new Color(0, 0, 0, 255), true);
                    Drawing.DrawText(
                        "Clone Mode is Active" +
                        $"[{Utils.KeyToText(Menu.Item("hotkeyClone").GetValue<KeyBind>().Key)}]",
                        startPos + new Vector2(10, 10), new Vector2(20), new Color(0, 155, 255),
                        FontFlags.AntiAlias | FontFlags.DropShadow | FontFlags.Additive | FontFlags.Custom |
                        FontFlags.StrikeOut);
                }
                if (_globalTarget2 != null && _globalTarget2.IsAlive)
                {
                    if (!_drawType)
                    {
                        pos = Drawing.WorldToScreen(_globalTarget2.Position);
                        Drawing.DrawText("CloneTarget", pos, new Vector2(0, 50), Color.Red,
                            FontFlags.AntiAlias | FontFlags.DropShadow);
                        var name = "materials/ensage_ui/heroes_horizontal/" +
                                   _globalTarget2.Name.Replace("npc_dota_hero_", "") + ".vmat";
                        size = new Vector2(50, 50);
                        Drawing.DrawRect(startPos + new Vector2(10, 35), size + new Vector2(13, -6),
                            Textures.GetTexture(name));
                        Drawing.DrawRect(startPos + new Vector2(10, 35), size + new Vector2(14, -5),
                            new Color(0, 0, 0, 255), true);
                    }
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
            else if (!_drawType && Menu.Item("AutoPush.Enable").GetValue<KeyBind>().Active)
            {
                var startPos = new Vector2(Drawing.Width - 250, 100);
                var size = new Vector2(180, 40);
                Drawing.DrawRect(startPos, size, new Color(0, 0, 0, 100));
                Drawing.DrawRect(startPos, size, new Color(0, 0, 0, 255), true);
                Drawing.DrawText("AutoPush is Active " + $"[{Utils.KeyToText(Menu.Item("AutoPush.Enable").GetValue<KeyBind>().Key)}]", startPos + new Vector2(10, 10), new Vector2(20), new Color(0, 155, 255),
                    FontFlags.AntiAlias | FontFlags.DropShadow | FontFlags.Additive | FontFlags.Custom |
                    FontFlags.StrikeOut);
            }
            if (!_drawType && Menu.Item("AutoHeal.Enable").GetValue<KeyBind>().Active)
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
            Drawing.DrawText("target", pos, new Vector2(0, 50), Color.Red, FontFlags.AntiAlias | FontFlags.DropShadow);
            foreach (var me in Objects.Tempest.GetCloneList(_mainHero).ToList())
            {
                try
                {
                    DrawEffects(me, _globalTarget);
                }
                catch { Print("error Type: 1"); }
            
            }
            foreach (var me in Objects.Necronomicon.GetNecronomicons(_mainHero).ToList())
            {
                try
                {
                    DrawEffects(me, _globalTarget);
                }
                catch (Exception)
                {
                    Print("error Type: 2");
                }
                
            }
            try
            {
                DrawEffects(_mainHero, _globalTarget);
            }
            catch (Exception)
            {
                Print("error Type: 3");
            }
            
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
            if (Game.IsPaused) return;
            if (!_mainHero.IsValid || _mainHero==null)
                return;
            foreach (var clone in from clone in Objects.Tempest.GetCloneList(_mainHero)
                where Menu.Item("AutoHeal.Enable").GetValue<KeyBind>().Active
                let enemy = ObjectManager.GetEntities<Unit>()
                    .Any(
                        x =>
                            x.Team == _mainHero.GetEnemyTeam() && x.IsAlive && x.IsVisible &&
                            x.Distance2D(_mainHero) < Menu.Item("AutoHeal.Range").GetValue<Slider>().Value)
                where !enemy
                select clone)
            {
                CloneUseHealItems(clone, _mainHero, clone.Distance2D(_mainHero));
            }
            foreach (var clone in Objects.Tempest.GetCloneList(_mainHero))
            {
                foreach (var item in clone.Inventory.Items.Where(x=>x.Cooldown>0))
                {
                    var spell = SpellBaseList.Find(x => x.Name == item.StoredName());
                    if (spell==null)
                    {
                        var newSpell = new Spell(item);
                        SpellBaseList.Add(newSpell);
                        newSpell.Update(item);
                        //Print("Init new item: "+item.StoredName());
                    }
                    else
                    {
                        //spell.SetCooldown(item.Cooldown);
                        //spell.Update(item);
                    }
                }
            }
            
            #region code for spark spam
            if (Menu.Item("spamHotkey").GetValue<KeyBind>().Active)
            {
                SparkSpam(_mainHero);
                return;
            }
            #endregion

            #region Flusher
            if (_globalTarget2 != null)
                FlushEffectForDyingUnits();
            #endregion

            #region code for clone combo2 hotkey
            if (Menu.Item("hotkeyClone").GetValue<KeyBind>().Active)
            {
                // if target is not valid, set target to the hero closest to the mouse
                if (_globalTarget2 == null || !_globalTarget2.IsValid || !_globalTarget2.IsAlive)
                {
                    _globalTarget2 = TargetSelection == TargetSelectedEnum.ClosestToMouse
                        ? ClosestToMouse(_mainHero, 500)
                        : TargetSelection == TargetSelectedEnum.ClosestToHero
                            ? ClosestToHero(_mainHero, 500)
                            : FastestKillable(_mainHero, 500);
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
                //Print("Me: "+midas.Cooldown);
                if (midas.CanBeCasted() && Utils.SleepCheck(_mainHero.Handle + "midas") && _mainHero.IsAlive && !_mainHero.IsInvisible())
                {
                    var enemy = ObjectManager.GetEntities<Unit>()
                        .Where(
                            x =>
                                !x.IsMagicImmune() && x.Team != _mainHero.Team &&
                                (x.NetworkName == ClassId.CDOTA_BaseNPC_Creep_Lane.ToString() ||
                                 x.NetworkName == ClassId.CDOTA_BaseNPC_Creep_Siege.ToString() ||
                                 x.NetworkName == ClassId.CDOTA_BaseNPC_Creep_Neutral.ToString() ||
                                 x.NetworkName == ClassId.CDOTA_BaseNPC_Invoker_Forged_Spirit.ToString()) && x.IsSpawned &&
                                !x.IsAncient && x.IsAlive &&
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
                    /*Print("----------- ");
                    foreach (var item in clone.Inventory.Items)
                    {
                        Print(item.Cost+"item: "+item.Name+": "+item.Cooldown+"/"+item.CooldownLength+". State: "+item.AbilityState);
                        
                    }
                    foreach (var spell in clone.Spellbook.Spells.Where(x=>x.IsAbilityType(AbilityType.Basic)))
                    {
                        Print("item: "+spell.Name+": "+spell.Cooldown+"/"+spell.CooldownLength+". State: "+spell.AbilityState);
                    }*/
                    
                    if (midas == null || !midas.CanBeCasted() || SpellBaseList.Find(x => x.Name == midas.StoredName())!=null) continue;
                    
                    var enemy = ObjectManager.GetEntities<Unit>()
                        .Where(
                            x =>
                                !x.IsMagicImmune() && x.Team != _mainHero.Team &&
                                (x.NetworkName == ClassId.CDOTA_BaseNPC_Creep_Lane.ToString() ||
                                 x.NetworkName == ClassId.CDOTA_BaseNPC_Creep_Siege.ToString() ||
                                 x.NetworkName == ClassId.CDOTA_BaseNPC_Creep_Neutral.ToString()) && x.IsSpawned && !x.IsAncient && x.IsAlive &&
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
                    Heroes.GetByTeam(necr.GetEnemyTeam())
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
            if (!hero.IsAlive)
                return;
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
                var mod = hero.FindModifier("modifier_kill");
                if (mod == null || mod.RemainingTime >= 10)
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
                        if (spell != null && IsAbilityEnable(spell.StoredName(), calcForPushing: true) &&
                            spell.CanBeCasted() && Utils.SleepCheck("shield" + handle))
                            // handle used to uniquely identify the current hero's cooldowns
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
                else if (Utils.SleepCheck("Tempest.Attack.Cd" + handle) && !hero.IsAttacking() && isTempest)
                {
                    hero.Attack(pos);
                    Utils.Sleep(1000, "Tempest.Attack.Cd" + handle);
                }
                // smart attack for necrobook (unaggro under tower)
                if (!isTempest && Utils.SleepCheck(hero.StoredName() + "attack"))
                {
                    SmartAttack(hero, myCreeps, nearestTower, pos);
                }
                // if there are creeps in the vicinity, make tempest use mjollnir and necronomicon
                if (enemyCreeps.Any(x => x.Distance2D(hero) <= 800) && isTempest)
                {
                    var spell = hero.Spellbook.Spell3;
                    if (spell != null && IsAbilityEnable(spell.StoredName(), calcForPushing: true) &&
                        spell.CanBeCasted() && Utils.SleepCheck(spell.StoredName() + handle))
                        // handle used to uniquely identify the current hero's cooldowns
                    {
                        spell.UseAbility(enemyCreeps.First().Position);
                        Utils.Sleep(1500, spell.StoredName() + handle);
                    }
                    spell = hero.Spellbook.Spell2;
                    if (enemyCreeps.Count>=2 && spell != null && IsAbilityEnable(spell.StoredName(), calcForPushing: true) &&
                        spell.CanBeCasted() && Utils.SleepCheck(spell.StoredName() + handle))
                        // handle used to uniquely identify the current hero's cooldowns
                    {
                        spell.UseAbility(Prediction.InFront(hero, 100));
                        Utils.Sleep(1500, spell.StoredName() + handle);
                    }
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
                var dagger =
                    enumerable.Any(
                        x =>
                            x.Name == "item_blink" && x.Cooldown == 0 &&
                            SpellBaseList.Find(z => z.Name == x.StoredName()) == null);
                
                // uses all items available
                if (!hero.IsInvisible())
                    if (ItemUsage(hero, enumerable, target, d,
                        Menu.Item("BkbUsage").GetValue<StringList>().SelectedIndex == (int) BkbUsage.Clones ||
                        Menu.Item("BkbUsage").GetValue<StringList>().SelectedIndex == (int) BkbUsage.All, true))
                        SpellsUsage(hero, target, d, dagger, true);
                var invis = hero.FindItem(HideItemList[1]) ?? hero.FindItem(HideItemList[2]);
                if (invis != null)
                {
                    Print("invis!" + Utils.SleepCheck("invis" + invis.Handle) +
                          $" {invis.CanBeCasted()} {SpellBaseList.Find(z => z.Name == invis.StoredName()) == null}",print:false);
                    if (Utils.SleepCheck("invis" + invis.Handle) && invis.CanBeCasted() &&
                        SpellBaseList.Find(z => z.Name == invis.StoredName()) == null)
                    {
                        invis.UseAbility();
                        Utils.Sleep(250, "invis" + invis.Handle);
                        return;
                    }
                }
                // do orbwalking if enabled
                // otherwise simply attack target
                if (hero.IsDisarmed() || !Utils.SleepCheck("magField")) continue;
                if (Menu.Item("OrbWalking.Enable").GetValue<bool>() && OrbMinDist < hero.Distance2D(target))
                {
                    Orbwalk(hero, target);
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
            Ability spell;
            foreach (var hero in Objects.Tempest.GetCloneList(me))
            {
                spell = hero.Spellbook.Spell3;
                if (spell == null || !spell.CanBeCasted() || !Utils.SleepCheck("spam" + hero.Handle)) continue;
                spell.UseAbility(Game.MousePosition);
                Utils.Sleep(1000, "spam" + hero.Handle);
            }
            if (!me.IsAlive || !Utils.SleepCheck("spam" + me.Handle)) return;
            spell = me.Spellbook.Spell3;
            if (spell == null || !spell.CanBeCasted()) return;
            spell.UseAbility(Game.MousePosition);
            Utils.Sleep(1000, "spam" + me.Handle);
        }

        private static void DoCombo(Hero me, Hero target)
        {
            var distance = me.Distance2D(target);
            IEnumerable<Item> inv;
            Item[] enumerable;
            bool dagger;
            double targetHull = target.HullRadius;
            if (Menu.Item("order").GetValue<StringList>().SelectedIndex == (int) Orders.Caster &&
                !Menu.Item("hotkeyClone").GetValue<KeyBind>().Active)
            {
                foreach (var hero in Objects.Tempest.GetCloneList(me))
                {
                    var d = hero.Distance2D(target) - _myHull - targetHull;
                    inv = hero.Inventory.Items;
                    enumerable = inv as Item[] ?? inv.ToArray();
                    dagger =
                        enumerable.Any(
                            x =>
                                x.Name == "item_blink" && x.Cooldown == 0 &&
                                SpellBaseList.Find(z => z.Name == x.StoredName()) == null);
                    // uses all items available
                    if (!hero.IsInvisible())
                        if (ItemUsage(hero, enumerable, target, d,
                            Menu.Item("BkbUsage").GetValue<StringList>().SelectedIndex == (int)BkbUsage.Clones ||
                            Menu.Item("BkbUsage").GetValue<StringList>().SelectedIndex == (int)BkbUsage.All, true))
                            SpellsUsage(hero, target, d, dagger, true);

                    /*SpellsUsage(hero, target, d, dagger, true);
                    ItemUsage(hero, enumerable, target, d,
                        Menu.Item("BkbUsage").GetValue<StringList>().SelectedIndex == (int) BkbUsage.Clones ||
                        Menu.Item("BkbUsage").GetValue<StringList>().SelectedIndex == (int) BkbUsage.All, true);*/
                    if (Menu.Item("OrbWalking.Enable").GetValue<bool>() && OrbMinDist < hero.Distance2D(target))
                    {
                        Orbwalk(hero, target);
                    }
                    else
                    {
                        if (!Utils.SleepCheck("clone_attacking" + hero.Handle)) continue;
                        hero.Attack(target);
                        Utils.Sleep(350, "clone_attacking" + hero.Handle);
                    }
                    /*
                    if (Menu.Item("OrbWalking.Enable").GetValue<bool>())
                    {
                        Orbwalk(hero, target);
                    }
                    else
                    {
                        if (!Utils.SleepCheck("clone_attacking" + hero.Handle)) continue;
                        hero.Attack(target);
                        Utils.Sleep(350, "clone_attacking" + hero.Handle);
                    }*/
                }
            }
            var illusions =
                ObjectManager.GetEntities<Hero>()
                    .Where(
                        x =>
                            x.IsAlive && x.IsControllable && x.Team == me.Team && x.IsIllusion &&
                            !x.HasModifier("modifier_kill"))
                    .ToList();
            foreach (var illusion in illusions.TakeWhile(illusion => Utils.SleepCheck("clone_attacking" + illusion.Handle)))
            {
                illusion.Attack(target);
                Utils.Sleep(350, "clone_attacking" + illusion.Handle);
            }
            var necr = Objects.Necronomicon.GetNecronomicons(me);
            foreach (
                var necronomicon in
                    necr.TakeWhile(
                        illusion =>
                            Utils.SleepCheck("clone_attacking" + illusion.Handle) && illusion.Distance2D(target) <= 1500 &&
                            !Equals(illusion, me)))
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
            SpellsUsage(me, target, distance, dagger,false);
            ItemUsage(me,enumerable, target, distance,
                Menu.Item("BkbUsage").GetValue<StringList>().SelectedIndex == (int) BkbUsage.Me ||
                Menu.Item("BkbUsage").GetValue<StringList>().SelectedIndex == (int) BkbUsage.All);
            //Orbwalk(me,target);
            if (Menu.Item("OrbWalking.Enable").GetValue<bool>() && OrbMinDist < me.Distance2D(target))
            {
                Orbwalk(me, target);
            }
            else
            {
                if (!Utils.SleepCheck("clone_attacking" + me.Handle)) return;
                me.Attack(target);
                Utils.Sleep(350, "clone_attacking" + me.Handle);
            }
        }

        /**
        * SpellsUsage does the following
        * 1) Use Q spell on target
        * 2) Use W if close enough to target and blink dagger not available
        * 3) Use E with prediction (based on enemy pathing) if blink dagger is not available
        * 4) Use R if available (?)
        **/
        private static bool SparkUsage => Menu.Item("SparkUsage").GetValue<bool>();
        private static void SpellsUsage(Hero me, Hero target, double distance, bool daggerIsReady, bool tempest)
        {
            if (blinkSleeper.Sleeping(me))
                return;
            var spellbook = me.Spellbook;
            var q = spellbook.SpellQ;
            var w = spellbook.SpellW;
            var e = spellbook.SpellE;

            if (q != null && IsAbilityEnable(q.StoredName(), tempest) && q.CanBeCasted() && q.CanHit(target) &&
                Utils.SleepCheck(me.Handle + q.Name))
            {
                q.UseAbility(target);
                Utils.Sleep(500, me.Handle + q.Name);
            }
            if (w != null && IsAbilityEnable(w.StoredName(), tempest) && w.CanBeCasted() && Utils.SleepCheck(w.Name) &&
                !me.HasModifier("modifier_arc_warden_magnetic_field") && distance <= 600 && !daggerIsReady && target.IsVisible)
            {
                if (!Menu.Item("MagneticField").GetValue<bool>() && target.IsMelee)
                {
                    Utils.Sleep(500, "magField");
                    w.UseAbility(Prediction.InFront(me, -250));
                }
                else
                    w.UseAbility(Prediction.InFront(me, 250));
                Utils.Sleep(500, w.Name);
            }
            if (e != null && IsAbilityEnable(e.StoredName(), tempest) && e.CanBeCasted() &&
                Utils.SleepCheck(me.Handle + e.Name) && !daggerIsReady && !Prediction.IsTurning(target) && (!SparkUsage || me.IsVisibleToEnemies))
            {
                var predVector3 = target.NetworkActivity == NetworkActivity.Move &&
                                  Menu.Item("usePrediction").GetValue<bool>()
                    ? Prediction.InFront(target, target.MovementSpeed*3 + Game.Ping/1000)
                    : target.Position;
                e.UseAbility(predVector3);
                Utils.Sleep(1000, me.Handle + e.Name);
            }

            var r = me.Spellbook.SpellR;
            if (!IsAbilityEnable(r.StoredName(), tempest))
                return;
            if (r == null || !IsAbilityEnable(r.StoredName(), tempest) || !r.CanBeCasted() ||
                !Utils.SleepCheck(me.Handle + r.Name) || distance > 900 || Objects.Tempest.GetCloneList(me).Any())
                return;
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
        * 5) Uses ultimate if all items expect of refresher was casted
        **/
        private static bool ItemUsage(Hero me, IEnumerable<Item> inv, Hero target, double distance, bool useBkb, bool byIllusion = false)
        {
            if (me.IsChanneling() || !Utils.SleepCheck("DaggerTime") || me.IsStunned()) return false;
            // use all items given in Items list (line 53)
            var enumerable = inv as Item[] ?? inv.ToArray();
            var phase =
                enumerable.Find(
                    x =>
                        x.StoredName() == HideItemList[0] && byIllusion &&
                        SpellBaseList.Find(z => z.Name == x.Name) == null && Utils.SleepCheck(x.Name + me.Handle));
            if (phase != null)
            {
                phase.UseAbility();
                Utils.Sleep(250, $"{phase.StoredName() + me.Handle}");
            }
            var inventory =
                enumerable.Where(
                    x => 
                         (IsItemEnable(x.StoredName(), byIllusion) || CloneOnlyComboItems.Contains(x.StoredName()) ||
                          x.StoredName() == "item_dust" || x.StoredName() == "item_black_king_bar") && x.CanBeCasted() &&
                         (!byIllusion || SpellBaseList.Find(z => z.Name == x.Name) == null)
                             /* && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(x.Name)*/&&
                         CheckForPike(me, target, distance, x)).ToList();
            var mom = inventory.FirstOrDefault(x => x.StoredName() == "item_mask_of_madness");
            if (mom != null)
            {
                if (me.Spellbook.Spells.Any(x => x.CanBeCasted()))
                    inventory.Remove(mom);
            }
            var items =
                inventory.Where(
                    x =>
                        Utils.SleepCheck(x.Name + me.Handle) && Items.Keys.Contains(x.Name) &&
                        ((x.CastRange == 0 &&
                          distance <=
                          (x.Name == "item_blink" ? 1150 + Menu.Item("Dagger.CloseRange").GetValue<Slider>().Value : 1000)) ||
                         /*x.CastRange+50 >= distance*/x.CanHit(target))).OrderByDescending(y => GetComboOrder(y, byIllusion));
            var slarkMod = target.HasModifiers(new[] { "modifier_slark_dark_pact", "modifier_slark_dark_pact_pulses" }, false);
            foreach (var item in items)
            {
                var name = item.StoredName();
                if (name == "item_ethereal_blade")
                    _ethereal.Sleep(1000);
                if (name == "item_dagon" || name == "item_dagon_2" || name == "item_dagon_3" || name == "item_dagon_4" || name == "item_dagon_5")
                    if (_ethereal.Sleeping && !target.HasModifier("modifier_item_ethereal_blade_ethereal"))
                        continue;
                if (name == "item_hurricane_pike")
                {
                    item.UseAbility(me);
                    Utils.Sleep(250, $"{name + me.Handle}");
                    continue;
                }
                if (item.IsAbilityBehavior(AbilityBehavior.NoTarget))
                {
                    item.UseAbility();
                    Print($"[Using]: {item.Name} (10)", print: false);
                }
                else if (item.IsAbilityBehavior(AbilityBehavior.UnitTarget))
                    if (item.TargetTeamType == TargetTeamType.Enemy || item.TargetTeamType == TargetTeamType.All ||
                        item.TargetTeamType == TargetTeamType.Custom)
                    {
                        if (item.IsDisable())
                        {
                            if (!slarkMod && !target.IsLinkensProtected())
                            {
                                var duration = Utils.DisableDuration(target);
                                if ( duration <= 0.2)
                                {
                                    item.UseAbility(target);
                                    Print($"[Using]: {item.Name} (9)", print: false);
                                    /*Utils.Sleep(350, $"{item.Name + me.Handle}");
                                    continue;*/
                                }
                            }
                            /*if (item.CastStun(target))
                                {
                                    Print($"[Using]: {item.Name} (9)", print: false);
                                    Utils.Sleep(350, $"{item.Name + me.Handle}");
                                    continue;
                                }*/
                        }
                        else if (item.IsSilence())
                        {
                            if (!slarkMod)
                                if (!target.IsSilenced())
                                {
                                    item.UseAbility(target);
                                    Print($"[Using]!: {item.Name} (8)", print: false);
                                }
                        }
                        else if ((item.StoredName().Contains("dagon") || item.StoredName() == "item_ethereal_blade") &&
                                 target.HasModifiers(
                                     new[]
                                     {
                                         "modifier_templar_assassin_refraction_absorb",
                                         "modifier_templar_assassin_refraction_absorb_stacks",
                                         "modifier_oracle_fates_edict",
                                         "modifier_abaddon_borrowed_time"
                                     }, false))
                        {
                            Print("can damage this shit", print: false);
                            continue;
                        }
                        else
                        {
                            item.UseAbility(target);
                            Print($"[Using]: {item.Name} (1)", print: false);
                            Utils.Sleep(350, $"{item.Name + me.Handle}");
                            continue;
                        }
                        
                        /*item.UseAbility(target);
                        Print($"[Using]: {item.Name} (3)", print: false);*/
                    }
                    else
                    {
                        item.UseAbility(me);
                        Print($"[Using]: {item.Name} (4)", print: false);
                    }
                else
                {
                    if (name == "item_blink")
                    {
                        if (distance > 1150)
                        {
                            var angle = me.FindAngleBetween(target.Position, true);
                            var point = new Vector3(
                                (float)
                                    (target.Position.X -
                                     CloseRange*
                                     Math.Cos(angle)),
                                (float)
                                    (target.Position.Y -
                                     CloseRange*
                                     Math.Sin(angle)),
                                target.Position.Z);
                            var dist = me.Distance2D(point);
                            if (dist >= MinDistance && dist <= 1150)
                            {
                                item.UseAbility(point);
                                blinkSleeper.Sleep(250,me);
                                Print($"[Using]: {item.Name} (5)", print: false);
                            }
                        }
                        else if (distance > MinDistance)
                        {
                            var angle = me.FindAngleBetween(target.Position, true);
                            var point = new Vector3(
                                (float)
                                    (target.Position.X -
                                     ExtraDistance*
                                     Math.Cos(angle)),
                                (float)
                                    (target.Position.Y -
                                     ExtraDistance*
                                     Math.Sin(angle)),
                                target.Position.Z);
                            item.UseAbility(point);
                            blinkSleeper.Sleep(250, me);
                            Print($"[Using]: {item.Name} (6)", print: false);
                        }
                    }
                    else
                    {
                        item.UseAbility(target.NetworkPosition);
                        Print($"[Using]: {item.Name} (7)", print: false);
                    }
                }
                Utils.Sleep(250, $"{item.Name + me.Handle}");
            }

            #region old shit
            /*
            var v = items.FirstOrDefault();
            if (v == null && target.IsMelee)
            {
                var pike =
                    inventory.Find(x => Utils.SleepCheck(x.Name + me.Handle) && x.StoredName() == "item_hurricane_pike");
                if (pike != null && pike.CanBeCasted(target) && target.Distance2D(me) <= pike.GetCastRange() &&
                    Utils.SleepCheck("item_cd" + me.Handle))
                {
                    var angle = (float) Math.Max(
                        Math.Abs(target.RotationRad - Utils.DegreeToRadian(target.FindAngleBetween(me.Position))) - 0.20,
                        0);
                    if (!Prediction.IsTurning(target) && angle == 0)
                    {
                        pike.UseAbility(target);
                        Utils.Sleep(500, "item_cd" + me.Handle);
                    }
                }
            }

            if (v != null && Utils.SleepCheck("item_cd" + me.Handle))
            {
                /*Print(v.Name);
                if (v.Name == "item_manta")
                {
                    Print("cd: "+v.Cooldown+"/"+(SpellBaseList.Find(z => z.Name == v.Name)==null));
                }
                //Print(v.Name+"["+Game.GameTime+"]");
                if (v.IsAbilityBehavior(AbilityBehavior.NoTarget))
                {
                    v.UseAbility();
                }
                else if (v.IsAbilityBehavior(AbilityBehavior.UnitTarget))
                {
                    if (v.TargetTeamType == TargetTeamType.Enemy || v.TargetTeamType == TargetTeamType.All)
                    {
                        if (v.IsDisable())
                        {
                            if (!slarkMod && !target.IsLinkensProtected())
                                if (v.CastStun(target))
                                {

                                }
                        }
                        else if (v.IsSilence())
                        {
                            if (!slarkMod)
                                if (!target.IsSilenced())
                                    v.UseAbility(target);
                        }
                        else if ((v.StoredName().Contains("dagon") || v.StoredName() == "item_ethereal_blade") &&
                                 target.HasModifiers(
                                     new[]
                                     {
                                         "modifier_templar_assassin_refraction_absorb",
                                         "modifier_templar_assassin_refraction_absorb_stacks"
                                     }, false))
                            Print("underRefraction", print: false);
                        else
                            v.UseAbility(target);
                    }
                    else
                    {
                        v.UseAbility(me);
                    }
                }
                else
                {
                    if (distance > 1150)
                    {
                        var angle = me.FindAngleBetween(target.Position, true);
                        var point = new Vector3(
                            (float)
                                (target.Position.X -
                                 Menu.Item("Dagger.CloseRange").GetValue<Slider>().Value*
                                 Math.Cos(angle)),
                            (float)
                                (target.Position.Y -
                                 Menu.Item("Dagger.CloseRange").GetValue<Slider>().Value*
                                 Math.Sin(angle)),
                            target.Position.Z);
                        var dist = me.Distance2D(point);
                        if (dist >= Menu.Item("Dagger.MinDistance").GetValue<Slider>().Value && dist <= 1150)
                            v.UseAbility(point);
                    }
                    else if (distance > Menu.Item("Dagger.MinDistance").GetValue<Slider>().Value)
                    {
                        var angle = me.FindAngleBetween(target.Position, true);
                        var point = new Vector3(
                            (float)
                                (target.Position.X -
                                 Menu.Item("Dagger.ExtraDistance").GetValue<Slider>().Value*
                                 Math.Cos(angle)),
                            (float)
                                (target.Position.Y -
                                 Menu.Item("Dagger.ExtraDistance").GetValue<Slider>().Value*
                                 Math.Sin(angle)),
                            target.Position.Z);
                        v.UseAbility(point);
                    }
                }
                Utils.Sleep(500, v.Name + me.Handle);
                Utils.Sleep(100, "item_cd" + me.Handle);
            }
            */
            #endregion

            var pike = inventory.Find(x => Utils.SleepCheck(x.Name + me.Handle) && x.Name == "item_hurricane_pike");
            if (pike!=null && pike.IsValid)
            {
                if (target.IsMelee)
                {
                    if (target.Distance2D(me) <= pike.GetCastRange())
                    {
                        var angle = (float)Math.Max(
                            Math.Abs(target.RotationRad -
                                     Utils.DegreeToRadian(target.FindAngleBetween(me.Position))) - 0.20, 0);
                        if (!Prediction.IsTurning(target) && angle == 0)
                        {
                            pike.UseAbility(target);
                            Utils.Sleep(500, pike.Name + me.Handle);
                        }
                    }
                    else
                    {
                    }
                }
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
                    var dust = inventory.Find(x => Utils.SleepCheck(x.Name + me.Handle) && x.Name == "item_dust");
                    if (dust != null && (!Menu.Item("Dust.Check").GetValue<bool>() || target.CanGoInvis() || target.IsInvisible()))
                    {
                        dust.UseAbility();
                        Utils.Sleep(250, dust.StoredName() + me.Handle);
                    }
                    if (mod && !repel && !guard)
                    {
                        continue;
                    }
                    var items2 =
                        inventory.Where(
                            x =>
                                Utils.SleepCheck(x.Name + me.Handle) && CloneOnlyComboItems.Contains(x.Name) &&
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
                        Utils.SleepCheck(x.Name + me.Handle) && CloneOnlyComboItems.Contains(x.Name)).ToList();
                if (dif.Any())
                    TryToDispell(me, dif, both, main, tempest);
            }

            // code for using bkb
            if (useBkb && distance<900)
            {
                var bkb = inventory.FirstOrDefault(x => Utils.SleepCheck(x.Name + me.Handle) && x.Name == "item_black_king_bar");
                if (bkb != null && bkb.CanBeCasted() && Utils.SleepCheck(bkb.Name + me.Handle))
                {
                    bkb.UseAbility();
                    Utils.Sleep(500, bkb.Name + me.Handle);
                }
            }

            // Uses ultimate if all items expect of refresher was casted
            var refreshItems = inventory.Where(
                x =>
                    Items.Keys.Contains(x.StoredName()));
            if (refreshItems.Any()) return !items.Any();
            var r = me.Spellbook.SpellR;
            if (r == null || r.CanBeCasted()) return !items.Any();
            var refresher = inventory.FirstOrDefault(x => Utils.SleepCheck(x.Name + me.Handle) && x.Name == "item_refresher");
            refresher?.UseAbility();
            Utils.Sleep(500, refresher?.Name + me.Handle);
            return !items.Any();
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
                foreach (var hero in Heroes.GetByTeam(me.Team).Where(x=>x!=null && x.IsValid && x.IsAlive && x.Distance2D(me)<=600 && (x.IsHexed() || x.IsSilenced())))
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

        private static readonly Dictionary<uint, Orbwalker> OrbDict = new Dictionary<uint, Orbwalker>();
        private static void Orbwalk(Hero me,Unit target)
        {
            if (me == null)
            {
                return;
            }
            Orbwalker orb;
            if (!OrbDict.TryGetValue(me.Handle, out orb))
            {
                OrbDict.Add(me.Handle, new Orbwalker(me));
                return;
            }
            orb.OrbwalkOn(target, followTarget: OrbwalkerType==1);
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
        private static Hero ClosestToHero(Hero source, float range = 600)
        {
            var mousePosition = Game.MousePosition;
            var enemyHeroes = ObjectManager.GetEntities<Hero>()
                .Where(
                    x =>
                        x.Team == source.GetEnemyTeam() && !x.IsIllusion && x.IsAlive && x.IsVisible &&
                        x.Distance2D(mousePosition) <= range /*&& !x.IsMagicImmune()*/)
                .OrderBy(x => x.Distance2D(source.Position));
            return enemyHeroes.FirstOrDefault();
        }
        private static Hero FastestKillable(Hero source, float range = 600)
        {
            var mousePosition = Game.MousePosition;
            var enemyHeroes = ObjectManager.GetEntities<Hero>()
                .Where(
                    x =>
                        x.Team == source.GetEnemyTeam() && !x.IsIllusion && x.IsAlive && x.IsVisible &&
                        x.Distance2D(mousePosition) <= range /*&& !x.IsMagicImmune()*/)
                .OrderBy(x => x.Health);
            return enemyHeroes.FirstOrDefault();
        }

        #region Effects

        private static readonly Dictionary<uint, ParticleEffect> Effects = new Dictionary<uint, ParticleEffect>();

        private static bool CheckForPike(Hero me, Hero target, double distance, Item x)
        {
            if (x.StoredName() != "item_hurricane_pike")
                return true;
            var angle = (float)Math.Max(
                Math.Abs(me.RotationRad -
                         Utils.DegreeToRadian(me.FindAngleBetween(target.Position))) - 0.20, 0);
            return !Prediction.IsTurning(me) && angle == 0 && distance >= 600;
        }

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

        private static void Print(string toString,bool print=true)
        {
            if (print)
                Game.PrintMessage(toString);
        }
        #endregion
    }
}