using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using SharpDX;
using Techies_Annihilation.Features;
using Techies_Annihilation.Utils;

namespace Techies_Annihilation.BombFolder
{
    public class Stacker
    {
        public bool IsActive => Counter > 0;
        public int Counter;
        public Stacker()
        {
            Counter = 0;
        }
    }
    public class BombManager
    {
        private const float StackerRange=200f;
        public readonly Unit Bomb;
        public readonly bool IsRemoteMine;
        public float Damage;
        public float Radius;
        public ParticleEffect RangEffect;
        public bool Active;
        public Vector3 BombPosition;
        public Dictionary<uint, float> HeroManager { get; set; }
        private Enums.BombStatus _status;
        public Stacker Stacker;
        public Enums.BombStatus Status
        {
            get { return _status; }
            set
            {
                StatusStartTIme = Game.RawGameTime;
                _status = value;
                //Printer.Print($"changed to -> {_status} time: {StatusStartTIme}");
            }
        }

        public float GetBombDelay(Hero target)
        {
            float temp;
            return HeroManager.TryGetValue(target.Handle, out temp) ? Game.RawGameTime-temp : 0;
        }

        public float StatusStartTIme;
        public BombManager(Unit bomb, bool isRemoteMine)
        {
            Bomb = bomb;
            IsRemoteMine = isRemoteMine;
            Damage = isRemoteMine
                ? Core.RemoteMine.GetAbilityData("damage", Core.RemoteMine.Level) + (Core.Me.AghanimState() ? 150 : 0)
                : Core.LandMine.GetAbilityData("damage", Core.LandMine.Level);
            Radius= isRemoteMine
                ? Core.RemoteMine.GetAbilityData("radius")
                : Core.LandMine.GetAbilityData("radius");
            RangEffect = new ParticleEffect("materials/ensage_ui/particles/range_display_mod.vpcf", Bomb.Position);
            RangEffect.SetControlPoint(1, new Vector3(Radius, 255, 0));
            RangEffect.SetControlPoint(2,
                bomb.NetworkActivity != NetworkActivity.Spawn
                    ? new Vector3(255, 255, 255)
                    : new Vector3(100, 100, 100));
            BombPosition = bomb.Position;
            Active = bomb.NetworkActivity != NetworkActivity.Spawn;
            HeroManager = new Dictionary<uint, float>();
            //Printer.Print($"Damage: {Damage} || Raduis: {Radius} || Activity: {bomb.NetworkActivity}");
            if (IsRemoteMine)
            {
                Stacker = new Stacker();
                var closest =
                    Core.Bombs.Where(
                        x =>
                            !x.Equals(this) && x.IsRemoteMine && x.Stacker.IsActive &&
                            x.BombPosition.Distance2D(BombPosition) <= StackerRange)
                        .OrderBy(y => y.BombPosition.Distance2D(BombPosition))
                        .FirstOrDefault();
                if (closest != null)
                {
                    closest.Stacker.Counter++;
                    //Printer.Print($"add [{closest.Stacker.Counter}]");
                }
                else
                {
                    Stacker.Counter++;
                    //Printer.Print($"new [{Stacker.Counter}]");
                }
            }
        }

        public void UnStacker()
        {
            if (IsRemoteMine)
            {
                var closest =
                    Core.Bombs.Where(
                        x =>
                            !x.Equals(this) && x.IsRemoteMine && x.Stacker.IsActive &&
                            x.BombPosition.Distance2D(BombPosition) <= StackerRange)
                        .OrderBy(y => y.BombPosition.Distance2D(BombPosition))
                        .FirstOrDefault();
                if (closest != null)
                {
                    closest.Stacker.Counter--;
                }
                else if (Stacker.IsActive)
                {
                    Stacker.Counter--;
                    if (Stacker.IsActive)
                    {
                        closest =
                            Core.Bombs.Where(
                                x =>
                                    !x.Equals(this) && x.IsRemoteMine && !x.Stacker.IsActive &&
                                    x.BombPosition.Distance2D(BombPosition) <= StackerRange)
                                .OrderBy(y => y.BombPosition.Distance2D(BombPosition))
                                .FirstOrDefault();
                        if (closest != null)
                        {
                            RefreshStacker();
                        }
                    }
                }
            }
        }

        private void RefreshStacker()
        {
            foreach (
                var manager in
                    Core.Bombs.Where(
                        manager =>
                            !manager.Equals(this) && manager.IsRemoteMine /*&&
                            manager.BombPosition.Distance2D(BombPosition) <= StackerRange*2 + 50*/))
            {
                manager.Stacker.Counter = 0;
                manager.InitNewStacker(this);
            }
        }


        public void OnUltimateScepter()
        {
            var before = Damage;
            Damage += 150;
            Printer.Print($"[Aghanim] update from {before} to {Damage}");
        }

        public void UpdateDamage()
        {
            var before = Damage;
            Damage = IsRemoteMine
                ? Core.RemoteMine.GetAbilityData("damage", Core.RemoteMine.Level)
                : Core.LandMine.GetAbilityData("damage", Core.LandMine.Level);
            Printer.Print($"update from {before} to {Damage}");
        }

        public bool CanHit(Hero target)
        {
            var handle = target.Handle;

            var targetPos = target.Position;
            if (!(targetPos.Distance2D(BombPosition) <= Radius))
            {
                HeroManager.Remove(handle);
                return false;
            }
            var pred = Prediction.PredictedXYZ(target, 250 + Game.Ping);
            var canHit = pred.Distance2D(BombPosition) <= Radius;
            if (canHit)
            {
                float underHit;
                if (!HeroManager.TryGetValue(handle, out underHit))
                {
                    HeroManager.Add(handle, Game.RawGameTime);
                }
            }
            else
            {
                HeroManager.Remove(handle);
            }
            return canHit;
        }
        public bool CanHit(Vector3 targetPos,Hero target)
        {
            return targetPos.Distance2D(BombPosition) <= Radius;
            /*
            if (!(targetPos.Distance2D(BombPosition) <= Radius)) return false;
            var pred = Prediction.PredictedXYZ(target, 250 + Game.Ping);
            return pred.Distance2D(BombPosition) <= Radius;*/
        }
        public void Detonate()
        {
            if (IsRemoteMine)
                Bomb.Spellbook.Spell1.UseAbility();
        }
    }
}