using AR_Docent_MVC.Models;
using AR_Docent_MVC.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AR_Docent_MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private AzureKeyVaultService _azureKeyVaultService;
        private ARBlobStorageService _blobStorageService;
        private SqlDatabaseService<Product> _sqlService;

        public HomeController(ILogger<HomeController> logger,
            AzureKeyVaultService azureKeyVaultService,
            ARBlobStorageService blobStorage,
            SqlDatabaseService<Product> sqlService)
        {
            _logger = logger;
            _azureKeyVaultService = azureKeyVaultService;
            _blobStorageService = blobStorage;
            _sqlService = sqlService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
