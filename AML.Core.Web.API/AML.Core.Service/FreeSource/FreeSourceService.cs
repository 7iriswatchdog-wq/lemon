using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.FreeSource;
using AML.Core.ServiceContract.FreeSource;
using AML.DTO.DTO.FreeSource;
using AML.ViewModel.ViewModels.FreeSource;
using Microsoft.Extensions.Configuration;
using System.Xml.Serialization;
using static AML.DTO.DTO.FreeSource.BlackListMongoDTO;

namespace AML.Core.Service.FreeSource
{
    public class FreeSourceService : IFreeSourceService
    {
        IFreeSourceRepository _freeSourceRepository;
        public FreeSourceService(IFreeSourceRepository freeSourceRepository, IConfiguration configuration)
        {
            _freeSourceRepository = freeSourceRepository;
        }
        public ServiceResponse<int> Create(BlackListMongoDTO.BLACKLIST dto)
        {
            return _freeSourceRepository.Create(dto);
        }

        public string ReadFile(string FilePath, string Source)
        {
            string response = string.Empty;
            if (Source == FreeResouce.OFAC)
            {
                string resp = _freeSourceRepository.ReadFile(FilePath);
                BLACKLIST req = new BLACKLIST();
                using (var stringReader = new System.IO.StringReader(resp))
                {
                    var serializer = new XmlSerializer(typeof(BLACKLIST));
                    req = serializer.Deserialize(stringReader) as BLACKLIST;
                }
                response = _freeSourceRepository.InsertNameList(req);
            }
            return response;
        }
    }
}
