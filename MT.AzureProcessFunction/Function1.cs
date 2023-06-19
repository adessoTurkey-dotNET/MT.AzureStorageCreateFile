using System;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using System.Data;
using System.IO;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using MT.AzureStorageLib.Entities;
using System.Threading.Tasks;
using MT.AzureStorageLib.Services.Interfaces;
using MT.AzureStorageLib.Services.Concrete;
using System.Linq;
using Azure.Storage.Queues.Models;


namespace MT.AzureProcessFunction
{
    public class Function1
    {


        [FunctionName("Function1")]
        public async  Task Run([QueueTrigger("textnumberqueue")] UserContactFileQueue userContactFileQueue, ILogger log)
        {
            ConnectionString.AzureStorageConnectionString = @$"{Environment.GetEnvironmentVariable("connString")}";
            

            IBlobStorage _blobStorage = new BlobStorage();
            TableStorage<Person> _noSqlStorage = new TableStorage<Person>();
            TableStorage<Report> _reportStorage = new TableStorage<Report>();
            try
            {
                var isUser = await _noSqlStorage.GetAsync(userContactFileQueue.UserId, userContactFileQueue.City);
                using var memoryStream = new MemoryStream();
                var wb = new XLWorkbook();
                var ds = new DataSet();
                ds.Tables.Add(await GetTable("persons"));

                wb.Worksheets.Add(ds);
                wb.SaveAs(memoryStream);


                memoryStream.Position = 0;

                await _blobStorage.UploadAsync(memoryStream, isUser.Name + userContactFileQueue.ReportId + ".xlsx", EContainerName.excel);

                log.LogInformation($"File was added to {isUser.Name}.");
              var currentReport=  await _reportStorage.GetAsync(userContactFileQueue.ReportId,userContactFileQueue.UserId);
                currentReport.ReportState = FileStatus.Completed.ToString();
                currentReport.CreatedDate = DateTime.Now;
                await _reportStorage.UpdateAsync(currentReport);

                HttpClient httpClient = new HttpClient();
                HttpResponseMessage response;
                try
                {
                    response = await httpClient.GetAsync("http://localhost:4037/api/Notification/CompleteCreatingProcess/" + userContactFileQueue.connectionId);

                }
                catch (Exception ex)
                {
                    log.LogInformation($"Function Error :  {ex.ToString()}");
                    throw ex;
                }
              
                log.LogInformation($" Client {response.StatusCode} {response.Content} was informed.");
                log.LogInformation($" Client {userContactFileQueue.connectionId} was informed.");

            }
            catch (Exception ex)
            {
            log.LogInformation($" hataaaa {ex.Message} ");
            }

        }


        private async  Task<DataTable> GetTable(string tableName)
        {
            TableStorage<Person> _noSqlStorage = new TableStorage<Person>();
            DataTable table = new DataTable { TableName = tableName };
            table.Columns.Add("Location", typeof(string));
            table.Columns.Add("PersonCount", typeof(int));
            table.Columns.Add("PhoneNumber", typeof(int));

            var people =  _noSqlStorage.All().ToList();
          

            var qry = from cont in people
                      where cont. RowKey!= null
                      group cont by cont.PartitionKey
             into grp
                      select new
                      {
                          Location = grp.Key,
                          PersonCount = grp.Select(x => x.UserId).Distinct().Count(),
                          PhoneNumber = grp.Select(x => x.PhoneNumber).Count()
                      };

            foreach (var row in qry.OrderBy(x => x.PersonCount))
            {
                table.Rows.Add(row.Location, row.PersonCount, row.PhoneNumber);
                Console.WriteLine("{0}: {1} : {2}", row.Location, row.PersonCount, row.PhoneNumber);
            }

            return table;
        }
    }
}

