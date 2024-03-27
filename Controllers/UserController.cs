using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using minimalwebapi.Classes;
using minimalwebapi.models.PersonModel;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.RegularExpressions;
using validators.userValidator;


namespace Person.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PersonsController : ControllerBase
    {
        private readonly IMongoCollection<PersonModel> _collection;
        private readonly IConfiguration _configuration;
        private readonly ValidationService _validationService;

        public PersonsController(
            IMongoCollection<PersonModel> collection,
            IConfiguration configuration,
            ValidationService validationService)
        {
            _collection = collection;
            _configuration = configuration;
            _validationService = validationService;
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
            var person = await _collection.Find(p => p.Id == id).FirstOrDefaultAsync();
            if (person == null)
            {
                return NotFound();
            }
            return Ok(person);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreatePerson(PersonModel person)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Email validation
            if (!Regex.IsMatch(person.Email, @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$"))
            {
                return BadRequest("Invalid email format.");
            }

            // CPF validation
            if (!_validationService.IsValidCPF(person.CPF) || await _validationService.CPFExists(person.CPF))
            {
                return BadRequest("Invalid or existing CPF.");
            }

            // Email validation
            if (await _validationService.EmailExists(person.Email))
            {
                return BadRequest("existing Email.");
            }

            // ID generation
            if (string.IsNullOrWhiteSpace(person.Id) || !ObjectId.TryParse(person.Id, out _))
            {
                person.Id = ObjectId.GenerateNewId().ToString();
            }

            await _collection.InsertOneAsync(person);
            return CreatedAtAction(nameof(GetPersonById), new { id = person.Id }, person);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePerson(string id, PersonModel person)
        {
            if (id != person.Id)
            {
                return BadRequest();
            }

            var updatedPerson = await _collection.FindOneAndReplaceAsync(p => p.Id == id, person);
            if (updatedPerson == null)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> DeletePerson(string id)
        {
            var result = await _collection.DeleteOneAsync(p => p.Id == id);
            if (result.DeletedCount == 0)
            {
                return NotFound();
            }
            return NoContent();
        }


    }
}
