using Microsoft.AspNetCore.Mvc;
using MT.AzureStorageLib.Entities;
using MT.AzureStorageLib.Services.Concrete;
using MT.AzureStorageLib.Services.Interfaces;
using MT.WebApp.Models;
using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Text;

namespace MT.WebApp.Controllers
{
    public class FileController : Controller
    {


        private readonly TableStorage<Person> _noSqlStorage;
        private readonly TableStorage<Report> _reportStorage;
        private readonly IBlobStorage _blobStorage;

        public FileController(TableStorage<Person> noSqlStorage, IBlobStorage blobStorage, TableStorage<Report> reportStorage)
        {
            _noSqlStorage = noSqlStorage;
            _blobStorage = blobStorage;
            _reportStorage = reportStorage;
        }


        public async Task<IActionResult> Index()
        {
           
            var person = _noSqlStorage.All().ToList();
            ViewBag.people=person;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddNumberToPerson(string userId, string city, string phoneNumbers)
        {
            List<string> numberList = new List<string>();
           numberList.Add(phoneNumbers);
         

            var isUser = await _noSqlStorage.GetAsync(userId, city);

            if (isUser != null)
            {
                if (isUser.PhoneNumber != null) numberList.AddRange(isUser.PhoneNumber);

                isUser.PhoneNumber = numberList;
            }
        

            await _noSqlStorage.AddAsync(isUser);

            return RedirectToAction("Index");
        }



        [HttpPost]
        public async Task<IActionResult> SendPersonToQueue(UserContactFileQueue userContactFileQueue)

        {

            var report = new Report();
            
            report.RequestDate = DateTime.Now;
            report.ReportState = FileStatus.Creating.ToString();
            report.UserId=userContactFileQueue.UserId;
            report.ReportId=Guid.NewGuid().ToString();
            report.RowKey = report.ReportId;
            report.PartitionKey = report.UserId;
            report.ETag = null;
           var createdReport = await _reportStorage.AddAsync(report);
            userContactFileQueue.ReportId = createdReport.ReportId;

            var jsonString = JsonConvert.SerializeObject(userContactFileQueue);

                var jsonStringBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonString));

                // to get AzureQueue Object by Reflection
                Assembly disAssembly = Assembly.Load("MT.AzureStorageLib");
                var azQueueType = disAssembly.GetType($"{disAssembly.GetName().Name}.Services.Concrete.AzQueue");

                var azQueue = Activator.CreateInstance(azQueueType, new object[] { "textnumberqueue" });
                await (Task)azQueueType.GetTypeInfo()
                  .GetDeclaredMethod("SendMessageAsync")
                  .Invoke(azQueue, new object[] { $"{jsonStringBase64}" });

                return Ok("Files  is being created.");
           
           





            //AzQueue azQueue = new AzQueue("textimagequeue");
            // await azQueue.SendMessageAsync(jsonStringBase64);


        }
    }
}
