using DRLP.Data;
using DRLP.Services;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.Azure;
using HtmlAgilityPack;

namespace DRLP.AzureTest
{
    class Program
    {
        const string TableReference = "Rally";

        static void Main(string[] args)
        {
            var leaguesId = CloudConfigurationManager.GetSetting("LeaguesId").Split(';');
            if (!leaguesId.Any())
                Console.Write("ERROR : No league to check. Please set the LeaguesId setting");

            foreach (var item in leaguesId)
            {
                RallyEntity r = GetCurrentEventId(item);
                Console.WriteLine($"Retreiving data for league {item} - {r.LeagueTitle} => {r.EventId}");

                Task<Rally> t = Task<Rally>.Factory.StartNew(() =>
                {
                    RacenetApiParser racenetApiParser = new RacenetApiParser(CloudConfigurationManager.GetSetting("EventAPI"));
                    return racenetApiParser.GetRallyData(r.EventId, null);
                });

                CloudStorageAccount storageAccount = CloudStorageAccount.Parse
                        (CloudConfigurationManager.GetSetting("StorageConnectionString"));
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                CloudTable table = tableClient.GetTableReference(TableReference);
                TableOperation retrieveOperation = TableOperation.Retrieve<RallyEntity>(r.LeagueId, r.EventId);
                TableResult retrievedResult = table.Execute(retrieveOperation);
                if (retrievedResult.Result != null)
                {
                    RallyEntity entity = ((RallyEntity)retrievedResult.Result);
                    Rally rallyData = JsonConvert.DeserializeObject<Rally>(entity.Data);
                    if (t.Result.StageCount >= rallyData.StageCount)
                    {
                        entity.Data = JsonConvert.SerializeObject(t.Result);
                        retrieveOperation = TableOperation.Replace(entity);
                        table.Execute(retrieveOperation);
                        Console.WriteLine("Data updated in Azure !");
                    }
                }
                else
                {
                    r.Data = JsonConvert.SerializeObject(t.Result);
                    TableOperation insertOperation = TableOperation.Insert(r);
                    table.Execute(insertOperation);
                    Console.WriteLine("Data added in Azure !");
                }
                Console.WriteLine($"Done...");
            }            
        }

        static RallyEntity GetCurrentEventId(string leagueId)
        {
            var html = CloudConfigurationManager.GetSetting("LeagueURL") + leagueId;
            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load(html);
            string eventId = htmlDoc.DocumentNode.SelectNodes("//div[@data-ng-event-id]").FirstOrDefault().Attributes["data-ng-event-id"].Value;
            string leagueTitle = htmlDoc.DocumentNode.SelectSingleNode("//title").InnerText.Split('|')[0].Trim();
            return new RallyEntity
            {
                PartitionKey = leagueId,
                RowKey = eventId,
                LeagueTitle = leagueTitle,
            };
        }
    }

    class RallyEntity : TableEntity
    {        public RallyEntity()
        {

        }
        public string Data { get; set; }
        public string LeagueTitle { get; set; }
        public string EventId { get { return RowKey; } }
        public string LeagueId { get { return PartitionKey; } }
    }
}
