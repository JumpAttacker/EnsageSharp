using System;
using System.Collections.Generic;
using System.Linq;
using ArcAnnihilation.Units;
using ArcAnnihilation.Units.behaviour.Orbwalking;
using ArcAnnihilation.Units.behaviour.Range;
using Ensage;
using Ensage.SDK.Extensions;
using Ensage.SDK.Helpers;
using Ensage.SDK.Renderer.Particle;
using SharpDX;
using UnitExtensions = Ensage.Common.Extensions.UnitExtensions;

namespace ArcAnnihilation.Utils
{
    public class Orbwalker
    {
        private readonly HashSet<NetworkActivity> _attackActivityList = new HashSet<NetworkActivity>
        {
            NetworkActivity.Attack,
            NetworkActivity.Attack2,
            NetworkActivity.AttackEvent
        };

        public enum OrbwalkingMode
        {
            Idle, Combo, Pushing
        }

        public OrbwalkingMode Mode;
        private Orbwalker(UnitBase basic)
        {
            BasicUnit = basic;
            if (basic.Hero != null)
                Owner = basic.Hero;
            else
            {
                var necr = basic as Necronomicon;
                if (necr != null) Owner = necr.Necr;
                else
                    Printer.Log("cant init "+ basic, true);
            }
        }

        public bool LaneClearRateLimitResult { get; set; }

        public float LaneClearRateLimitTime { get; set; }

        public float LastAttackOrderIssuedTime { get; set; }

        public float LastAttackTime { get; set; }

        public float LastMoveOrderIssuedTime { get; set; }

        public Unit LastTarget { get; set; }

        public Unit Owner { get; set; }

        public UnitBase BasicUnit;

        public float TurnEndTime { get; set; }

        private ParticleManager EffectManager { get; set; }

        private bool Initialized { get; set; }

        public static Orbwalker GetNewOrbwalker(UnitBase basic)
        {
            return new Orbwalker(basic);
        }

        public bool Attack(Unit unit)
        {
            var time = Game.RawGameTime;
            if (time - LastAttackOrderIssuedTime < BasicUnit.CooldownOnAttacking)
            {
                return false;
            }

            TurnEndTime = Game.RawGameTime + Game.Ping / 2000f + Owner.TurnTime(unit.NetworkPosition) + 0.1f;
            Owner.Attack(unit);
            return true;
        }

        public bool CanAttack(Unit target)
        {
            var rotationTime = Owner.TurnTime(target.NetworkPosition);
            return Owner.CanAttack() && Game.RawGameTime + 0.1f + rotationTime + Game.Ping / 2000f - LastAttackTime > 1f / Owner.AttacksPerSecond;
        }

        public bool CanMove(Unit target)
        {
            return Game.RawGameTime - 0.1f + Game.Ping / 2000f - LastAttackTime > Owner.AttackPoint();
        }

        public Unit GetTarget()
        {
            try
            {
                switch (Mode)
                {
                    case OrbwalkingMode.Pushing:
                        if (MenuManager.TowerPriority)
                        {
                            var tower =
                                ObjectManager.GetEntitiesFast<Tower>()
                                    .FirstOrDefault(
                                        unit =>
                                            unit.IsValid && unit.IsAlive && unit.Team != Owner.Team &&
                                            Owner.IsValidOrbwalkingTarget(unit));

                            if (tower != null)
                            {
                                return tower;
                            }
                        }
                        var barracks =
                            ObjectManager.GetEntitiesFast<Building>()
                                .FirstOrDefault(
                                    unit =>
                                        unit.IsValid && unit.IsAlive && unit.Team != Owner.Team && !(unit is Tower) &&
                                        Owner.IsValidOrbwalkingTarget(unit) && unit.Name != "portrait_world_unit");

                        if (barracks != null)
                        {
                            return barracks;
                        }

                        var jungleMob =
                            EntityManager<Creep>.Entities.FirstOrDefault(
                                unit =>
                                    unit.IsValid && unit.IsSpawned && unit.IsAlive && unit.IsNeutral &&
                                    Owner.IsValidOrbwalkingTarget(unit));

                        if (jungleMob != null)
                        {
                            return jungleMob;
                        }

                        var creep =
                            EntityManager<Creep>.Entities.Where(
                                    unit =>
                                        unit.IsValid && unit.IsSpawned && unit.IsAlive && unit.Team != Owner.Team &&
                                        (Owner.IsValidOrbwalkingTarget(unit) || Owner.Distance2D(unit) <= 500))
                                .OrderBy(x => x.Health)
                                .FirstOrDefault();

                        if (creep != null)
                        {
                            return creep;
                        }
                        if (!MenuManager.TowerPriority)
                        {
                            var tower =
                                ObjectManager.GetEntitiesFast<Tower>()
                                    .FirstOrDefault(
                                        unit =>
                                            unit.IsValid && unit.IsAlive && unit.Team != Owner.Team &&
                                            Owner.IsValidOrbwalkingTarget(unit));

                            if (tower != null)
                            {
                                return tower;
                            }
                        }
                        var others =
                            ObjectManager.GetEntitiesFast<Unit>()
                                .FirstOrDefault(
                                    unit =>
                                        unit.IsValid && !(unit is Hero) && !(unit is Creep) && unit.IsAlive &&
                                        !unit.IsInvulnerable() && unit.Team != Owner.Team &&
                                        Owner.IsValidOrbwalkingTarget(unit) && unit.ClassId != ClassId.CDOTA_BaseNPC);

                        if (others != null)
                        {
                            return others;
                        }
                        break;
                    case OrbwalkingMode.Combo:
                        return Core.Target;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return null;
        }

        public bool Load()
        {
            if (Initialized)
            {
                return false;
            }

            Initialized = true;
            EffectManager = new ParticleManager();
            if (BasicUnit.DrawRanger is DrawAttackRange)
                UpdateManager.Subscribe(OnDrawingsUpdate, 1000);
            

            Game.OnIngameUpdate += OnOnIngameUpdate;
            Entity.OnInt32PropertyChange += Hero_OnInt32PropertyChange;

            return true;
        }

        public bool Move(Vector3 position)
        {
            var time = Game.RawGameTime;
            if (time - LastMoveOrderIssuedTime < 5 / 1000f)
            {
                // 0.005f
                return false;
            }

            LastMoveOrderIssuedTime = Game.RawGameTime;

            Owner.Move(position);
            return true;
        }

        public bool Unload()
        {
            if (!Initialized)
            {
                return false;
            }

            Initialized = false;
            if (BasicUnit.DrawRanger is DrawAttackRange)
                UpdateManager.Unsubscribe(OnDrawingsUpdate);
            Game.OnIngameUpdate -= OnOnIngameUpdate;
            Entity.OnInt32PropertyChange -= Hero_OnInt32PropertyChange;

            EffectManager?.Dispose();
            return true;
        }

        private void OnOnIngameUpdate(EventArgs args)
        {
            if (!MenuManager.IsEnable)
                return;

            Mode = OrbwalkingMode.Idle;

            var isTempest = BasicUnit as Tempest;
            if (isTempest != null && (Game.IsPaused || Game.IsChatOpen || !BasicUnit.IsAlive || !isTempest.IsValid))
            {
                return;
            }
            if (OrderManager.CurrentOrder == OrderManager.Orders.DefaultCombo ||
                (OrderManager.CurrentOrder == OrderManager.Orders.TempestCombo && (isTempest != null ||
                 !(BasicUnit is MainHero))))
            {
                if (BasicUnit.OrbwalkingBehaviour is CanUseOrbwalking)
                    Mode = OrbwalkingMode.Combo;
            }
            else if (OrderManager.CurrentOrder == OrderManager.Orders.AutoPushing)
            {
                Mode = OrbwalkingMode.Pushing;
                if (BasicUnit is MainHero)
                    return;
            }
            else if (Mode == OrbwalkingMode.Idle)
            {
                EffectManager.Remove("attackTarget" + Owner.Handle);
                return;
            }
            // turning
            if (TurnEndTime > Game.RawGameTime)
            {
                return;
            }
            var target = GetTarget();

            if ((target == null || !CanAttack(target) || UnitExtensions.IsAttackImmune(target)) && CanMove(target))
            {
                if (BasicUnit.OrbwalkingBehaviour is CanUseOrbwalking)
                    BasicUnit.MoveAction(target);
                return;
            }

            if (target != null && CanAttack(target))
            {
                LastTarget = target;
                Attack(target);
            }


            if (BasicUnit.OrbwalkingBehaviour is CanUseOrbwalking)
            {
                if (LastTarget != null && LastTarget.IsValid && LastTarget.IsAlive)
                {
                    EffectManager.DrawRange(LastTarget, "attackTarget" + Owner.Handle, 60, Color.Red);
                }
                else
                {
                    EffectManager.Remove("attackTarget" + Owner.Handle);
                }
            }
        }

        private void Hero_OnInt32PropertyChange(Entity sender, Int32PropertyChangeEventArgs args)
        {
            if (!ReferenceEquals(sender, Owner))
            {
                return;
            }

            if (!args.PropertyName.Equals("m_networkactivity", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            var newNetworkActivity = (NetworkActivity)args.NewValue;

            if (_attackActivityList.Contains(newNetworkActivity))
            {
                LastAttackTime = Game.RawGameTime - Game.Ping / 2000f;
            }
        }

        private void OnDrawingsUpdate()
        {
            if (!MenuManager.IsEnable)
                return;
            EffectManager.DrawRange(Owner, "attackRange" + Owner.Handle, Owner.AttackRange(Owner), Color.LimeGreen);
        }
    }
}
