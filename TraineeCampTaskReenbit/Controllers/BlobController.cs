using Azure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;
using TraineeCampTaskReenbit.DTO;
using TraineeCampTaskReenbit.Services;
using TraineeCampTaskReenbit.validator;


namespace TraineeCampTaskReenbit.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlobController : ControllerBase
    {
        private IBlobService blobService;
        public BlobController(IBlobService BlobService)
        {
            blobService = BlobService;
        }
        // GET: api/<BlobController>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            List<BlobRequestDTO?> files = await blobService.GetAllAsync();
            return Ok(files);
        }

        

        // POST api/<BlobController>
        [HttpPost]
        [FileFormatValidationAttribute]
        public async Task<IActionResult> Post(IFormFile file, string email)
        {
            BlobResponseDTO? response = await blobService.UploadAsync(file, email);
            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> Download(string filename)
        {
            BlobRequestDTO? file = await blobService.DownloadAsync(filename);
            if (file == null)
            {
                return BadRequest($"Error while downloading {filename}");
            }
            else
            {
                return File(file.Content, file.ContentType, file.Name);
            }
        }


        // DELETE api/<BlobController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult>  Delete(string fileName)
        {
            BlobResponseDTO response = await blobService.DeleteAsync(fileName);

            if (response.isSuccess != true)
            {
                return BadRequest(response.Message);
            }
            else
            {
                return Ok(response.Message);
            }
        }
    }
}
