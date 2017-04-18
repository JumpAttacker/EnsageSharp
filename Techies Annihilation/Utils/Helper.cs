using System.Linq;
using Ensage;
using Ensage.Common.Enums;
using Ensage.Common.Extensions;
using SharpDX;
using Techies_Annihilation.Features;
using AbilityId = Ensage.Common.Enums.AbilityId;

namespace Techies_Annihilation.Utils
{
    internal static class Helper
    {
        public static string PrintVector(this Vector3 vec)
        {
            return $"({vec.X};{vec.Y};{vec.Z})";
        }
        public static string PrintVector(this Vector2 vec)
        {
            return $"({vec.X};{vec.Y})";
        }
        public static int GetAbilityDelay(this Ability ability, Unit target)
        {
            return (int)((ability.FindCastPoint() + Core.Me.GetTurnTime(target)) * 1000.0 + Game.Ping);
        }
        public static int GetAbilityDelay(this Ability ability)
        {
            return (int)((ability.FindCastPoint()) * 1000.0 + Game.Ping);
        }

        public static int GetAbilityDelay(this Ability ability, Vector3 targetPosition)
        {
            return (int)((ability.FindCastPoint() + Core.Me.GetTurnTime(targetPosition)) * 1000.0 + Game.Ping);
        }

        public static bool HasItem(this Unit unit, ItemId classId)
        {
            return unit.Inventory.Items.Any(item => item.GetItemId() == classId);
        }
        public static bool HasAbility(this Unit unit, AbilityId classId)
        {
            return unit.Spellbook.Spells.Any(item => item.GetAbilityId() == classId);
        }

        public static bool CanDie(this Hero hero,bool checkForAegis=false)
        {
            var mod = !hero.HasModifiers(
                new[]
                {
                    "modifier_dazzle_shallow_grave", "modifier_oracle_false_promise",
                    "modifier_skeleton_king_reincarnation_scepter_active"
                },
                false);
            if (checkForAegis)
                return mod && !hero.HasItem(ItemId.item_aegis);
            return mod;
        }
    }
}