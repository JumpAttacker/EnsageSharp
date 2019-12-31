using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using Ensage.SDK.Helpers;
using Ensage.SDK.Menu;
using log4net;
using OverlayInformation.Features.beh;
using PlaySharp.Toolkit.Logging;
using SharpDX;

namespace OverlayInformation.Features.Open_Dota
{
    public class OpenDotaHelper : Movable
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly List<PlayerInfo> _playerInfoList;

        public OpenDotaHelper(Config config)
        {
            Config = config;
            var panel = Config.Factory.Menu("Open Dota Helper");
            Enable = panel.Item("Enable", true);
            CanMove = panel.Item("Movable", true);
            PosX = panel.Item("Position X", new Slider(700, 0, 2000));
            PosY = panel.Item("Position Y", new Slider(700, 0, 2000));
            SizeX = panel.Item("Size X", new Slider(28, 1));
            SizeY = panel.Item("Size Y", new Slider(14, 1));
            TempSize = panel.Item("Temp Size", new Slider(14, 1));
            _playerInfoList = new List<PlayerInfo>();
            LoadMovable(config.Main.Context.Value.Input);
            if (Enable)
            {
                UpdateManager.BeginInvoke(Loading, 500);
                //UpdateManager.BeginInvoke(PartyFake, 500);
                Drawing.OnDraw += DrawingOnOnDraw;
            }

            Enable.Item.ValueChanged += (sender, args) =>
            {
                if (args.GetNewValue<bool>())
                {
                    UpdateManager.BeginInvoke(Loading, 500);
                    //UpdateManager.BeginInvoke(PartyFake, 500);
                    Drawing.OnDraw += DrawingOnOnDraw;
                }
                else
                {
                    Drawing.OnDraw -= DrawingOnOnDraw;
                }
            };
        }

        public Config Config { get; }

        public MenuItem<Slider> TempSize { get; set; }

        public MenuItem<Slider> PosY { get; set; }

        public MenuItem<Slider> PosX { get; set; }
        public MenuItem<Slider> SizeX { get; set; }

        public MenuItem<Slider> SizeY { get; set; }

        public MenuItem<bool> CanMove { get; set; }

        public MenuItem<bool> Enable { get; set; }

        private async void Loading()
        {
            if (Game.GameState != GameState.PreGame)
                return;
            _playerInfoList.Clear();
            for (uint i = 0; i < Game.MaximumClients; i++)
            {
                var player = ObjectManager.GetPlayerById(i);
                if (player == null || !player.IsValid || player.IsFakeClient)
                    continue;
                try
                {
                    Console.WriteLine(new string('-', Console.BufferWidth));
                    var steamId = player.PlayerSteamId;
                    Log.Debug($"Player({i}): {player.Name} => id: {steamId}");
                    if (steamId <= 10)
                    {
                        Log.Error("Wrong steam id!");
                        continue;
                    }

                    var test = await FindWinRateAsync(steamId);
                    if (test < 0 || test > 100)
                    {
                        Log.Error("Cant load this player!");
                        continue;
                    }

                    var playerReq = await GetPlayerAsync(steamId);
                    var wr = await FindFullWinRateAsync(steamId);
                    //var accName = GetValue("personaname\":", playerReq);
                    var estimate = 0;
                    var stdDev = 0;
                    var solo = 0;
                    var party = 0;
                    var country = "";
                    var possibleMmr = "";
                    var matches = "";
                    var infoAboutHero = "";
                    /*int estimate = Convert.ToInt32(GetValue("estimate\":", playerReq));
                    int stdDev = Convert.ToInt32(GetValue("stdDev\":", playerReq));
                    int solo = Convert.ToInt32(GetValue("solo_competitive_rank\":", playerReq));
                    int party = Convert.ToInt32(GetValue("competitive_rank\":", playerReq));
                    var country = GetValue("loccountrycode\":", playerReq);
                    var possibleMmr = $"{estimate - stdDev}-{estimate + stdDev}";*/
                    try
                    {
                        //Console.WriteLine("estimate: "+ GetValue("estimate\":", playerReq));
                        estimate = Convert.ToInt32(GetValue("{\"estimate\":", playerReq));
                    }
                    catch (Exception)
                    {
                        //Log.Error("1");
                    }

                    try
                    {
                        var item = GetValue("stdDev\":", playerReq);
                        item = item.Substring(0, item.IndexOf(".", StringComparison.Ordinal));
                        stdDev = Convert.ToInt32(item);
                    }
                    catch (Exception)
                    {
                        //Log.Error("2");
                    }

                    try
                    {
                        var item = GetValue("solo_competitive_rank\":", playerReq);
                        solo = Convert.ToInt32(item /*.Substring(1, item.Length - 2)*/);
                    }
                    catch (Exception)
                    {
                        //Log.Error("3");
                    }

                    try
                    {
                        var item = GetValue("\"competitive_rank\":", playerReq);
                        party = Convert.ToInt32(item /*.Substring(1, item.Length - 2)*/);
                    }
                    catch (Exception)
                    {
                        //Log.Error("4");
                    }

                    try
                    {
                        country = GetValue("loccountrycode\":", playerReq);
                    }
                    catch (Exception)
                    {
                        //Log.Error("5");
                    }

                    try
                    {
                        possibleMmr = $"{estimate - stdDev}-{estimate + stdDev}";
                    }
                    catch (Exception)
                    {
                        //Log.Error("6");
                    }

                    try
                    {
                        matches = await FindMatches(steamId);
                    }
                    catch (Exception)
                    {
                        //Log.Error("7");
                    }

                    try
                    {
                        infoAboutHero = await FindInfoAboutHero(steamId, (uint) player.Hero.HeroId);
                    }
                    catch (Exception)
                    {
                        //Log.Error("8");
                    }
                    //Log.Debug("test: "+ matches);

                    Log.Debug(
                        $"[WinRate: {wr}] [solo: {solo}] [party {party}] [estimate mmr: {possibleMmr}] [{country}] history: {matches}");
                    var totalGames = "";
                    var wins = "";
                    try
                    {
                        totalGames = GetValue("games", infoAboutHero).TrimStart(':');
                    }
                    catch (Exception)
                    {
                    }

                    try
                    {
                        wins = GetValue("win", infoAboutHero).TrimStart(':');
                    }
                    catch (Exception)
                    {
                    }

                    var wrOnCurrentHero = 0;
                    try
                    {
                        wrOnCurrentHero = (int) ((float) Convert.ToInt32(wins) / Convert.ToInt32(totalGames) * 100.0f);
                    }
                    catch (Exception)
                    {
                    }

                    try
                    {
                        Log.Debug(
                            $"[Hero: {player?.Hero?.GetRealName()} -> [Games {totalGames}] [Wins {wins}] [WR {wrOnCurrentHero}%]");
                    }
                    catch (Exception)
                    {
                    }

                    _playerInfoList.Add(new PlayerInfo((int) i, solo, party, country, possibleMmr, wr, matches,
                        player?.Name, player?.Hero, totalGames, wins, wrOnCurrentHero));
                }
                catch (Exception e)
                {
                    Log.Debug($"error with player: {player.Name} ({i}) -> {e}");
                }
            }
        }

        private void DrawingOnOnDraw(EventArgs args)
        {
            if (Game.GameState != GameState.PreGame)
                return;
            var pos = new Vector2(PosX.Value.Value, PosY.Value.Value);
            var startPosition = pos;
            /*var size = new Vector2(SizeX * 10, SizeY * 10);
            var stageSize = new Vector2(size.X / 7f, size.Y / 10f);
            var itemSize = new Vector2(stageSize.X / .7f, stageSize.Y);
            var emptyTexture = Textures.GetTexture("materials/ensage_ui/items/emptyitembg.vmat");*/

            foreach (var info in _playerInfoList)
            {
                var text = new StringBuilder();
                if (info.Wr.Length > 0)
                    text.Append($" {info.Wr}");
                if (info.Solo > 0)
                    text.Append($" solo: {info.Solo}");
                if (info.Party > 0)
                    text.Append($" party: {info.Party}");
                if (info.PossibleMmr.Length > 0)
                    text.Append($" estimate: {info.PossibleMmr}");
                if (info.Matches.Length > 0)
                    text.Append($" {info.Matches}");
                if (info.Party > 0)
                    text.Append($"{info.Wr}");
                if (info.Party > 0)
                    text.Append($"{info.Wr}");
                var size = DrawTextOnCenter(pos, info.Name);
                pos += new Vector2(size.X, 0);
                size = DrawTextOnCenter(pos, text.ToString());
                pos += new Vector2(0, size.Y);
                if (info.Hero != null)
                {
                    var count = 0;
                    size = DrawHeroIcon(pos, info.Hero.Name, size.Y);
                    try
                    {
                        count = Convert.ToInt32(info.TotalGames);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    if (count > 0)
                    {
                        text.Clear();
                        text.Append($" -> Games: {info.TotalGames} Wins: {info.Wins} WR: {info.WrOnCurrentHero}%");

                        //size = DrawHeroIcon(pos, "npc_dota_hero_antimage", size.Y);
                        pos += new Vector2(size.X, 0);
                        size = DrawTextOnCenter(pos, text.ToString());
                        pos += new Vector2(0, size.Y);
                    }
                }

                pos = new Vector2(startPosition.X, pos.Y);
            }

            if (CanMove)
                if (CanMoveWindow(ref startPosition, new Vector2(pos.X - startPosition.X, pos.Y - startPosition.Y),
                    true))
                {
                    PosX.Item.SetValue(new Slider((int) startPosition.X, 0, 2000));
                    PosY.Item.SetValue(new Slider((int) startPosition.Y, 0, 2000));
                }
        }

        private Vector2 DrawTextOnCenter(Vector2 pos, string text)
        {
            var textSize = Drawing.MeasureText(text, "Arial",
                new Vector2(TempSize), FontFlags.AntiAlias | FontFlags.StrikeOut);
            Drawing.DrawRect(pos, textSize, new Color(155, 155, 155, 155));
            Drawing.DrawText(
                text, "Arial",
                pos, new Vector2(TempSize),
                Color.White,
                FontFlags.AntiAlias | FontFlags.StrikeOut);

            Drawing.DrawRect(pos, textSize, Color.White, true);
            return textSize;
        }

        private Vector2 DrawHeroIcon(Vector2 pos, string name, float size)
        {
            var iconSize = new Vector2(size * 1.7f, size);
            Drawing.DrawRect(pos, iconSize, Textures.GetHeroTexture(name));
            Drawing.DrawRect(pos, iconSize, Color.White, true);
            return iconSize;
        }

        private void DrawText(string text, Vector2 pos, Vector2 maxSize)
        {
            var textSize = Drawing.MeasureText(text, "Arial",
                new Vector2(maxSize.Y * 0.9f, 0), FontFlags.AntiAlias | FontFlags.StrikeOut);

            var textPos = pos + new Vector2(maxSize.X / 2 - textSize.X / 2, maxSize.Y / 2 - textSize.Y / 2);
            //var textPos = pos + new Vector2(0, 0);
            Drawing.DrawRect(pos, maxSize, new Color(0, 0, 0, 100));
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

        #region Fakes

        private async void PartyFake()
        {
            Log.Debug("starting fake");
            if (true)
                for (uint i = 0; i < 10; i++)
                    try
                    {
                        Console.WriteLine(new string('-', Console.BufferWidth));
                        uint steamId = 105248644;
                        Log.Debug($"Player({i}): {"FAKE"} => id: {steamId}");
                        if (steamId <= 10)
                        {
                            Log.Error("Wrong steam id!");
                            continue;
                        }

                        var test = await FindWinRateAsync(steamId);
                        if (test < 0 || test > 100)
                        {
                            Log.Error("Cant load this player!");
                            continue;
                        }

                        var playerReq = await GetPlayerAsync(steamId);
                        var wr = await FindFullWinRateAsync(steamId);
                        //var accName = GetValue("personaname\":", playerReq);
                        var estimate = 0;
                        var stdDev = 0;
                        var solo = 0;
                        var party = 0;
                        var country = "";
                        var possibleMmr = "";
                        var matches = "";
                        /*int estimate = Convert.ToInt32(GetValue("estimate\":", playerReq));
                        int stdDev = Convert.ToInt32(GetValue("stdDev\":", playerReq));
                        int solo = Convert.ToInt32(GetValue("solo_competitive_rank\":", playerReq));
                        int party = Convert.ToInt32(GetValue("competitive_rank\":", playerReq));
                        var country = GetValue("loccountrycode\":", playerReq);
                        var possibleMmr = $"{estimate - stdDev}-{estimate + stdDev}";*/
                        try
                        {
                            //Console.WriteLine("estimate: "+ GetValue("estimate\":", playerReq));
                            estimate = Convert.ToInt32(GetValue("{\"estimate\":", playerReq));
                        }
                        catch (Exception)
                        {
                            //Log.Error("1");
                        }

                        try
                        {
                            var item = GetValue("stdDev\":", playerReq);
                            item = item.Substring(0, item.IndexOf(".", StringComparison.Ordinal));
                            stdDev = Convert.ToInt32(item);
                        }
                        catch (Exception)
                        {
                            //Log.Error("2");
                        }

                        try
                        {
                            var item = GetValue("solo_competitive_rank\":", playerReq);
                            solo = Convert.ToInt32(item /*.Substring(1, item.Length - 2)*/);
                        }
                        catch (Exception)
                        {
                            //Log.Error("3");
                        }

                        try
                        {
                            var item = GetValue("\"competitive_rank\":", playerReq);
                            party = Convert.ToInt32(item /*.Substring(1, item.Length - 2)*/);
                        }
                        catch (Exception)
                        {
                            //Log.Error("4");
                        }

                        try
                        {
                            country = GetValue("loccountrycode\":", playerReq);
                        }
                        catch (Exception)
                        {
                            //Log.Error("5");
                        }

                        try
                        {
                            possibleMmr = $"{estimate - stdDev}-{estimate + stdDev}";
                        }
                        catch (Exception)
                        {
                            //Log.Error("6");
                        }

                        try
                        {
                            matches = await FindMatches(steamId);
                        }
                        catch (Exception)
                        {
                            //Log.Error("7");
                        }

                        //Log.Debug("test: "+ matches);
                        _playerInfoList.Add(new PlayerInfo((int) i, solo, party, country, possibleMmr, wr, matches,
                            "FAKE " + i));
                        Log.Debug(
                            $"[WinRate: {wr}] [solo: {solo}] [party {party}] [estimate mmr: {possibleMmr}] [{country}] history: {matches}");
                        //var success = await TryToFindPlayerAsync(player.Name);
                        //var success = TryToFindPlayer(player.Name);

                        /*Log.Debug($"Try To Find player with search API -> player({i}): " + player.Name + " -> (steamId)" +
                                  player.PlayerSteamID +
                                  $" -> success: {success.Length != 2} bufferSize [{success.Length}]");
                        if (success.Length != 2)
                        {
                            var accId = GetValue("account_id\":", success);
                            var accName = GetValue("personaname\":", success);
                            Log.Debug("id: " + accId);
                            Log.Debug("personaname: " + accName);
                            Log.Debug("wr: " + FindWinRate(Convert.ToUInt32(accId)) + "%");
                            //Log.Debug("GetPlayer: " + GetPlayer(accId));
                        }
                        else
                        {
                            Log.Debug($"cant find {player.Name}!");
                        }*/
                    }
                    catch (Exception e)
                    {
                        Log.Debug($"error with player: ({i}) -> {e}");
                    }

            Log.Debug("ending fake");
        }

        private async void SingleFake()
        {
            Log.Debug("loading!");
            var s = await GetPlayerAsync(1);
            Log.Debug(s);
            s = await FindInfoAboutHero(1, (uint) ObjectManager.LocalHero.HeroId);
            Log.Debug(s);
            var totalGames = GetValue("games", s).TrimStart(':');
            var wins = GetValue("win", s).TrimStart(':');
            var wrOnCurrentHero = (float) Convert.ToInt32(wins) / Convert.ToInt32(totalGames) * 100.0f;
            Log.Debug(
                $"[Hero: {ObjectManager.LocalHero.GetRealName()} -> [Games {totalGames}] [Wins {wins}] [WR {wrOnCurrentHero}%]");
        }

        #endregion

        #region Helpers

        private async Task<string> TryToFindPlayerAsync(string name, bool print = false)
        {
            var request = WebRequest.Create($"https://api.opendota.com/api/search?q={name}&similarity=1");
            string strContent;
            using (var response = (HttpWebResponse) await Task.Factory
                .FromAsync(request.BeginGetResponse,
                    request.EndGetResponse,
                    null))
            using (var content = response.GetResponseStream())
            using (var reader = new StreamReader(content))
            {
                strContent = reader.ReadToEnd();
                if (print)
                    Console.WriteLine(strContent);
            }

            return strContent;
        }

        private async Task<string> GetPlayerAsync(uint id)
        {
            var webRequest = WebRequest.Create($"https://api.opendota.com/api/players/{id}");
            string strContent;
            var response = (HttpWebResponse) await Task.Factory
                .FromAsync(webRequest.BeginGetResponse,
                    webRequest.EndGetResponse,
                    null);
            using (var content = response.GetResponseStream())
            using (var reader = new StreamReader(content))
            {
                strContent = reader.ReadToEnd();
                //Console.WriteLine(strContent);
            }

            return strContent;
        }

        private async Task<int> FindWinRateAsync(uint id)
        {
            var webRequest = WebRequest.Create($"https://api.opendota.com/api/players/{id}/wl");
            //((HttpWebRequest)webRequest).UserAgent = ".NET Framework";
            string strContent;
            var response = (HttpWebResponse) await Task.Factory
                .FromAsync(webRequest.BeginGetResponse,
                    webRequest.EndGetResponse,
                    null);
            using (var content = response.GetResponseStream())
            using (var reader = new StreamReader(content))
            {
                strContent = reader.ReadToEnd();
                //Console.WriteLine(strContent);
            }

            var win = Convert.ToInt32(GetValue("win\":", strContent));
            var lose = Convert.ToInt32(GetValue("lose\":", strContent));
            return (int) (win / (win + (double) lose) * 100f);
        }

        private async Task<string> FindFullWinRateAsync(uint id)
        {
            var webRequest = WebRequest.Create($"https://api.opendota.com/api/players/{id}/wl");
            //((HttpWebRequest)webRequest).UserAgent = ".NET Framework";
            string strContent;
            var response = (HttpWebResponse) await Task.Factory
                .FromAsync(webRequest.BeginGetResponse,
                    webRequest.EndGetResponse,
                    null);
            using (var content = response.GetResponseStream())
            using (var reader = new StreamReader(content))
            {
                strContent = reader.ReadToEnd();
            }

            var win = Convert.ToInt32(GetValue("win\":", strContent));
            var lose = Convert.ToInt32(GetValue("lose\":", strContent));
            var wr = (int) (win / (win + (double) lose) * 100f);
            return $"({win}/{lose}) {wr}%";
        }

        private async Task<string> FindMatches(uint id, int gameLimit = 5, int gameMode = 22)
        {
            var webRequest =
                WebRequest.Create(
                    $"https://api.opendota.com/api/players/{id}/matches?limit={gameLimit}&game_mode={gameMode}");
            //((HttpWebRequest)webRequest).UserAgent = ".NET Framework";
            var response = (HttpWebResponse) await Task.Factory
                .FromAsync(webRequest.BeginGetResponse,
                    webRequest.EndGetResponse,
                    null);
            var info = "[";
            using (var content = response.GetResponseStream())
            using (var reader = new StreamReader(content))
            {
                var strContent = reader.ReadToEnd();
                for (var i = 0; i < gameLimit; i++)
                {
                    var start = strContent.IndexOf("{", StringComparison.Ordinal);
                    var end = strContent.IndexOf("}", StringComparison.Ordinal);
                    if (start == -1 || end == -1)
                        continue;
                    var tempLine = strContent.Substring(start, end);
                    strContent = strContent.Remove(start, end);
                    //Console.WriteLine(new string('-', Console.BufferWidth));
                    //Console.WriteLine(tempLine);
                    var playerSlot = Convert.ToInt32(GetValue("player_slot\":", tempLine));
                    var radiantWin = GetValue("radiant_win\":", tempLine) == "true";
                    var win = playerSlot <= 10 && radiantWin || playerSlot > 10 && !radiantWin;
                    info += win ? "+" : "-";
                }
            }

            info += "]";
            return info;
        }

        private async Task<string> FindInfoAboutHero(uint playerid, uint heroId)
        {
            var webRequest =
                WebRequest.Create($"https://api.opendota.com/api/players/{playerid}/heroes?hero_id={heroId}");
            string strContent;
            var response = (HttpWebResponse) await Task.Factory
                .FromAsync(webRequest.BeginGetResponse,
                    webRequest.EndGetResponse,
                    null);
            using (var content = response.GetResponseStream())
            using (var reader = new StreamReader(content))
            {
                strContent = reader.ReadToEnd();
                //Console.WriteLine(strContent);
            }

            var trimmed = strContent.Trim();
            var end = trimmed.Substring(0, trimmed.IndexOf('}') + 1);
            return end;
        }

        private string GetValue(string value, string path2)
        {
            var path = string.Copy(path2);
            var search = value;
            var index = path.IndexOf(search, StringComparison.Ordinal);
            var index2 = path.IndexOf(",", index, StringComparison.Ordinal);
            if (index2 == -1)
                index2 = path.IndexOf("}", index, StringComparison.Ordinal);
            //Log.Fatal($"[{index}] [{search.Length}] [{index2}]");
            var kek = path.Substring(index + search.Length, index2 - index - search.Length).Trim('\"');
            if (kek == "null")
                return 0.ToString();
            return kek;
        }

        #endregion
    }
}