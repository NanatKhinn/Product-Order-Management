namespace ProductOrderApi.Models
{
        public class RegisterDto
        {
            public string Email { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }
            public string Role { get; set; }
    }

        public class LoginDto
        {
            public string LoginID { get; set; }
            public string Password { get; set; }
        }
}
