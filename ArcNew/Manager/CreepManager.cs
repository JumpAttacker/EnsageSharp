using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.SDK.Helpers;

namespace ArcAnnihilation.Manager
{
    public class CreepManager : IDisposable
    {
        private static CreepManager _creepManager;
        private bool _disposed;
        public CreepManager()
        {
            UpdateManager.Subscribe(Callback, 25);
            _disposed = false;
        }

        private void Callback()
        {
            GetCreeps = EntityManager<Creep>.Entities.Where(unit =>
                unit.IsValid && unit.IsAlive && unit.IsSpawned).ToList();

            /*ObjectManager.GetEntitiesFast<Creep>()
                    .Where(
                        unit =>
                            unit.IsValid && unit.IsAlive && unit.IsSpawned)
                    .ToList();*/
        }

        public static CreepManager GetCreepManager()
        {
            return _creepManager ?? (_creepManager = new CreepManager());
        }

        public List<Creep> GetCreeps { get; private set; } = new List<Creep>();

        public void Dispose()
        {
            if (_disposed)
                return;

            UpdateManager.Unsubscribe(Callback);
            _disposed = true;
        }
    }
}
