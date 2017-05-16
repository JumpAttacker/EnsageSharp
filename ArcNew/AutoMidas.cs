using System.Collections.Specialized;
using System.Linq;
using ArcAnnihilation.Manager;
using ArcAnnihilation.Units;
using ArcAnnihilation.Utils;
using Ensage;
using Ensage.Common;
using Ensage.Common.Enums;
using Ensage.Common.Extensions;
using Ensage.Common.Objects.UtilityObjects;
using Ensage.SDK.Helpers;
using Ensage.SDK.Inventory;
using Ensage.SDK.Service;

namespace ArcAnnihilation
{
    public class AutoMidas
    {
        public Hero Me;
        public Item Midas;
        public UnitBase Base;
        private readonly Sleeper _sleeper;
        public AutoMidas(UnitBase me)
        {
            Base = me;
            Me = me.Hero;
            _sleeper = new Sleeper();
            var manager = new InventoryManager(new EnsageServiceContext(Me));
            Printer.Print("trying to init new midas manager for "+me);
            var working = true;
            manager.CollectionChanged += (sender, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Add)
                {
                    if (working)
                    {
                        working = false;
                        DelayAction.Add(200, () => working = true);
                    }
                    foreach (InventoryItem iitem in args.NewItems)
                    {
                        if (iitem.Id == ItemId.item_hand_of_midas)
                        {
                            Midas = iitem.Item;
                            UpdateManager.Subscribe(MidasChecker, 500);
                            Printer.Both($"[{me}] Midas on!");
                        }
                    }
                }
                else if (args.Action == NotifyCollectionChangedAction.Remove)
                {
                    if (working)
                    {
                        foreach (InventoryItem iitem in args.OldItems)
                        {
                            if (iitem.Id == ItemId.item_hand_of_midas)
                            {
                                UpdateManager.Unsubscribe(MidasChecker);
                                Midas = null;
                                Printer.Both($"[{me}] Midas off!");
                            }
                        }
                    }
                }
            };
        }

        private void MidasChecker()
        {
            if (!Me.IsAlive)
                return;
            if (Midas != null && Midas.CanBeCasted() && !_sleeper.Sleeping)
            {
                var creep =
                    CreepManager.GetCreepManager()
                        .GetCreeps.Where(x => x.IsValid && x.Team != Me.Team && Midas.CanHit(x))
                        .OrderByDescending(x => x.Health).FirstOrDefault();
                if (creep != null)
                {
                    Midas.UseAbility(creep);
                    _sleeper.Sleep(500);
                }
            }
        }


        public static AutoMidas GetNewInstance(UnitBase me)
        {
            return new AutoMidas(me);
        }

    }
}