using AR_Docent_MVC.Config;
using AR_Docent_MVC.Models;
using AR_Docent_MVC.Service;
using Azure.AI.Vision.Core.Internal;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AR_Docent_MVC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnityController : ControllerBase
    {
        private ARBlobStorageService _storageService;
        private SqlDatabaseService<Product> _sqlService;

        public UnityController(ARBlobStorageService storageService,
            SqlDatabaseService<Product> sqlService) : base()
        {
            _storageService = storageService;
            _sqlService = sqlService;
        }



        // GET: api/<ValuesController>
        [HttpGet]
        public string Get()
        {
            List<Product> products = _sqlService.GetItems("product");
            for (int i = 0; i < products.Count; i++)
            {
                products[i].img_name = _storageService.GetItemDownloadUrl(ServerConfig.imgContainerName, products[i].img_name);
                products[i].audio_name = _storageService.GetItemDownloadUrl(ServerConfig.audioContainerName, products[i].audio_name);
            }

            return JsonSerializer.Serialize<IEnumerable<Product>>(products, 
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                }
            );
        }
    }
}
