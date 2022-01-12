using AzureBlob.Api.Service;
using AzureBlob.Api.Models;
using Microsoft.AspNetCore.Mvc;
using MimeTypes;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AzureBlob.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly IFileService _fileManagerLogic;

        public ImageController(IFileService fileManagerLogic)
        {
            _fileManagerLogic = fileManagerLogic;
        }

        [Route("delete")]
        [HttpGet]
        public async Task<IActionResult> Delete(string fileName)
        {
            await _fileManagerLogic.Delete(fileName);
            return Ok();
        }

        [Route("download")]
        [HttpGet]
        public async Task<IActionResult> Download(string fileName)
        {
            var imageBytes = await _fileManagerLogic.Get(fileName);
            return new FileContentResult(imageBytes, "application/octet-stream")
            {
                FileDownloadName = Guid.NewGuid().ToString() + Path.GetExtension(fileName),
            };
        }

        [Route("get")]
        [HttpGet]
        public async Task<IActionResult> Get(string fileName)
        {
            var imageBytes = await _fileManagerLogic.Get(fileName);
            var mimeType = MimeTypeMap.GetMimeType(Path.GetExtension(fileName));
            return File(imageBytes, mimeType);
        }

        [Route("list")]
        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            var list = await _fileManagerLogic.GetList();

            return Ok(list);
        }

        [Route("upload")]
        [HttpPost]
        public async Task<IActionResult> Upload([FromForm] FileModel model)
        {
            if (model.ImageFile != null)
            {
                await _fileManagerLogic.Upload(model);
            }
            return Ok();
        }
    }
}