using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using minimalwebapi.Authentication;
using minimalwebapi.Classes;
using minimalwebapi.models.CompanyModel;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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
        private readonly IConfiguration _configuration;

        public CompanyController(
            DbConnection db,
            ILogger<CompanyController> logger,
            JwtTokenService jwtTokenService,
            IConfiguration configuration)
        {
            _collection = db.GetCollection<CompanyModel>("Company");
            _companyCredentialCollection = db.GetCollection<CompanyCredential>("CompanyCredential");
            _jwtTokenService = jwtTokenService;
            _logger = logger;
            _configuration = configuration;
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
            var company = await _collection.Find(c => c.Id == id).FirstOrDefaultAsync();
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
                company.Id = null;
                await _collection.InsertOneAsync(company);
                // Check if the company's id was set (MongoDB should set it automatically upon insertion)
                if (company.Id == null)
                {
                    throw new InvalidOperationException("Failed to create company. ID was not set.");
                }

                // After inserting the company, you should have the company's ID. Now create the credential.
                if (!string.IsNullOrEmpty(company.Password))
                {
                    var companyCredential = new CompanyCredential
                    {
                        PersonId = company.Id,
                        HashedPassword = BCrypt.Net.BCrypt.HashPassword(company.Password)
                    };

                    await _companyCredentialCollection.InsertOneAsync(companyCredential);
                }
                else
                {
                    _logger.LogError("Password is empty or null for company: {CompanyName}", company.CompanyName);
                    // Handle the lack of a password appropriately, perhaps by returning an error response.
                }

                var createdCompanyUri = Url.Action(nameof(GetCompanyById), new { company.Id });
                return Created(createdCompanyUri, new { company.Id, company });
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
            if (id != company.Id)
            {
                return BadRequest();
            }

            var updateResult = await _collection.ReplaceOneAsync(c => c.Id == id, company);

            if (updateResult.MatchedCount == 0)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> DeleteCompany(string id)
        {
            var deleteResult = await _collection.DeleteOneAsync(c => c.Id == id);
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

                var companyCredential = await _companyCredentialCollection.Find(c => c.PersonId == company.Id).FirstOrDefaultAsync();
                if (companyCredential == null)
                {
                    _logger.LogError("Credentials not found for company ID: {CompanyId}", company.Id);
                    return Unauthorized("Credentials not found.");
                }

                if (!BCrypt.Net.BCrypt.Verify(loginModel.Password, companyCredential.HashedPassword))
                {
                    _logger.LogError("Invalid credentials for company: {CompanyId}", company.Id);
                    return Unauthorized("Invalid credentials.");
                }

                var token = _jwtTokenService?.GenerateJwtToken(company);
                if (token == null)
                {
                    _logger.LogError("JWT token generation failed for company: {CompanyId}", company.Id);
                    return StatusCode(500, "Token generation failed.");
                }

                return Ok(new { token });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred during login.");
                return StatusCode(500, "An internal error occurred.");
            }
        }

        [HttpPost("verify-token")]
        public IActionResult VerifyToken()
        {
            try
            {
                var authHeader = Request.Headers["Authorization"].ToString();
                if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
                    return Unauthorized("Authorization header is missing or not a Bearer token.");

                var token = authHeader.Substring("Bearer ".Length).Trim();
                var secretKey = _configuration["Jwt:Key"];
                var isValid = ValidateJwtToken(token, secretKey);

                if (!isValid)
                    return Unauthorized("Invalid or expired token.");

                return Ok("Token is valid.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred during token verification.");
                return StatusCode(500, "An internal error occurred.");
            }
        }

        private bool ValidateJwtToken(string token, string secretKey)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };

                SecurityToken validatedToken;
                var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
                return true; // Token é válido
            }
            catch (Exception ex)
            {
                // Token é inválido
                return false;
            }
        }
    }
}
