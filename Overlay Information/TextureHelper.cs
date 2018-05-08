using System.Collections.Generic;
using Ensage;
using Ensage.SDK.Renderer;
using Ensage.SDK.Service;
using NLog;

namespace OverlayInformation
{
    public static class TextureHelper
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();
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
            Log.Debug($"Load from dota ability texture: {id}");
            Render.LoadFromDota($"{id}", $@"resource\flash3\images\spellicons\{id}.png");
        }

        public static void LoadItemTexture(AbilityId id)
        {
            if (LoadedList.Contains(id.ToString()))
            {
                Log.Debug($"Already Loaded from dota item texture: {id}");
                return;
            }
            LoadedList.Add(id.ToString());
            var itemString = id.ToString().Remove(0, 5);
            Log.Debug($"Load from dota item texture: {itemString}");
            Render.LoadFromDota($"{id}", $@"resource\flash3\images\items\{itemString}.png");
        }
        public static void LoadItemTexture(string id)
        {
            if (LoadedList.Contains(id))
            {
                Log.Debug($"Already Loaded from dota item texture: {id}");
                return;
            }
            LoadedList.Add(id);
            var itemString = id.Remove(0, 5);
            Log.Debug($"Load from dota item texture: {itemString}");
            Render.LoadFromDota($"{id}", $@"resource\flash3\images\items\{itemString}.png");
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
            Log.Debug($"Load from dota hero[horizontal] texture: {id} TextureKey: [{itemString}]");
            Render.LoadFromFile(id.ToString(),
                $@"{GamePath}\game\dota\materials\ensage_ui\miniheroes\png\{itemString}.png");
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
    }
}