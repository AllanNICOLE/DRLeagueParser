using DRLP.Data;
using System;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Web;

namespace DRLP.Services
{
    public class RacenetApiParser
    {
        private HttpClient httpClient;

        public RacenetApiParser(string apiURL)
        {
            ApiURL = apiURL;
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.CookieContainer = new CookieContainer();
            httpClient = new HttpClient(httpClientHandler, true);
        }

        public string ApiURL { get; set; }

        /// <summary>
        /// Creates the URIs and parses responses into Rally objects
        /// </summary>
        public Rally GetRallyData(string eventId, IProgress<int> progress)
        {
            try
            {
                var uriBuilder = new UriBuilder(ApiURL);
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                query["eventId"] = eventId;
                query["noCache"] = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds.ToString();

                // get the overall results and stage count
                query["page"] = "1";
                query["stageId"] = "0";                 // overall results
                uriBuilder.Query = query.ToString();
                var requestUri = uriBuilder.ToString();

                var rallyDataResult = ExecuteRequest(requestUri).Result;

                var totalStages = rallyDataResult.TotalStages;
                var rallyData = new Rally();

                // for each stage
                for (int i = 1; i <= totalStages; i++)
                {
                    var currentPage = 1;
                    var totalPages = 99;
                    var leaderboardTotal = 99;
                    var stageData = new Stage();

                    // for each page in stage
                    var retryCount = 0;
                    while (currentPage <= totalPages)
                    {
                        // return what we have, the rally may not have been finished yet
                        // todo: status messaging?
                        if (retryCount >= 10)
                            return rallyData;

                        query["page"] = currentPage.ToString();
                        query["stageId"] = i.ToString();        // stage number
                        uriBuilder.Query = query.ToString();
                        requestUri = uriBuilder.ToString();

                        rallyDataResult = ExecuteRequest(requestUri).Result;

                        // sometimes we don't get entries, if so, retry the request (this might be fixed by storing cookies)
                        if (rallyDataResult == null || rallyDataResult.Entries == null || rallyDataResult.Entries.Count < 1)
                        {
                            retryCount++;
                            continue;
                        }

                        leaderboardTotal = rallyDataResult.LeaderboardTotal;
                        totalPages = rallyDataResult.Pages;

                        if (totalPages == 0 || leaderboardTotal == 0)
                            break;

                        // for each driver in page
                        foreach (var entry in rallyDataResult.Entries)
                            stageData.AddDriver(new DriverTime(entry.Position, entry.PlayerId, entry.Name, entry.VehicleName, entry.Time, entry.DiffFirst));

                        // increment page
                        currentPage++;
                    }

                    if (stageData.Count != leaderboardTotal)
                            throw new Exception("Racenet data incomplete for stage " + i);

                    rallyData.AddStage(stageData);
                    if (progress != null)
                        progress.Report(rallyData.StageCount);
                }

                return rallyData;
            }
            catch (Exception)
            {
                return null;
            }
            
        }

        /// <summary>
        /// Performs the web requests
        /// </summary>
        private async Task<RacenetRallyData> ExecuteRequest(string requestUri)
        {
            var serializer = new DataContractJsonSerializer(typeof(RacenetRallyData));

            var response = httpClient.GetAsync(requestUri).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseStream = await response.Content.ReadAsStreamAsync();
                var racenetStageData = serializer.ReadObject(responseStream) as RacenetRallyData;
                return racenetStageData;
            }

            return null;
        }
    }
}
