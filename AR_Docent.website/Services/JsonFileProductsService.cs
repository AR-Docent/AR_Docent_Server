using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using AR_Docent.website.Models;
using Microsoft.AspNetCore.Hosting;

namespace AR_Docent.website.Services
{
    public class JsonFileProductsService
    {
        public IWebHostEnvironment WebHostEnvironment { get; private set; }

        public JsonFileProductsService(IWebHostEnvironment webHostEnvironment)
        {
            WebHostEnvironment = webHostEnvironment;
        }

        private string JsonFileName
        {
            get => Path.Combine(WebHostEnvironment.WebRootPath, "data", "products.json");
        }

        public IEnumerable<Product> GetProducts()
        {
            using (var jsonFileReader = File.OpenText(JsonFileName))
            {
                return JsonSerializer.Deserialize<Product[]>(jsonFileReader.ReadToEnd(),
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
            }
        }

        public void AddRating(string productId, int rating)
        {
            IEnumerable<Product> products = GetProducts();
            Product query = products.First(x => x.ID == productId);

            if (query.Rating == null)
            {
                query.Rating = new int[] { rating };
            }
            else
            {
                List<int> ratings = query.Rating.ToList();
                ratings.Add(rating);

                query.Rating = ratings.ToArray<int>();
            }

            using (FileStream outputStream = File.Open(JsonFileName, FileMode.Truncate))
            {
                JsonSerializer.Serialize<IEnumerable<Product>>(
                    new Utf8JsonWriter(outputStream, new JsonWriterOptions
                    {
                        SkipValidation = true,
                        Indented = true
                    }),
                    products);
            }
        }
    }
}
