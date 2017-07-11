using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.Common.Objects.UtilityObjects;
using log4net;
using PlaySharp.Toolkit.Logging;

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
            Log.Info($"[{final.Name}] -> [{one.Name}] | [{two.Name}] | [{three.Name}]");
        }

        public AbilityInfo(Ability itemAbility)
        {
            Ability = itemAbility;
            Log.Info($"[{Ability.Name}] -> item");
        }

        public async Task<bool> UseAbility(Hero target, CancellationToken token, float ExtraTime)
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
                        if (target.HasModifier("modifier_invoker_cold_snap"))
                        {
                            Ability.UseAbility(target.Position);
                        }
                        else if (!Ability.CastSkillShot(target))
                        {
                            return false;
                        }
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
                            var timeForCast = timing + ExtraTime/100f + Game.Ping / 1000;
                            var delayTime = (int) ((time - timeForCast) * 1000);
                            Log.Warn($"[SS] delay time: {delayTime} rem time: {time} Time for cast: {timeForCast}");
                            await Task.Delay(delayTime, token);
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
                    Ability.UseAbility();
                    break;
                case AbilityId.invoker_ice_wall:
                    var angle = Ensage.SDK.Extensions.UnitExtensions.FindRotationAngle(ObjectManager.LocalHero, target.NetworkPosition);
                    //if (Ensage.SDK.Extensions.EntityExtensions.Distance2D(target, Ability.Owner) > 300 || angle>0.99)
                    while (Ensage.SDK.Extensions.EntityExtensions.Distance2D(target, Ability.Owner) > 300 ||
                           angle > 0.99)
                    {
                        angle = Ensage.SDK.Extensions.UnitExtensions.FindRotationAngle(ObjectManager.LocalHero, target.NetworkPosition);
                        var hero = Ability.Owner as Hero;
                        hero?.Move(target.NetworkPosition);
                        Log.Debug($"Dist: [{Ensage.SDK.Extensions.EntityExtensions.Distance2D(target, Ability.Owner)}] angle: [{angle}]");
                        await Task.Delay(100, token);
                    }
                    
                    Ability.UseAbility();
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
    }
}