using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Enums;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Common.Objects.UtilityObjects;

namespace AutoDeward
{
    public static class MenuManager
    {
        private static readonly Menu Menu = new Menu("Auto Deward", "Auto Deward", true);
        public static bool IsEnable => Menu.Item("Enable").GetValue<bool>();

        public static bool IsItemEnable(ItemId item)
            => Menu.Item("item_dict").GetValue<AbilityToggler>().IsEnabled(item.ToString());
        private static bool _loaded;
        public static void Init()
        {
            if (_loaded)
                return;
            _loaded = true;
            var dict = new Dictionary<string, bool>()
            {
                {ItemId.item_bfury.ToString(),true},
                {ItemId.item_quelling_blade.ToString(),true},
                {ItemId.item_iron_talon.ToString(),true},
                {ItemId.item_tango.ToString(),true},
                {ItemId.item_tango_single.ToString(),true},
            };
            Menu.AddItem(new MenuItem("Enable", "Enable").SetValue(true));
            Menu.AddItem(new MenuItem("item_dict", "Items:").SetValue(new AbilityToggler(dict)));
            Menu.AddToMainMenu();
        }
    }

    public static class Core
    {
        private static Hero _me;
        private static Sleeper _sleeper;
        private static void GameOnOnUpdate(EventArgs args)
        {
            if (_sleeper.Sleeping || !MenuManager.IsEnable)
                return;
            _sleeper.Sleep(100);
            var item = _me.GetItemById(ItemId.item_bfury) ??
                       _me.GetItemById(ItemId.item_quelling_blade) ??
                       _me.GetItemById(ItemId.item_iron_talon) ??
                       _me.GetItemById(ItemId.item_tango) ?? 
                       _me.GetItemById(ItemId.item_tango_single);

            if (item == null || !item.CanBeCasted()) return;
            var target =
                ObjectManager.GetEntitiesFast<Unit>()
                    .FirstOrDefault(
                        x =>
                            (x.ClassId == ClassId.CDOTA_NPC_Observer_Ward ||
                             x.ClassId == ClassId.CDOTA_NPC_Observer_Ward_TrueSight) && x.Team != _me.Team &&
                            x.IsAlive && x.IsVisible && item.CanHit(x));
            if (target==null)
                return;
            item.UseAbility(target);
            _sleeper.Sleep(500);
        }

        public static Item GetItem(this Hero me,ItemId itemId)
        {
            return MenuManager.IsItemEnable(itemId) ? me.GetItemById(itemId) : null;
        }

        public static void Handle()
        {
            _me = ObjectManager.LocalHero;
            _sleeper = new Sleeper();
            Game.OnUpdate += GameOnOnUpdate;
        }

        public static void UnHandle()
        {
            Game.OnUpdate -= GameOnOnUpdate;
        }
    }

    internal class Program
    {
        private static void Main()
        {
            Events.OnLoad += (sender, args) =>
            {
                MenuManager.Init();
                Core.Handle();
            };
            Events.OnClose += (sender, args) =>
            {
                Core.UnHandle();
            };
        }
    }
}
