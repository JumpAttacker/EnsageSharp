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
using SharpDX;
using AbilityId = Ensage.AbilityId;

namespace ArcAnnihilation.OrderState
{
    public class Lane
    {
        public string Name;
        public List<Vector3> Points;
        public Vector3 ClosestPosition;

        public Lane(string name, List<Vector3> points)
        {
            this.Name = name;
            this.Points = points;
            ClosestPosition = Vector3.Zero;
        }
    }

    public class AutoPushing : Order
    {
        #region lanes

        private readonly List<Vector3> _radiantTopLanes = new List<Vector3>
        {
            new Vector3(-6545,-2815,256),
            new Vector3(-6369,-661,384),
            new Vector3(-6318.565f,1398.681f,384.0001f),
            new Vector3(-6295.22f,2728.575f,384),
            new Vector3(-6249.306f,4836.18f,384),
            new Vector3(-5555.483f,5896.666f,384),
            new Vector3(-2762.254f,6007.369f,384),
            new Vector3(-687.4045f,6022.142f,384.0001f),
            new Vector3(1427.135f,5954.591f,384),
            new Vector3(3409.737f,5780.926f,384)
        };
        private readonly List<Vector3> _radiantMidLanes = new List<Vector3>
        {
            new Vector3(-4390,-3877,384),
            new Vector3(-2016,-1663,256),
            new Vector3(368,432,256),
            new Vector3(2724,2070,255),
            new Vector3(4430,3836,384)
        };

        private readonly List<Vector3> _radiantBotLanes = new List<Vector3>
        {
            new Vector3(-4026, -6105, 384),
            new Vector3(-2141, -6183, 256),
            new Vector3(9, -6291, 384),
            new Vector3(2184, -6320, 384),
            new Vector3(4220, -6126, 384),
            new Vector3(5732, -5547, 384),
            new Vector3(6313, -4135, 384),
            new Vector3(6260, -2527, 384),
            new Vector3(6295, -965, 383),
            new Vector3(6337, 492, 384),
            new Vector3(6402, 1906, 256),
            new Vector3(6366, 3378, 383)
        };

        #endregion

        public Vector3 ClosestPosition { get; set; }
        public readonly Lane BotLane;
        public readonly Lane MidLane;
        public readonly Lane TopLane;
        public override bool CanBeExecuted => MenuManager.AutoPushingCombo.GetValue<KeyBind>().Active;
        private readonly Sleeper _sleeper;
        public Lane ClosestLane;

        public AutoPushing()
        {
            if (ObjectManager.LocalHero.Team == Team.Radiant)
            {
                BotLane = new Lane("Bot", _radiantBotLanes);
                MidLane = new Lane("Mid", _radiantMidLanes);
                TopLane = new Lane("Top", _radiantTopLanes);
            }
            else
            {
                _radiantBotLanes.Reverse();
                _radiantMidLanes.Reverse();
                _radiantTopLanes.Reverse();
                BotLane = new Lane("Bot", _radiantBotLanes);
                MidLane = new Lane("Mid", _radiantMidLanes);
                TopLane = new Lane("Top", _radiantTopLanes);
            }
            _sleeper = new Sleeper();
        }


        public override void Execute()
        {
            if (Core.TempestHero != null && Core.TempestHero.Hero.IsAlive)
            {
                if (Core.TempestHero.Orbwalker.GetTarget() == null)
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
                                    CreepManager.GetCreepManager()
                                        .GetCreeps.Where(x => x.Team != ObjectManager.LocalHero.Team);
                                Creep creepForTravels = null;

                                foreach (var v in temp)
                                {
                                    creepForTravels = CreepManager.GetCreepManager()
                                        .GetCreeps.FirstOrDefault(
                                            y =>
                                                y.IsValid && y.HealthPercent() > 0.75 && v.IsInRange(y.Position, 1500) &&
                                                y.Team == ObjectManager.LocalHero.Team &&
                                                enemyCreeps.Any(z => z.IsInRange(y, 1500)));
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
                        if (_sleeper.Sleeping)
                            return;
                        _sleeper.Sleep(250);
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
                        Core.TempestHero.Spark.UseAbility(Core.TempestHero.Orbwalker.GetTarget().Position);
                        _sleeper.Sleep(500);
                    }

                    var itemForPushing = Core.TempestHero.Hero.GetItemById(ItemId.item_mjollnir);
                    if (itemForPushing != null && itemForPushing.CanBeCasted())
                    {
                        var allyCreep =
                            CreepManager.GetCreepManager()
                                .GetCreeps
                                .FirstOrDefault(
                                    x =>
                                        x.Team == ObjectManager.LocalHero.Team &&
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
        }

        private Lane GetClosestLane(Unit hero)
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
        }

        private float CheckForDist(Vector3 pos, Unit hero)
        {
            return pos.Distance2D(hero.Position);
        }
    }
}