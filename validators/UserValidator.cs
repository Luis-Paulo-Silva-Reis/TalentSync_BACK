using MongoDB.Driver;
using minimalwebapi.models.PersonModel;

namespace validators.userValidator
{
    public class ValidationService

    {
        private readonly IMongoCollection<PersonModel> _collection;


        public ValidationService(IMongoCollection<PersonModel> collection)
        {
            _collection = collection;
        }
        public bool IsValidCNPJ(string cnpj)
        {
            cnpj = cnpj.Trim();
            cnpj = cnpj.Replace(".", "").Replace("-", "").Replace("/", "");

            if (cnpj.Length != 14)
                return false;

            int[] multiplier1 = new int[12] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplier2 = new int[13] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            int sum, rest;
            string digit, tempCnpj;

            tempCnpj = cnpj.Substring(0, 12);
            sum = 0;

            for (int i = 0; i < 12; i++)
                sum += int.Parse(tempCnpj[i].ToString()) * multiplier1[i];

            rest = (sum % 11);
            if (rest < 2)
                rest = 0;
            else
                rest = 11 - rest;

            digit = rest.ToString();
            tempCnpj = tempCnpj + digit;
            sum = 0;
            for (int i = 0; i < 13; i++)
                sum += int.Parse(tempCnpj[i].ToString()) * multiplier2[i];

            rest = (sum % 11);
            if (rest < 2)
                rest = 0;
            else
                rest = 11 - rest;

            digit = digit + rest.ToString();

            return cnpj.EndsWith(digit);
        }

        /*

        public async Task<bool> CNPJExists(string cnpj)
        {
            // Query your database to check if CNPJ exists
            return await _collection.Find(p => p.CNPJ == cnpj).AnyAsync();
        }*/

        public bool IsValidCPF(string cpf)
        {
            cpf = cpf.Trim();
            cpf = cpf.Replace(".", "").Replace("-", "");

            if (cpf.Length != 11)
                return false;

            for (int i = 0; i < 10; i++)
                if (i.ToString().PadLeft(11, char.Parse(i.ToString())) == cpf)
                    return false;

            int add = 0;
            for (int i = 0; i < 9; i++)
                add += int.Parse(cpf[i].ToString()) * (10 - i);

            int rev = 11 - (add % 11);
            if (rev == 10 || rev == 11)
                rev = 0;

            if (rev != int.Parse(cpf[9].ToString()))
                return false;

            add = 0;
            for (int i = 0; i < 10; i++)
                add += int.Parse(cpf[i].ToString()) * (11 - i);

            rev = 11 - (add % 11);
            if (rev == 10 || rev == 11)
                rev = 0;

            if (rev != int.Parse(cpf[10].ToString()))
                return false;

            return true;
        }

        public async Task<bool> CPFExists(string cpf)
        {
            // Query your database to check if CPF exists
            return await _collection.Find(p => p.CPF == cpf).AnyAsync();
        }

        public async Task<bool> EmailExists(string cpf)
        {
            // Query your database to check if CPF exists
            return await _collection.Find(p => p.Email == cpf).AnyAsync();
        }

    }
}