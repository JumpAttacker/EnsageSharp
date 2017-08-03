using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using InvokerAnnihilationCrappa.Features.behavior;

namespace InvokerAnnihilationCrappa
{
    public class Combo : Clickable
    {
        public List<AbilityInfo> AbilityInfos { get; set; }
        private static int _counter;
        public int Id;
        public int AbilityCount;
        public int CurrentAbility;

        public Combo(Invoker invoker, AbilityInfo[] abilityInfo)
        {
            AbilityInfos = abilityInfo.ToList();
            Id = _counter++;
            AbilityCount = abilityInfo.Length;
            CurrentAbility = 0;
            LoadClickable();
            OnClick += () =>
            {
                if (invoker.Config.ComboPanel.CanChangeByClicking)
                    invoker.SelectedCombo = Id;
            };
        }

        public void AddToCombo(AbilityInfo ability)
        {
            var index = AbilityInfos.FindIndex(x => x.Ability.Id == AbilityId.invoker_deafening_blast);
            var after3 = Math.Min(index == -1 ? 3 : index + 1, AbilityInfos.Count);
            AbilityInfos.Insert(after3, ability);
            AbilityCount++;
        }

        public void RemoveFromCombo(AbilityInfo ability)
        {
            AbilityInfos.Remove(ability);
            AbilityCount--;
        }

        public Combo Dispose()
        {
            UnloadClickable();
            _counter--;
            return this;
        }
    }
}