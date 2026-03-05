using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using System;

namespace AML.Core.Repository
{
    public class MongoRepository
    {
        public IConfiguration _Configuration;
        public MongoRepository(IConfiguration configuration)
        {
            _Configuration = configuration;
        }
        public IMongoDatabase MongoDatabase { get; }
        public IMongoDatabase GetMongoConnection()
        {
            try
            {
                var conString = _Configuration.GetSection("MongoDbConString").Value;
                var mongoUrl = new MongoUrl(conString);
                IMongoClient client = new MongoClient(mongoUrl);
                return client.GetDatabase(mongoUrl.DatabaseName);
            }
            catch(Exception ex){
                return null;
            }
        }

    }
}
