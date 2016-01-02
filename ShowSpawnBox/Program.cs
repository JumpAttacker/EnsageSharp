using System;
using System.Collections.Generic;
using System.Reflection;
using Ensage;
using Ensage.Common;
using SharpDX;

namespace SpawnBox
{
    internal static class Program
    {
        private static bool _loaded;
        private static readonly Dictionary<string, ParticleEffect> Effect = new Dictionary<string, ParticleEffect>();
        private static readonly Dictionary<string, ParticleEffect> Effect2 = new Dictionary<string, ParticleEffect>();
        private static readonly Dictionary<string, ParticleEffect> Effect3 = new Dictionary<string, ParticleEffect>();
        private static readonly Dictionary<string, ParticleEffect> Effect4 = new Dictionary<string, ParticleEffect>();
        private const string EffectPath = @"particles\world_environmental_fx\candle_flame.vpcf";
        private static readonly int[,] Spots = {
            {2690, -4409, 3529, -5248}, {3936, -3277, 5007, -4431}, 
			{1088, -3200, 2303, -4543}, {-3307, 383, -2564, -413},
			{-1023, -2728, 63, -3455}, {-2227, -3968, -1463, -4648},
			{-4383, 1295, -3136, 400}, {3344, 942, 4719, 7},
			{-3455, 4927, -2688, 3968}, {-4955, 4071, -3712, 3264}, 
			{3456, -384, 4543, -1151}, {-1967, 3135, -960, 2176},
			{-831, 4095, 0, 3200}, {448, 3775, 1663, 2816}
        };

        private static void Main()
        {
            _loaded = false;
            Game.OnUpdate += Game_OnUpdate;
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
                Game.PrintMessage(
                    "<font face='Comic Sans MS, cursive'><font color='#00aaff'>" + "SpawnBox" +
                    " By Jumpering" +
                    " loaded!</font> <font color='#aa0000'>v" + Assembly.GetExecutingAssembly().GetName().Version,
                    MessageType.LogMessage);
            }
            if (!Game.IsInGame || me == null)
            {
                _loaded = false;
                PrintInfo("> Spawn Box unLoaded");
                Effect.Clear();
                Effect2.Clear();
                Effect3.Clear();
                Effect4.Clear();
                return;
            }
            if (!Game.IsInGame || !_loaded || !Utils.SleepCheck("Refer")) return;
            Utils.Sleep(500, "Refer");
            for (var i = 0; i < 14; i++)
            {
                var coint1 = Math.Floor(Math.Floor((decimal)(Spots[i, 2] - Spots[i, 0])) / 50);
                var coint2 = Math.Abs(Math.Floor(Math.Floor((decimal)(Spots[i, 3] - Spots[i, 1])) / 50));
                ParticleEffect effect;
                Vector2 screen;

                if (Drawing.WorldToScreen(new Vector3(Spots[i, 0], Spots[i, 1], 0), out screen))
                {
                    for (var a = 1; a < coint1; a++)
                    {
                        var first = new Vector3(Spots[i, 0] + a * 50, Spots[i, 1], 500);
                        var second = new Vector3(Spots[i, 2] - a * 50, Spots[i, 3], 500);
                        if (!Effect.ContainsKey(string.Format("{0} / {1}", i, a)))
                        {
                            effect = new ParticleEffect(EffectPath,
                                first);
                            effect.SetControlPoint(0, first);
                            Effect.Add(string.Format("{0} / {1}", i, a), effect);
                        }
                        if (!Effect2.ContainsKey(string.Format("{0} / {1}", i, a)))
                        {
                            effect = new ParticleEffect(EffectPath,
                                second);
                            effect.SetControlPoint(0, second);
                            Effect2.Add(string.Format("{0} / {1}", i, a), effect);
                        }
                    }
                    for (var a = 1; a < coint2; a++)
                    {
                        var first = new Vector3(Spots[i, 0], Spots[i, 1] - a * 50, 500);
                        var second = new Vector3(Spots[i, 2], Spots[i, 3] + a * 50, 500);
                        if (!Effect3.ContainsKey(string.Format("{0} / {1}", i, a)))
                        {
                            effect = new ParticleEffect(EffectPath,
                                first);
                            effect.SetControlPoint(0, first);
                            Effect3.Add(string.Format("{0} / {1}", i, a), effect);
                        }
                        if (!Effect4.ContainsKey(string.Format("{0} / {1}", i, a)))
                        {
                            effect = new ParticleEffect(EffectPath,
                                second);
                            effect.SetControlPoint(0, second);
                            Effect4.Add(string.Format("{0} / {1}", i, a), effect);
                        }
                    }
                }
                else
                {
                    for (var a = 1; a < coint1; a++)
                    {
                        if (Effect.TryGetValue(string.Format("{0} / {1}", i, a), out effect))
                        {
                            effect.Dispose();
                            Effect.Remove(string.Format("{0} / {1}", i, a));
                        }
                        if (Effect2.TryGetValue(string.Format("{0} / {1}", i, a), out effect))
                        {
                            effect.Dispose();
                            Effect2.Remove(string.Format("{0} / {1}", i, a));
                        }
                    }
                    for (var a = 1; a < coint2; a++)
                    {
                        if (Effect3.TryGetValue(string.Format("{0} / {1}", i, a), out effect))
                        {
                            effect.Dispose();
                            Effect3.Remove(string.Format("{0} / {1}", i, a));
                        }
                        if (Effect4.TryGetValue(string.Format("{0} / {1}", i, a), out effect))
                        {
                            effect.Dispose();
                            Effect4.Remove(string.Format("{0} / {1}", i, a));
                        }
                    }
                }

            }

        }

        private static void PrintInfo(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.White, arguments);
        }

        private static void PrintSuccess(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.Green, arguments);
        }
        private static void PrintError(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.Red, arguments);
        }

        private static void PrintEncolored(string text, ConsoleColor color, params object[] arguments)
        {
            var clr = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text, arguments);
            Console.ForegroundColor = clr;
        }
    }
}
