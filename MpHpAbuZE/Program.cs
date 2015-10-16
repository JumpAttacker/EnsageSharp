using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using SharpDX;


namespace MpHpAbuZE
{
    class Program
    {

        #region Members

        private static bool _loaded;
        private static Hero _me;
        private static Player _player;
        private static byte _stage;
        public static bool Drop { get; set; }
        private static Vector3 _oldPos;
        #endregion

        #region Methods

        private static void Main()
        {
            Game.OnUpdate += Game_OnUpdate;
            _loaded = false;
            Game.OnWndProc += Game_OnWndProc;
        }
        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (args.WParam != 'V' || Game.IsChatOpen || args.Msg == (ulong)Utils.WindowsMessages.WM_KEYUP)
            {
                return;
            }
            Drop = true;
            Game.ExecuteCommand(string.Format("dota_player_units_auto_attack_after_spell {0}",Drop?0:1));
            //PrintInfo(Drop.ToString());
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (!_loaded)
            {
                _me = ObjectMgr.LocalHero;
                _player = ObjectMgr.LocalPlayer;
                if (!Game.IsInGame || _me == null)
                {
                    return;
                }
                _loaded = true;
                PrintSuccess("> MpHpArbus Loaded");
            }

            if (!Game.IsInGame || _me == null)
            {
                _loaded = false;
                PrintInfo("> MpHpArbus unLoaded");
                return;
            }

            if (Game.IsPaused || !Utils.SleepCheck(_me.Handle.ToString()))
            {
                return;
            }

            if (_player == null || _player.Team == Team.Observer)
                return;
            if (!Drop)
            {
                _oldPos = _me.NetworkPosition;
                return;
            }
            var inInvis = _me.IsInvisible();
            var inStun = _me.IsStunned();
            var isChannel = _me.IsChanneling();
            var bottle = _me.Inventory.Items.FirstOrDefault(x => x.Name == "item_bottle");
            if (inStun || inInvis || isChannel) return;
            if (bottle != null)
            {
                var isMoving = _oldPos != _me.NetworkPosition;
                if (isMoving)
                {
                    Drop = false;
                    _stage = 1;
                }
            }
            switch (_stage)
            {
                case 0:
                    if (!Utils.SleepCheck("cd")) break;
                    var items = _me.Inventory.Items.Where(
                        x =>
                            x.Name != "item_soul_ring" && x.Name != "item_arcane_boots" && x.Name != "item_bottle" &&
                            x.Name != "item_magic_stick" && x.Name != "item_magic_wand").ToList();
                    var arcaneBoots = _me.Inventory.Items.FirstOrDefault(x => x.Name == "item_arcane_boots");
                    var soulRing = _me.Inventory.Items.FirstOrDefault(x => x.Name == "item_soul_ring");
                    
	                var stick = _me.Inventory.Items.FirstOrDefault(x => x.Name == "item_magic_stick");
                    var wand = _me.Inventory.Items.FirstOrDefault(x => x.Name == "item_magic_wand");
                    foreach (var s in items)
                    {
                        _me.DropItem(s, _me.NetworkPosition);
                    }
                    if (stick != null)
                    {
                        if (stick.CanBeCasted())
                        {
                            stick.UseAbility();
                        }
                        _me.DropItem(stick, _me.NetworkPosition);
                    }
                    if (wand != null)
                    {
                        if (wand.CanBeCasted())
                        {
                            wand.UseAbility();
                        }
                        _me.DropItem(wand, _me.NetworkPosition);
                    }
                    if (arcaneBoots != null)
                    {
                        if (arcaneBoots.CanBeCasted())
                            arcaneBoots.UseAbility();
                        _me.DropItem(arcaneBoots, _me.NetworkPosition);
                    }
                    if (soulRing != null && soulRing.CanBeCasted())
                    {
                        soulRing.UseAbility();
                    }
                    /*PrintInfo("------------------------");
                    foreach (var x in _me.Modifiers)
                    {
                        PrintInfo("Modifiers: "+x.Name);
                    }*/
                    if (bottle != null/* && bottle.CurrentCharges > 0 && bottle.CurrentCharges<=3*/)
                    {
                        if (bottle.CanBeCasted() &&
                            _me.Modifiers.All(x => x.Name != "modifier_bottle_regeneration"))
                        {
                            bottle.UseAbility();
                            Utils.Sleep(1000, "cd");
                        }
                    }
                    else
                    {
                        _stage = 1;
                    }
                    Utils.Sleep(200, "w84cast");
                    break;
                case 1:
                    if (!Utils.SleepCheck("w84cast")) break;
                    var forPick = ObjectMgr.GetEntities<PhysicalItem>().Where(
                        x =>
                            x.Distance2D(_me.NetworkPosition) <= 250).ToList();
                    foreach (var s in forPick)
                    {
                        _me.PickUpItem(s);
                    }
                    Utils.Sleep(1000, "cd");
                    _stage = 0;
                    Drop = false;
                    Game.ExecuteCommand(string.Format("dota_player_units_auto_attack_after_spell {0}", Drop ? 0 : 1));
                    break;
                default:
                    PrintError(".!.");
                    break;
            }
        }

        #endregion

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
