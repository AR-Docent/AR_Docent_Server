using Microsoft.AspNetCore.Http;
using Microsoft.CognitiveServices.Speech;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AR_Docent_MVC.Models
{
    public class Product
    {
        public int id { get; set; }
        public string name { get; set; }
        public string title { get; set; }
        [NotMapped]
        public IFormFile image { get; set; }
        [NotMapped]
        public AudioDataStream audio { get; set; }
        public string content { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime created_at { get; set; }
        public string img_name { get; set; }
        public string audio_name { get; set; }
    }
}
