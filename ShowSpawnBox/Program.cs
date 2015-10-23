using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ensage;
using SharpDX;
using SharpDX.Direct3D9;

namespace SpawnBox
{
    class Program
    {
        private const string Ver = "1.0";
        private static bool _loaded;
        private static readonly Dictionary<int, ParticleEffect> Effect = new Dictionary<int, ParticleEffect>();
        private static readonly Dictionary<int, ParticleEffect> Effect2 = new Dictionary<int, ParticleEffect>();
        private static bool Create;
        private static readonly int[,] Spots = {
            {2240, -4288, 3776, -5312}, {2688, -2944, 3776, -4096},
            {1088, -3200, 2304, -4544}, {-3530, 768, -2560, -256},
            {-1026, -2368, 62, -3451}, {-1728, -3522, -706, -4478},
            {-3459, 4928, -2668, 3968}, {-5056, 4352, -3712, 3264},
            {3390, -105, 4739, -1102}, {-1921, 3138, -964, 2308},
            {-832, 4098, -3, 3203}, {447, 3778, 1659, 2822}
        };

        private static void Main(string[] args)
        {
            _loaded = false;
            Game.OnUpdate += Game_OnUpdate;
            //Drawing.OnDraw += Drawing_OnDraw;
            //Drawing.OnEndScene += Drawing_OnEndScene;
        }

        private static void Drawing_OnEndScene(EventArgs args)
        {
            //Drawing.DrawRect(new Vector2(0,0),new Vector2(200,200),Color.Red);
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            var me = ObjectMgr.LocalHero;
            if (!_loaded)
            {
                if (!Game.IsInGame || me == null)
                {
                    return;
                }
                _loaded = true;
                PrintSuccess("> Spawn Box loaded! v" + Ver);
            }
            if (!Game.IsInGame || me == null)
            {
                _loaded = false;
                PrintInfo("> Spawn Box unLoaded");
                return;
            }
            if (!Game.IsInGame || !_loaded) return;
            for (var i = 0; i < 12; i++)
            {
                //PrintError(Spots[i, 0].ToString());
                
                Vector2 screen;

                if (Drawing.WorldToScreen(new Vector3(Spots[i, 0], Spots[i, 1], 0), out screen))
                {
                    Vector2 size;
                    if (Drawing.WorldToScreen(new Vector3(Spots[i, 2], Spots[i, 3], 0), out size))
                    {
                        Drawing.DrawRect(screen, size - screen, Color.AliceBlue);
                    }
                //Drawing.DrawRect(screen, size-size,
                        //Color.Red);//Spots[i, 2] - Spots[i, 0], Spots[i, 3] - Spots[i, 1]
                   // Drawing.DrawRect(new Vector2(200, 200), new Vector2(200, 200), Color.Red);
                }
            }
        }
        private static void Game_OnUpdate(EventArgs args)
        {
            var me = ObjectMgr.LocalHero;
            if (!_loaded)
            {
                if (!Game.IsInGame || me == null)
                {
                    return;
                }
                _loaded = true;
                PrintSuccess("> Spawn Box loaded! v" + Ver);
                Create = false;
            }
            if (!Game.IsInGame || me == null)
            {
                _loaded = false;
                PrintInfo("> Spawn Box unLoaded");
                return;
            }
            if (!Game.IsInGame || !_loaded) return;
            //PrintSuccess(me.Position.Z.ToString());
            if (Create) return;
            Create = true;
            for (var i = 0; i < 12; i++)
            {
                var coint1 = Math.Floor(Math.Floor((decimal) (Spots[i, 2] - Spots[i, 0]))/50);
                var coint2 = Math.Abs(Math.Floor(Math.Floor((decimal) (Spots[i, 3] - Spots[i, 1]))/50));
                ParticleEffect effect;
                for (var a = 1; a < coint1; a++)
                {
                    var first = new Vector3(Spots[i, 0] + a*50, Spots[i, 1], 500);
                    var second = new Vector3(Spots[i, 2] - a*50, Spots[i, 3], 500);
                    effect = new ParticleEffect(@"particles\world_environmental_fx\candle_flame_medium.vpcf",
                        first);
                    effect.SetControlPoint(0, first);
                    //Effect.Add(i, effect);
                    effect = new ParticleEffect(@"particles\world_environmental_fx\candle_flame_medium.vpcf",
                        second);
                    effect.SetControlPoint(0, second);
                    //Effect2.Add(i, effect);
                }
                /*
                 * x1=0 y1=1 
                 * x2=2 y2=3
                 * 
                 * */
                for (var a = 1; a < coint2; a++)
                {
                    var first = new Vector3(Spots[i, 0], Spots[i, 1] - a*50, 500);
                    var second = new Vector3(Spots[i, 2], Spots[i, 3] + a*50, 500);
                    effect = new ParticleEffect(@"particles\world_environmental_fx\candle_flame_medium.vpcf",
                        first);
                    effect.SetControlPoint(0, first);
                    //Effect.Add(i, effect);
                    effect = new ParticleEffect(@"particles\world_environmental_fx\candle_flame_medium.vpcf",
                        second);
                    effect.SetControlPoint(0, second);
                    //Effect2.Add(i, effect);
                }
                
            }

        }
        public static void PrintInfo(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.White, arguments);
        }
        public static void PrintSuccess(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.Green, arguments);
        }
        public static void PrintError(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.Red, arguments);
        }
        public static void PrintEncolored(string text, ConsoleColor color, params object[] arguments)
        {
            var clr = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text, arguments);
            Console.ForegroundColor = clr;
        }
    }
}
