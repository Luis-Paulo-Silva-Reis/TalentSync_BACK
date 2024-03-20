using Microsoft.AspNetCore.Mvc;
using minimalwebapi.Classes;
using minimalwebapi.models.CompanyModel;
using MongoDB.Driver;
using minimalwebapi.Authentication;

namespace Company.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CompanyController : ControllerBase
    {
        private readonly IMongoCollection<CompanyModel> _collection;
        private readonly IMongoCollection<CompanyCredential> _companyCredentialCollection;
        private readonly JwtTokenService _jwtTokenService;
        private readonly ILogger<CompanyController> _logger;


        // Inject both ILogger and DbConnection in the constructor
        public CompanyController(DbConnection db, ILogger<CompanyController> logger, JwtTokenService jwtTokenService)
        {
            _collection = db.GetCollection<CompanyModel>("Company");
            _companyCredentialCollection = db.GetCollection<CompanyCredential>("CompanyCredential");
            _logger = logger; // Initialize the logger
            _jwtTokenService = jwtTokenService;  // Correctly initialize the JwtTokenService



        }

        [HttpGet]
        public async Task<IActionResult> GetCompanies()
        {
            var companies = await _collection.Find(_ => true).ToListAsync();
            return Ok(companies);
        }

        [HttpGet("{id:length(24)}", Name = "GetCompanyById")]
        public async Task<IActionResult> GetCompanyById(string id)
        {
            var company = await _collection.Find(c => c.id == id).FirstOrDefaultAsync();
            if (company == null)
            {
                return NotFound();
            }
            return Ok(company);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCompany(CompanyModel company)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("Model state is invalid for CreateCompany: {ModelState}",
                                 ModelState.Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) }));
                return BadRequest(ModelState);
            }
            try
            {
                company.id = null;
                await _collection.InsertOneAsync(company);
                // Check if the company's id was set (MongoDB should set it automatically upon insertion)
                if (company.id == null)
                {
                    throw new InvalidOperationException("Failed to create company. ID was not set.");
                }

                // After inserting the company, you should have the company's ID. Now create the credential.
                if (!string.IsNullOrEmpty(company.Password))
                {
                    var companyCredential = new CompanyCredential
                    {
                        PersonId = company.id,
                        HashedPassword = BCrypt.Net.BCrypt.HashPassword(company.Password)
                    };

                    await _companyCredentialCollection.InsertOneAsync(companyCredential);
                }
                else
                {
                    _logger.LogError("Password is empty or null for company: {CompanyName}", company.CompanyName);
                    // Handle the lack of a password appropriately, perhaps by returning an error response.
                }

                var createdCompanyUri = Url.Action(nameof(GetCompanyById), new { id = company.id });
                return Created(createdCompanyUri, new { id = company.id, company });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while creating a new company: {CompanyName}. Exception: {ExceptionMessage}, StackTrace: {StackTrace}",
                                 company.CompanyName, ex.Message, ex.StackTrace);
                return StatusCode(500, new { error = "An unexpected error occurred.", detail = ex.Message });
            }
        }




        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> UpdateCompany(string id, CompanyModel company)
        {
            if (id != company.id)
            {
                return BadRequest();
            }

            var updateResult = await _collection.ReplaceOneAsync(c => c.id == id, company);

            if (updateResult.MatchedCount == 0)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> DeleteCompany(string id)
        {
            var deleteResult = await _collection.DeleteOneAsync(c => c.id == id);
            if (deleteResult.DeletedCount == 0)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            if (loginModel == null)
            {
                _logger.LogError("LoginModel is null.");
                return BadRequest("LoginModel is null.");
            }

            try
            {
                var company = await _collection.Find(c => c.Email == loginModel.Email).FirstOrDefaultAsync();
                if (company == null)
                {
                    _logger.LogError("Company not found for email: {Email}", loginModel.Email);
                    return NotFound("Company not found.");
                }

                var companyCredential = await _companyCredentialCollection.Find(c => c.PersonId == company.id).FirstOrDefaultAsync();
                if (companyCredential == null)
                {
                    _logger.LogError("Credentials not found for company ID: {CompanyId}", company.id);
                    return Unauthorized("Credentials not found.");
                }

                if (!BCrypt.Net.BCrypt.Verify(loginModel.Password, companyCredential.HashedPassword))
                {
                    _logger.LogError("Invalid credentials for company: {CompanyId}", company.id);
                    return Unauthorized("Invalid credentials.");
                }

                var token = _jwtTokenService?.GenerateJwtToken(company);
                if (token == null)
                {
                    _logger.LogError("JWT token generation failed for company: {CompanyId}", company.id);
                    return StatusCode(500, "Token generation failed.");
                }

                return Ok(new { token = token });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred during login.");
                return StatusCode(500, "An internal error occurred.");
            }
        }
    }
}

