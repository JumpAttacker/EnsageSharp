using System.Linq;
using ArcAnnihilation.Units;
using Ensage;
using Ensage.Common.Enums;
using Ensage.Common.Extensions;
using Ensage.SDK.Helpers;

namespace ArcAnnihilation
{
    public class AutoMidas
    {
        public Hero Me;
        public Item Midas;
        public UnitBase Base;
        public AutoMidas(UnitBase me)
        {
            Base = me;
            Me = me.Hero;
            UpdateManager.Subscribe(MidasChecker, 100);

            /*Drawing.OnDraw += args =>
            {
                if (MenuManager.DebugInGame || MenuManager.DebugInConsole)
                {
                    if (Midas != null)
                        Drawing.DrawText($"Midas {(Midas.CanBeCasted() ? "On" : "Off")}", new Vector2(200, 10),
                            Color.White,
                            FontFlags.None);
                }
            };*/

            /*var manager = new InventoryManager(new EnsageServiceContext(Me));
            Printer.Print("trying to init new midas manager for " + me);
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
                        if (iitem.Id == AbilityId.item_hand_of_midas)
                        {
                            Midas = iitem.Item;
                            UpdateManager.Subscribe(MidasChecker, 100);
                            Printer.Both($"[{me}] Midas on! (main -> {me is MainHero})");
                        }
                    }
                }
                else if (args.Action == NotifyCollectionChangedAction.Remove)
                {
                    if (working)
                    {
                        foreach (InventoryItem iitem in args.OldItems)
                        {
                            if (iitem.Id == AbilityId.item_hand_of_midas)
                            {
                                //UpdateManager.Unsubscribe(MidasChecker);
                                Midas = null;
                                Printer.Both($"[{me}] Midas off! (main -> {me is MainHero})");
                            }
                        }
                    }
                }
            };*/
        }

        private void MidasChecker()
        {
            if (!MenuManager.AutoMidas)
                return;
            if (!Me.IsAlive || Me.IsInvisible())
                return;
            if (Midas == null || !Midas.IsValid)
            {
                Midas = Me.GetItemById(ItemId.item_hand_of_midas);
                return;
            }
            if (Midas.CanBeCasted())
            {
                var creep =
                    EntityManager<Creep>.Entities.Where(
                            x =>
                                x.IsValid && x.IsAlive && x.Team != Me.Team && Midas.CanHit(x) && !x.IsAncient &&
                                !x.IsMagicImmune() && x.IsSpawned)
                        .OrderByDescending(x => x.Health).FirstOrDefault();
                if (creep != null)
                {
                    Midas.UseAbility(creep);
                    /*Printer.Log(
                        $"AutoMidas -> {creep.Name} ({creep.Health}/{creep.MaximumHealth}) (id: {creep.Handle})");*/
                }
            }
        }

        public static AutoMidas GetNewInstance(UnitBase me)
        {
            return new AutoMidas(me);
        }
    }
}