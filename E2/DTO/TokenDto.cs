namespace E2.DTO
{
    public class TokenDto
    {
        public string Token { get; set; }

        public string RefreshToken { get; set; }

        public TokenDto(string token, string refreshToken)
        {
            Token = token;
            RefreshToken = refreshToken;
        }
    }
}
