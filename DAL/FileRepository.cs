using Microsoft.EntityFrameworkCore;
using DBA;
using DTO;
using AutoMapper;

namespace DAL
{
    public class FileRepository
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public FileRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task ActionOnFileAsync(string filename, string action)
        {
            //Create a new record that describe the action performed on a file
            _context.Files.Add(new FileModel
            {
                filename = filename,
                action = action,
                date = DateOnly.FromDateTime(DateTime.Now)
            });

            await _context.SaveChangesAsync();
        }

        public async Task<List<FileDTO>> GetLogsAsync()
        {
            //Retrieve all file action logs from the database
            return _mapper.Map<List<FileDTO>>(await _context.Files.ToListAsync());
        }
    }
}
