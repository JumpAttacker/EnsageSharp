using ArcAnnihilation.Manager;

namespace ArcAnnihilation.OrderState
{
    public class TempestCombo : Order
    {
        public override bool NeedTarget => true;
        public override void Execute()
        {
            if (Core.TempestHero != null && Core.TempestHero.CanCallCombo)
                Core.TempestHero.ExTask(Core.TempestHero.Combo(Core.ComboToken.Token));
            foreach (var necronomicon in NecronomiconManager.GetNecronomicons)
                if (necronomicon.CanCallCombo)
                    necronomicon.ExTask(necronomicon.Combo(Core.ComboToken.Token));
        }
    }
}