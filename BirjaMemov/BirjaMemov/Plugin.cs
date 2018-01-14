using System.ComponentModel.Composition;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;
using Ensage.SDK.Service;
using Ensage.SDK.Service.Metadata;
using Ensage.SDK.Extensions;
using Ensage.SDK.Helpers;
using log4net;

using PlaySharp.Toolkit.Logging;
using Ensage;
using Ensage.Common.Extensions;

namespace BirjaMemov
{
    [ExportPlugin(
        mode: StartupMode.Auto,
        name: "BirjaMemov",
        version: "1.0.0.0",
        author: "Ensage",
        description: "",
        units: new[] {  HeroId.npc_dota_hero_axe })]
    public sealed class BirjaMemov : Plugin
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [ImportingConstructor]
        public BirjaMemov([Import] IServiceContext context)
        {
            Context = context;
        }

        public IServiceContext Context { get; }
        public Ability KillStealAbility;
        protected override void OnActivate()
        {
            var spells = (Context.Owner as Hero).Spellbook.Spells;
            foreach (var spell in spells)
            {
                if (spell.Name == "culling_blade_datadriven")
                {
                    KillStealAbility = spell;
                    UpdateManager.BeginInvoke(KillSteal);
                    break;
                }
            }
        }

        private async void KillSteal()
        {
            while (Game.GameMode == GameMode.Demo)
            {
                var damage = KillStealAbility.GetAbilitySpecialData("kill_threshold", KillStealAbility.Level);
                var enemy = EntityManager<Hero>.Entities.FirstOrDefault(x =>
                    x.IsAlive && !x.IsAlly(Context.Owner) && x.IsVisible && !x.IsIllusion && x.Health + x.HealthRegeneration < damage &&
                    x.IsInRange(Context.Owner, 200));
                if (enemy != null && KillStealAbility.CanBeCasted())
                {
                    KillStealAbility.UseAbility(enemy);
                    await Task.Delay((int) (350 + Game.Ping));
                }
                await Task.Delay(100);
            }
        }

        protected override void OnDeactivate()
        {
        }
    }
}
