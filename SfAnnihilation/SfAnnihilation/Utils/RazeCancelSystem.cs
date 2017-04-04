using System;
using System.Collections.Generic;
using Ensage;
using Ensage.Common.Objects.UtilityObjects;
using SfAnnihilation.Features;

namespace SfAnnihilation.Utils
{
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
                if (ability.CanHit(target, Math.Max(0,element.AbilityDelay-customDelay)) && target.IsAlive)
                {
                    if (element.Sleeper.Sleeping)
                        tempList.Add(element);
                }
                else
                {
                    if (ability.IsInAbilityPhase)
                    {
                        Core.Me.Stop();
                    }
                }
            }
            StopList = tempList;
        }

        public static void New(Ability s, Hero target)
        {
            if (StopList.Find(x => x.X.Equals(s)) == null)
                StopList.Add(new StopElement(s, target));
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
            }
        }
    }
}