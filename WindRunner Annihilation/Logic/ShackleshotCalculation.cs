using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using SharpDX;

namespace WindRunner_Annihilation.Logic
{
    public static class ShackleshotCalculation
    {
        public static Unit Target;
        private static LineHelpers _rangeEfeectMaster;
        private static DebugLines _debugLines;
        private static ParticleEffect _targetRange;

        private static bool DrawRange => Members.Menu.Item("Range.ShackleAoE.Enable").GetValue<bool>();
        public static void OnCalc(EventArgs args)
        {
            if (_rangeEfeectMaster == null)
                _rangeEfeectMaster = new LineHelpers();
            if (_debugLines == null)
                _debugLines = new DebugLines();

            var tempTarget = TargetSelector.ClosestToMouse(Members.MyHero);
            if ((tempTarget == null && Target != null) || (tempTarget!=null && Target!=null && !tempTarget.Equals(Target)))
            {
                if (Target != null)
                {
                    var i = 0;
                    while (_debugLines.Dispose(Target))
                    {
                        Printer.Print($"[Dispose]# {++i}");
                    }
                }
                if (_targetRange != null)
                {
                    _targetRange.Dispose();
                    _targetRange = null;
                }
            }
            Target = tempTarget;
            if (Target==null)
                return;
            if (_targetRange == null)
            {
                if (DrawRange)
                {
                    _targetRange =
                        Target.AddParticleEffect("materials/ensage_ui/particles/range_display_mod.vpcf");
                    _targetRange.SetControlPoint(1, new Vector3(575, 255, 0));
                    _targetRange.SetControlPoint(2, new Vector3(75, 75, 75));
                }
            }
            var trees = ObjectManager.GetEntities<Entity>()
                .Where(x => x.Name == "ent_dota_tree" && x.IsAlive && x.IsValid && x.Distance2D(Target.Position) < 575 && x.IsAlive).ToList();

            var units = ObjectManager.GetEntities<Unit>()
                .Where(
                    x =>
                        !Target.Equals(x) && x.Distance2D(Target.Position) < 575 && x.IsAlive &&
                        x.Team == Members.MyHero.GetEnemyTeam() && x.IsVisible).ToList();

            //trees.AddRange(units);
            var list=new List<Vector3>();
            foreach (var entity in trees)
            {
                var angle = (float)Math.Max(
                    Math.Abs(Members.MyHero.FindAngleBetween(entity.Position, true) -
                             Members.MyHero.FindAngleBetween(Target.Position, true)) - .21, 0);
                _rangeEfeectMaster.DrawEffect(entity, "materials/ensage_ui/particles/line.vpcf", angle == 0);
                _debugLines.DrawEffect(entity, "materials/ensage_ui/particles/line.vpcf", angle == 0);

                var ang = Target.FindAngleBetween(entity.Position, true);
                for (var i = 1; i < 16; i++)
                {
                    var tempPos = Target.Position -
                          new Vector3((float)(i * 50 * Math.Cos(ang)), (float)(i * 50 * Math.Sin(ang)), 0);
                    if (NavMesh.GetCellFlags(tempPos) == NavMeshCellFlags.Walkable)
                        list.Add(tempPos);
                }

            }
            
            foreach (var entity in units)
            {
                var angle = (float)Math.Max(
                    Math.Abs(Members.MyHero.FindAngleBetween(entity.Position, true) -
                             Members.MyHero.FindAngleBetween(Target.Position, true)) - .21, 0);
                _rangeEfeectMaster.DrawEffect(entity, "materials/ensage_ui/particles/line.vpcf", angle == 0);
                _debugLines.DrawEffect(entity, "materials/ensage_ui/particles/line.vpcf", angle == 0);
                var ang = Target.FindAngleBetween(entity.Position, true);
                for (var i = 1; i < 16; i++)
                {
                    var tempPos = Target.Position -
                          new Vector3((float)(i * 50 * Math.Cos(ang)), (float)(i * 50 * Math.Sin(ang)), 0);
                    if (NavMesh.GetCellFlags(tempPos) == NavMeshCellFlags.Walkable)
                        list.Add(tempPos);
                }
            }
            Members.BestPoinits = list;
        }
    }
}