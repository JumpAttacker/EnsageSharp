using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Objects.UtilityObjects;
using SharpDX;
using SharpDX.Direct3D9;

namespace OverlayInformation
{
    public static class HudInfoNew
    {
        #region Constants

        /// <summary>
        ///     The map bottom.
        /// </summary>
        private const float MapBottom = -7404;

        /// <summary>
        ///     The map left.
        /// </summary>
        private const float MapLeft = -8185;

        /// <summary>
        ///     The map right.
        /// </summary>
        private const float MapRight = 7641;

        /// <summary>
        ///     The map top.
        /// </summary>
        private const float MapTop = 7624;

        #endregion

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
        ///     The map height.
        /// </summary>
        private static float mapHeight = Math.Abs(MapBottom - MapTop);

        /// <summary>
        ///     The map width.
        /// </summary>
        private static float mapWidth = Math.Abs(MapLeft - MapRight);

        /// <summary>
        ///     The current minimap.
        /// </summary>
        private static Minimap currentMinimap;

        private static float minimapMapScaleX;

        private static float minimapMapScaleY;

        /// <summary>
        ///     The minimaps.
        /// </summary>
        private static Dictionary<Vector2, Minimap> minimaps = new Dictionary<Vector2, Minimap>
        {
            {
                // 4:3
                new Vector2(
                    800,
                    600),
                new Minimap(
                    new Vector2(4, 11),
                    new Vector2(
                        151,
                        146))
            },
            {
                new Vector2(
                    1024,
                    768),
                new Minimap(
                    new Vector2(5, 11),
                    new Vector2(
                        193,
                        186))
            },
            {
                new Vector2(
                    1152,
                    864),
                new Minimap(
                    new Vector2(6, 12),
                    new Vector2(
                        217,
                        211))
            },
            {
                new Vector2(
                    1280,
                    960),
                new Minimap(
                    new Vector2(6, 13),
                    new Vector2(
                        241,
                        235))
            },
            {
                new Vector2(
                    1280,
                    1024),
                new Minimap(
                    new Vector2(6, 13),
                    new Vector2(
                        255,
                        229))
            },
            {
                new Vector2(
                    1600,
                    1200),
                new Minimap(
                    new Vector2(8, 14),
                    new Vector2(
                        304,
                        288))
            },
            {
                // 16:9
                new Vector2
                    (
                    1280,
                    720),
                new Minimap
                    (
                    new Vector2
                        (
                        4,
                        12),
                    new Vector2
                        (
                        181,
                        174))
            },
            {
                new Vector2(
                    1360,
                    768),
                new Minimap(
                    new Vector2(4, 12),
                    new Vector2(
                        193,
                        186))
            },
            {
                new Vector2(
                    1366,
                    768),
                new Minimap(
                    new Vector2(4, 12),
                    new Vector2(
                        193,
                        186))
            },
            {
                new Vector2(
                    1600,
                    900),
                new Minimap(
                    new Vector2(4, 12),
                    new Vector2(
                        228,
                        217))
            },
            {
                new Vector2(
                    1920,
                    1080),
                new Minimap(
                    new Vector2(
                        5,
                        12),
                    new Vector2(
                        240,
                        +265))
            },
            {
                new Vector2(
                    2560,
                    1440),
                new Minimap(
                    new Vector2(
                        5,
                        12),
                    new Vector2(
                        372,
                        341))
            },
            {
                new Vector2(
                    2560,
                    1080),
                new Minimap(
                    new Vector2(
                        5,
                        11),
                    new Vector2(
                        272,
                        261))
            },
            {
                // 16:10
                new Vector2(
                    1024,
                    600),
                new Minimap(
                    new Vector2(4, 12),
                    new Vector2(
                        151,
                        146))
            },
            {
                new Vector2(
                    1280,
                    768),
                new Minimap(
                    new Vector2(4, 12),
                    new Vector2(
                        193,
                        186))
            },
            {
                new Vector2(
                    1280,
                    800),
                new Minimap(
                    new Vector2(4, 12),
                    new Vector2(
                        203,
                        192))
            },
            {
                new Vector2(
                    1440,
                    900),
                new Minimap(
                    new Vector2(4, 12),
                    new Vector2(
                        227,
                        217))
            },
            {
                new Vector2(
                    1680,
                    1050),
                new Minimap(
                    new Vector2(
                        4,
                        12),
                    new Vector2(
                        267,
                        252))
            },
            {
                new Vector2(
                    1920,
                    1200),
                new Minimap(
                    new Vector2(
                        5,
                        11),
                    new Vector2(
                        304,
                        288))
            },
            {
                new Vector2(
                    2560,
                    1600),
                new Minimap(
                    new Vector2(
                        20,
                        28),
                    new Vector2(
                        391,
                        356))
            },
            {
                new Vector2(
                    2880,
                    1800),
                new Minimap(
                    new Vector2(
                        30,
                        38),
                    new Vector2(
                        430,
                        396))
            }
        };

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
            currentMinimap =
                minimaps.FirstOrDefault(
                    x => Math.Abs(x.Key.X - ScreenSize.X) < 10 && Math.Abs(x.Key.Y - ScreenSize.Y) < 10).Value;
            if (currentMinimap == null)
            {
                Console.WriteLine("Could not find minimap data for your resolution");
            }
            else
            {
                minimapMapScaleX = currentMinimap.Size.X / mapWidth;
                minimapMapScaleY = currentMinimap.Size.Y / mapHeight;
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

        #region Public Properties

        /// <summary>
        ///     Gets the mouse position from minimap.
        /// </summary>
        public static Vector2 MousePositionFromMinimap
        {
            get
            {
                var mouse = Game.MouseScreenPosition;

                var scaledX = mouse.X - currentMinimap.Position.X;
                var scaledY = ScreenSize.Y - mouse.Y - currentMinimap.Position.Y;

                var x = scaledX / minimapMapScaleX + MapLeft;
                var y = scaledY / minimapMapScaleY + MapBottom;

                if (Math.Abs(x) > 7900 || Math.Abs(y) > 7200)
                {
                    return Vector2.Zero;
                }

                return new Vector2(x, y);
            }
        }

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
                if (unit.ClassID == ClassID.CDOTA_Unit_Hero_Meepo)
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
                id = hero.Player.ID;
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
        public static Vector2 GetTopPanelSize(Hero hero)
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
        public static double GetTopPanelSizeX(Hero hero)
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
        public static double GetTopPanelSizeY(Hero hero)
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

        /// <summary>
        ///     The world to minimap.
        /// </summary>
        /// <param name="mapPosition">
        ///     The map position.
        /// </param>
        /// <returns>
        ///     The <see cref="Vector2" />.
        /// </returns>
        public static Vector2 WorldToMinimap(this Vector3 mapPosition)
        {
            var x = mapPosition.X - MapLeft;
            var y = mapPosition.Y - MapBottom;

            var scaledX = Math.Min(Math.Max(x * minimapMapScaleX, 0), currentMinimap.Size.X);
            var scaledY = Math.Min(Math.Max(y * minimapMapScaleY, 0), currentMinimap.Size.Y);

            var screenX = currentMinimap.Position.X + scaledX;
            var screenY = ScreenSize.Y - scaledY - currentMinimap.Position.Y;

            return new Vector2((float)Math.Floor(screenX), (float)Math.Floor(screenY));
        }

        #endregion

        #region Methods

        private static void Drawing_OnDraw(EventArgs args)
        {
            Drawing.DrawRect(GetTopPanelPosition(Members.MyHero), GetTopPanelSize(Members.MyHero), Color.White);
            return;
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