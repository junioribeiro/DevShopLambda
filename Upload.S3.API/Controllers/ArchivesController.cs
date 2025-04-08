using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Upload.S3.API.Application.Domain;
using Upload.S3.API.Models;

namespace Upload.S3.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArchivesController : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(typeof(Archive), (int)HttpStatusCode.OK)]
        public async Task<ActionResult> AddArchive([FromForm] ArchiveViewModel archive)
        {
            return Ok();
        }
    }
}
