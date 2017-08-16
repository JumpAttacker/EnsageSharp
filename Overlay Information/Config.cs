using System;
using Ensage.SDK.Menu;
using OverlayInformation.Features;
using OverlayInformation.Features.Open_Dota;
using OverlayInformation.Features.Teleport_Catcher;
using SharpDX;

namespace OverlayInformation
{
    public class Config : IDisposable
    {
        public Config(OverlayInformation overlayInformation)
        {
            Factory = MenuFactory.Create("Overlay Information");
            Factory.Target.SetFontColor(Color.YellowGreen);

            Main = overlayInformation;
            TopPanel = new TopPanel(this);
            HeroOverlay = new HeroOverlay(this);
            CourEsp = new CourEsp(this);
            ItemPanel = new ItemPanel(this);
            NetworthPanel = new NetworthPanel(this);
            ShrineHelper = new ShrineHelper(this);
            TpCatcher = new TpCatcher(this);
            LastPositionTracker = new LastPositionTracker(this);
            OpenDotaHelper = new OpenDotaHelper(this);
            ShowMeMore = new ShowMeMore(this);
            RoshanTimer = new RoshanTimer(this);
        }

        public CourEsp CourEsp { get; set; }

        public RoshanTimer RoshanTimer { get; set; }

        public ShowMeMore ShowMeMore { get; set; }

        public OpenDotaHelper OpenDotaHelper { get; set; }

        public LastPositionTracker LastPositionTracker { get; set; }

        public TpCatcher TpCatcher { get; set; }

        public ShrineHelper ShrineHelper { get; set; }

        public NetworthPanel NetworthPanel { get; set; }

        public ItemPanel ItemPanel { get; set; }

        public HeroOverlay HeroOverlay { get; set; }

        public TopPanel TopPanel { get; set; }

        public OverlayInformation Main;
        public MenuItem<bool> DebugMessages { get; set; }
        public MenuFactory Factory { get; }

        public void Dispose()
        {
            TopPanel.OnDeactivate();
            HeroOverlay.OnDeactivate();
            ItemPanel.OnDeactivate();
            NetworthPanel.OnDeactivate();
            ShrineHelper.OnDeactivate();
            TpCatcher.OnDeactivate();
            LastPositionTracker.OnDeactivate();
            OpenDotaHelper.OnDeactivate();
            ShowMeMore.Dispose();
            RoshanTimer.Dispose();
            Factory?.Dispose();
        }
    }
}