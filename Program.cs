using minimalwebapi.Classes; // Importa��o da classe Classes do namespace minimalwebapi.
using MongoDB.Bson;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args); // Cria��o de um objeto 'builder' utilizando o m�todo est�tico CreateBuilder da classe WebApplication, passando args como argumento.

builder.Services.AddSingleton<DbConnection>(sp =>
{
    // Substitua "connectionString" e "databaseName" com suas informa��es reais.
    return new DbConnection("mongodb+srv://lpreis:mgdhcz8dt@talentsync.iwpm9dp.mongodb.net/?retryWrites=true&w=majority&appName=talentSync", "talentSync");
});

builder.Services.ConfigureCors(); // Configura��o do servi�o de Cross-Origin Resource Sharing (CORS) atrav�s do m�todo ConfigureCors da propriedade Services do objeto builder.

var app = builder.Build(); // Constru��o da aplica��o web a partir do objeto builder, armazenando-a na vari�vel 'app'.

app.UseCors("CorsPolicy"); // Utiliza��o do middleware CORS, passando "CorsPolicy" como par�metro.

app.MapGet("/", async (DbConnection db) =>
{
    var collection = db.GetCollection<BsonDocument>("jobs");

    var projection = Builders<BsonDocument>.Projection
        .Include("Profissao")
        .Include("Tipo")
        .Include("Level")
        .Include("LocalDeTrabalho")
        .Include("PCD");

    var documents = await collection.Find(new BsonDocument()).Project(projection).ToListAsync();

    // Mapeia os documentos para uma lista de JobDto contendo apenas os campos retornados
    var jobs = documents.Select(doc => new JobDto
    {
        Profissao = doc["Profissao"].AsString,
        Tipo = doc["Tipo"].AsString,
        Level = doc["Level"].AsString,
        LocalDeTrabalho = doc["LocalDeTrabalho"].AsString,
        PCD = doc["PCD"].AsString
    });

    return Results.Ok(jobs);
});

app.MapGet("/usuarios", () => "Hello usuarios"); // Mapeamento da rota "/usuarios" com um m�todo GET que retorna a mensagem "Hello usuarios".

app.Run(); // Execu��o da aplica��o.
