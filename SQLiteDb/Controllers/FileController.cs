using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SQLiteDb.Models;
using SQLiteDb.Repositories;
using System;
using System.IO;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SQLiteDb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FileController : ControllerBase
    {
        private readonly FileRepository _fileRepository;
        private readonly IWebHostEnvironment _environment;
        private readonly string _uploadPath;

        public FileController(IWebHostEnvironment environment, FileRepository fileRepository)
        {
            _fileRepository = fileRepository;
            _environment = environment;
            _uploadPath = Path.Combine(_environment.WebRootPath, "uploads");
        }


        [HttpPost("upload")]
        public async Task<IActionResult> UploadFileAsync([FromForm] IFormFile file)
        {
            // Check if the file is null or its length is zero
            if (file == null || file.Length == 0)
            {
                return BadRequest();
            }

            // Ensure the directory for storing files exists, if it exists create the full file path
            CheckDirectory();
            string filePath = Path.Combine(_uploadPath, file.FileName);

            // Use a FileStream to write the uploaded file to the file system
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            //Record the file action in the file repository and returns an OK response
            await _fileRepository.ActionOnFileAsync(file.FileName, "Uploaded");
            return Ok();
        }


        [HttpGet("download")]
        public async Task<IActionResult> DownloadFileAsync([FromQuery] string filename)
        {
            // Check if the filename is provided
            if (filename == null)
            {
                return BadRequest("You must provide a filename");
            }

            // Ensure the directory for downloading files exists, if it exists create the full file path
            CheckDirectory();
            string filePath = Path.Combine(_uploadPath, filename);

            // Check if the file exists at the specified file path
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("The file does not exists");
            }

            // Log the file download action
            await _fileRepository.ActionOnFileAsync(filename, "Downloaded");

            // Return the file as a response to the client
            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, "application/octet-stream", filename);
        }


        [HttpGet("delete")]
        public async Task<IActionResult> DeleteFileAsync([FromQuery] string filename)
        {
            // Check if the filename is provided
            if (filename == null)
            {
                return BadRequest("You must provide a filename");
            }

            // Ensure the directory for downloading files exists, if it exists create the full file path
            CheckDirectory();
            string filePath = Path.Combine(_uploadPath, filename);

            // Check if the file exists at the specified file path
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound($"The file does not exists");
            }

            // Attempt to delete the file at the specified file path, log the action and returns a NoContent response
            System.IO.File.Delete(filePath);
            await _fileRepository.ActionOnFileAsync(filename, "Deleted");
            return NoContent();
        }


        [HttpGet("logs")]
        public async Task<IActionResult> GetLogsAsync()
        {
            //Retrieves all the file action logs
            return Ok(await _fileRepository.GetLogsAsync());
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
