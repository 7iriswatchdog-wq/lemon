using MongoDB.Bson;

namespace Aml.Screening.MongoModal.Entity
{
    public interface IEntity<TKey>
    {
        TKey _id { get; set; }
    }

    public interface IEntity : IEntity<ObjectId>
    {
    }
}
