using Ensage;
using Ensage.Common;
using Techies_Annihilation.BombFolder;
using Techies_Annihilation.Features;
using Techies_Annihilation.Utils;

namespace Techies_Annihilation
{
    internal class Program
    {
        private static void Main()
        {
            Events.OnLoad += (sender, args) =>
            {
                DelayAction.Add(1000, () =>
                {
                    var me = ObjectManager.LocalHero;
                    if (me.ClassId != ClassId.CDOTA_Unit_Hero_Techies)
                        return;
                    MenuManager.Init();

                    Core.Init(me);
                    Game.OnIngameUpdate += Core.OnUpdate;
                    Drawing.OnDraw += DrawHelper.OnDraw;
                    Drawing.OnDraw += BombStatus.OnDraw;
                    Drawing.OnDraw += StackDrawing.OnDraw;
                    ObjectManager.OnAddEntity += BombCatcher.OnAddEntity;
                    ObjectManager.OnRemoveEntity += BombCatcher.OnRemoveEntity;
                    Entity.OnInt32PropertyChange += BombCatcher.OnInt32Change;
                    BombCatcher.Update();
                    BombDamageManager.Init();
                    Game.OnIngameUpdate += ForceStaff.OnUpdate;
                    Printer.Both("Techies loaded!", true);
                    /*foreach (var data in Core.Suicide.AbilitySpecialData)
                    {
                        Printer.Print($"{data.Name} -> {data.Value} -> {data.Count} -> {data.IsSpellDamageValue}");
                    }*/
                });
            };
        }
    }
}