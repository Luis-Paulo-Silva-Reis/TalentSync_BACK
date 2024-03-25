using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace minimalwebapi.models.CompanyModel
{
    public class CompanyModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string Id { get; set; }
        public required string CompanyName { get; set; }
        public required string CompanyWebSite { get; set; }
        public required string CompanyCNPJ { get; set; }
        public required string CompanyAbout { get; set; }
        public required string CompanyPresentation { get; set; }
        public required string CompanyType { get; set; }
        public required string CompanyFoundationDate { get; set; }
        public required string CompanyAmountEmployers { get; set; }
        public required string CompanyRemotePolitcs { get; set; }
        public required string CompanySocialMidiaFacebook { get; set; }
        public required string CompanySocialMidiaX { get; set; }
        public required string CompanySocialMidiaLinkedIn { get; set; }
        public required string CompanySocialMidiaInstagram { get; set; }
        public required string CompanySocialMidiaTiktok { get; set; }
        public required string CompanySocialMidiaYouTube { get; set; }
        public required string CompanyBanner { get; set; }

        [BsonIgnore]
        public required string Password { get; set; }  // This will not be saved in the database

        public required string Email { get; set; }
    }
    public class CompanyCredential
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string PersonId { get; set; }
        public string HashedPassword { get; set; }


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

    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

}