using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.SDK.Helpers;
using Ensage.SDK.Menu;
using Ensage.SDK.Renderer;
using log4net;
using OverlayInformation.Features.Teleport_Catcher;
using PlaySharp.Toolkit.Logging;
using SharpDX;

namespace OverlayInformation.Features
{
    public class ShrineHelper
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Dictionary<uint, Ability> _abilityDictinart;
        private readonly Dictionary<TowerOrShrine, ParticleEffect> _effects;
        private Dictionary<TowerOrShrine, bool> _underVision;

        public ShrineHelper(Config config)
        {
            _underVision = new Dictionary<TowerOrShrine, bool>();
            _abilityDictinart = new Dictionary<uint, Ability>();
            _effects = new Dictionary<TowerOrShrine, ParticleEffect>();

            Config = config;
            var panel = Config.Factory.Menu("Shrine Helper");
            Enable = panel.Item("Enable", true);
            DrawVisible = panel.Item("Draw on minimap if under enemy's vision", true);
            TextOnMinimapSize = panel.Item("Text size (minimap)", new Slider(18, 10, 25));
            RenderMode = Config.Main.Renderer;
            Enable.Item.ValueChanged += (sender, args) =>
            {
                if (!DrawVisible)
                    return;
                if (args.GetNewValue<bool>())
                {
                    RenderMode.Draw += ValueOnDraw;
                    UpdateManager.Subscribe(Callback, 150);
                    RenderMode.Draw += ValueOnDraw;
                    Entity.OnInt32PropertyChange += EntityOnOnInt32PropertyChange;
                }
                else
                {
                    RenderMode.Draw -= ValueOnDraw;
                    UpdateManager.Unsubscribe(Callback);
                    Entity.OnInt32PropertyChange -= EntityOnOnInt32PropertyChange;
                    Drawing.OnDraw -= DrawingOnOnDraw;
                    if (_effects.Any())
                        foreach (var element in _effects.ToDictionary(x => x.Key, y => y.Value))
                            UnHandleEffect(element.Key);
                }
            };
            DrawVisible.Item.ValueChanged += (sender, args) =>
            {
                if (!Enable)
                    return;
                if (args.GetNewValue<bool>())
                    Drawing.OnDraw += DrawingOnOnDraw;
                else
                    RenderMode.Draw -= ValueOnDraw;
            };
            if (Enable)
            {
                if (DrawVisible) RenderMode.Draw += ValueOnDraw;

                UpdateManager.Subscribe(Callback, 150);
                Entity.OnInt32PropertyChange += EntityOnOnInt32PropertyChange;

                Drawing.OnDraw += DrawingOnOnDraw;
            }

            Shrines = new List<TowerOrShrine>();
            foreach (var source in EntityManager<Unit>.Entities.Where(
                x =>
                    x.IsValid && x.IsAlive && x.Team == config.Main.Context.Value.Owner.Team &&
                    x.NetworkName == "CDOTA_BaseNPC_Healer"))
                Shrines.Add(new TowerOrShrine(source));

            foreach (var me in Shrines)
            {
                bool visible;
                if (!_underVision.TryGetValue(me, out visible)) _underVision.Add(me, me.Unit.IsVisibleToEnemies);
            }
        }

        public Config Config { get; }

        public IRenderManager RenderMode { get; set; }

        public MenuItem<Slider> TextOnMinimapSize { get; set; }

        public List<TowerOrShrine> Shrines { get; set; }

        public MenuItem<bool> DrawVisible { get; set; }

        public MenuItem<bool> Enable { get; set; }

        private void EntityOnOnInt32PropertyChange(Entity sender, Int32PropertyChangeEventArgs args)
        {
            if (args.PropertyName == "m_iTaggedAsVisibleByTeam")
            {
                var me = Shrines.Find(x => x.Unit.Equals(sender));
                if (me != null)
                {
                    var newValue = args.NewValue;
                    var oldValue = args.OldValue;
                    var isVisible = newValue == 14;
                    if (newValue != oldValue)
                    {
                        bool visible;
                        if (!_underVision.TryGetValue(me, out visible))
                            _underVision.Add(me, isVisible);
                        else
                            _underVision[me] = isVisible;
                    }
                }
            }
        }

        private void DrawingOnOnDraw(EventArgs args)
        {
            if (Enable)
                foreach (var shrine in Shrines)
                {
                    if (shrine.Unit == null || !shrine.Unit.IsValid)
                        continue;
                    var v = shrine.Unit;
                    var pos = HUDInfo.GetHPbarPosition(v);
                    if (pos.IsZero)
                        continue;
                    var filler = GetFiller(shrine);
                    if (filler == null || filler.AbilityState == AbilityState.Ready)
                        continue;
                    var cd = filler.Cooldown;
                    var cdLength = filler.CooldownLength;
                    var hpBarSize = HUDInfo.GetHPBarSizeX();
                    var size = new Vector2(hpBarSize * 2, 15);
                    var buff = v.FindModifier("modifier_filler_heal_aura");
                    var isBuff = buff != null;
                    var remTine = buff?.RemainingTime;
                    var cdDelta = isBuff ? buff.RemainingTime * size.X / 5 : cd * size.X / cdLength;
                    pos += new Vector2(-hpBarSize / 2, hpBarSize * 1.5f);
                    if (true)
                    {
                        Drawing.DrawRect(pos, new Vector2(size.X, size.Y), Color.Black);
                        Drawing.DrawRect(pos, new Vector2(isBuff ? cdDelta : size.X - cdDelta, size.Y),
                            isBuff ? Color.Orange : Color.YellowGreen);
                        Drawing.DrawRect(pos, new Vector2(size.X, size.Y), Color.Black, true);
                    }

                    if (true)
                    {
                        var text = isBuff ? $"{(int) (remTine / 5 * 100)}%" : $"{(int) (100 - cd / cdLength * 100)}%";
                        var textSize = Drawing.MeasureText(text, "Arial",
                            new Vector2(size.Y * 1, size.Y / 2), FontFlags.AntiAlias);
                        var textPos = pos + new Vector2(size.X / 2 - textSize.X / 2, size.Y - textSize.Y);
                        /*Drawing.DrawRect(textPos - new Vector2(0, 0),
                        new Vector2(textSize.X, textSize.Y),
                        new Color(0, 0, 0, 200));*/
                        Drawing.DrawText(
                            text,
                            textPos,
                            new Vector2(textSize.Y, 0),
                            Color.White,
                            FontFlags.AntiAlias | FontFlags.StrikeOut);
                    }
                }
        }

        private void Callback()
        {
            if (!Enable)
                return;
            var me = Config.Main.Context.Value.Owner;
            var myPos = me.Position;
            foreach (var shrine in Shrines)
            {
                if (shrine.Unit == null || !shrine.Unit.IsValid)
                    continue;
                //var inRange = me.IsInRange(shrine, 700);
                var inRange = myPos.Distance2D(shrine.Position) <= 700;

                if (inRange && CheckForAbility(shrine))
                    HandleEffect(shrine);
                else
                    UnHandleEffect(shrine);
            }
        }

        private void ValueOnDraw(IRenderer renderer)
        {
            var removeList = _underVision;
            foreach (var b in _underVision.ToList())
            {
                if (!b.Value) continue;
                var me = b.Key;
                if (!me.Unit.IsValid || !me.IsAlive)
                {
                    removeList.Remove(me);
                }
                else
                {
                    var pos = me.Position;
                    var mapPos = pos.WorldToMinimap();
                    renderer.DrawText(mapPos - new Vector2(TextOnMinimapSize / 2f, TextOnMinimapSize), "V",
                        System.Drawing.Color.White, TextOnMinimapSize);
                }
            }

            _underVision = removeList;
        }

        private bool CheckForAbility(TowerOrShrine shrine)
        {
            Ability s;
            var v = shrine.Unit;
            var handle = v.Handle;
            if (_abilityDictinart.TryGetValue(handle, out s))
                return s?.AbilityState == AbilityState.Ready || s?.Cooldown >= 295;
            s = v.Spellbook.Spells.FirstOrDefault(x => x.Id == AbilityId.filler_ability);
            if (s != null)
                _abilityDictinart.Add(handle, s);
            return s != null && (s.AbilityState == AbilityState.Ready || s.Cooldown >= 295);
        }

        private Ability GetFiller(TowerOrShrine shrine)
        {
            Ability s;
            var v = shrine.Unit;
            var handle = v.Handle;
            if (_abilityDictinart.TryGetValue(handle, out s)) return s;
            s = v.Spellbook.Spells.FirstOrDefault(x => x.Id == AbilityId.filler_ability);
            _abilityDictinart.Add(handle, s);
            return s;
        }

        private void UnHandleEffect(TowerOrShrine shrine)
        {
            var unit = shrine.Unit;
            ParticleEffect effect;
            if (_effects.TryGetValue(shrine, out effect)) effect.Dispose();
            _effects.Remove(shrine);
        }

        private void HandleEffect(TowerOrShrine shrine)
        {
            var unit = shrine.Unit;
            ParticleEffect effect;
            if (!_effects.TryGetValue(shrine, out effect))
            {
                effect = unit.AddParticleEffect("materials/ensage_ui/particles/range_display_mod.vpcf");
                effect.SetControlPoint(1, new Vector3(500, 255, 0));
                effect.SetControlPoint(2, new Vector3(0, 155, 255));
                _effects.Add(shrine, effect);
            }
        }

        public void OnDeactivate()
        {
            RenderMode.Draw -= ValueOnDraw;
            UpdateManager.Unsubscribe(Callback);
        }
    }
}