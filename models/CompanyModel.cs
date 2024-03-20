using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace minimalwebapi.models.CompanyModel
{
    public class CompanyModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; }
        public string CompanyName { get; set; }
        public string CompanyWebSite { get; set; }
        public string CompanyCNPJ { get; set; }
        public string CompanyAbout { get; set; }
        public string CompanyPresentation { get; set; }
        public string CompanyType { get; set; }
        public string CompanyFoundationDate { get; set; }
        public string CompanyAmountEmployers { get; set; }
        public string CompanyRemotePolitcs { get; set; }
        public string CompanySocialMidiaFacebook { get; set; }
        public string CompanySocialMidiaX { get; set; }
        public string CompanySocialMidiaLinkedIn { get; set; }
        public string CompanySocialMidiaInstagram { get; set; }
        public string CompanySocialMidiaTiktok { get; set; }
        public string CompanySocialMidiaYouTube { get; set; }
        public string CompanyBanner { get; set; }

        [BsonIgnore]
        public string Password { get; set; }  // This will not be saved in the database

        public string Email { get; set; }
    }
    public class CompanyCredential
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; }
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