using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace DRLP.Services
{
    /// <summary>
    /// Extract data from DiRT WebSite
    /// </summary>
    public class DiRTWebSiteParser
    {
        public string LeagueURL { get; set; } = "https://www.dirtgame.com/fr/leagues/league/";

        public LeagueInfo GetLeagueInfo(int leagueId)
        {
            HtmlWeb webParser = new HtmlWeb();
            var htmlDoc = webParser.Load(LeagueURL + leagueId.ToString());
            return new LeagueInfo
            {
                LeagueId = leagueId,
                CurrentEventId = Convert.ToInt32(htmlDoc.DocumentNode.SelectNodes("//div[@data-ng-event-id]").FirstOrDefault().Attributes["data-ng-event-id"].Value),
                LeagueTitle = htmlDoc.DocumentNode.SelectSingleNode("//title").InnerText.Split('|')[0].Trim(),
            };
        }
    }
}
