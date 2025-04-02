namespace DTO
{
    public class UserDTO
    {
        public int id { get; set; }
        public required string username { get; set; }
        public required string password { get; set; }
        public required string email { get; set; }
    }
}
