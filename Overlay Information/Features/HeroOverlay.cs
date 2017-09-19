using System;
using System.Linq;
using Ensage;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using Ensage.Items;
using Ensage.SDK.Menu;
using SharpDX;

namespace OverlayInformation.Features
{
    public class HeroOverlay
    {
        public Config Config { get; }
        
        public HeroOverlay(Config config)
        {
            Config = config;
            var panel = Config.Factory.Menu("Hero Overlay");
            EnableForMainHero = panel.Item("Enable for main hero", true);
            ExtraPositionX = panel.Item("Extra position X", new Slider(0, -50, 50));
            ExtraPositionY = panel.Item("Extra position Y", new Slider(0, -50, 50));
            ExtraSizeX = panel.Item("Extra size X", new Slider(0, -50, 50));
            ExtraSizeY = panel.Item("Extra size Y", new Slider(0, -50, 50));
            var manaBars = panel.Menu("ManaBars");
            
            ManaBars = manaBars.Item("Enable", true);
            ManaBarsForAlly = manaBars.Item("Enable for ally", true);
            ManaBarsForEnemy = manaBars.Item("Enable for enemy", true);
            ManaBarsNumbers = manaBars.Item("Draw value", true);
            ManaBarsSize = manaBars.Item("Size", new Slider(12, 5, 20));

            var abilityOverlay = panel.Menu("Ability overlay");
            AbilityOverlay = abilityOverlay.Item("Enable", true);
            AbilityLevelTextSize = abilityOverlay.Item("Ability Level text size", new Slider(5, 2, 10));
            AbilityCooldownTextSize = abilityOverlay.Item("Ability Cooldown/ManaCost text size", new Slider(6, 2, 10));
            AbilitySize = abilityOverlay.Item("Size", new Slider(7, 1, 20));

            var itemOverlay = panel.Menu("Item overlay");
            ItemOverlay = itemOverlay.Item("Enable", true);
            ItemDrawCharges = itemOverlay.Item("Draw charges", true);
            ItemDangItems = itemOverlay.Item("Danger items only", false);
            ItemTextSize = itemOverlay.Item("Cooldown/ManaCost text size", new Slider(10, 2, 10));
            ItemSize = itemOverlay.Item("Size", new Slider(7, 1, 20));
            ItemBorderClr = itemOverlay.Item("Border color", new StringList("white", "black"));

            Drawing.OnDraw += DrawingOnOnDraw;
            HealthBarSize = new Vector2(HudInfo.GetHPBarSizeX(), HudInfo.GetHpBarSizeY());
        }

        public MenuItem<bool> EnableForMainHero { get; set; }

        public MenuItem<Slider> ExtraSizeX { get; set; }
        public MenuItem<Slider> ExtraSizeY { get; set; }

        public MenuItem<Slider> ExtraPositionX { get; set; }
        public MenuItem<Slider> ExtraPositionY { get; set; }

        public MenuItem<bool> ItemDrawCharges { get; set; }

        public MenuItem<bool> ItemDangItems { get; set; }

        public MenuItem<StringList> ItemBorderClr { get; set; }

        public MenuItem<Slider> AbilitySize { get; set; }

        public MenuItem<Slider> ItemSize { get; set; }

        public MenuItem<Slider> ItemTextSize { get; set; }

        public MenuItem<bool> ItemOverlay { get; set; }

        public MenuItem<Slider> AbilityCooldownTextSize { get; set; }

        public MenuItem<Slider> AbilityLevelTextSize { get; set; }

        public MenuItem<Slider> ManaBarsSize { get; set; }

        public MenuItem<bool> ManaBarsNumbers { get; set; }

        public MenuItem<bool> ManaBarsForEnemy { get; set; }

        public MenuItem<bool> ManaBarsForAlly { get; set; }

        public MenuItem<bool> AbilityOverlay { get; set; }

        public MenuItem<bool> ManaBars { get; set; }

        public Vector2 HealthBarSize { get; set; }

        private void DrawingOnOnDraw(EventArgs args)
        {
            var heroes = Config.Main.Updater.Heroes;
            var itemBorderWhite = ItemBorderClr.Value.SelectedIndex == 0;
            foreach (var heroCont in heroes)
            {
                if (!EnableForMainHero && heroCont.IsOwner)
                    continue;
                var hero = heroCont.Hero;
                if (!hero.IsAlive)
                    continue;
                if (!hero.IsVisible)
                    continue;
                var pos = HudInfo.GetHPbarPosition(hero) + new Vector2(ExtraPositionX, ExtraPositionY);
                if (pos.IsZero)
                    continue;
                var copy = pos;
                var size = new Vector2(HudInfo.GetHPBarSizeX(hero) + ExtraSizeX,
                    HudInfo.GetHpBarSizeY(hero) + ExtraSizeY);
                if (heroCont.IsOwner)
                {
                    pos += new Vector2(-1, size.Y);
                    size -= new Vector2(1, 0);
                }
                else
                {
                    pos += new Vector2(0, size.Y - 2);
                }
                if (ManaBars)
                {
                    if (heroCont.IsAlly && ManaBarsForAlly || !heroCont.IsAlly && ManaBarsForEnemy)
                    {
                        var mana = heroCont.Mana;
                        pos = DrawingHelper.DrawManaBar(pos, mana * size.X / heroCont.MaxMana, size,
                            new Color(0, 155, 255, 255),
                            new Color(0, 0, 0, 255), ((int)mana).ToString(), ManaBarsNumbers,
                            ManaBarsSize.Value.Value);
                    }
                }

                if (AbilityOverlay)
                {
                    var tempSize = size.X * AbilitySize / 30f;
                    var abilitySize = new Vector2(tempSize);
                    
                    //IEnumerable<Ability> abilities = heroCont.Abilities;
                    //var enumerable = abilities as Ability[] ?? abilities.ToArray();
                    //var abilityCount = abilities.Count();
                    var abilities = heroCont.Abilities2.Where(x => x.IsValid && !x.IsHidden).ToList();
                    var abilityCount = abilities.Count;
                    //var extraAbilitites = abilityCount - 4;
                    pos += new Vector2((size.X - tempSize * abilityCount) / 2f, 0);
                    /*if (extraAbilitites > 0)
                    {
                        pos -= new Vector2(abilitySize.X * extraAbilitites / 2f, 0);
                    }*/

                    /*foreach (var ability in abilities)
                    {
                        if (ability == null || !ability.IsValid || ability.IsHidden)
                        {
                            heroCont.RefreshAbilities();
                            continue;
                        }
                        pos = DrawAbilityState(pos, ability, abilitySize);
                    }*/

                    try
                    {
                        foreach (var ability in abilities.OrderBy(x=>x.AbilitySlot))
                        {
                            pos = DrawAbilityState(pos, ability, abilitySize);
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                if (ItemOverlay)
                {
                    var tempSize = size.X * ItemSize / 30f;
                    var abilitySize = new Vector2(tempSize);
                    var abilities = ItemDangItems ? heroCont.DangItems : heroCont.Items;
                    var abilityCount = abilities.Count;
                    //var extraAbilitites = abilityCount - 4;
                    pos = copy;
                    pos += new Vector2((size.X - tempSize * abilityCount) / 2f, -abilitySize.Y);
                    /*if (extraAbilitites > 0)
                    {
                        //pos -= new Vector2(abilitySize.X * extraAbilitites / 2f, 0);
                        
                    }*/

                    foreach (var ability in abilities)
                    {
                        if (ability == null || !ability.IsValid)
                            continue;
                        pos = DrawItemState(pos, ability, abilitySize, itemBorderWhite ? Color.White : Color.Black);
                    }
                }
            }
        }

        private Vector2 DrawAbilityState(Vector2 pos, AbilityHolder ability, Vector2 maxSize)
        {
            var abilityState = ability.AbilityState;
            var level = ability.Ability.Level;
            Drawing.DrawRect(pos, maxSize, ability.Texture);
            if (level > 0)
            {
                switch (abilityState)
                {
                    case AbilityState.Ready:
                        if (ability.Ability.IsInAbilityPhase)
                            Drawing.DrawRect(pos, maxSize, new Color(255, 255, 0, 50));
                        break;
                    case AbilityState.NotEnoughMana:
                        var mana = ability.Ability.ManaCost - ability.Owner.Mana;
                        var manaText = ((int)mana).ToString();
                        DrawAbilityMana(manaText, pos, maxSize);
                        break;
                    case AbilityState.OnCooldown:
                        var cooldown = ability.Cooldown + 1;
                        var cdText = ((int)cooldown).ToString();
                        DrawAbilityCooldown(cdText, pos, maxSize);
                        break;
                }
                if (ability.MaximumLevel > 1)
                    DrawAbilityLevel(level, pos, maxSize);
            }
            else
            {
                Drawing.DrawRect(pos, maxSize, new Color(0, 0, 0, 125));
            }

            Drawing.DrawRect(pos, maxSize, Color.Black, true);
            return pos + new Vector2(maxSize.X, 0);
        }

        public Vector2 DrawItemState(Vector2 pos, Item ability, Vector2 maxSize, Color color)
        {
            var abilityState = ability.AbilityState;
            var bottletype = ability as Bottle;
            var itemSize = new Vector2(maxSize.X * 1.5f, maxSize.Y);
            if (bottletype != null && bottletype.StoredRune != RuneType.None)
            {
                var itemTexture =
                    Textures.GetTexture(
                        $"materials/ensage_ui/items/{ability.Name.Replace("item_", "") + "_" + bottletype.StoredRune}.vmat");
                Drawing.DrawRect(pos, itemSize, itemTexture);
            }
            else
            {
                Drawing.DrawRect(pos, itemSize, Textures.GetItemTexture(ability.Name));
            }
            //Drawing.DrawRect(pos, new Vector2(maxSize.X * 70 / 100f, maxSize.Y / 2f), Color.White, true);
            switch (abilityState)
            {
                case AbilityState.Ready:
                    if (ItemDrawCharges)
                    {
                        var chrages = ability.CurrentCharges;
                        if (chrages > 0)
                            DrawItemCharge(chrages, pos, maxSize);
                    }
                    break;
                case AbilityState.NotEnoughMana:
                    var mana = ability.ManaCost - ((Hero)ability.Owner).Mana;
                    var manaText = ((int)mana).ToString();
                    DrawAbilityMana(manaText, pos, maxSize);
                    break;
                case AbilityState.OnCooldown:
                    var cooldown = Math.Min(99, ability.Cooldown + 1);
                    var cdText = ((int)cooldown).ToString();
                    DrawItemCooldown(cdText, pos, maxSize);
                    break;
            }

            Drawing.DrawRect(pos, new Vector2(maxSize.X, maxSize.Y), color, true);
            return pos + new Vector2(maxSize.X - 1, 0);
        }
        public Vector2 DrawItemState(Vector2 pos, AbilityHolder ability, Vector2 maxSize, Color color)
        {
            var abilityState = ability.AbilityState;
            var bottletype = ability.Ability as Bottle;
            var itemSize = new Vector2(maxSize.X * 1.5f, maxSize.Y);
            if (bottletype != null && bottletype.StoredRune != RuneType.None)
            {
                var itemTexture =
                    Textures.GetTexture(
                        $"materials/ensage_ui/items/{ability.Name.Replace("item_", "") + "_" + bottletype.StoredRune}.vmat");
                Drawing.DrawRect(pos, itemSize, itemTexture);
            }
            else
            {
                Drawing.DrawRect(pos, itemSize, Textures.GetItemTexture(ability.Name));
            }
            //Drawing.DrawRect(pos, new Vector2(maxSize.X * 70 / 100f, maxSize.Y / 2f), Color.White, true);
            switch (abilityState)
            {
                case AbilityState.Ready:
                    if (ItemDrawCharges)
                    {
                        var chrages = ability.Item.CurrentCharges;
                        if (chrages > 0)
                            DrawItemCharge(chrages, pos, maxSize);
                    }
                    break;
                case AbilityState.NotEnoughMana:
                    var mana = ability.Ability.ManaCost - ability.Owner.Mana;
                    var manaText = ((int)mana).ToString();
                    DrawAbilityMana(manaText, pos, maxSize);
                    break;
                case AbilityState.OnCooldown:
                    var cooldown = Math.Min(99, ability.Cooldown + 1);
                    var cdText = ((int)cooldown).ToString();
                    DrawItemCooldown(cdText, pos, maxSize);
                    break;
            }

            Drawing.DrawRect(pos, new Vector2(maxSize.X, maxSize.Y), color, true);
            return pos + new Vector2(maxSize.X - 1, 0);
        }

        public Vector2 DrawAbilityState(Vector2 pos, Ability ability, Vector2 maxSize)
        {
            var abilityState = ability.AbilityState;
            var level = ability.Level;
            Drawing.DrawRect(pos, maxSize, Textures.GetSpellTexture(ability.Name));
            if (level > 0)
            {
                switch (abilityState)
                {
                    case AbilityState.Ready:
                        if (ability.IsInAbilityPhase)
                            Drawing.DrawRect(pos, maxSize, new Color(255, 255, 0, 50));
                        break;
                    case AbilityState.NotEnoughMana:
                        var mana = ability.ManaCost - ((Hero) ability.Owner).Mana;
                        var manaText = ((int)mana).ToString();
                        DrawAbilityMana(manaText, pos, maxSize);
                        break;
                    case AbilityState.OnCooldown:
                        var cooldown = ability.Cooldown + 1;
                        var cdText = ((int)cooldown).ToString();
                        DrawAbilityCooldown(cdText, pos, maxSize);
                        break;
                }
                if (ability.MaximumLevel > 1)
                    DrawAbilityLevel(level, pos, maxSize);
            }
            else
            {
                Drawing.DrawRect(pos, maxSize, new Color(0, 0, 0, 125));
            }

            Drawing.DrawRect(pos, maxSize, Color.Black, true);
            return pos + new Vector2(maxSize.X, 0);
        }

        private void DrawAbilityLevel(uint level, Vector2 pos, Vector2 maxSize)
        {
            var text = level.ToString();
            var textSize = Drawing.MeasureText(text, "Arial",
                new Vector2(maxSize.Y * AbilityLevelTextSize / 10f, 0), FontFlags.AntiAlias | FontFlags.StrikeOut);

            //var textPos = pos + new Vector2(maxSize.X / 2 - textSize.X / 2, maxSize.Y / 2 - textSize.Y / 2);
            var textPos = pos + new Vector2(0, maxSize.Y - textSize.Y);
            Drawing.DrawRect(textPos, textSize, new Color(0, 0, 0, 200));
            Drawing.DrawText(
                text, "Arial",
                textPos, new Vector2(textSize.Y, 0),
                Color.White,
                FontFlags.AntiAlias | FontFlags.StrikeOut);
        }
        private void DrawItemCharge(uint charge, Vector2 pos, Vector2 maxSize)
        {
            var text = charge.ToString();
            var textSize = Drawing.MeasureText(text, "Arial",
                new Vector2(maxSize.Y * 0.5f, 0), FontFlags.AntiAlias | FontFlags.StrikeOut);

            var textPos = pos + new Vector2(5, maxSize.Y - textSize.Y);
            Drawing.DrawRect(textPos, textSize, new Color(0, 0, 0, 200));
            Drawing.DrawText(
                text, "Arial",
                textPos, new Vector2(textSize.Y, 0),
                Color.White,
                FontFlags.AntiAlias | FontFlags.StrikeOut);
        }

        private void DrawAbilityCooldown(string text, Vector2 pos, Vector2 maxSize)
        {
            var textSize = Drawing.MeasureText(text, "Arial",
                new Vector2(maxSize.Y * AbilityCooldownTextSize / 10f, 0), FontFlags.AntiAlias | FontFlags.StrikeOut);

            //var textPos = pos + new Vector2(maxSize.X / 2 - textSize.X / 2, maxSize.Y / 2 - textSize.Y / 2);
            var textPos = pos + new Vector2(0, 0);
            Drawing.DrawRect(textPos, textSize, new Color(0, 0, 0, 200));
            Drawing.DrawText(
                text, "Arial",
                textPos, new Vector2(textSize.Y, 0),
                Color.White,
                FontFlags.AntiAlias | FontFlags.StrikeOut);
        }

        private void DrawItemCooldown(string text, Vector2 pos, Vector2 maxSize)
        {
            var textSize = Drawing.MeasureText(text, "Arial",
                new Vector2(maxSize.Y * ItemTextSize / 10f, 0), FontFlags.AntiAlias | FontFlags.StrikeOut);

            var textPos = pos + new Vector2(maxSize.X / 2 - textSize.X / 2, maxSize.Y / 2 - textSize.Y / 2);
            //var textPos = pos + new Vector2(0, 0);
            Drawing.DrawRect(pos, maxSize, new Color(0, 0, 0, 100));
            Drawing.DrawText(
                text, "Arial",
                textPos, new Vector2(textSize.Y, 0),
                Color.White,
                FontFlags.AntiAlias | FontFlags.StrikeOut);
        }

        private void DrawAbilityMana(string text, Vector2 pos, Vector2 maxSize)
        {
            var textSize = Drawing.MeasureText(text, "Arial",
                new Vector2(maxSize.Y * AbilityCooldownTextSize / 10f, 0), FontFlags.AntiAlias | FontFlags.StrikeOut);

            var textPos = pos + new Vector2(maxSize.X / 2 - textSize.X / 2, maxSize.Y / 2 - textSize.Y / 2);
            //var textPos = pos + new Vector2(0, 0);
            Drawing.DrawRect(pos, maxSize, new Color(50, 50, 200, 190));
            Drawing.DrawText(
                text, "Arial",
                textPos, new Vector2(textSize.Y, 0),
                Color.White,
                FontFlags.AntiAlias | FontFlags.StrikeOut);
        }

        public void OnDeactivate()
        {
            Drawing.OnDraw -= DrawingOnOnDraw;
        }
    }
}