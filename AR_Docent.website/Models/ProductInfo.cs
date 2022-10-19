using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;

namespace AR_Docent.website.Models
{
    public class ProductInfo
    {
        public int id { get; set; }
        public string name { get; set; }
        public string title { get; set; }
        [NotMapped]
        public IFormFile image { get; set; }
        public string content { get; set; }
        public string created_at { get; set; }
    }
}
