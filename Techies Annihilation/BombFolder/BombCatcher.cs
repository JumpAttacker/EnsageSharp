using System.Linq;
using Ensage;
using Ensage.Common.Extensions;
using SharpDX;
using Techies_Annihilation.Features;
using Techies_Annihilation.Utils;

namespace Techies_Annihilation.BombFolder
{
    internal class BombCatcher
    {
        public static void Init(Unit bomb, bool isRemoteMine)
        {
            if (Core.Bombs.Find(x => x.Bomb.Equals(bomb)) != null)
            {
                //Printer.Both("already here: " + bomb.Handle);
                return;
            }
            //Printer.Both($"Added: {bomb.Handle} -> isRemoteMine: {isRemoteMine}");
            Core.Bombs.Add(new BombManager(bomb, isRemoteMine));
        }

        public static void Remove(Unit bomb)
        {
            var remove = Core.Bombs.Find(x => x.Bomb.Equals(bomb));
            if (remove != null)
            {
                Core.Bombs.Remove(remove);
                remove.RangEffect.Dispose();
                //Printer.Both("Removed: "+remove.Bomb.Handle);
            }
            else
            {
                //Printer.Both("cant find in system");
            }
        }

        public static void OnAddEntity(EntityEventArgs args)
        {
            var entity = args.Entity;
            if (entity.Team == Core.Me.Team && entity.ClassId == ClassId.CDOTA_NPC_TechiesMines)
            {
                var isUltimate = entity.Spellbook().Spell1 != null;
                Init((Unit)entity, isUltimate);
            }
        }

        public static void OnRemoveEntity(EntityEventArgs args)
        {
            var entity = args.Entity;
            if (entity.Team == Core.Me.Team && entity.ClassId == ClassId.CDOTA_NPC_TechiesMines)
            {
                Remove((Unit)entity);
            }
        }

        public static void Update()
        {
            var bombs =
                ObjectManager.GetEntitiesFast<Unit>()
                    .Where(x => x.Team == Core.Me.Team && x.ClassId == ClassId.CDOTA_NPC_TechiesMines)
                    .ToList();
            foreach (var bomb in bombs)
            {
                var isUltimate = bomb.Spellbook().Spell1 != null;
                Init(bomb, isUltimate);
            }
        }

        public static void OnInt32Change(Entity sender, Int32PropertyChangeEventArgs args)
        {
            var x = sender;
            //if (sender is Hero)Printer.Print($"{args.PropertyName}: {args.OldValue} -> {args.NewValue}");
            if (x.Team == Core.Me.Team && x.ClassId == ClassId.CDOTA_NPC_TechiesMines)
            {
                //Printer.Print($"{args.PropertyName}: {args.OldValue} -> {args.NewValue}");
                if (args.PropertyName == "m_iHealth")
                {
                    if (args.NewValue == 0)
                    {
                        Remove((Unit)x);
                    }
                }
                else if (args.PropertyName == "m_NetworkActivity")
                {
                    if (sender.Spellbook().Spell1 != null)
                    {
                        if (args.NewValue == 1500)
                        {
                            var update = Core.Bombs.Find(z => z.Bomb.Equals(x));
                            if (update != null)
                            {
                                update.Active = true;
                                update.RangEffect.SetControlPoint(2, new Vector3(255, 255, 255));
                            }
                        }
                        else
                        {
                            var update = Core.Bombs.Find(z => z.Bomb.Equals(x));
                            if (update != null)
                            {
                                update.Active = false;
                            }
                        }
                    }
                    else
                    {
                        if (args.NewValue == 1500)
                        {
                            var update = Core.Bombs.Find(z => z.Bomb.Equals(x));
                            if (update != null)
                            {
                                update.RangEffect.SetControlPoint(2, new Vector3(255, 255, 255));
                                update.Active = true;
                                if (update.Bomb.IsVisibleToEnemies)
                                    update.Status = Enums.BombStatus.WillDetonate;
                            }
                        }
                        else
                        {
                            var update = Core.Bombs.Find(z => z.Bomb.Equals(x));
                            if (update != null)
                            {
                                update.Active = false;
                            }
                        }
                    }
                }
                else if (args.PropertyName== "m_iTaggedAsVisibleByTeam")
                {
                    var update = Core.Bombs.Find(z => z.Bomb.Equals(x));
                    if (update != null)
                    {
                        update.Status = (Enums.BombStatus) args.NewValue;
                    }
                }
            }
        }
    }
}