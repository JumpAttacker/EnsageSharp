using System.Collections.Generic;
using ArcAnnihilation.Units;
using ArcAnnihilation.Utils;
using Ensage;

namespace ArcAnnihilation.Manager
{
    public class NecronomiconManager
    {
        private static NecronomiconManager _necronomiconManager;

        public NecronomiconManager()
        {
            GetNecronomicons = new List<Necronomicon>();
            GetRangeNecronomicons = new List<RangeNecr>();
            ObjectManager.OnAddEntity += args =>
            {
                var hero = args.Entity as Unit;
                if (hero == null || !hero.IsValid || hero.Team != Core.MainHero.Hero.Team || !hero.IsControllable) return;

                if (hero.Name.Contains("npc_dota_necronomicon_warrior") ||
                    hero.Name.Contains("npc_dota_necronomicon_archer"))
                {
                    Necronomicon necr;
                    if (hero.Name.Contains("npc_dota_necronomicon_archer"))
                    {
                        necr = new RangeNecr(hero);
                        GetRangeNecronomicons.Add((RangeNecr) necr);
                        Printer.Both("added [Range] Necronomicon: " + hero.Name);
                    }
                    else
                    {
                        necr = new MeleeNecr(hero);
                        Printer.Both("added [Melee] Necronomicon: " + hero.Name);
                    }
                    necr.Init();
                    GetNecronomicons.Add(necr);
                }
            };

            Entity.OnInt32PropertyChange += (sender, args) =>
            {
                var hero = sender as Unit;
                if (hero == null) return;

                if (hero.Name.Contains("npc_dota_necronomicon_warrior") ||
                    hero.Name.Contains("npc_dota_necronomicon_archer"))
                {
                    if (args.PropertyName != "m_iHealth") return;

                    if (args.NewValue == 0)
                    {
                        var target = GetNecronomicons.Find(x => x.Necr.Equals(hero));
                        if (target != null)
                        {
                            target.Orbwalker.Unload();
                            GetNecronomicons.Remove(target);
                            Printer.Both("removed necro: " + hero.Name);
                        }
                    }
                }
            };
        }

        public static NecronomiconManager GetNecronomiconManager()
        {
            return _necronomiconManager ?? (_necronomiconManager = new NecronomiconManager());
        }

        public static List<Necronomicon> GetNecronomicons { get; private set; }

        public static List<RangeNecr> GetRangeNecronomicons { get; private set; }
    }
}