using System.Collections.Generic;
using Ensage;
using Ensage.Abilities;
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
        public static Hero LifeStealer { get; set; }
        public static Unit ScanEnemy { get; set; }
        public static Hero Tinker { get; set; }
        public static Hero Techies { get; set; }
        public static Hero ArcWarden { get; set; }
        public static Hero Meepo { get; set; }
        public static DividedWeStand MeepoDivided { get; set; }
        public static ClassId MyClass { get; set; }
        

        public static List<Hero> Heroes=new List<Hero>();
        public static List<Hero> AllyHeroes=new List<Hero>();
        public static List<Hero> EnemyHeroes = new List<Hero>();
        public static List<Hero> MeepoIgnoreList = new List<Hero>();
        public static List<Player> Players=new List<Player>();
        public static List<Player> AllyPlayers = new List<Player>();
        public static List<Player> EnemyPlayers = new List<Player>();
        public static List<Unit> BaseList=new List<Unit>();

        public static Dictionary<uint, List<Ability>> AbilityDictionary;
        public static Dictionary<uint, List<Item>> ItemDictionary;
        public static Dictionary<uint, List<Item>> StashItemDictionary;
        public static Dictionary<string, long> NetWorthDictionary;

        public static Font RoshanFont;
        public static float DeathTime;
        public static bool RoshIsAlive;
        public static double RoshanMinutes;
        public static double RoshanSeconds;
        public static float AegisTime { get; set; }
        public static double AegisMinutes { get; set; }
        public static double AegisSeconds { get; set; }
        public static bool AegisEvent { get; set; }
        public static Item Aegis { get; set; }
        public static bool AegisWasFound { get; set; }

        public static readonly Dictionary<Unit, ParticleEffect> Effects2 = new Dictionary<Unit, ParticleEffect>();
        public static readonly Dictionary<Unit, ParticleEffect> Effects1 = new Dictionary<Unit, ParticleEffect>();
        public static readonly Dictionary<string, Vector2> TopPanelPostiion = new Dictionary<string, Vector2>();
        public static readonly List<DrawHelper.StatusInfo> StatInfo = new List<DrawHelper.StatusInfo>();
        public static Dictionary<string, float> PredictionTimes = new Dictionary<string, float>(); 
        public static bool Apparition; 
        public static readonly List<uint> AAlist=new List<uint>();
        public static bool IsClicked;

        public static Manabars Manabars;
        public static ItemOverlay ItemOverlay;
        public static HeroesList HeroesList;
        public static DamageCalculation DamageCalculation;
        public static AbilityOverlay AbilityOverlay;
        public static List<Courier> CourList;

        public static List<string> ShowIllusionList = new List<string>()
        {
            "materials/ensage_ui/particles/smoke_illusions_mod.vpcf",
            "materials/ensage_ui/particles/illusions_mod.vpcf",
            "materials/ensage_ui/particles/illusions_mod_v2.vpcf",
            "materials/ensage_ui/particles/illusions_mod_balloons.vpcf",
            "particles/items2_fx/shadow_amulet_active_ground_proj.vpcf"
        };
    }
}
