namespace AuthService.Data
{
    public class User
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string PasswordHash { get; set; }

        public UserState State { get; set; }

        public string ConirmationToken { get; set; }
    }
}
