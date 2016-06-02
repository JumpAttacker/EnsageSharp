using System.Collections.Generic;
using Ensage;
using Ensage.Common.Menu;
using SharpDX;
using Color = SharpDX.Color;
using Font = SharpDX.Direct3D9.Font;

namespace OverlayInformation
{
    internal static class Members
    {
        public static readonly Menu Menu = new Menu("OverlayInformation", "OIv2", true).SetFontColor(Color.LimeGreen);

        public static Hero MyHero { get; set; }
        public static Player MyPlayer { get; set; }
        public static Hero PAisHere { get; set; }
        public static bool BaraIsHere { get; set; }
        public static Hero Mirana { get; set; }
        public static Hero Windrunner { get; set; }
        public static Hero Invoker { get; set; }
        public static Hero Kunkka { get; set; }
        public static Hero Lina { get; set; }
        public static Hero Leshrac { get; set; }
        
        public static List<Hero> Heroes=new List<Hero>();
        public static List<Hero> AllyHeroes=new List<Hero>();
        public static List<Hero> EnemyHeroes = new List<Hero>();
        public static List<Player> Players=new List<Player>();
        public static List<Player> AllyPlayers = new List<Player>();
        public static List<Player> EnemyPlayers = new List<Player>();
        public static List<Unit> BaseList=new List<Unit>();

        public static Dictionary<string, List<Ability>> AbilityDictionary;
        public static Dictionary<string, List<Item>> ItemDictionary;
        public static Dictionary<string, List<Item>> StashItemDictionary;

        public static Font RoshanFont;
        public static float DeathTime;
        public static bool RoshIsAlive;
        public static double RoshanMinutes;
        public static double RoshanSeconds;

        public static readonly Dictionary<Unit, ParticleEffect> Effects2 = new Dictionary<Unit, ParticleEffect>();
        public static readonly Dictionary<Unit, ParticleEffect> Effects1 = new Dictionary<Unit, ParticleEffect>();
        public static readonly Dictionary<string, Vector2> TopPanelPostiion = new Dictionary<string, Vector2>();
        public static readonly List<DrawHelper.StatusInfo> StatInfo = new List<DrawHelper.StatusInfo>();
        public static Dictionary<string, float> PredictionTimes = new Dictionary<string, float>(); 
        public static bool Apparition; 
        public static readonly List<uint> AAlist=new List<uint>();
        public static bool IsClicked;
    }
}