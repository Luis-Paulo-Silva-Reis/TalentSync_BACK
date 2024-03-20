using Microsoft.IdentityModel.Tokens;
using minimalwebapi.models.CompanyModel;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace minimalwebapi.Authentication
{
    public class JwtTokenService
    {
        // This method generates an RSA key pair.
        public RSAParameters GenerateRsaKeyPair()
        {
            using (var rsa = new RSACryptoServiceProvider(2048)) // Specify the key size
            {
                return rsa.ExportParameters(true);
            }
        }

        // This method generates a JWT token using the RSA private key.
        private readonly IConfiguration _configuration;

        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateSecureKey()
        {
            var key = new byte[32]; // 32 bytes will give us 256 bits.
            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(key);
                return Convert.ToBase64String(key);
            }
        }


        public string GenerateJwtToken(CompanyModel company)
        {
            try
            {
                var keyString = _configuration["Jwt:Key"];
                if (string.IsNullOrWhiteSpace(keyString) || Encoding.UTF8.GetBytes(keyString).Length < 32)
                {
                    throw new InvalidOperationException("JWT Key is not set or is too short in the configuration. Current length: " + Encoding.UTF8.GetBytes(keyString).Length * 8 + " bits");
                }

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, company.CompanyName),
            new Claim(JwtRegisteredClaimNames.Email, company.Email),
            // Include additional claims as needed
        };

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(30), // Set token expiry as needed
                    signingCredentials: creds);

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                // Consider logging the exception details here
                throw new InvalidOperationException($"Error generating JWT token: {ex.Message}", ex);
            }
        }


        // This method validates a JWT token using the RSA public key.
        public bool ValidateJwtToken(string token, RSAParameters publicKey)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new RsaSecurityKey(publicKey),
                ValidateIssuer = false,
                ValidateAudience = false
            };

            try
            {
                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
