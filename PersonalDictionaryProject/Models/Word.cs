namespace PersonalDictionaryProject.Models
{
    public class Word
    {
        public int Id { get; set; }
        public string WordText { get; set; } 
        public string Definition { get; set; }
        public string Example { get; set; }
        public string Language { get; set; } 
        public bool IsPublic { get; set; }
        public bool IsApproved { get; set; } 
        public string UserId { get; set; }
        public User User { get; set; }
        public bool ? IsApprovedYet { get; set; }
    }

}
