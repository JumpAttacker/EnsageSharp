using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.Common.Objects.UtilityObjects;
using Ensage.SDK.Helpers;
using log4net;
using PlaySharp.Toolkit.Logging;
using SharpDX;
using UnitExtensions = Ensage.SDK.Extensions.UnitExtensions;

namespace InvokerAnnihilationCrappa
{
    public class AbilityInfo
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public Ability Ability { get; }
        public Ability One { get; }
        public Ability Two { get; }
        public Ability Three { get; }
        private static readonly Sleeper AfterTornado = new Sleeper();
        public AbilityInfo(Ability one, Ability two, Ability three, Ability final)
        {
            Ability = final;
            One = one;
            Two = two;
            Three = three;
            Name = Ability.Name;
            Log.Info($"[{final.Name}] -> [{one.Name}] | [{two.Name}] | [{three.Name}]");
        }

        public void LoadInvoker(Invoker invo)
        {
            Me = invo;
        }

        private Invoker Me { get; set; }
        public string Name { get; set; }
        public AbilityInfo(Ability itemAbility)
        {
            Ability = itemAbility;
            Name = Ability.Name;
            Log.Info($"[{Ability.Name}] -> item");
        }

        public async Task<bool> UseAbility(Hero target, CancellationToken token)
        {
            var comboModifiers = target.HasModifiers(new[]
            {
                "modifier_obsidian_destroyer_astral_imprisonment_prison", "modifier_eul_cyclone",
                "modifier_shadow_demon_disruption", "modifier_invoker_tornado"
            }, false);
            float time;
            var isStunned = target.IsStunned(out time);
            switch (Ability.Id)
            {
                case AbilityId.invoker_cold_snap:
                    if (target.IsMagicImmune())
                        return false;
                    Ability.UseAbility(target);
                    break;
                case AbilityId.invoker_ghost_walk:
                    Ability.UseAbility();
                    break;
                case AbilityId.invoker_tornado:
                    if (comboModifiers && isStunned)
                    {
                        return true;
                    }
                    if (!Ability.CastSkillShot(target))
                        return false;
                    AfterTornado.Sleep((float) Ability.GetHitDelay(target)*1000);
                    break;
                case AbilityId.invoker_emp:
                    if (AfterTornado.Sleeping)
                        return false;
                    Ability.UseAbility(target.Position);
                    break;
                case AbilityId.invoker_alacrity:
                    Ability.UseAbility(Ability.Owner as Hero);
                    break;
                case AbilityId.invoker_chaos_meteor:
                    if (AfterTornado.Sleeping)
                        return false;
                    if (comboModifiers && isStunned)
                    {
                        var timing = 1.3f;
                        if (time <= timing + Game.Ping / 1000)
                        {
                            Ability.UseAbility(target.Position);
                        }
                        else
                        {
                            await Task.Delay((int)((time - timing + Game.Ping / 1000) * 1000), token);
                            Ability.UseAbility(target.Position);
                        }
                    }
                    else
                    {
                        //var blastModifier = target.FindModifier("modifier_invoker_deafening_blast_knockback");
                        //if (blastModifier == null)
                        //{
                        if (
                            target.HasModifiers(
                                new[] {"modifier_invoker_cold_snap", "modifier_invoker_deafening_blast_knockback"},
                                false))
                        {
                            Ability.UseAbility(target.NetworkPosition);
                        }
                        else if (!Ability.CastSkillShot(target))
                        {
                            return false;
                        }
                        /*}
                        else
                        {
                            var remTime = blastModifier.RemainingTime;
                            var newPost
                        }*/
                    }
                    break;
                case AbilityId.invoker_sun_strike:
                    if (AfterTornado.Sleeping)
                        return false;
                    if (comboModifiers && isStunned)
                    {
                        var timing = 1.7f;
                        if (time <= timing + Game.Ping / 1000)
                        {
                            Ability.UseAbility(target.Position);
                        }
                        else
                        {
                            var timeForCast = timing + Me.Config.SsExtraDelay/ 100f + Game.Ping / 1000;
                            var delayTime = (int) ((time - timeForCast) * 1000);
                            Log.Warn($"[SS] delay time: {delayTime} rem time: {time} Time for cast: {timeForCast}");
                            await Task.Delay(Math.Max(delayTime, 1), token);
                            Ability.UseAbility(target.Position);
                        }
                    }
                    else
                    {
                        if (!Ability.CastSkillShot(target))
                            return false;
                    }
                    break;
                case AbilityId.invoker_forge_spirit:
                    var forges =
                        EntityManager<Unit>.Entities.Any(
                            x =>
                                x.IsValid && x.Team==Me.Owner.Team && x.ClassId == ClassId.CDOTA_BaseNPC_Invoker_Forged_Spirit &&
                                UnitExtensions.HealthPercent(x) > .55f);
                    if (!forges)
                        Ability.UseAbility();
                    break;
                case AbilityId.invoker_ice_wall:
                    if (Me.Config.AutoIceWall)
                    {
                        var angle = UnitExtensions.FindRotationAngle(ObjectManager.LocalHero,
                            target.NetworkPosition);
                        //if (Ensage.SDK.Extensions.EntityExtensions.Distance2D(target, Ability.Owner) > 300 || angle>0.99)
                        while (Ensage.SDK.Extensions.EntityExtensions.Distance2D(target, Ability.Owner) > 300 ||
                               angle > 0.99)
                        {
                            angle = UnitExtensions.FindRotationAngle(ObjectManager.LocalHero,
                                target.NetworkPosition);
                            var hero = Ability.Owner as Hero;
                            hero?.Move(target.NetworkPosition);
                            Log.Debug(
                                $"Dist: [{Ensage.SDK.Extensions.EntityExtensions.Distance2D(target, Ability.Owner)}] angle: [{angle}]");
                            await Task.Delay(100, token);
                        }
                        Ability.UseAbility();
                    }
                    else
                    {
                        if (IceWallCanHit(target))
                        {
                            Me.BlockerSleeper.Sleep(300);
                            Me.Owner.Stop();
                            //Me.Owner.Move(Me.Owner.NetworkPosition + UnitExtensions.Direction(Me.Owner, 10));
                            //await Task.Delay(100, token);
                            Ability.UseAbility();
                            await Task.Delay(300, token);
                        }
                        else
                            return false;
                    }
                    break;
                case AbilityId.invoker_deafening_blast:
                    if (target.IsMagicImmune())
                        return false;
                    if (!Ability.CastSkillShot(target))
                        return false;
                    break;
                case AbilityId.item_cyclone:
                    if (comboModifiers && isStunned)
                    {
                        return true;
                    }
                    Ability.UseAbility(target);
                    await WaitForCombo(target);
                    break;
                case AbilityId.item_refresher:
                    Ability.UseAbility();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return true;
        }

        private async Task WaitForCombo(Hero target)
        {
            while (!target.HasModifiers(new[]
            {
                "modifier_obsidian_destroyer_astral_imprisonment_prison", "modifier_eul_cyclone",
                "modifier_shadow_demon_disruption", "modifier_invoker_tornado"
            }, false))
            {
                await Task.Delay(100);
            }
        }

        private bool IceWallCanHit(Hero target)
        {
            var myPos = Me.Owner.NetworkPosition;
            var startPos = myPos + UnitExtensions.Direction(Me.Owner, 200);
            var rotation = Me.Owner.NetworkRotationRad;
            var toLeft = rotation - Math.PI / 2;
            var toRight = rotation + Math.PI / 2;
            for (var i = 0; i < 7; i++)
            {
                var pos = startPos + Direction(toLeft, 80 * i);
                if (Ensage.SDK.Extensions.EntityExtensions.IsInRange(target, pos, 50))
                {
                    return true;
                }
            }
            for (var i = 0; i < 7; i++)
            {
                var pos = startPos + Direction(toRight, 80 * i);
                if (Ensage.SDK.Extensions.EntityExtensions.IsInRange(target, pos, 50))
                {
                    return true;
                }
            }
            return false;
        }

        private static Vector3 Direction(double angle, float length = 1f)
        {
            return new Vector3((float)Math.Cos(angle) * length, (float)Math.Sin(angle) * length, 0);
        }

        public void UpdateKey(uint valueKey)
        {
            Key = valueKey;
        }

        public uint Key { get; set; }
    }
}