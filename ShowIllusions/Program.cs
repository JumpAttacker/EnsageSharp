using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using SharpDX;

namespace ShowIllusions
{
    class Program
    {

        #region Members

        private static bool _loaded;
        private static Hero _me;
        private static Player _player;
        private static readonly Dictionary<Unit, ParticleEffect> Effects = new Dictionary<Unit, ParticleEffect>();
        private static readonly Dictionary<Unit, ParticleEffect> Visible = new Dictionary<Unit, ParticleEffect>();

        #endregion


        private static void Main()
        {
            Game.OnUpdate += Game_OnUpdate;
            _loaded = false;
        }
        private static void Game_OnUpdate(EventArgs args)
        {
            if (!_loaded)
            {
                _me = ObjectMgr.LocalHero;
                _player = ObjectMgr.LocalPlayer;
                if (!Game.IsInGame || _me == null)
                {
                    return;
                }
                _loaded = true;
                PrintSuccess("> Show Illusion Loaded");
            }

            if (!Game.IsInGame || _me == null)
            {
                _loaded = false;
                PrintInfo("> Show Illusion unLoaded");
                return;
            }

            if (Game.IsPaused)
            {
                return;
            }

            if (_player == null || _player.Team == Team.Observer)
                return;
            var illusions = ObjectMgr.GetEntities<Hero>()
                .Where(
                    x =>
                        x.IsIllusion && x.Team != _player.Team);
            foreach (var s in illusions)
            {
                HandleEffect(s);
            }
        }

        private static void HandleEffect(Unit unit)
        {
            ParticleEffect effect;
            if (unit.IsAlive && unit.IsVisibleToEnemies)
            {
                if (Visible.TryGetValue(unit, out effect)) return;
                effect = unit.AddParticleEffect("particles/items2_fx/smoke_of_deceit_buff.vpcf"); //particles/items_fx/diffusal_slow.vpcf
                Visible.Add(unit, effect);
            }
            else
            {
                if (!Visible.TryGetValue(unit, out effect)) return;
                effect.Dispose();
                Visible.Remove(unit);
            }
        }

        #region Helpers

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

        #endregion

    }
}
