using Microsoft.AspNetCore.Mvc;
using minimalwebapi.Classes;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace minimalwebapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobController : ControllerBase
    {
        private readonly JobService _jobService;

        public JobController(JobService jobService)
        {
            _jobService = jobService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllJobs()
        {
            var jobs = await _jobService.GetAllJobs();
            return Ok(jobs);
        }

        [HttpGet("usuarios")]
        public IActionResult GetUsuarios()
        {
            return Ok("Hello usuarios");
        }
    }
}
