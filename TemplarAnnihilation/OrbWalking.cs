using System;
using System.Collections.Generic;
using Ensage;
using Ensage.Common;
using Ensage.Common.Menu;
using Ensage.Common.Objects.UtilityObjects;
using Ensage.SDK.Extensions;
using Ensage.SDK.Renderer.Particle;
using SharpDX;
using AbilityExtensions = Ensage.Common.Extensions.AbilityExtensions;

namespace TemplarAnnihilation
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
            Idle, Combo
        }

        public OrbwalkingMode Mode;
        private Orbwalker(Unit me)
        {
            Owner = me;
        }

        public bool LaneClearRateLimitResult { get; set; }

        public float LaneClearRateLimitTime { get; set; }

        public float LastAttackOrderIssuedTime { get; set; }

        public float LastAttackTime { get; set; }

        public float LastMoveOrderIssuedTime { get; set; }

        public Unit LastTarget { get; set; }

        public Unit Owner { get; set; }


        public float TurnEndTime { get; set; }

        private ParticleManager EffectManager { get; set; }

        private bool Initialized { get; set; }
        private Sleeper _sleeper;

        public static Orbwalker GetNewOrbwalker(Unit me)
        {
            return new Orbwalker(me);
        }

        public bool Attack(Unit unit)
        {
            var time = Game.RawGameTime;
            if (time - LastAttackOrderIssuedTime < CooldownOnAttacking)
            {
                return false;
            }

            TurnEndTime = Game.RawGameTime + Game.Ping / 2000f + Owner.TurnTime(unit.NetworkPosition) + 0.1f;
            Owner.Attack(unit);
            return true;
        }

        public double CooldownOnAttacking = 0.05;

        public bool CanAttack(Unit target)
        {
            var rotationTime = Owner.TurnTime(target.NetworkPosition);
            return Owner.CanAttack() && Game.RawGameTime + 0.1f + rotationTime + Game.Ping / 2000f - LastAttackTime > 1f / Owner.AttacksPerSecond;
        }

        public bool CanMove()
        {
            return Game.RawGameTime - 0.1f + Game.Ping / 2000f - LastAttackTime > Owner.AttackPoint();
        }

        public Unit GetTarget()
        {
            var target = TargetSelector.ClosestToMouse(Owner, 500);
            if (target != null && (Owner.IsValidOrbwalkingTarget(target) || Owner.HasModifier("modifier_item_hurricane_pike_range")))
                return target;
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

            Game.OnIngameUpdate += OnOnIngameUpdate;
            Entity.OnInt32PropertyChange += Hero_OnInt32PropertyChange;
            _sleeper=new Sleeper();
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
            /*if (sleeper.Sleeping)
                return false;*/

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
            Game.OnIngameUpdate -= OnOnIngameUpdate;
            Entity.OnInt32PropertyChange -= Hero_OnInt32PropertyChange;

            EffectManager?.Dispose();
            return true;
        }

        private void OnOnIngameUpdate(EventArgs args)
        {
            Mode = OrbwalkingMode.Idle;

            if (Game.IsPaused || Game.IsChatOpen)
            {
                return;
            }
            if (Members.Menu.Item("Combo.key").GetValue<KeyBind>().Active)
            {
                Mode = OrbwalkingMode.Combo;

            }
            else if (Mode == OrbwalkingMode.Idle)
            {
                EffectManager.Remove("attackTarget" + Owner.Handle);
                return;
            }
            if (TurnEndTime > Game.RawGameTime)
            {
                return;
            }
            var target = GetTarget();

            if ((target == null || !CanAttack(target)) && CanMove())
            {
                if (Move(Game.MousePosition))
                    return;
            }

            if (target != null && CanAttack(target))
            {
                LastTarget = target;
                var meld = ObjectManager.LocalHero.GetAbilityById(AbilityId.templar_assassin_meld);
                if (AbilityExtensions.CanBeCasted(meld) && !_sleeper.Sleeping)
                {
                    meld.UseAbility();
                    _sleeper.Sleep(300);
                }
                Attack(target);
            }
            if (LastTarget != null && LastTarget.IsValid && LastTarget.IsAlive)
            {
                EffectManager.DrawRange(LastTarget, "attackTarget" + Owner.Handle, 60, Color.Red);
            }
            else
            {
                EffectManager.Remove("attackTarget" + Owner.Handle);
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
    }
}
