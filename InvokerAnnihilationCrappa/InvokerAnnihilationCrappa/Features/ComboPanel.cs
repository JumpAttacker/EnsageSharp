using System;
using System.Collections.Generic;
using System.Globalization;
using Ensage;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using Ensage.SDK.Menu;
using InvokerAnnihilationCrappa.Features.behavior;
using SharpDX;

namespace InvokerAnnihilationCrappa.Features
{
    public class ComboPanel : Movable
    {
        private readonly Config _main;
        public List<Combo> Combos;
        public ComboPanel(Config main)
        {
            _main = main;
            var panel = main.Factory.Menu("Combo Panel");
            Enable = panel.Item("Enable", true);
            Movable = panel.Item("Movable", false);
            Size = panel.Item("Size", new Slider(5, 0, 100));
            PosX = panel.Item("Position X", new Slider(500, 0, 2000));
            PosY = panel.Item("Position Y", new Slider(500, 0, 2000));
            IncCombo = panel.Item("Inc Combo", new KeyBind(0x6B));
            DecCombo = panel.Item("Dec Combo", new KeyBind(0x6D));
            CanChangeByClicking = panel.Item("Change combo by clicking on them", true);
            IncCombo.PropertyChanged += (sender, args) =>
            {
                if (IncCombo.Value.Active)
                {
                    _main.Invoker.SelectedCombo++;
                    if (_main.Invoker.SelectedCombo >= Combos.Count)
                        _main.Invoker.SelectedCombo = 0;
                }
            };
            DecCombo.PropertyChanged += (sender, args) =>
            {
                if (DecCombo.Value.Active)
                {
                    _main.Invoker.SelectedCombo--;
                    if (_main.Invoker.SelectedCombo < 0)
                        _main.Invoker.SelectedCombo = Combos.Count-1;
                }
            };
            if (Enable)
            {
                Drawing.OnDraw += DrawingOnOnDraw;
            }
            LoadMovable();
            Combos = new List<Combo>
            {
                new Combo(_main.Invoker,
                    new[] {_main.Invoker.ColdSnap, _main.Invoker.Alacrity, _main.Invoker.ForgeSpirit}),
                new Combo(_main.Invoker,
                    new[]
                    {
                        _main.Invoker.Tornado, _main.Invoker.Emp, _main.Invoker.IceWall, _main.Invoker.Meteor,
                        _main.Invoker.Blast, _main.Invoker.ColdSnap, _main.Invoker.Alacrity, _main.Invoker.ForgeSpirit
                    }),
                new Combo(_main.Invoker,
                    new[]
                    {
                        _main.Invoker.Tornado, _main.Invoker.Emp, _main.Invoker.IceWall, _main.Invoker.ColdSnap,
                        _main.Invoker.Alacrity, _main.Invoker.ForgeSpirit, _main.Invoker.Blast
                    }),
                new Combo(_main.Invoker,
                    new[]
                    {
                        _main.Invoker.Tornado, _main.Invoker.SunStrike, _main.Invoker.Meteor, _main.Invoker.Blast,
                        _main.Invoker.ColdSnap
                    }),
                new Combo(_main.Invoker,
                    new[]
                    {
                        _main.Invoker.Tornado, _main.Invoker.Emp, _main.Invoker.ColdSnap, _main.Invoker.Alacrity,
                        _main.Invoker.ForgeSpirit
                    }),
                new Combo(_main.Invoker,
                    new[]
                    {
                        _main.Invoker.Tornado, _main.Invoker.Meteor, _main.Invoker.Blast, _main.Invoker.ColdSnap,
                        _main.Invoker.ForgeSpirit, _main.Invoker.Alacrity
                    }),
            };
            Enable.Item.ValueChanged += (sender, args) =>
            {
                if (args.GetNewValue<bool>())
                    Drawing.OnDraw += DrawingOnOnDraw;
                else
                    Drawing.OnDraw -= DrawingOnOnDraw;
            };
        }

        public MenuItem<bool> CanChangeByClicking { get; set; }

        public MenuItem<KeyBind> DecCombo { get; set; }

        public MenuItem<KeyBind> IncCombo { get; set; }

        public MenuItem<Slider> Size { get; set; }

        private void DrawingOnOnDraw(EventArgs args)
        {
            Vector2 startPos = new Vector2(PosX.Value.Value, PosY.Value.Value);
            var size = new Vector2(Size * 5 * _main.Invoker.AbilityInfos.Count, Size * 5);
            if (Movable)
            {
                var tempSize = new Vector2(200, 200);
                if (CanMoveWindow(ref startPos, tempSize, true))
                {
                    PosX.Item.SetValue(new Slider((int)startPos.X, 0, 2000));
                    PosY.Item.SetValue(new Slider((int)startPos.Y, 0, 2000));
                    return;
                }
            }
            var pos = startPos;
            var iconSize = new Vector2(Size * 5);
            var selectedComboPos = new Vector2();
            var selectedComboCounter = 0;
            foreach (var combo in Combos)
            {
                var selectedAbility = combo.AbilityInfos[combo.CurrentAbility].Ability;
                var clickStartPos = pos;
                var count = combo.AbilityCount;
                
                foreach (var info in combo.AbilityInfos)
                {
                    var ability = info.Ability;
                    var isItem = info.Ability is Item;
                    if (!_main.AbilitiesInCombo.Value.IsEnabled(ability.Name) && !isItem)
                    {
                        count--;
                        continue;
                    }
                    var texture = isItem
                        ? Textures.GetItemTexture(ability.StoredName())
                        : Textures.GetSpellTexture(ability.StoredName());
                    Drawing.DrawRect(pos, isItem ? new Vector2(iconSize.X * 1.5f, iconSize.Y) : iconSize, texture);
                    var cd = ability.Cooldown;
                    if (cd > 0)
                    {
                        var text = ((int) (cd + 1)).ToString(CultureInfo.InvariantCulture);
                        Drawing.DrawText(
                            text,
                            pos, size / 10,
                            Color.White,
                            FontFlags.AntiAlias | FontFlags.StrikeOut);
                    }
                    if (_main.Invoker.Mode.CanExecute && !Game.IsKeyDown(0x11) && selectedAbility.Equals(ability) &&
                        combo.Id == _main.Invoker.SelectedCombo)
                        Drawing.DrawRect(pos, iconSize, new Color(0, 155, 255, 125));
                    pos += new Vector2(iconSize.X, 0);
                }
                var clickSize = new Vector2(iconSize.X * count, iconSize.Y);
                if (combo.Id == _main.Invoker.SelectedCombo && selectedComboPos.IsZero)
                {
                    selectedComboPos = pos;
                    selectedComboCounter = count;
                }

                combo.Update(clickStartPos, clickSize);

                pos.X = startPos.X;
                pos.Y += iconSize.Y;
            }

            var sizeX = iconSize.X * selectedComboCounter;
            Drawing.DrawRect(selectedComboPos - new Vector2(sizeX, 0) - 1, new Vector2(sizeX, iconSize.Y) + 2,
                new Color(0, 255, 0, 255), true);
        }

        public void OnDeactivate()
        {
            UnloadMovable();
            if (Enable)
                Drawing.OnDraw -= DrawingOnOnDraw;
        }

        public MenuItem<Slider> PosX { get; set; }
        public MenuItem<Slider> PosY { get; set; }
        public MenuItem<bool> Movable { get; set; }
        public MenuItem<bool> Enable { get; set; }
    }
}