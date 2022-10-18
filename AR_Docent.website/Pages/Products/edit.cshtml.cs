using AR_Docent.website.Models;
using AR_Docent.website.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System;

namespace AR_Docent.website.Pages.Products
{
    public class editModel : PageModel
    {
        public ProductInfo productInfo { get; private set; } = new ProductInfo();
        public string errorMessage { get; private set; } = "";
        public string successMessage { get; private set; } = "";
        
        public void OnGet()
        {
            try
            {
                string _id = Request.Query["id"];
                using (SqlConnection connection = new SqlConnection(SqlConfig.connectionString))
                {
                    connection.Open();
                    string _sqlQuery = "SELECT * FROM product WHERE id=@id";
                    using (SqlCommand cmd = new SqlCommand(_sqlQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@id", _id);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                productInfo.id = reader.GetInt32(0);
                                productInfo.title = reader.GetString(1);
                                productInfo.name = reader.GetString(2);
                                productInfo.created_at = reader.GetDateTime(3).ToString();
                                productInfo.content = reader.GetString(4);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return;
            }
        }
        
        public void OnPost()
        {
            productInfo.id = int.Parse(Request.Form["id"]);
            productInfo.title = Request.Form["title"];
            productInfo.name = Request.Form["name"];
            productInfo.created_at = Request.Form["created_at"];
            productInfo.content = Request.Form["content"];

            if (productInfo.content.Length == 0 || productInfo.name.Length == 0 ||
                productInfo.title.Length == 0)
            {
                errorMessage = "All the fields are required.";
                return;
            }
            try
            {
                using (SqlConnection connection = new SqlConnection(SqlConfig.connectionString))
                {
                    connection.Open();
                    string _sqlQuery = "UPDATE product " +
                                        "SET title=@title, name=@name, content=@content " +
                                        "WHERE id=@id";
                    using (SqlCommand cmd = new SqlCommand(_sqlQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@title", productInfo.title);
                        cmd.Parameters.AddWithValue("@name", productInfo.name);
                        cmd.Parameters.AddWithValue("@content", productInfo.content);
                        cmd.Parameters.AddWithValue("@id", productInfo.id);

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
