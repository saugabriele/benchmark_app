using BLL.Enums;
using DAL;
using DTO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BLL
{
    public class FileManager
    {
        private readonly FileRepository _fileRepository;
        private readonly IWebHostEnvironment _environment;
        private readonly string _uploadPath;

        public FileManager(IWebHostEnvironment environment, FileRepository fileRepository)
        {
            _fileRepository = fileRepository;
            _environment = environment;
            _uploadPath = Path.Combine(_environment.WebRootPath, "uploads");
        }

        public async Task<FileEnum> UploadFileAsync(IFormFile file)
        {
            // Check if the file is null or its length is zero
            if (file == null || file.Length == 0)
            {
                return FileEnum.fileNotProvidedOrWithLenghtZero;
            }

            // Ensure the directory for storing files exists, if it exists create the full file path
            CheckDirectory();
            string filePath = Path.Combine(_uploadPath, file.FileName);

            // Use a FileStream to write the uploaded file to the file system
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            //Record the file action in the file repository
            await _fileRepository.ActionOnFileAsync(file.FileName, "Uploaded");
            return FileEnum.none;
        }

        public async Task<(FileEnum, byte[]?)> DownloadFileAsync([FromQuery] string filename)
        {
            // Check if the filename is provided
            if (filename == null)
            {
                return (FileEnum.fileNotProvided, null);
            }

            // Ensure the directory for downloading files exists, if it exists create the full file path
            CheckDirectory();
            string filePath = Path.Combine(_uploadPath, filename);

            // Check if the file exists at the specified file path
            if (!System.IO.File.Exists(filePath))
            {
                return (FileEnum.fileNotFound, null);
            }

            // Log the file download action
            await _fileRepository.ActionOnFileAsync(filename, "Downloaded");

            // Return the file bytes 
            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return (FileEnum.none, fileBytes);
        }

        public async Task<FileEnum> DeleteFileAsync([FromQuery] string filename)
        {
            // Check if the filename is provided
            if (filename == null)
            {
                return FileEnum.fileNotProvided;
            }

            // Ensure the directory for downloading files exists, if it exists create the full file path
            CheckDirectory();
            string filePath = Path.Combine(_uploadPath, filename);

            // Check if the file exists at the specified file path
            if (!System.IO.File.Exists(filePath))
            {
                return FileEnum.fileNotFound;
            }

            // Attempt to delete the file at the specified file path, log the action and returns
            System.IO.File.Delete(filePath);
            await _fileRepository.ActionOnFileAsync(filename, "Deleted");
            return FileEnum.none;
        }

        public async Task<List<FileDTO>> GetLogsAsync()
        {
            //Retrieves all the file action logs
            return await _fileRepository.GetLogsAsync();
        }

        private void CheckDirectory()
        {
            //Verifies that the directory exists
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }
    }
}
