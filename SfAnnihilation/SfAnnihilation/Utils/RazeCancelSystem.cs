using System;
using System.Collections.Generic;
using Ensage;
using Ensage.Common.Objects.UtilityObjects;
using SfAnnihilation.Features;

namespace SfAnnihilation.Utils
{
    public static class RazeCancelSystemNewOne
    {
        public static Ability CurrenyAbility;
        public static Hero Target;
        public static float StartTime;
        public static float AbilityDelay;
        public static float LifeTime;
        public static Sleeper Sleeper = new Sleeper();

        static RazeCancelSystemNewOne()
        {
            Game.OnIngameUpdate += args =>
            {
                if (!IsValid)
                    Core.Me.Stop();
            };
        }
        
        public static bool IsValid
            =>
                CurrenyAbility != null && Target != null && CurrenyAbility.IsInAbilityPhase &&
                CurrenyAbility.CanHit(Target, Math.Max(0, AbilityDelay - CustomDelay)) &&
                Target.IsAlive;

        public static float CustomDelay => (Game.RawGameTime - StartTime)*1000;
        public static void InitNewMember(Ability s,Hero newTarget)
        {
            CurrenyAbility = s;
            Target = newTarget;
            StartTime = Game.RawGameTime;
            AbilityDelay = s.GetAbilityDelay();
            Sleeper.Sleep(AbilityDelay);
        }
    }
    internal class RazeCancelSystem
    {
        public static List<StopElement> StopList = new List<StopElement>();
        public static void Updater(EventArgs args)
        {
            var tempList=new List<StopElement>();
            foreach (var element in StopList)
            {
                var ability = element.X;
                var target = element.Target;

                var customDelay = (Game.RawGameTime - element.StartTime)*1000;
                var lifeTime = element.AbilityDelay - customDelay;
                if (element.X.Equals(Core.RazeLow))
                    Printer.Both(lifeTime);
                if (ability.CanHit(target, Math.Max(0, lifeTime)) && target.IsAlive)
                {
                    if (element.Sleeper.Sleeping && lifeTime>0)
                        tempList.Add(element);
                }
                else
                {
                    if (ability.IsInAbilityPhase)
                    {
                        Core.Me.Stop();
                        if (element.X.Equals(Core.RazeLow))
                            Printer.Both("===================STOP========================");
                    }
                }
            }
            StopList = tempList;
        }

        public static bool New(Ability s, Hero target)
        {
            if (StopList.Find(x => x.X.Equals(s)) != null) return false;
            StopList.Add(new StopElement(s, target));
            return true;
        }

        public class StopElement
        {
            public readonly Hero Target;
            public Ability X { get; set; }
            public Sleeper Sleeper;
            public float StartTime;
            public float AbilityDelay;
            public StopElement(Ability x, Hero target)
            {
                Target = target;
                X = x;
                Sleeper = new Sleeper();
                StartTime = Game.RawGameTime;
                AbilityDelay = x.GetAbilityDelay();
                Sleeper.Sleep(AbilityDelay);
                if (x.Equals(Core.RazeLow))
                    Printer.Both("===================NEW========================");
            }
        }
    }
}