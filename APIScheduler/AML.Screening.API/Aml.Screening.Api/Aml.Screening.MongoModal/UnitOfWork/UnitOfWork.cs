using Aml.Screening.MongoModal.Modal;
using Aml.Screening.MongoModal.MongoRepository;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Aml.Screening.MongoModal.UnitOfWork
{
    public class UnitOfWork_NameListRepository : BaseMongoRepository<NAMELIST>
    {
        private readonly IMongoDatabase _dataContext = null;

        public UnitOfWork_NameListRepository(IConfiguration configuration) : base(configuration)
        {
            _dataContext = GetMongoConnection();
        }

        protected override IMongoCollection<NAMELIST> Collection => _dataContext.GetCollection<NAMELIST>("NAMELIST");
        public int Save()
        {
            throw new System.NotImplementedException();
        }
    }
    public class UnitOfWork_BlackListRepository : BaseMongoRepository<BLACKLIST>
    {
        private readonly IMongoDatabase _dataContext = null;

        public UnitOfWork_BlackListRepository(IConfiguration configuration) : base(configuration)
        {
            _dataContext = GetMongoConnection();
        }

        protected override IMongoCollection<BLACKLIST> Collection => _dataContext.GetCollection<BLACKLIST>("BLACKLIST");
        public int Save()
        {
            throw new System.NotImplementedException();
        }
    }
    public class UnitOfWork_CaseLogRepository : BaseMongoRepository<CASELOG>
    {
        private readonly IMongoDatabase _dataContext = null;

        public UnitOfWork_CaseLogRepository(IConfiguration configuration) : base(configuration)
        {
            _dataContext = GetMongoConnection();
        }

        protected override IMongoCollection<CASELOG> Collection => _dataContext.GetCollection<CASELOG>("CASELOG");
        public int Save()
        {
            throw new System.NotImplementedException();
        }
    }


    public class UnitOfWork_TranCaseLogRepository : BaseMongoRepository<TRANSACTION_CASELOG>
    {
        private readonly IMongoDatabase _dataContext = null;

        public UnitOfWork_TranCaseLogRepository(IConfiguration configuration) : base(configuration)
        {
            _dataContext = GetMongoConnection();
        }

        protected override IMongoCollection<TRANSACTION_CASELOG> Collection => _dataContext.GetCollection<TRANSACTION_CASELOG>("TRANSACTION_CASELOG");
        public int Save()
        {
            throw new System.NotImplementedException();
        }
    }
}
