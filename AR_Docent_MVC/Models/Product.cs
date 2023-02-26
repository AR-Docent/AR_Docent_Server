using Microsoft.AspNetCore.Http;
using Microsoft.CognitiveServices.Speech;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace AR_Docent_MVC.Models
{
    public class Product
    {
        public int id { get; set; }
        
        [Required]
        public string name { get; set; }
        
        [Required]
        public string title { get; set; }
        
        [Required]
        [NotMapped]
        public IFormFile image { get; set; }
        
        [NotMapped]
        public AudioDataStream audio { get; set; }
        
        [Required]
        [StringLength(500)]
        public string content { get; set; }
        
        [DataType(DataType.DateTime)]
        public DateTime created_at { get; set; }
        
        public string img_name { get; set; }
        
        public string audio_name { get; set; }
        
        [Required]
        public int img_width { get; set; }
        [Required]
        public int img_height { get; set; }

        public override string ToString() => JsonSerializer.Serialize<Product>(this);
    }
}
