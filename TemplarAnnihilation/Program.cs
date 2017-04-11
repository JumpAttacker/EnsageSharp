using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using Ensage.Common.Objects.UtilityObjects;
using SharpDX;

namespace TemplarAnnihilation
{
    
    internal class Program
    {
        private static bool _loaded;
        private static Sleeper _updater;

        private static void Main()
        {
            Members.Menu.AddItem(new MenuItem("Enable", "Enable").SetValue(true));
            var settings = new Menu("Settings", "Settings");

            settings.AddItem(new MenuItem("Range.Enable", "Psi Baldes Helper").SetValue(true)).ValueChanged +=
                (sender, args) =>
                {
                    if (args.GetNewValue<bool>())
                    {
                        DrawRange();
                    }
                    else
                    {
                        if (Members.SpellRange != null && Members.SpellRange.IsValid && !Members.SpellRange.IsDestroyed)
                            Members.SpellRange.Dispose();
                    }
                };
            
            var devolper = new Menu("Developer", "Developer");
            devolper.AddItem(new MenuItem("Dev.Text.enable", "Debug messages").SetValue(false));
            devolper.AddItem(new MenuItem("Dev.Drawing.enable", "Debug drawing").SetValue(false));
            Members.Menu.AddSubMenu(settings);
            Members.Menu.AddSubMenu(devolper);
            _loaded = false;
            Events.OnLoad += (sender, args) =>
            {
                if (_loaded)
                    return;
                Load();
                _loaded = true;
            };
            if (!_loaded && ObjectManager.LocalHero != null &&
                ObjectManager.LocalHero.ClassId == Members.MyClassId && Game.IsInGame)
            {
                Load();
                _loaded = true;
            }

            Events.OnClose += (sender, args) =>
            {
                if (!_loaded)
                    return;
                Members.Menu.RemoveFromMainMenu();
                Game.OnUpdate -= Action.Game_OnUpdate;
                Game.OnUpdate -= UpdateItems;
                Drawing.OnDraw -= Action.OnDrawing;
                _loaded = false;
            };
        }

        private static void Load()
        {
            if (ObjectManager.LocalHero.ClassId != Members.MyClassId)
                return;
            if (Members.MyHero == null || !Members.MyHero.IsValid)
            {
                Members.MyHero = ObjectManager.LocalHero;
                Members.MyTeam = ObjectManager.LocalHero.Team;
            }
            _updater = new Sleeper();
            Orbwalking.Load();
            Members.Items = new List<string>();
            Members.Menu.AddToMainMenu();
            Game.OnUpdate += Action.Game_OnUpdate;
            Drawing.OnDraw += Action.OnDrawing;
            Game.OnUpdate += UpdateItems;
            if (Members.Menu.Item("Range.Enable").GetValue<bool>())
            {
                Printer.Print("fist drawing!");
                DrawRange();
            }
        }

        private static void DrawRange()
        {
            Members.SpellRange =
                Members.MyHero.AddParticleEffect("materials/ensage_ui/particles/range_display_mod.vpcf");
            var range = Members.MyHero.GetAttackRange();
            Members.SpellRange.SetControlPoint(0, Members.MyHero.Position);
            Members.SpellRange.SetControlPoint(1, new Vector3(range, 255, 0));
            Members.SpellRange.SetControlPoint(2, new Vector3(255, 255, 255));
        }

        private static bool IsEnable => Members.Menu.Item("Enable").GetValue<bool>();
        //private static uint _psiBaldeLevel;
        private static uint _oldAttackRange;
        private static void UpdateItems(EventArgs args)
        {
            if (!IsEnable)
                return;
            if (_updater.Sleeping)
                return;
            if (_oldAttackRange == 0)
                _oldAttackRange = (uint) Members.MyHero.GetAttackRange();
            _updater.Sleep(100);
            //var psiBaldeLevel = Abilities.FindAbility("templar_assassin_psi_blades").Level;
            if (/*_psiBaldeLevel != psiBaldeLevel || */_oldAttackRange!= (uint)Members.MyHero.GetAttackRange() || !Members.SpellRange.IsValid || Members.SpellRange.IsDestroyed)
            {
                try
                {
                    Printer.Print(
                        $"[SpellRange][Updater]: ({_oldAttackRange != (uint) Members.MyHero.GetAttackRange()})({!Members.SpellRange.IsValid})({Members.SpellRange.IsDestroyed})");
                }
                catch (Exception)
                {
                    Printer.Print($"[SpellRange][Updater]: (error)");
                }
                var oldValue = Members.Menu.Item("Range.Enable").GetValue<bool>();
                //_psiBaldeLevel = psiBaldeLevel;
                _oldAttackRange = (uint) Members.MyHero.GetAttackRange();
                Members.Menu.Item("Range.Enable").SetValue(!oldValue);
                Members.Menu.Item("Range.Enable").SetValue(oldValue);
                
            }
        }
    }
}
