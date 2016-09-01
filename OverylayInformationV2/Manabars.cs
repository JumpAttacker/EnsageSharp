using System;
using Ensage;
using Ensage.Common;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using SharpDX;

namespace OverlayInformation
{
    public class Manabars
    {
        private static bool _loaded;
        private static bool IsEnable => Members.Menu.Item("manaBars.Enable").GetValue<bool>();
        private static float ManaBarSize => (float) Members.Menu.Item("manaBars.Size").GetValue<Slider>().Value/100;
        private static int R => Members.Menu.Item("manaBars.Red").GetValue<Slider>().Value;
        private static int G => Members.Menu.Item("manaBars.Green").GetValue<Slider>().Value;
        private static int B => Members.Menu.Item("manaBars.Blue").GetValue<Slider>().Value;

        public Manabars()
        {
            _loaded = false;
            Events.OnLoad += (sender, args) =>
            {
                if (_loaded)
                {
                    return;
                }
                Load();
                _loaded = true;
            };
            if (!_loaded && ObjectManager.LocalHero != null && Game.IsInGame)
            {
                Load();
                _loaded = true;
            }

            Events.OnClose += (sender, args) =>
            {
                Drawing.OnDraw -= Drawing_OnDraw;
                _loaded = false;
            };
        }

        private static void Load()
        {
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Checker.IsActive())
                return;
            if (!IsEnable)
                return;
            foreach (var v in Manager.HeroManager.GetEnemyViableHeroes())
            {
                try
                {
                    var pos = HUDInfo.GetHPbarPosition(v);
                    if (pos.IsZero)
                        continue;
                    pos += new Vector2(0, HUDInfo.GetHpBarSizeY() - 2);
                    var size = new Vector2(HUDInfo.GetHPBarSizeX(), HUDInfo.GetHpBarSizeY()*ManaBarSize);
                    var manaDelta = new Vector2(v.Mana * size.X / v.MaximumMana, 0);
                    Drawing.DrawRect(pos, size, Color.Black);
                    Drawing.DrawRect(pos, new Vector2(manaDelta.X, size.Y), new Color(R, G, B, 255));
                    Drawing.DrawRect(pos, size, Color.Black, true);
                }
                catch (Exception)
                {
                    Printer.Print($"[Manabars]: {v.StoredName()}");
                }
            }
        }
    }
}
