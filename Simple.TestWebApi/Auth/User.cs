namespace Simple.TestWebApi.Auth
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }

        private static int Uid = 0;
        /// <summary>
        /// Simulate user authentication
        /// </summary>
        public static User Get(string user, string passwd, bool allow)
        {
            if (!allow) return null;

            return new User()
            {
                Id = ++Uid,
                Username = user,
                Role = "AUTH_USER",
            };
        }
    }
}
