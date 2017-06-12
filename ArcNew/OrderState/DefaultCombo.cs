using ArcAnnihilation.Manager;
using Ensage.Common.Menu;

namespace ArcAnnihilation.OrderState
{
    public class DefaultCombo : Order
    {
        public override bool NeedTarget => true;
        public override bool CanBeExecuted => MenuManager.DefaultCombo.GetValue<KeyBind>().Active;
        public override void Execute()
        {
            if (Core.MainHero.CanCallCombo)
                Core.MainHero.ExTask(Core.MainHero.Combo(Core.ComboToken.Token));
            if (Core.TempestHero != null && Core.TempestHero.CanCallCombo)
                Core.TempestHero.ExTask(Core.TempestHero.Combo(Core.ComboToken.Token));
            foreach (var necronomicon in NecronomiconManager.GetNecronomicons)
                if (necronomicon.CanCallCombo)
                    necronomicon.ExTask(necronomicon.Combo(Core.ComboToken.Token));
        }
    }
}