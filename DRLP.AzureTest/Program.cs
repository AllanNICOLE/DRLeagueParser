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

namespace DRLP.AzureTest
{
    class Program
    {
        

        static void Main(string[] args)
        {
            string eventId = ConfigurationManager.AppSettings["EventId"];

            Task<Rally> t = Task<Rally>.Factory.StartNew(() =>
            {
                RacenetApiParser racenetApiParser = new RacenetApiParser(ConfigurationManager.AppSettings["LeagueURL"]);
                return racenetApiParser.GetRallyData(eventId, null);
            });
            
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse
                    (ConfigurationManager.AppSettings["StorageConnectionString"]);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("Rally");
            TableOperation retrieveOperation = TableOperation.Retrieve<RallyEntity>("1", eventId);
            Task<TableResult> retrievedResult = table.ExecuteAsync(retrieveOperation);
            retrievedResult.Wait();
            if (retrievedResult.Result.Result != null)
            {
                RallyEntity entity = ((RallyEntity)retrievedResult.Result.Result);
                Rally rallyData = JsonConvert.DeserializeObject<Rally>(entity.Data);
                if (t.Result.StageCount >= rallyData.StageCount)
                {
                    entity.Data = JsonConvert.SerializeObject(t.Result, Formatting.Indented);
                    retrieveOperation = TableOperation.Replace(entity);
                    table.Execute(retrieveOperation);
                    Console.WriteLine("Data updated in Azure !");
                }                
            } else
            {
                RallyEntity re = new RallyEntity(eventId, "1");
                re.Data = JsonConvert.SerializeObject(t.Result, Formatting.Indented);
                re.Info = ConfigurationManager.AppSettings["EventInfo"];
                TableOperation insertOperation = TableOperation.Insert(re);
                table.Execute(insertOperation);
                Console.WriteLine("Data added in Azure !");
            }
            Console.Read();
        }
    }

    class RallyEntity : TableEntity
    {
        public RallyEntity(string eventId, string leagueId)
        {
            PartitionKey = leagueId;
            RowKey = eventId;            
        }
        public RallyEntity()
        {

        }
        public string Data { get; set; }
        public string Info { get; set; }
    }
}
