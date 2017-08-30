using System;
using Ensage;
using Ensage.Common.Enums;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using SharpDX;

namespace SfAnnihilation.DrawingStuff
{
    public class Element
    {
        public string Text { get; set; }
        public DotaTexture Texture;
        public Vector2 Position;
        public float Size;

        public Element(ItemId itemBlackKingBar)
        {
            Texture = Textures.GetItemTexture(itemBlackKingBar.ToString());
            Position = MenuManager.DrawBkbStatusPosition;
            Size = MenuManager.DrawBkbStatusSize;
        }

        public Element(string text)
        {
            Text = text;
            Position = MenuManager.DrawAimStatusPosition;
            Size = MenuManager.DrawAimStatusSize;
        }
    }
    public static class InfoDrawing
    {
        public static Element BkbElement;
        public static Element AimInfo;
        static InfoDrawing()
        {
            BkbElement = new Element(ItemId.item_black_king_bar);
            AimInfo = new Element("Aim");
        }

        public static void OnDraw(EventArgs args)
        {
            if (AimInfo != null && MenuManager.DrawAimStatus)
            {
                Drawing.DrawText(AimInfo.Text + (MenuManager.AimKillStealOnly ? " [KS]" : string.Empty), AimInfo.Position,
                    new Vector2(AimInfo.Size), MenuManager.AimIsActive ? new Color(0, 255, 0) : new Color(255, 0, 0),
                    FontFlags.AntiAlias | FontFlags.StrikeOut);
            }
            if (BkbElement == null || !MenuManager.DrawBkbStatus ||
                Features.Core.Me.FindItem(ItemId.item_black_king_bar.ToString(), true) == null ||
                Features.Core.Me.FindItem(ItemId.item_cyclone.ToString(), true) == null) return;
            var pos = BkbElement.Position;
            var size = BkbElement.Size;
            Drawing.DrawRect(pos, new Vector2(size, size / 2f), BkbElement.Texture);
            var clr = MenuManager.BkbIsEulCombo ? new Color(0, 255, 0, 50) : new Color(255, 0, 0, 50);
            Drawing.DrawRect(pos, new Vector2(size*70/100f, size/2f), clr);
            Drawing.DrawRect(pos, new Vector2(size*70/100f, size/2f), Color.White, true);
        }

        public static void BkbChanger(object sender, OnValueChangeEventArgs e)
        {
            var item = sender as MenuItem;
            if (item == null)
                return;
            if (item.Name == "Drawing.BkbStatusInEulCombo.Size")
            {
                BkbElement.Size = MenuManager.DrawBkbStatusSize;
            }
            else
            {
                BkbElement.Position = MenuManager.DrawBkbStatusPosition;
            }
        }
        public static void AimChanger(object sender, OnValueChangeEventArgs e)
        {
            var item = sender as MenuItem;
            if (item == null)
                return;
            if (item.Name == "Drawing.Aim.Size")
            {
                AimInfo.Size = MenuManager.DrawAimStatusSize;
            }
            else
            {
                AimInfo.Position = MenuManager.DrawAimStatusPosition;
            }
        }
    }
}