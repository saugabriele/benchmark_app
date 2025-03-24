using Microsoft.EntityFrameworkCore;
using SQLiteDb.Models;

namespace SQLiteDb.Repositories
{
    public class FileRepository
    {
        private readonly AppDbContext _context;

        public FileRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task ActionOnFileAsync(string filename, string action)
        {
            //Create a new record that describe the action performed on a file
            _context.Files.Add(new FileRecord
            {
                filename = filename,
                action = action,
                date = DateOnly.FromDateTime(DateTime.Now)
            });

            await _context.SaveChangesAsync();
        }



        public async Task<List<FileRecord>> GetLogsAsync()
        {
            //Retrieve all file action logs from the database
            return await _context.Files.ToListAsync();
        }
    }
}
