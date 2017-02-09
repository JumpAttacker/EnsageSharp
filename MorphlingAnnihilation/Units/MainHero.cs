using System.Reflection;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.Common.Objects.UtilityObjects;
using log4net;
using MorphlingAnnihilation.Interface;
using PlaySharp.Toolkit.Logging;

namespace MorphlingAnnihilation.Units
{
    public class MainHero : ExtendedUnit
    {
        private static Sleeper _morphSleeper;
        private static Sleeper _safeTpSleeper;
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public override void UseItems(Hero target)
        {

        }

        public MainHero(Hero me) : base(me)
        {
            _morphSleeper = new Sleeper();
            _safeTpSleeper = new Sleeper();
        }

        public void Morph()
        {
            if (_morphSleeper.Sleeping)
                return;
            var toAgi = Me.Spellbook.Spell3;
            var toStr = Me.Spellbook.Spell4;
            var minHp = MenuManager.MorphGetMinHealth;
            var minMp = MenuManager.MorphGetMinMana;
            var curentHp = Me.Health;
            var curentMp = Me.Mana;
            if (toAgi != null && toAgi.CanBeCasted() && minMp <= curentMp)
            {
                if (curentHp < minHp)
                {
                    if (!Me.HasModifier("modifier_morphling_morph_str"))
                    {
                        toStr.ToggleAbility();
                        Log.Debug($"[Enable] [{curentHp} < {minHp}]");
                    }
                }
                else if (Me.HasModifier("modifier_morphling_morph_str"))
                {
                    toStr.ToggleAbility();
                    Log.Debug($"[Disable] [{curentHp} >= {minHp}]");
                }
            }
            _morphSleeper.Sleep(300);
        }

        public void SafeTp()
        {
            if (_safeTpSleeper.Sleeping)
                return;
            var safetp = Me.FindSpell("morphling_morph_replicate");
            var repka = Me.FindSpell("morphling_replicate");
            var minHp = MenuManager.MinHpForSafeTp;
            var curentHp = Me.Health;

            if (safetp != null && repka != null && safetp.CanBeCasted() && minHp > curentHp && repka.Cooldown > 0)
            {
                Log.Debug($"[SafeTP] {minHp}>{curentHp}");
                safetp.UseAbility();
                _safeTpSleeper.Sleep(repka.Cooldown*1000);
            }
        }
    }
}