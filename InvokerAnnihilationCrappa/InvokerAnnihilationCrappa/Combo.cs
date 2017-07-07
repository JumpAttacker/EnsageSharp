using InvokerAnnihilationCrappa.Features.behavior;

namespace InvokerAnnihilationCrappa
{
    public class Combo : Clickable
    {
        public AbilityInfo[] AbilityInfos { get; set; }
        private static int _counter;
        public int Id;
        public int AbilityCount;
        public int CurrentAbility;

        public Combo(Invoker invoker, AbilityInfo[] abilityInfo)
        {
            AbilityInfos = abilityInfo;
            Id = _counter++;
            AbilityCount = abilityInfo.Length;
            CurrentAbility = 0;
            LoadClickable();
            OnClick += () =>
            {
                invoker.SelectedCombo = Id;
            };
        }

        public Combo Dispose()
        {
            UnloadClickable();
            _counter--;
            return this;
        }
    }
}