using System.Diagnostics.CodeAnalysis;

namespace DBA
{
    public class UserModel
    {
        public int id { get; set; }
        public required string username { get; set; }
        public required string password { get; set; }
        public required string email { get; set; }
        public required string salt { get; set; }
        public required string hashedPassword { get; set; }

        [SetsRequiredMembers]
        public UserModel(string username, string password, string email, string salt, string hashedPassword)
        {
            this.username = username;
            this.password = password;
            this.email = email;
            this.salt = salt;
            this.hashedPassword = hashedPassword;
        }
    }
}
