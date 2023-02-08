using AR_Docent_MVC.Config;
using AR_Docent_MVC.Models;
using AR_Docent_MVC.Service;
using Azure.AI.Vision.Core.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Web.CodeGeneration.Contracts.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AR_Docent_MVC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnityController : ControllerBase
    {
        private ARBlobStorageService _storageService;
        private SqlDatabaseService<Product> _sqlService;

        private ILogger<UnityController> _logger;

        private SasTokenContainer _sasTokens;

        public UnityController(
            ILogger<UnityController> logger,
            ARBlobStorageService storageService,
            SqlDatabaseService<Product> sqlService
            ) : base()
        {
            _storageService = storageService;
            _sqlService = sqlService;
            _logger = logger;

            _sasTokens = new SasTokenContainer();
            foreach (string name in ServerConfig.containers)
            {
                _sasTokens[name] = _storageService.GenerateSasBlob(name);
            }
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
                    _logger.LogDebug($"item {i} start");
                    UnityInfo item = new()
                    {
                        Id = products[i].id,
                        Name = products[i].name,
                        Title = products[i].title,
                        Audio_name = products[i].audio_name,
                        Image_name = products[i].img_name,
                        Image_url = GetDownloadUrl(ServerConfig.imgContainerName, products[i].img_name),
                        Audio_url = GetDownloadUrl(ServerConfig.audioContainerName, products[i].audio_name),
                        Content = products[i].content,
                    };
                    _logger.LogDebug($"item {i} finish");
                    info.Add(item);
                }
                /*
                return WebUtility.HtmlEncode(Encoding.UTF8.GetString(JsonSerializer.SerializeToUtf8Bytes<List<UnityInfo>>(info,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        //Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                        //Encoder = default,
                    })
                    ));
                */
                return Encoding.UTF8.GetString(JsonSerializer.SerializeToUtf8Bytes<IEnumerable<UnityInfo>>(info));
            }
            catch (Exception e)
            {
                Debug.Write(e.Message);
                _logger.LogDebug(e.StackTrace);
                return HttpStatusCode.InternalServerError.ToString() + e.Message;
            }
        }

        private string GetDownloadUrl(string containerName, string name)
        {
            return _storageService.GetItemUrl(containerName, name) + "?" + _sasTokens[containerName];
        }
    }
}
