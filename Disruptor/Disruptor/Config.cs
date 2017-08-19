using System;
using Ensage.SDK.Menu;
using SharpDX;

namespace Disruptor
{
    public class Config : IDisposable
    {
        public Config(Disruptor disruptor)
        {
            Factory = MenuFactory.Create("Disruptor.Glimpse");
            Main = disruptor;
            DrawFinalPosition = Factory.Item("Draw final position", true);
            DrawFullPath = Factory.Item("Draw full path", true);
            SuperFastMode = Factory.Item("Fast mode (less fps)", true);

            SuperFastMode.PropertyChanged += (sender, args) =>
            {
                foreach (var container in disruptor.Timer)
                {
                    container.Dispose();
                }
                disruptor.Timer.Clear();
            };
        }

        public MenuItem<bool> SuperFastMode { get; set; }

        public MenuItem<bool> DrawFullPath { get; set; }

        public MenuItem<bool> DrawFinalPosition { get; set; }

        public MenuFactory Factory { get; set; }

        public Disruptor Main { get; set; }

        public void Dispose()
        {
            Factory?.Dispose();
        }
    }
}