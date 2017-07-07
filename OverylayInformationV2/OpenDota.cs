using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Objects;
using log4net;
using PlaySharp.Toolkit.Logging;
using SharpDX;
using SharpDX.Direct3D9;

namespace OverlayInformation
{
    public static class HeroPickStageScreenHelper
    {
        private static readonly float FirstLeftHeroPos;
        private static readonly float FirstRightHeroPos;
        private static readonly float IconSize;
        public static Vector2 ScreenSize { get; set; }
        static HeroPickStageScreenHelper()
        {
            float size;
            float rightCoef;
            float leftCoef;
            ScreenSize = new Vector2(Drawing.Width, Drawing.Height);
            var ratio = Math.Floor((decimal)(ScreenSize.X / ScreenSize.Y * 100));
            switch ((int)ratio)
            {
                case 160: //16:10
                    leftCoef = 2.45f;
                    rightCoef = 1.68f;
                    size =14f;
                    break;
                case 125: //4:3
                    leftCoef = 2.46f;
                    rightCoef = 1.68f;
                    size =14.3f;
                    break;
                case 177: //16:9
                    leftCoef = 2.47f;
                    rightCoef = 1.68f;
                    size = 15.6f;
                    break;
                default:
                    Printer.PrintError(
                        @"Your screen resolution is not supported and drawings might have wrong size/position, (" +
                        ratio + ")");
                    leftCoef = 2.47f;
                    rightCoef = 1.68f;
                    size = 15.6f;
                    break;
            }
            FirstLeftHeroPos = ScreenSize.X / leftCoef;
            FirstRightHeroPos = ScreenSize.X / rightCoef;
            IconSize = ScreenSize.X/size;
            //Console.WriteLine($"Rate: {ratio} Left: {FirstLeftHeroPos} Right: {FirstRightHeroPos} size: {IconSize} Center: {centerOfScreen}");
        }

        public static float GetPlayerPosition(int id)
        {
            float pos;
            if (id > 4)
            {
                //var extra = id > 4 ? IconSize*(id - 4) : 0;
                var extra = IconSize*(id - 5);
                pos = FirstRightHeroPos + extra;
            }
            else
                pos = FirstLeftHeroPos - IconSize*(5-id);
            
            return pos;
        }
    }
    public class PlayerInfo
    {
        public string Matches { get; set; }
        public string Name { get; set; }

        public int Id;
        public int Solo;
        public int Party;
        public string Country;
        public string PossibleMmr;
        public string Wr;
        public Hero Hero;
        public string TotalGames;
        public string Wins;
        public int WrOnCurrentHero;

        public PlayerInfo(int id, int solo, int party, string country, string possibleMmr, string winrate, string matches, string name)
        {
            Matches = matches;
            Id = id;
            Solo = solo;
            Party = party;
            Country = country;
            PossibleMmr = possibleMmr;
            Wr = winrate;
            Name = name.Length > 10 ? name.Substring(0, 10) : name;
        }

        public PlayerInfo(int id, int solo, int party, string country, string possibleMmr, string winrate,
            string matches, string name, Hero hero, string totalGames, string wins, int wrOnCurrentHero)
            : this(id, solo, party, country, possibleMmr, winrate, matches, name)
        {
            Hero = hero;
            TotalGames = totalGames;
            Wins = wins;
            WrOnCurrentHero = wrOnCurrentHero;
        }
    }
    public class OpenDota
    {
        private static readonly Font Text;
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly List<PlayerInfo> PlayerInfoList;
        private static bool _loaded;
        private static readonly bool Init;
        private static bool Check => Game.GameState == GameState.PreGame;
        private static bool IsEnable => Members.Menu.Item("OpenDota.Enable").GetValue<bool>();
        static OpenDota()
        {
            if (!IsEnable)
                return;
            if (Init)
                return;
            Init = true;
            Printer.PrintInfo("[OpenDota] Init");
            /*if (Drawing.RenderMode != RenderMode.Dx9)
            {
                Printer.Print("You're using not dx9, OpenDota helper will not working!", true);
                Printer.PrintError("You're using not dx9, OpenDota helper will not working!");
                return;
            }*/
            /*Console.WriteLine(
                $"Screen Size: {HeroPickStageScreenHelper.ScreenSize.X}/{HeroPickStageScreenHelper.ScreenSize.Y}");*/
            //SingleFake();
            
            PlayerInfoList = new List<PlayerInfo>();
            /*if (Drawing.Direct3DDevice9 != null)
                Text = new Font(
                    Drawing.Direct3DDevice9,
                    new FontDescription
                    {
                        FaceName = "Tahoma",
                        Height = 14,
                        OutputPrecision = FontPrecision.Default,
                        Quality = FontQuality.Default
                    });*/
            Game.OnUpdate += GameOnOnUpdate;
            /*AppDomain.CurrentDomain.DomainUnload += CurrentDomainDomainUnload;
            Drawing.OnPostReset += Drawing_OnPostReset;
            Drawing.OnPreReset += Drawing_OnPreReset;
            Drawing.OnEndScene += Drawing_OnEndScene;*/
            Drawing.OnDraw+=DrawingOnOnDraw;
            Events.OnLoad += (sender, args) =>
            {
                //DelayAction.Add(1000, SingleFake);
            };

            //DelayAction.Add(1000,PartyFake);
        }

        private static void DrawingOnOnDraw(EventArgs args)
        {
            try
            {
                if (!Checker.IsActive()) return;
                if (!Members.Menu.Item("OpenDota.Enable").GetValue<bool>()) return;
            }
            catch (Exception)
            {
                // ignored
            }

            if (Check)
            {
                var newlist = PlayerInfoList.ToList();
                foreach (var playerInfo in newlist)
                {
                    var id = playerInfo.Id;
                    var startXPosition = HeroPickStageScreenHelper.GetPlayerPosition(id);
                    var position = new Vector2(startXPosition, 35);
                    var size = HudInfoNew.GetTopPanelSizeY();
                    position += new Vector2(0, (float)size * 1.8f);
                    var defClr = Color.White;
                    //DrawShadowText(playerInfo.Name, (int)position.X, (int)position.Y, defClr);
                    Drawing.DrawText($"[{playerInfo.Name}]", position, defClr, FontFlags.None);
                    position.Y += 15;
                    //DrawShadowText(playerInfo.Wr, (int)position.X, (int)position.Y, defClr);
                    Drawing.DrawText($"[{playerInfo.Wr}]", position, defClr, FontFlags.None);
                    position.Y += 15;

                    /*DrawShadowText(
                        playerInfo.Solo == 0 ? $"Estimated: {playerInfo.PossibleMmr}" : $"Solo: {playerInfo.Solo}",
                        (int)position.X, (int)position.Y, defClr);*/

                    Drawing.DrawText(
                        playerInfo.Solo == 0 ? $"Estimated: {playerInfo.PossibleMmr}" : $"Solo: {playerInfo.Solo}",
                        position, defClr, FontFlags.None);
                    if (playerInfo.Party > 0)
                    {
                        position.Y += 15;
                        //DrawShadowText($"Party: {playerInfo.Party}", (int)position.X, (int)position.Y, defClr);
                        Drawing.DrawText($"[{playerInfo.Party}]", position, defClr, FontFlags.None);
                    }
                    var gameHistorySize = playerInfo.Matches.Length - 2;
                    if (gameHistorySize >= 1)
                    {
                        position.Y += 15;
                        for (var i = 0; i < gameHistorySize; i++)
                        {
                            var isTrue = playerInfo.Matches[i + 1] == '+';
                            var clr = isTrue ? Color.Green : Color.Red;
                            position.X += 10;
                            var text = '⬤';//●
                            //DrawShadowText($"{text}", (int)position.X, (int)position.Y, clr);
                            Drawing.DrawText($"[{text}]", position, clr, FontFlags.None);
                        }
                    }
                    if (playerInfo.Country.Length > 0)
                    {
                        try
                        {
                            var n = Convert.ToInt32(playerInfo.Country);
                            if (n > 0)
                            {
                                position.Y += 15;
                                //DrawShadowText($"[{playerInfo.Country}]", (int) position.X, (int) position.Y, defClr);
                                Drawing.DrawText($"[{playerInfo.Country}]", position, defClr, FontFlags.None);
                            }
                        }
                        catch (Exception)
                        {
                        }

                    }
                    if (playerInfo.TotalGames.Length > 0)
                    {
                        try
                        {
                            var n = Convert.ToInt32(playerInfo.TotalGames);
                            if (n == 0)
                                continue;
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                        var totalGames = Convert.ToInt32(playerInfo.TotalGames);
                        var wins = Convert.ToInt32(playerInfo.Wins);
                        var loses = totalGames - wins;
                        var wr = playerInfo.WrOnCurrentHero;
                        position.Y += 15;
                        position.X = startXPosition;
                        Drawing.DrawText($"[{playerInfo.Hero?.GetRealName()}: {wins}/{loses} ({wr}%)]", position, defClr,
                            FontFlags.None);
                        //DrawShadowText($"[{playerInfo.Hero?.GetRealName()}: {wins}/{loses} ({wr}%)]", (int)position.X, (int)position.Y, defClr);
                    }
                }
            }
        }

        private static void GameOnOnUpdate(EventArgs args)
        {
            try
            {
                if (!Checker.IsActive()) return;
                if (!Members.Menu.Item("OpenDota.Enable").GetValue<bool>()) return;
            }
            catch (Exception)
            {
                // ignored
            }
            if (_loaded)
                return;
            if (Check)
            {
                _loaded = true;
                Console.WriteLine("[OpenDota] [pre]Loaded");
                DelayAction.Add(500, () =>
                {
                    Console.WriteLine("[OpenDota] Loaded");
                    Beeeeaaaaaar();
                });
                //Beeeeaaaaaar();
            }
        }

        private static async void Beeeeaaaaaar()
        {
            for (uint i = 0; i < Game.MaximumClients; i++)
            {
                var player = ObjectManager.GetPlayerById(i);
                if (player == null || !player.IsValid || player.IsFakeClient)
                    continue;
                try
                {
                    Printer.PrintSuccess(new string('-', Console.BufferWidth));
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
                    int estimate = 0;
                    int stdDev = 0;
                    int solo = 0;
                    int party = 0;
                    string country = "";
                    string possibleMmr = "";
                    string matches = "";
                    string infoAboutHero = "";
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
                        solo = Convert.ToInt32(item/*.Substring(1, item.Length - 2)*/);
                    }
                    catch (Exception)
                    {
                        //Log.Error("3");
                    }
                    try
                    {
                        var item = GetValue("\"competitive_rank\":", playerReq);
                        party = Convert.ToInt32(item/*.Substring(1, item.Length - 2)*/);
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
                        infoAboutHero = await FindInfoAboutHero(steamId, (uint)player.Hero.HeroId);
                    }
                    catch (Exception)
                    {
                        //Log.Error("8");
                    }
                    //Log.Debug("test: "+ matches);

                    Log.Debug(
                        $"[WinRate: {wr}] [solo: {solo}] [party {party}] [estimate mmr: {possibleMmr}] [{country}] history: {matches}");
                    string totalGames = "";
                    string wins = "";
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
                        wrOnCurrentHero = (int)((float)Convert.ToInt32(wins) / Convert.ToInt32(totalGames) * 100.0f);
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

                    PlayerInfoList.Add(new PlayerInfo((int)i, solo, party, country, possibleMmr, wr, matches, player?.Name, player?.Hero, totalGames, wins, wrOnCurrentHero));
                }
                catch (Exception e)
                {
                    Log.Debug($"error with player: {player.Name} ({i}) -> {e}");
                }

            }
        }

        private static void Drawing_OnEndScene(EventArgs args)
        {
            try
            {
                if (!Checker.IsActive()) return;
                if (!Members.Menu.Item("OpenDota.Enable").GetValue<bool>()) return;
            }
            catch (Exception)
            {
                // ignored
            }

            if (Drawing.Direct3DDevice9 == null || Drawing.Direct3DDevice9.IsDisposed)
            {
                return;
            }
            if (Check)
            {
                var newlist = PlayerInfoList.ToList();
                foreach (var playerInfo in newlist)
                {
                    var id = playerInfo.Id;
                    var startXPosition = HeroPickStageScreenHelper.GetPlayerPosition(id);
                    var position = new Vector2(startXPosition, 35);
                    var size = HudInfoNew.GetTopPanelSizeY();
                    position += new Vector2(0, (float)size * 1.8f);
                    var defClr = Color.White;
                    DrawShadowText(playerInfo.Name, (int)position.X, (int)position.Y, defClr);
                    position.Y += 15;
                    DrawShadowText(playerInfo.Wr, (int)position.X, (int)position.Y, defClr);
                    position.Y += 15;
                    DrawShadowText(
                        playerInfo.Solo == 0 ? $"Estimated: {playerInfo.PossibleMmr}" : $"Solo: {playerInfo.Solo}",
                        (int)position.X, (int)position.Y, defClr);
                    if (playerInfo.Party > 0)
                    {
                        position.Y += 15;
                        DrawShadowText($"Party: {playerInfo.Party}", (int)position.X, (int)position.Y, defClr);
                    }
                    var gameHistorySize = playerInfo.Matches.Length - 2;
                    if (gameHistorySize >= 1)
                    {
                        position.Y += 15;
                        for (var i = 0; i < gameHistorySize; i++)
                        {
                            var isTrue = playerInfo.Matches[i + 1] == '+';
                            var clr = isTrue ? Color.Green : Color.Red;
                            position.X += 10;
                            var text = '⬤';//●
                            DrawShadowText($"{text}", (int)position.X, (int)position.Y, clr);
                        }
                    }
                    if (playerInfo.Country.Length > 0)
                    {
                        try
                        {
                            var n = Convert.ToInt32(playerInfo.Country);
                            if (n > 0)
                            {
                                position.Y += 15;
                                DrawShadowText($"[{playerInfo.Country}]", (int) position.X, (int) position.Y, defClr);
                            }
                        }
                        catch (Exception)
                        {
                        }
                        
                    }
                    if (playerInfo.TotalGames.Length > 0)
                    {
                        try
                        {
                            var n = Convert.ToInt32(playerInfo.TotalGames);
                            if (n == 0)
                                continue;
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                        var totalGames = Convert.ToInt32(playerInfo.TotalGames);
                        var wins = Convert.ToInt32(playerInfo.Wins);
                        var loses = totalGames-wins;
                        var wr = playerInfo.WrOnCurrentHero;
                        position.Y += 15;
                        position.X = startXPosition;
                        DrawShadowText($"[{playerInfo.Hero?.GetRealName()}: {wins}/{loses} ({wr}%)]", (int)position.X, (int)position.Y, defClr);
                    }
                }
            }
        }

        #region Fakes
        private static async void PartyFake()
        {
            Log.Debug("starting fake");
            if (true)
            {
                _loaded = true;
                for (uint i = 0; i < 10; i++)
                {
                    try
                    {
                        Printer.PrintSuccess(new string('-', Console.BufferWidth));
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
                        int estimate = 0;
                        int stdDev = 0;
                        int solo = 0;
                        int party = 0;
                        string country = "";
                        string possibleMmr = "";
                        string matches = "";
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
                        PlayerInfoList.Add(new PlayerInfo((int)i, solo, party, country, possibleMmr, wr, matches,
                            "FAKE "+i));
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

                }
            }
            Log.Debug("ending fake");
        }

        private static async void SingleFake()
        {
            Log.Debug("loading!");
            var s = await GetPlayerAsync(1);
            Log.Debug(s);
            s = await FindInfoAboutHero(1, (uint) ObjectManager.LocalHero.HeroId);
            Log.Debug(s);
            var totalGames = GetValue("games", s).TrimStart(':');
            var wins = GetValue("win", s).TrimStart(':');
            var wrOnCurrentHero = (float)Convert.ToInt32(wins) / Convert.ToInt32(totalGames) * 100.0f;
            Log.Debug(
                $"[Hero: {ObjectManager.LocalHero.GetRealName()} -> [Games {totalGames}] [Wins {wins}] [WR {wrOnCurrentHero}%]");
        }

        #endregion

        #region render stuff

        private static Vector2 DrawText(string text, Vector2 tSize, Vector2 startPos)
        {
            var textSize = Drawing.MeasureText(text, "Arial",
                tSize, FontFlags.None);
            Drawing.DrawRect(startPos, textSize, new Color(0, 0, 0, 155));
            var textPos = startPos;
            Drawing.DrawText(
                text,
                textPos, tSize,
                Color.White,
                FontFlags.AntiAlias | FontFlags.StrikeOut);
            return textSize;
        }

        private static Vector2 DrawHeroIcon(Hero target, Vector2 size, Vector2 startPos)
        {
            var extra = new Vector2(size.X / 3, 0);
            var finalSize = size + extra;
            Drawing.DrawRect(startPos, finalSize, Textures.GetHeroTexture(target.StoredName()));
            Drawing.DrawRect(startPos, finalSize, new Color(0, 0, 0, 255), true);
            return finalSize;
        }

        private static void DrawShadowText(string stext, int x, int y, Color clr)
        {
            Text.DrawText(null, stext, x + 1, y + 1, Color.Black);
            Text.DrawText(null, stext, x, y, clr);
        }

        private static void CurrentDomainDomainUnload(object sender, EventArgs e)
        {
            try { if (!Checker.IsActive()) return; } catch { }
            Text?.Dispose();
        }

        private static void Drawing_OnPostReset(EventArgs args)
        {
            try { if (!Checker.IsActive()) return; } catch { }
            Text?.OnLostDevice();
        }

        private static void Drawing_OnPreReset(EventArgs args)
        {
            try { if (!Checker.IsActive()) return; } catch { }
            Text?.OnLostDevice();
        }

        #endregion

        #region Helpers
        private static string TryToFindPlayer(string name, bool print = false)
        {
            var webRequest = WebRequest.Create($"https://api.opendota.com/api/search?q={name}&similarity=1");
            string strContent;
            using (var response = webRequest.GetResponse())
            using (var content = response.GetResponseStream())
            using (var reader = new StreamReader(content))
            {
                strContent = reader.ReadToEnd();
                if (print)
                    Console.WriteLine(strContent);
            }
            return strContent;
        }
        private static async Task<string> TryToFindPlayerAsync(string name, bool print = false)
        {
            var request = WebRequest.Create($"https://api.opendota.com/api/search?q={name}&similarity=1");
            string strContent;
            using (var response = (HttpWebResponse)await Task.Factory
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
        private static string GetPlayer(string id)
        {
            var webRequest = WebRequest.Create($"https://api.opendota.com/api/players/{id}");
            string strContent;
            using (var response = webRequest.GetResponse())
            using (var content = response.GetResponseStream())
            using (var reader = new StreamReader(content))
            {
                strContent = reader.ReadToEnd();
                //Console.WriteLine(strContent);
            }
            return strContent;
        }
        private static async Task<string> GetPlayerAsync(uint id)
        {
            var webRequest = WebRequest.Create($"https://api.opendota.com/api/players/{id}");
            string strContent;
            var response = (HttpWebResponse)await Task.Factory
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
        private static int FindWinRate(uint id)
        {
            var webRequest = WebRequest.Create($"https://api.opendota.com/api/players/{id}/wl");
            //((HttpWebRequest)webRequest).UserAgent = ".NET Framework";
            string strContent;
            using (var response = webRequest.GetResponse())
            using (var content = response.GetResponseStream())
            using (var reader = new StreamReader(content))
            {
                strContent = reader.ReadToEnd();
                //Console.WriteLine(strContent);
            }
            int win = Convert.ToInt32(GetValue("win\":", strContent));
            int lose = Convert.ToInt32(GetValue("lose\":", strContent));
            return (int)(win / (win + (double)lose) * 100f);
        }
        private static async Task<int> FindWinRateAsync(uint id)
        {
            var webRequest = WebRequest.Create($"https://api.opendota.com/api/players/{id}/wl");
            //((HttpWebRequest)webRequest).UserAgent = ".NET Framework";
            string strContent;
            var response = (HttpWebResponse)await Task.Factory
                .FromAsync(webRequest.BeginGetResponse,
                    webRequest.EndGetResponse,
                    null);
            using (var content = response.GetResponseStream())
            using (var reader = new StreamReader(content))
            {
                strContent = reader.ReadToEnd();
                //Console.WriteLine(strContent);
            }
            int win = Convert.ToInt32(GetValue("win\":", strContent));
            int lose = Convert.ToInt32(GetValue("lose\":", strContent));
            return (int)(win / (win + (double)lose) * 100f);
        }
        private static async Task<string> FindFullWinRateAsync(uint id)
        {
            var webRequest = WebRequest.Create($"https://api.opendota.com/api/players/{id}/wl");
            //((HttpWebRequest)webRequest).UserAgent = ".NET Framework";
            string strContent;
            var response = (HttpWebResponse)await Task.Factory
                .FromAsync(webRequest.BeginGetResponse,
                    webRequest.EndGetResponse,
                    null);
            using (var content = response.GetResponseStream())
            using (var reader = new StreamReader(content))
            {
                strContent = reader.ReadToEnd();
            }
            int win = Convert.ToInt32(GetValue("win\":", strContent));
            int lose = Convert.ToInt32(GetValue("lose\":", strContent));
            var wr = (int)(win / (win + (double)lose) * 100f);
            return $"({win}/{lose}) {wr}%";
        }
        private static async Task<string> FindMatches(uint id, int gameLimit = 5, int gameMode = 22)
        {
            var webRequest = WebRequest.Create($"https://api.opendota.com/api/players/{id}/matches?limit={gameLimit}&game_mode={gameMode}");
            //((HttpWebRequest)webRequest).UserAgent = ".NET Framework";
            var response = (HttpWebResponse)await Task.Factory
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
                    var win = (playerSlot <= 10 && radiantWin) || (playerSlot > 10 && !radiantWin);
                    info += win ? "+" : "-";
                }
            }
            info += "]";
            return info;
        }
        private static async Task<string> FindInfoAboutHero(uint playerid,uint heroId)
        {
            var webRequest = WebRequest.Create($"https://api.opendota.com/api/players/{playerid}/heroes?hero_id={heroId}");
            string strContent;
            var response = (HttpWebResponse)await Task.Factory
                .FromAsync(webRequest.BeginGetResponse,
                    webRequest.EndGetResponse,
                    null);
            using (var content = response.GetResponseStream())
            using (var reader = new StreamReader(content))
            {
                strContent = reader.ReadToEnd();
                //Console.WriteLine(strContent);
            }
            string trimmed = strContent.Trim();
            var end = trimmed.Substring(0, trimmed.IndexOf('}') + 1);
            return end;
        }

        private static string GetValue(string value, string path2)
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
