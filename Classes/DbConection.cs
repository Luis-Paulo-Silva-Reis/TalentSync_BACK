﻿using MongoDB.Driver;


namespace minimalwebapi.Classes

{
    public class DbConnection

    {
        private readonly IMongoDatabase _database;

        public DbConnection(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return _database.GetCollection<T>(collectionName);
        }

    }
}



