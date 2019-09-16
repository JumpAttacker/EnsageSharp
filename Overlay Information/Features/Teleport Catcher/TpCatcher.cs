using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using Ensage.SDK.Extensions;
using Ensage.SDK.Helpers;
using Ensage.SDK.Menu;
using Ensage.SDK.Renderer;
using log4net;
using PlaySharp.Toolkit.Logging;
using SharpDX;
using Color = System.Drawing.Color;

namespace OverlayInformation.Features.Teleport_Catcher
{
    public class TpCatcher
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly MenuItem<Slider> _fontSize;

        public readonly List<Vector3> ColorList = new List<Vector3>
        {
            new Vector3(0.2f, 0.4588236f, 1),
            new Vector3(0.4f, 1, 0.7490196f),
            new Vector3(0.7490196f, 0, 0.7490196f),
            new Vector3(0.9529412f, 0.9411765f, 0.04313726f),
            new Vector3(1, 0.4196078f, 0),
            new Vector3(0.9960784f, 0.5254902f, 0.7607843f),
            new Vector3(0.6313726f, 0.7058824f, 0.2784314f),
            new Vector3(0.3960784f, 0.8509804f, 0.9686275f),
            new Vector3(0, 0.5137255f, 0.1294118f),
            new Vector3(0.6431373f, 0.4117647f, 0)
        };

        private readonly MenuItem<StringList> Type;

        public List<TeleportEffect> Effects;
        public List<TowerOrShrine> TowerOrShrines;

        public TpCatcher(Config config)
        {
            Config = config;
            var panel = Config.Factory.Menu("Tp Catcher");
            Enable = panel.Item("Enable", true);
            Notification = panel.Item("Notification", true);
            DrawNames = panel.Item("Draw name on minimap", false);
            _fontSize = panel.Item("Font/Icon size", new Slider(18, 1, 25));
            ExtraTime = panel.Item("Extra drawing time for tp (ms)", new Slider(0, 0, 5000));
            Type = panel.Item("Drawing type", new StringList("Icon", "Colored cyrcle"));
            Render = config.Main.Renderer;

            Effects = new List<TeleportEffect>();

            TowerOrShrines = new List<TowerOrShrine>();
            foreach (var unit in EntityManager<Unit>.Entities.Where(
                x =>
                    x.IsValid && (x.NetworkName == "CDOTA_BaseNPC_Healer" || x.NetworkName == "CDOTA_BaseNPC_Tower"))
            )
                TowerOrShrines.Add(new TowerOrShrine(unit));
            EntityManager<Unit>.EntityRemoved += (sender, x) =>
            {
                if (x.NetworkName == "CDOTA_BaseNPC_Healer" || x.NetworkName == "CDOTA_BaseNPC_Tower")
                {
                    var remove = TowerOrShrines.Find(y => y.Unit == x);
                    if (remove != null) TowerOrShrines.Remove(remove);
                }
            };
            if (Enable)
            {
                Entity.OnParticleEffectAdded += OnParticle;
                Render.Draw += RenderOnDraw;
                Drawing.OnDraw += DrawingOnOnDraw;
                UpdateManager.Subscribe(EffectChecker, 100);
            }

            Enable.Item.ValueChanged += (sender, args) =>
            {
                if (args.GetNewValue<bool>())
                {
                    Entity.OnParticleEffectAdded += OnParticle;
                    Render.Draw += RenderOnDraw;
                    UpdateManager.Subscribe(EffectChecker, 100);
                    Drawing.OnDraw += DrawingOnOnDraw;
                }
                else
                {
                    Entity.OnParticleEffectAdded -= OnParticle;
                    Render.Draw -= RenderOnDraw;
                    Drawing.OnDraw -= DrawingOnOnDraw;
                    UpdateManager.Unsubscribe(EffectChecker);
                }
            };
        }

        public Config Config { get; }

        public MenuItem<bool> DrawNames { get; set; }

        public MenuItem<bool> Notification { get; set; }

        public MenuItem<Slider> ExtraTime { get; set; }

        public IRenderManager Render { get; set; }

        public MenuItem<bool> Enable { get; set; }

        private void DrawingOnOnDraw(EventArgs args)
        {
            foreach (var tpEffect in Effects) //.Where(x => !x.IsAlly))
            {
                var position = Drawing.WorldToScreen(tpEffect.StartPos);
                if (position.IsZero)
                    continue;
                var player = tpEffect.Player;
                var time = tpEffect.GetRemTime;
                if (time + ExtraTime / 1000f < 0)
                    continue;
                var size = new Vector2(50);
                var pos = position - size / 2;
                if (player == null || !player.IsValid)
                {
                    Drawing.DrawRect(pos, new Vector2(40), SharpDX.Color.Red);

                    Drawing.DrawText(time.ToString("F1"), position + new Vector2(-size.X / 2f, size.Y / 2f),
                        new Vector2(20),
                        SharpDX.Color.White, FontFlags.None);
                    continue;
                }

                var hero = player.Hero;
                if (hero == null || !hero.IsValid)
                    continue;

                Drawing.DrawRect(pos, new Vector2(50), Textures.GetHeroRoundTexture(hero.Name));

                Drawing.DrawText(time.ToString("F1"), position + new Vector2(-size.X / 2f, size.Y / 2f),
                    new Vector2(20),
                    SharpDX.Color.White, FontFlags.None);
            }
        }

        private void EffectChecker()
        {
            var tempRemover = Effects.ToList();
            foreach (var tpEffect in tempRemover)
            {
                var effect = tpEffect.Effect;
                if (effect == null || !effect.IsValid || effect.IsDestroyed ||
                    tpEffect.GetRemTime + ExtraTime / 1000f < 0)
                    Effects.Remove(tpEffect);
            }
        }

        private void RenderOnDraw(IRenderer renderer)
        {
            if (Effects == null || !Effects.Any()) return;
            try
            {
                foreach (var tpEffect in Effects) //.Where(x=>!x.IsAlly))
                    try
                    {
                        if (tpEffect.Player == null)
                            continue;
                        /*var vecClr = Config.TpCatcher.ColorList[tpEffect.Id] * 255;
                        var color = Color.FromArgb((int)vecClr.X, (int)vecClr.Y, (int)vecClr.Z);*/

                        /*Render.DrawRectangle(new RectangleF(200, 200, 200, 200), color, 100);
                        Render.DrawCircle(new Vector2(500,500), 50, color);*/
                        var pos = tpEffect.StartPos.WorldToMinimap();
                        if (Type.Value.SelectedIndex == 0)
                        {
                            try
                            {
                                renderer.DrawTexture(tpEffect.Hero.HeroId.ToString(),
                                    new RectangleF(pos.X - _fontSize / 2f, pos.Y - _fontSize / 2f, _fontSize,
                                        _fontSize));
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"Error: {e}");
                            }
                        }
                        else
                        {
                            renderer.DrawCircle(pos, 10, tpEffect._color);
                            if (DrawNames)
                                renderer.DrawText(pos - new Vector2(_fontSize, 0), tpEffect.Hero.GetDisplayName(),
                                    tpEffect._color, _fontSize);
                        }

                        //Render.DrawCircle(tpEffect.StartPos.WorldToMinimap(), 5, tpEffect.IsAlly ? Color.RoyalBlue : Color.Red);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void OnParticle(Entity sender, ParticleEffectAddedEventArgs args)
        {
            var name = args.Name;
            if (name.Contains("teleport_start") || name.Contains("teleport_end"))
                DelayAction.Add(10, () =>
                {
                    var isStart = name.Contains("teleport_start");
                    var effect = args.ParticleEffect;
                    var a = effect.GetControlPoint(0);
                    if (a.IsZero)
                        return;
                    var b = effect.GetControlPoint(2);
                    var tp = new TeleportEffect(effect, a, b, isStart, this);
                    Effects.Add(tp);
                });
        }

        public void OnDeactivate()
        {
            Drawing.OnDraw -= DrawingOnOnDraw;
            Render.Draw -= RenderOnDraw;
            UpdateManager.Unsubscribe(EffectChecker);
        }

        public class TeleportEffect
        {
            private readonly TpCatcher _main;
            public Color _color;

            public TeleportEffect(ParticleEffect effect, Vector3 startPos, Vector3 clr, bool isStart,
                TpCatcher tpCatcher)
            {
                _main = tpCatcher;
                Effect = effect;
                StartPos = startPos;
                Clr = clr;
                IsStart = isStart;
                StartTime = Game.RawGameTime;
                LifeTime = 3.0f;
                HasTravelBoots = false;
                Id = Player == null ? 0 : Player.Id;
                var vecClr = tpCatcher.ColorList[Id] * 255;
                _color = Color.FromArgb((int) vecClr.X, (int) vecClr.Y, (int) vecClr.Z);
                Player = TryToGetPlayer();
                if (!isStart)
                {
                    //TryToGetnPlayerExp();
                }
            }

            public ParticleEffect Effect { get; }
            public Vector3 StartPos { get; }
            public Vector3 Clr { get; set; }
            public bool IsStart { get; }
            public bool IsAlly { get; set; }
            public float StartTime { get; set; }
            public Player Player { get; set; }
            public Hero Hero { get; set; }
            public float LifeTime { get; set; }
            public float GetRemTime => LifeTime - Game.RawGameTime + StartTime;
            public bool HasTravelBoots { get; set; }

            public int Id { get; set; }

            private void Callback()
            {
                var tpStarter = _main.Effects.FirstOrDefault(x =>
                    !x.IsStart && !x.Clr.IsZero && Math.Abs(StartTime - x.StartTime) < 5);
                if (tpStarter != null)
                {
                    Clr = tpStarter.Clr;
                    Player = TryToGetPlayer();
                    //TryToGetnPlayerExp();
                    if (!HasTravelBoots)
                        LifeTime = tpStarter.LifeTime;
                    UpdateManager.Unsubscribe(Callback);
                }
                else if (Game.RawGameTime - StartTime > 50)
                {
                    UpdateManager.Unsubscribe(Callback);
                }
            }

            private void TryToGetnPlayerExp()
            {
                Player = Effect.Owner as Player;
                if (Player == null)
                {
                    Log.Error("cant entity to player!");
                    return;
                }

                Hero = Player?.Hero;
                IsAlly = Player?.Team == ObjectManager.LocalHero.Team;
                HasTravelBoots = Hero.GetItemById(AbilityId.item_travel_boots) != null ||
                                 Hero.GetItemById(AbilityId.item_travel_boots_2) != null;
                var closest =
                    _main.TowerOrShrines.Where(
                            x => x.IsAlive && x.Team == Hero.Team && x.Unit.IsInRange(StartPos, 1150))
                        .OrderBy(x => x.Unit.Position.Distance2D(StartPos)).FirstOrDefault();
                if (closest != null)
                {
                    closest.Inc();
                    LifeTime = closest.CalculateLifeTime();
                    Log.Debug($"[{Effect.Handle}] LifeTime -> {LifeTime}");
                }
                else
                {
                    Log.Debug("Cant find closest Tower or Shrine");
                }
            }

            private Player TryToGetPlayer()
            {
                var id = (uint) _main.ColorList.FindIndex(x => x == Clr);
                if (id <= 10)
                {
                    var player = ObjectManager.GetPlayerById(id);
                    if (player == null || !player.IsValid) return null;
                    IsAlly = player.Team == ObjectManager.LocalHero.Team;
                    var hero = player.Hero;
                    if (hero != null && hero.IsValid)
                    {
                        Hero = hero;
                        if (!IsStart)
                        {
                            try
                            {
                                HasTravelBoots = hero.GetItemById(AbilityId.item_travel_boots) != null ||
                                                 hero.GetItemById(AbilityId.item_travel_boots_2) != null;
                            }
                            catch (Exception)
                            {
                                HasTravelBoots = false;
                            }

                            if (!HasTravelBoots)
                            {
                                var closest =
                                    _main.TowerOrShrines.Where(
                                            x => x.IsAlive && x.Team == hero.Team && x.Unit.IsInRange(StartPos, 1150))
                                        .OrderBy(x => x.Unit.Position.Distance2D(StartPos)).FirstOrDefault();
                                if (closest != null)
                                {
                                    closest.Inc();
                                    LifeTime = closest.CalculateLifeTime();
                                    //Log.Debug($"[{Effect.Handle}] LifeTime -> {LifeTime}");
                                }
                            }

                            Program.GenerateTpCatcherSideMessage(Hero?.Name,
                                HasTravelBoots
                                    ? AbilityId.item_travel_boots.ToString()
                                    : AbilityId.item_tpscroll.ToString(),
                                5000);
                        }

                        Id = Player == null ? 0 : Player.Id;
                        var vecClr = _main.ColorList[Id] * 255;
                        _color = Color.FromArgb((int) vecClr.X, (int) vecClr.Y, (int) vecClr.Z);
                        //Log.Debug($"Player ({id}) -> {player.Name} and hero was found -> {hero.GetDisplayName()}");
                    }

                    return player;
                }

                //Log.Error($"Wrong id: {id} (Start: {IsStart})");
                return null;
            }
        }
    }
}