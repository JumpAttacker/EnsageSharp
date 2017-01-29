using System.Collections.Concurrent;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Enums;
using Ensage.Common.Extensions;
using SharpDX;

namespace Auto_Disable
{
    public static class Helper
    {
        public static int GetAbilityDelay(Unit target, Ability ability)
        {
            return (int)((ability.FindCastPoint() + Core.Me.GetTurnTime(target)) * 1000.0 + Game.Ping);
        }

        public static int GetAbilityDelay(Vector3 targetPosition, Ability ability)
        {
            return (int)((ability.FindCastPoint() + Core.Me.GetTurnTime(targetPosition)) * 1000.0 + Game.Ping);
        }
        private static readonly ConcurrentDictionary<string, Item> ItemDictionary = new ConcurrentDictionary<string, Item>();
        private static readonly ConcurrentDictionary<string, Ability> AbilityDictionary = new ConcurrentDictionary<string, Ability>();

        public static bool HasDangAbility(this Hero v)
        {
            var spells =
                v.Spellbook.Spells.Any(x => x.IsDisable() && x.CanHit(Core.Me) && x.CanBeCasted());
            var items =
                v.Inventory.Items.Any(x => x.IsDisable() && x.CanHit(Core.Me) && x.CanBeCasted());
            return spells || items;
        }
        public static Item FindItem322(this Unit unit, ItemId itemId, bool cache = false)
        {
            if (!unit.IsVisible || !unit.HasInventory)
            {
                return null;
            }

            if (!cache)
            {
                return unit.Inventory.Items.FirstOrDefault(x => x != null && x.IsValid && x.GetItemId() == itemId);
            }

            Item item;
            var n = unit.Handle + itemId.ToString();
            if (!ItemDictionary.TryGetValue(n, out item) || item == null || !item.IsValid
                || Utils.SleepCheck("Common.FindItem322." + itemId))
            {
                item = unit.Inventory.Items.FirstOrDefault(x => x != null && x.IsValid && x.GetItemId() == itemId);
                if (ItemDictionary.ContainsKey(n))
                {
                    ItemDictionary[n] = item;
                }
                else
                {
                    ItemDictionary.TryAdd(n, item);
                }

                Utils.Sleep(1000, "Common.FindItem322." + itemId);
            }

            if (item == null || !item.IsValid)
            {
                return null;
            }

            return item;
        }
        public static Ability FindSpell322(this Unit unit, AbilityId abilityId, bool cache = false)
        {
            if (!cache)
            {
                return unit.Spellbook.Spells.FirstOrDefault(x => x.GetAbilityId() == abilityId);
            }

            Ability ability;
            var n = unit.Handle + abilityId.ToString();
            if (!AbilityDictionary.TryGetValue(n, out ability) || ability == null || !ability.IsValid
                || Utils.SleepCheck("Common.FindSpell322." + abilityId))
            {
                ability = unit.Spellbook.Spells.FirstOrDefault(x => x.GetAbilityId() == abilityId);
                if (AbilityDictionary.ContainsKey(n))
                {
                    AbilityDictionary[n] = ability;
                }
                else
                {
                    AbilityDictionary.TryAdd(n, ability);
                }

                Utils.Sleep(1000, "Common.FindSpell322." + abilityId);
            }

            if (ability == null || !ability.IsValid)
            {
                return null;
            }

            return ability;
        }

        public static bool CanBeCasted(this Ability ability, Unit target)
        {
            if (ability == null || !ability.IsValid)
            {
                return false;
            }

            if (target == null || !target.IsValid)
            {
                return false;
            }

            if (!target.IsValidTarget(checkTeam:false))
            {
                return false;
            }

            var canBeCasted = ability.CanBeCasted();
            if (!target.IsMagicImmune())
            {
                return canBeCasted;
            }

            var data = ability.CommonProperties();
            Printer.Print($"Ability: (Magic: {data?.MagicImmunityPierce}) (CanBeCasted: {canBeCasted}) --> target: {target.Name}");
            return data?.MagicImmunityPierce ?? canBeCasted;
        }
    }
}