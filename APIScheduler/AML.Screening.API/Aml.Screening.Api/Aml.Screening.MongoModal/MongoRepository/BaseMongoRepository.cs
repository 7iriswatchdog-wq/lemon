using Aml.Screening.MongoModal.Entity;
using Aml.Screening.MongoModal.IMongoRepository;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Misc;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aml.Screening.MongoModal.MongoRepository
{
    public abstract class BaseMongoRepository<TEntity> : IRepository<TEntity, ObjectId> where TEntity : IEntity
    {
        public IConfiguration Configuration;
        private string _mongoConnectionString = string.Empty;

        public BaseMongoRepository(IConfiguration configuration)
        {
            Configuration = configuration;
            _mongoConnectionString = configuration.GetConnectionString("mongodb");
        }
        #region

        public IMongoDatabase GetMongoConnection()
        {
            var url = _mongoConnectionString;
            var mongoUrl = new MongoUrl(url);
            IMongoClient client = new MongoClient(mongoUrl);
            return client.GetDatabase(mongoUrl.DatabaseName);
        }
        #endregion
        protected abstract IMongoCollection<TEntity> Collection { get; }

        public virtual async Task<TEntity> GetByIdAsync(ObjectId id)
        {
            if (id != null)
            {
                throw new ArgumentException("message", nameof(id));
            }

            return await Collection.Find(x => x._id.Equals(id)).FirstOrDefaultAsync();
        }

        public virtual async Task<TEntity> SaveAsync(TEntity entity)
        {
            if (entity._id != null)
            {
                entity._id = ObjectId.GenerateNewId();
            }

            await Collection.ReplaceOneAsync(
                x => x._id.Equals(entity._id),
                entity,
                new UpdateOptions
                {
                    IsUpsert = true
                });

            return entity;
        }

        /// <summary>
        /// Saves the one asynchronous.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public virtual async Task<TEntity> SaveOneAsync(TEntity entity)
        {
            if (entity != null)
            {
                await Collection.InsertOneAsync(entity);
            }
            return entity;
        }
        public virtual async Task DeleteAsync(ObjectId id)
        {
            if (id == null)
            {
                throw new ArgumentException("message", nameof(id));
            }
            await Collection.DeleteOneAsync(x => x._id.Equals(id));
        }
        public virtual void DeleteById(ObjectId id)
        {
            if (id == null)
            {

                throw new ArgumentException("message", nameof(id));
            }

             Collection.DeleteOne(x => x._id.Equals(id));
           

        }
        public virtual TEntity FindOneAndDelete(Expression<Func<TEntity, bool>> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            return Collection.FindOneAndDelete(predicate);
        }
        public virtual double DeleteMAny(Expression<Func<TEntity, bool>> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            var result = Collection.DeleteMany(predicate);
            return result.IsAcknowledged != false ? result.DeletedCount : 0;
        }
        public virtual void InsertMany(IEnumerable<TEntity> documents, InsertManyOptions options = null)
        {
            Collection.InsertMany(documents);
        }
        public virtual async Task<ICollection<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            try
            {
                var t = await Collection.Find(predicate).ToListAsync();
            }
            catch (Exception ex)
            {
                throw;
            }

            return await Collection.Find(predicate).ToListAsync();
        }
        public virtual async Task<ICollection<TEntity>> FindAllLimitAsync(Expression<Func<TEntity, bool>> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            return await Collection.Find(predicate).ToListAsync();

        }
        public virtual async Task<ICollection<TEntity>> FindAllLimitAsync(FilterDefinition<TEntity> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            return await Collection.Find(predicate).ToListAsync();

        }
        public virtual async Task<TEntity> FindOneAsync(Expression<Func<TEntity, bool>> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            return await Collection.Find(predicate).FirstOrDefaultAsync();
        }
        public virtual async Task<TEntity> FindOneAsync(FilterDefinition<TEntity> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            return await Collection.Find(predicate).FirstOrDefaultAsync();
        }
        public virtual TEntity SaveSync(TEntity entity)
        {
            if (entity._id != null)
            {
                entity._id = ObjectId.GenerateNewId();
            }

            Collection.ReplaceOneAsync(
               x => x._id.Equals(entity._id),
               entity,
               new UpdateOptions
               {
                   IsUpsert = true
               });

            return entity;
        }
        public virtual long UpdateOne(FilterDefinition<TEntity> UpdateQuery, UpdateDefinition<TEntity> update)
        {
            var colln = Collection.Find(UpdateQuery).ToList();
            if (colln.Count > 0)
            {
                var result = Collection.UpdateOne(UpdateQuery, update);
                if (result.IsAcknowledged)
                    return result.IsModifiedCountAvailable ?
                        result.ModifiedCount : 0;
            }
            return 0;
        }

        
        //public virtual UpdateResult UpdateOne(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> update, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        //{
        //    return UpdateOne(filter, update, options, (IEnumerable<WriteModel<TEntity>> requests, BulkWriteOptions bulkWriteOptions) => BulkWrite(requests, bulkWriteOptions, cancellationToken));
        //}
        //private UpdateResult UpdateOne(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> update, UpdateOptions options, Func<IEnumerable<WriteModel<TEntity>>, BulkWriteOptions, BulkWriteResult<TEntity>> bulkWrite)
        //{
        //    Ensure.IsNotNull(filter, "filter");
        //    Ensure.IsNotNull(update, "update");
        //    options = options ?? new UpdateOptions();
        //    UpdateOneModel<TEntity> updateOneModel = new UpdateOneModel<TEntity>(filter, update)
        //    {
        //        ArrayFilters = options.ArrayFilters,
        //        Collation = options.Collation,
        //        Hint = options.Hint,
        //        IsUpsert = options.IsUpsert
        //    };
        //    try
        //    {
        //        BulkWriteOptions arg = new BulkWriteOptions
        //        {
        //            BypassDocumentValidation = options.BypassDocumentValidation
        //        };
        //        return UpdateResultFromCore(bulkWrite(new UpdateOneModel<TEntity>[1] { updateOneModel }, arg));
        //    }
        //    catch (MongoBulkWriteException<TEntity> bulkException)
        //    {
        //        throw MongoWriteException.FromBulkWriteException(bulkException);
        //    }
        //}
        public virtual ICollection<TEntity> FindAll(FilterDefinition<TEntity> predicate)
        {
            var depF = Collection.Find(predicate);
            Console.WriteLine(depF);
            return depF.ToList();
        }
        /// <summary>
        /// Counts the specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        /// <author>Anand</author>
        /// <datetime>01/09/2019.11:15 PM</datetime>
        public virtual long Count(FilterDefinition<TEntity> predicate)
        {
            return Collection.Find(predicate).Count();
        }
        public virtual ICollection<TEntity> FindAllLimit(FilterDefinition<TEntity> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            return Collection.Find(predicate).ToList();

        }
        public virtual ICollection<TEntity> FindAllPaging(FilterDefinition<TEntity> predicate, SortDefinition<TEntity> sort, int currentPage, int pageSize)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            return Collection.Find(predicate).Sort(sort).Skip((currentPage - 1) * pageSize).Limit(pageSize).ToList();

        }
        public virtual TEntity FindOne(FilterDefinition<TEntity> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            return Collection.Find(predicate).FirstOrDefault();
        }
        public virtual TEntity InsertOne(TEntity entity)
        {
            entity._id = ObjectId.GenerateNewId();
            Collection.InsertOne(entity);            
            return entity;
        }



    }
}
