using AML.Core.RepositoryContract.InternalWatchList;
using AML.DTO.DTO.FreeSource;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using static AML.DTO.DTO.FreeSource.BlackListMongoDTO;

namespace AML.Core.Repository.InternalWatchList
{
    public class InternalWatchListMongoRepository : MongoRepository, IInternalWatchListMongoRepository
    {
        private IMongoDatabase _database;
        private const string CollectionName = "blocklist";

        public InternalWatchListMongoRepository(IConfiguration configuration) : base(configuration)
        {
            _database = GetMongoConnection();
        }

        public bool InsertBlockList(NAMELIST entity)
        {
            try
            {
                var collection = _database.GetCollection<NAMELIST>(CollectionName);
                var now = DateTime.UtcNow;
                if (string.IsNullOrEmpty(entity.CREATEDON))
                {
                    entity.CREATEDON = now.AddHours(4).ToString("dd/MM/yyyy HH:mm:ss"); // Assuming Dubai (+4) for the string representation
                }
                if (entity.CREATEDDATE == DateTime.MinValue)
                {
                    entity.CREATEDDATE = now;
                }
                if (entity.UPDATEDDATE == DateTime.MinValue)
                {
                    entity.UPDATEDDATE = now;
                }
                collection.InsertOne(entity);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public List<NAMELIST> GetBlockListLogs(string startDate, string endDate, string source, int skip, int take, out int totalRecords)
        {
            try
            {
                var collection = _database.GetCollection<NAMELIST>(CollectionName);
                var filterBuilder = Builders<NAMELIST>.Filter;
                var filter = filterBuilder.Empty;

                if (!string.IsNullOrEmpty(startDate))
                {
                    if (DateTime.TryParseExact(startDate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.AdjustToUniversal, out DateTime sDate))
                    {
                        filter &= filterBuilder.Gte(x => x.CREATEDDATE, sDate);
                    }
                }

                if (!string.IsNullOrEmpty(endDate))
                {
                    if (DateTime.TryParseExact(endDate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.AdjustToUniversal, out DateTime eDate))
                    {
                        // Add one day to include the entire end date
                        filter &= filterBuilder.Lte(x => x.CREATEDDATE, eDate.AddDays(1).AddTicks(-1));
                    }
                }

                if (!string.IsNullOrEmpty(source))
                {
                    filter &= filterBuilder.Regex(x => x.TYPE, new MongoDB.Bson.BsonRegularExpression(source, "i"));
                }

                var query = collection.Find(filter);
                totalRecords = (int)query.CountDocuments();

                return query.SortByDescending(x => x.CREATEDDATE)
                            .Skip(skip)
                            .Limit(take)
                            .ToList();
            }
            catch (Exception)
            {
                totalRecords = 0;
                return new List<NAMELIST>();
            }
        }
    }
}
