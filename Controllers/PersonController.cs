using Microsoft.AspNetCore.Mvc;
using minimalwebapi.Classes;
using minimalwebapi.models.PersonModel;
using MongoDB.Driver;


namespace Person.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PersonsController : ControllerBase
    {
        private readonly IMongoCollection<PersonModel> _collection;

        public PersonsController(DbConnection db)
        {
            _collection = db.GetCollection<PersonModel>("user");
        }

        [HttpGet]
        public async Task<IActionResult> GetPersons()
        {
            var persons = await _collection.Find(_ => true).ToListAsync();
            return Ok(persons);
        }

        [HttpGet("{id:length(24)}", Name = "GetPerson")]
        public async Task<IActionResult> GetPersonById(string id)
        {
            var person = await _collection.Find(p => p.id == id).FirstOrDefaultAsync();
            if (person == null)
            {
                return NotFound();
            }
            return Ok(person);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePerson(PersonModel person)
        {
          

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _collection.InsertOneAsync(person);
            return CreatedAtAction(nameof(GetPersonById), new { id = person.id }, person);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePerson(string id, PersonModel person)
        {
            if (id != person.id)
            {
                return BadRequest();
            }

            var updatedPerson = await _collection.FindOneAndReplaceAsync(p => p.id == id, person);
            if (updatedPerson == null)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> DeletePerson(string id)
        {
            var result = await _collection.DeleteOneAsync(p => p.id == id);
            if (result.DeletedCount == 0)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
