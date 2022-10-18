using AR_Docent.website.Models;
using AR_Docent.website.Services;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System;
using System.Linq;

namespace AR_Docent.website.Pages.Products
{
    public class createModel : PageModel
    {
        public ProductInfo productInfo { get; private set; } = new ProductInfo();
        public string errorMessage { get; private set; } = "";
        public string successMessage { get; private set; } = "";
        
        public void OnGet()
        {
        }

        public void OnPost()
        {
            productInfo.title = Request.Form["title"];
            productInfo.name = Request.Form["name"];
            productInfo.content = Request.Form["content"];

            if (productInfo.content.Length == 0 || productInfo.name.Length == 0 ||
                productInfo.title.Length == 0)
            {
                errorMessage = "All the fields are required.";
                return;
            }
            //save the new productinfo in to the database
            try
            {
                ImageStorage imageStorage = new ImageStorage();
                imageStorage.Initialize(StorageConfig.connectionString, StorageConfig.imageContainerName);
                imageStorage.Save(productInfo.image, productInfo.id.ToString());
                
                using (SqlConnection connection = new SqlConnection(SqlConfig.connectionString))
                {
                    connection.Open();
                    string _sqlQuery = "INSERT INTO product " +
                                        "(title, name, content) VALUES " +
                                        "(@title, @name, @content);";
                    using (SqlCommand cmd = new SqlCommand(_sqlQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@title", productInfo.title);
                        cmd.Parameters.AddWithValue("@name", productInfo.name);
                        cmd.Parameters.AddWithValue("@content", productInfo.content);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return;
            }
            successMessage = "New ProductInfo Added Correctly.";
            productInfo = new ProductInfo();

            Response.Redirect("/Products/Index");
        }
    }
}
