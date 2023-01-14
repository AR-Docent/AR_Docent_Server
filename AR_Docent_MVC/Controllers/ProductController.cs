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
        public async Task<ActionResult> Index()
        {
            try
            {
                /*
                using (SqlConnection connection = new SqlConnection(_azureKey.sqlConnectionString))
                {
                    connection.Open();
                    string _query = "SELECT * FROM product";
                    using (SqlCommand cmd = new SqlCommand(_query, connection))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Product product = new Product();

                                product.id = reader.GetInt32(0);
                                product.title = reader.GetString(1);
                                product.name = reader.GetString(2);
                                product.created_at = reader.GetDateTime(3);
                                product.content = reader.GetString(4);
                                product.img_name = reader.GetString(5);

                                products.Add(product);
                            }
                        }
                    }
                }
                */
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
        public async Task<ActionResult> Details(int id)
        {
            //Product product = new Product();
            try
            {
                /*
                using (SqlConnection connection = new SqlConnection(_azureKey.sqlConnectionString))
                {
                    connection.Open();
                    string sql = "SELECT * FROM product WHERE id=@id";
                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                product.id = reader.GetInt32(0);
                                product.title = reader.GetString(1);
                                product.name = reader.GetString(2);
                                product.created_at = reader.GetDateTime(3);
                                product.content = reader.GetString(4);
                                product.img_name = _storageService.GetItemUrl(ServerConfig.imgContainerName, reader.GetString(5));
                                product.audio_name = _storageService.GetItemUrl(ServerConfig.audioContainerName, reader.GetString(6));
                            }
                        }
                    }
                }
                */
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
                /*
                using (SqlConnection connection = new SqlConnection(_azureKey.sqlConnectionString))
                {
                    connection.Open();
                    string _sqlQuery = "INSERT INTO product " +
                                        "(title, name, content, img_name, audio_name) VALUES " +
                                        "(@title, @name, @content, @img_name, @audio_name);";
                    using (SqlCommand cmd = new SqlCommand(_sqlQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@title", product.title);
                        cmd.Parameters.AddWithValue("@name", product.name);
                        cmd.Parameters.AddWithValue("@content", product.content);
                        cmd.Parameters.AddWithValue("@img_name", product.img_name);
                        cmd.Parameters.AddWithValue("@audio_name", product.audio_name);

                        cmd.ExecuteNonQuery();
                    }
                }
                */
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
                /*
                using (SqlConnection connection = new SqlConnection(_azureKey.sqlConnectionString))
                {
                    connection.Open();

                    string del_imgName = null;
                    string del_audioName = null;

                    string sql = "SELECT * FROM product WHERE id=@id";
                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                del_imgName = reader.GetString(5);
                                del_audioName = reader.GetString(6);
                            }
                        }
                    }
                }
                */
                Product product = _sqlService.GetItemById("product", id);
                del_audioName = product.audio_name;
                del_imgName = product.img_name;
                
                if (del_imgName == null || del_audioName == null)
                {
                    return NotFound();
                }

                await _storageService.Delete(ServerConfig.imgContainerName, del_imgName);
                await _storageService.Delete(ServerConfig.audioContainerName, del_audioName);
                /*
                sql = "DELETE FROM product WHERE id=@id";
                using (SqlCommand cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
                */
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
