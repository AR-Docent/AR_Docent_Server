using AR_Docent.website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System;
using System.Linq;

namespace AR_Docent.website.Pages.Products
{
    public class createModel : PageModel
    {
        private static readonly string connectionString = "Server = tcp:ar-docent-server.database.windows.net,1433;Initial Catalog = AR_Docent_Data; Persist Security Info=False;User ID = admin_; Password=1q2w3e4r!; MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout = 30;";

        public ProductInfo productInfo = new ProductInfo();
        public string errorMessage = "";
        public string successMessage = "";
        
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
                using (SqlConnection connection = new SqlConnection(connectionString))
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
