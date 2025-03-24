using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace SQLiteDb.Models
{
    public class User
    {
        public int id { get; set; }
        public required string username { get; set; }
        public required string password { get; set; }
        public required string email { get; set; }
        public required string salt { get; set; }
        public required string hashedPassword { get; set; }

        [SetsRequiredMembers]
        public User(string username, string password, string email, string salt, string hashedPassword)
        {
            this.username = username;
            this.password = password;
            this.email = email;
            this.salt = salt;
            this.hashedPassword = hashedPassword;
        }

        public UserDTO UserDTOFromUser()
        {
            return new UserDTO
            {
                id = this.id,
                username = this.username,
                password = this.password,
                email = this.email
            };
        }
    }

    public class UserDTO
    {
        public int id { get; set; }
        public required string username { get; set; }
        public required string password { get; set; }
        public required string email { get; set; }
    }
}
