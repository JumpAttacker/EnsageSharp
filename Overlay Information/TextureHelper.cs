using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Ensage;
using Ensage.SDK.Renderer;
using Ensage.SDK.Service;
using Ensage.SDK.VPK;
using NLog;

namespace OverlayInformation
{
    public static class TextureHelper
    {
        public static string GamePath = Game.GamePath;
        private static readonly List<string> LoadedList = new List<string>();
        public static void Init(IServiceContext conext)
        {
            Render = conext.TextureManager;
        }

        private static ITextureManager Render { get; set; }

        public static void LoadAbilityTexture(AbilityId id)
        {
            if (LoadedList.Contains(id.ToString()))
                return;
            LoadedList.Add(id.ToString());
            Console.WriteLine($"Load from dota ability texture: {id}");
            Render.LoadFromDota($"{id}", $@"panorama\images\spellicons\{id}_png.vtex_c");
            /*Render.LoadFromFile(id.ToString(),
                $@"{GamePath}\game\dota\materials\ensage_ui\spellicons\png\{id}.png");*/
            //Render.LoadFromDota($"{id}", $@"resource\flash3\images\spellicons\{id}.png");
            //Render.LoadFromDota($"{id}", $@"resource\flash3\images\spellicons\{id}.png");
        }

        public static void LoadItemTexture(AbilityId id)
        {
            if (LoadedList.Contains(id.ToString()))
                return;
            LoadedList.Add(id.ToString());
            var itemString = id.ToString().Remove(0, 5);
            Console.WriteLine($"Load from dota item texture: {itemString}");
            //Render.LoadFromDota($"{id}", $@"resource\flash3\images\items\{itemString}.png");
            LoadFromDota($"{id}", $@"panorama\images\items\{itemString}_png.vtex_c");
            /*Render.LoadFromFile(id.ToString(),
                $@"{GamePath}\game\dota\materials\ensage_ui\items\png\{itemString}.png");*/
        }
        /// <summary>
        /// load horizontal hero texure
        /// </summary>
        /// <param name="id"></param>
        public static void LoadHeroTexture(HeroId id)
        {
            if (LoadedList.Contains(id.ToString()))
                return;
            LoadedList.Add(id.ToString());
            var itemString = id.ToString().Remove(0, 14);
            Console.WriteLine($"Load from dota hero[horizontal] texture: {id} TextureKey: [{itemString}]");
            Render.LoadFromDota($"{id}", $@"panorama\images\heroes\icons\{id}_png.vtex_c");
            /*Render.LoadFromFile(id.ToString(),
                $@"{GamePath}\game\dota\materials\ensage_ui\miniheroes\png\{itemString}.png");*/
        }

        public static void LoadAbilityTexture(params AbilityId[] id)
        {
            foreach (var abilityId in id)
            {
                LoadAbilityTexture(abilityId);
            }
        }

        //npc_dota_hero_axehorizontal
        //npc_dota_hero_axehorizontal

        public static void LoadItemTexture(params AbilityId[] id)
        {
            foreach (var abilityId in id)
            {
                LoadItemTexture(abilityId);
            }
        }

        public static void LoadHeroTexture(HeroId[] id)
        {
            foreach (var abilityId in id)
            {
                LoadHeroTexture(abilityId);
            }
        }


        private static readonly VpkBrowser VpkBrowser = new VpkBrowser();

        public static bool LoadFromDota(string textureKey, string file, Rectangle rectangle = default)
        {
            try
            {
                var bitmapStream = VpkBrowser.FindImage(file);
                if (bitmapStream != null)
                {
                    var stream = new MemoryStream();
                    new Bitmap(bitmapStream).Clone(rectangle.IsEmpty ? new Rectangle(12, 0, 64, 64) : rectangle, PixelFormat.Undefined).Save(stream, ImageFormat.Png);
                    return Render.LoadFromStream(textureKey, stream);
                }

                bitmapStream = VpkBrowser.FindImage(@"panorama\images\spellicons\invoker_empty1_png.vtex_c");
                if (bitmapStream != null)
                {
                    return Render.LoadFromStream(textureKey, bitmapStream);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[Error][TextureKey: [{textureKey}]]{e}");
            }

            return false;
        }
    }
}