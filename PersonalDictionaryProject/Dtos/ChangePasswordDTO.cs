namespace PersonalDictionaryProject.Dtos
{
    public class ChangePasswordDTO
    {
        public string Id { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
