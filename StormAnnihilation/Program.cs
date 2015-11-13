using System;
using System.Linq;
using System.Reflection;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using SharpDX;

namespace StormAnnihilation
{
    internal class Program
    {
        #region Members

        private static bool _loaded;
        public static bool GoAction { get; private set; }
        private static readonly string Ver = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        private const int WmKeyup = 0x0101;
        private static readonly int[] TravelSpeeds = { 1250, 1875, 2500 };
        private static readonly double[] DamagePerUnit = { 0.08, 0.12, 0.16 };
        private static int _totalDamage;
        private static float _remainingMana;
        private static bool _leftMouseIsPress;
        private static bool _timetochange;
        private static ulong _useComboKey='G';
        private static bool _lastStateAction;
        private static bool _showMenu=true;
        private static Vector2 _sizer;
        public static Hero EnemyTargetHero { get; private set; }

        #endregion

        #region Main

        private static void Main()
        {
            #region Init

            _loaded = false;
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnWndProc += Game_OnWndProc;
            
            #endregion
        }

        #endregion

        #region Methods
        private static void Drawing_OnDraw(EventArgs args)
        {
            var player = ObjectMgr.LocalPlayer;
            if (player == null || player.Team == Team.Observer || !_loaded)
            {
                return;
            }
            if (ObjectMgr.LocalHero.ClassID != ClassID.CDOTA_Unit_Hero_StormSpirit) return;
            var startPos = new Vector2(50, 200);
            var maxSize = new Vector2(120, 100);
            if (_showMenu)
            {
                _sizer.X += 4;
                _sizer.Y += 4;
                _sizer.X = Math.Min(_sizer.X, maxSize.X);
                _sizer.Y = Math.Min(_sizer.Y, maxSize.Y);

                Drawing.DrawRect(startPos, _sizer, new Color(0, 155, 255, 100));
                Drawing.DrawRect(startPos, _sizer, new Color(0, 0, 0, 255), true);
                Drawing.DrawRect(startPos + new Vector2(-5, -5), _sizer + new Vector2(10, 10),
                    new Color(0, 0, 0, 255), true);
                DrawButton(startPos + new Vector2(_sizer.X - 20, -20), 20, 20, ref _showMenu, true, Color.Gray,
                    Color.Gray);
                if (!Equals(_sizer, maxSize)) return;
                /*
                DrawButton(startPos + new Vector2(10, 10), 100, 20, ref _shouldUseDagger, true,
                    new Color(0, 200, 150),
                    new Color(200, 0, 0, 100), "Dagger On Start");
                */
                DrawButton(startPos + new Vector2(10, _sizer.Y - 70), 100, 20, ref _timetochange, true,
                    new Color(0, 200, 150),
                    new Color(200, 0, 0, 100), "Change Hotkey");

                Drawing.DrawText(
                    string.Format("Status: [{0}]", GoAction ? "ON" : "OFF"),
                    startPos + new Vector2(10, _sizer.Y - 35), Color.White,
                    FontFlags.AntiAlias | FontFlags.DropShadow);
                Drawing.DrawText(string.Format("ComboKey {0}", (char)_useComboKey),
                    startPos + new Vector2(10, _sizer.Y - 20), Color.White,
                    FontFlags.AntiAlias | FontFlags.DropShadow);
            }
            else
            {
                _sizer.X -= 4;
                _sizer.Y -= 4;
                _sizer.X = Math.Max(_sizer.X, 20);
                _sizer.Y = Math.Max(_sizer.Y, 0);
                Drawing.DrawRect(startPos, _sizer, new Color(0, 155, 255, 100));
                Drawing.DrawRect(startPos, _sizer, new Color(0, 0, 0, 255), true);
                DrawButton(startPos + new Vector2(_sizer.X - 20, -20), 20, 20, ref _showMenu, true, Color.Gray,
                    Color.Gray);
            }
        }
        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (Game.IsChatOpen || !_loaded)
                return;
            if (_timetochange && args.Msg == WmKeyup && args.WParam >= 0x41 && args.WParam <= 0x5A)
            {
                _timetochange = false;
                _useComboKey = args.WParam;
                return;
            }
            
            if (args.WParam == _useComboKey)
            {
                GoAction = args.Msg != WmKeyup;
                if (GoAction != _lastStateAction)
                {
                    _lastStateAction = GoAction;
                    Game.ExecuteCommand(string.Format("dota_player_units_auto_attack_after_spell {0}", GoAction ? 0 : 1));
                }
                if (!GoAction)
                {
                    EnemyTargetHero = null;
                }
            }
            if (args.WParam != 1 || !Utils.SleepCheck("clicker"))
            {
                _leftMouseIsPress = false;
                return;
            }
            _leftMouseIsPress = true;
        }
        private static void Game_OnUpdate(EventArgs args)
        {
            #region check

            var me = ObjectMgr.LocalHero;

            if (!_loaded)
            {

                if (!Game.IsInGame || me == null)
                {
                    return;
                }
                if (me.ClassID == ClassID.CDOTA_Unit_Hero_StormSpirit)
                {
                    _loaded = true;
                    PrintSuccess("> Storm Annihilation loaded! v" + Ver);
                }
            }

            if (!Game.IsInGame || me == null)
            {
                _loaded = false;
                PrintInfo("> Storm Annihilation unLoaded");
                return;
            }

            #endregion

            if (!GoAction) return;
            if (EnemyTargetHero == null || !EnemyTargetHero.IsValid)
            {
                EnemyTargetHero = ClosestToMouse(me, 150);
            }
            if (EnemyTargetHero == null || !EnemyTargetHero.IsValid || !EnemyTargetHero.IsAlive || !me.CanCast()) return;

            var zip = me.Spellbook.Spell4;
            if (zip==null || zip.Level==0) return;
            var inUltimate = me.Modifiers.Any(x => x.Name == "modifier_storm_spirit_ball_lightning");
            var inPassve = me.Modifiers.Any(x => x.Name == "modifier_storm_spirit_overload");
            var zipLevel = zip.Level;
            var distance = me.Distance2D(EnemyTargetHero);
            var travelSpeed = TravelSpeeds[zipLevel - 1];
            var damage = DamagePerUnit[zipLevel - 1];
            var damageRadius = 50 + 75 * zipLevel;
            var startManaCost = 30 + me.MaximumMana * 0.08;
            var costPerUnit = (12 + me.MaximumMana * 0.007) / 100.0;

            var totalCost = (int)(startManaCost + costPerUnit * (int)Math.Floor((decimal)distance / 100) * 100);

            var travelTime = distance / travelSpeed;

            var enemyHealth = EnemyTargetHero.Health;
            _remainingMana = me.Mana - totalCost + (me.ManaRegeneration * (travelTime + 1));

            var hpPerc = me.Health / (float)me.MaximumHealth;
            var mpPerc = me.Mana / me.MaximumMana;
            if (Utils.SleepCheck("mana_items"))
            {
                var soulring = me.FindItem("item_soul_ring");
                if (soulring != null && soulring.CanBeCasted() && hpPerc >= .1 && mpPerc <= .7)
                {
                    soulring.UseAbility();
                    Utils.Sleep(200, "mana_items");
                }
                var stick = me.FindItem("item_magic_stick") ?? me.FindItem("item_magic_wand");
                if (stick != null && stick.CanBeCasted() && stick.CurrentCharges != 0 && (mpPerc <= .5 || hpPerc <= .5))
                {
                    stick.UseAbility();
                    Utils.Sleep(200, "mana_items");
                }
            }

            #region Ultimate and Attack

            if (!inUltimate && Utils.SleepCheck("castUlt"))
            {
                if (inPassve && distance < me.AttackRange)
                {
                    if (Utils.SleepCheck("Attacking") && !me.IsAttacking())
                    {
                        me.Attack(EnemyTargetHero);
                        Utils.Sleep(me.AttackSpeedValue, "Attacking");
                    }
                }
                else if (_remainingMana > 0)
                {
                    if (distance >= damageRadius)
                    {
                        zip.UseAbility(Prediction.SkillShotXYZ(me, EnemyTargetHero, (float)zip.FindCastPoint(), travelSpeed,
                            damageRadius)); //TODO mb more accurate
                        me.Attack(EnemyTargetHero, true);
                    }
                    else
                    {
                        zip.UseAbility(me.Position);
                        me.Attack(EnemyTargetHero, true);
                    }

                    Utils.Sleep(500, "castUlt");
                }
            }

            #endregion

            _totalDamage = (int) (damage * distance);
            _totalDamage = (int) EnemyTargetHero.DamageTaken(_totalDamage, DamageType.Magical, me);

            if (enemyHealth < _totalDamage) return; //target ll die only from ultimate

            #region items

            if (Utils.SleepCheck("items"))
            {
                var hex = me.FindItem("item_sheepstick");
                var orchid = me.FindItem("item_orchid");
                if (hex != null && hex.CanBeCasted(EnemyTargetHero) && !EnemyTargetHero.IsHexed() && !EnemyTargetHero.IsStunned())
                {
                    hex.UseAbility(EnemyTargetHero);
                    Utils.Sleep(250, "items");
                }
                else if (orchid != null && orchid.CanBeCasted(EnemyTargetHero) && !EnemyTargetHero.IsHexed() && !EnemyTargetHero.IsSilenced() &&
                         !EnemyTargetHero.IsStunned())
                {
                    orchid.UseAbility(EnemyTargetHero);
                    Utils.Sleep(250, "items");
                }

                var shiva = me.FindItem("item_shivas_guard");
                if (shiva != null && shiva.CanBeCasted() && distance <= 900)
                {
                    shiva.UseAbility();
                    Utils.Sleep(250, "items");
                }
                var dagon = me.GetDagon();
                /*me.FindItem("item_dagon")
                    ?? me.FindItem("item_dagon_2")
                    ?? me.FindItem("item_dagon_3") ?? me.FindItem("item_dagon_4") ?? me.FindItem("item_dagon_5");*/
                if (dagon != null && dagon.CanBeCasted(EnemyTargetHero) && distance <= dagon.CastRange)
                {
                    dagon.UseAbility(EnemyTargetHero);
                    Utils.Sleep(250, "items");
                }
                if (shiva != null && shiva.CanBeCasted() && distance <= 900)
                {
                    shiva.UseAbility();
                    Utils.Sleep(250, "items");
                }
            }

            #endregion

            #region Spells

            if (Utils.SleepCheck("spells"))
            {
                var remnant = me.Spellbook.Spell1;
                var vortex = me.Spellbook.Spell2;
                if (remnant != null && remnant.CanBeCasted() && distance < 260)
                {
                    remnant.UseAbility();
                    Utils.Sleep(250, "spells");
                }
                else if (vortex != null && vortex.CanBeCasted(EnemyTargetHero) && distance < vortex.CastRange && !EnemyTargetHero.IsHexed() &&
                         !EnemyTargetHero.IsStunned())
                {
                    vortex.UseAbility(EnemyTargetHero);
                    Utils.Sleep(250, "spells");
                }
            }

            #endregion

            #region Dodging

            var mantaMod =
                me.Modifiers.Any(
                    x =>
                        x.Name == "modifier_orchid_malevolence_debuff" || x.Name == "modifier_lina_laguna_blade" ||
                        x.Name == "modifier_pudge_meat_hook" || x.Name == "modifier_skywrath_mage_ancient_seal" ||
                        x.Name == "modifier_lion_finger_of_death");
            if (mantaMod)
            {
                var manta = me.FindItem("item_manta");
                if (manta != null && manta.CanBeCasted() && (Utils.SleepCheck("dispell")))
                {
                    manta.UseAbility();
                    Utils.Sleep(250, "dispell");
                }
            }

            #endregion
        }

/*
        private static void Drawing_OnDraw(EventArgs args)
        {
            var me = ObjectMgr.LocalHero;
            if (!Game.IsInGame || me == null || !_loaded || Game.IsPaused || EnemyTargetHero==null) return;


        }
*/

        #endregion


        #region Helper
        private static Hero ClosestToMouse(Hero source, float range = 1000)
        {
            var mousePosition = Game.MousePosition;
            var enemyHeroes =
                ObjectMgr.GetEntities<Hero>()
                    .Where(
                        x =>
                            x.Team == source.GetEnemyTeam() && !x.IsIllusion && x.IsAlive && x.IsVisible
                            && x.Distance2D(mousePosition) <= range && !x.IsMagicImmune());
            Hero[] closestHero = { null };
            foreach (
                var enemyHero in
                    enemyHeroes.Where(
                        enemyHero =>
                            closestHero[0] == null ||
                            closestHero[0].Distance2D(mousePosition) > enemyHero.Distance2D(mousePosition)))
            {
                closestHero[0] = enemyHero;
            }
            return closestHero[0];
        }
        private static void DrawButton(Vector2 a, float w, float h, ref bool clicked, bool isActive, Color @on, Color off, string drawOnButtonText = "")
        {
            var isIn = Utils.IsUnderRectangle(Game.MouseScreenPosition, a.X, a.Y, w, h);
            if (isActive)
            {
                if (_leftMouseIsPress && Utils.SleepCheck("ClickButtonCd") && isIn)
                {
                    clicked = !clicked;
                    Utils.Sleep(250, "ClickButtonCd");
                }
                var newColor = isIn
                    ? new Color((int)(clicked ? @on.R : off.R), clicked ? @on.G : off.G, clicked ? @on.B : off.B, 150)
                    : clicked ? @on : off;
                Drawing.DrawRect(a, new Vector2(w, h), newColor);
                Drawing.DrawRect(a, new Vector2(w, h), new Color(0, 0, 0, 255), true);
                if (drawOnButtonText != "")
                {
                    Drawing.DrawText(drawOnButtonText, a + new Vector2(10, 2), Color.White,
                    FontFlags.AntiAlias | FontFlags.DropShadow);
                }
            }
            else
            {
                Drawing.DrawRect(a, new Vector2(w, h), Color.Gray);
                Drawing.DrawRect(a, new Vector2(w, h), new Color(0, 0, 0, 255), true);
            }
        }
        private static void PrintInfo(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.White, arguments);
        }

        private static void PrintSuccess(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.Green, arguments);
        }

        // ReSharper disable once UnusedMember.Local
        private static void PrintError(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.Red, arguments);
        }

        private static void PrintEncolored(string text, ConsoleColor color, params object[] arguments)
        {
            var clr = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text, arguments);
            Console.ForegroundColor = clr;
        }

        #endregion


    }
}
