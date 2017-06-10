using System;
using System.Threading;
using ArcAnnihilation.Manager;
using ArcAnnihilation.Panels;
using ArcAnnihilation.Units;
using ArcAnnihilation.Utils;
using Ensage;
using Ensage.Common;
using Ensage.Common.Menu;
using Ensage.Common.Threading;
using Ensage.SDK.Helpers;

namespace ArcAnnihilation
{
    public static class Core
    {
        public static Hero Target;
        public static CancellationTokenSource ComboToken;
        public static UnitBase MainHero;
        public static UnitBase TempestHero;

        static Core()
        {
            /*var manager = new InventoryManager(new EnsageServiceContext(ObjectManager.LocalHero));
            manager.CollectionChanged += (sender, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Add)
                {
                    var newItem = args.NewItems[0];
                }
            };*/
        }

        public static void Init()
        {
            MainHero = new MainHero();
            MainHero.Init();
            AutoMidas.GetNewInstance(MainHero);
            GameDispatcher.OnUpdate += GameDispatcherOnOnUpdate;
            UpdateManager.Subscribe(TempestUpdater,500);
        }

        private static void TempestUpdater()
        {
            if (TempestManager.Tempest != null && TempestManager.Tempest.IsValid)
            {
                if (MenuManager.IsItemPanelEnable)
                    ItemPanel.GetItemPanel().Load();
                TempestHero = new Tempest();
                TempestHero.Init();
                UpdateManager.Unsubscribe(TempestUpdater);
                AutoMidas.GetNewInstance(TempestHero);
                DelayAction.Add(200, () =>
                {
                    if (MenuManager.IsAutoPushPanelEnable)
                        PushLaneSelector.GetInstance().Load();
                });

            }
        }

        public static void GameDispatcherOnOnUpdate(EventArgs args)
        {
            if (!MenuManager.IsEnable)
                return;
            if (OrderManager.CurrentOrder.CanBeExecuted)
            {
                if (!Helper.TargetFinder() && OrderManager.CurrentOrder.NeedTarget)
                    return;
                ComboToken = new CancellationTokenSource();
                OrderManager.CurrentOrder.Execute();
                /*if (_loaded)
                {
                    GameDispatcher.OnUpdate -= GameDispatcherOnOnUpdate;
                    _loaded = false;
                }*/
            }
        }

        private static bool _disableFunc;
        public static void ComboStatusChanger(object sender, OnValueChangeEventArgs e)
        {
            if (_disableFunc)
                return;
            //if (e.GetNewValue<KeyBind>().Active == e.GetOldValue<KeyBind>().Active) return;
            if (e.GetNewValue<KeyBind>().Active)
            {
                var menu = sender as MenuItem;
                if (menu==null)
                    return;
                if (menu.Equals(MenuManager.DefaultCombo))
                {
                    OrderManager.ChangeOrder(OrderManager.Orders.DefaultCombo);
                }
                else if (menu.Equals(MenuManager.TempestCombo))
                {
                    OrderManager.ChangeOrder(OrderManager.Orders.TempestCombo);
                }
                else if (menu.Equals(MenuManager.SparkSpamCombo))
                {
                    OrderManager.ChangeOrder(OrderManager.Orders.SparkSpam);
                }
                else if (menu.Equals(MenuManager.SparkSpamTempestOnlyCombo))
                {
                    OrderManager.ChangeOrder(OrderManager.Orders.SparkSpamTempest);
                }
                else if (menu.Equals(MenuManager.AutoPushingCombo))
                {
                    OrderManager.ChangeOrder(OrderManager.Orders.AutoPushing);
                }
                else
                {
                    OrderManager.ChangeOrder(OrderManager.Orders.Idle);
                }
                /*if (!_loaded)
                {
                    _loaded = true;
                    GameDispatcher.OnUpdate += GameDispatcherOnOnUpdate;
                }*/
            }
            else
            {
                if (ComboToken != null)
                {
                    ComboToken?.Cancel();
                    ComboToken?.Dispose();
                    ComboToken = null;
                }
                Target = null;
                var menu = sender as MenuItem;
                if (menu == null)
                    return;
                if (menu.Equals(MenuManager.DefaultCombo))
                {
                    OrderManager.ChangeOrder(OrderManager.Orders.Idle);
                }
                else if (menu.Equals(MenuManager.TempestCombo))
                {
                    OrderManager.ChangeOrder(OrderManager.Orders.Idle);
                }
                else if (menu.Equals(MenuManager.SparkSpamCombo))
                {
                    OrderManager.ChangeOrder(OrderManager.Orders.Idle);
                }
                else if (menu.Equals(MenuManager.SparkSpamTempestOnlyCombo))
                {
                    OrderManager.ChangeOrder(OrderManager.Orders.Idle);
                }
                else if (menu.Equals(MenuManager.AutoPushingCombo))
                {
                    OrderManager.ChangeOrder(OrderManager.Orders.Idle);
                }
                else
                {
                    OrderManager.ChangeOrder(OrderManager.Orders.Idle);
                }
                _disableFunc = true;
                MenuManager.DefaultCombo.SetValue(new KeyBind(MenuManager.DefaultCombo.GetValue<KeyBind>().Key, KeyBindType.Press));
                MenuManager.TempestCombo.SetValue(new KeyBind(MenuManager.TempestCombo.GetValue<KeyBind>().Key, KeyBindType.Toggle));
                MenuManager.SparkSpamCombo.SetValue(new KeyBind(MenuManager.SparkSpamCombo.GetValue<KeyBind>().Key, KeyBindType.Press));
                MenuManager.SparkSpamTempestOnlyCombo.SetValue(new KeyBind(MenuManager.SparkSpamTempestOnlyCombo.GetValue<KeyBind>().Key, KeyBindType.Toggle));
                MenuManager.AutoPushingCombo.SetValue(new KeyBind(MenuManager.AutoPushingCombo.GetValue<KeyBind>().Key, KeyBindType.Toggle));
                _disableFunc = false;
            }
        }
    }
}