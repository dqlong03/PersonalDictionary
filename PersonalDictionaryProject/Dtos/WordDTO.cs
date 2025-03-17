using PersonalDictionaryProject.Models;

namespace PersonalDictionaryProject.Dtos
{
    public class WordDTO
    {
        public int Id { get; set; }
        public string WordText { get; set; }
        public string Definition { get; set; }
        public string Example { get; set; }
        public string Language { get; set; }
        public bool IsPublic { get; set; }
    }
}
