using System.Collections.Generic;
using Ensage;
using Ensage.Common.Enums;
using Ensage.Common.Objects.UtilityObjects;

namespace Auto_Disable
{
    public class Members
    {
        public static List<string> Items=new List<string>();
        public static List<string> Spells=new List<string>();


        public static List<ItemId> WhiteList = new List<ItemId>
        {
            ItemId.item_blink
        };

        public static List<ItemId> EscapeItemList = new List<ItemId>
        {
            ItemId.item_blink
        };
        public static List<AbilityId> EscapeAbilityList = new List<AbilityId>
        {
            AbilityId.faceless_void_time_walk,AbilityId.antimage_blink,AbilityId.queenofpain_blink,AbilityId.sandking_burrowstrike
        };

        public static Sleeper Updater;

        public static Team MyTeam;

        public static readonly Dictionary<ClassID, AbilityId> Initiators = new Dictionary<ClassID, AbilityId>();
        public static readonly List<ClassID> DangIltimate = new List<ClassID>
        {
            ClassID.CDOTA_Unit_Hero_Magnataur,
            ClassID.CDOTA_Unit_Hero_Tidehunter,
            ClassID.CDOTA_Unit_Hero_FacelessVoid
        };
    }
}
