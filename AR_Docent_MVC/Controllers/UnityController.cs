﻿using AR_Docent_MVC.Config;
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
        public string Get()
        {
            List<Product> products = _sqlService.GetItems("product");
            List<UnityInfo> info = new();

            try
            {
                for (int i = 0; i < products.Count; i++)
                {
                    _logger.LogInformation($"item {i} start");
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
                    _logger.LogInformation($"item {i} finish");
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
                _logger.LogInformation(e.StackTrace);
                return HttpStatusCode.InternalServerError.ToString() + e.Message;
            }
        }
    }
}
