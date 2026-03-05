using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.FreeSource;
using AML.Core.RepositoryContract.Sanction;
using AML.Core.ServiceContract.Sanction;
using AML.DTO.DTO.FreeSource;
using AML.DTO.DTO.Sanction;
using AML.ViewModel.ViewModels.Sanction;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using static AML.DTO.DTO.FreeSource.BlackListMongoDTO;

namespace AML.Core.Service.Sanction
{
    public class SanctionService : ISanctionService
    {
        IFreeSourceRepository _freeSourceRepository;
        IWatchListRepository _watchListRepository;
        public SanctionService(IFreeSourceRepository freeSourceRepository, IWatchListRepository watchListRepository, IConfiguration configuration)
        {
            _freeSourceRepository = freeSourceRepository;
            _watchListRepository = watchListRepository;
        }
        public ServiceResponse<int> Create(WatchListDTO model)
        {
            var result = _watchListRepository.Create(model);
            if (result.Result > 0)
            {
                BlackListMongoDTO.BLACKLIST dto = new BlackListMongoDTO.BLACKLIST();
                dto.UID = "CBW-" + result.Result;
                dto.TYPE = "CBW";
                dto.FULLNAME = model.FirstName + " " + model.MiddleName + " " + model.LastName;
                dto.FIRST_NAME = model.FirstName;
                dto.LAST_NAME = model.LastName;
                dto.MIDDLE_NAME = model.MiddleName;
                List<IDLIST> ID_Details = new List<IDLIST>();
                IDLIST id = new IDLIST();
                id.IDTYPE = "PASSPORT";
                id.IDNUMBER = model.PassportNo;
                ID_Details.Add(id);
                dto.IDLIST = ID_Details;
                List<DOBLIST> DOBLIST = new List<DOBLIST>();
                DOBLIST dob = new DOBLIST();
                dob.DOB = model.DOB;
                DOBLIST.Add(dob);
                dto.DOBLIST = DOBLIST;
                List<NATIONALITYLIST> NATIONALITYLIST = new List<NATIONALITYLIST>();
                NATIONALITYLIST n = new NATIONALITYLIST();
                n.NATCOUNTRY = model.Nationality != null ? model.Nationality : "";
                NATIONALITYLIST.Add(n);
                dto.NATIONALITYLIST = NATIONALITYLIST;
                dto.REMARKS = model.Remarks;
                List<ADDINFOLIST> ADDINFOLIST = new List<ADDINFOLIST>();
                ADDINFOLIST addinfo = new ADDINFOLIST();
                addinfo.ADDINFO1 = model.Source;
                addinfo.ADDINFO2 = model.Narration;
                dto.ADDINFOLIST = ADDINFOLIST;
                ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
                _freeSourceRepository.InsertNameList(dto);
                serviceResponse.Result = 1;
                return serviceResponse;
            }
            return result;
        }

        public List<WatchListDTO> GetAll()
        {
            return _watchListRepository.GetAll().Result;
        }
    }
}
