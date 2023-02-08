using System.Text.Json;

namespace AR_Docent_MVC.Models
{
    public class UnityInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Image_name { get; set; }
        public string Audio_name { get; set; }
        public string Image_url { get; set; }
        public string Audio_url { get; set; }
        public string Content { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize<UnityInfo>(this);
        }
    }
}
