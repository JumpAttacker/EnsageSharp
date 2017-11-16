using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Ensage;
using Ensage.Abilities;
using Ensage.Common.Objects;
using Ensage.SDK.Extensions;
using Ensage.SDK.Helpers;
using log4net;
using PlaySharp.Toolkit.Logging;

namespace OverlayInformation
{
    public class Holder : IDisposable
    {
        public List<AbilityHolder> Holders;

        public Holder()
        {
            Holders = new List<AbilityHolder>();
            UpdateManager.Subscribe(Flush, 5000);
        }

        private void Flush()
        {
            var temp = Holders.Where(x => x.IsValid);
            Holders = temp.ToList();
        }

        public AbilityHolder GetOrCreate(Ability ability)
        {
            var find = Holders.Find(x => x.IsValid && x.Ability.Equals(ability));
            if (find != null) return find;
            find = new AbilityHolder(ability);
            Holders.Add(find);
            return find;
        }

        public void Dispose()
        {
            UpdateManager.Unsubscribe(Flush);
        }
    }
    public class AbilityHolder
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public Ability Ability;
        public uint Handle;
        public Hero Owner;
        public AbilityId Id;
        public AbilityState AbilityState;
        public DotaTexture Texture;
        public float Cooldown;
        public bool IsUltimate;
        public bool IsHidden;
        public bool IsValid => Ability!=null && Ability.IsValid;
        public int MaximumLevel { get; set; }
        public AbilitySlot AbilitySlot { get; set; }
        public string Name { get; set; }
        public Item Item;
        public uint Cost;
        public AbilityHolder(Ability ability)
        {
            Ability = ability;
            Handle = ability.Handle;
            Name = ability.Name;
            MaximumLevel = Ability.MaximumLevel;
            Owner = (Hero)ability.Owner;
            Id = Ability.Id;
            AbilityState = ability.AbilityState;
            Texture = Ability is Item
                ? Textures.GetItemTexture(ability.Name)
                : Textures.GetSpellTexture(ability.Name);
            Cooldown = Ability.Cooldown;
            IsUltimate = ability.AbilityType == AbilityType.Ultimate;
            IsHidden = ability.IsHidden;
            AbilitySlot = ability.AbilitySlot;
            Item = ability as Item;
            if (Item != null)
            {
                Cost = Item.Cost;
            }
            UpdateManager.BeginInvoke(async () =>
            {
                while (ability.IsValid)
                {
                    AbilityState = ability.AbilityState;
                    Cooldown = Ability.Cooldown;
                    IsHidden = ability.IsHidden;
                    AbilitySlot = ability.AbilitySlot;
                    await Task.Delay(300);
                }
                //Log.Debug($"[{Owner.Name}] end for -> {Id}");
            });
        }
    }

    public class CourContainer : IDisposable
    {
        public Courier Cour { get; }
        public bool IsAlly { get; }
        public OverlayInformation Main { get; }

        public CourContainer(Courier cour, bool isAlly, OverlayInformation main)
        {
            Cour = cour;
            IsAlly = isAlly;
            Main = main;
            Items = new List<Item>();
            UpdateManager.Subscribe(UpdateItems, 500);
            UpdateManager.Subscribe(FlushChecker, 1000);
        }

        private void FlushChecker()
        {
            if (Cour == null || !Cour.IsValid)
            {
                Dispose();
            }
        }

        private void UpdateItems()
        {
            Items = Cour.Inventory.Items.ToList();
        }

        public List<Item> Items { get; set; }

        public void Dispose()
        {
            UpdateManager.Unsubscribe(UpdateItems);
            UpdateManager.Unsubscribe(FlushChecker);
        }
    }

    public class HeroContainer
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public bool IsAlly { get; }
        public OverlayInformation Main { get; }
        public bool IsOwner { get; }
        public Hero Hero { get; }
        //public Ability Ultimate;
        public AbilityHolder Ultimate;
        public List<AbilityHolder> Abilities2;
        public List<AbilityHolder> Items;
        public List<AbilityHolder> DangItems;
        public float LastTimeUnderVision;
        public float Health;
        public float MaxHealth;
        public float Mana;
        public float MaxMana;
        public bool IsVisible;
        public uint Networth;
        public bool DontDraw;
        public bool AghanimState;
        public Inventory HeroInventory { get; set; }
        private static readonly List<AbilityId> DangeItemList = new List<AbilityId>
        {
            AbilityId.item_blink,
            AbilityId.item_gem,
            AbilityId.item_silver_edge,
            AbilityId.item_sheepstick,
            AbilityId.item_orchid,
            AbilityId.item_bloodthorn,
            AbilityId.item_black_king_bar,
            AbilityId.item_glimmer_cape,
            AbilityId.item_invis_sword
        };

        //private readonly InventoryManager _manager;
        public float TimeInFog;
        public AbilityState AbilityState { get; set; }
        public Holder HolderHelper;
        public string Name { get; set; }
        public List<Ability> GetAllAbilities => Hero.Spellbook.Spells.Where(
                x => x.AbilityType == AbilityType.Basic || x.AbilityType == AbilityType.Ultimate)
            .ToList();
        public HeroContainer(Hero hero, bool isAlly, OverlayInformation main)
        {
            Name = hero.Name;
            Id = hero.Player == null ? 0 : hero.Player.Id;
            HolderHelper = new Holder();
            IsAlly = isAlly;
            Main = main;
            IsOwner = hero.Equals(ObjectManager.LocalHero);
            Hero = hero;
            //Ultimate = hero.Spellbook.Spells.First(x => x.AbilityType == AbilityType.Ultimate);
            LastTimeUnderVision = Game.RawGameTime;
            Items = new List<AbilityHolder>();
            DangItems = new List<AbilityHolder>();
            Abilities2 = new List<AbilityHolder>();
            foreach (var ability in GetAllAbilities)
            {
                var holder = HolderHelper.GetOrCreate(ability);//new AbilityHolder(ability);
                Abilities2.Add(holder);
                if (holder.IsUltimate)
                    Ultimate = holder;
                Log.Info($"{ability.Name} -> {(ability.AbilityType == AbilityType.Basic ? "basic" : "ultimate")}");
            }
            HeroInventory = Hero.Inventory;
            /*_manager = new InventoryManager(new EnsageServiceContext(hero));
            //manager.CollectionChanged += ManagerOnCollectionChanged;
            _manager.CollectionChanged += (sender, args) =>
            {
                //Items.Clear();
                DangItems.Clear();
                Items = _manager.Inventory.Items.ToList();
                Networth = 0;
                var tmpAgh = hero.HasAghanimsScepter();
                
                if (!AghanimState && tmpAgh || AghanimState && !tmpAgh)
                {
                    RefreshAbilities();
                }
                AghanimState = tmpAgh;
                foreach (var item in Items)
                {
                    Networth += item.Cost;
                    try
                    {
                        if (DangeItemList.Contains(item.Id))
                            DangItems.Add(item);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("GEGE -> "+e);
                    }
                }
            };*/
            UpdateInfo();
            UpdateItems();
            UpdateManager.Subscribe(UpdateItems, 500);
            UpdateManager.Subscribe(UpdateInfo, 250);
            UpdateManager.Subscribe(FlushChecker,1000);

            var dividedWeStand = hero.Spellbook.SpellR as DividedWeStand;
            if (dividedWeStand != null && hero.ClassId == ClassId.CDOTA_Unit_Hero_Meepo && dividedWeStand.UnitIndex > 0)
            {
                DontDraw = true;
            }

            ClassId = hero.ClassId;
            if (ClassId == ClassId.CDOTA_Unit_Hero_Rubick || ClassId == ClassId.CDOTA_Unit_Hero_DoomBringer/* ||
                classId == ClassId.CDOTA_Unit_Hero_Invoker*/ || ClassId == ClassId.CDOTA_Unit_Hero_Morphling)
            {
                UpdateManager.Subscribe(AbilityUpdater, 750);
            }

            /*Main.Context.Value.AbilityDetector.AbilityCasted += (sender, args) =>
            {
                Game.PrintMessage(args.Ability.Ability.Name);
            };
            Main.Context.Value.AbilityDetector.AbilityCastStarted += (sender, args) =>
            {
                Game.PrintMessage(args.Ability.Ability.Name);
            };*/
        }

        public ClassId ClassId { get; set; }

        private void FlushChecker()
        {
            if (Hero == null || !Hero.IsValid)
            {
                if (Game.GameState == GameState.GameInProgress)
                {
                    var player = ObjectManager.GetPlayerById((uint) Id);
                    Log.Error(
                        $"CUSTOM FLUSH FOR {Name} id -> [{Id}] -> player -> [{(player != null ? player.Name : "null")}]");
                }
                Flush();
            }
            else if (Hero.IsIllusion && !Hero.HasModifier("modifier_morphling_replicate") && Hero.IsAlive && Hero.IsVisible)
            {
                Log.Error(
                    $"Flush cuz illusion {Name} id -> [{Id}]");
                Flush();
            }
        }

        private void UpdateInfo()
        {
            if (Hero == null || !Hero.IsValid)
                return;
            IsVisible = Hero.IsVisible;
            if (IsVisible)
                LastTimeUnderVision = Game.RawGameTime;
            else
            {
                TimeInFog = Game.RawGameTime - LastTimeUnderVision;
                return;
            }
            Health = Hero.Health;
            Mana = Hero.Mana;
            MaxHealth = Hero.MaximumHealth;
            MaxMana = Hero.MaximumMana;
            AbilityState = Ultimate.AbilityState;
        }
        private void UpdateItems()
        {
            if (Hero == null || !Hero.IsValid)
                return;
            if (!Hero.IsAlive || !Hero.IsVisible)
                return;
            DangItems.Clear();
            Items = new List<AbilityHolder>();
            Networth = 0;
            foreach (var item in HeroInventory.Items)
            {
                var localHolder = HolderHelper.GetOrCreate(item);
                Items.Add(localHolder);
                Networth += item.Cost;
                if (!Main.Config.HeroOverlay.ItemDangItems) continue;
                if (DangeItemList.Contains(item.Id))
                    DangItems.Add(localHolder);
            }
            var tmpAgh = Hero.HasAghanimsScepter();

            if (!AghanimState && tmpAgh || AghanimState && !tmpAgh)
            {
                RefreshAbilities2();
            }
            AghanimState = tmpAgh;
        }

        public int Id { get; set; }

        private void AbilityUpdater()
        {
            //var needToRefresh = Abilities.Any(x => x == null || !x.IsValid || x.IsHidden);
            var needToRefresh = Abilities2.Any(x => x == null || !x.IsValid || x.IsHidden);
            if (needToRefresh)
                RefreshAbilities2();
        }

        /*private void ManagerOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {

            if (args.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (InventoryItem iitem in args.NewItems)
                {
                    Networth += iitem.Item.Cost;
                    if (DangeItemList.Contains(iitem.Id))
                        DangItems.Add(iitem.Item);
                    Items.Add(iitem.Item);
                }
            }
            else if (args.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (InventoryItem iitem in args.OldItems)
                {
                    Networth -= iitem.Item.Cost;
                    if (DangeItemList.Contains(iitem.Id))
                        DangItems.Remove(iitem.Item);
                    Items.Remove(iitem.Item);
                }
            }
        }*/

        public void RefreshAbilities2()
        {
            var abilities =
                Hero.Spellbook.Spells.Where(
                    x =>
                        (x.AbilityType == AbilityType.Basic || x.AbilityType == AbilityType.Ultimate) &&
                        Abilities2.Find(y => y.Handle == x.Handle) == null);
            foreach (var ability in abilities)
            {
                Abilities2.Add(HolderHelper.GetOrCreate(ability));
                //Abilities2.Add(new AbilityHolder(ability));
                //Log.Error($"added new ability -> {ability.Name} ({ability.Owner.Name})");
            }
            Abilities2.RemoveAll(x => !x.IsValid/* || x.IsHidden*/);
        }

        public void Flush()
        {
            //_manager.Deactivate();
            if (ClassId == ClassId.CDOTA_Unit_Hero_Rubick || ClassId == ClassId.CDOTA_Unit_Hero_DoomBringer /* ||
                classId == ClassId.CDOTA_Unit_Hero_Invoker*/ || ClassId == ClassId.CDOTA_Unit_Hero_Morphling)
            {
                UpdateManager.Unsubscribe(AbilityUpdater);
            }

            UpdateManager.Unsubscribe(UpdateItems);
            UpdateManager.Unsubscribe(UpdateInfo);
            UpdateManager.Unsubscribe(FlushChecker);
            HolderHelper.Dispose();
        }
    }
}