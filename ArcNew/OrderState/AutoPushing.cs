using System;
using System.Collections.Generic;
using System.Linq;
using ArcAnnihilation.Manager;
using ArcAnnihilation.Panels;
using ArcAnnihilation.Utils;
using Ensage;
using Ensage.Common.Enums;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using Ensage.Common.Objects.UtilityObjects;
using Ensage.SDK.Extensions;
using Ensage.SDK.Helpers;
using Ensage.SDK.Renderer.Particle;
using SharpDX;
using AbilityId = Ensage.AbilityId;

namespace ArcAnnihilation.OrderState
{
    public class AutoPushing : Order
    {
        public override bool CanBeExecuted => MenuManager.AutoPushingCombo.GetValue<KeyBind>().Active;
        public ParticleManager ParticleManager = new ParticleManager();
        private readonly Sleeper _sleeper;
        //public Lane ClosestLane;
        public Map Map;
        public AutoPushing()
        {
            Map = new Map();
            var isRadiant = ObjectManager.LocalHero.Team == Team.Radiant;
            TopPath = isRadiant ? Map.RadiantTopRoute : Map.DireTopRoute;
            MidPath = isRadiant ? Map.RadiantMiddleRoute : Map.DireMiddleRoute;
            BotPath = isRadiant ? Map.RadiantBottomRoute : Map.DireBottomRoute;
            _sleeper = new Sleeper();
        }

        public List<Vector3> TopPath { get; set; }
        public List<Vector3> MidPath { get; set; }
        public List<Vector3> BotPath { get; set; }
        public string Status;
        public override void Execute()
        {
            if (Core.TempestHero != null && Core.TempestHero.Hero.IsAlive)
            {
                if (Ensage.SDK.Extensions.UnitExtensions.HasModifier(Core.TempestHero.Hero, "modifier_teleporting"))
                    return;
                if (Ensage.SDK.Extensions.UnitExtensions.IsChanneling(Core.TempestHero.Hero))
                    return;
                if (_sleeper.Sleeping)
                    return;
                _sleeper.Sleep(150);
                var currentLane = GetLane(Core.TempestHero.Hero);
                CurrentLane = currentLane;
                var target = Core.TempestHero.Orbwalker.GetTarget();
                if (target==null || target.Position.IsZero || !target.IsAlive)
                {
                    var path = FindOrGetNeededPath(Core.TempestHero.Hero);
                    var lastPoint = path[path.Count - 1];
                    var closest = path.Where(
                            x =>
                                x.Distance2D(lastPoint) < Core.TempestHero.Hero.Position.Distance2D(lastPoint) - 300)
                        .OrderBy(pos => CheckForDist(pos, Core.TempestHero.Hero))
                        .FirstOrDefault();
                    Core.TempestHero.Hero.Move(closest);
                    Status = "Moving";
                    if (MenuManager.UseTravels)
                    {
                        var travels = Core.TempestHero.Hero.GetItemById(AbilityId.item_travel_boots) ??
                                      Core.TempestHero.Hero.GetItemById(AbilityId.item_travel_boots_2);
                        if (travels != null && travels.CanBeCasted())
                        {
                            Status = "Moving [?TP]";
                            var temp = path.ToList();
                            temp.Reverse();
                            if (MenuManager.CheckForCreeps)
                            {
                                var enemyCreeps =
                                    EntityManager<Creep>.Entities.Where(
                                        x =>
                                            x.IsValid && x.IsVisible && x.IsAlive &&
                                            x.Team != ObjectManager.LocalHero.Team &&
                                            Map.GetLane(x) == currentLane);
                                Creep creepForTravels = null;

                                var allyCreeps =
                                    EntityManager<Creep>.Entities.Where(
                                        allyCreep => allyCreep.IsValid && allyCreep.IsAlive &&
                                                     allyCreep.Team == ObjectManager.LocalHero.Team &&
                                                     Map.GetLane(allyCreep) == currentLane &&
                                                     allyCreep.HealthPercent() > 0.75).ToList();
                                foreach (var point in temp)
                                {
                                    creepForTravels = allyCreeps.FirstOrDefault(
                                        allyCreep =>
                                            point.IsInRange(allyCreep.Position, 1500) &&
                                            enemyCreeps.Any(z => z.IsInRange(allyCreep, 1500)));
                                    if (creepForTravels != null)
                                        break;
                                }
                                if (creepForTravels != null && creepForTravels.Distance2D(Core.TempestHero.Hero) > 1500)
                                {
                                    var point = path[path.Count - 1];
                                    var distance1 = point.Distance2D(creepForTravels);
                                    var distance2 = point.Distance2D(Core.TempestHero.Hero);

                                    if (distance1 < distance2 || Map.GetLane(Core.TempestHero.Hero) != currentLane)
                                    {
                                        travels.UseAbility(creepForTravels);
                                        Status = "Doing Tp";
                                        _sleeper.Sleep(1000);
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                
                                var finalPos = temp.First();
                                var ally =
                                    EntityManager<Creep>.Entities.Where(
                                            allyCreep => allyCreep.IsValid && allyCreep.IsAlive &&
                                                         allyCreep.Team == ObjectManager.LocalHero.Team &&
                                                         Map.GetLane(allyCreep) == currentLane &&
                                                         allyCreep.HealthPercent() > 0.75)
                                        .OrderBy(y => Ensage.SDK.Extensions.EntityExtensions.Distance2D(y, finalPos))
                                        .FirstOrDefault();
                                var allyTwr =
                                    EntityManager<Tower>.Entities.Where(
                                            allyCreep => allyCreep.IsValid && allyCreep.IsAlive &&
                                                         allyCreep.Team == ObjectManager.LocalHero.Team &&
                                                         Map.GetLane(allyCreep) == currentLane &&
                                                         allyCreep.HealthPercent() > 0.1)
                                        .OrderBy(y => Ensage.SDK.Extensions.EntityExtensions.Distance2D(y, finalPos))
                                        .FirstOrDefault();

                                Unit tpTarget = null;
                                if (ally != null && allyTwr != null)
                                {
                                    var dist1 = finalPos.Distance2D(ally);
                                    var dist2 = finalPos.Distance2D(allyTwr);
                                    
                                    if (dist1 > dist2)
                                    {
                                        tpTarget = allyTwr;
                                    }
                                    else
                                    {
                                        tpTarget = ally;
                                    }
                                }

                                if (tpTarget != null && tpTarget.Distance2D(Core.TempestHero.Hero) > 1500)
                                {
                                    var point = path[path.Count - 1];
                                    var distance1 = point.Distance2D(tpTarget);
                                    var distance2 = point.Distance2D(Core.TempestHero.Hero);
                                    
                                    if (distance1 < distance2 || Map.GetLane(Core.TempestHero.Hero) != currentLane)
                                    {
                                        travels.UseAbility(tpTarget.Position);
                                        _sleeper.Sleep(1000);
                                        Status = "Doing Tp";
                                        return;
                                    }
                                }
                            }
                        }
                    }

                    try
                    {
                        foreach (var illusion in IllusionManager.GetIllusions)
                        {
                            illusion.Hero.Move(closest);
                        }
                        foreach (var necro in NecronomiconManager.GetNecronomicons)
                        {
                            necro.Necr.Move(closest);
                        }
                    }
                    catch (Exception e)
                    {
                        Printer.Both("kek " + e.Message);
                    }
                }
                else
                {
                    Status = $"Pushing{(target is Tower ? " Tower" : "")}";
                    if (Core.TempestHero.Spark.CanBeCasted())
                    {
                        Printer.Log($"[AutoPushing][Spark][{target.Name} ({target.NetworkName})]->{target.Position.PrintVector()}", true);
                        if (!target.Position.IsZero)
                        {
                            Core.TempestHero.Spark.UseAbility(target.Position);
                            Status = "Casting spark";
                            _sleeper.Sleep(500);
                            return;
                        }
                    }

                    var itemForPushing = Core.TempestHero.Hero.GetItemById(ItemId.item_mjollnir);
                    if (itemForPushing != null && itemForPushing.CanBeCasted())
                    {
                        var allyCreep =
                            EntityManager<Creep>.Entities
                                .FirstOrDefault(
                                    x =>
                                         x.IsAlive && x.Team == ObjectManager.LocalHero.Team &&
                                        x.IsInRange(Core.TempestHero.Hero, 500) && x.HealthPercent() <= 0.92 &&
                                        x.IsMelee);
                        if (allyCreep != null)
                        {
                            itemForPushing.UseAbility(allyCreep);
                            _sleeper.Sleep(500);
                        }
                    }
                    itemForPushing = Core.TempestHero.Hero.GetItemById(ItemId.item_manta);

                    if (itemForPushing != null && itemForPushing.CanBeCasted())
                    {
                        itemForPushing.UseAbility();
                        _sleeper.Sleep(500);
                    }

                    itemForPushing = Core.TempestHero.Hero.GetItemById(ItemId.item_necronomicon) ??
                                     Core.TempestHero.Hero.GetItemById(ItemId.item_necronomicon_2) ??
                                     Core.TempestHero.Hero.GetItemById(ItemId.item_necronomicon_3);
                    if (itemForPushing != null && itemForPushing.CanBeCasted())
                    {
                        itemForPushing.UseAbility();
                        _sleeper.Sleep(500);
                    }
                    if (Core.TempestHero.Orbwalker.GetTarget() is Tower)
                    {
                        var field = Core.TempestHero.MagneticField;
                        if (field.CanBeCasted())
                        {
                            var pos =
                            (Core.TempestHero.Orbwalker.GetTarget().NetworkPosition -
                             Core.TempestHero.Hero.NetworkPosition).Normalized();
                            pos *= (280 + 150);
                            pos = Core.TempestHero.Orbwalker.GetTarget().NetworkPosition - pos;
                            field.UseAbility(pos);
                            _sleeper.Sleep(1000);
                        }
                    }
                }
                if (MenuManager.AutoPushingTargetting)
                {
                    var enemyHero =
                        Heroes.GetByTeam(ObjectManager.LocalHero.GetEnemyTeam())
                            .FirstOrDefault(
                                x => x.IsVisible && x.IsAlive && x.IsInRange(Core.TempestHero.Hero,
                                    MenuManager
                                        .AutoPushingTargettingRange) /*Core.TempestHero.Hero.IsValidOrbwalkingTarget(x)*/);
                    if (enemyHero != null)
                    {
                        //OrderManager.ChangeOrder(OrderManager.Orders.SparkSpamTempest);
                        Core.Target = enemyHero;
                        MenuManager.TempestCombo.SetValue(new KeyBind(MenuManager.TempestCombo.GetValue<KeyBind>().Key,
                            KeyBindType.Toggle, true));
                        Core.Target = enemyHero;
                        ParticleManager?.Remove("targetting_range");
                    }
                    else
                    {
                        if (MenuManager.DrawTargettingRange)
                            ParticleManager.DrawRange(Core.TempestHero.Hero, "targetting_range",
                                MenuManager.AutoPushingTargettingRange, Color.White);
                    }
                }
            }
            else
            {
                Status = "Tempest is dead";
                var illusions = IllusionManager.GetIllusions.Where(x => x.Orbwalker.GetTarget() == null).ToList();
                var necros = NecronomiconManager.GetNecronomicons.Where(x => x.Orbwalker.GetTarget() == null).ToList();
                var any = illusions.Any() || necros.Any();
                if (any)
                {
                    foreach (var illusion in illusions)
                    {
                        var path = FindOrGetNeededPath(illusion.Hero);
                        var lastPoint = path[path.Count - 1];
                        var closest = path.Where(
                                x =>
                                    x.Distance2D(lastPoint) < Core.TempestHero.Hero.Position.Distance2D(lastPoint) - 300)
                            .OrderBy(pos => CheckForDist(pos, Core.TempestHero.Hero))
                            .FirstOrDefault();
                        illusion.Hero.Move(closest);
                    }
                    foreach (var necro in necros)
                    {
                        var path = FindOrGetNeededPath(necro.Necr);
                        var lastPoint = path[path.Count - 1];
                        var closest = path.Where(
                                x =>
                                    x.Distance2D(lastPoint) < Core.TempestHero.Hero.Position.Distance2D(lastPoint) - 300)
                            .OrderBy(pos => CheckForDist(pos, Core.TempestHero.Hero))
                            .FirstOrDefault();
                        necro.Necr.Move(closest);
                    }
                    _sleeper.Sleep(500);
                }
            }
        }

        public MapArea CurrentLane { get; set; }

        private MapArea GetLane(Hero hero)
        {
            if (!MenuManager.IsSummmoningAndPushing)
                if (PushLaneSelector.GetInstance().Loaded)
                {
                    var selected = PushLaneSelector.GetInstance().GetSelectedLane;
                    if (!string.IsNullOrEmpty(selected))
                    {
                        selected = selected.Substring(5);
                        if (selected != "Pushing")
                        {
                            if (selected.Equals("Top"))
                            {
                                return MapArea.Top;
                            }
                            if (selected.Equals("Mid"))
                            {
                                return MapArea.Middle;
                            }
                            if (selected.Equals("Bot"))
                            {
                                return MapArea.Bottom;
                            }
                        }
                    }
                }
            var lane = MenuManager.IsSummmoningAndPushing ? GetLane(Game.MousePosition) : Map.GetLane(hero);
            switch (lane)
            {
                case MapArea.Top:
                    return MapArea.Top;
                case MapArea.Middle:
                    return MapArea.Middle;
                case MapArea.Bottom:
                    return MapArea.Bottom;
                case MapArea.DireTopJungle:
                    return MapArea.Top;
                case MapArea.RadiantBottomJungle:
                    return MapArea.Bottom;
                case MapArea.RadiantTopJungle:
                    return MapArea.Top;
                case MapArea.DireBottomJungle:
                    return MapArea.Bottom;
                default:
                    return MapArea.Middle;
            }
        }

        private List<Vector3> FindOrGetNeededPath(Unit hero)
        {
            if (!MenuManager.IsSummmoningAndPushing)
                if (PushLaneSelector.GetInstance().Loaded)
                {
                    var selected = PushLaneSelector.GetInstance().GetSelectedLane;
                    if (!string.IsNullOrEmpty(selected))
                    {
                        selected = selected.Substring(5);
                        if (selected != "Pushing")
                        {
                            if (selected.Equals("Top"))
                            {
                                return TopPath;
                            }
                            if (selected.Equals("Mid"))
                            {
                                return MidPath;
                            }
                            if (selected.Equals("Bot"))
                            {
                                return BotPath;
                            }
                        }
                    }
                }
            
            if (!MenuManager.IsSummmoningAndPushing)
            {
                var currentLane = Map.GetLane(hero);
                switch (currentLane)
                {
                    case MapArea.Top:
                        return TopPath;
                    case MapArea.Middle:
                        return MidPath;
                    case MapArea.Bottom:
                        return BotPath;
                    case MapArea.DireTopJungle:
                        return TopPath;
                    case MapArea.RadiantBottomJungle:
                        return BotPath;
                    case MapArea.RadiantTopJungle:
                        return TopPath;
                    case MapArea.DireBottomJungle:
                        return BotPath;
                    default:
                        return MidPath;
                }
            }
            else
            {
                var currentLane = GetLane(Game.MousePosition);

                switch (currentLane)
                {
                    case MapArea.Top:
                        PushLaneSelector.GetInstance().SetSelectedLane(PushLaneSelector.LanesType.OnlyTop);
                        return TopPath;
                    case MapArea.Middle:
                        PushLaneSelector.GetInstance().SetSelectedLane(PushLaneSelector.LanesType.OnlyMid);
                        return MidPath;
                    case MapArea.Bottom:
                        PushLaneSelector.GetInstance().SetSelectedLane(PushLaneSelector.LanesType.OnlyBot);
                        return BotPath;
                    case MapArea.DireTopJungle:
                        PushLaneSelector.GetInstance().SetSelectedLane(PushLaneSelector.LanesType.OnlyTop);
                        return TopPath;
                    case MapArea.RadiantBottomJungle:
                        PushLaneSelector.GetInstance().SetSelectedLane(PushLaneSelector.LanesType.OnlyBot);
                        return BotPath;
                    case MapArea.RadiantTopJungle:
                        PushLaneSelector.GetInstance().SetSelectedLane(PushLaneSelector.LanesType.OnlyTop);
                        return TopPath;
                    case MapArea.DireBottomJungle:
                        PushLaneSelector.GetInstance().SetSelectedLane(PushLaneSelector.LanesType.OnlyBot);
                        return BotPath;
                    default:
                        PushLaneSelector.GetInstance().SetSelectedLane(PushLaneSelector.LanesType.OnlyMid);
                        return MidPath;
                }
            }
        }

        public MapArea GetLane(Vector3 pos)
        {
            if (Map.Top.IsInside(pos))
            {
                return MapArea.Top;
            }
            if (Map.Middle.IsInside(pos))
            {
                return MapArea.Middle;
            }
            if (Map.Bottom.IsInside(pos))
            {
                return MapArea.Bottom;
            }
            if (Map.River.IsInside(pos))
            {
                return MapArea.River;
            }
            if (Map.RadiantBase.IsInside(pos))
            {
                return MapArea.RadiantBase;
            }
            if (Map.DireBase.IsInside(pos))
            {
                return MapArea.DireBase;
            }
            if (Map.Roshan.IsInside(pos))
            {
                return MapArea.RoshanPit;
            }
            if (Map.DireBottomJungle.IsInside(pos))
            {
                return MapArea.DireBottomJungle;
            }
            if (Map.DireTopJungle.IsInside(pos))
            {
                return MapArea.DireTopJungle;
            }
            if (Map.RadiantBottomJungle.IsInside(pos))
            {
                return MapArea.RadiantBottomJungle;
            }
            if (Map.RadiantTopJungle.IsInside(pos))
            {
                return MapArea.RadiantTopJungle;
            }

            return MapArea.Unknown;
        }

        /*public override void Execute()
        {
            if (Core.TempestHero != null && Core.TempestHero.Hero.IsAlive)
            {
                if (_sleeper.Sleeping)
                    return;
                if (Core.TempestHero.Orbwalker.GetTarget() == null || Core.TempestHero.Orbwalker.GetTarget().Position.IsZero)
                {
                    ClosestLane = GetClosestLane(Core.TempestHero.Hero);
                    var lastPoint = ClosestLane.Points[ClosestLane.Points.Count - 1];
                    ClosestLane.ClosestPosition =
                        ClosestLane.Points.Where(
                                x =>
                                    x.Distance2D(lastPoint) < Core.TempestHero.Hero.Position.Distance2D(lastPoint) - 300)
                            .OrderBy(pos => CheckForDist(pos, Core.TempestHero.Hero))
                            .FirstOrDefault();

                    if (ClosestLane.ClosestPosition != null && !ClosestLane.ClosestPosition.IsZero)
                    {
                        if (MenuManager.UseTravels)
                        {
                            var travels = Core.TempestHero.Hero.GetItemById(AbilityId.item_travel_boots) ??
                                          Core.TempestHero.Hero.GetItemById(AbilityId.item_travel_boots_2);
                            if (travels != null && travels.CanBeCasted() && !_sleeper.Sleeping)
                            {
                                var temp = ClosestLane.Points.ToList();
                                temp.Reverse();
                                var enemyCreeps =
                                    EntityManager<Creep>.Entities.Where(x => x.IsValid && x.IsVisible && x.IsAlive && x.Team != ObjectManager.LocalHero.Team);
                                Creep creepForTravels = null;

                                foreach (var point in temp)
                                {
                                    creepForTravels = EntityManager<Creep>.Entities.FirstOrDefault(
                                            allyCreep =>
                                                allyCreep.IsValid && allyCreep.IsAlive && allyCreep.Team == ObjectManager.LocalHero.Team && 
                                                allyCreep.HealthPercent() > 0.75 &&
                                                point.IsInRange(allyCreep.Position, 1500) &&
                                                enemyCreeps.Any(z => z.IsInRange(allyCreep, 1500)));
                                    if (creepForTravels != null)
                                        break;
                                }
                                if (creepForTravels != null && creepForTravels.Distance2D(Core.TempestHero.Hero) > 1500)
                                {
                                    travels.UseAbility(creepForTravels);
                                    _sleeper.Sleep(500);
                                }
                            }
                        }
                        
                        
                        Core.TempestHero.Hero.Move(ClosestLane.ClosestPosition);
                        try
                        {
                            foreach (var illusion in IllusionManager.GetIllusions)
                            {
                                illusion.Hero.Move(ClosestLane.ClosestPosition);
                            }
                            foreach (var necro in NecronomiconManager.GetNecronomicons)
                            {
                                necro.Necr.Move(ClosestLane.ClosestPosition);
                            }
                        }
                        catch (Exception e)
                        {
                            Printer.Both("kek "+e.Message);
                        }
                        ClosestPosition = ClosestLane.ClosestPosition;
                    }
                    
                }
                else if (!_sleeper.Sleeping)
                {
                    if (Core.TempestHero.Spark.CanBeCasted())
                    {
                        var target = Core.TempestHero.Orbwalker.GetTarget();
                        if (target != null)
                        {
                            Printer.Log($"[AutoPushing][Spark][{target.Name}]->{target.Position.PrintVector()}", true);
                            if (!target.Position.IsZero)
                            {
                                Core.TempestHero.Spark.UseAbility(target.Position);
                                _sleeper.Sleep(500);
                            }
                        }
                    }

                    var itemForPushing = Core.TempestHero.Hero.GetItemById(ItemId.item_mjollnir);
                    if (itemForPushing != null && itemForPushing.CanBeCasted())
                    {
                        var allyCreep =
                            EntityManager<Creep>.Entities
                                .FirstOrDefault(
                                    x =>
                                         x.IsAlive && x.Team == ObjectManager.LocalHero.Team &&
                                        x.IsInRange(Core.TempestHero.Hero, 500) && x.HealthPercent() <= 0.92 &&
                                        x.IsMelee);
                        if (allyCreep != null)
                        {
                            itemForPushing.UseAbility(allyCreep);
                            _sleeper.Sleep(500);
                        }
                    }
                    itemForPushing = Core.TempestHero.Hero.GetItemById(ItemId.item_manta);

                    if (itemForPushing != null && itemForPushing.CanBeCasted())
                    {
                        itemForPushing.UseAbility();
                        _sleeper.Sleep(500);
                    }

                    itemForPushing = Core.TempestHero.Hero.GetItemById(ItemId.item_necronomicon) ??
                                     Core.TempestHero.Hero.GetItemById(ItemId.item_necronomicon_2) ??
                                     Core.TempestHero.Hero.GetItemById(ItemId.item_necronomicon_3);
                    if (itemForPushing != null && itemForPushing.CanBeCasted())
                    {
                        itemForPushing.UseAbility();
                        _sleeper.Sleep(500);
                    }
                    if (Core.TempestHero.Orbwalker.GetTarget() is Tower)
                    {
                        var field = Core.TempestHero.MagneticField;
                        if (field.CanBeCasted())
                        {
                            var pos =
                            (Core.TempestHero.Orbwalker.GetTarget().NetworkPosition -
                             Core.TempestHero.Hero.NetworkPosition).Normalized();
                            pos *= (280 + 150);
                            pos = Core.TempestHero.Orbwalker.GetTarget().NetworkPosition - pos;
                            field.UseAbility(pos);
                            _sleeper.Sleep(1000);
                        }
                    }
                }
                if (MenuManager.AutoPushingTargetting)
                {
                    var enemyHero =
                        Heroes.GetByTeam(ObjectManager.LocalHero.GetEnemyTeam())
                            .FirstOrDefault(x => Core.TempestHero.Hero.IsValidOrbwalkingTarget(x));
                    if (enemyHero != null)
                    {
                        //OrderManager.ChangeOrder(OrderManager.Orders.SparkSpamTempest);
                        Core.Target = enemyHero;
                        MenuManager.TempestCombo.SetValue(new KeyBind(MenuManager.TempestCombo.GetValue<KeyBind>().Key,
                            KeyBindType.Toggle, true));
                        Core.Target = enemyHero;
                    }
                }
                _sleeper.Sleep(250);
            }
            else
            {
                if (_sleeper.Sleeping)
                    return;
                _sleeper.Sleep(400);
                foreach (var illusion in IllusionManager.GetIllusions.Where(x => x.Orbwalker.GetTarget() == null))
                {
                    ClosestLane = GetClosestLane(illusion.Hero);
                    var pushPos = ClosestLane.Points.LastOrDefault();
                    illusion.Hero.Move(pushPos);
                }
                foreach (var necro in NecronomiconManager.GetNecronomicons.Where(x => x.Orbwalker.GetTarget() == null))
                {
                    ClosestLane = GetClosestLane(necro.Necr);
                    var pushPos = ClosestLane.Points.LastOrDefault();
                    necro.Necr.Move(pushPos);
                }
            }
        }*/

        /*private Lane GetClosestLane(Unit hero)
        {
            if (PushLaneSelector.GetInstance().Loaded)
            {
                var selected = PushLaneSelector.GetInstance().GetSelectedLane;
                if (!string.IsNullOrEmpty(selected))
                {
                    selected = selected.Substring(5);
                    if (selected != "Pushing")
                    {
                        if (selected.Equals(TopLane.Name))
                        {
                            return TopLane;
                        }
                        if (selected.Equals(MidLane.Name))
                        {
                            return MidLane;
                        }
                        if (selected.Equals(BotLane.Name))
                        {
                            return BotLane;
                        }
                    }
                }
            }
            var pos = hero.Position;
            var top =
                TopLane.Points.OrderBy(x => pos.Distance2D(x)).FirstOrDefault();
            var mid =
                MidLane.Points.OrderBy(x => pos.Distance2D(x)).FirstOrDefault();
            var bot =
                BotLane.Points.OrderBy(x => pos.Distance2D(x)).FirstOrDefault();
            if (CheckForDist(top, hero) < CheckForDist(mid, hero))
                if (CheckForDist(top, hero) < CheckForDist(bot, hero))
                {
                    return TopLane;
                }
                else
                {
                    return BotLane;
                }
            if (CheckForDist(mid, hero) < CheckForDist(bot, hero))
            {
                return MidLane;
            }
            return BotLane;
        }*/

        private float CheckForDist(Vector3 pos, Unit hero)
        {
            return pos.Distance2D(hero.Position);
        }

        public string GetStatus()
        {
            return Status;
        }
    }
}