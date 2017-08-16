using System;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Windows.Input;
using Ensage;
using Ensage.SDK.Input;
using Ensage.SDK.Inventory;
using Ensage.SDK.Renderer;
using Ensage.SDK.Renderer.DX11;
using Ensage.SDK.Renderer.DX9;
using Ensage.SDK.Renderer.Particle;
using Ensage.SDK.Service;
using Ensage.SDK.Service.Metadata;
using log4net;
using PlaySharp.Toolkit.Logging;

namespace OverlayInformation
{
    [ExportPlugin("OverlayInformation", author:"JumpAttacker")]
    public class OverlayInformation : Plugin
    {
        public Lazy<IServiceContext> Context { get; set; }
        public ID3D11Context D11Context { get; }
        public ID3D9Context D9Context { get; set; }
        public BrushCache BrushCache { get; }
        public IInventoryManager InventoryManager { get; set; }
        public IRendererManager Renderer { get; set; }
        public IParticleManager ParticleManager { get; set; }
        public Updater Updater;
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public Hero Owner;

        [ImportingConstructor]
        public OverlayInformation([Import] Lazy<IServiceContext> context, [Import] Lazy<ID3D11Context> d11Context,
            [Import] Lazy<BrushCache> brushCache, [Import] Lazy<ID3D9Context> d9Context)
        {
            Context = context;
            if (Drawing.RenderMode == RenderMode.Dx11)
            {
                D11Context = d11Context.Value;
                BrushCache = brushCache.Value;
            }
            else
            {
                D9Context = d9Context.Value;
            }
        }

        public Config Config;
        protected override void OnActivate()
        {
            InventoryManager = Context.Value.Inventory;
            Renderer = Context.Value.Renderer;
            ParticleManager = Context.Value.Particle;
            Owner = (Hero) Context.Value.Owner;
            Updater = new Updater(this);
            Log.Info("Updater loaded");
            Config = new Config(this);
            Log.Info("Config loaded");
            /*InventoryManager.Value.Activate();
            Log.Info("InventoryManager loaded");*/

            InitLocalCheats();
        }

        private void InitLocalCheats()
        {
            Config.DebugMessages = Config.Factory.Item("Debug Messages", false);
            if (Config.DebugMessages)
            {
                Context.Value.Input.RegisterHotkey("tap", Key.Tab, RefreshKey);
            }

            Config.DebugMessages.Item.ValueChanged += (sender, args) =>
            {
                if (args.GetNewValue<bool>())
                {
                    Context.Value.Input.RegisterHotkey("tap", Key.Tab, RefreshKey);
                }
                else
                {
                    Context.Value.Input.UnregisterHotkey("tap");
                }
            };
        }

        private void RefreshKey(KeyEventArgs keyEventArgs)
        {
            Game.ExecuteCommand("dota_hero_refresh");
        }

        protected override void OnDeactivate()
        {
            if (Config.DebugMessages)
            {
                Context.Value.Input.UnregisterHotkey("tap");
            }
            //InventoryManager.Value.Deactivate();

            Config?.Dispose();

            Updater.OnDeactivate();
        }
    }
}