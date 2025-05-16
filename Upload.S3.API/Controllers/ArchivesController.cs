using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http;
using Upload.S3.API.Application.Domain;
using Upload.S3.API.Application.Services;
using Upload.S3.API.Models;

namespace Upload.S3.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArchivesController : ControllerBase
    {
        private readonly IArchiveRepository _archiveRepository;
        private readonly IAmazonS3Service _amazonS3Service;

        public ArchivesController(IArchiveRepository archiveRepository, IAmazonS3Service amazonS3Service)
        {
            _archiveRepository = archiveRepository;
            _amazonS3Service = amazonS3Service;
        }

        [HttpPost]
        [ProducesResponseType(typeof(Archive), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult> AddArchive([FromForm] ArchiveViewModel model)
        {
            var bucket = "junio.app";
            try
            {                
                var key = $"medias/{Guid.NewGuid()}";
                var uploadFile = await _amazonS3Service.UploadFileAsync(bucket, key, model.Path);
                var urlPreAsign = await _amazonS3Service.GeneratePresignedURL(bucket, key);

                if (!uploadFile)
                    return StatusCode(500, "fail in add archive");
                return Ok(urlPreAsign);
                //var archive = new Archive(model.Title, key);
                //var result = _archiveRepository.AddArchive(archive);
                //return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(Archive), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> GetAllFiles()
        {
            var files = await _archiveRepository.GetAll();
            if (files is null || files.Count() == 0)
                return BadRequest("file not found");

            return Ok(files);

        }

        //public async Task<bool> UploadObject(string filePath, string url)
        //{
        //    using var streamContent = new StreamContent(
        //        new FileStream(filePath, FileMode.Open, FileAccess.Read));

        //    var response = await HttpClient.PutAsync(url, streamContent);
        //    return response.IsSuccessStatusCode;
        //}
    }
}
