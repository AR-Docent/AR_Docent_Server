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
using System.Text.Json;

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

        public UnityController(
            ILogger<UnityController> logger,
            ARBlobStorageService storageService,
            SqlDatabaseService<Product> sqlService
            ) : base()
        {
            _storageService = storageService;
            _sqlService = sqlService;
            _logger = logger;
        }

        // GET: api/<ValuesController>
        [HttpGet]
        public ActionResult<string> Get()
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
                        Content = products[i].content,
                        Image_width = products[i].img_width,
                    };
                    _logger.LogDebug($"item {i} finish");
                    info.Add(item);
                }
                return Encoding.UTF8.GetString(JsonSerializer.SerializeToUtf8Bytes<IEnumerable<UnityInfo>>(info));
            }
            catch (Exception e)
            {
                Debug.Write(e.Message);
                _logger.LogDebug(e.StackTrace);
                return StatusCode(500);
            }
        }

        [HttpGet("GetImageById/{id}")]
        public ActionResult<string> GetImageById(int id)
        {
            Product product = _sqlService.GetItemById("product", id);
            if (product == null)
            {
                return NotFound();
            }
            string url = _storageService.GenerateSasBlob(ServerConfig.imgContainerName, product.img_name);
            return Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(url));
        }

        [HttpGet("GetAudioById/{id}")]
        public ActionResult<string> GetAudioById(int id)
        {
            Product product = _sqlService.GetItemById("product", id);
            if (product == null)
            {
                return NotFound();
            }
            string url = _storageService.GenerateSasBlob(ServerConfig.audioContainerName, product.audio_name);
            return Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(url));
        }
    }
}
