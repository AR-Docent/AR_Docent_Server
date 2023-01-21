using System.Text.Json;

namespace AR_Docent_MVC.Models
{
    public class UnityInfo
    {
        public int id { get; set; }
        public string name { get; set; }
        public string title { get; set; }
        public string image_name { get; set; }
        public string audio_name { get; set; }
        public string image_url { get; set; }
        public string audio_url { get; set; }
        public string content { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize<UnityInfo>(this);
        }
    }
}
