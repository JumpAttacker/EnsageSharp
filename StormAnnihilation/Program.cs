using System;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;

namespace StormAnnihilation
{
    internal class Program
    {
        #region Members

        private static bool _loaded;
        public static bool GoAction { get; private set; }
        private const string Ver = "0.1";
        private const int WmKeyup = 0x0101;
        private static readonly int[] TravelSpeeds = { 1250, 1875, 2500 };
        private static readonly double[] DamagePerUnit = { 0.08, 0.12, 0.16 };
        private static int _totalDamage;
        private static float _remainingMana;
        public static Hero EnemyTargetHero { get; private set; }

        #endregion

        #region Main

        private static void Main()
        {
            #region Init

            _loaded = false;
            Game.OnUpdate += Game_OnUpdate;
            //Drawing.OnDraw += Drawing_OnDraw;
            Game.OnWndProc += Game_OnWndProc;
            
            #endregion
        }

        #endregion

        #region Methods
        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (args.WParam != 'F' || Game.IsChatOpen)
                return;
            GoAction = args.Msg != WmKeyup;
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
            EnemyTargetHero = ClosestToMouse(me);
            if (EnemyTargetHero == null) return;
            if (!GoAction) return;

            var zip = me.Spellbook.Spell4;
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
                        zip.UseAbility(Prediction.SkillShotXYZ(me, EnemyTargetHero, (float)zip.GetCastPoint(), travelSpeed,
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

            _totalDamage = (int)(damage * distance);

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
