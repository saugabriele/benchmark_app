using Microsoft.EntityFrameworkCore;
using AutoMapper;
using DBA;
using DTO;

namespace DAL
{
    public class UserRepository
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public UserRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<UserDTO?> GetUserByEmailAsync(string email)
        {
            //Fetch a User by their email from the database
            return _mapper.Map<UserDTO>(await _context.Users.FirstOrDefaultAsync(u => u.email == email));
        }

        public async Task<UserDTO?> GetUserByEmailAndUsernameAsync(string email, string username)
        {
            // Fetch a User by their email, username, and password from the database
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.email == email && u.username == username);

            return _mapper.Map<UserDTO>(user);
        }

        public async Task<string> GetUserPasswordSalt(string email)
        {
            //Fetch the salt value used to compute the hash of the password
            return (await _context.Users.FirstOrDefaultAsync(u => u.email == email)).salt;
        }

        public async Task CreateNewUserAsync(string username, string hashedPassword, string email, string salt)
        {
            //Create a new user in the database
            var user = new UserModel(username, hashedPassword, email, salt, hashedPassword);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateDatabaseAsync(string oldEmail, string newEmail)
        {
            UserModel? registeredUser = await _context.Users.FirstOrDefaultAsync(u => u.email == oldEmail);
            
            //Update the User object and save it in the database
            registeredUser.email = newEmail;
            
            //Update database
            await _context.SaveChangesAsync();
        }
    }
}
