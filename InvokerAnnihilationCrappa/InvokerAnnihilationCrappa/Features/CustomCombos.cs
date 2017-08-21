using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ensage;
using Ensage.Common.Menu;
using Ensage.SDK.Menu;
using InvokerAnnihilationCrappa.Features.behavior;
using log4net;
using PlaySharp.Toolkit.Logging;

namespace InvokerAnnihilationCrappa.Features
{
    public class LittleCustomCombo : IDisposable
    {
        public Config Main { get; }

        public LittleCustomCombo(string name, MenuFactory panel, List<string> list, Config config)
        {
            Main = config;
            var menu = panel.Menu(name);
            ComboEnable = menu.Item($"Enable {name}", false);
            Combo = menu.Item($"{name}.", new PriorityChanger(list));

            if (ComboEnable)
            {
                Update();
            }

            ComboEnable.PropertyChanged += (sender, args) =>
            {
                if (ComboEnable)
                {
                    Update();
                }
                else
                {
                    Dispose();
                }
            };

            Combo.PropertyChanged += (sender, args) =>
            {
                if (!ComboEnable)
                    return;
                Dispose();
                Update();
            };
        }

        public Combo CCombo { get; set; }

        public void Update()
        {
            var infos = new List<AbilityInfo>();
            foreach (var source in Combo.Value.Dictionary.OrderByDescending(x => x.Value))
            {
                var abilityName = source.Key;
                var find = Main.Invoker.AbilityInfos.Find(x => x.Name == abilityName);
                if (find != null)
                {
                    if (find.Ability.Id == AbilityId.invoker_ghost_walk)
                        continue;
                    infos.Add(find);
                }
                else
                {
                    Console.WriteLine($"cant find ability -> {abilityName}");
                }
            }
            CCombo = new Combo(Main.Invoker, infos.ToArray());
            AddToComboList();
        }

        public void AddToComboList()
        {
            if (CCombo!=null && Main.ComboPanel.Combos.Contains(CCombo))
                Main.ComboPanel.Combos.Remove(CCombo);

            Main.ComboPanel.Combos.Add(CCombo);
        }

        public MenuItem<PriorityChanger> Combo { get; set; }

        public MenuItem<bool> ComboEnable { get; set; }

        public void Dispose()
        {
            if (Main.ComboPanel.Combos.Contains(CCombo))
                Main.ComboPanel.Combos.Remove(CCombo);
            CCombo.Dispose();
        }
    }
    public class CustomCombos
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Config _main;
        private readonly List<LittleCustomCombo> _combos;
        public CustomCombos(Config config)
        {
            _main = config;
            _combos = new List<LittleCustomCombo>();
            var panel = config.Factory.Menu("Custom Combos");
            var list =
                _main.Invoker.AbilityInfos.Where(
                        x => !(x.Ability is Item) && x.Ability.Id != AbilityId.invoker_ghost_walk)
                    .Select(ability => ability.Name)
                    .ToList();
            _combos.Add(new LittleCustomCombo("1", panel, list.ToList(), config));
            _combos.Add(new LittleCustomCombo("2", panel,list.ToList(), config));
            _combos.Add(new LittleCustomCombo("3", panel,list.ToList(), config));
            _combos.Add(new LittleCustomCombo("4", panel,list.ToList(), config));
            _combos.Add(new LittleCustomCombo("5", panel,list.ToList(), config));
        }
    }
}