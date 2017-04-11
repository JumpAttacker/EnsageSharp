using System;
using System.Collections.Generic;
using Ensage;
using Ensage.Common;
using SharpDX;

namespace OverlayInformation
{
    public static class HudInfoNew
    {
        #region Static Fields

        /// <summary>
        ///     The dire compare.
        /// </summary>
        public static double DireCompare;

        /// <summary>
        ///     The health bar height.
        /// </summary>
        private static readonly double HpBarHeight;

        /// <summary>
        ///     The health bar width.
        /// </summary>
        private static readonly double HpBarWidth;

        /// <summary>
        ///     The health bar x.
        /// </summary>
        private static readonly double HpBarX;

        /// <summary>
        ///     The health bar y.
        /// </summary>
        private static readonly float HpBarY;

        /// <summary>
        ///     The monitor.
        /// </summary>
        internal static readonly float Monitor;

        /// <summary>
        ///     The player id dictionary.
        /// </summary>
        private static readonly Dictionary<float, int> PlayerIdDictionary = new Dictionary<float, int>();

        /// <summary>
        ///     The radiant compare.
        /// </summary>
        public static double RadiantCompare;

        /// <summary>
        ///     The rate.
        /// </summary>
        private static readonly float Rate;

        /// <summary>
        ///     The screen size.
        /// </summary>
        private static readonly Vector2 ScreenSize;

        /// <summary>
        ///     The x.
        /// </summary>
        internal static double X;


        /// <summary>
        ///     The y.
        /// </summary>
        private static double y;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes static members of the <see cref="HUDInfo" /> class.
        /// </summary>
        static HudInfoNew()
        {
            double tinfoHeroDown;
            double panelHeroSizeX;
            float compareWidth;
            ScreenSize = new Vector2(Drawing.Width, Drawing.Height);
            if (ScreenSize.X == 0)
            {
                Console.WriteLine("Ensage couldnt determine your resolution, try to launch in window mode");
                return;
            }

            var ratio = Math.Floor((decimal)(ScreenSize.X / ScreenSize.Y * 100));
            if (ratio == 213)
            {
                compareWidth = 1600;
                panelHeroSizeX = 45.28;
                tinfoHeroDown = 25.714;
                DireCompare = 2.402;
                RadiantCompare = 3.08;
                HpBarHeight = 7;
                HpBarWidth = 69;
                HpBarX = 36;
                HpBarY = 23;
            }
            else if (ratio == 177)
            {
                compareWidth = 1600;
                panelHeroSizeX = 52.8900000000004;
                tinfoHeroDown = 25.714;
                DireCompare = 2.5001;
                RadiantCompare = 3.409;
                HpBarHeight = 10;
                HpBarWidth = 84;
                HpBarX = 43;
                HpBarY = 27;
            }
            else if (ratio == 166)
            {
                compareWidth = 1280;
                panelHeroSizeX = 47.19;
                tinfoHeroDown = 25.714;
                DireCompare = 2.59;
                RadiantCompare = 3.64;
                HpBarHeight = 7.4;
                HpBarWidth = 71;
                HpBarX = 37;
                HpBarY = 22;
            }
            else if (ratio == 160)
            {
                compareWidth = 1280;
                panelHeroSizeX = 48.95;
                tinfoHeroDown = 25.714;
                DireCompare = 2.609;
                RadiantCompare = 3.78;
                HpBarHeight = 9;
                HpBarWidth = 75;
                HpBarX = 38.3;
                HpBarY = 25;
            }
            else if (ratio == 150)
            {
                compareWidth = 1280;
                panelHeroSizeX = 51.39;
                tinfoHeroDown = 25.714;
                DireCompare = 2.64;
                RadiantCompare = 4.02;
                HpBarHeight = 8;
                HpBarWidth = 79.2;
                HpBarX = 40.2;
                HpBarY = 24;
            }
            else if (ratio == 133)
            {
                compareWidth = 1024;
                panelHeroSizeX = 47.21;
                tinfoHeroDown = 25.714;
                DireCompare = 2.775;
                RadiantCompare = 4.57;
                HpBarHeight = 8;
                HpBarWidth = 71;
                HpBarX = 36.6;
                HpBarY = 23;
            }
            else if (ratio == 125)
            {
                compareWidth = 1280;
                panelHeroSizeX = 58.3;
                tinfoHeroDown = 25.714;
                DireCompare = 2.78;
                RadiantCompare = 4.65;
                HpBarHeight = 11;
                HpBarWidth = 96.5;
                HpBarX = 49;
                HpBarY = 32;
            }
            else
            {
                Console.WriteLine(
                    @"Your screen resolution is not supported and drawings might have wrong size/position, (" + ratio
                    + ")");
                compareWidth = 1600;
                panelHeroSizeX = 65;
                tinfoHeroDown = 25.714;
                DireCompare = 2.655;
                RadiantCompare = 5.985;
                HpBarHeight = 10;
                HpBarWidth = 83.5;
                HpBarX = 43;
                HpBarY = 28;
            }

            Monitor = ScreenSize.X / compareWidth;
            Rate = Math.Max(Monitor, 1);
            X = panelHeroSizeX * Monitor;
            y = ScreenSize.Y / tinfoHeroDown;
            //Drawing.OnDraw += Drawing_OnDraw;
            //var mipos = new Vector3(MapLeft, MapTop, 0).WorldToMinimap();
            //var minimap = new Render.Rectangle(
            //    mipos.X, 
            //    mipos.Y, 
            //    currentMinimap.Size.X, 
            //    currentMinimap.Size.Y, 
            //    new ColorBGRA(100, 100, 100, 50));
            //minimap.Add();
            /*_line = new Line(Drawing.Direct3DDevice9);

            Drawing.OnEndScene += args =>
            {
                if (Drawing.Direct3DDevice9 == null)
                {
                    return;
                }
                var pos = currentMinimap.Position+new Vector2(0,800);
                var size = currentMinimap.Size;
                DrawLine(
                    pos.X, 
                    pos.Y,
                    pos.X + size.X, 
                    pos.Y + size.Y,
                    2, Color.YellowGreen);


            };
            Drawing.OnPostReset += args =>
            {
                _line.OnResetDevice();
            };
            Drawing.OnPreReset += args =>
            {
                _line.OnLostDevice();
            };*/
        }
        /*public static void DrawLine(float x1, float y1, float x2, float y2, float w, Color color)
        {
            var vLine = new[] { new Vector2(x1, y1), new Vector2(x2, y2) };

            _line.GLLines = true;
            _line.Antialias = false;
            _line.Width = w;

            _line.Begin();
            _line.Draw(vLine, color);
            _line.End();

        }*/

        //private static Line _line;
        #endregion



        #region Public Methods and Operators

        /// <summary>
        ///     Returns HealthBar position for given unit
        /// </summary>
        /// <param name="unit">
        ///     The unit.
        /// </param>
        /// <returns>
        ///     The <see cref="Vector2" />.
        /// </returns>
        public static Vector2 GetHPbarPosition(Unit unit)
        {
            var pos = unit.Position + new Vector3(0, 0, unit.HealthBarOffset);
            Vector2 screenPos;
            if (!Drawing.WorldToScreen(pos, out screenPos))
            {
                return Vector2.Zero;
            }

            var localHero = ObjectManager.LocalHero;
            if (localHero != null && Equals(unit, localHero))
            {
                if (unit.ClassId == ClassId.CDOTA_Unit_Hero_Meepo)
                {
                    return screenPos + new Vector2((float)(-HpBarX * 1.05 * Monitor), (float)(-HpBarY * 1.3 * Monitor));
                }

                return screenPos + new Vector2((float)(-HpBarX * 1.05 * Monitor), (float)(-HpBarY * 1.38 * Monitor));
            }

            return screenPos + new Vector2((float)(-HpBarX * Monitor), -HpBarY * Monitor);
        }

        /// <summary>
        ///     Returns HealthBar X position for given unit
        /// </summary>
        /// <param name="unit">
        ///     The unit.
        /// </param>
        /// <returns>
        ///     The <see cref="float" />.
        /// </returns>
        public static float GetHPBarSizeX(Unit unit = null)
        {
            var hero = ObjectManager.LocalHero;
            if (unit != null && hero != null && Equals(unit, hero))
            {
                return (float)((float)HpBarWidth * Monitor * 1.1);
            }

            return (float)HpBarWidth * Monitor;
        }

        /// <summary>
        ///     Returns HealthBar Y position for given unit
        /// </summary>
        /// <param name="unit">
        ///     The unit.
        /// </param>
        /// <returns>
        ///     The <see cref="float" />.
        /// </returns>
        public static float GetHpBarSizeY(Unit unit = null)
        {
            var hero = ObjectManager.LocalHero;
            if (unit != null && hero != null && Equals(unit, hero))
            {
                return (float)(HpBarHeight * Monitor * 1.05);
            }

            return (float)(HpBarHeight * Monitor);
        }

        /// <summary>
        ///     Returns top panel position for given hero
        /// </summary>
        /// <param name="hero">
        ///     The hero.
        /// </param>
        /// <returns>
        ///     The <see cref="Vector2" />.
        /// </returns>
        public static Vector2 GetTopPanelPosition(Hero hero)
        {
            int id;
            if (hero.Player == null)
            {
                if (PlayerIdDictionary.ContainsKey(hero.Handle))
                {
                    id = PlayerIdDictionary[hero.Handle];
                }
                else
                {
                    return Vector2.Zero;
                }
            }
            else
            {
                id = hero.Player.Id;
            }

            if (!PlayerIdDictionary.ContainsKey(hero.Handle))
            {
                PlayerIdDictionary.Add(hero.Handle, id);
            }
            else
            {
                PlayerIdDictionary[hero.Handle] = id;
            }

            return new Vector2((float)(GetXX(hero) - 20 * Monitor + X * id), 0);
        }
        public static Vector2 GetFakeTopPanelPosition(int id, Team team)
        {
            return new Vector2((float)(GetFakeXX(team) - 20 * Monitor + X * id), 0);
        }

        /// <summary>
        ///     Returns top panel size
        /// </summary>
        /// <param name="hero">
        ///     The hero.
        /// </param>
        /// <returns>
        ///     The <see cref="double[]" />.
        /// </returns>
        public static Vector2 GetTopPanelSize(Hero hero=null)
        {
            var size = new Vector2((float) GetTopPanelSizeX(hero), (float) GetTopPanelSizeY(hero));
            return size;
        }

        /// <summary>
        ///     Returns top panel hero icon width
        /// </summary>
        /// <param name="hero">
        ///     The hero.
        /// </param>
        /// <returns>
        ///     The <see cref="double" />.
        /// </returns>
        public static double GetTopPanelSizeX(Hero hero=null)
        {
            return X;
        }

        /// <summary>
        ///     Returns top panel hero icon height
        /// </summary>
        /// <param name="hero">
        ///     The hero.
        /// </param>
        /// <returns>
        ///     The <see cref="double" />.
        /// </returns>
        public static double GetTopPanelSizeY(Hero hero=null)
        {
            return 35 * Rate;
        }

        /// <summary>
        ///     The ratio percentage.
        /// </summary>
        /// <returns>
        ///     The <see cref="float" />.
        /// </returns>
        public static float RatioPercentage()
        {
            return Monitor;
        }

        /// <summary>
        ///     Returns screen width
        /// </summary>
        /// <returns>
        ///     The <see cref="float" />.
        /// </returns>
        public static float ScreenSizeX()
        {
            return ScreenSize.X;
        }

        /// <summary>
        ///     Returns screen height
        /// </summary>
        /// <returns>
        ///     The <see cref="float" />.
        /// </returns>
        public static float ScreenSizeY()
        {
            return ScreenSize.Y;
        }

        #endregion

        #region Methods

        private static void Drawing_OnDraw(EventArgs args)
        {
            Drawing.DrawRect(GetTopPanelPosition(Members.MyHero), GetTopPanelSize(Members.MyHero), Color.White);
            var v = ObjectManager.LocalHero;
            /*for (int i = 0; i < 5; i++)
            {
                Drawing.DrawRect(GetFakeTopPanelPosition(i, Team.Radiant), GetTopPanelSize(v), Color.White);
            }*/
            /*for (int i = 5; i < 10; i++)
            {
                Drawing.DrawRect(GetFakeTopPanelPosition(i, Team.Dire), GetTopPanelSize(v), Color.White);
            }*/

            Drawing.DrawRect(GetFakeTopPanelPosition(0, Team.Radiant), GetTopPanelSize(v), Color.White);

            Drawing.DrawRect(GetFakeTopPanelPosition(9, Team.Dire), GetTopPanelSize(v), Color.White);

            Drawing.DrawRect(GetFakeTopPanelPosition(7, Team.Dire), GetTopPanelSize(v), Color.White);

            Drawing.DrawRect(GetFakeTopPanelPosition(5, Team.Dire), GetTopPanelSize(v), Color.White);

        }

        /// <summary>
        ///     The get xx.
        /// </summary>
        /// <param name="hero">
        ///     The hero.
        /// </param>
        /// <returns>
        ///     The <see cref="double" />.
        /// </returns>
        private static double GetXX(Entity hero)
        {
            var screenSize = new Vector2(Drawing.Width, Drawing.Height);
            if (hero.Team == Team.Radiant)
            {
                return screenSize.X / RadiantCompare + 1;
            }

            return screenSize.X / DireCompare + 1;
        }
        private static double GetFakeXX(Team team)
        {
            var screenSize = new Vector2(Drawing.Width, Drawing.Height);
            if (team == Team.Radiant)
            {
                return screenSize.X / RadiantCompare + 1;
            }

            return screenSize.X / DireCompare + 1;
        }

        #endregion
    }
}