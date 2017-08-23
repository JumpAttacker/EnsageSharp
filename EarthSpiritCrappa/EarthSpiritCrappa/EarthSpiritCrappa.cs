using System;
using System.ComponentModel.Composition;
using System.Reflection;
using Ensage.SDK.Service;
using Ensage.SDK.Service.Metadata;
using Ensage.SDK.Extensions;
using System.Windows.Input;
using EarthSpiritCrappa.LittleCombos;
using Ensage.Common.Menu;

using log4net;

using PlaySharp.Toolkit.Logging;
using Ensage;
using Ensage.SDK.Abilities.Items;
using Ensage.SDK.Inventory.Metadata;


namespace EarthSpiritCrappa
{
    [ExportPlugin(
        mode: StartupMode.Auto,
        name: "Earth Spirit Crappahilation",
        author: "JumpAttacker",
        units: new[] { HeroId.npc_dota_hero_earth_spirit })]
    public sealed class EarthSpiritCrappa : Plugin
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [ImportingConstructor]
        public EarthSpiritCrappa([Import] IServiceContext context)
        {
            Context = context;
        }

        public Hero Owner;

        public IServiceContext Context { get; }

        protected override void OnActivate()
        {
            Owner = (Hero) Context.Owner;
            Log.Info($"Owner -> {Owner.GetDisplayName()}");
            Config = new Config(this);
            Log.Info($"Load config");
            Context.TargetSelector.Activate();
            Log.Info($"TargetSelector activaed");
            Context.Orbwalker.Activate();
            Log.Info($"Orbwalker activaed");
            var keyId = KeyInterop.KeyFromVirtualKey((int) Config.ComboKey.Item.GetValue<KeyBind>().Key);
            Mode = new EarthSpiritMode(Context, keyId, this);
            Log.Info($"Combo Key -> {Mode.Key}");
            Mode.Activate();
            Log.Info($"Mode activaed");
            Context.Orbwalker.RegisterMode(Mode);
            Log.Info($"Mode Registered");
            Config.ComboKey.Item.ValueChanged += (sender, e) =>
            {
                var keyCode = e.GetNewValue<KeyBind>().Key;
                if (keyCode == e.GetOldValue<KeyBind>().Key)
                {
                    return;
                }
                var key = KeyInterop.KeyFromVirtualKey((int)keyCode);
                Mode.Key = key;
                Log.Info("new hotkey: " + key);
            };

            Context.Inventory.Attach(this);
            Log.Info($"Inventory Attach");

            Smash = Owner.GetAbilityById(AbilityId.earth_spirit_boulder_smash);
            Boulder = Owner.GetAbilityById(AbilityId.earth_spirit_rolling_boulder);
            Grip = Owner.GetAbilityById(AbilityId.earth_spirit_geomagnetic_grip);
            StoneCaller = Owner.GetAbilityById(AbilityId.earth_spirit_stone_caller);
            Magnetize = Owner.GetAbilityById(AbilityId.earth_spirit_magnetize);
            EnchantRemnant = Owner.GetAbilityById(AbilityId.earth_spirit_petrify);
            Log.Info($"Getting abilities");

            StoneManager = new StoneManager(this);
            Log.Info($"init StoneManager");

            PushCombo = new PushCombo(this);
            RollCombo = new RollingCombo(this);
            GripCombo = new PullCombo(this);
            EnchantCombo = new EnchantCombo(this);

            AbilityRange = new AbilityRanger(this);

        }

        public EnchantCombo EnchantCombo { get; set; }

        public AbilityRanger AbilityRange { get; set; }

        public PullCombo GripCombo { get; set; }

        public RollingCombo RollCombo { get; set; }

        public PushCombo PushCombo { get; set; }

        public StoneManager StoneManager { get; set; }
        public Ability Smash { get; set; }
        public Ability Boulder { get; set; }
        public Ability Grip { get; set; }
        public Ability StoneCaller { get; set; }
        public Ability Magnetize { get; set; }
        public Ability EnchantRemnant { get; set; }
        public EarthSpiritMode Mode { get; set; }
        public Config Config { get; set; }

        [ItemBinding]
        public item_cyclone Eul { get; set; }
        [ItemBinding]
        public item_blink Blink { get; set; }
        [ItemBinding]
        public item_sheepstick Hex { get; set; }
        [ItemBinding]
        public item_veil_of_discord Veil { get; set; }
        [ItemBinding]
        public item_heavens_halberd Halbert { get; set; }
        [ItemBinding]
        public item_lotus_orb Lotus { get; set; }
        [ItemBinding]
        public item_shivas_guard Shivas { get; set; }

        protected override void OnDeactivate()
        {
            Context.Inventory.Detach(this);
            Context.Inventory.Deactivate();
            AbilityRange.Dispose();
            Mode.Deactivate();
            Config?.Dispose();
        }
    }
}
