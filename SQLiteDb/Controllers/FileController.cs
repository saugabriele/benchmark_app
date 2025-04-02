using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BLL;
using BLL.Enums;
using DTO;


namespace SQLiteDb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FileController : ControllerBase
    {
        private readonly FileManager _fileManager;

        public FileController(FileManager fileManager)
        {
            _fileManager = fileManager;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFileAsync([FromForm] IFormFile file)
        {
            /*
             * Uploads a file to the server.
             *
             * Parameters:
             *   file (IFormFile): The file to upload.
             *
             * Returns:
             *   200 OK - If the file was uploaded successfully.
             *   400 Bad Request - If the file is not provided or has zero length.
             */

            FileEnum errorCode = await _fileManager.UploadFileAsync(file);

            switch (errorCode)
            {
                case FileEnum.fileNotProvidedOrWithLenghtZero:
                    return BadRequest("You must provide a valid filename or file");
                case FileEnum.none:
                    return Ok();
                default:
                    return BadRequest();
            }
        }

        [HttpGet("download")]
        public async Task<IActionResult> DownloadFileAsync([FromQuery] string filename)
        {
            /*
             * Downloads a file from the server.
             *
             * Parameters:
             *   filename (string): The name of the file to download.
             *
             * Returns:
             *   200 OK - With the file content if the file exists.
             *   400 Bad Request - If no filename is provided.
             *   404 Not Found - If the file does not exist.
             */

            (FileEnum errorCode, byte[] fileBytes) = await _fileManager.DownloadFileAsync(filename);

            switch (errorCode)
            {
                case FileEnum.fileNotProvided:
                    return BadRequest("You must provide a filename");
                case FileEnum.fileNotFound:
                    return NotFound("The file does not exists");
                case FileEnum.none:
                    return File(fileBytes, "application/octet-stream", filename);
                default:
                    return BadRequest();
            }
        }


        [HttpGet("delete")]
        public async Task<IActionResult> DeleteFileAsync([FromQuery] string filename)
        {
            /*
             * Deletes a file from the server.
             * 
             * Parameters:
             *   filename (string): The name of the file to delete.
             *
             * Returns:
             *   204 No Content - If the file was deleted successfully.
             *   400 Bad Request - If no filename is provided.
             *   404 Not Found - If the file does not exist.
             */

            FileEnum errorCode = await _fileManager.DeleteFileAsync(filename);

            switch (errorCode)
            {
                case FileEnum.fileNotProvided:
                    return BadRequest("You must provide a filename");
                case FileEnum.fileNotFound:
                    return NotFound("The file does not exists");
                case FileEnum.none:
                    return NoContent();
                default:
                    return BadRequest();
            }
        }


        [HttpGet("logs")]
        public async Task<IActionResult> GetLogsAsync()
        {
            /*
             * Retrieves the logs of all file actions performed on the server.
             *
             * Returns:
             *   200 OK - With a list of file logs.
             */

            List<FileDTO> logs = await _fileManager.GetLogsAsync();
            return Ok(logs);
        }
    }
}
