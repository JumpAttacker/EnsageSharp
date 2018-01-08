using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using Ensage.Common.Objects.UtilityObjects;
using Ensage.SDK.Input;
using Ensage.SDK.Menu;
using InvokerAnnihilationCrappa.Features.behavior;
using log4net;
using PlaySharp.Toolkit.Logging;
using SharpDX;
using AbilityId = Ensage.Common.Enums.AbilityId;
using MouseButtons = Ensage.SDK.Input.MouseButtons;
using MouseEventArgs = Ensage.SDK.Input.MouseEventArgs;

namespace InvokerAnnihilationCrappa.Features
{
    public class SmartSphere : Movable
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public SmartSphere(Config config)
        {
            _main = config;
            var panel = config.Factory.Menu("Smart Sphere");
            Enable = panel.Item("Enable", true);
            DrawPanel = panel.Item("Draw Panel", true);
            Movable = panel.Item("Movable", false);
            Size = panel.Item("Size", new Slider(10, 1, 50));
            PosX = panel.Item("Position X", new Slider(500, 0, 2000));
            PosY = panel.Item("Position Y", new Slider(500, 0, 2000));
            Cooldown = panel.Item("Cooldown (ms)", new Slider(500, 0, 2000));
            Input = new InputManager();

            _buttons = new Button[6];
            _buttons[0] = new Button(Textures.GetSpellTexture(_main.Invoker.Quas.Name), true, _main.Invoker.Quas);
            _buttons[1] = new Button(Textures.GetSpellTexture(_main.Invoker.Wex.Name), true, _main.Invoker.Wex);
            _buttons[2] = new Button(Textures.GetSpellTexture(_main.Invoker.Exort.Name), true, _main.Invoker.Exort, true);

            _buttons[3] = new Button(Textures.GetSpellTexture(_main.Invoker.Quas.Name), false, _main.Invoker.Quas, true);
            _buttons[4] = new Button(Textures.GetSpellTexture(_main.Invoker.Wex.Name), false, _main.Invoker.Wex);
            _buttons[5] = new Button(Textures.GetSpellTexture(_main.Invoker.Exort.Name), false, _main.Invoker.Exort);
            _sleeper = new Sleeper();
            if (Enable)
            {
                if (DrawPanel)
                {
                    Drawing.OnDraw += DrawingOnOnDraw;
                    Input.MouseClick += OnMouseClick;
                }
                
                Player.OnExecuteOrder += PlayerOnOnExecuteOrder;
            }
            else
            {
                if (DrawPanel)
                {
                    DrawPanel.Item.SetValue(false);
                }
            }

            LoadMovable(_main.Invoker.Input.Value);
            Enable.Item.ValueChanged += (sender, args) =>
            {
                if (args.GetNewValue<bool>())
                {
                    Player.OnExecuteOrder += PlayerOnOnExecuteOrder;
                }
                else
                {
                    Player.OnExecuteOrder -= PlayerOnOnExecuteOrder;
                }

            };
            DrawPanel.Item.ValueChanged += (sender, args) =>
            {
                if (args.GetNewValue<bool>())
                {
                    Input.MouseClick += OnMouseClick;
                    Drawing.OnDraw += DrawingOnOnDraw;
                }
                else
                {
                    Input.MouseClick -= OnMouseClick;
                    Drawing.OnDraw -= DrawingOnOnDraw;
                }
            };
        }

        public MenuItem<Slider> Cooldown { get; set; }

        public class Button
        {
            public Ability Ability { get; }
            public DotaTexture Texture;
            public Vector2 Position;
            public Vector2 Size;
            public bool UnderMouse;
            public bool Active;
            public bool OnAttack;

            public Button(DotaTexture texture, bool onAttack, Ability ability, bool enable=false)
            {
                Ability = ability;
                Log.Debug("new button -> " + $"{texture.Name} -> for {(onAttack ? "Attack" : "Move")}");
                Texture = texture;
                OnAttack = onAttack;
                Active = enable;
            }
        }

        private void OnMouseClick(object sender, MouseEventArgs e)
        {
            if (e.Buttons != MouseButtons.LeftDown)
                return;
            var button = _buttons.FirstOrDefault(x => x.UnderMouse);
            if (button == null) return;
            button.Active = true;
            foreach (var b in _buttons.Where(x => !x.UnderMouse && button.OnAttack==x.OnAttack))
            {
                b.Active = false;
            }
        }

        public InputManager Input { get; set; }
        private readonly Button[] _buttons;

        public MenuItem<bool> DrawPanel { get; set; }
        private readonly Sleeper _sleeper;
        private void PlayerOnOnExecuteOrder(Player sender, ExecuteOrderEventArgs args)
        {
            if (!args.IsPlayerInput)
                return;
            if (args.Entities.All(x => !ReferenceEquals(x, _main.Invoker.Owner)))
                return;
            if (_main.Invoker.Mode.CanExecute)
                return;
            if (_main.Invoker.Owner.IsInvisible())
                return;
            if (
                _main.Invoker.Owner.HasModifiers(
                    new[] {"modifier_invoker_ghost_walk_self", "modifier_rune_invis", "modifier_invisible"}, false))
                return;
            var order = args.OrderId;
            if (order == OrderId.Ability)
            {
                var ability = args.Ability.GetAbilityId();
                if (ability == AbilityId.invoker_quas ||
                    ability == AbilityId.invoker_wex || ability == AbilityId.invoker_exort)
                    _sleeper.Sleep(Cooldown.Value);
            }
            if (_sleeper.Sleeping)
                return;
            if (order == OrderId.AttackLocation || order == OrderId.AttackTarget)
            {
                if (ObjectManager.LocalHero.IsSilenced())
                    return;
                var ability = _buttons.First(x => x.Active && x.OnAttack).Ability;
                if (ability.Level==0)
                    return;
                switch (ability.GetAbilityId())
                {
                    case AbilityId.invoker_quas:
                        if (_main.Invoker.SpCounter.Q == 3)
                            return;
                        break;
                    case AbilityId.invoker_wex:
                        if (_main.Invoker.SpCounter.W == 3)
                            return;
                        break;
                    case AbilityId.invoker_exort:
                        if (_main.Invoker.SpCounter.E == 3)
                            return;
                        break;
                }
                ability.UseAbility();
                ability.UseAbility();
                ability.UseAbility();
            }
            else if (order == OrderId.MoveLocation || order == OrderId.MoveTarget)
            {
                if (args.Target != null && args.Target.ClassId == ClassId.CDOTA_BaseNPC_Healer)
                    return;
                if (ObjectManager.LocalHero.IsSilenced())
                    return;
                var ability = _buttons.First(x => x.Active && !x.OnAttack).Ability;
                if (ability.Level == 0)
                    return;
                switch (ability.GetAbilityId())
                {
                    case AbilityId.invoker_quas:
                        if (_main.Invoker.SpCounter.Q == 3)
                            return;
                        break;
                    case AbilityId.invoker_wex:
                        if (_main.Invoker.SpCounter.W == 3)
                            return;
                        break;
                    case AbilityId.invoker_exort:
                        if (_main.Invoker.SpCounter.E == 3)
                            return;
                        break;
                }
                ability.UseAbility();
                ability.UseAbility();
                ability.UseAbility();
            }
        }

        private readonly Config _main;

        public MenuItem<Slider> Size { get; set; }

        private void DrawingOnOnDraw(EventArgs args)
        {
            if (!Enable)
                return;
            var startPos = new Vector2(PosX.Value.Value, PosY.Value.Value);
            var text = "On Attacking";
            var tSize = new Vector2(Size);
            var textSize = Drawing.MeasureText($"{text}", "Arial", tSize,
                FontFlags.AntiAlias | FontFlags.StrikeOut);
            if (Movable)
            {
                var tempSize = new Vector2(textSize.X*1.5f, textSize.Y*2);
                if (CanMoveWindow(ref startPos, tempSize, true))
                {
                    PosX.Item.SetValue(new Slider((int)startPos.X, 0, 2000));
                    PosY.Item.SetValue(new Slider((int)startPos.Y, 0, 2000));
                    return;
                }
            }

            var pos = startPos;
            Drawing.DrawRect(pos, textSize + new Vector2(textSize.Y*3, 0), new Color(155, 155, 155, 55));
            Drawing.DrawText(
                text,
                pos, tSize,
                Color.White,
                FontFlags.AntiAlias | FontFlags.StrikeOut);
            var iconSize = new Vector2(textSize.Y);
            DrawButton(pos + new Vector2(textSize.X, 0), iconSize, _buttons[0]);
            var step = new Vector2(iconSize.X, 0);
            DrawButton(pos + new Vector2(textSize.X, 0) + step, iconSize, _buttons[1]);
            DrawButton(pos + new Vector2(textSize.X, 0) + step * 2, iconSize, _buttons[2]);

            text = "On Moving";
            /*tSize = new Vector2(Size);
            textSize = Drawing.MeasureText($"{text}", "Arial", tSize,
                FontFlags.AntiAlias | FontFlags.StrikeOut);*/
            pos = startPos + new Vector2(0, textSize.Y);
            Drawing.DrawRect(pos, textSize + new Vector2(textSize.Y * 3, 0), new Color(155, 155, 155, 55));
            Drawing.DrawText(
                text,
                pos, tSize,
                Color.White,
                FontFlags.AntiAlias | FontFlags.StrikeOut);
            iconSize = new Vector2(textSize.Y);
            DrawButton(pos + new Vector2(textSize.X, 0), iconSize, _buttons[3]);
            step = new Vector2(iconSize.X, 0);
            DrawButton(pos + new Vector2(textSize.X, 0) + step, iconSize, _buttons[4]);
            DrawButton(pos + new Vector2(textSize.X, 0) + step * 2, iconSize, _buttons[5]);
        }

        public void OnDeactivate()
        {
            if (Enable)
            {
                Drawing.OnDraw -= DrawingOnOnDraw;
                Input.MouseClick -= OnMouseClick;
            }
        }

        private static void DrawButton(Vector2 pos,Vector2 tSize, Button button)
        {
            var startPos = pos;
            Drawing.DrawRect(pos, tSize, button.Texture);
            if (Ensage.Common.Utils.IsUnderRectangle(Game.MouseScreenPosition, startPos.X, startPos.Y, tSize.X,
                tSize.Y))
            {
                Drawing.DrawRect(startPos, tSize, new Color(0, 255, 0, 100));
                button.UnderMouse = true;
            }
            else
                button.UnderMouse = false;

            if (button.Active)
                Drawing.DrawRect(startPos, tSize, new Color(0, 255, 0, 255),true);
        }
        public MenuItem<Slider> PosX { get; set; }
        public MenuItem<Slider> PosY { get; set; }
        public MenuItem<bool> Movable { get; set; }
        public MenuItem<bool> Enable { get; set; }
    }
}