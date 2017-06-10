using System.Linq;
using ArcAnnihilation.Utils;
using Ensage;
using Ensage.SDK.Extensions;
using Ensage.SDK.Helpers;

namespace ArcAnnihilation.Manager
{
    internal class TempestManager
    {
        public static void Fresh()
        {
            Tempest =
                EntityManager<Hero>.Entities
                    .FirstOrDefault(
                        x => x != null && x.IsValid && x.Team == ObjectManager.LocalHero.Team && x.IsAlive && x.HasModifier("modifier_kill"));
            if (Tempest == null || !Tempest.IsValid)
                UpdateManager.Subscribe(TempestUpdater, 100);
        }

        private static void TempestUpdater()
        {
            if (Tempest == null || !Tempest.IsValid)
            {
                Tempest =
                    EntityManager<Hero>.Entities
                        .FirstOrDefault(
                            x =>
                                x != null && x.IsValid && x.Team == ObjectManager.LocalHero.Team && x.IsAlive &&
                                x.HasModifier("modifier_kill"));
                if (Tempest != null)
                {
                    Printer.Both($"[TempestManager][Add] id: {Tempest.Handle}");
                    UpdateManager.Unsubscribe(TempestUpdater);
                }
            }
        }

        public static Hero Tempest { get; set; }
    }
}