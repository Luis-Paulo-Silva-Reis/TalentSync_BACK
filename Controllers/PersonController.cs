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


namespace Person.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PersonsController : ControllerBase
    {
        private readonly IMongoCollection<PersonModel> _collection;
        private readonly IMongoCollection<PersonModel> _usersCollection;
        private readonly IConfiguration _configuration;

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

        [HttpPost("create")]
        public async Task<IActionResult> CreateUser(PersonModel user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _usersCollection.InsertOneAsync(user);
            var token = GenerateJwtToken(user);
            return Ok(new { user, token });
        }

        private string GenerateJwtToken(PersonModel user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Nome),
                    // Add other required claims as needed
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
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
