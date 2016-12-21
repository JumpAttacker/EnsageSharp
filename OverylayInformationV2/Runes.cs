using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common.Objects.UtilityObjects;
using SharpDX;

namespace OverlayInformation
{
    internal static class Runes
    {
        private static readonly List<Rune> InSystem=new List<Rune>(); 
        private static readonly Dictionary<RuneType, string> RuneTypes = new Dictionary<RuneType, string>
        {
            {RuneType.Arcane, "materials/ensage_ui/minirunes/arcane.vmat"},
            {RuneType.Bounty, "materials/ensage_ui/minirunes/bounty.vmat"},
            {RuneType.DoubleDamage, "materials/ensage_ui/minirunes/doubledamage.vmat"},
            {RuneType.Haste, "materials/ensage_ui/minirunes/haste.vmat"},
            {RuneType.Illusion, "materials/ensage_ui/minirunes/illusion.vmat"},
            {RuneType.Invisibility, "materials/ensage_ui/minirunes/invis.vmat"},
            {RuneType.None, ""},
            {RuneType.Regeneration, "materials/ensage_ui/minirunes/regen.vmat"}
        };

        private static Sleeper _sleeper;

        public static void Flush()
        {
            _sleeper = new Sleeper();
        }

        public static void Draw(EventArgs args)
        {
            if (!Checker.IsActive()) return;
            if (!Members.Menu.Item("runevision.DrawOnMinimap.Enable").GetValue<bool>()) return;
            foreach (var rune in InSystem.Where(rune => rune != null && rune.IsValid && rune.RuneType!=RuneType.Bounty))
            {
                var v = rune;
                var size3 = new Vector2(10, 25) + new Vector2(13, -6);
                var w2M = Helper.WorldToMinimap(v.NetworkPosition);
                var name = RuneTypes[v.RuneType];
                Drawing.DrawRect(w2M - new Vector2(size3.X / 2, size3.Y / 2), size3,
                    Drawing.GetTexture(name));
            }
        }

        public static void Action()
        {
            if (_sleeper.Sleeping)return;
            _sleeper.Sleep(10);
            var runes =
                ObjectManager.GetEntities<Rune>()
                    .Where(x => x.RuneType != RuneType.None && x.RuneType != RuneType.Bounty && !InSystem.Contains(x))
                    .ToList();
            foreach (var rune in runes)
            {
                InSystem.Add(rune);
                if (Members.Menu.Item("runevision.PrintText.Enable").GetValue<bool>())
                    Game.PrintMessage(
                        "<font size='20'> Rune: " + "<font face='Comic Sans MS, cursive'><font color='#00aaff'>"
                        + rune.RuneType + " on <font color='#FF0000'>" + (rune.Position.X < 0 ? "TOP" : "BOT") +
                        " </font>");
            }
        }
    }
}