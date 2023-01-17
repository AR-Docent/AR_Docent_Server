using AR_Docent_MVC.Config;
using AR_Docent_MVC.Models;
using AR_Docent_MVC.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CognitiveServices.Speech;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace AR_Docent_MVC.Controllers
{
    public class ProductController : Controller
    {
        private AzureKeyVaultService _azureKey;
        private ARBlobStorageService _storageService;
        private TextToAudioService _audioService;
        private SqlDatabaseService<Product> _sqlService;
        
        private List<Product> products;
        public ProductController(AzureKeyVaultService azureKey,
            ARBlobStorageService storageService,
            TextToAudioService audioService,
            SqlDatabaseService<Product> sqlService) : base()
        {
            products = new List<Product>();
            _azureKey = azureKey;
            _storageService = storageService;
            _audioService = audioService;
            _sqlService = sqlService;
        }

        // GET: ProductController
        public ActionResult Index()
        {
            try
            {
                products = _sqlService.GetItems("product");
                ViewData["items"] = products;
            }
            catch (Exception ex)
            {
                return View(ex);
            }
            return View();
        }

        // GET: ProductController/Details/5
        public ActionResult Details(int id)
        {
            try
            {
                Product product = _sqlService.GetItemById("product", id);
                product.img_name = _storageService.GetItemUrl(ServerConfig.imgContainerName, product.img_name);
                product.audio_name = _storageService.GetItemUrl(ServerConfig.audioContainerName, product.audio_name);
                Debug.WriteLine(product.id.ToString(), product.title, product.name, product.created_at, product.content, product.img_name, product.audio_name);
                return View(product);

            }
            catch (SqlException ex)
            {
                return View(ex);
            }
        }

        // GET: ProductController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ProductController/Create
        //public ActionResult Create(IFormCollection collection)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([ModelBinder]Product product)
        {
            try
            {
                product.img_name =  _storageService.StringGenerator(8) + Path.GetExtension(product.image.FileName);
                product.audio_name = _storageService.StringGenerator(8) + ".wav";
                Task.Run(async ()=>
                {
                    await _storageService.Upload(product.image, ServerConfig.imgContainerName, product.img_name);
                    byte[] audio = await _audioService.TextToSpeech(product.content);
                    await _storageService.Upload(audio, ServerConfig.audioContainerName, product.audio_name);
                });
                _sqlService.AddItem("product", product);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
        /*
        // GET: ProductController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            return View();
        }

        // POST: ProductController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
        */
        // GET: ProductController/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                string del_imgName = null;
                string del_audioName = null;

                Product product = _sqlService.GetItemById("product", id);
                del_audioName = product.audio_name;
                del_imgName = product.img_name;
                
                if (del_imgName == null || del_audioName == null)
                {
                    return NotFound();
                }

                await _storageService.Delete(ServerConfig.imgContainerName, del_imgName);
                await _storageService.Delete(ServerConfig.audioContainerName, del_audioName);
                _sqlService.DeleteByID("product", id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                //return View();
                Debug.WriteLine(ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }
        /*
        // POST: ProductController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
        */
    }
}
