using Ensage;

namespace OverlayInformation.Features.Open_Dota
{
    public class PlayerInfo
    {
        public string Country;
        public Hero Hero;

        public int Id;
        public int Party;
        public string PossibleMmr;
        public int Solo;
        public string TotalGames;
        public string Wins;
        public string Wr;
        public int WrOnCurrentHero;

        public PlayerInfo(int id, int solo, int party, string country, string possibleMmr, string winrate,
            string matches, string name)
        {
            Matches = matches;
            Id = id;
            Solo = solo;
            Party = party;
            Country = country;
            PossibleMmr = possibleMmr;
            Wr = winrate;
            Name = name.Length > 10 ? name.Substring(0, 10) : name;
        }

        public PlayerInfo(int id, int solo, int party, string country, string possibleMmr, string winrate,
            string matches, string name, Hero hero, string totalGames, string wins, int wrOnCurrentHero)
            : this(id, solo, party, country, possibleMmr, winrate, matches, name)
        {
            Hero = hero;
            TotalGames = totalGames;
            Wins = wins;
            WrOnCurrentHero = wrOnCurrentHero;
        }

        public string Matches { get; set; }
        public string Name { get; set; }
    }
}