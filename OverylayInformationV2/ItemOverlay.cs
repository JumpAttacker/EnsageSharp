using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using Ensage.Common.Objects.UtilityObjects;
using SharpDX;

namespace OverlayInformation
{
    public class ItemOverlay
    {
        private static bool _loaded;
        private static Sleeper _sleeper;
        private static bool IsEnable => Members.Menu.Item("itemOverlay.Enable").GetValue<bool>();
        private static bool DangeItems => Members.Menu.Item("itemOverlay.DangItems").GetValue<bool>();
        private static bool DangOldMethod => Members.Menu.Item("itemOverlay.OldMethod").GetValue<bool>();
        private static bool IsAlly => Members.Menu.Item("itemOverlay.Ally").GetValue<bool>();
        private static bool IsEnemy => Members.Menu.Item("itemOverlay.Enemy").GetValue<bool>();
        private static float Size => (float) Members.Menu.Item("itemOverlay.Size").GetValue<Slider>().Value/100;
        private static float Extra => (float) Members.Menu.Item("itemOverlay.Extra").GetValue<Slider>().Value/10;


        private static float DefSize;

        public ItemOverlay()
        {
            _loaded = false;
            Events.OnLoad += (sender, args) =>
            {
                if (_loaded)
                {
                    return;
                }
                Load();
                _loaded = true;
            };
            if (!_loaded && ObjectManager.LocalHero != null && Game.IsInGame)
            {
                Load();
                _loaded = true;
            }

            Events.OnClose += (sender, args) =>
            {
                Drawing.OnDraw -= Drawing_OnDraw;
                _loaded = false;
            };
        }

        private static void Load()
        {
            _sleeper = new Sleeper();
            
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Checker.IsActive())
                return;
            if (!IsEnable)
                return;
            DefSize = HUDInfo.GetHPBarSizeX() / 6 * Size;
            if (IsAlly)
                foreach (var v in Manager.HeroManager.GetAllyViableHeroes())
                {
                    try
                    {
                        DrawItems(v,false);
                    }
                    catch (Exception)
                    {
                        Printer.Print($"[ItemOverlay][ally]: {v.StoredName()}");
                    }
                    
                }
            if (IsEnemy)
                foreach (var v in Manager.HeroManager.GetEnemyViableHeroes())
                {
                    try
                    {
                        DrawItems(v,true);
                    }
                    catch (Exception)
                    {
                        Printer.Print($"[ItemOverlay][enemy]: {v.StoredName()}");
                    }
                }
        }

        private static void DrawItems(Hero v, bool forEnemy)
        {
            var pos = HUDInfo.GetHPbarPosition(v);
            if (pos.IsZero)
                return;
            List<Item> items;
            try
            {
                if (!Members.ItemDictionary.TryGetValue(v.Name, out items))
                {
                    return;
                }
            }
            catch (Exception)
            {
                Printer.Print("[ItemOverlay][DrawItems]: " + v.StoredName());
                return;
            }
            
            var count = 0;
            var itemBoxSizeY = (float)(DefSize / 1.24);
            var newSize = new Vector2(DefSize, itemBoxSizeY);
            var halfSize = HUDInfo.GetHPBarSizeX()/2;
            var maxSizeX = Math.Max((float)items.Count/2*newSize.X + DefSize/(float) 2.6, halfSize);
            pos -= new Vector2(-halfSize + maxSizeX, DefSize + DefSize/Extra);
            if (DangeItems && forEnemy)
            {
                items = items.Where(Check).ToList();
                if (DangOldMethod)
                {
                    DrawOldMethod(v,items);
                    return;
                }
            }
            foreach (var item in items)
            {
                var extraPos = new Vector2(DefSize*count, 0);
                var itemPos = pos + extraPos;
                var normalSize = newSize + new Vector2(4, DefSize/(float) 2.6 + 4);
                var normalPos = itemPos - new Vector2(2, 2);
                Drawing.DrawRect(normalPos, newSize+new Vector2(4, DefSize / (float)2.6+4), Color.Black);
                Drawing.DrawRect(itemPos, newSize + DefSize / (float)2.6, Textures.GetItemTexture(item.StoredName()));
                DrawState(item, normalPos, normalSize,v.Mana);
                count++;
            }
        }

        private static void DrawOldMethod(Hero v,List<Item> Items)
        {
            float count = 0;
            var iPos = HUDInfo.GetHPbarPosition(v);
            var iSize = new Vector2(HUDInfo.GetHPBarSizeX(v), HUDInfo.GetHpBarSizeY(v));
            foreach (var item in Items)
            {
                var itemname = Textures.GetItemTexture(item.StoredName());
                Drawing.DrawRect(iPos + new Vector2(count, 50),
                    new Vector2(iSize.X/3, (float) (iSize.Y*2.5)),
                    itemname);
                if (item.AbilityState == AbilityState.OnCooldown)
                {
                    var cd = ((int) item.Cooldown).ToString(CultureInfo.InvariantCulture);
                    Drawing.DrawText(cd, iPos + new Vector2(count, 40), Color.White,
                        FontFlags.AntiAlias | FontFlags.DropShadow);
                }
                if (item.AbilityState == AbilityState.NotEnoughMana)
                {
                    Drawing.DrawRect(iPos + new Vector2(count, 50),
                        new Vector2(iSize.X/4, (float) (iSize.Y*2.5)), new Color(0, 0, 200, 100));
                }
                count += iSize.X/4;
            }
        }

        private static bool Check(Item item)
        {
            return Members.Menu.Item("itemOverlay.List").GetValue<AbilityToggler>().IsEnabled(item.Name);
        }

        private static void DrawState(Item item, Vector2 itemPos2, Vector2 normalSize,float mana)
        {
            var itemPos = itemPos2 + new Vector2(1, 0);
            var size = normalSize - new Vector2(14, 3);
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
                var ultimateCd = ((int) Math.Min(Math.Abs(mana - item.ManaCost), 99)).ToString(
                    CultureInfo.InvariantCulture);
                var textSize = Drawing.MeasureText(ultimateCd, "Arial",
                    new Vector2((float)(size.Y * .75), size.Y / 2), FontFlags.AntiAlias);
                var textPos = itemPos + new Vector2(0, size.Y - textSize.Y);
                Drawing.DrawRect(itemPos,
                    normalSize,
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
