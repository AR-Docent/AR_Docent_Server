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
            List<UnityInfo> info = new List<UnityInfo>();

            for (int i = 0; i < products.Count; i++)
            {
                UnityInfo infoItem = new UnityInfo();

                infoItem.id = products[i].id;
                infoItem.name = products[i].name;
                infoItem.audio_name = products[i].audio_name;
                infoItem.image_name = products[i].img_name;
                infoItem.image_url = _storageService.GetItemDownloadUrl(ServerConfig.imgContainerName, products[i].img_name);
                infoItem.audio_url = _storageService.GetItemDownloadUrl(ServerConfig.audioContainerName, products[i].audio_name);
                infoItem.content = products[i].content;

                info.Add(infoItem);
            }

            return JsonSerializer.Serialize<IEnumerable<UnityInfo>>(info, 
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                }
            );
        }
    }
}
