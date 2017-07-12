using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.SDK.Extensions;
using Ensage.SDK.Helpers;
using Ensage.SDK.Menu;
using log4net;
using PlaySharp.Toolkit.Logging;
using SharpDX;

namespace InvokerAnnihilationCrappa.Features
{
    public class AutoSunStrike
    {
        private readonly Config _main;
        private ParticleEffect _predictionEffect;
        private readonly Dictionary<Hero, bool> _notificationForHero;
        public AutoSunStrike(Config main)
        {
            _main = main;
            var panel = main.Factory.Menu("Auto SunStike");
            Enable = panel.Item("Enable Auto SunStrike", true);
            Enable.Item.SetTooltip("on stunned enemy");
            OnlyKillSteal = panel.Item("Kill steal only", true);
            InvokeSunStrike = panel.Item("Invoke sun strike", true);
            DrawDamageHero = panel.Item("Draw Damage on hero", true);
            DrawDamageTop = panel.Item("Draw Damage on top panel", true);
            DrawPrediction = panel.Item("Draw Prediction", true);
            DrawPredictionInvoked = panel.Item("Draw Prediction only if ss invoked", true);
            DrawPredictionKillSteal = panel.Item("Draw Prediction only if enemy will die from ss", true);
            UseOnTeleportToo = panel.Item("Use SunStrike for heroes under tp", true);
            Notification = panel.Item("Notification if ss can kill enemy hero", true);
            _notificationForHero = new Dictionary<Hero, bool>();
            if (Enable)
            {
                UpdateManager.BeginInvoke(Callback);
            }

            if (DrawPrediction)
            {
                UpdateManager.Subscribe(PredictionCallBack);
            }

            DrawPrediction.Item.ValueChanged += (sender, args) =>
            {
                if (args.GetNewValue<bool>())
                    UpdateManager.Subscribe(PredictionCallBack);
                else
                    UpdateManager.Unsubscribe(PredictionCallBack);
            };

            Enable.Item.ValueChanged += (sender, args) =>
            {
                if (args.GetNewValue<bool>())
                    UpdateManager.BeginInvoke(Callback);
            };

            Drawing.OnDraw += DrawingOnOnDraw;
        }

        public MenuItem<bool> Notification { get; set; }

        public MenuItem<bool> UseOnTeleportToo { get; set; }

        public MenuItem<bool> DrawPredictionKillSteal { get; set; }

        private void PredictionCallBack()
        {
            if (_main.Invoker.TargetManager == null || !_main.Invoker.TargetManager.IsValueCreated ||
                _main.Invoker.TargetManager.Value == null || !_main.Invoker.TargetManager.Value.IsActive)
                return;
            try
            {
                if (!_main.Invoker.TargetManager.Value.Active.IsActive)
                    return;
            }
            catch (Exception)
            {
                Log.Warn("Seems you set in Target Selector -> Activa selector -> none. Please activate this");
                return;
            }
            
            if (!DrawPredictionInvoked && _main.Invoker.SunStrike.Ability.IsHidden)
            {
                FlushEffect();
                return;
            }
            var target = _main.Invoker.TargetManager.Value.Active.GetTargets().FirstOrDefault();
            if (target == null)
            {
                FlushEffect();
                return;
            }
            if (target.Health - GetSunStikeDamage > 0)
            {
                if (DrawPredictionKillSteal)
                {
                    FlushEffect();
                    return;
                }
            }
            Vector3 predict = target.Predict(1700);
            if (_predictionEffect == null)
            {
                _predictionEffect = new ParticleEffect(@"particles\ui_mouseactions\range_display.vpcf",
                    predict);
                _predictionEffect.SetControlPoint(1, new Vector3(175, 0, 0));
            }
            _predictionEffect.SetControlPoint(0, predict);
            //_main.Invoker.ParticleManager.Value.DrawCircle(target.Predict(1700), "SunStrikePrediction", 200, Color.White);
        }

        private void FlushEffect()
        {
            _predictionEffect?.Dispose();
            _predictionEffect = null;
        }

        public MenuItem<bool> InvokeSunStrike { get; set; }
        public MenuItem<bool> DrawDamageHero { get; set; }
        public MenuItem<bool> DrawDamageTop { get; set; }
        public MenuItem<bool> DrawPrediction { get; set; }
        public MenuItem<bool> DrawPredictionInvoked { get; set; }
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public MenuItem<bool> OnlyKillSteal { get; set; }

        private float GetSunStikeDamage
            => (_main.Invoker.SunStrike.Ability.SpellAmplification() + 1) * (37.5f + 62.5f * _main.Invoker.Exort.Level);

        private async void Callback()
        {
            var sunStike = _main.Invoker.SunStrike;
            while (Enable)
            {
                if (!_main.Invoker.Mode.CanExecute && sunStike.Ability.AbilityState == AbilityState.Ready)
                {
                    var canBeCasted = sunStike.Ability.ManaCost + _main.Invoker.InvokeAbility.ManaCost <
                                      _main.Invoker.Owner.Mana;
                    if (canBeCasted)
                    {
                        var heroes =
                            EntityManager<Hero>.Entities.Where(
                                x => x.IsAlive && !x.IsAlly(_main.Invoker.Owner) && x.IsVisible && !x.IsIllusion);
                        foreach (var hero in heroes)
                        {
                            if (OnlyKillSteal || Notification)
                            {
                                var heroWillNotDie = hero.Health + hero.HealthRegeneration * 1.7f > GetSunStikeDamage;
                                if (Notification)
                                {
                                    bool value;
                                    if (!_notificationForHero.TryGetValue(hero, out value))
                                    {
                                        _notificationForHero.Add(hero, true);
                                    }
                                    if (!heroWillNotDie && !value)
                                    {
                                        _notificationForHero[hero] = true;
                                        Program.GenerateSideMessage(hero.Name.Replace("npc_dota_hero_", ""), sunStike.Ability.Name);
                                    }
                                    else if (value && heroWillNotDie)
                                    {
                                        _notificationForHero[hero] = false;
                                    }
                                }
                                if (OnlyKillSteal && heroWillNotDie)
                                {
                                    continue;
                                }
                            }
                            float time;
                            var stunned = hero.IsStunned(out time);
                            var comboModifiers = hero.HasModifiers(new[]
                            {
                                "modifier_obsidian_destroyer_astral_imprisonment_prison", "modifier_eul_cyclone",
                                "modifier_shadow_demon_disruption", "modifier_invoker_tornado"
                            }, false);
                            if (stunned)
                            {
                                if (InvokeSunStrike && comboModifiers && time > 1.7)
                                {
                                    if (!sunStike.Ability.CanBeCasted())
                                    {
                                        Log.Info("[before] invoke for Auto SS");
                                        await _main.Invoker.InvokeAsync(sunStike);
                                    }
                                }
                                if (comboModifiers && time <= 1.69 && time >= 1.35 ||
                                    !comboModifiers && time > 1.7 && !hero.IsInvul())
                                {
                                    if (sunStike.Ability.CanBeCasted())
                                    {
                                        Log.Info("casted SS due Auto SS");
                                        sunStike.Ability.UseAbility(hero.Position);
                                    }
                                    else if (InvokeSunStrike)
                                    {
                                        Log.Info("invoke for Auto SS");
                                        var invoked = await _main.Invoker.InvokeAsync(sunStike);
                                        if (invoked)
                                        {
                                            Log.Info("casted SS due Auto SS");
                                            sunStike.Ability.UseAbility(hero.Position);
                                        }
                                    }
                                    
                                    await Task.Delay(500);
                                }
                            }
                            else if (UseOnTeleportToo)
                            {
                                var tpMod = hero.FindModifier("modifier_teleporting");
                                var remTime = tpMod?.RemainingTime;

                                if (!(remTime > 1.7)) continue;
                                if (InvokeSunStrike)
                                {
                                    Log.Info("invoke for Auto SS");
                                    var invoked = await _main.Invoker.InvokeAsync(sunStike);
                                    if (invoked)
                                    {
                                        Log.Info("casted SS due Auto SS");
                                        sunStike.Ability.UseAbility(hero.Position);
                                    }
                                }
                                else if (sunStike.Ability.CanBeCasted())
                                {
                                    Log.Info("casted SS due Auto SS");
                                    sunStike.Ability.UseAbility(hero.Position);
                                }

                                await Task.Delay(500);
                            }
                        }
                    }
                    else
                    {
                        //Log.Error("auto ss cant be casted due low mana");
                    }
                    
                }
                await Task.Delay(10);
            }
        }

        private Modifier GetOneOfModifiers(Hero target, IEnumerable<string> modifiers)
        {
            return modifiers.Select(target.FindModifier).FirstOrDefault(mod => mod != null);
        }

        private void DrawingOnOnDraw(EventArgs args)
        {
            var heroes =
                EntityManager<Hero>.Entities.Where(
                    x => x.IsAlive && !x.IsAlly(_main.Invoker.Owner) && x.IsVisible);
            if (!DrawDamageHero && !DrawDamageTop)
                return;
            if (_main.Invoker.Exort.Level==0)
                return;
            var damage = GetSunStikeDamage;
            foreach (var hero in heroes)
            {
                var text = $"{(int)(hero.Health - damage)}";
                if (DrawDamageHero)
                {
                    var pos = HUDInfo.GetHPbarPosition(hero);
                    if (!pos.IsZero)
                    {
                        var size = new Vector2(HUDInfo.HpBarY / 1.5f);
                        var textSize = Drawing.MeasureText(text, "Arial", size,
                            FontFlags.AntiAlias | FontFlags.StrikeOut);
                        pos -= new Vector2(textSize.X + 5, 0);
                        Drawing.DrawText(text, pos, size, Color.White,
                            FontFlags.AntiAlias | FontFlags.StrikeOut);
                    }
                }
                if (DrawDamageTop)
                {
                    var sizeY = (float) HUDInfo.GetTopPanelSizeY(hero);
                    var pos = HUDInfo.GetTopPanelPosition(hero) +
                              new Vector2(0, sizeY*2);
                    var size = new Vector2(sizeY / 1.5f);
                    /*var textSize = Drawing.MeasureText(text, "Arial", size,
                        FontFlags.AntiAlias | FontFlags.StrikeOut);*/
                    Drawing.DrawText(text, pos, size, Color.White,
                        FontFlags.AntiAlias | FontFlags.StrikeOut);
                }
            }
        }

        public void OnDeactivate()
        {
            Drawing.OnDraw -= DrawingOnOnDraw;
        }

        public MenuItem<bool> Enable { get; set; }
    }
}