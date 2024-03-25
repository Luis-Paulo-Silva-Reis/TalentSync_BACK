
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Org.BouncyCastle.Crypto.Generators;
using System.Globalization;

namespace minimalwebapi.models.PersonModel
{
    public class PersonModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string Id { get; set; }
        public required string Nome { get; set; }
        public required string Sobrenome { get; set; }
        [BsonElement("Data_nascimento")]
        public string DateOfBirthString { get; set; }
        // Property to store the parsed date
        [BsonIgnore] // Ignore this property during MongoDB serialization/deserialization
        public DateTime DateOfBirth { get; set; }
        public required string Email { get; set; }
        public required string Telefone { get; set; }
        public required string Pais_origem { get; set; }
        public required string CPF { get; set; }
        public required string Genero { get; set; }
        public required string PCD { get; set; }
        public required string LinkedIn { get; set; }
        public required string Cidade { get; set; }
        public required string Estado { get; set; }
        public required string Pais { get; set; }

        public void ParseDateOfBirth()
        {
            // Define the expected date format
            string[] formats = ["dd/MM/yyyy"];

            // Parse the date string into DateTime using the specified format
            if (!string.IsNullOrEmpty(DateOfBirthString) && DateTime.TryParseExact(DateOfBirthString, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
            {
                DateOfBirth = parsedDate;
            }
            else
            {
                // Handle parsing error or invalid date format
                // For example, you could set a default value or throw an exception
                DateOfBirth = DateTime.MinValue;
            }
        }
    }

    public class PersonCredential
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string Id { get; set; }
        public required string PersonId { get; set; }
        public required string HashedPassword { get; set; }


        [BsonIgnore]
        public string Password { get; set; }

        public void HashPassword()
        {
            if (!string.IsNullOrEmpty(Password))
            {
                // Hash the password
                Password = BCrypt.Net.BCrypt.HashPassword(Password);
            }
        }

        public bool VerifyPassword(string password)
        {
            return BCrypt.Net.BCrypt.Verify(password, Password);
        }
    }
}


