using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common;
using Ensage.Common.AbilityInfo;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using Ensage.Common.Objects.UtilityObjects;
using Ensage.Common.Threading;
using SharpDX;
using WindRunner_Annihilation.Logic;

namespace WindRunner_Annihilation
{
    public class Core
    {
        #region LittleMembers

        private static CancellationTokenSource _tks;
        private static Task _testCombo;
        private static readonly MultiSleeper ComboSleeper = new MultiSleeper();
        private static bool IsEnable => Members.Menu.Item("Enable").GetValue<bool>();
        private static bool OrbEnable => Members.Menu.Item("Orbwalking.Enable").GetValue<bool>();
        private static bool OrbFollow => Members.Menu.Item("Orbwalking.FollowTarget").GetValue<bool>();
        private static bool OrbInStun => Members.Menu.Item("Orbwalking.WhileTargetInStun").GetValue<bool>();
        private static bool ComboKey => Members.Menu.Item("Combo.Enable").GetValue<KeyBind>().Active;

        #endregion

        #region Constructor

        public Core()
        {
            MenuManager.Init();

            
            
            Members.MyHero = ObjectManager.LocalHero;
            Members.MyTeam = ObjectManager.LocalHero.Team;
            GameDispatcher.OnUpdate += Core.GameDispatcherOnOnUpdate;
            Game.OnUpdate += Core.UpdateItems;
            Game.OnUpdate += ShackleshotCalculation.OnCalc;
            Drawing.OnDraw += DrawShackleshot.Draw;
            Members.Updater = new Sleeper();
            Members.BestPoinits = new List<Vector3>();
        }

        #endregion

        #region MainMethod

        public static void OnValueChanged(object sender, OnValueChangeEventArgs args)
        {
            var oldOne = args.GetOldValue<KeyBind>().Active;
            var newOne = args.GetNewValue<KeyBind>().Active;
            if (oldOne == newOne || newOne) return;
            try
            {
                _tks.Cancel();
            }
            catch (Exception)
            {
                // ignored
            }


            /*var oldOne = args.GetOldValue<KeyBind>().Key;
            var newOne = args.GetNewValue<KeyBind>().Key;
            if (oldOne == newOne) return;
            if (_myLittleCombo == null) return;
            _myLittleCombo.Dispose();
            _myLittleCombo = new Combo(ComboFunction, (Key) newOne);
            Printer.Print($"[Key]: {(Key) newOne} | {newOne} | {(int)Key.D}");*/
        }

        private static async Task Action(CancellationToken cancellationToken)
        {
            var target = ShackleshotCalculation.Target;
            if (target == null)
                return;

            await UseBlink(target,cancellationToken);

            //await UseAbility(new Ability(), Target, cancellationToken);

            var inventory =
                Members.MyHero.Inventory.Items.Where(x => x.CanBeCasted() && x.CanHit(target));
            var enumerable = inventory as Item[] ?? inventory.ToArray();

            var linkerBreakers =
                enumerable.Where(
                    x =>
                        Helper.IsItemEnable(x, false) && target.IsLinkensProtected() && !ComboSleeper.Sleeping(x));
            foreach (var item in linkerBreakers)
            {
                await UseItem(item, target, cancellationToken);
            }
            var itemInCombo =
                enumerable.Where(
                    x => Helper.IsItemEnable(x) && !target.IsLinkensProtected() && !ComboSleeper.Sleeping(x));
            foreach (var item in itemInCombo)
            {
                await UseItem(item, target, cancellationToken);
            }
            
            /*var myItem = inventory.FirstOrDefault();
            if (myItem != null)
            {
                await UseItem(myItem, target, cancellationToken);
            }*/
            if (OrbEnable && (!target.IsStunned() || !OrbInStun))
            {
                Orbwalking.Orbwalk(target, followTarget: OrbFollow);
            }
            else if (Utils.SleepCheck("attack_rate"))
            {
                if (!Members.MyHero.IsAttacking())
                {
                    Members.MyHero.Attack(target);
                    Utils.Sleep(125, "attack_rate");
                }
            }
        }

        private static async Task UseBlink(Unit target, CancellationToken cancellationToken)
        {
            while (true)
            {
                var position = Helper.GetBestPositionForStun();
                if (!position.IsZero)
                {
                    var blink = Members.MyHero.FindItem("item_blink");
                    var stun = Members.MyHero.FindSpell(Members.AbilityList[0]);
                    var ult = Members.MyHero.FindSpell(Members.AbilityList[3]);
                    if (blink != null && blink.CanBeCasted())
                    {
                        var dist = Members.MyHero.Distance2D(position);
                        if (dist <= 1150)
                        {
                            blink.UseAbility(position);
                            await Task.Delay(50, cancellationToken);
                        }
                        else
                        {
                            Members.MyHero.Move(position);
                            await Task.Delay(125, cancellationToken);
                            continue;
                        }
                    }
                    if (stun != null && stun.CanBeCasted() && !target.IsLinkensProtected())
                    {
                        if (ult.CanHit(target))
                        {
                            stun.UseAbility(target);
                            await Task.Delay((int) (150 + Game.Ping), cancellationToken);
                        }
                    }
                    if (ult.CanBeCasted() && !target.IsLinkensProtected())
                    {
                        if (ult.CanHit(target))
                        {
                            ult.UseAbility(target);
                            await Task.Delay(500, cancellationToken);
                        }
                        else
                        {
                            Members.MyHero.Move(target.Position);
                            await Task.Delay(125, cancellationToken);
                            continue;
                        }
                    }
                }

                break;
            }
        }

        #endregion

        #region Helpers

        private static async Task UseItem(Item ability, Unit target, CancellationToken cancellationToken,
            int extraDelay = 0)
        {
            ComboSleeper.Sleep(250, ability);
            ability.UseAbility(target);
            Printer.Print($"[{(int) Game.RawGameTime}] [Item] {ability.Name}: {50}");
            await Task.Delay(10, cancellationToken);
        }

        private static async Task UseAbility(Ability ability, Unit target, CancellationToken cancellationToken,
            int extraDelay = 0)
        {
            if (ability.CanBeCasted() && ability.CanHit(target) && Helper.IsAbilityEnable(ability.StoredName()))
            {
                var cPont = ability.GetCastDelay(Members.MyHero, target, true)*1000 + 250;

                switch (ability.StoredName())
                {
                    case "riki_smoke_screen":
                        ability.UseAbility(Prediction.PredictedXYZ(target, (float) cPont));
                        break;
                    default:
                        ability.UseAbility(target);
                        break;

                }
                Printer.Print($"[{(int) Game.RawGameTime}] [Ability] {ability.Name}: {cPont}");
                await Task.Delay((int) cPont, cancellationToken);
            }
            else
            {
                //Printer.Print($"[CANT]: ({ability.Name})");
            }
        }

        #endregion

        #region Updaters

        public static async void GameDispatcherOnOnUpdate(EventArgs args)
        {
            if (!IsEnable)
                return;
            if (_testCombo != null && !_testCombo.IsCompleted)
                return;
            if (ComboKey)
            {
                _tks = new CancellationTokenSource();
                _testCombo = Action(_tks.Token);
                try
                {
                    await _testCombo;
                }
                catch (OperationCanceledException e)
                {
                    Printer.Print("cancel");
                }
            }
            /*if (_myLittleCombo != null)
            {
                await _myLittleCombo.Execute();
            }*/
        }

        public static void UpdateItems(EventArgs args)
        {
            if (!IsEnable)
                return;
            if (Members.Updater.Sleeping)
                return;
            Members.Updater.Sleep(500);
            var inventory = Members.MyHero.Inventory.Items;
            var enumerable = inventory as IList<Item> ?? inventory.ToList();
            var neededItems = enumerable.Where(item => !Members.BlackList.Contains(item.StoredName()) && !Members.Items.Contains(item.StoredName()) &&
                                                       (item.IsDisable() || item.IsNuke() || item.IsPurge()
                                                        || item.IsSilence() || 
                                                        item.IsSlow() || item.IsSkillShot() ||
                                                        Members.WhiteList.Contains(item.StoredName())));
            foreach (var item in neededItems)
            {
                Members.Items.Add(item.StoredName());
                Members.Menu.Item("itemEnable")
                    .GetValue<AbilityToggler>().Add(item.StoredName());
                Members.Menu.Item("itemEnableLinken")
                    .GetValue<AbilityToggler>().Add(item.StoredName());
                Printer.Print($"[NewItem]: {item.StoredName()}");

            }
            var tempList = enumerable.Select(neededItem => neededItem.StoredName()).ToList();
            var removeList = new List<string>();
            foreach (var item in Members.Items.Where(x => !tempList.Contains(x)))
            {
                Members.Menu.Item("itemEnable")
                    .GetValue<AbilityToggler>().Remove(item);
                Members.Menu.Item("itemEnableLinken")
                    .GetValue<AbilityToggler>().Remove(item);
                removeList.Add(item);
                Printer.Print($"[RemoveItem]: {item}");
            }
            foreach (var item in removeList)
            {
                Members.Items.Remove(item);
            }
        }

        #endregion
    }
}