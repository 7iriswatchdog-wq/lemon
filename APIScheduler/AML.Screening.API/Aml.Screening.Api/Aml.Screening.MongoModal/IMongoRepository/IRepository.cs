using Aml.Screening.MongoModal.Entity;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Aml.Screening.MongoModal.IMongoRepository
{
    public interface IRepository<TEntity, in TKey> where TEntity : IEntity<TKey>
    {
        Task<TEntity> GetByIdAsync(TKey id);

        Task<TEntity> SaveAsync(TEntity entity);
        long UpdateOne(FilterDefinition<TEntity> UpdateQuery, UpdateDefinition<TEntity> update);

        Task<TEntity> SaveOneAsync(TEntity entity);

        Task DeleteAsync(TKey id);

        void DeleteById(TKey id);
        TEntity FindOneAndDelete(Expression<Func<TEntity, bool>> predicate);
        double DeleteMAny(Expression<Func<TEntity, bool>> predicate);

        Task<ICollection<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> predicate);

        Task<ICollection<TEntity>> FindAllLimitAsync(Expression<Func<TEntity, bool>> predicate);

        Task<ICollection<TEntity>> FindAllLimitAsync(FilterDefinition<TEntity> predicate);

        Task<TEntity> FindOneAsync(Expression<Func<TEntity, bool>> predicate);

        ICollection<TEntity> FindAll(FilterDefinition<TEntity> predicate);
        long Count(FilterDefinition<TEntity> predicate);
        ICollection<TEntity> FindAllLimit(FilterDefinition<TEntity> predicate);
        ICollection<TEntity> FindAllPaging(FilterDefinition<TEntity> predicate, SortDefinition<TEntity> sort, int currentPage, int pageSize);
        TEntity FindOne(FilterDefinition<TEntity> predicate);
        TEntity SaveSync(TEntity entity);
        TEntity InsertOne(TEntity entity);
     
    }
}
