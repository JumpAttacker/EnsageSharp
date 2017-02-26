using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using log4net;
using PlaySharp.Toolkit.Logging;
using SharpDX;
using Rectangle = Ensage.Common.Objects.RenderObjects.Rectangle;

namespace OverlayInformation
{
    public static class VisionHelper
    {
        private static readonly Dictionary<Player, RectangleStruct> RectDictionary = new Dictionary<Player, RectangleStruct>();
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public struct RectangleStruct
        {
            public readonly Rectangle Rect;
            public readonly DrawingEndScene Draw;

            public RectangleStruct(Rectangle rect, DrawingEndScene draw)
            {
                Rect = rect;
                Draw = draw;
            }
        }

        public static void Flush()
        {
            foreach (var q in RectDictionary.Where(x => x.Value.Rect.IsInitialized))
            {
                q.Value.Rect.Dispose();
                Drawing.OnEndScene -= q.Value.Draw;
            }
        }

        public static void OnChange(Entity sender, Int32PropertyChangeEventArgs args)
        {
            var hero = sender as Hero;
            if (hero == null)
                return;
            if (hero.Team != Members.MyHero.Team || hero.IsIllusion())
                return;
            if (args.PropertyName != "m_iTaggedAsVisibleByTeam")
                return;
            DelayAction.Add(50, () =>
            {
                var visible = args.NewValue == 0x1E;
                var player = hero.Player;
                if (player==null)
                    return;
                RectangleStruct st;
                if (!RectDictionary.TryGetValue(player, out st))
                {
                    var newRect =
                        new Rectangle(
                            new Vector2((float) HudInfoNew.GetTopPanelSizeX(hero), (float) HUDInfo.GetTopPanelSizeY(hero)),
                            Clr) {Position = HudInfoNew.GetTopPanelPosition(hero)};
                    st = new RectangleStruct(newRect, eventArgs => newRect.Render());
                    RectDictionary.Add(player, st);
                    //Log.Info($"Init new player {player.Name}({hero.GetRealName()})");
                }
                var rect = st.Rect;
                var draw = st.Draw;
                if (visible)
                {
                    if (IsEnable)
                    {
                        if (!rect.IsInitialized)
                        {
                            rect.Initialize();
                            rect.Color = Clr; //new ColorBGRA(0,155,255,10);
                            Drawing.OnEndScene += draw;
                        }
                    }
                }
                else
                {
                    if (rect.IsInitialized)
                    {
                        rect.Dispose();
                        Drawing.OnEndScene -= draw;
                    }
                }
            });
        }
        public static ColorBGRA Clr
            =>
                new ColorBGRA(
                    (byte)Members.Menu.Item("AllyVision.Red").GetValue<Slider>().Value,
                    (byte)Members.Menu.Item("AllyVision.Green").GetValue<Slider>().Value,
                    (byte)Members.Menu.Item("AllyVision.Blue").GetValue<Slider>().Value,
                    (byte)Members.Menu.Item("AllyVision.Alpha").GetValue<Slider>().Value);

        public static bool IsEnable
            => Members.Menu.Item("toppanel.AllyVision.Type").GetValue<StringList>().SelectedIndex == 0;

    }
}