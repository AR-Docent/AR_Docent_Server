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
            /*
            List<UnityInfo> info = new List<UnityInfo>(products.Count);

            for (int i = 0; i < products.Count; i++)
            {
                info[i].id = products[i].id;
                info[i].name = products[i].name;
                info[i].audio_name = products[i].audio_name;
                info[i].image_name = products[i].img_name;
                info[i].image_url = _storageService.GetItemDownloadUrl(ServerConfig.imgContainerName, products[i].img_name);
                info[i].audio_url = _storageService.GetItemDownloadUrl(ServerConfig.audioContainerName, products[i].audio_name);
                info[i].content = products[i].content;
            }
            
            return JsonSerializer.Serialize<IEnumerable<UnityInfo>>(info, 
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                }
            );
            */
            return JsonSerializer.Serialize<IEnumerable<Product>>(products,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                }
            );
        }
    }
}
