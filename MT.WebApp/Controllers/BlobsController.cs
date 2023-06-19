using Microsoft.AspNetCore.Mvc;
using MT.AzureStorageLib.Services.Interfaces;
using MT.WebApp.Models;

namespace MT.WebApp.Controllers
{
  
        public class BlobsController : Controller
        {
            private readonly IBlobStorage _blobStorage;

            public BlobsController(IBlobStorage blobStorage)
            {
                _blobStorage = blobStorage;
            }

            public async Task<IActionResult> Index()
            {
            var imgPath = "img/excel.png";
                var names = _blobStorage.GetNames(EContainerName.excel);
                string blobUrl = $"{_blobStorage.BlobUrl}/{EContainerName.excel.ToString()}";
                ViewBag.blobs = names.Select(x => new FileBlob { Name = x, Url = imgPath }).ToList();

               
                return View();
            }

            [HttpPost]
            public async Task<IActionResult> Upload(IFormFile picture)
            {
     

                var newFileName = Guid.NewGuid().ToString() + Path.GetExtension(picture.FileName);

                await _blobStorage.UploadAsync(picture.OpenReadStream(), newFileName, EContainerName.excel);

                
                return RedirectToAction("Index");
            }

            [HttpGet]
            public async Task<IActionResult> Download(string fileName)
            {
                var stream = await _blobStorage.DownloadAsync(fileName, EContainerName.excel);

                return File(stream, "application/octet-stream", fileName);
            }

            [HttpGet]
            public async Task<IActionResult> Delete(string fileName)
            {
                await _blobStorage.DeleteAsync(fileName, EContainerName.excel);
                return RedirectToAction("Index");
            }
        }
    }

