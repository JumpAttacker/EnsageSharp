using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;
using Ensage.SDK.Service;
using Ensage.SDK.Service.Metadata;
using Ensage.SDK.Extensions;
using Ensage.SDK.Handlers;
using Ensage.SDK.Abilities;
using Ensage.SDK.Helpers;
using Ensage.SDK.Input;
using Ensage.SDK.Inventory;
using Ensage.SDK.Orbwalker;
using Ensage.SDK.Prediction;
using Ensage.SDK.Renderer;
using Ensage.SDK.Renderer.Particle;
using Ensage.SDK.TargetSelector;
using Ensage.SDK.Menu;
using Ensage.Common.Menu;
using log4net;
using PlaySharp.Toolkit.Logging;
using SharpDX;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.SDK.Abilities.Items;
using Ensage.SDK.Inventory.Metadata;

namespace InfinityBash
{
    [ExportPlugin(
        mode: StartupMode.Auto,
        name: "InfinityBash",
        version: "1.0.0.0",
        author: "Ensage",
        description: "",
        units: new HeroId[]
        {
            /* HeroId.npc_dota_hero_abaddon*/
        })]
    public sealed class InfinityBash : Plugin
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [ImportingConstructor]
        public InfinityBash([Import] IServiceContext context)
        {
            this.Context = context;
        }

        public IServiceContext Context { get; }

        protected override void OnActivate()
        {
            Me = Context.Owner as Hero;

            Entity.OnInt64PropertyChange += (sender, args) =>
            {
                if (args.PropertyName != "m_nUnitState64" || args.NewValue != 32 || args.OldValue != 0) return;
                var target = sender as Unit;
                if (target == null || !Me.IsInAttackRange(target)) return;
                var basher = Me.GetItemById(AbilityId.item_abyssal_blade);
                if (basher == null)
                    return;
                basher.DisassembleItem();
                UpdateManager.BeginInvoke(async () =>
                {
                    var unlocked = new List<AbilityId>();
                    while (true)
                    {
                        var a = Me.GetItemById(AbilityId.item_basher) ??
                                Me.Inventory.Backpack.FirstOrDefault(x => x.Id == AbilityId.item_basher);
                        var b = Me.GetItemById(AbilityId.item_vanguard) ??
                                Me.Inventory.Backpack.FirstOrDefault(x => x.Id == AbilityId.item_vanguard);
                        var c = Me.GetItemById(AbilityId.item_recipe_abyssal_blade) ??
                                Me.Inventory.Backpack.FirstOrDefault(x => x.Id == AbilityId.item_recipe_abyssal_blade);
                        var list = new List<Item> {a, b, c};
                        foreach (var item in list)
                        {
                            if (!unlocked.Exists(x => x == item.Id) && AddItem(item))
                                unlocked.Add(item.Id);
                        }

                        if (unlocked.Count >= 3)
                            return;
                        await Task.Delay(1);
                    }
                }, (int) (Game.Ping * 2f + 100f));
            };
        }

        private static bool AddItem(Item item)
        {
            if (item == null || !item.IsCombineLocked) return false;
            item.UnlockCombining();
            return true;
        }

        public Hero Me { get; set; }
    }
}