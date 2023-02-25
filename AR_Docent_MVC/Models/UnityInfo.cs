using System.Text.Json;

namespace AR_Docent_MVC.Models
{
    public class UnityInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int Image_width { get; set; }
        public int Image_height { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize<UnityInfo>(this);
        }
    }
}
