using Microsoft.AspNetCore.Mvc;
using MT.AzureStorageLib.Entities;
using MT.AzureStorageLib.Services.Concrete;

namespace MT.WebApp.Controllers
{
    public class PersonController : Controller
    {
        private readonly TableStorage<Person> _noSqlStorage;

        public PersonController(TableStorage<Person> noSqlStorage)
        {
            _noSqlStorage = noSqlStorage;
        }

        public IActionResult Index()
        {
            ViewBag.people = _noSqlStorage.All().ToList();
            ViewBag.IsUpdate = false;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Person person)
        {
            person.City = person.PartitionKey;
            person.UserId = person.RowKey;

            await _noSqlStorage.AddAsync(person);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Update(string rowKey, string partitionKey)
        {
            var product = await _noSqlStorage.GetAsync(rowKey, partitionKey);

            ViewBag.people = _noSqlStorage.All().ToList();
            ViewBag.IsUpdate = true;

            return View("Index", product);
        }

        [HttpPost]
        public async Task<IActionResult> Update(Person person)
        {
            ViewBag.IsUpdate = true;

            await _noSqlStorage.UpdateAsync(person);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string rowKey, string partitionKey)
        {
            await _noSqlStorage.DeleteAsync(rowKey, partitionKey);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Query(string name)
        {
            ViewBag.IsUpdate = false;
            ViewBag.people = _noSqlStorage.Query(x => x.Name == name).ToList();

            return View("Index");
        }
    }
}
