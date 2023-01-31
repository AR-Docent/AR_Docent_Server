using AR_Docent_MVC.Config;
using AR_Docent_MVC.Models;
using AR_Docent_MVC.Service;
using Azure.AI.Vision.Core.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.VisualStudio.Web.CodeGeneration.Contracts.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
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
            List<UnityInfo> info = new();

            try
            {
                for (int i = 0; i < products.Count; i++)
                {
                    UnityInfo item = new()
                    {
                        id = products[i].id,
                        name = products[i].name,
                        audio_name = products[i].audio_name,
                        image_name = products[i].img_name,
                        image_url = _storageService.GetItemDownloadUrl(ServerConfig.imgContainerName, products[i].img_name),
                        audio_url = _storageService.GetItemDownloadUrl(ServerConfig.audioContainerName, products[i].audio_name),
                        content = products[i].content,
                    };
                    info.Add(item);
                }
                return WebUtility.HtmlEncode(JsonSerializer.Serialize<IEnumerable<UnityInfo>>(info,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                    }
                ));
            }
            catch (Exception e)
            {
                Debug.Write(e.Message);
                return HttpStatusCode.InternalServerError.ToString() + e.Message;
            }
        }
    }
}
