using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using Ensage.Common.Objects.UtilityObjects;
using Ensage.Items;
using SharpDX;

namespace OverlayInformation
{
    internal static class ItemPanel
    {
        private static readonly float Percent = HUDInfo.RatioPercentage();
        public static void Draw(EventArgs args)
        {
            if (!Checker.IsActive()) return;
            if (!Members.Menu.Item("itempanel.Enable").GetValue<bool>()) return;
            var startPos = new Vector2(Members.Menu.Item("itempanel.X").GetValue<Slider>().Value,
                Members.Menu.Item("itempanel.Y").GetValue<Slider>().Value);
            var size = new Vector2(Members.Menu.Item("itempanel.SizeX").GetValue<Slider>().Value*Percent,
                Members.Menu.Item("itempanel.SizeY").GetValue<Slider>().Value*Percent);
            Drawing.DrawRect(startPos, size + new Vector2(2, 0), new Color(0, 0, 0, 100));
            var r = Members.Menu.Item("itempanel.Red").GetValue<Slider>().Value;
            var g = Members.Menu.Item("itempanel.Green").GetValue<Slider>().Value;
            var b = Members.Menu.Item("itempanel.Blue").GetValue<Slider>().Value;
            Drawing.DrawRect(startPos, size+new Vector2(2,0), new Color(r, g, b, 255), true);
            Drawing.DrawLine(startPos + new Vector2(2, size.Y / 6), startPos + new Vector2(size.X, size.Y / 6),
                new Color(r, g, b, 255));
            var text = "Item Panel";
            var textSize = Drawing.MeasureText(text, "Arial",
                new Vector2((float)(size.Y/6 * .80), (float) (size.Y / 6*.95)), FontFlags.AntiAlias);
            var textPos = startPos;
            Drawing.DrawText(
                text,
                textPos+new Vector2(2,2),
                new Vector2(textSize.Y, 0),
                Color.White,
                FontFlags.AntiAlias | FontFlags.StrikeOut);
            DrawItems(startPos + new Vector2(2, size.Y / 6+2),size,r,g,b);
            if (Members.Menu.Item("itempanel.Button.Enable").GetValue<bool>())
            {
                var buttonStartPos = startPos + new Vector2(size.X - size.Y / 6, 2);
                var buttonSize = new Vector2(size.Y / 6 - 0, size.Y / 6 - 3);
                text = "S";
                Drawing.DrawRect(buttonStartPos, buttonSize, new Color(r, g, b, 255), true);
                textSize = Drawing.MeasureText(text, "Arial",
                    new Vector2((float)(buttonSize.X), (float)(buttonSize.Y)), FontFlags.AntiAlias);
                
                if (Utils.IsUnderRectangle(Game.MouseScreenPosition, buttonStartPos.X, buttonStartPos.Y, buttonSize.X,
                    buttonSize.Y))
                {
                    Drawing.DrawRect(buttonStartPos, buttonSize, new Color(0, 0, 0, 200));
                    if (Members.IsClicked && Utils.SleepCheck("cd_click"))
                    {
                        Utils.Sleep(250,"cd_click");
                        Members.Menu.Item("itempanel.Stash.Enable")
                            .SetValue(!Members.Menu.Item("itempanel.Stash.Enable").GetValue<bool>());
                    }
                }
                Drawing.DrawText(
                    text,
                    buttonStartPos + new Vector2(buttonSize.X/3, buttonSize.Y/5),
                    textSize,
                    Color.White,
                    FontFlags.AntiAlias | FontFlags.StrikeOut);
            }
            if (Members.Menu.Item("itempanel.Stash.Enable").GetValue<bool>())
            {
                text = "Stash Panel";
                var startPos2 = startPos + new Vector2(size.X, 0);
                Drawing.DrawRect(startPos2, size + new Vector2(2, 0), new Color(0, 0, 0, 100));
                textSize = Drawing.MeasureText(text, "Arial",
                    new Vector2((float) (size.Y/6*.80), (float) (size.Y/6*.95)), FontFlags.AntiAlias);
                Drawing.DrawText(
                    text,
                    startPos2 + new Vector2(5, 2),
                    new Vector2(textSize.Y, 0),
                    Color.White,
                    FontFlags.AntiAlias | FontFlags.StrikeOut);
                Drawing.DrawRect(startPos2, size + new Vector2(2, 0), new Color(r, g, b, 255), true);
                Drawing.DrawLine(startPos2 + new Vector2(2, size.Y/6), startPos2 + new Vector2(size.X, size.Y/6),
                    new Color(r, g, b, 255));
                DrawStashItems(startPos2 + new Vector2(2, size.Y / 6 + 2), size, r, g, b);
            }
        }

        private static void DrawStashItems(Vector2 pos, Vector2 size, int r, int g, int b)
        {
            var i = 0;
            if (Members.ItemDictionary.Count == 0) { return; }
            foreach (var v in Members.EnemyHeroes)
            {
                if (Members.MeepoIgnoreList.Contains(v))
                    continue;
                /*var dividedWeStand = v.FindSpell("meepo_divided_we_stand") as DividedWeStand;
                if (dividedWeStand != null && v.ClassID == ClassID.CDOTA_Unit_Hero_Meepo && dividedWeStand.ID > 0)
                    continue;*/
                List<Item> items;
                try
                {
                    if (!Members.StashItemDictionary.TryGetValue(v.Handle, out items))
                        continue;
                }
                catch (Exception)
                {
                    Printer.Print("[DrawStashItems]: " + v.StoredName());
                    continue;
                }
                var n = 0;
                var heroPos = pos + new Vector2(0, (size.Y / 7 + 3) * i + 2);
                foreach (var item in items)
                {
                    try
                    {
                        var texturename = item.IsRecipe
                            ? "materials/ensage_ui/items/recipe.vmat"
                            : $"materials/ensage_ui/items/{item.Name.Replace("item_", "")}.vmat";
                        if (item is Bottle)
                        {
                            var bottletype = item as Bottle;
                            if (bottletype.StoredRune != RuneType.None)
                            {
                                texturename =
                                    $"materials/ensage_ui/items/{item.Name.Replace("item_", "") + "_" + bottletype.StoredRune}.vmat";
                            }
                        }
                        var itemPos = heroPos + new Vector2(size.X / 7 * n + 5, 0);
                        DrawItem(itemPos, size / 6, texturename, r, g, b);
                        //DrawState(item, v, itemPos, size / 6);
                        n++;
                    }
                    catch (Exception)
                    {
                        //Printer.Print("Hero: "+v.Name+". Item: "+item.Name);
                    }
                }
                for (var j = 0; j < 6 - n; j++)
                {
                    var itemPos = heroPos + new Vector2(size.X / 7 * (n + j) + 5, 0);
                    DrawItem(itemPos, size / 6, "materials/ensage_ui/items/emptyitembg.vmat", r, g, b);
                }
                i++;
            }
        }

        private static void DrawItems(Vector2 pos,Vector2 size,int r,int g,int b)
        {
            var i = 0;
            if (Members.ItemDictionary.Count == 0) {return;}
            foreach (var v in Members.EnemyHeroes)
            {
                if (Members.MeepoIgnoreList.Contains(v))
                    continue;
                /*var dividedWeStand = v.FindSpell("meepo_divided_we_stand") as DividedWeStand;
                if (dividedWeStand != null && v.ClassID == ClassID.CDOTA_Unit_Hero_Meepo && dividedWeStand.ID > 0)
                    continue;*/
                List<Item> items;
                try
                {
                    if (!Members.ItemDictionary.TryGetValue(v.Handle, out items))
                        continue;
                }
                catch (Exception)
                {
                    Printer.Print("[DrawItems]: "+v.StoredName());
                    continue;
                }
                
                var heroPos = pos + new Vector2(0, (size.Y/7 + 3)*i+2);
                Drawing.DrawRect(heroPos, size/7,
                    Textures.GetTexture("materials/ensage_ui/heroes_horizontal/" +
                                        v.StoredName().Substring("npc_dota_hero_".Length) + ".vmat"));
                var n = 0;

                foreach (var item in items)
                {
                    try
                    {
                        var texturename = item.IsRecipe
                            ? "materials/ensage_ui/items/recipe.vmat"
                            : $"materials/ensage_ui/items/{item.Name.Replace("item_", "")}.vmat";
                        if (item is Bottle)
                        {
                            var bottletype = item as Bottle;
                            if (bottletype.StoredRune != RuneType.None)
                            {
                                texturename =
                                    $"materials/ensage_ui/items/{item.Name.Replace("item_", "") + "_" + bottletype.StoredRune}.vmat";
                            }
                        }
                        var itemPos = heroPos + new Vector2(size.X/7, 0) + new Vector2(size.X/7*n + 5, 0);
                        DrawItem(itemPos, size/6, texturename, r, g, b);
                        DrawState(item, v, itemPos, size/6);
                        n++;
                    }
                    catch (Exception)
                    {
                        //Printer.Print("Hero: "+v.Name+". Item: "+item.Name);
                    }
                }
                for (var j = 0; j < 6-n; j++)
                {
                    var itemPos = heroPos + new Vector2(size.X / 7, 0) + new Vector2(size.X / 7 * (n + j) + 5, 0);
                    DrawItem(itemPos, size / 6, "materials/ensage_ui/items/emptyitembg.vmat", r, g, b);
                }
                i++;
            }
            
        }

        private static void DrawState(Item item, Hero v, Vector2 itemPos2, Vector2 size2)
        {
            var itemPos = itemPos2 + new Vector2(1, 0);
            var size = size2 - new Vector2(14, 3);
            if (item.AbilityState == AbilityState.OnCooldown)
            {
                var ultimateCd =
                                    ((int)Math.Min(item.Cooldown+1, 999)).ToString(CultureInfo.InvariantCulture);
                var textSize = Drawing.MeasureText(ultimateCd, "Arial",
                    new Vector2((float)(size.Y * .75), size.Y / 2), FontFlags.AntiAlias);
                //Print(v.Name + " cd: " + ultimateCd);
                var textPos = itemPos + new Vector2(0, size.Y - textSize.Y);
                Drawing.DrawRect(textPos - new Vector2(0, 0),
                    new Vector2(textSize.X, textSize.Y),
                    new Color(0, 0, 0, 200));
                Drawing.DrawText(
                    ultimateCd,
                    textPos,
                    new Vector2(textSize.Y, 0),
                    Color.White,
                    FontFlags.AntiAlias | FontFlags.StrikeOut);     
            }
            else if (item.AbilityState == AbilityState.NotEnoughMana)
            {
                var ultimateCd =((int)Math.Min(Math.Abs(v.Mana - item.ManaCost), 999)).ToString(
                                        CultureInfo.InvariantCulture);
                //Print(v.Name + " mana: " + ultimateCd);
                var textSize = Drawing.MeasureText(ultimateCd, "Arial",
                    new Vector2((float)(size.Y * .75), size.Y / 2), FontFlags.AntiAlias);
                var textPos = itemPos + new Vector2(0, size.Y - textSize.Y);
                Drawing.DrawRect(itemPos,
                    new Vector2(size.X, size.Y),
                    new Color(0, 75, 155, 155));
                /*Drawing.DrawRect(textPos - new Vector2(0, 0),
                    new Vector2(textSize.X, textSize.Y),
                    new Color(0, 0, 0, 200));*/
                Drawing.DrawText(
                    ultimateCd,
                    textPos,
                    new Vector2(textSize.Y, 0),
                    Color.White, 
                    FontFlags.AntiAlias | FontFlags.StrikeOut);
                
            }
        }

        private static void DrawItem(Vector2 pos, Vector2 size,string texture,int r,int g,int b)
        {
            var extraPos = texture == "materials/ensage_ui/items/emptyitembg.vmat" ? new Vector2(2,-2) : new Vector2(0,-2);
            Drawing.DrawRect(pos, size + extraPos,
                Textures.GetTexture(texture));
            Drawing.DrawRect(pos, size - new Vector2(13, 2), new Color(r, g, b, 255), true);

        }

    }

    internal static class NewItemPanel
    {
        private static readonly Sleeper Sleeper=new Sleeper();
        private static readonly Sleeper Toggler=new Sleeper();
        private static Vector2 _globalDif;
        public static void OnDraw(EventArgs args)
        {
            if (!Checker.IsActive()) return;
            if (!Members.Menu.Item("itempanel.new.Enable").GetValue<bool>()) return;
            var startPos = new Vector2(Members.Menu.Item("itempanel.X").GetValue<Slider>().Value,
                Members.Menu.Item("itempanel.Y").GetValue<Slider>().Value);
            var size = new Vector2(Members.Menu.Item("itempanel.SizeX").GetValue<Slider>().Value,
                Members.Menu.Item("itempanel.SizeY").GetValue<Slider>().Value);
            var mPos = Game.MouseScreenPosition;
            if (Utils.IsUnderRectangle(Game.MouseScreenPosition, startPos.X, startPos.Y, size.X, size.Y))
            {
                if (Members.IsClicked)
                {
                    if (!Sleeper.Sleeping)
                    {
                        _globalDif = mPos - startPos;
                        Sleeper.Sleep(500);
                    }
                    startPos = mPos - _globalDif;
                    Members.Menu.Item("itempanel.X").SetValue(new Slider((int)startPos.X, 0, 2000));
                    Members.Menu.Item("itempanel.Y").SetValue(new Slider((int)startPos.Y, 0, 2000));
                }
            }
            var r = Members.Menu.Item("itempanel.Red").GetValue<Slider>().Value;
            var g = Members.Menu.Item("itempanel.Green").GetValue<Slider>().Value;
            var b = Members.Menu.Item("itempanel.Blue").GetValue<Slider>().Value;
            if (Members.Menu.Item("itempanel.Stash.Enable").GetValue<bool>())
            {
                if (Utils.IsUnderRectangle(Game.MouseScreenPosition, startPos.X + size.X, startPos.Y, size.X, size.Y))
                {
                    if (Members.IsClicked && !Toggler.Sleeping)
                    {
                        Toggler.Sleep(250);

                        Members.Menu.Item("itempanel.Stash.Enable")
                            .SetValue(!Members.Menu.Item("itempanel.Stash.Enable").GetValue<bool>());
                    }
                }
            }
            else if (Members.Menu.Item("itempanel.Button.Enable").GetValue<bool>())
            {
                var togglePos=new Vector2(startPos.X + size.X,startPos.Y);
                var toggleSize=new Vector2(10, size.Y);
                Drawing.DrawRect(togglePos, toggleSize,new Color(r,g,b),true);
                if (Utils.IsUnderRectangle(Game.MouseScreenPosition, togglePos.X, togglePos.Y,toggleSize.X,toggleSize.Y))
                {
                    if (Members.IsClicked && !Toggler.Sleeping)
                    {
                        Toggler.Sleep(250);

                        Members.Menu.Item("itempanel.Stash.Enable")
                            .SetValue(!Members.Menu.Item("itempanel.Stash.Enable").GetValue<bool>());
                    }
                    Drawing.DrawRect(togglePos + 1, toggleSize-2, new Color(0, 155,0,100));
                }
            }

            Drawing.DrawRect(startPos, size + new Vector2(2, 0), new Color(0, 0, 0, 100));
            
            
            Drawing.DrawRect(startPos, size+new Vector2(1,5), new Color(0, 0, 0, 50));
            Drawing.DrawRect(startPos - new Vector2(1), size + new Vector2(3,5), new Color(r, g, b), true);
           

            var i = 0;
            if (Members.ItemDictionary.Count == 0)
            {
                return;
            }
            var stageSize = new Vector2(size.X / 6f, size.Y / 5f);
            var itemSize = new Vector2(stageSize.X * 70 / 100f, stageSize.Y);

            foreach (var v in Members.EnemyHeroes.Where(v => !Members.MeepoIgnoreList.Contains(v)))
            {
                var items = Manager.HeroManager.GetItemList(v);
                var heroIconPos = startPos + new Vector2(2) + new Vector2(0, stageSize.Y*i++);
                Drawing.DrawRect(heroIconPos, stageSize, Textures.GetHeroTexture(v.StoredName()));
                Drawing.DrawRect(heroIconPos, stageSize, new Color(r,g,b), true);
                if (!items.Any())
                    continue;
                for (int j = 0; j < 7; j++)
                {
                    var itemTexture = Textures.GetTexture("materials/ensage_ui/items/emptyitembg.vmat");
                    Item item = null;
                    if (j < items.Count)
                    {
                        item = items[j];
                        var curItem = item;
                        try
                        {
                            itemTexture = Textures.GetItemTexture(curItem.StoredName());
                            if (item is Bottle)
                            {
                                var bottletype = item as Bottle;
                                if (bottletype.StoredRune != RuneType.None)
                                {
                                    itemTexture =
                                        Textures.GetTexture(
                                            $"materials/ensage_ui/items/{item.Name.Replace("item_", "") + "_" + bottletype.StoredRune}.vmat");
                                }
                            }
                        }
                        catch
                        {
                            //Printer.PrintError($"[ItemPanel]: {v.Name}/ count: {j}/ {item?.Name}");
                        }
                    }
                    var curPos = heroIconPos + new Vector2(stageSize.X + 2, 0) + new Vector2(j*stageSize.X*70/100f, 0);
                    Drawing.DrawRect(curPos, stageSize, itemTexture);
                    Drawing.DrawRect(curPos, itemSize, new Color(r,g,b), true);
                    if (item != null)
                        DrawState(item, v, curPos, itemSize);
                    if (Members.Menu.Item("itempanel.Stash.Enable").GetValue<bool>())
                    {
                        List<Item> stashItems;
                        try
                        {
                            if (!Members.StashItemDictionary.TryGetValue(v.Handle, out stashItems))
                                continue;
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                        itemTexture = Textures.GetTexture("materials/ensage_ui/items/emptyitembg.vmat");
                        var k = j + 7;
                        if (j < stashItems.Count)
                        {
                            item = stashItems[j];
                            var curItem = item;
                            try
                            {
                                itemTexture = Textures.GetItemTexture(curItem.StoredName());
                                if (item is Bottle)
                                {
                                    var bottletype = item as Bottle;
                                    if (bottletype.StoredRune != RuneType.None)
                                    {
                                        itemTexture =
                                            Textures.GetTexture(
                                                $"materials/ensage_ui/items/{item.Name.Replace("item_", "") + "_" + bottletype.StoredRune}.vmat");
                                    }
                                }
                            }
                            catch
                            {
                                Printer.PrintError($"[ItemPanel]: {v.Name}/ count: {j}/ {item?.Name}");
                            }
                        }
                        curPos = heroIconPos + new Vector2(stageSize.X + 10, 0) + new Vector2(k * stageSize.X * 70 / 100f, 0);
                        Drawing.DrawRect(curPos, stageSize, itemTexture);
                        Drawing.DrawRect(curPos, itemSize, new Color(r, g, b), true);
                    }
                }
                
            }
        }
        private static void DrawState(Item item, Hero v, Vector2 itemPos, Vector2 size)
        {
            if (!item.IsValid)
                return;
            if (item.AbilityState == AbilityState.OnCooldown)
            {
                var ultimateCd =
                    ((int) Math.Min(item.Cooldown + 1, 99)).ToString(CultureInfo.InvariantCulture);
                var textSize = Drawing.MeasureText(ultimateCd, "Arial",
                    new Vector2((float)(size.Y * .75), size.Y / 2), FontFlags.AntiAlias);
                var textPos = itemPos + new Vector2(0, size.Y - textSize.Y);
                Drawing.DrawRect(textPos - new Vector2(0, 0),
                    new Vector2(textSize.X, textSize.Y),
                    new Color(0, 0, 0, 200));
                Drawing.DrawText(
                    ultimateCd,
                    textPos,
                    new Vector2(textSize.Y, 0),
                    Color.White,
                    FontFlags.AntiAlias | FontFlags.StrikeOut);
            }
            else if (item.AbilityState == AbilityState.NotEnoughMana)
            {
                var ultimateCd = ((int) Math.Min(Math.Abs(v.Mana - item.ManaCost), 99)).ToString(
                    CultureInfo.InvariantCulture);
                var textSize = Drawing.MeasureText(ultimateCd, "Arial",
                    new Vector2((float)(size.Y * .75), size.Y / 2), FontFlags.AntiAlias);
                var textPos = itemPos + new Vector2(0, size.Y - textSize.Y);
                Drawing.DrawRect(itemPos,
                    new Vector2(size.X, size.Y),
                    new Color(0, 75, 155, 155));
                Drawing.DrawText(
                    ultimateCd,
                    textPos,
                    new Vector2(textSize.Y, 0),
                    Color.White,
                    FontFlags.AntiAlias | FontFlags.StrikeOut);

            }
        }
    }
}