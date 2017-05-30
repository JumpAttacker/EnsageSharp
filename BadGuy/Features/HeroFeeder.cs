using Ensage;
using Ensage.Common.Menu;
using Ensage.Common.Objects.UtilityObjects;

namespace BadGuy.Features
{
    public static class HeroFeeder
    {
        private static readonly Sleeper RateSleeper = new Sleeper();
        public static void Updater()
        {
            var me = ObjectManager.LocalHero;
            if (!me.IsAlive)
                return;
            if (RateSleeper.Sleeping)
                return;
            var fount = Helpers.GetEnemyFountain();
            if (fount == null)
                return;
            RateSleeper.Sleep(BadGuy.Config.HeroFeeder.Rate);
            if (BadGuy.Config.HeroFeeder.Type.Item.GetValue<StringList>().SelectedIndex == 0)
                me.Move(fount.Position);
            else
                me.Attack(fount.Position);
        }
    }
}