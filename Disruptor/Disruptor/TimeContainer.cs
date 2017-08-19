using System;
using System.Collections.Generic;
using Ensage;
using Ensage.SDK.Helpers;
using SharpDX;

namespace Disruptor
{
    public class TimeContainer : IDisposable
    {
        public Dictionary<double, Vector3> Positions;
        public Hero Owner;
        private readonly bool _superFast;

        public TimeContainer(Hero owner, bool superFast)
        {
            Positions = new Dictionary<double, Vector3> {{ Math.Round(Game.RawGameTime, 2), owner.Position}};
            Owner = owner;
            _superFast = superFast;
            if (!superFast)
                UpdateManager.Subscribe(Cleaner, 500);
        }

        private void Cleaner()
        {
            Cleaner(Math.Round(Game.RawGameTime, 2));
        }

        private void Cleaner(double f)
        {
            var temp = new Dictionary<double, Vector3>();
            var time = f;//Game.RawGameTime;
            foreach (var position in Positions)
            {
                var posTime = position.Key;
                if (time - posTime <= 4)
                    temp.Add(posTime,position.Value);
            }
            Positions = temp;
        }

        public void Update(double time)
        {
            if (Positions.ContainsKey(time))
                return;
            Positions.Add(time, Owner.Position);
            if (_superFast)
                Cleaner(time);
        }

        public void Dispose()
        {
            UpdateManager.Unsubscribe(Cleaner);
            Positions.Clear();
        }
    }
}