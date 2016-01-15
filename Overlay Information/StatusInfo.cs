using Ensage;
using Ensage.Common.Extensions;

namespace OverlayInformation
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
            var curStat = GetStatus();
            if (_status != GetStatus())
            {
                _time = Game.GameTime;
                _status = curStat;
            }
            if (curStat == "visible") return curStat;
            return curStat + " " + (int)(Game.GameTime - _time);
        }

        public string GetStatus()
        {
            return !_hero.IsValid ? "heh" : _hero.IsInvisible() ? "invis" : _hero.IsVisible ? "visible" : "in fog";
        }

    }
}