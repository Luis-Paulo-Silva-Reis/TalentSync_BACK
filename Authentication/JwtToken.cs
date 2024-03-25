using Microsoft.IdentityModel.Tokens;
using minimalwebapi.models.CompanyModel;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;



namespace minimalwebapi.Authentication
{
    public class JwtTokenService(IConfiguration configuration)
    {
        private readonly IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        public RSAParameters GenerateRsaKeyPair()
        {
            using var rsa = new RSACryptoServiceProvider(2048);
            return rsa.ExportParameters(true);
        }

        public string GenerateSecureKey()
        {
            var key = new byte[32];
            using var generator = RandomNumberGenerator.Create();
            generator.GetBytes(key);
            return Convert.ToBase64String(key);
        }

        public string GenerateJwtToken(CompanyModel company)
        {
            try
            {
                var keyString = _configuration["Jwt:Key"];
                Console.WriteLine("keyString = " + keyString);
                if (string.IsNullOrWhiteSpace(keyString) || Encoding.UTF8.GetBytes(keyString).Length < 32)
                {
                    throw new InvalidOperationException($"JWT Key is not set or is too short in the configuration. Current length: {Encoding.UTF8.GetBytes(keyString).Length * 8} bits");
                }

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, company.CompanyName),
            new(JwtRegisteredClaimNames.Email, company.Email),
            new("companyId", company.Id), // Adicione o ID da empresa como uma reivindicação personalizada
            // Adicione outras reivindicações conforme necessário
        };

                var token = new JwtSecurityToken(
                    issuer: "Issuer",
                    audience: "Audience",
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(30),
                    signingCredentials: creds);

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                // Considere registrar os detalhes da exceção aqui
                throw new InvalidOperationException($"Error generating JWT token: {ex.Message}", ex);
            }
        }


        public bool ValidateJwtToken(string token, RSAParameters publicKey)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new RsaSecurityKey(publicKey),
                ValidateIssuer = true,
                ValidateAudience = true
            };

            try
            {
                tokenHandler.ValidateToken(token, validationParameters, out _);
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal bool ValidateJwtToken(string token, RSAParameters? rsaParams)
        {
            throw new NotImplementedException();
        }

        internal bool ValidateJwtToken(string token, string? rsaParams)
        {
            throw new NotImplementedException();
        }
    }
}
