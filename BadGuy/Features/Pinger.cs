using System.Threading.Tasks;
using Ensage;
using Ensage.Common.Extensions.SharpDX;
using Ensage.Common.Menu;
using Ensage.Common.Objects;

namespace BadGuy.Features
{
    public static class Pinger
    {
        public static async void Updater()
        {
            var printType = BadGuy.Config.Pinger.Type.Item.GetValue<StringList>().SelectedIndex == 0
                ? PingType.Danger
                : PingType.Normal;
            while (BadGuy.Config.Pinger.Enable)
            {
                foreach (var hero in Heroes.GetByTeam(ObjectManager.LocalHero.Team))
                {
                    if (BadGuy.Config.Pinger.Toggler.Value.IsEnabled(hero.StoredName()))
                    {
                        Network.MapPing(hero.Position.ToVector2(), printType);
                        await Task.Delay(BadGuy.Config.Pinger.Rate);
                    }
                }
                await Task.Delay(5);
            }
        }
    }
}