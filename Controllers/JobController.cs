using Microsoft.AspNetCore.Mvc;
using minimalwebapi.Classes;
using minimalwebapi.models.JobModel;
using MongoDB.Driver;


namespace jobs.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly IMongoCollection<JobModel> _collection;

        public JobsController(DbConnection db)
        {
            _collection = db.GetCollection<JobModel>("jobs");
        }

        [HttpGet]
        public async Task<IActionResult> GetJobs()
        {
            var jobs = await _collection.Find(_ => true).ToListAsync();
            return Ok(jobs);
        }

        [HttpGet("{id:length(24)}", Name = "GetJob")]
        public async Task<IActionResult> GetJobById(string id)
        {
            var job = await _collection.Find(j => j.id == id).FirstOrDefaultAsync();
            if (job == null)
            {
                return NotFound();
            }
            return Ok(job);
        }

        [HttpPost]
        public async Task<IActionResult> CreateJob(JobModel job)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _collection.InsertOneAsync(job);
            return CreatedAtAction(nameof(GetJobById), new { id = job.id }, job);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateJob(string id, JobModel job)
        {
            if (id != job.id)
            {
                return BadRequest();
            }

            var updatedJob = await _collection.FindOneAndReplaceAsync(j => j.id == id, job);
            if (updatedJob == null)
            {
                return NotFound();
            }

            return NoContent();
        }


        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> DeleteJob(string id)
        {
            var result = await _collection.DeleteOneAsync(j => j.id == id);
            if (result.DeletedCount == 0)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
