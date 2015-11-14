using Ensage;
using Ensage.Common.Extensions;

namespace OverlayInformationLight
{
    internal class StatusInfo
    {
        private readonly Hero _hero;
        private float _time;
        private string _status;

        public StatusInfo(Hero hero, float time)
        {
            _hero = hero;
            _time = time;
            _status = "";
        }

        public Hero GetHero()
        {
            return _hero;
        }

        public string GetTime()
        {
            var curStat=GetStatus();
            if (_status == GetStatus()) return curStat + " " + (int) (Game.GameTime - _time);
            _status = curStat;
            _time = Game.GameTime;
            return curStat + " " + (int) (Game.GameTime - _time);
        }

        public string GetStatus()
        {
            return _hero.IsInvisible() ? "invis" : _hero.IsVisible ? "visible" : "in fog";
        }
    }
}