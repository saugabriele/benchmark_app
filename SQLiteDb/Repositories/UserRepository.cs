using Microsoft.EntityFrameworkCore;
using SQLiteDb.Models;
using System.Text;
using System.Linq.Expressions;

namespace SQLiteDb.Repositories
{
    public class UserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            //Fetch a User by their email from the database
            return await _context.Users.FirstOrDefaultAsync(u => u.email == email);
        }

        public async Task CreateNewUserAsync(User user)
        {
            //Create a new user in the database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
    }
}
