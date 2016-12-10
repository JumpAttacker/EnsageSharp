using System;
using System.Threading;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using SharpDX;

namespace TinkerAnnihilation
{
    internal class ReamBlink
    {
        private static bool ComboKey => Members.Menu.Item("RearmBlink.Enable").GetValue<KeyBind>().Active;
        private static bool IsEnable => Members.Menu.Item("Enable").GetValue<bool>();
        private static int ExtraDelay => Members.Menu.Item("RearmBlink.ExtraDelay").GetValue<Slider>().Value;
        private static ParticleEffect _effect;
        private static CancellationTokenSource _tks;
        private static Task _testCombo;
        private static readonly int[] RearmTime = {3000, 1500, 750};

        private static int GetRearmTime(Ability s) => RearmTime[s.Level - 1];

        public static void OnValueChanged(object sender, OnValueChangeEventArgs args)
        {
            var oldOne = args.GetOldValue<KeyBind>().Active;
            var newOne = args.GetNewValue<KeyBind>().Active;
            if (oldOne == newOne) return;
            if (newOne)
            {
                _effect?.Dispose();
                _effect = Members.MyHero.AddParticleEffect("materials/ensage_ui/particles/line.vpcf");
                var frontPoint = Members.MyHero.InFront(1200);
                _effect.SetControlPoint(1, Members.MyHero.Position);
                _effect.SetControlPoint(2, frontPoint);
                _effect.SetControlPoint(3, new Vector3(255, 50, 0));
                _effect.SetControlPoint(4, new Vector3(255, 255, 255));
            }
            else
                try
                {
                    _tks.Cancel();
                    _effect?.Dispose();
                }
                catch (Exception)
                {
                    // ignored
                }
        }

        public static async void OnUpdate(EventArgs args)
        {
            if (!IsEnable)
                return;

            if (ComboKey)
            {
                if (_effect != null && _effect.IsValid)
                {
                    var frontPoint = Members.MyHero.InFront(1200);
                    _effect.SetControlPoint(1, Members.MyHero.Position);
                    _effect.SetControlPoint(2, frontPoint);
                }
            }

            if (_testCombo != null && !_testCombo.IsCompleted)
            {
                return;
            }
            if (ComboKey)
            {
                _tks = new CancellationTokenSource();
                _testCombo = Action(_tks.Token);
                try
                {
                    await _testCombo;
                    _testCombo = null;
                }
                catch (OperationCanceledException)
                {
                    _testCombo = null;
                }
            }

        }

        private static async Task Action(CancellationToken cancellationToken)
        {
            var rearm = Members.MyHero.Spellbook().SpellR;
            var blink = Members.MyHero.FindItem("item_blink");
            if (rearm.CanBeCasted())
            {
                rearm?.UseAbility();
                var time = (int) (GetRearmTime(rearm) + Game.Ping + ExtraDelay + rearm.FindCastPoint()*1000);
                await Task.Delay(time, cancellationToken);
            }
            blink?.UseAbility(Game.MousePosition);
            await Task.Delay(100, cancellationToken);
        }
    }
}