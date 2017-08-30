using Ensage;
using Ensage.Common.Objects.UtilityObjects;
using SfAnnihilation.Features;

namespace SfAnnihilation.Utils
{
    public static class RazeCancelSystem
    {
        public static Ability CurrenyAbility;
        public static Hero Target;
        public static float StartTime;
        public static float AbilityDelay;
        public static float LifeTime;
        public static Sleeper Sleeper = new Sleeper();

        static RazeCancelSystem()
        {
            /*var effect = new ParticleEffect("materials/ensage_ui/particles/range_display_mod.vpcf", ObjectManager.LocalHero.Position);
            effect.SetControlPoint(1, new Vector3(50, 255, 0));
            effect.SetControlPoint(2, new Vector3(255, 0, 0));*/
            Game.OnIngameUpdate += args =>
            {
                /*if (Target != null)
                {
                    var pred = Prediction.PredictedXYZ(Target, AbilityDelay - CustomDelay);
                    effect.SetControlPoint(0, pred);
                }*/
                if (!IsValid)
                {
                    //Printer.Both($"Ability: {CurrenyAbility != null} Target: {Target != null} Phase: {CurrenyAbility?.IsInAbilityPhase} canHit: {CurrenyAbility.CanHit(Target, AbilityDelay - CustomDelay)} Alive: {Target?.IsAlive} Visible: {Target?.IsVisible} Sleeping: {Sleeper.Sleeping}");
                    if (CurrenyAbility != null)
                    {
                        Core.Me.Stop();
                        CurrenyAbility = null;
                    }
                    return;
                }
                
                // Printer.Print($"Custom: {CustomDelay} Time: {AbilityDelay - CustomDelay}");
            };
        }
        
        public static bool IsValid
            =>
                CurrenyAbility != null && Target != null && (CurrenyAbility.IsInAbilityPhase || Sleeper.Sleeping) &&
                CurrenyAbility.CanHit(Target, AbilityDelay - CustomDelay) &&
                Target.IsAlive /*&& Target.IsVisible*/;

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
}