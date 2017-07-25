using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Common.Objects.UtilityObjects;
using Ensage.SDK.Extensions;
using Ensage.SDK.Helpers;
using Ensage.SDK.Renderer.Particle;
using SharpDX;
using AbilityExtensions = Ensage.Common.Extensions.AbilityExtensions;
using EntityExtensions = Ensage.Common.Extensions.EntityExtensions;
using UnitExtensions = Ensage.SDK.Extensions.UnitExtensions;

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
        private MultiSleeper _sleeper;

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
            return UnitExtensions.CanAttack(Owner) && Game.RawGameTime + 0.1f + rotationTime + Game.Ping / 2000f - LastAttackTime > 1f / Owner.AttacksPerSecond;
        }

        public bool CanMove()
        {
            return Game.RawGameTime - 0.1f + Game.Ping / 2000f - LastAttackTime > UnitExtensions.AttackPoint(Owner);
        }

        private Hero _globalTarget;

        public Hero GetTarget(bool otherChecks = true)
        {
            if (_globalTarget != null && _globalTarget.IsAlive)
            {
                if (otherChecks)
                {
                    if (!Owner.IsValidOrbwalkingTarget(_globalTarget) &&
                        !UnitExtensions.HasModifier(Owner, "modifier_item_hurricane_pike_range"))
                        return null;
                }
                return _globalTarget;
            }
            _globalTarget = TargetSelector.ClosestToMouse(Owner);
            if (_globalTarget != null &&
                (!otherChecks || Owner.IsValidOrbwalkingTarget(_globalTarget) ||
                 UnitExtensions.HasModifier(Owner, "modifier_item_hurricane_pike_range")))
                return _globalTarget;
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
            _sleeper = new MultiSleeper();
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
                _globalTarget = null;
                EffectManager.Remove("attackTarget" + Owner.Handle);
                return;
            }
            if (TurnEndTime > Game.RawGameTime)
            {
                return;
            }
            var target = GetTarget();
            if (!UnitExtensions.HasModifier(Owner,"modifier_templar_assassin_meld"))
            {
                var tempTarget = GetTarget(false);
                if (tempTarget != null)
                {
                    var blink = Owner.GetItemById(AbilityId.item_blink);
                    var tempPos = new Vector3();
                    if (blink != null && blink.CanBeCasted() && blink.CanHit(tempTarget, ref tempPos) &&
                        !_sleeper.Sleeping(blink))
                    {
                        blink.UseAbility(tempPos);
                        _sleeper.Sleep(300, blink);
                    }
                    if (!UnitExtensions.HasModifier(tempTarget, "modifier_templar_assassin_trap_slow"))
                    {
                        var trap = UnitExtensions.GetAbilityById(ObjectManager.LocalHero,
                            AbilityId.templar_assassin_psionic_trap);
                        var trapNearTarget =
                            EntityManager<Unit>.Entities.FirstOrDefault(
                                x =>
                                    x.IsValid && x.Name == "npc_dota_templar_assassin_psionic_trap" &&
                                    Ensage.SDK.Extensions.EntityExtensions.Distance2D(tempTarget, x.Position) <= 400);
                        if (trapNearTarget != null)
                        {
                            if (!_sleeper.Sleeping(trap + "activate"))
                            {
                                var activator = trapNearTarget.Spellbook.Spell1;
                                if (activator.CanBeCasted())
                                {
                                    activator.UseAbility();
                                }
                                _sleeper.Sleep(300, trap + "activate");
                            }
                        }
                        else if (trap.CanBeCasted())
                        {
                            if (!_sleeper.Sleeping(trap + "place"))
                            {
                                trap.UseAbility(tempTarget.Position);
                                _sleeper.Sleep(300, trap + "place");
                            }
                        }
                    }

                }
            }
            if ((target == null || !CanAttack(target)) && CanMove())
            {
                if (Move(Game.MousePosition))
                    return;
            }
            
            if (target != null && CanAttack(target))
            {
                LastTarget = target;
                var refraction = UnitExtensions.GetAbilityById(ObjectManager.LocalHero, AbilityId.templar_assassin_refraction);
                if (refraction.CanBeCasted() && !_sleeper.Sleeping(refraction))
                {
                    refraction.UseAbility();
                    _sleeper.Sleep(300, refraction);
                }
                var meld = UnitExtensions.GetAbilityById(ObjectManager.LocalHero, AbilityId.templar_assassin_meld);
                if (meld.CanBeCasted() && !_sleeper.Sleeping(meld))
                {
                    meld.UseAbility();
                    _sleeper.Sleep(300,meld);
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
