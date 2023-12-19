namespace KKProject.Models
{
    public class AuthResponse
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public long uid { get; set; }
        public long openid { get; set; }
        public string scope { get; set; }
        public string token_type { get; set; }
        public long expires_in { get; set; } 
    }
}
