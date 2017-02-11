using System;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Common.Objects.DrawObjects;
using Ensage.Common.Objects.UtilityObjects;
using MorphlingAnnihilation.Units;
using SharpDX;

namespace MorphlingAnnihilation
{
    internal static class Core
    {
        public static MainHero MainHero;
        public static Replicate Replicate;
        public static Hybrid Hybrid;
        private static Sleeper _updater;
        private static Hero _globalTarget;
        
        public static void Updater(EventArgs args)
        {
            if (!MenuManager.IsEnable)
                return;
            if (MainHero == null)
                return;
            if (MenuManager.AllComboKey && MenuManager.LockTarget)
            {
                if (_globalTarget == null || !_globalTarget.IsValid)
                {
                    _globalTarget = TargetSelector.ClosestToMouse(Members.MainHero);
                }
                    
            }
            
            if (MainHero.IsAlive)
            {
                if (MenuManager.AutoBalance)
                {
                    MainHero.Morph();
                }
                if (MenuManager.SafeTp)
                {
                    MainHero.SafeTp();
                }
                if (MenuManager.HeroComboKey && !MenuManager.AllComboKey)
                {
                    var target = TargetSelector.ClosestToMouse(Members.MainHero);
                    if (target != null)
                        MainHero.DoCombo(target);
                }
                if (MenuManager.AllComboKey)
                {
                    if (MenuManager.LockTarget)
                    {
                        MainHero.DoCombo(_globalTarget);
                    }
                }
            }
            if (Replicate != null && Replicate.IsValid && Replicate.IsAlive)
            {
                if (MenuManager.ReplicateComboKey && !MenuManager.AllComboKey)
                {
                    Replicate.AttackTarget();
                }
                if (MenuManager.AllComboKey)
                {
                    Replicate.AttackTarget(_globalTarget);
                }
            }
            if (Hybrid != null && Hybrid.IsValid && Hybrid.IsAlive)
            {
                if (MenuManager.HybridComboKey && !MenuManager.AllComboKey)
                {
                    Hybrid.DoCombo();
                }
                if (MenuManager.AllComboKey)
                {
                    Hybrid.DoCombo(_globalTarget);
                }
            }
        }

        public static void UpdateLogic(EventArgs args)
        {
            if (!MenuManager.IsEnable)
                return;
            if (_updater==null)
                _updater=new Sleeper();
            if (_updater.Sleeping)
                return;
            _updater.Sleep(500);
            var illus =
                ObjectManager.GetEntities<Hero>().Where(x => x.IsValid && x.IsIllusion && x.IsControllable && x.IsAlive).ToArray();
            var replicate = illus.FirstOrDefault(x => x.HasModifier("modifier_morph_replicate"));
            var hybrid = illus.FirstOrDefault(x => x.HasModifier("modifier_morph_hybrid_special"));

            if (MainHero == null || !MainHero.IsValid)
                if (Members.MainHero != null && Members.MainHero.IsValid)
                    MainHero = new MainHero(Members.MainHero);

            if (Replicate == null || !Replicate.IsValid)
                if (replicate != null)
                    Replicate = new Replicate(replicate);

            if (Hybrid == null || !Hybrid.IsValid)
                if (hybrid != null)
                    Hybrid = new Hybrid(hybrid);
        }
        public static void OnValueChanged(object sender, OnValueChangeEventArgs arg)
        {
            var newValue = arg.GetNewValue<KeyBind>().Active;
            var oldValue = arg.GetOldValue<KeyBind>().Active;
            var menu = sender as MenuItem;
            if (menu==null)
                return;
            var name = menu.Name;
            if (oldValue != newValue && !newValue)
            {
                if (name == "all.hotkey")
                    _globalTarget = null;
                else if (name == "replicate.hotkey")
                    Replicate?.FlushGlobalHero();
                else if (name == "hybrid.hotkey")
                    Hybrid?.FlushGlobalHero();
            }
        }
    }
}