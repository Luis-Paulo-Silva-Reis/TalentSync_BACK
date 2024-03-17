using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace minimalwebapi.data
{
    public static class DbConnectionExtensions
    {
        public static void AddMongoDbConnection(this IServiceCollection services, string connectionString, string databaseName)
        {
            services.AddSingleton<DbConnection>(sp =>
            {
                var client = new MongoClient(connectionString);
                var database = client.GetDatabase(databaseName);
                return new DbConnection(database);
            });
        }
    }

    public class DbConnection
    {
        public IMongoDatabase Database { get; }

        public DbConnection(IMongoDatabase database)
        {
            Database = database;
        }
    }
}
