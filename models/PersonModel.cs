
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Globalization;

namespace minimalwebapi.models.PersonModel
{
    public class PersonModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; }
        public string Nome { get; set; }
        public string Sobrenome { get; set; }

        [BsonElement("Data_nascimento")]
        public string DateOfBirthString { get; set; }

        // Property to store the parsed date
        [BsonIgnore] // Ignore this property during MongoDB serialization/deserialization
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public string Pais_origem { get; set; }
        public string CPF { get; set; }
        public string Genero { get; set; }
        public string PCD { get; set; }
        public string LinkedIn { get; set; }
        public string Cidade { get; set; }
        public string Estado { get; set; }
        public string Pais { get; set; }

        public void ParseDateOfBirth()
        {
            // Define the expected date format
            string[] formats = { "dd/MM/yyyy" };

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



  



}
