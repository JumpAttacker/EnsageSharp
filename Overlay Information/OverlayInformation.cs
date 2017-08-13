using System;
using System.ComponentModel.Composition;
using System.Reflection;
using Ensage;
using Ensage.SDK.Inventory;
using Ensage.SDK.Renderer;
using Ensage.SDK.Renderer.DX11;
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
        public BrushCache BrushCache { get; }
        public IInventoryManager InventoryManager { get; set; }
        public IRendererManager Renderer { get; set; }
        public IParticleManager ParticleManager { get; set; }
        public Updater Updater;
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public Hero Owner;

        [ImportingConstructor]
        public OverlayInformation([Import] Lazy<IServiceContext> context, [Import] ID3D11Context d11Context,
            [Import] BrushCache brushCache)
        {
            Context = context;
            D11Context = d11Context;
            BrushCache = brushCache;
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
        }


        protected override void OnDeactivate()
        {
            //InventoryManager.Value.Deactivate();
            
            Config?.Dispose();

            Updater.OnDeactivate();
        }
    }
}