using System.Collections.Generic;
using ArcAnnihilation.Units;
using ArcAnnihilation.Utils;
using Ensage;
using Ensage.Common;
using Ensage.SDK.Extensions;

namespace ArcAnnihilation.Manager
{
    public class IllusionManager
    {
        private static IllusionManager _illusionManager;

        public IllusionManager()
        {
            GetIllusions = new List<Illusion>();

            ObjectManager.OnAddEntity += args =>
            {
                DelayAction.Add(150, () =>
                {
                    var hero = args.Entity as Hero;
                    if (hero == null || !hero.IsIllusion || hero.Team != Core.MainHero.Hero.Team || !hero.IsControllable ||
                        hero.HasModifier("modifier_kill")) return;

                    Printer.Both("added illusion: " + hero.Name);
                    var ill = new Illusion(hero);
                    ill.Init();
                    GetIllusions.Add(ill);
                });
            };

            Entity.OnInt32PropertyChange += (sender, args) =>
            {
                var hero = sender as Hero;
                if (hero == null || !hero.IsIllusion) return;

                if (args.PropertyName != "m_iHealth") return;

                

                if (args.NewValue == 0)
                {
                    var target = GetIllusions.Find(x => x.Hero.Equals(hero));
                    if (target != null)
                    {
                        target.Orbwalker.Unload();
                        GetIllusions.Remove(target);
                        Printer.Both("removed illusion: " + hero.Name);
                    }
                }
            };
        }

        public static IllusionManager GetCreepManager()
        {
            return _illusionManager ?? (_illusionManager = new IllusionManager());
        }

        public static List<Illusion> GetIllusions { get; private set; }
    }
}