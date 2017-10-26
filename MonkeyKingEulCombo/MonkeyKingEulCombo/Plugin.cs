using System.ComponentModel.Composition;
using System.Reflection;
using System.Windows.Input;
using Ensage.SDK.Service;
using Ensage.SDK.Service.Metadata;
using Ensage.Common.Menu;

using log4net;

using PlaySharp.Toolkit.Logging;
using Ensage;
using Ensage.SDK.Abilities.Items;
using Ensage.SDK.Inventory.Metadata;

namespace MonkeyKingEulCombo
{
    [ExportPlugin(
        mode: StartupMode.Auto,
        name: "MonkeyKing Eul Combo",
        version: "1.0.0.0",
        author: "JumpAttacker",
        description: "Eul->Ult->Stun",
        units: new[] {  HeroId.npc_dota_hero_monkey_king })]
    public sealed class MonkeyKingEulCombo : Plugin
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [ImportingConstructor]
        public MonkeyKingEulCombo([Import] IServiceContext context)
        {
            Context = context;
        }

        public Config Config;
        public IServiceContext Context { get; }
        public ComboWombo Mode;
        protected override void OnActivate()
        {
            Config = new Config();
            Mode = new ComboWombo(Context, Config.Key, this);
            Mode.UpdateConfig(Config);
            Mode.Load();
            OrbWalkWasActive = Context.Orbwalker.IsActive;
            if (!OrbWalkWasActive)
                Context.Orbwalker.Activate();
            TargetSelectorWasActive = Context.TargetSelector.IsActive;
            if (!TargetSelectorWasActive)
                Context.TargetSelector.Activate();
            Config.Key.Item.ValueChanged += HotkeyChanged;
            Context.Orbwalker.RegisterMode(Mode);
            var key = KeyInterop.KeyFromVirtualKey((int)Config.Key.Item.GetValue<KeyBind>().Key);
            Mode.Key = key;
            Context.Inventory.Attach(this);
            Log.Debug("loaded.");
        }

        public bool TargetSelectorWasActive { get; set; }

        public bool OrbWalkWasActive { get; set; }

        private void HotkeyChanged(object sender, OnValueChangeEventArgs e)
        {
            var keyCode = e.GetNewValue<KeyBind>().Key;
            if (keyCode == e.GetOldValue<KeyBind>().Key)
            {
                return;
            }
            var key = KeyInterop.KeyFromVirtualKey((int)keyCode);
            Mode.Key = key;
            Log.Debug($"new key -> {key}");
        }

        [ItemBinding]
        public item_cyclone Eul { get; set; }

        protected override void OnDeactivate()
        {
            if (!OrbWalkWasActive)
                Context.Orbwalker.Deactivate();
            if (!TargetSelectorWasActive)
                Context.TargetSelector.Deactivate();
            Mode.Unload();
            Mode.Dispose();
            Config?.Dispose();
            Log.Debug("unloaded.");
        }
    }
}
