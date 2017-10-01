using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArcAnnihilation.Units.behaviour.Abilities;
using ArcAnnihilation.Units.behaviour.Enabled;
using ArcAnnihilation.Units.behaviour.Items;
using ArcAnnihilation.Units.behaviour.Orbwalking;
using ArcAnnihilation.Units.behaviour.Range;
using ArcAnnihilation.Utils;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Threading;
using Orbwalker = ArcAnnihilation.Utils.Orbwalker;

namespace ArcAnnihilation.Units
{
    public abstract class UnitBase
    {
        public ICanUseAbilties AbilitiesBehaviour;
        public ICanUseOrbwalking OrbwalkingBehaviour;
        public ICanUseItems ItemsBehaviour;
        public IAbilityChecker AbilityChecker;
        public IDrawAttackRange DrawRanger;
        public Ability Flux;
        public Ability MagneticField;
        public Ability Spark;
        public Ability TempestDouble;
        public Hero Hero;
        public Orbwalker Orbwalker;
        public float LastMoveOrderIssuedTime;
        public float CooldownOnMoving = 0.015f;
        public float CooldownOnAttacking = 0.015f;
        public Task ComboTask;
        public bool IsAlive => Hero.IsAlive;
        public bool CanCallCombo => ComboTask == null || ComboTask.IsCanceled || ComboTask.IsCompleted || ComboTask.IsFaulted;

        protected UnitBase()
        {
            AbilitiesBehaviour = new CanNotUseAbilties();
            OrbwalkingBehaviour = new CantUseOrbwalking();
            ItemsBehaviour = new CanNotUseItems();
            DrawRanger = new DontDrawAttackRange();
        }
        public void ExTask(Task combo)
        {
            ComboTask = combo;
        }

        public async Task TargetFinder(CancellationToken cancellationToken)
        {
            while (Core.Target == null || !Core.Target.IsValid)
            {
                Core.Target = TargetSelector.ClosestToMouse(Core.MainHero.Hero, 500);
                Printer.Both(Core.Target != null
                    ? $"[TargetFinder] new target: {Core.Target.Name} | {Core.Target.Handle}"
                    : $"[TargetFinder] trying to find target!");
                await Task.Delay(100, cancellationToken);
            }
        }

        public virtual async Task Combo(CancellationToken cancellationToken)
        {
            if (Hero.IsAlive && !Hero.IsInvisible())
            {
                await TargetFinder(cancellationToken);
                var afterItems = await UseItems(cancellationToken);
                if (afterItems)
                {
                    /*if (Hero.GetItemById(ItemId.item_sheepstick) == null ||
                        !Hero.GetItemById(ItemId.item_sheepstick).CanBeCasted())*/
                    await UseAbilities(cancellationToken);
                }
            }
            await Await.Delay(25, cancellationToken);
            //await Await.Delay(500, cancellationToken);
            /*
            await AbilitiesBehaviour.UseAbilities(this);
            await ItemsBehaviour.UseItems(this);
            await OrbwalkingBehaviour.Attack(this);
            */
        }

        public virtual async Task<bool> UseItems(CancellationToken cancellationToken)
        {
            return await ItemsBehaviour.UseItems(this);
        }

        public virtual async Task UseAbilities(CancellationToken cancellationToken)
        {
            await AbilitiesBehaviour.UseAbilities(this);
        }

        public virtual void Init()
        {
            InitAbilities();
            if (OrbwalkingBehaviour is CanUseOrbwalking || OrbwalkingBehaviour is CanUseOrbwalkingOnlyForPushing)
            {
                Orbwalker = Orbwalker.GetNewOrbwalker(this);
                Orbwalker.Load();
            }
            try
            {
                Printer.Both($"[{this}][init] -> [{Hero?.Name}] [{Hero?.Handle}] [{Orbwalker?.GetHashCode()}]");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public abstract void InitAbilities();
        public abstract void MoveAction(Unit target);
        public abstract IEnumerable<Item> GetItems();
    }
}
